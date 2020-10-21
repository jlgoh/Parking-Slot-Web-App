using AutoMapper;
using ParkingSlotAPI.Controllers;
using ParkingSlotAPI.Database;
using ParkingSlotAPI.Entities;
using ParkingSlotAPI.Helpers;
using ParkingSlotAPI.Models;
using ParkingSlotAPI.Services;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Repository
{
    public interface IParkingRepository
    {
        PagedList<Carpark> GetCarparks(CarparkResourceParameters carparkResourceParameters);
        Carpark GetCarpark(Guid carparkId);
        IEnumerable<Carpark> GetAllCarparks();
        void AddCarpark(Carpark carpark);
        void DeleteCarpark(Carpark carpark);
        void UpdateCarpark(Carpark carpark);
        void UpdateCarparks(List<Carpark> carparks);
        void SaveChanges();
    }

    public class ParkingRepository : IParkingRepository
    {
        private ParkingContext _context;
        private IMapper _mapper;
        private ICarparkRatesRepository _carparkRatesRepository;
        private readonly IPropertyMappingService _propertyMappingService;
        public ParkingRepository(ParkingContext context, IMapper mapper, ICarparkRatesRepository carparkRatesRepository, IPropertyMappingService propertyMappingService)
        {
            _context = context;
            _mapper = mapper;
            _carparkRatesRepository = carparkRatesRepository;
            _propertyMappingService = propertyMappingService;
        }

        public IEnumerable<Carpark> GetAllCarparks()
        {
            return _context.Carparks.OrderBy(a => a.CarparkId);
        }

        public double CalculateCarparkPrice(Guid id, DateTime StartTime,DateTime EndTime, String vehicleType)
        {
            var duration = 0.0;

            double Price = 0;
            Boolean IsNUll = false, NonExistenceCarparkRate = false, InvalidDate = false, FlatRateExistence = false;
            DateTime RateStartTimeFromDB = new DateTime(1, 1, 1); ;
            DateTime RateEndTimeFromDB = new DateTime(1, 1, 1);
            var carpark = _carparkRatesRepository.GetCarparkRateById(id, vehicleType);
            List<CarparkRate> CarParkRateList = carpark.ToList();


			if (EndTime.ToString() == "1/1/0001 12:00:00 AM" && StartTime.ToString() == "1/1/0001 12:00:00 AM")
			{
				duration = 60;
			}
			else if (EndTime.ToString() != "1/1/0001 12:00:00 AM" && StartTime.ToString() == "1/1/0001 12:00:00 AM")
			{
				StartTime = DateTime.Now;
				StartTime = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day, StartTime.Hour, StartTime.Minute, 0);

				duration = (EndTime - StartTime).TotalMinutes;
			}
			else
			{
				duration = (EndTime - StartTime).TotalMinutes;
			}
			if (duration >= 0)
			{
				if (CarParkRateList.Count != 0)
				{

					var result = new Calculation(StartTime, (int)duration);
					List<HoursPerDay> dayOfWeek = result.getparkingDay(StartTime, EndTime);


					foreach (HoursPerDay EachHoursPerDay in dayOfWeek)
					{




						if (EachHoursPerDay.getDay() > 0 && EachHoursPerDay.getDay() < 6)
						{
							//weekdays calculation
							if (vehicleType != "Motorcycle")
							{
								double TimeChecker = EachHoursPerDay.getDayDuration();

								while (TimeChecker != 0)
								{
									int checkAllIfFailed = 0;
									for (int i = 0; i < CarParkRateList.Count; i++)
									{
										if ((Convert.ToInt32(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's'))) <= 30)
										{
											double durationOfStaticTimeInMin = Convert.ToDouble(CarParkRateList[i].Duration) * 60;
											if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
											{
												RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

												RateEndTimeFromDB = RateStartTimeFromDB.AddMinutes(durationOfStaticTimeInMin);
											}
											else if (EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
											{

												RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
												RateStartTimeFromDB = RateEndTimeFromDB.Subtract(new TimeSpan(0, (int)durationOfStaticTimeInMin, 0));
											}
											else
											{
												RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
												RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
											}


											double durationOfDynamicTimeInMin = result.getPeriodDuration(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay());


											if (RateStartTimeFromDB.TimeOfDay == EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay &&
												durationOfDynamicTimeInMin <= durationOfStaticTimeInMin && TimeChecker > 0)
											{
												result.setDuration((int)EachHoursPerDay.getDayDuration());
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= durationOfDynamicTimeInMin;
												checkAllIfFailed--;
											}

											else if (TimeChecker > 0 && result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{


												result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);

												checkAllIfFailed--;

											}
											else if (TimeChecker > 0 && result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{
												double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
												result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));

												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - ((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue)));

												checkAllIfFailed--;

											}


											if (result.TimePeriodOverlaps(StartTime, EndTime, RateStartTimeFromDB, RateEndTimeFromDB) == false)
											{
												checkAllIfFailed++;


											}
											if (TimeChecker == 0)
											{
												break;
											}
										}
										else if ((Convert.ToInt32(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's'))) > 30)
										{
											FlatRateExistence = true;
										}

									}
									if (checkAllIfFailed >= CarParkRateList.Count)
									{
										NonExistenceCarparkRate = true;

										break;
									}
									/*	else if(FlatRateExistence==true&& TimeChecker!=0)
										{

										}*/
								}
							}
							else
							{
								//////motorcycle 
								double TimeChecker = EachHoursPerDay.getDayDuration();
								while (TimeChecker != 0)
								{
									int checkAllIfFailed = 0;
									for (int i = 0; i < CarParkRateList.Count; i++)
									{
										double durationOfStaticTimeInMin = Convert.ToDouble(CarParkRateList[i].Duration) * 60;
										if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
										{
											RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

											RateEndTimeFromDB = RateStartTimeFromDB.AddMinutes(durationOfStaticTimeInMin);
										}
										else if (EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
										{

											RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
											RateStartTimeFromDB = RateEndTimeFromDB.Subtract(new TimeSpan(0, (int)durationOfStaticTimeInMin, 0));
										}
										else
										{
											RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
											RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
										}
										double durationOfDynamicTimeInMin = result.getPeriodDuration(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay());

										if (RateStartTimeFromDB.TimeOfDay == EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay &&
													durationOfDynamicTimeInMin <= durationOfStaticTimeInMin && TimeChecker > 0)
										{
											result.setDuration((int)EachHoursPerDay.getDayDuration());
											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
											TimeChecker -= durationOfDynamicTimeInMin;
											checkAllIfFailed--;
										}

										else if (result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{


											result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
											TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											checkAllIfFailed--;


										}
										else if (result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{
											double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
											result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));

											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
											TimeChecker -= ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - ((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue)));

											checkAllIfFailed--;
										}

										else
										{
											checkAllIfFailed++;


										}
									}
									if (checkAllIfFailed >= CarParkRateList.Count)
									{
										NonExistenceCarparkRate = true;

										break;
									}
								}

							}

						}
						else if (EachHoursPerDay.getDay() == 6)
						{
							//Sat calculation
							if (vehicleType != "Motorcycle")
							{
								double TimeChecker = EachHoursPerDay.getDayDuration();

								while (TimeChecker != 0)
								{
									int checkAllIfFailed = 0;
									for (int i = 0; i < CarParkRateList.Count; i++)
									{
										if ((Convert.ToInt32(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's'))) <= 30)
										{
											double durationOfStaticTimeInMin = Convert.ToDouble(CarParkRateList[i].Duration) * 60;
											if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
											{
												RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

												RateEndTimeFromDB = RateStartTimeFromDB.AddMinutes(durationOfStaticTimeInMin);
											}
											else if (EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
											{

												RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
												RateStartTimeFromDB = RateEndTimeFromDB.Subtract(new TimeSpan(0, (int)durationOfStaticTimeInMin, 0));
											}
											else
											{
												RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
												RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
											}
											double durationOfDynamicTimeInMin = result.getPeriodDuration(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay());


											if (RateStartTimeFromDB.TimeOfDay == EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay &&
												durationOfDynamicTimeInMin <= durationOfStaticTimeInMin && TimeChecker > 0)
											{
												result.setDuration((int)EachHoursPerDay.getDayDuration());
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= durationOfDynamicTimeInMin;
												checkAllIfFailed--;
											}

											else if (TimeChecker > 0 && result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{


												result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);

												checkAllIfFailed--;

											}
											else if (TimeChecker > 0 && result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{
												double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
												result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));

												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - ((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue)));

												checkAllIfFailed--;

											}


											if (result.TimePeriodOverlaps(StartTime, EndTime, RateStartTimeFromDB, RateEndTimeFromDB) == false)
											{
												checkAllIfFailed++;


											}
											if (TimeChecker == 0)
											{
												break;
											}
										}
										else if ((Convert.ToInt32(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's'))) > 30)
										{
											FlatRateExistence = true;
										}


									}
									if (checkAllIfFailed >= CarParkRateList.Count)
									{
										NonExistenceCarparkRate = true;

										break;
									}
									/*	else if(FlatRateExistence==true&& TimeChecker!=0)
									{

									}*/
								}
							}
							else
							{
								//motorcycle
								double TimeChecker = EachHoursPerDay.getDayDuration();
								while (TimeChecker != 0)
								{
									int checkAllIfFailed = 0;
									for (int i = 0; i < CarParkRateList.Count; i++)
									{
										double durationOfStaticTimeInMin = Convert.ToDouble(CarParkRateList[i].Duration) * 60;
										if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
										{
											RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

											RateEndTimeFromDB = RateStartTimeFromDB.AddMinutes(durationOfStaticTimeInMin);
										}
										else if (EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
										{

											RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
											RateStartTimeFromDB = RateEndTimeFromDB.Subtract(new TimeSpan(0, (int)durationOfStaticTimeInMin, 0));
										}
										else
										{
											RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
											RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
										}
										double durationOfDynamicTimeInMin = result.getPeriodDuration(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay());

										if (RateStartTimeFromDB.TimeOfDay == EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay &&
													durationOfDynamicTimeInMin <= durationOfStaticTimeInMin && TimeChecker > 0)
										{
											result.setDuration((int)EachHoursPerDay.getDayDuration());
											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
											TimeChecker -= durationOfDynamicTimeInMin;
											checkAllIfFailed--;
										}

										else if (result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{


											result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
											TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											checkAllIfFailed--;


										}
										else if (result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{
											double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
											result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));

											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));

											TimeChecker -= ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - ((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue)));

											checkAllIfFailed--;
										}

										else
										{
											checkAllIfFailed++;


										}
									}
									if (checkAllIfFailed >= CarParkRateList.Count)
									{
										NonExistenceCarparkRate = true;

										break;
									}
								}

							}
						}
						else
						{
							//SUN and PH calculation
							if (vehicleType != "Motorcycle")
							{
								double TimeChecker = EachHoursPerDay.getDayDuration();

								while (TimeChecker != 0)
								{
									int checkAllIfFailed = 0;
									for (int i = 0; i < CarParkRateList.Count; i++)
									{

										if ((Convert.ToInt32(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's'))) <= 30)
										{
											double durationOfStaticTimeInMin = Convert.ToDouble(CarParkRateList[i].Duration) * 60;
											if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
											{
												RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

												RateEndTimeFromDB = RateStartTimeFromDB.AddMinutes(durationOfStaticTimeInMin);
											}
											else if (EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
											{

												RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
												RateStartTimeFromDB = RateEndTimeFromDB.Subtract(new TimeSpan(0, (int)durationOfStaticTimeInMin, 0));
											}
											else
											{
												RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
												RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
											}
											double durationOfDynamicTimeInMin = result.getPeriodDuration(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay());


											if (RateStartTimeFromDB.TimeOfDay == EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay &&
												durationOfDynamicTimeInMin <= durationOfStaticTimeInMin && TimeChecker > 0)
											{
												result.setDuration((int)EachHoursPerDay.getDayDuration());
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= durationOfDynamicTimeInMin;
												checkAllIfFailed--;
											}

											else if (TimeChecker > 0 && result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{


												result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);

												checkAllIfFailed--;

											}
											else if (TimeChecker > 0 && result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{
												double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
												result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));

												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - ((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue)));

												checkAllIfFailed--;

											}


											if (result.TimePeriodOverlaps(StartTime, EndTime, RateStartTimeFromDB, RateEndTimeFromDB) == false)
											{
												checkAllIfFailed++;


											}
											if (TimeChecker == 0)
											{
												break;
											}
										}
										else if ((Convert.ToInt32(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's'))) > 30)
										{
											FlatRateExistence = true;
										}

									}
									if (checkAllIfFailed >= CarParkRateList.Count)
									{
										NonExistenceCarparkRate = true;

										break;
									}
									/*	else if(FlatRateExistence==true&& TimeChecker!=0)
									{

									}*/
								}
							}
							else
							{
								// motorcycle
								double TimeChecker = EachHoursPerDay.getDayDuration();
								while (TimeChecker != 0)
								{
									int checkAllIfFailed = 0;
									for (int i = 0; i < CarParkRateList.Count; i++)
									{
										double durationOfStaticTimeInMin = Convert.ToDouble(CarParkRateList[i].Duration) * 60;
										if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
										{
											RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

											RateEndTimeFromDB = RateStartTimeFromDB.AddMinutes(durationOfStaticTimeInMin);
										}
										else if (EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
										{

											RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
											RateStartTimeFromDB = RateEndTimeFromDB.Subtract(new TimeSpan(0, (int)durationOfStaticTimeInMin, 0));
										}
										else
										{
											RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
											RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
										}

										double durationOfDynamicTimeInMin = result.getPeriodDuration(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay());

										if (RateStartTimeFromDB.TimeOfDay == EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay &&
													durationOfDynamicTimeInMin <= durationOfStaticTimeInMin && TimeChecker > 0)
										{
											result.setDuration((int)EachHoursPerDay.getDayDuration());
											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
											TimeChecker -= durationOfDynamicTimeInMin;
											checkAllIfFailed--;
										}

										else if (result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{


											result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
											TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											checkAllIfFailed--;


										}
										else if (result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{
											double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
											result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));

											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
											TimeChecker -= ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - ((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue)));

											checkAllIfFailed--;
										}

										else
										{
											checkAllIfFailed++;


										}
									}
									if (checkAllIfFailed >= CarParkRateList.Count)
									{
										NonExistenceCarparkRate = true;

										break;
									}
								}
							}
						}

					}


				}
				else
				{
					IsNUll = true;
					Price = 0;
				}
			}
			else if (0 < duration)
			{
				InvalidDate = true;
			}
			//Price = Math.Round(Price, 2, MidpointRounding.ToEven);


			return Price;




        }

        public double GetDistance(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude)
        {
            var fromCoord = new GeoCoordinate(fromLatitude, fromLongitude);
            var toCoord = new GeoCoordinate(toLatitude, toLongitude);

            

            var distance = fromCoord.GetDistanceTo(toCoord);

            return distance;
        }

        public PagedList<Carpark> GetCarparks(CarparkResourceParameters carparkResourceParameters)
        {
            var error = "";
            var collectionBeforePaging = _context.Carparks.AsQueryable();

            // filter agency type
            if (!string.IsNullOrEmpty(carparkResourceParameters.AgencyType))
            {
                var agencyTypeForWhereClause = carparkResourceParameters.AgencyType.Trim();

                if (agencyTypeForWhereClause == "HDB" || agencyTypeForWhereClause == "LTA" || agencyTypeForWhereClause == "URA")
                {
                    collectionBeforePaging = collectionBeforePaging.Where(a => a.AgencyType == agencyTypeForWhereClause);
                }
                else
                {
                    error += $"Cannot find the specified agency type {carparkResourceParameters.AgencyType}. ";
                }
            }

            // filter vehicle type
            if (!string.IsNullOrEmpty(carparkResourceParameters.VehType))
            {
                if (carparkResourceParameters.VehType == "Car")
                {
                    collectionBeforePaging = collectionBeforePaging.Where(a => a.LotType.Contains('C'));
                }
                else if (carparkResourceParameters.VehType == "Motorcycle")
                {
                    collectionBeforePaging = collectionBeforePaging.Where(a => a.LotType.Contains('M'));
                }
                else if (carparkResourceParameters.VehType == "Heavy Vehicle")
                {
                    collectionBeforePaging = collectionBeforePaging.Where(a => a.LotType.Contains('H'));
                }
                else if (carparkResourceParameters.VehType == "All")
                {
                    // do nothing.
                }
                else
                {
                    error += $"Cannot find the specified vehicle type {carparkResourceParameters.VehType}. ";
                }
            }
            
            if (carparkResourceParameters.Latitude != Double.MinValue || carparkResourceParameters.Longitude != Double.MinValue)
            {
                collectionBeforePaging = collectionBeforePaging.Where(carpark => GetDistance(carparkResourceParameters.Latitude, carparkResourceParameters.Longitude, Double.Parse(carpark.XCoord), Double.Parse(carpark.YCoord)) <= carparkResourceParameters.Range);
            }

            // filtering of price
            // if start date time and end date time are specified.
            //if (carparkResourceParameters.StartDateTime != DateTime.MinValue || carparkResourceParameters.EndDateTime != DateTime.MinValue)
            //{
            //    var carparks = collectionBeforePaging.ToList();


            //    carparks.ForEach(a =>
            //    {
            //        if (carparkResourceParameters.VehType == "All")
            //        {
            //            double carPrice = 0.0, hvPrice = 0.0, mPrice = 0.0;

            //            var asplit = a.LotType.Split(",");

            //            foreach (var x in asplit)
            //            {
            //                if (x.Contains("C"))
            //                {
            //                    carPrice = CalculateCarparkPrice(a.Id, FormatDateTime(carparkResourceParameters.StartDateTime), FormatDateTime(carparkResourceParameters.EndDateTime), "Car");
            //                }
            //                else if (x.Contains("M"))
            //                {
            //                    mPrice = CalculateCarparkPrice(a.Id, FormatDateTime(carparkResourceParameters.StartDateTime), FormatDateTime(carparkResourceParameters.EndDateTime), "Motorcycle");
            //                }
            //                else if (x.Contains("H"))
            //                {
            //                    hvPrice = CalculateCarparkPrice(a.Id, FormatDateTime(carparkResourceParameters.StartDateTime), FormatDateTime(carparkResourceParameters.EndDateTime), "Heavy Vehicle");
            //                }
            //            }

            //            if (asplit.Length == 1)
            //            {
            //                if (carPrice != 0.0)
            //                {
            //                    a.Price = carPrice;
            //                }
            //                else if (mPrice != 0.0)
            //                {
            //                    a.Price = mPrice;
            //                }
            //                else if (hvPrice != 0.0)
            //                {
            //                    a.Price = hvPrice;
            //                }
            //            }
            //            else if (asplit.Length == 2)
            //            {
            //                if (carPrice != 0.0 && mPrice != 0.0)
            //                {
            //                    a.Price = Math.Min(carPrice, mPrice);
            //                }
            //                else if (carPrice != 0.0 && hvPrice != 0.0)
            //                {
            //                    a.Price = Math.Min(carPrice, hvPrice);
            //                }
            //                else if (mPrice != 0.0 && hvPrice != 0.0)
            //                {
            //                    a.Price = Math.Min(mPrice, hvPrice);
            //                }
            //            }
            //            else if (asplit.Length == 3)
            //            {
            //                a.Price = Math.Min(Math.Min(carPrice, mPrice), hvPrice);
            //            }

            //        }
            //        else
            //        {
            //            var price = CalculateCarparkPrice(a.Id, FormatDateTime(carparkResourceParameters.StartDateTime), FormatDateTime(carparkResourceParameters.EndDateTime), carparkResourceParameters.VehType);

            //            a.Price = price;
            //        }
            //    });

            //    // price is defaulted to max double, but if specified then it will set to lower or equal to that price.
            //    collectionBeforePaging = collectionBeforePaging.Where(a => a.Price <= carparkResourceParameters.Price).OrderBy(a => a.Price);

            //}
            //else
            //// if datetime not specified.
            //{
            //    var carparks = collectionBeforePaging.ToList();

            //    carparks.ForEach(a =>
            //    {
            //        if (carparkResourceParameters.VehType == "All")
            //        {
            //            double carPrice = 0.0, hvPrice = 0.0, mPrice = 0.0;

            //            var asplit = a.LotType.Split(",");

            //            foreach (var x in asplit)
            //            {
            //                if (x.Contains("C"))
            //                {
            //                    carPrice = CalculateCarparkPrice(a.Id, FormatDateTime(DateTime.Now), FormatDateTime(DateTime.Now.AddHours(1)), "Car");
            //                }
            //                else if (x.Contains("M"))
            //                {
            //                    mPrice = CalculateCarparkPrice(a.Id, FormatDateTime(DateTime.Now), FormatDateTime(DateTime.Now.AddHours(1)), "Motorcycle");
            //                }
            //                else if (x.Contains("H"))
            //                {
            //                    hvPrice = CalculateCarparkPrice(a.Id, FormatDateTime(DateTime.Now), FormatDateTime(DateTime.Now.AddHours(1)), "Heavy Vehicle");
            //                }
            //            }

            //            if (asplit.Length == 1)
            //            {
            //                if (carPrice != 0.0)
            //                {
            //                    a.Price = carPrice;
            //                }
            //                else if (mPrice != 0.0)
            //                {
            //                    a.Price = mPrice;
            //                }
            //                else if (hvPrice != 0.0)
            //                {
            //                    a.Price = hvPrice;
            //                }
            //            }
            //            else if (asplit.Length == 2)
            //            {
            //                if (carPrice != 0.0 && mPrice != 0.0)
            //                {
            //                    a.Price = Math.Min(carPrice, mPrice);
            //                }
            //                else if (carPrice != 0.0 && hvPrice != 0.0)
            //                {
            //                    a.Price = Math.Min(carPrice, hvPrice);
            //                }
            //                else if (mPrice != 0.0 && hvPrice != 0.0)
            //                {
            //                    a.Price = Math.Min(mPrice, hvPrice);
            //                }
            //            }
            //            else if (asplit.Length == 3)
            //            {
            //                a.Price = Math.Min(Math.Min(carPrice, mPrice), hvPrice);
            //            }

            //        }
            //        else
            //        {
            //            // set start datetime to now and end datetime to one hour after now
            //            var price = CalculateCarparkPrice(a.Id, FormatDateTime(DateTime.Now), FormatDateTime(DateTime.Now.AddHours(1)), carparkResourceParameters.VehType);

            //            a.Price = price;
            //        }
            //    });

            //    // price is defaulted to max double, but if specified then it will set to lower or equal to that price.
            //    collectionBeforePaging = collectionBeforePaging.Where(a => a.Price <= carparkResourceParameters.Price).OrderBy(a => a.Price);
            //}

            // Electronic or Coupon Parking
            if (carparkResourceParameters.IsElectronic == true)
            {
                collectionBeforePaging = collectionBeforePaging.Where(a => a.ParkingSystem == "ELECTRONIC PARKING");
            }
            else
            {
                collectionBeforePaging = collectionBeforePaging.Where(a => a.ParkingSystem == "COUPON PARKING");
            }

            // Is Central or Not
            if (carparkResourceParameters.IsCentral)
            {
                collectionBeforePaging = collectionBeforePaging.Where(a => a.IsCentral);
            }


            // Search Query
            if (!string.IsNullOrEmpty(carparkResourceParameters.SearchQuery))
            {
                var searchQueryForWhereClause = carparkResourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging.Where(a => a.CarparkName.ToLowerInvariant().Contains(searchQueryForWhereClause));
            }

            // Sorting By CarparkName
            collectionBeforePaging = collectionBeforePaging.ApplySort(carparkResourceParameters.OrderBy,
                                _propertyMappingService.GetPropertyMapping<CarparkDto, Carpark>());

            

            if (error.Any())
            {
                throw new AppException(error);
            }

            return PagedList<Carpark>.Create(collectionBeforePaging, carparkResourceParameters.PageNumber, carparkResourceParameters.PageSize);
        }

        public DateTime FormatDateTime(DateTime time)
        {
            var datetime = DateTime.ParseExact($"{time.Month}/{time.Day}/{time.Year} {time.Hour}:{time.Minute}:0", "M/d/yyyy H:m:s", CultureInfo.InvariantCulture);

            return datetime;
        }

        public Carpark GetCarpark(Guid carparkId)
        {
            return _context.Carparks.FirstOrDefault(a => a.Id == carparkId);
        }

        public Carpark GetCarparkById(string carparkId)
        {
            return _context.Carparks.FirstOrDefault(a => a.CarparkId == carparkId);
        }

        public void AddCarpark(Carpark carpark)
        {
            carpark.Id = Guid.NewGuid();
            _context.Carparks.Add(carpark);
        }

        public void UpdateCarpark(Carpark carpark)
        {
            _context.Carparks.Update(carpark);
        }

        public void UpdateCarparks(List<Carpark> carparks)
        {
            _context.Carparks.AddRange(carparks);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void DeleteCarpark(Carpark carpark)
        {
            _context.Carparks.Remove(carpark);
        }
    }
}
