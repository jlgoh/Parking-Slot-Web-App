using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Models
{
    public class CarparkDto
    {
        public Guid Id { get; set; }

        public string CarparkId { get; set; }

        public string CarparkName { get; set; }

        public string LotType { get; set; }

        public string AgencyType { get; set; }

        public string ParkingSystem { get; set; }

        public bool IsCentral { get; set; }

        public string Address { get; set; }

        public string XCoord { get; set; }
        public string YCoord { get; set; }
        public int TotalAvailableLots { get; set; }
        public int TotalLots { get; set; }
        public int CarAvailability { get; set; }
        public int MAvailability { get; set; }
        public int HVAvailability { get; set; }
        public int CarCapacity { get; set; }
        public int MCapacity { get; set; }
        public int HVCapacity { get; set; }
        public double Price { get; set; }
    }
}
