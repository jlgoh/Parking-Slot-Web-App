using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Models
{
    public class FeedbackDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public string IsResolved { get; set; }
    }   
}

