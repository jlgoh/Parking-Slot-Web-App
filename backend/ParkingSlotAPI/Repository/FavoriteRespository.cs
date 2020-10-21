using System;
using System.Collections.Generic;
using System.Linq;
using ParkingSlotAPI.Entities;
using ParkingSlotAPI.Database;
using System.Threading.Tasks;
using ParkingSlotAPI.Helpers;

namespace ParkingSlotAPI.Repository
{
    public interface IFavoriteRepository
    {
        IEnumerable<Favorite> GetFavoritesForUser(Guid userId);
        Favorite GetFavoriteForUser(Guid userId, Guid carparkId);
        void AddFavoriteForUser(Guid userId, Favorite favorite);
        void SaveFavorite(Favorite favorite);
        void DeleteFavorite(Favorite favorite);
        bool Save();
    }
    public class FavoriteRepository : IFavoriteRepository
    {
        private ParkingContext _context;
        private IUserRepository _userRepository;
        public FavoriteRepository(ParkingContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        public IEnumerable<Favorite> GetFavoritesForUser(Guid userId)
        {
            return _context.Favorites.Where(f => f.UserId == userId).OrderBy(f => f.CarparkId).ToList();
        }

        public Favorite GetFavoriteForUser(Guid userId, Guid carparkId)
        {
            return _context.Favorites.FirstOrDefault(f => f.UserId == userId && f.CarparkId == carparkId);
        }

        private bool FavoriteExists(Guid userId, Guid carparkId)
        {
            return _context.Favorites.Any(a => a.UserId == userId && a.CarparkId == carparkId);
        }

        public void AddFavoriteForUser(Guid userId, Favorite favorite)
        {
            var user = _userRepository.GetUser(userId);

            if (user != null)
            {
                if (FavoriteExists(userId, favorite.CarparkId))
                {
                    throw new AppException($"This carpark {favorite.CarparkId} already exists as a favorite for user {userId}!");
                }
                else
                {
                    if (favorite.Id == Guid.Empty)
                    {
                        favorite.Id = Guid.NewGuid();
                    }

                    user.Favorites.Add(favorite);
                }
            }
        }

        public void SaveFavorite(Favorite favorite)
        {
            favorite.Id = Guid.NewGuid();
            _context.Favorites.Add(favorite);
        }

        public void DeleteFavorite(Favorite favorite)
        {
            _context.Favorites.Remove(favorite);
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}

