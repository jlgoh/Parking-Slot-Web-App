using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.PublicAPIEntities
{
    public class URACarparkAvailability
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public Detail[] Result { get; set; }
    }

    public class Detail
    {
        public string carparkNo { get; set; }
        public string lotsAvailable { get; set; }
        public string lotType { get; set; }
        public Geometries[] geometries { get; set; }
    }

    public class Geometries
    {
        public string coordinates { get; set; }
    }
}
