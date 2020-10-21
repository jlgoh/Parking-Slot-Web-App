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
using Microsoft.AspNetCore.Authorization;
using ParkingSlotAPI.Helpers;

namespace ParkingSlotAPI.Controllers
{
    [Route("api/users/{UserId}/[controller]")]
    [ApiController]
    public class FavoritesController : ControllerBase
    {

        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        
        public FavoritesController(IFavoriteRepository favoriteRepository, IUserRepository userRepository, IMapper mapper)
        {
            _favoriteRepository = favoriteRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetFavoritesForUser(Guid userId)
        {
            if (!_userRepository.UserExists(userId))
            {
                return NotFound(new { message = $"User {userId} does not exist" });
            }

            var favoritesForUserFromRepo = _favoriteRepository.GetFavoritesForUser(userId);

            var favoritesForAuthor = _mapper.Map<IEnumerable<FavoriteDto>>(favoritesForUserFromRepo);

            if (!favoritesForAuthor.Any())
            {
                return NoContent();
            }

            return Ok(favoritesForAuthor);
        }

        [HttpGet("{id}", Name = "GetFavoriteForUser")]
        public IActionResult GetFavoriteForUser(Guid userId, Guid id)
        {
            if (!_userRepository.UserExists(userId))
            {
                return NotFound(new { message = $"User {userId} does not exist" });
            }

            var favoriteForAuthorFromRepo = _favoriteRepository.GetFavoriteForUser(userId, id);

            if (favoriteForAuthorFromRepo == null)
            {
                return NotFound(new { message = $"Favorite {id} for User {userId} does not exist" });
            }

            var favoriteForAuthor = _mapper.Map<FavoriteDto>(favoriteForAuthorFromRepo);

            return Ok(favoriteForAuthor);
        }

        [HttpPost]
        public IActionResult AddFavoriteForUser(Guid userId,[FromBody] FavoriteForCreationDto favorite)
        {
            try
            {
                if (!_userRepository.UserExists(userId))
                {
                    return NotFound(new { message = $"User {userId} does not exist" });
                }

                var favoriteEntity = _mapper.Map<Favorite>(favorite);

                _favoriteRepository.AddFavoriteForUser(userId, favoriteEntity);

                if (!_favoriteRepository.Save())
                {
                    throw new AppException($"Adding a favorite for user {userId} failed on save.");
                }

                var favoriteToReturn = _mapper.Map<FavoriteDto>(favoriteEntity);

                return CreatedAtRoute("GetFavoriteForUser", new { userId = userId, id = favoriteToReturn.Id }, favoriteToReturn);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{carparkId}")]
        public IActionResult DeleteFavoriteForUser(Guid userId, Guid carparkId)
        {
            if (!_userRepository.UserExists(userId))
            {
                return NotFound(new { message = $"User {userId} does not exist" });
            }

            var favoriteForUserFromRepo = _favoriteRepository.GetFavoriteForUser(userId, carparkId);

            if (favoriteForUserFromRepo == null)
            {
                return NotFound(new { message = $"Favorite carpark {carparkId} for User {userId} does not exist" });
            }

            _favoriteRepository.DeleteFavorite(favoriteForUserFromRepo);

            if (!_favoriteRepository.Save())
            {
                throw new Exception($"Deleting favorite carpark {carparkId} for user {userId} failed on save");
            }

            return NoContent();
        }
    }
}

