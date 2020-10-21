using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Models
{
    public class UserForAuthenticationDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
