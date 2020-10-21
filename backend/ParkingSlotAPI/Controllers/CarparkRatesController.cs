using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ParkingSlotAPI.Entities;
using ParkingSlotAPI.PublicAPI;
using ParkingSlotAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarparkRatesController : ControllerBase
    {
        private readonly IFetchPublicAPI _fetchPublicAPI;
		private readonly ICarparkRatesRepository ICarparkRateRepository;
		private readonly IMapper _mapper;

		public CarparkRatesController(IFetchPublicAPI fetchPublicAPI, ICarparkRatesRepository calRepository, IMapper mapper)
        {
            _fetchPublicAPI = fetchPublicAPI;
			ICarparkRateRepository = calRepository;
			_mapper = mapper;
		}

		[HttpGet("{id}")]
		public ActionResult getCarparkRateById(Guid id)

		{
			var carpark = ICarparkRateRepository.GetCarparkRateByIdPerHour(id);
			List<CarparkRate> CarParkRateList = carpark.ToList();
			return Ok(CarParkRateList);
        }
    }
}