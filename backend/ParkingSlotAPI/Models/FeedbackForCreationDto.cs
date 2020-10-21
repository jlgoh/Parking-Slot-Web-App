﻿using ParkingSlotAPI.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Models
{
    public class FeedbackForCreationDto
    {
        [Required]
        [MaxLength(50)]
        public string Topic { get; set; }

        [Required]
        [MaxLength(50)]
        public string Description { get; set; }
    }
}
