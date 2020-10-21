using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkingSlotAPI.Entities;
using ParkingSlotAPI.Helpers;
using ParkingSlotAPI.Models;
using ParkingSlotAPI.PublicAPI;
using ParkingSlotAPI.Repository;

namespace ParkingSlotAPI.Controllers
{
    static class Timer
    {
        public static DateTime RequestedDT = DateTime.Now;
    }


    [Route("api/[controller]")]
    [ApiController]
    public class CarparkController : ControllerBase
    {
        private readonly IParkingRepository _parkingRepository;
        private readonly ICarparkRatesRepository _carparkRatesRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;

        FetchPublicAPI publicAPI = new FetchPublicAPI();

        const int maxParkingPageSize = 20;

        public CarparkController(IParkingRepository parkingRepository, IMapper mapper, IUrlHelper urlHelper, ICarparkRatesRepository carparkRatesRepository)
        {
            _parkingRepository = parkingRepository;
            _mapper = mapper;
            _urlHelper = urlHelper;
            _carparkRatesRepository = carparkRatesRepository;
        }

        [HttpGet("All")]
        public IActionResult GetAllCarparks()
        {
            var carparksFromRepo = _parkingRepository.GetAllCarparks();

            if (carparksFromRepo == null)
            {
                return NotFound();
            }

            var carparks = _mapper.Map<IEnumerable<CarparkMarkerDto>>(carparksFromRepo);
            return Ok(carparks);

        }

        [HttpGet(Name = "GetCarparks")]
        public IActionResult GetCarparks([FromQuery] CarparkResourceParameters carparkResourceParameters)
        {
            try
            {
                var carparksFromRepo = _parkingRepository.GetCarparks(carparkResourceParameters);

                if (carparksFromRepo == null)
                {
                    return NotFound();
                }

                var previousPageLink = carparksFromRepo.HasPrevious ?
                    CreateCarparksResourceUri(carparkResourceParameters,
                    ResourceUriType.PreviousPage) : null;

                var x = CreateCarparksResourceUri(carparkResourceParameters,
                    ResourceUriType.NextPage);

                var nextPageLink = carparksFromRepo.HasNext ?
                    CreateCarparksResourceUri(carparkResourceParameters,
                    ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    totalCount = carparksFromRepo.TotalCount,
                    pageSize = carparksFromRepo.PageSize,
                    currentPage = carparksFromRepo.CurrentPage,
                    totalPages = carparksFromRepo.TotalPages,
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink
                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                var minutes = (DateTime.Now - Timer.RequestedDT).TotalMinutes;

                if (minutes > 1)
                {
                    List<Carpark> carparksToAdd = new List<Carpark>();

                    foreach (var carparkFromRepo in carparksFromRepo)
                    {
                        if (carparkFromRepo.AgencyType == "HDB")
                        {
                            var c = UpdateHDBAvailability(carparkFromRepo);
                            carparksToAdd.Add(c);
                        }
                        else if (carparkFromRepo.AgencyType == "LTA")
                        {
                            var c = UpdateLTAAvailability(carparkFromRepo);
                            carparksToAdd.Add(c);
                        }
                        else if (carparkFromRepo.AgencyType == "URA")
                        {
                            var c = UpdateURAAvailability(carparkFromRepo);
                            carparksToAdd.Add(c);
                        }
                    }

                    var carparks = _mapper.Map<IEnumerable<CarparkDto>>(carparksToAdd);

                    if (!carparks.Any())
                    {
                        return NoContent();
                    }

                    return Ok(carparks);
                }
                else
                {
                    var carparks = _mapper.Map<IEnumerable<CarparkDto>>(carparksFromRepo);
                    return Ok(carparks);
                }
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private string CreateCarparksResourceUri( CarparkResourceParameters carparkResourceParameters, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetCarparks",
                        new
                        {
                            searchQuery = carparkResourceParameters.SearchQuery,
                            agencyType = carparkResourceParameters.AgencyType,
                            pageNumber = carparkResourceParameters.PageNumber - 1,
                            pageSize = carparkResourceParameters.PageSize
                        });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetCarparks",
                        new
                        {
                            searchQuery = carparkResourceParameters.SearchQuery,
                            agencyType = carparkResourceParameters.AgencyType,
                            pageNumber = carparkResourceParameters.PageNumber + 1,
                            pageSize = carparkResourceParameters.PageSize
                        });
                default:
                    return _urlHelper.Link("GetCarparks",
                        new
                        {
                            searchQuery = carparkResourceParameters.SearchQuery,
                            agencyType = carparkResourceParameters.AgencyType,
                            pageNumber = carparkResourceParameters.PageNumber,
                            pageSize = carparkResourceParameters.PageSize
                        });
            }
        }

        private Carpark UpdateURAAvailability(Carpark carparkFromRepo)
        {
            var task = Task.Run(async () => await publicAPI.GetURAAvailability());

            var result = task.Result;

            var carparks = result.FindAll(a => a.carparkNo == carparkFromRepo.CarparkId);

            if (carparks != null)
            {
                foreach(var v in carparks)
                {
                    if (v.lotType == "C")
                    {
                        carparkFromRepo.CarAvailability = int.Parse(v.lotsAvailable);
                    }
                    else if (v.lotType == "M")
                    {
                        carparkFromRepo.MAvailability = int.Parse(v.lotsAvailable);
                    }
                    else if (v.lotType == "H")
                    {
                        carparkFromRepo.HVAvailability = int.Parse(v.lotsAvailable);
                    }
                }

                if (carparks.Count == 0)
                {
                    carparkFromRepo.TotalAvailableLots = -1;
                }
                else
                {
                    carparkFromRepo.TotalAvailableLots = carparkFromRepo.CarAvailability + carparkFromRepo.MAvailability + carparkFromRepo.HVAvailability;
                }



                _parkingRepository.UpdateCarpark(carparkFromRepo);

                _parkingRepository.SaveChanges();
            }
            else
            {
                carparkFromRepo.TotalAvailableLots = -1;
                carparkFromRepo.CarAvailability = -1;
                carparkFromRepo.MAvailability = -1;
                carparkFromRepo.HVAvailability = -1;

                _parkingRepository.UpdateCarpark(carparkFromRepo);

                _parkingRepository.SaveChanges();
            }

            Timer.RequestedDT = DateTime.Now;

            return carparkFromRepo;

        }

        private Carpark UpdateLTAAvailability(Carpark carparkFromRepo)
        {
            var task = Task.Run(async () => await publicAPI.GetParkingInfoAsync());

            var result = task.Result;

            var v = result.FirstOrDefault(a => a.CarparkId == carparkFromRepo.CarparkId);

            if (v != null)
            {
                carparkFromRepo.CarAvailability = v.CarAvailability;
                carparkFromRepo.TotalAvailableLots = v.TotalAvailableLots;

                _parkingRepository.UpdateCarpark(carparkFromRepo);

                _parkingRepository.SaveChanges();

            }

            Timer.RequestedDT = DateTime.Now;

            return carparkFromRepo;
       
        }

        private Carpark UpdateHDBAvailability(Carpark carparkFromRepo)
        {
            var task = Task.Run(async () => await publicAPI.GetHDBAvailabilityAsync());

            var result = task.Result;

            var v = result.FirstOrDefault(a => a.carpark_number.Equals(carparkFromRepo.CarparkId));

            if (v != null)
            {
                if (v.carpark_info.Length > 0)
                {
                    foreach (var x in v.carpark_info)
                    {
                        if (x.lot_type.Equals("C"))
                        {
                            carparkFromRepo.CarAvailability = int.Parse(x.lots_available);
                        }
                        else if (x.lot_type.Equals("H"))
                        {
                            carparkFromRepo.HVAvailability = int.Parse(x.lots_available);
                        }
                        else if (x.lot_type.Equals("Y"))
                        {
                            carparkFromRepo.MAvailability = int.Parse(x.lots_available);
                        }
                    }

                    carparkFromRepo.TotalAvailableLots = carparkFromRepo.CarAvailability + carparkFromRepo.HVAvailability + carparkFromRepo.MAvailability;

                    _parkingRepository.UpdateCarpark(carparkFromRepo);

                    _parkingRepository.SaveChanges();
                }
            }

            Timer.RequestedDT = DateTime.Now;

            return carparkFromRepo;
        }

        [HttpGet("{id}")]
        public IActionResult GetCarpark(Guid id)
        {
            var carparkFromRepo = _parkingRepository.GetCarpark(id);

            if (carparkFromRepo == null)
            {
                return NotFound();
            }

            var minutes = (DateTime.Now - Timer.RequestedDT).TotalMinutes;

            if (minutes > 1)
            {
                if (carparkFromRepo.AgencyType == "HDB")
                {
                    carparkFromRepo = UpdateHDBAvailability(carparkFromRepo);

                    var carpark = _mapper.Map<CarparkDto>(carparkFromRepo);

                    return Ok(carpark);
                }
                else if (carparkFromRepo.AgencyType == "LTA")
                {
                    carparkFromRepo = UpdateLTAAvailability(carparkFromRepo);

                    var carpark = _mapper.Map<CarparkDto>(carparkFromRepo);

                    return Ok(carpark);
                }
                else if (carparkFromRepo.AgencyType == "URA")
                {
                    carparkFromRepo = UpdateURAAvailability(carparkFromRepo);

                    var carpark = _mapper.Map<CarparkDto>(carparkFromRepo);

                    return Ok(carpark);
                }
            }
            else
            {
                var carpark = _mapper.Map<CarparkDto>(carparkFromRepo);

                return Ok(carpark);
            }

            return BadRequest();
        }
    }
}