using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Services
{
	public class HoursPerDay
	{
		private DateTime StartTimeOfTheDay, EndTimeOfTheDay;
		private double dayDuration;
		int day;

		public HoursPerDay(DateTime startTimeOfTheDay, DateTime endTimeOfTheDay, double dayDuration, int day)
		{
			this.StartTimeOfTheDay = startTimeOfTheDay;
			this.EndTimeOfTheDay = endTimeOfTheDay;
			this.dayDuration = dayDuration;
			this.day = day;
		}
		public DateTime getStartTimeOfTheDay()
		{
			return this.StartTimeOfTheDay;
		}
		public void setStartTimeOfTheDay(DateTime value)
		{
			this.StartTimeOfTheDay = value;
		}
		public DateTime getEndTimeOfTheDay()
		{
			return this.EndTimeOfTheDay;
		}
		public void setEndTimeOfTheDay(DateTime value)
		{
			this.EndTimeOfTheDay = value;
		}
		public double getDayDuration()
		{
			return this.dayDuration;
		}
		public void setDayDuration(double value)
		{
			this.dayDuration = value;
		}
		public int getDay()
		{
			return this.day;
		}
		public void setDay(int value)
		{
			this.day = value;
		}


	}
}
