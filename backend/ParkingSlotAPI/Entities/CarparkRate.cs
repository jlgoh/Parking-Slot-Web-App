using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Entities
{
    public class CarparkRate
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey("CarparkId")]
        public Carpark Carpark { get; set; }
        public Guid CarparkId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string WeekdayRate { get; set; }
        public string WeekdayMin { get; set; }
        public string SatdayRate { get; set; }
        public string SatdayMin { get; set; }
        public string SunPHRate { get; set; }
        public string SunPHMin { get; set; }
        public string VehicleType { get; set; }
        public string Remarks { get; set; }
		public string Duration { get; set; }
	}
}
