using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using AutoMapper;
using ParkingSlotAPI.Entities;
using ParkingSlotAPI.Models;
using ParkingSlotAPI.Repository;
using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using ParkingSlotAPI.Helpers;

namespace ParkingSlotAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;

        public FeedbacksController(IFeedbackRepository feedbackRepository, IMapper mapper, IUserRepository userRepository, IUrlHelper urlHelper)
        {
            _feedbackRepository = feedbackRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _urlHelper = urlHelper;
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet(Name = "GetFeedbacks")]
        public IActionResult GetFeedbacks([FromQuery] FeedbackResourceParameters feedbackResourceParameters)
        {
            var feedbacksFromRepo = _feedbackRepository.GetFeedbacks(feedbackResourceParameters);

            if (feedbacksFromRepo == null)
            {
                return NotFound();
            }

            var previousPageLink = feedbacksFromRepo.HasPrevious ?
                 CreateFeedbacksResourceUri(feedbackResourceParameters,
                 ResourceUriType.PreviousPage) : null;

            var x = CreateFeedbacksResourceUri(feedbackResourceParameters,
                ResourceUriType.NextPage);

            var nextPageLink = feedbacksFromRepo.HasNext ?
                CreateFeedbacksResourceUri(feedbackResourceParameters,
                ResourceUriType.NextPage) : null;

            var paginationMetadata = new
            {
                totalCount = feedbacksFromRepo.TotalCount,
                pageSize = feedbacksFromRepo.PageSize,
                currentPage = feedbacksFromRepo.CurrentPage,
                totalPages = feedbacksFromRepo.TotalPages,
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink
            };

            Response.Headers.Add("X-Pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            var feedbacks = _mapper.Map<IEnumerable<FeedbackDto>>(feedbacksFromRepo);

            return Ok(feedbacks);
        }

        private string CreateFeedbacksResourceUri(FeedbackResourceParameters feedbackResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFeedbacks",
                        new
                        {
                            orderBy = feedbackResourceParameters.OrderBy,
                            pageNumber = feedbackResourceParameters.PageNumber - 1,
                            pageSize = feedbackResourceParameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFeedbacks",
                        new
                        {
                            orderBy = feedbackResourceParameters.OrderBy,
                            pageNumber = feedbackResourceParameters.PageNumber + 1,
                            pageSize = feedbackResourceParameters.PageSize
                        });
                default:
                    return _urlHelper.Link("GetFeedbacks",
                        new
                        {
                            orderBy = feedbackResourceParameters.OrderBy,
                            pageNumber = feedbackResourceParameters.PageNumber,
                            pageSize = feedbackResourceParameters.PageSize
                        });
            }
        }

        [AllowAnonymous]
        [HttpGet("user/{userId}", Name = "GetFeedbacksForUser")]
        public IActionResult GetFeedbacksForUser(Guid userId)
        {
            if (!_userRepository.UserExists(userId))
            {
                return NotFound(new { message = $"User {userId} does not exist" });
            }

            var feedbacksForUserFromRepo = _feedbackRepository.GetFeedbacksForUser(userId);

            var feedbacksForUser = _mapper.Map<IEnumerable<FeedbackDto>>(feedbacksForUserFromRepo);

            if (!feedbacksForUser.Any())
            {
                return NoContent();
            }

            return Ok(feedbacksForUser);
        }

        [AllowAnonymous]
        [HttpGet("{id}/user/{userId}", Name = "GetFeedbackForUser")]
        public IActionResult GetFeedbackForUser(Guid id, Guid userId)
        {
            if (!_userRepository.UserExists(userId))
            {
                return NotFound(new { message = $"User {userId} does not exist" });
            }

            var feedbackForUserFromRepo = _feedbackRepository.GetFeedbackForUser(userId, id);

            if (feedbackForUserFromRepo == null)
            {
                return NotFound(new { message = $"Feedback {id} for User {userId} does not exist" });
            }

            var feedbackForUser = _mapper.Map<FeedbackDto>(feedbackForUserFromRepo);

            return Ok(feedbackForUser);
        }

        [AllowAnonymous]
        [HttpPost("user/{userId}")]
        public IActionResult AddFeedbackForUser(Guid userId, [FromBody] FeedbackForCreationDto feedback)
        {
            try
            {
                if (!_userRepository.UserExists(userId))
                {
                    return NotFound(new { message = $"User {userId} does not exist" });
                }

                var feedbackEntity = _mapper.Map<Feedback>(feedback);

                _feedbackRepository.AddFeedbackForUser(userId, feedbackEntity);

                if (!_feedbackRepository.Save())
                {
                    throw new Exception($"Adding a feedback for user {userId} failed on save.");
                }

                var feedbackToReturn = _mapper.Map<FeedbackDto>(feedbackEntity);

                return CreatedAtRoute("GetFeedbackForUser", new {userId = userId, id = feedbackToReturn.Id }, feedbackToReturn);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        [AllowAnonymous]
        [HttpDelete("{id}/user/{userId}")]
        public IActionResult DeleteFeedbackForUser(Guid id, Guid userId)
        {

            if (!_userRepository.UserExists(userId))
            {
                return NotFound(new { message = $"User {userId} does not exist" });
            }

            var feedbackForUserFromRepo = _feedbackRepository.GetFeedbackForUser(userId, id);

            if (feedbackForUserFromRepo == null)
            {
                return NotFound(new { message = $"Feedback {id} for User {userId} does not exist" });
            }

            _feedbackRepository.DeleteFeedback(feedbackForUserFromRepo);

            if (!_feedbackRepository.Save())
            {
                throw new Exception($"Deleting feedback {id} failed on save");
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public IActionResult UpdateFeedback(Guid id, [FromBody] FeedbackForUpdateDto feedbackForUpdateDto)
        {
            if (feedbackForUpdateDto == null)
            {
                return BadRequest();
            }

            var feedbackFromRepo = _feedbackRepository.GetFeedback(id);

            if (feedbackFromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(feedbackForUpdateDto, feedbackFromRepo);

            _feedbackRepository.UpdateFeedback(feedbackFromRepo);

            if (!_feedbackRepository.Save())
            {
                throw new Exception($"Updating feedback {id} failed on save");
            }

            return NoContent();
        }
    }
}

