using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        public ICollection<Favorite> Favorites { get; set; }
            = new List<Favorite>();

        public ICollection<Feedback> Feedbacks { get; set; }
            = new List<Feedback>();

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }
       
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        [Required]
        [MaxLength(50)]
        public string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
