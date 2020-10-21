using ParkingSlotAPI.Entities;
using ParkingSlotAPI.PublicAPI;
using ParkingSlotAPI.PublicAPIEntities;
using SVY21;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ParkingSlotAPI.Database
{
    public static class ParkingContextExtensions
    {

        public static void EnsureSeedDataForContext(this ParkingContext context)
        {

            FetchPublicAPI publicAPI = new FetchPublicAPI();

            if (!context.Carparks.Any())
            {
                var carparks = new List<Carpark>();

                var task = Task.Run(async () => await publicAPI.GetHDBParkingInfoAsync());

                carparks = task.Result;

                context.Carparks.AddRange(carparks);

                context.SaveChanges();
            }
            else
            {
                // fixHDBCarparkData(context);
                // fixURACarparkData4(context);

                //var carparks = new List<Carpark>();

                //var task = Task.Run(async () => await publicAPI.GetParkingInfoAsync());

                //carparks = task.Result;

                //context.Carparks.AddRange(carparks);

                //context.SaveChanges();

                // ConvertDateTimeFormat(context);

                //var carparks = new List<Carpark>();

                // FixShoppingMallCarparkRates(context);
                // FixHDBCarparkRates(context);

                // FixURACarparkRatesData(context);

                //FixShoppingMallData(context);

                //FixLTAData(context);
                // FixHDBCarparkAvailabilityData(context);

                //fixURACarparkData2(context);
                //fixURACarparkAvailabilityData(context);
                // fixURACarparkData3(context);


                // carparks = fixURACarparkData();
                // context.Carparks.AddRange(carparks);

                // context.SaveChanges();

                //var carparkAvailability = new List<Carpark_Data>();

                //var task = Task.Run(async () => await publicAPI.GetHDBAvailabilityAsync());

                //carparkAvailability = task.Result;

                //foreach (var value in carparkAvailability)
                //{
                //    var ttlAvailable = 0;
                //    var ttlLots = 0;

                //    if (value.carpark_info.Length > 0)
                //    {
                //        foreach (var d in value.carpark_info)
                //        {
                //            ttlAvailable += int.Parse(d.lots_available);
                //            ttlLots += int.Parse(d.total_lots);
                //        }
                //    }

                //    var v = context.Carparks.FirstOrDefault(a => a.CarparkId.Equals(value.carpark_number));

                //   if (v != null)
                //   {
                //        v.TotalAvailableLots = ttlAvailable;
                //        v.TotalLots = ttlLots;

                //        context.Carparks.Update(v);
                //   }
                //}

                //context.SaveChanges();

            }
        }

        public static List<Carpark> fixURACarparkData()
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();

            var carparks = new List<Carpark>();

            // URA Carpark Info
            var task = Task.Run(async () => await publicAPI.GetURAParkingInfoAsync());

            var URACarparkInfo = task.Result;

            var carparkNo = "";

            foreach (var r in URACarparkInfo)
            {
                if (carparkNo != r.ppCode)
                {
                    double xcoord = 0.00, ycoord = 0.00;

                    if (r.geometries != null)
                    {
                        var uncooord = r.geometries[0].coordinates.Split(",");
                        var x = uncooord[0];
                        var y = uncooord[1];

                        Svy21Coordinate svy21 = new Svy21Coordinate(double.Parse(x), double.Parse(y));

                        LatLongCoordinate latLong = svy21.ToLatLongCoordinate();

                        xcoord = latLong.Latitude;
                        ycoord = latLong.Longitude;
                    }

                    Carpark carpark = new Carpark()
                    {
                        Id = Guid.NewGuid(),
                        CarparkId = r.ppCode,
                        CarparkName = r.ppName,
                        Address = r.ppName,
                        AgencyType = "URA",
                        IsCentral = false,
                        XCoord = xcoord.ToString(),
                        YCoord = ycoord.ToString(),
                        ParkingSystem = "",
                        LotType = ""
                    };

                    carparks.Add(carpark);
                }

                carparkNo = r.ppCode;
            }

            return carparks;
        }

        public static void fixURACarparkData2(ParkingContext context)
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();

            var carparks = new List<Carpark>();

            // URA Carpark Info
            var task = Task.Run(async () => await publicAPI.GetURAParkingInfoAsync());

            var URACarparkInfo = task.Result;

            var carparkNo = "";

            foreach (var r in URACarparkInfo)
            {
                if (carparkNo != r.ppCode)
                {
                    var v = context.Carparks.FirstOrDefault(a => a.CarparkId.Equals(r.ppCode));

                    if (v != null)
                    {
                        if (r.parkingSystem == "C")
                        {
                            v.ParkingSystem = "COUPON PARKING";
                        }
                        else if (r.parkingSystem == "B")
                        {
                            v.ParkingSystem = "ELECTRONIC PARKING";
                        }

                        context.Carparks.Update(v);
                        context.SaveChanges();
                    }
                }

                carparkNo = r.ppCode;
            }

        }

        public static void fixHDBCarparkData(ParkingContext context)
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();

            var task = Task.Run(async () => await publicAPI.GetHDBParkingInfoAsync());

            var HDBCarparkInfo = task.Result;

            foreach (var r in HDBCarparkInfo)
            {
                var v = context.Carparks.FirstOrDefault(a => a.CarparkId.Equals(r.CarparkId));

                if (v != null)
                {
                    v.XCoord = r.XCoord;
                    v.YCoord = r.YCoord;


                    context.Carparks.Update(v);
                    context.SaveChanges();
                }
            }
        }

        public static void fixURACarparkData4(ParkingContext context)
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();

            var carparks = new List<Carpark>();

            // URA Carpark Info
            var task = Task.Run(async () => await publicAPI.GetURAParkingInfoAsync());

            var URACarparkInfo = task.Result;

            var carparkNo = "";

            foreach (var r in URACarparkInfo)
            {
                if (carparkNo != r.ppCode)
                {
                    var v = context.Carparks.FirstOrDefault(a => a.CarparkId.Equals(r.ppCode));

                    if (v != null)
                    {
                        double xcoord = 0.00, ycoord = 0.00;

                        if (r.geometries != null)
                        {
                            var uncooord = r.geometries[0].coordinates.Split(",");
                            var x = float.Parse(uncooord[0]);
                            var y = float.Parse(uncooord[1]);

                            var task2 = Task.Run(async () => await publicAPI.GetCoordinates(x, y));

                            var coordinates = task2.Result;

                            xcoord = coordinates.latitude;
                            ycoord = coordinates.longitude;
                            // Svy21Coordinate svy21 = new Svy21Coordinate(double.Parse(x), double.Parse(y));

                            // LatLongCoordinate latLong = svy21.ToLatLongCoordinate();

                            // xcoord = latLong.Latitude;
                            // ycoord = latLong.Longitude;

                            v.XCoord = xcoord.ToString();
                            v.YCoord = ycoord.ToString();

                            context.Carparks.Update(v);
                            context.SaveChanges();
                        }

                    }
                }

                carparkNo = r.ppCode;
            }

        }

        public static void fixURACarparkData3(ParkingContext context)
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();

            var carparks = new List<Carpark>();

            // URA Carpark Info
            var task = Task.Run(async () => await publicAPI.GetURAParkingInfoAsync());

            var URACarparkInfo = task.Result;

            var carparkNo = "";
            var carparkType = "";

            foreach (var r in URACarparkInfo)
            {
                var v = context.Carparks.FirstOrDefault(a => a.CarparkId.Equals(r.ppCode));

                if (v != null)
                {
                    if (r.vehCat == "Car")
                    {
                        if (!v.LotType.Contains("C"))
                        {
                            v.LotType += "C, ";
                            v.CarCapacity = r.parkCapacity;
                        }
                    }
                    else if (r.vehCat == "Motorcycle")
                    {
                        if (!v.LotType.Contains("M"))
                        {
                            v.LotType += "M, ";
                            v.MCapacity = r.parkCapacity;
                        }
                    }
                    else if (r.vehCat == "Heavy Vehicle")
                    {
                        if (!v.LotType.Contains("H"))
                        {
                            v.LotType += "H, ";
                            v.HVCapacity = r.parkCapacity;
                        }
                    }

                    v.TotalLots = v.CarCapacity + v.MCapacity + v.HVCapacity;

                    context.Carparks.Update(v);
                    context.SaveChanges();
                }

                carparkNo = r.ppCode;
                carparkType = r.vehCat;
            }

        }

        public static void FixHDBCarparkAvailabilityData(ParkingContext context)
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();
            var task = Task.Run(async () => await publicAPI.GetHDBAvailabilityAsync());

            var HDBCarparkAvailability = task.Result;

            foreach (var value in HDBCarparkAvailability)
            {
                var v = context.Carparks.FirstOrDefault(a => a.CarparkId.Equals(value.carpark_number));

                if (v != null)
                {
                    if (value.carpark_info.Length > 0)
                    {
                        foreach (var x in value.carpark_info)
                        {
                            if (x.lot_type.Equals("C"))
                            {
                                v.LotType += "C, ";
                                v.CarCapacity = int.Parse(x.total_lots);
                                v.CarAvailability = int.Parse(x.lots_available);
                            }
                            else if (x.lot_type.Equals("H"))
                            {
                                v.LotType += "H, ";
                                v.HVCapacity = int.Parse(x.total_lots);
                                v.HVAvailability = int.Parse(x.lots_available);
                            }
                            else if (x.lot_type.Equals("Y"))
                            {
                                v.LotType += "M, ";
                                v.MCapacity = int.Parse(x.total_lots);
                                v.MAvailability = int.Parse(x.lots_available);
                            }
                        }
                    }

                    v.TotalLots = v.CarCapacity + v.HVCapacity + v.MCapacity;
                    v.TotalAvailableLots = v.CarAvailability + v.HVAvailability + v.MAvailability;

                    context.Carparks.Update(v);
                    context.SaveChanges();
                }
            }
        }


        public static void fixURACarparkAvailabilityData(ParkingContext context)
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();

            // URA Carpark Availability
            var task2 = Task.Run(async () => await publicAPI.GetURAAvailability());

            var URACarparkAvailability = task2.Result;

            foreach (var value in URACarparkAvailability)
            {

                var v = context.Carparks.FirstOrDefault(a => a.CarparkId.Equals(value.carparkNo));

                // carpark lot type availability
                if (v != null)
                {
                    if (value.lotType == "C")
                    {
                        v.CarAvailability = int.Parse(value.lotsAvailable);
                        v.LotType += "C,";
                    }
                    else if (value.lotType == "H")
                    {
                        v.HVAvailability = int.Parse(value.lotsAvailable);
                        v.LotType += "H,";
                    }
                    else if (value.lotType == "M")
                    {
                        v.MAvailability = int.Parse(value.lotsAvailable);
                        v.LotType += "M,";
                    }
                    else
                    {
                        v.LotType += "No Data";
                    }

                    v.TotalAvailableLots = v.CarAvailability + v.HVAvailability + v.MAvailability;

                }
                context.Carparks.Update(v);
                context.SaveChanges();
            }


        }

        public static void FixLTAData(ParkingContext context)
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();

            // URA Carpark Info
            var task2 = Task.Run(async () => await publicAPI.GetParkingInfoAsync());

            var LTACarpark = task2.Result;

            context.Carparks.AddRange(LTACarpark);

            context.SaveChanges();
        }


        public static void FixShoppingMallData(ParkingContext context)
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();

            // URA Carpark Info
            var task2 = Task.Run(async () => await publicAPI.GetShoppingMallAsync());

            var ShoppingmallCarpark = task2.Result;

            List<Carpark> carparks = new List<Carpark>();

            int i = 1;
            foreach (var shop in ShoppingmallCarpark)
            {
                Carpark carpark = new Carpark()
                {
                    Id = Guid.NewGuid(),
                    CarparkName = shop.carpark,
                    Address = shop.category,
                    AgencyType = "Shopping Mall",
                    ParkingSystem = "",
                    CarAvailability = 0,
                    CarCapacity = 0,
                    CarparkId = $"SHP{i}",
                    HVAvailability = 0,
                    HVCapacity = 0,
                    IsCentral = false,
                    LotType = "C",
                    MAvailability = 0,
                    MCapacity = 0,
                    TotalAvailableLots = 0,
                    TotalLots = 0,
                    XCoord = "",
                    YCoord = ""
                };

                carparks.Add(carpark);

                i++;
            }

            context.Carparks.AddRange(carparks);

            context.SaveChanges();
        }


        public static void FixURACarparkRatesData(ParkingContext context)
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();

            var task2 = Task.Run(async () => await publicAPI.GetURAParkingInfoAsync());

            var result = task2.Result;

            foreach (var r in result)
            {
                var h = context.Carparks.FirstOrDefault(a => a.CarparkId == r.ppCode);

                if (h != null)
                {
                    // if there isn't an id filled out (ie: we're not upserting),
                    // we should generate one

                    CarparkRate carparkRate = new CarparkRate
                    {
                        Id = Guid.NewGuid(),
                        // CarparkId = h.Id,
                        StartTime = r.startTime,
                        EndTime = r.endTime,
                        Remarks = r.remarks,
                        SatdayMin = r.satdayMin,
                        SunPHMin = r.sunPHMin,
                        VehicleType = r.vehCat,
                        WeekdayMin = r.weekdayMin,
                        WeekdayRate = r.weekdayRate,
                        SatdayRate = r.satdayRate,
                        SunPHRate = r.sunPHRate
                    };

                    context.CarparkRates.Add(carparkRate);
                }
            }

            context.SaveChanges();

        }

        public static void FixHDBCarparkRates(ParkingContext context)
        {
            var weekDayMin = "";
            var weekDayRate = "";
            var satDayMin = "";
            var satDayRate = "";
            var sunPHMin = "";
            var sunPHRate = "";
            var startTime = "";
            var endTime = "";
            var vehType = "";
            var phStartTime = "";
            var phEndTime = "";
            var phRate = "";
            var phDays = "";
            var phMin = "";

            foreach (var carpark in context.Carparks)
            {
                if (carpark.AgencyType == "HDB")
                {
                    if (carpark.IsCentral && carpark.LotType.Contains("C"))
                    {
                        weekDayMin = "30 mins";
                        weekDayRate = "$1.20";
                        satDayMin = "30 mins";
                        satDayRate = "$1.20";
                        sunPHMin = "30 mins";
                        sunPHRate = "$0.60";
                        startTime = "07.30 AM";
                        endTime = "05.00 PM";
                        vehType = "Car";
                    }

                    if (!carpark.IsCentral && carpark.LotType.Contains("C"))
                    {
                        weekDayMin = "30 mins";
                        weekDayRate = "$0.60";
                        satDayMin = "30 mins";
                        satDayRate = "$0.60";
                        sunPHMin = "30 mins";
                        sunPHRate = "$0.60";
                        startTime = "12.00 AM";
                        endTime = "11.59 PM";
                        vehType = "Car";
                    }

                    if (carpark.LotType.Contains("M"))
                    {
                        weekDayMin = "WHOLE DAY";
                        weekDayRate = "$0.65";
                        satDayMin = "WHOLE DAY";
                        satDayRate = "$0.65";
                        sunPHMin = "WHOLE DAY";
                        sunPHRate = "$0.65";
                        startTime = "07.00 AM";
                        endTime = "10.30 PM";
                        vehType = "Motorcycle";
                    }

                    if (carpark.LotType.Contains("H"))
                    {
                        weekDayMin = "30 mins";
                        weekDayRate = "$1.20";
                        satDayMin = "30 mins";
                        satDayRate = "$1.20";
                        sunPHMin = "30 mins";
                        sunPHRate = "$1.20";
                        startTime = "12.00 AM";
                        endTime = "11.59 PM";
                        vehType = "Heavy Vehicle";
                    }

                    if (carpark.CarparkId.Equals("ACB"))
                    {
                        phStartTime = "10.00 AM";
                        phEndTime = "10.30 PM";
                        phRate = "$1.40";
                        phDays = "ALL DAYS";
                    }

                    if (carpark.CarparkId.Equals("B6")
                        || carpark.CarparkId.Equals("B6M")
                        || carpark.CarparkId.Equals("B7"))
                    {
                        phStartTime = "10.00 AM";
                        phEndTime = "08.00 PM";
                        phRate = "$0.80";
                        phDays = "ALL DAYS";
                        phMin = "30 mins";
                    }

                    if (carpark.CarparkId.Equals("KB10")
                        || carpark.CarparkId.Equals("KB11")
                        || carpark.CarparkId.Equals("KB12"))
                    {
                        phStartTime = "07.00 AM";
                        phEndTime = "02.00 PM";
                        phRate = "$0.80";
                        phDays = "MON - SAT";
                        phMin = "30 mins";
                    }

                    if (carpark.CarparkId.Equals("BBB"))
                    {
                        phStartTime = "10.00 AM";
                        phEndTime = "10.30 PM";
                        phRate = "$1.40";
                        phDays = "ALL DAYS";
                        phMin = "30 mins";
                    }

                    if (carpark.CarparkId.Equals("CY"))
                    {
                        phStartTime = "10.00 AM";
                        phEndTime = "10.30 PM";
                        phRate = "$0.80";
                        phDays = "ALL DAYS";
                        phMin = "30 mins";
                    }

                    if (carpark.CarparkId.Equals("GSM"))
                    {
                        phStartTime = "07.00 AM";
                        phEndTime = "10.30 PM";
                        phRate = "$0.80";
                        phDays = "FRI - SUN && Public Holidays";
                        phMin = "30 mins";
                    }

                    if (carpark.CarparkId.Equals("CR1"))
                    {
                        phStartTime = "10.00 AM";
                        phEndTime = "10.30 PM";
                        phRate = "$0.80";
                        phDays = "ALL DAYS";
                        phMin = "30 mins";
                    }

                    var carparkRate = new CarparkRate()
                    {
                        Id = Guid.NewGuid(),
                        // CarparkId = carpark.Id,
                        StartTime = startTime,
                        EndTime = endTime,
                        SatdayMin = satDayMin,
                        SatdayRate = satDayRate,
                        SunPHMin = sunPHMin,
                        SunPHRate = sunPHRate,
                        VehicleType = vehType,
                        WeekdayMin = weekDayMin,
                        WeekdayRate = weekDayRate,
                        //PHDays = phDays,
                        //PHEndTime = phEndTime,
                        //PHRate = phRate,
                        //PHStartTime = phStartTime
                    };

                    context.CarparkRates.Add(carparkRate);

                    weekDayMin = "";
                    weekDayRate = "";
                    satDayMin = "";
                    satDayRate = "";
                    sunPHMin = "";
                    sunPHRate = "";
                    startTime = "";
                    endTime = "";
                    vehType = "";
                    phStartTime = "";
                    phEndTime = "";
                    phRate = "";
                    phDays = "";
                    phMin = "";
                }
            }

            context.SaveChanges();
        }

        public static void FixShoppingMallCarparkRates(ParkingContext context)
        {
            FetchPublicAPI publicAPI = new FetchPublicAPI();

            var task = Task.Run(async () => await publicAPI.GetShoppingMallAsync());

            var result = task.Result;

            var x = "";

            foreach (var v in result)
            {
                var h = context.Carparks.FirstOrDefault(a => a.CarparkName == v.carpark);

                if (h != null)
                {
                    x += $"Weekdays Rate: {v.weekdays_rate_1}, {v.weekdays_rate_2}, Saturday Rate: {v.saturday_rate}, Sunday / Public Holiday Rate: {v.sunday_publicholiday_rate}";

                    CarparkRate carparkRate = new CarparkRate
                    {
                        Id = Guid.NewGuid(),
                        // CarparkId = h.Id,
                        Remarks = x
                    };

                    context.CarparkRates.Add(carparkRate);
                }

                x = "";
            }

            context.SaveChanges();
        }

        //public static void ConvertDateTimeFormat(ParkingContext context)
        //{
        //    string s = "";
        //    foreach (var v in context.CarparkRates)
        //    {

        //        if (v.PHEndTime2 == null || v.PHEndTime2 == "")
        //        {
        //        }
        //        else
        //        {
        //            s = "";

        //            if (v.PHEndTime2.Contains("PM"))
        //            {
        //                var t4hr = int.Parse(v.PHEndTime2.Substring(0, 2)) + 12;

        //                var x = v.PHEndTime2.Substring(3, 3);
        //                x = x.Replace(" ", string.Empty);

        //                s = t4hr + ":" + x;
        //            }
        //            else
        //            {
        //                var x = v.PHEndTime2.Substring(3, 3);
        //                x = x.Replace(" ", string.Empty);

        //                s = v.PHEndTime2.Substring(0, 2) + ":" + x;
        //            }
        //            s += ":00";

        //            var result = Convert.ToDateTime(s);
        //            string TimeDateFormat = result.ToString("hh:mm:ss", CultureInfo.CurrentCulture);

        //            v.PHEndTime2 = s;
        //        }

        //        context.CarparkRates.Update(v);
        //    }

        //    context.SaveChanges();
        //}
    }
}
