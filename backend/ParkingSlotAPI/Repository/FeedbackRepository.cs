using System;
using System.Collections.Generic;
using System.Linq;
using ParkingSlotAPI.Entities;
using ParkingSlotAPI.Database;
using System.Threading.Tasks;
using ParkingSlotAPI.Helpers;
using ParkingSlotAPI.Models;
using ParkingSlotAPI.Services;

namespace ParkingSlotAPI.Repository
{
    public interface IFeedbackRepository
    {
        PagedList<Feedback> GetFeedbacks(FeedbackResourceParameters feedbackResourceParameters);
        Feedback GetFeedbackForUser(Guid userId, Guid feedbackId);
        IEnumerable<Feedback> GetFeedbacksForUser(Guid id);
        void AddFeedbackForUser(Guid userId, Feedback feedback);
        void DeleteFeedback(Feedback feedback);
        void UpdateFeedback(Feedback feedback);
        Feedback GetFeedback(Guid feedbackId);
        bool Save();
    }

    public class FeedbackRepository : IFeedbackRepository
    {
        private ParkingContext _context;
        private IUserRepository _userRepository;
        private readonly IPropertyMappingService _propertyMappingService;
        public FeedbackRepository(ParkingContext context, IUserRepository userRepository, IPropertyMappingService propertyMappingService)
        {
            _context = context;
            _userRepository = userRepository;
            _propertyMappingService = propertyMappingService;
        }

        public PagedList<Feedback> GetFeedbacks(FeedbackResourceParameters feedbackResourceParameters)
        {
            var collectionBeforePaging =
                _context.Feedbacks.ApplySort(feedbackResourceParameters.OrderBy,
                _propertyMappingService.GetPropertyMapping<FeedbackDto, Feedback>());

            return PagedList<Feedback>.Create(collectionBeforePaging, feedbackResourceParameters.PageNumber, feedbackResourceParameters.PageSize);

        }

        public IEnumerable<Feedback> GetFeedbacksForUser(Guid id)
        {
            return _context.Feedbacks.Where(a => a.UserId == id);
        }

        public Feedback GetFeedbackForUser(Guid userId, Guid feedbackId)
        {
            return _context.Feedbacks.FirstOrDefault(a => a.Id == feedbackId && a.UserId == userId);
        }

        public Feedback GetFeedback(Guid feedbackId)
        {
            return _context.Feedbacks.FirstOrDefault(a => a.Id == feedbackId);
        }

        private bool FeedbackExists(Guid userId, Guid feedbackId)
        {
            return _context.Feedbacks.Any(a => a.Id == userId && a.Id == feedbackId);
        }

        public void AddFeedbackForUser(Guid userId, Feedback feedback)
        {
            var user = _userRepository.GetUser(userId);

            if (user != null)
            {
                if (FeedbackExists(userId, feedback.Id))
                {
                    throw new AppException($"This feedback {feedback.Id} already exists for user {userId}!");
                }
                else
                {
                    if (feedback.Id == Guid.Empty)
                    {
                        feedback.Id = Guid.NewGuid();
                    }

                    feedback.IsResolved = false;
                    user.Feedbacks.Add(feedback);
                }
            }
        }

        public void DeleteFeedback(Feedback feedback)
        {
            _context.Feedbacks.Remove(feedback);
        }

        public void UpdateFeedback(Feedback feedback)
        {
            _context.Feedbacks.Update(feedback);
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}

