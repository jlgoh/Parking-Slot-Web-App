using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Models
{
    public class FeedbackForUpdateDto
    {
        public bool IsResolved { get; set; }
        public string Comments { get; set; }
    }
}
