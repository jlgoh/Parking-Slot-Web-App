using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Models
{
    public class CarparkRateDto
    {
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
