using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.PublicAPIEntities
{

    public class URACarparkInfo
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public Results[] Result { get; set; }
    }

    public class Results
    {
        public string weekdayMin { get; set; }
        public string weekdayRate { get; set; }
        public string ppCode { get; set; }
        public string parkingSystem { get; set; }
        public string ppName { get; set; }
        public string vehCat { get; set; }
        public string satdayMin { get; set; }
        public string satdayRate { get; set; }
        public string sunPHMin { get; set; }
        public string sunPHRate { get; set; }
        public Geometry[] geometries { get; set; }
        public string startTime { get; set; }
        public int parkCapacity { get; set; }
        public string endTime { get; set; }
        public string remarks { get; set; }
    }

    public class Geometry
    {
        public string coordinates { get; set; }
    }

}
