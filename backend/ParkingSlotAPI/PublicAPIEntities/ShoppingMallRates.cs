using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.PublicAPIEntities
{
    public class ShoppingMallRates
    {
        public string help { get; set; }
        public bool success { get; set; }
        public Resultss result { get; set; }
    }

    public class Resultss
    {
        public string resource_id { get; set; }
        public Fieldss[] fields { get; set; }
        public Recordsss[] records { get; set; }
        public _Links _linksss { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
    }

    public class _Linksss
    {
        public string start { get; set; }
        public string next { get; set; }
    }

    public class Fieldss
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class Recordsss
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
