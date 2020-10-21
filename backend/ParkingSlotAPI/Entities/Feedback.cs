using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Entities
{
    public class Feedback
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Topic { get; set; }

        [Required]
        [MaxLength(50)]
        public string Description { get; set; }

        public bool IsResolved { get; set; }

        [Required]
        [MaxLength(50)]
        public string Comments { get; set; }
    }
}

