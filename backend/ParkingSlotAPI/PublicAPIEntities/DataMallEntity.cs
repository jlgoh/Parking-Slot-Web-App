using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.PublicAPIEntities
{

    public class DataMallEntity
    {
        public string odatametadata { get; set; }
        public Value[] value { get; set; }
    }

    public class Value
    {
        public string CarParkID { get; set; }
        public string Area { get; set; }
        public string Development { get; set; }
        public string Location { get; set; }
        public int AvailableLots { get; set; }
        public string LotType { get; set; }
        public string Agency { get; set; }
    }
}
