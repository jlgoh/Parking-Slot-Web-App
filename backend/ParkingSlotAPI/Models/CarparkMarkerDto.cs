using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Models
{
    public class CarparkMarkerDto
    {
        public Guid Id { get; set; }
        public string CarparkId { get; set; }
        public string XCoord { get; set; }
        public string YCoord { get; set; }
    }
}
