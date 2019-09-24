﻿using System;
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

        public string Area { get; set; }

        public string AgencyType { get; set; }

        public string Address { get; set; }

        public string XCoord { get; set; }
        public string YCoord { get; set; }

    }
}