using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.PublicAPIEntities
{

    public class HDBAvailabilityEntity
    {
        public Item[] items { get; set; }
    }

    public class Item
    {
        public DateTime timestamp { get; set; }
        public Carpark_Data[] carpark_data { get; set; }
    }

    public class Carpark_Data
    {
        public Carpark_Info[] carpark_info { get; set; }
        public string carpark_number { get; set; }
        public DateTime update_datetime { get; set; }
    }

    public class Carpark_Info
    {
        public string total_lots { get; set; }
        public string lot_type { get; set; }
        public string lots_available { get; set; }
    }

}
