using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkingSlotAPI.Helpers;
using ParkingSlotAPI.Models;
using ParkingSlotAPI.Repository;
using ParkingSlotAPI.Services;
using ParkingSlotAPI.Entities;
using System.Collections;
using System.Globalization;

namespace ParkingSlotAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CalculationController : ControllerBase
	{
		private readonly ICarparkRatesRepository ICarparkRateRepository;
		private readonly IMapper _mapper;

		public CalculationController(ICarparkRatesRepository calRepository, IMapper mapper)
		{
			ICarparkRateRepository = calRepository;
			_mapper = mapper;
		}

        [HttpGet("calculate/{id}")]
        public IActionResult CalculateCarparkPrice(Guid id, [FromQuery] DateTime StartTime, [FromQuery] DateTime EndTime, [FromQuery] String vehicleType)
        {
            double price = 0.0;
            // get the carpark rates from the specified carpark
            var carparkRatesRecords = ICarparkRateRepository.GetCarparkRateById(id, vehicleType).ToList();

            // start time component
            var startTimeDayComponent = StartTime.DayOfWeek;
            var startTimeHourComponent = StartTime.Hour;
            var startTimeMinuteComponent = StartTime.Minute;


            // end time component
            var endTimeDayComponent = EndTime.DayOfWeek;
            var endTimeHourComponent = EndTime.Hour;
            var endTimeMinuteComponent = EndTime.Minute;

            // get duration
            var duration = EndTime - StartTime;


            // if the start time and end time are in the same day
            if (StartTime.DayOfWeek == EndTime.DayOfWeek)
            {
                var flatRateCarparkRatesRecords = carparkRatesRecords.Where(a => Double.Parse(a.WeekdayMin.Substring(0, a.WeekdayMin.Length - 5)) > 30).ToList();

                var startTimeWithinCarparkRatesRecords = carparkRatesRecords.Where(a => StartTime.TimeOfDay >= DateTime.Parse(a.StartTime).TimeOfDay).ToList();

                var endTimeWithinCarparkRatesRecords = carparkRatesRecords.Where(a => EndTime.TimeOfDay <= DateTime.Parse(a.EndTime).TimeOfDay).ToList();

                var startTimeOutsideCarparkRatesRecords = carparkRatesRecords.Where(a => StartTime.TimeOfDay < DateTime.Parse(a.EndTime).TimeOfDay).Except(endTimeWithinCarparkRatesRecords).Except(flatRateCarparkRatesRecords).ToList();

                var endTimeOutsideCarparkRatesRecords = carparkRatesRecords.Where(a => EndTime.TimeOfDay > DateTime.Parse(a.StartTime).TimeOfDay).ToList().Except(startTimeWithinCarparkRatesRecords).Except(flatRateCarparkRatesRecords).ToList();

                if (endTimeOutsideCarparkRatesRecords.Count() == endTimeWithinCarparkRatesRecords.Count())
                {
                    endTimeOutsideCarparkRatesRecords.Clear();
                }

                if (startTimeOutsideCarparkRatesRecords.Count() == startTimeWithinCarparkRatesRecords.Count())
                {
                    startTimeOutsideCarparkRatesRecords.Clear();
                }

                if (startTimeOutsideCarparkRatesRecords.Count() == endTimeOutsideCarparkRatesRecords.Count())
                {
                    startTimeOutsideCarparkRatesRecords.RemoveAt(startTimeOutsideCarparkRatesRecords.Count - 1);
                    endTimeOutsideCarparkRatesRecords.RemoveAt(0);
                }

                // if only one record is shown, this means time is within the database timeframe.
                if (startTimeWithinCarparkRatesRecords.Count() == 1 && endTimeWithinCarparkRatesRecords.Count() == 1 && startTimeOutsideCarparkRatesRecords.Count() == 0 && endTimeOutsideCarparkRatesRecords.Count() == 0)
                {
                    if (startTimeDayComponent == DayOfWeek.Saturday)
                    {
                        price = duration.TotalMinutes / Double.Parse(startTimeWithinCarparkRatesRecords[0].SatdayMin.Substring(0, startTimeWithinCarparkRatesRecords[0].SatdayMin.Length - 5)) * Double.Parse(startTimeWithinCarparkRatesRecords[0].SatdayRate.Remove(0, 1));
                    }
                    else if (startTimeDayComponent == DayOfWeek.Sunday)
                    {
                        price = duration.TotalMinutes / Double.Parse(startTimeWithinCarparkRatesRecords[0].SunPHMin.Substring(0, startTimeWithinCarparkRatesRecords[0].SunPHMin.Length - 5)) * Double.Parse(startTimeWithinCarparkRatesRecords[0].SunPHRate.Remove(0, 1));
                    }
                    else
                    {
                        price = duration.TotalMinutes / Double.Parse(startTimeWithinCarparkRatesRecords[0].WeekdayMin.Substring(0, startTimeWithinCarparkRatesRecords[0].WeekdayMin.Length - 5)) * Double.Parse(startTimeWithinCarparkRatesRecords[0].WeekdayRate.Remove(0, 1));
                    }
                }

                if (startTimeOutsideCarparkRatesRecords.Count() == 1 && endTimeWithinCarparkRatesRecords.Count() == 1 && startTimeWithinCarparkRatesRecords.Count() == 0 && endTimeOutsideCarparkRatesRecords.Count() == 0)
                {
                    var fromStartTimeToDatabaseEndTimeDuration = (DateTime.Parse(startTimeOutsideCarparkRatesRecords[0].EndTime).TimeOfDay - StartTime.TimeOfDay).TotalMinutes;

                    var fromDatabaseStartTimetoEndTimeDuration = (EndTime.TimeOfDay - DateTime.Parse(endTimeWithinCarparkRatesRecords[0].StartTime).TimeOfDay).TotalMinutes;

                    if (startTimeDayComponent == DayOfWeek.Saturday)
                    {
                        price += fromStartTimeToDatabaseEndTimeDuration / Double.Parse(startTimeOutsideCarparkRatesRecords[0].SatdayMin.Substring(0, startTimeOutsideCarparkRatesRecords[0].SatdayMin.Length - 5)) * Double.Parse(startTimeOutsideCarparkRatesRecords[0].SatdayRate.Remove(0, 1));
                        price += fromDatabaseStartTimetoEndTimeDuration / Double.Parse(endTimeWithinCarparkRatesRecords[0].SatdayMin.Substring(0, endTimeWithinCarparkRatesRecords[0].SatdayMin.Length - 5)) * Double.Parse(endTimeWithinCarparkRatesRecords[0].SatdayRate.Remove(0, 1));
                    }
                    else if (startTimeDayComponent == DayOfWeek.Sunday)
                    {
                        price += fromStartTimeToDatabaseEndTimeDuration / Double.Parse(startTimeOutsideCarparkRatesRecords[0].SunPHMin.Substring(0, startTimeOutsideCarparkRatesRecords[0].SunPHMin.Length - 5)) * Double.Parse(startTimeOutsideCarparkRatesRecords[0].SunPHRate.Remove(0, 1));
                        price += fromDatabaseStartTimetoEndTimeDuration / Double.Parse(endTimeWithinCarparkRatesRecords[0].SunPHMin.Substring(0, endTimeWithinCarparkRatesRecords[0].SunPHMin.Length - 5)) * Double.Parse(endTimeWithinCarparkRatesRecords[0].SunPHRate.Remove(0, 1));
                    }
                    else
                    {
                        price += fromStartTimeToDatabaseEndTimeDuration / Double.Parse(startTimeOutsideCarparkRatesRecords[0].WeekdayMin.Substring(0, startTimeOutsideCarparkRatesRecords[0].WeekdayMin.Length - 5)) * Double.Parse(startTimeOutsideCarparkRatesRecords[0].WeekdayRate.Remove(0, 1));
                        price += fromDatabaseStartTimetoEndTimeDuration / Double.Parse(endTimeWithinCarparkRatesRecords[0].WeekdayMin.Substring(0, endTimeWithinCarparkRatesRecords[0].WeekdayMin.Length - 5)) * Double.Parse(endTimeWithinCarparkRatesRecords[0].WeekdayRate.Remove(0, 1));
                    }
                }

                if (startTimeWithinCarparkRatesRecords.Count() == 1 && endTimeOutsideCarparkRatesRecords.Count() == 1 && startTimeOutsideCarparkRatesRecords.Count() == 0 && endTimeWithinCarparkRatesRecords.Count() == 0)
                {
                    var fromStartTimeToDatabaseEndTimeDuration = (DateTime.Parse(startTimeWithinCarparkRatesRecords[0].EndTime).TimeOfDay - StartTime.TimeOfDay).TotalMinutes;

                    var fromDatabaseStartTimeToEndTimeDuration = (EndTime.TimeOfDay - DateTime.Parse(endTimeOutsideCarparkRatesRecords[0].StartTime).TimeOfDay).TotalMinutes;

                    if (startTimeDayComponent == DayOfWeek.Saturday)
                    {
                        price += fromStartTimeToDatabaseEndTimeDuration / Double.Parse(startTimeWithinCarparkRatesRecords[0].SatdayMin.Substring(0, startTimeWithinCarparkRatesRecords[0].SatdayMin.Length - 5)) * Double.Parse(startTimeWithinCarparkRatesRecords[0].SatdayRate.Remove(0, 1));
                        price += fromDatabaseStartTimeToEndTimeDuration / Double.Parse(endTimeOutsideCarparkRatesRecords[0].SatdayMin.Substring(0, endTimeOutsideCarparkRatesRecords[0].SatdayMin.Length - 5)) * Double.Parse(endTimeOutsideCarparkRatesRecords[0].SatdayRate.Remove(0, 1));

                    }
                    else if (startTimeDayComponent == DayOfWeek.Sunday)
                    {
                        price += fromStartTimeToDatabaseEndTimeDuration / Double.Parse(startTimeWithinCarparkRatesRecords[0].SunPHMin.Substring(0, startTimeWithinCarparkRatesRecords[0].SunPHMin.Length - 5)) * Double.Parse(startTimeWithinCarparkRatesRecords[0].SunPHRate.Remove(0, 1));
                        price += fromDatabaseStartTimeToEndTimeDuration / Double.Parse(endTimeOutsideCarparkRatesRecords[0].SunPHMin.Substring(0, endTimeOutsideCarparkRatesRecords[0].SunPHMin.Length - 5)) * Double.Parse(endTimeOutsideCarparkRatesRecords[0].SunPHRate.Remove(0, 1));

                    }
                    else
                    {
                        price += fromStartTimeToDatabaseEndTimeDuration / Double.Parse(startTimeWithinCarparkRatesRecords[0].WeekdayMin.Substring(0, startTimeWithinCarparkRatesRecords[0].WeekdayMin.Length - 5)) * Double.Parse(startTimeWithinCarparkRatesRecords[0].WeekdayRate.Remove(0, 1));
                        price += fromDatabaseStartTimeToEndTimeDuration / Double.Parse(endTimeOutsideCarparkRatesRecords[0].WeekdayMin.Substring(0, endTimeOutsideCarparkRatesRecords[0].WeekdayMin.Length - 5)) * Double.Parse(endTimeOutsideCarparkRatesRecords[0].WeekdayRate.Remove(0, 1));

                    }
                }

                if (startTimeOutsideCarparkRatesRecords.Count() == 1 && endTimeOutsideCarparkRatesRecords.Count() == 1 && startTimeWithinCarparkRatesRecords.Count() == 0 && endTimeWithinCarparkRatesRecords.Count() == 0)
                {
                    var fromStartTimeToDatabaseEndTime = (DateTime.Parse(startTimeOutsideCarparkRatesRecords[0].EndTime).TimeOfDay - StartTime.TimeOfDay).TotalMinutes;
                    var fromDatabaseStartTimeToDatabaseEndTime = (DateTime.Parse(endTimeOutsideCarparkRatesRecords[0].EndTime) - DateTime.Parse(endTimeOutsideCarparkRatesRecords[0].StartTime)).TotalMinutes;
                    var fromDatabaseStartTimeToEndTime = (EndTime.TimeOfDay - DateTime.Parse(startTimeOutsideCarparkRatesRecords[0].StartTime).TimeOfDay).TotalMinutes;

                    if (startTimeDayComponent == DayOfWeek.Saturday)
                    {
                        price += fromStartTimeToDatabaseEndTime / Double.Parse(startTimeOutsideCarparkRatesRecords[0].SatdayMin.Substring(0, startTimeOutsideCarparkRatesRecords[0].SatdayMin.Length - 5)) * Double.Parse(startTimeOutsideCarparkRatesRecords[0].SatdayRate.Remove(0, 1));
                        price += fromDatabaseStartTimeToDatabaseEndTime / Double.Parse(endTimeOutsideCarparkRatesRecords[0].SatdayMin.Substring(0, endTimeOutsideCarparkRatesRecords[0].SatdayMin.Length - 5)) * Double.Parse(endTimeOutsideCarparkRatesRecords[0].SatdayRate.Remove(0, 1));
                        price += fromDatabaseStartTimeToEndTime / Double.Parse(startTimeOutsideCarparkRatesRecords[0].SatdayMin.Substring(0, startTimeOutsideCarparkRatesRecords[0].SatdayMin.Length - 5)) * Double.Parse(startTimeOutsideCarparkRatesRecords[0].SatdayRate.Remove(0, 1));
                    }
                    else if (startTimeDayComponent == DayOfWeek.Sunday)
                    {
                        price += fromStartTimeToDatabaseEndTime / Double.Parse(startTimeOutsideCarparkRatesRecords[0].SunPHMin.Substring(0, startTimeOutsideCarparkRatesRecords[0].SunPHMin.Length - 5)) * Double.Parse(startTimeOutsideCarparkRatesRecords[0].SunPHRate.Remove(0, 1));
                        price += fromDatabaseStartTimeToDatabaseEndTime / Double.Parse(endTimeOutsideCarparkRatesRecords[0].SunPHMin.Substring(0, endTimeOutsideCarparkRatesRecords[0].SunPHMin.Length - 5)) * Double.Parse(endTimeOutsideCarparkRatesRecords[0].SunPHRate.Remove(0, 1));
                        price += fromDatabaseStartTimeToEndTime / Double.Parse(startTimeOutsideCarparkRatesRecords[0].SunPHMin.Substring(0, startTimeOutsideCarparkRatesRecords[0].SunPHMin.Length - 5)) * Double.Parse(startTimeOutsideCarparkRatesRecords[0].SunPHRate.Remove(0, 1));
                    }
                    else
                    {
                        price += fromStartTimeToDatabaseEndTime / Double.Parse(startTimeOutsideCarparkRatesRecords[0].WeekdayMin.Substring(0, startTimeOutsideCarparkRatesRecords[0].WeekdayMin.Length - 5)) * Double.Parse(startTimeOutsideCarparkRatesRecords[0].WeekdayRate.Remove(0, 1));
                        price += fromDatabaseStartTimeToDatabaseEndTime / Double.Parse(endTimeOutsideCarparkRatesRecords[0].WeekdayMin.Substring(0, endTimeOutsideCarparkRatesRecords[0].WeekdayMin.Length - 5)) * Double.Parse(endTimeOutsideCarparkRatesRecords[0].WeekdayRate.Remove(0, 1));
                        price += fromDatabaseStartTimeToEndTime / Double.Parse(startTimeOutsideCarparkRatesRecords[0].WeekdayMin.Substring(0, startTimeOutsideCarparkRatesRecords[0].WeekdayMin.Length - 5)) * Double.Parse(startTimeOutsideCarparkRatesRecords[0].WeekdayRate.Remove(0, 1));
                    }
                }
            }
            else
            {

            }


            return Ok(new { Price = price });
        }

		[HttpGet("{id}")]
		public ActionResult CalculateCarparkPriceOverLongPeriod(Guid id, [FromQuery] DateTime StartTime, [FromQuery] DateTime EndTime, [FromQuery] String vehicleType)
		{

			var duration = 0.0;
			var Totalduration = 0.0;
			double carryOverValue = 0;
			double Price = 0;
			Boolean IsNUll = false, NonExistenceCarparkRate = false, InvalidDate = false, FlatRateExistence = false;
			DateTime RateStartTimeFromDB = new DateTime(1, 1, 1); ;
			DateTime RateEndTimeFromDB = new DateTime(1, 1, 1);
			var carpark = ICarparkRateRepository.GetCarparkRateById(id, vehicleType);
			List<CarparkRate> CarParkRateList = carpark.ToList();


			if (EndTime.ToString() == "1/1/0001 12:00:00 AM" && StartTime.ToString() == "1/1/0001 12:00:00 AM")
			{
				Totalduration = duration = 60;

			}
			else if (EndTime.ToString() != "1/1/0001 12:00:00 AM" && StartTime.ToString() == "1/1/0001 12:00:00 AM")
			{
				StartTime = DateTime.Now;
				StartTime = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day, StartTime.Hour, StartTime.Minute, 0);

				Totalduration = duration = (EndTime - StartTime).TotalMinutes;
			}
			else
			{
				Totalduration = duration = (EndTime - StartTime).TotalMinutes;
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
											if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0 && EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
											{
												RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day - 1 + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
												RateStartTimeFromDB = RateEndTimeFromDB.Subtract(new TimeSpan(0, (int)durationOfStaticTimeInMin, 0));
											}
											else if (CarParkRateList[i].StartTime == "00:00:00" && CarParkRateList[i].EndTime == "23:59:59")
											{
												RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

												RateEndTimeFromDB = RateStartTimeFromDB.AddMinutes(durationOfStaticTimeInMin);
											}
											else if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
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
												if (result.getDuration() % 30 == 0)
												{
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
												}


												else if (result.getDuration() % 30 != 0)
												{
													if (EachHoursPerDay.getStartTimeOfTheDay().Date == StartTime.Date)
													{
														carryOverValue += (30 - (result.getDuration() % 30));
													}
													else
													{
														carryOverValue += (result.getDuration() % 30);
													}

													result.setDuration((int)(result.getDuration() - (result.getDuration() % 30)));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
													if (carryOverValue == Totalduration - result.getDuration() && carryOverValue % 30 == 0)
													{
														result.setDuration((int)(carryOverValue));
														Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
														carryOverValue = 0;

													}
												}
												else if (TimeChecker == Totalduration)
												{
													result.setDuration((int)(result.getDuration() + carryOverValue));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
													carryOverValue = 0;

												}
												TimeChecker -= durationOfDynamicTimeInMin;
												checkAllIfFailed--;
												Totalduration -= result.getDuration();
											}

											else if (TimeChecker > 0 && result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{



												result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
												Totalduration -= result.getDuration();
												checkAllIfFailed--;

											}
											else if (TimeChecker > 0 && result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{
												double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
												result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												if (result.getDuration() % 30 == 0)
												{
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
												}
												else if (result.getDuration() % 30 != 0)
												{
													if (EachHoursPerDay.getStartTimeOfTheDay().Date == StartTime.Date)
													{
														carryOverValue += (30 - (result.getDuration() % 30));
													}
													else
													{
														carryOverValue += (result.getDuration() % 30);
													}
													result.setDuration((int)(result.getDuration() - (result.getDuration() % 30)));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
													/*	if (carryOverValue == Totalduration - result.getDuration() && carryOverValue % 30 == 0)
														{
															result.setDuration((int)(carryOverValue));
															Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
															carryOverValue = 0;

														}*/
												}
												else if (TimeChecker == Totalduration)
												{
													result.setDuration((int)(result.getDuration() + carryOverValue));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
													carryOverValue = 0;

												}
												int reminder = ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												TimeChecker -= reminder;
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - reminder);
												EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(reminder));
												Totalduration -= result.getDuration();
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


									}
									if (checkAllIfFailed >= CarParkRateList.Count)
									{
										NonExistenceCarparkRate = true;

										break;
									}

								}
								if (FlatRateExistence != true)
								{
									int totalDay = (int)(EndTime.Date - StartTime.Date).TotalDays;
									DateTime st = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day, 22, 30, 0);
									DateTime et = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day + 1, 7, 0, 0);
									for (int p = 0; p < totalDay; p++)
									{

										for (int i = 0; i < CarParkRateList.Count; i++)
										{
											if (result.TimePeriodOverlaps(st, et, StartTime, EndTime) == true && Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')) > 30)
											{
												double tmp = (Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')));
												if (tmp == 5.6)
												{
													Price -= (11.9) - (Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')));
												}
												else if (tmp == 5)
												{
													Price -= (10.2) - (Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')));
												}
												st = st.AddDays(1);
												et = et.AddDays(1);
											}

										}
									}
									FlatRateExistence = true;
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

											TimeChecker -= durationOfDynamicTimeInMin;
											checkAllIfFailed--;
											Totalduration -= result.getDuration();
											if (RateStartTimeFromDB.TimeOfDay.TotalMinutes != 1350 && RateEndTimeFromDB.TimeOfDay.TotalMinutes != 420 || TimeChecker == Totalduration)
											{
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
											}


										}

										else if (result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{


											result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);

											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));

											TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											checkAllIfFailed--;
											Totalduration -= result.getDuration();

										}
										else if (result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{
											double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
											result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											if (RateStartTimeFromDB.TimeOfDay.TotalMinutes != 1350 && RateEndTimeFromDB.TimeOfDay.TotalMinutes != 420 || TimeChecker == Totalduration)
											{
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')));
											}


											TimeChecker -= ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - ((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue)));
											Totalduration -= result.getDuration();
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
										if ((Convert.ToInt32(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's'))) <= 30)
										{
											double durationOfStaticTimeInMin = Convert.ToDouble(CarParkRateList[i].Duration) * 60;
											if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0 && EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
											{
												RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day - 1 + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
												RateStartTimeFromDB = RateEndTimeFromDB.Subtract(new TimeSpan(0, (int)durationOfStaticTimeInMin, 0));
											}
											else if (CarParkRateList[i].StartTime == "00:00:00" && CarParkRateList[i].EndTime == "23:59:59")
											{
												RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

												RateEndTimeFromDB = RateStartTimeFromDB.AddMinutes(durationOfStaticTimeInMin);
											}
											else if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
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
												if (result.getDuration() % 30 == 0)
												{
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
												}

												else if (TimeChecker == Totalduration)
												{
													result.setDuration((int)(result.getDuration() + carryOverValue));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
													carryOverValue = 0;

												}
												else if (result.getDuration() % 30 != 0)
												{
													if (EachHoursPerDay.getStartTimeOfTheDay().Date == StartTime.Date)
													{
														carryOverValue += (30 - (result.getDuration() % 30));
													}
													else
													{
														carryOverValue += (result.getDuration() % 30);
													}

													result.setDuration((int)(result.getDuration() - (result.getDuration() % 30)));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
													if (carryOverValue == Totalduration - result.getDuration() && carryOverValue % 30 == 0)
													{
														result.setDuration((int)(carryOverValue));
														Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
														carryOverValue = 0;

													}
												}
												TimeChecker -= durationOfDynamicTimeInMin;
												checkAllIfFailed--;
												Totalduration -= result.getDuration();
											}

											else if (TimeChecker > 0 && result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{


												result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
												Totalduration -= result.getDuration();
												checkAllIfFailed--;

											}
											else if (TimeChecker > 0 && result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{
												double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
												result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												if (result.getDuration() % 30 == 0)
												{
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
												}
												else if (result.getDuration() % 30 != 0)
												{
													if (EachHoursPerDay.getStartTimeOfTheDay().Date == StartTime.Date)
													{
														carryOverValue += (30 - (result.getDuration() % 30));
													}
													else
													{
														carryOverValue += (result.getDuration() % 30);
													}
													result.setDuration((int)(result.getDuration() - (result.getDuration() % 30)));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
													if (carryOverValue == Totalduration - result.getDuration() && carryOverValue % 30 == 0)
													{
														result.setDuration((int)(carryOverValue));
														Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
														carryOverValue = 0;

													}
												}
												else if (TimeChecker == Totalduration)
												{
													result.setDuration((int)(result.getDuration() + carryOverValue));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
													carryOverValue = 0;

												}
												int reminder = ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												TimeChecker -= reminder;
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - reminder);
												EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(reminder));
												Totalduration -= result.getDuration();
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


									}
									if (checkAllIfFailed >= CarParkRateList.Count)
									{
										NonExistenceCarparkRate = true;

										break;
									}

								}
								if (FlatRateExistence != true)
								{
									int totalDay = (int)(EndTime.Date - StartTime.Date).TotalDays;
									DateTime st = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day, 22, 30, 0);
									DateTime et = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day + 1, 7, 0, 0);
									for (int p = 0; p < totalDay; p++)
									{

										for (int i = 0; i < CarParkRateList.Count; i++)
										{
											if (result.TimePeriodOverlaps(st, et, StartTime, EndTime) == true && Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')) > 30)
											{
												double tmp = (Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')));
												if (tmp == 5.6)
												{
													Price -= (11.9) - (Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')));
												}
												else if (tmp == 5)
												{
													Price -= (10.2) - (Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')));
												}
												st = st.AddDays(1);
												et = et.AddDays(1);
											}

										}
									}
									FlatRateExistence = true;
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
											if (RateStartTimeFromDB.TimeOfDay.TotalMinutes != 1350 && RateEndTimeFromDB.TimeOfDay.TotalMinutes != 420 || TimeChecker == Totalduration)
											{
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
											}
											TimeChecker -= durationOfDynamicTimeInMin;
											checkAllIfFailed--;
											Totalduration -= result.getDuration();
										}

										else if (result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{


											result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
											TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											checkAllIfFailed--;
											Totalduration -= result.getDuration();


										}
										else if (result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{
											double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
											result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											if (RateStartTimeFromDB.TimeOfDay.TotalMinutes != 1350 && RateEndTimeFromDB.TimeOfDay.TotalMinutes != 420 || TimeChecker == Totalduration)
											{
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SatdayRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SatdayMin.Trim('m', 'i', 'n', 's')));
											}
											TimeChecker -= ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - ((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
											EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue)));
											Totalduration -= result.getDuration();
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
										if ((Convert.ToInt32(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's'))) <= 30)
										{
											double durationOfStaticTimeInMin = Convert.ToDouble(CarParkRateList[i].Duration) * 60;
											if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0 && EachHoursPerDay.getStartTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
											{
												RateEndTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getEndTimeOfTheDay().Day - 1 + "/" + EachHoursPerDay.getEndTimeOfTheDay().Month + "/" + EachHoursPerDay.getEndTimeOfTheDay().Year + " " + CarParkRateList[i].EndTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
												RateStartTimeFromDB = RateEndTimeFromDB.Subtract(new TimeSpan(0, (int)durationOfStaticTimeInMin, 0));
											}
											else if (CarParkRateList[i].StartTime == "00:00:00" && CarParkRateList[i].EndTime == "23:59:59")
											{
												RateStartTimeFromDB = DateTime.ParseExact(EachHoursPerDay.getStartTimeOfTheDay().Day + "/" + EachHoursPerDay.getStartTimeOfTheDay().Month + "/" + EachHoursPerDay.getStartTimeOfTheDay().Year + " " + CarParkRateList[i].StartTime, "d/M/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

												RateEndTimeFromDB = RateStartTimeFromDB.AddMinutes(durationOfStaticTimeInMin);
											}
											else if (EachHoursPerDay.getEndTimeOfTheDay().TimeOfDay.TotalMinutes == 0)
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
												if (result.getDuration() % 30 == 0)
												{
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
												}

												else if (TimeChecker == Totalduration)
												{
													result.setDuration((int)(result.getDuration() + carryOverValue));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
													carryOverValue = 0;

												}
												else if (result.getDuration() % 30 != 0)
												{
													if (EachHoursPerDay.getStartTimeOfTheDay().Date == StartTime.Date)
													{
														carryOverValue += (30 - (result.getDuration() % 30));
													}
													else
													{
														carryOverValue += (result.getDuration() % 30);
													}

													result.setDuration((int)(result.getDuration() - (result.getDuration() % 30)));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
													if (carryOverValue == Totalduration - result.getDuration() && carryOverValue % 30 == 0)
													{
														result.setDuration((int)(carryOverValue));
														Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
														carryOverValue = 0;

													}
												}
												TimeChecker -= durationOfDynamicTimeInMin;
												checkAllIfFailed--;
												Totalduration -= result.getDuration();
											}

											else if (TimeChecker > 0 && result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{


												result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
												TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
												Totalduration -= result.getDuration();
												checkAllIfFailed--;

											}
											else if (TimeChecker > 0 && result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
											{
												double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
												result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												if (result.getDuration() % 30 == 0)
												{
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
												}
												else if (result.getDuration() % 30 != 0)
												{
													if (EachHoursPerDay.getStartTimeOfTheDay().Date == StartTime.Date)
													{
														carryOverValue += (30 - (result.getDuration() % 30));
													}
													else
													{
														carryOverValue += (result.getDuration() % 30);
													}
													result.setDuration((int)(result.getDuration() - (result.getDuration() % 30)));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
													if (carryOverValue == Totalduration - result.getDuration() && carryOverValue % 30 == 0)
													{
														result.setDuration((int)(carryOverValue));
														Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
														carryOverValue = 0;

													}
												}
												else if (TimeChecker == Totalduration)
												{
													result.setDuration((int)(result.getDuration() + carryOverValue));
													Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
													carryOverValue = 0;

												}
												int reminder = ((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));
												TimeChecker -= reminder;
												EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - reminder);
												EachHoursPerDay.setStartTimeOfTheDay(EachHoursPerDay.getStartTimeOfTheDay().AddMinutes(reminder));
												Totalduration -= result.getDuration();
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


									}
									if (checkAllIfFailed >= CarParkRateList.Count)
									{
										NonExistenceCarparkRate = true;

										break;
									}

								}
								if (FlatRateExistence != true)
								{
									int totalDay = (int)(EndTime.Date - StartTime.Date).TotalDays;
									DateTime st = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day, 22, 30, 0);
									DateTime et = new DateTime(StartTime.Year, StartTime.Month, StartTime.Day + 1, 7, 0, 0);
									for (int p = 0; p < totalDay; p++)
									{

										for (int i = 0; i < CarParkRateList.Count; i++)
										{
											if (result.TimePeriodOverlaps(st, et, StartTime, EndTime) == true && Convert.ToDouble(CarParkRateList[i].WeekdayMin.Trim('m', 'i', 'n', 's')) > 30)
											{
												double tmp = (Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')));
												if (tmp == 5.6)
												{
													Price -= (11.9) - (Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')));
												}
												else if (tmp == 5)
												{
													Price -= (10.2) - (Convert.ToDouble(CarParkRateList[i].WeekdayRate.Trim('$')));
												}
												st = st.AddDays(1);
												et = et.AddDays(1);
											}

										}
									}
									FlatRateExistence = true;
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
											if (RateStartTimeFromDB.TimeOfDay.TotalMinutes != 1350 && RateEndTimeFromDB.TimeOfDay.TotalMinutes != 420 || TimeChecker == Totalduration)
											{
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
											}
											TimeChecker -= durationOfDynamicTimeInMin;
											checkAllIfFailed--;
											Totalduration -= result.getDuration();
										}

										else if (result.TimePeriodOverlaps(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{


											result.setDuration((int)(EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
											TimeChecker -= (int)(((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes));
											EachHoursPerDay.setDayDuration(EachHoursPerDay.getDayDuration() - (EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes);
											checkAllIfFailed--;
											Totalduration -= result.getDuration();


										}
										else if (result.TimePeriodOverlapsRight(EachHoursPerDay.getStartTimeOfTheDay(), EachHoursPerDay.getEndTimeOfTheDay(), RateStartTimeFromDB, RateEndTimeFromDB) == true)
										{
											double redundantValue = (double)(EachHoursPerDay.getEndTimeOfTheDay() - RateEndTimeFromDB).TotalMinutes;
											result.setDuration((int)((EachHoursPerDay.getEndTimeOfTheDay() - EachHoursPerDay.getStartTimeOfTheDay()).TotalMinutes - redundantValue));

											if (RateStartTimeFromDB.TimeOfDay.TotalMinutes != 1350 && RateEndTimeFromDB.TimeOfDay.TotalMinutes != 420 || TimeChecker == Totalduration)
											{
												Price += result.calculatePrice(Convert.ToDouble(CarParkRateList[i].SunPHRate.Trim('$')), Convert.ToDouble(CarParkRateList[i].SunPHMin.Trim('m', 'i', 'n', 's')));
											}
											Totalduration -= result.getDuration();


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
			Price = Math.Round(Price, 2, MidpointRounding.ToEven);



			return Ok(new { id, duration, Price, StartTime, EndTime, vehicleType, IsNUll, NonExistenceCarparkRate, InvalidDate });




		}
	}

	}