using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Entities
{
    public class Favorite
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("CarparkId")]
        public Carpark Carpark {get; set; }

        public Guid CarparkId { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; }

        public Guid UserId { get; set; }

    }
}