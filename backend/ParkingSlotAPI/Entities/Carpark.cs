using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Entities
{
    public class Carpark
    {
        [Required]
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string CarparkId { get; set; }

        [Required]
        public string CarparkName { get; set; }

        public ICollection<CarparkRate> CarparkRates { get; set; }
            = new List<CarparkRate>();

        public string ParkingSystem { get; set; }
        public int TotalAvailableLots { get; set; }
        public int TotalLots { get; set; }
        [Required]
        public string AgencyType { get; set; }
        public string Address { get; set; }
        public string XCoord { get; set; }
        public string YCoord { get; set; }
        public bool IsCentral { get; set; }
        public string LotType { get; set; }
        public int CarAvailability { get; set; }
        public int HVAvailability { get; set; }
        public int MAvailability { get; set; }
        public int CarCapacity { get; set; }
        public int HVCapacity { get; set; }
        public int MCapacity { get; set; }
        public double Price { get; set; }
    }
}
