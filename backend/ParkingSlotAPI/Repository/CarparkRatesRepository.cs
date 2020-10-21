using Microsoft.EntityFrameworkCore;
using ParkingSlotAPI.Database;
using ParkingSlotAPI.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Repository
{
    public interface ICarparkRatesRepository
    {
        IEnumerable<CarparkRate> GetCarparkRates(Guid carparkId);
        IEnumerable<CarparkRate> GetCarparkRateById(Guid carparkId, string vehicletype);
		IEnumerable<CarparkRate> GetCarparkRateByIdPerHour(Guid carparkId);

	}

    public class CarparkRatesRepository : ICarparkRatesRepository
    {
        private readonly ParkingContext _context;

        public CarparkRatesRepository(ParkingContext context)
        {
            _context = context;
        }

        public IEnumerable<CarparkRate> GetCarparkRates(Guid carparkId)
        {
            return _context.CarparkRates.Where(a => a.CarparkId == carparkId);
        }

        public IEnumerable<CarparkRate> GetCarparkRateById(Guid carparkId, string vehicletype)
        {
            return _context.CarparkRates.Where(o => o.CarparkId == carparkId && o.VehicleType == vehicletype);
        }
		public IEnumerable<CarparkRate> GetCarparkRateByIdPerHour(Guid carparkId)
		{
			return _context.CarparkRates.Where(o => o.CarparkId == carparkId );
		}

	}
}
