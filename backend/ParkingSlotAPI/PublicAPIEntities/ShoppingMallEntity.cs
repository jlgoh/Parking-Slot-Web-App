using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.PublicAPIEntities
{

    public class ShoppingMallEntity
    {
        public string help { get; set; }
        public bool success { get; set; }
        public Resultsq result { get; set; }
    }

    public class Resultsq
    {
        public string resource_id { get; set; }
        public Fields[] fields { get; set; }
        public Records[] records { get; set; }
        public _Linkss _links { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
    }

    public class _Linkss
    {
        public string start { get; set; }
        public string next { get; set; }
    }

    public class Fields
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class Records
    {
        public string category { get; set; }
        public string saturday_rate { get; set; }
        public string sunday_publicholiday_rate { get; set; }
        public string carpark { get; set; }
        public string weekdays_rate_1 { get; set; }
        public string weekdays_rate_2 { get; set; }
        public int _id { get; set; }
    }

}
