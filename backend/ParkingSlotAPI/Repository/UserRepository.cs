using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ParkingSlotAPI.Database;
using ParkingSlotAPI.Entities;
using ParkingSlotAPI.Helpers;
using ParkingSlotAPI.Models;
using ParkingSlotAPI.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Repository
{
    public interface IUserRepository
    {
        User Authenticate(string username, string password);
        PagedList<User> GetUsers(UserResourceParameters userResourceParameters);
        User Create(User user, string password);
        User GetUser(Guid userId);
        User GetUserByEmail(string Email);
        void UpdateUser(Guid userId, User userParam);
        void UpdatePassword(User user, string password);
        void DeleteUser(User user);
        bool UserExists(Guid userId);
        bool Save();
    }

    public class UserRepository : IUserRepository
    {

        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {
            new User { FirstName = "Andrew", LastName = "Cai", Email="andrewcai19972011@gmail.com", Id = Guid.NewGuid(), Role = "Admin" }
        };


        private ParkingContext _context;
        private readonly AppSettings _appSettings;
        private readonly IPropertyMappingService _propertyMappingService;

        public UserRepository(ParkingContext context, IOptions<AppSettings> appSettings, IPropertyMappingService propertyMappingService)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _propertyMappingService = propertyMappingService;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(x => x.Username == username);


            if (user == null)
                return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        public User Create(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");
            
            if (_context.Users.Any(x => x.Username == user.Username))
                throw new AppException("Username \"" + user.Username + "\" is already taken");

            if (user.Email != null)
            {
                if (_context.Users.Any(x => x.Email == user.Email))
                    throw new AppException("Email \"" + user.Email + "\" is already taken");
            }

            if (user.PhoneNumber != null)
            {
                if (_context.Users.Any(x => x.PhoneNumber == user.PhoneNumber))
                    throw new AppException("Phone number \"" + user.PhoneNumber + "\" is already taken");
            }

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public PagedList<User> GetUsers([FromQuery] UserResourceParameters userResourceParameters)
        {
            var collectionBeforePaging =
                _context.Users.ApplySort(userResourceParameters.OrderBy,
                _propertyMappingService.GetPropertyMapping<UserDto, User>());

            return PagedList<User>.Create(collectionBeforePaging, userResourceParameters.PageNumber, userResourceParameters.PageSize);

        }

        public User GetUser(Guid userId)
        {
            return _context.Users.FirstOrDefault(a => a.Id == userId);
        }

        public User GetUserByEmail(string Email)
        {
            return _context.Users.FirstOrDefault(a => a.Email == Email);
        }

        public void AddUser(User user)
        {
            user.Id = Guid.NewGuid();
            _context.Users.Add(user);
        }

        public void UpdateUser(Guid userId, User userParam)
        {
            var user = GetUser(userId);

            if (user == null)
            {
                throw new AppException("User not found");
            }

            // Update user properties
            user.FirstName = string.IsNullOrEmpty(userParam.FirstName) ? user.FirstName : userParam.FirstName;
            user.LastName = string.IsNullOrEmpty(userParam.LastName) ? user.LastName : userParam.LastName;
            user.Email = string.IsNullOrEmpty(userParam.Email) ? user.Email : userParam.Email;
            user.PhoneNumber = string.IsNullOrEmpty(userParam.PhoneNumber) ? user.PhoneNumber : userParam.PhoneNumber;

            _context.Users.Update(user);
        }

        public void UpdatePassword(User user, string password)
        {
            byte[] passwordHash, passwordSalt;

            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Update(user);

            if (!Save())
            {
                throw new AppException("Error in updating password.");
            }
        }

        public void DeleteUser(User user)
        {
            _context.Users.Remove(user);
        }

        public bool UserExists(Guid userId)
        {
            return _context.Users.Any(a => a.Id == userId);
        }
        
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}
