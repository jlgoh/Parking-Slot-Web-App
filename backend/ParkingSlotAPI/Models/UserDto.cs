using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Models
{
    public class Users
    {
        public IEnumerable<UserDto> UserDtos { get; set; }
            = new List<UserDto>();
        public int TotalCount { get; set; }
    }

    
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }
}
