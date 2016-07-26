using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Hosting;
using System.Web.Http;
using Microsoft.RewardsIntl.Platform.DataAccess.Azure.DataAccess;
using Microsoft.RewardsIntl.Platform.DataAccess.Common;
using DaObjects = Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects;
using Excel = Microsoft.Office.Interop.Excel;

namespace HereWeGoAPI.Controllers
{
    public class TripHackController : ApiController
    {
        private static IDataAccess dataAccess;

        static TripHackController()
        {
            dataAccess = DataAccessFactory.CreateDataAccessObject();
        }

        [HttpGet]
        public DaObjects.LocationInfo GetLocation(string locationId)
        {
            return dataAccess.GetObject<DaObjects.LocationInfo>(locationId, null);
        }

        [HttpGet]
        public IList<DaObjects.LocationInfo> GetLocations(string destinationId, DateTime start, DateTime end)
        {
            var destinationInfo = dataAccess.GetObject<DaObjects.Destination>(destinationId, null);
            if (destinationInfo == null || destinationInfo.Locations == null || destinationInfo.Locations.Count == 0)
            {
                return null;
            }

            var locations = new List<DaObjects.LocationInfo>();
            foreach (var locationId in destinationInfo.Locations)
            {
                var locationData = dataAccess.GetObject<DaObjects.LocationInfo>(locationId, null);
                if (locationData == null)
                {
                    continue;
                }

                // Add location adding logic based on time, rating etc
                locations.Add(locationData);
            }

            return locations.Count > 0 ? locations : null;
        }

        [HttpGet]
        public IList<DaObjects.LocationInfo> GetLocations(string destinationId)
        {
            var destinationInfo = dataAccess.GetObject<DaObjects.Destination>(destinationId, null);
            if (destinationInfo == null || destinationInfo.Locations == null || destinationInfo.Locations.Count == 0)
            {
                return null;
            }

            var locations = new List<DaObjects.LocationInfo>();
            foreach (var locationId in destinationInfo.Locations)
            {
                var locationData = dataAccess.GetObject<DaObjects.LocationInfo>(locationId, null);
                if (locationData == null)
                {
                    continue;
                }

                // Add location adding logic based on time, rating etc
                locations.Add(locationData);
            }

            return locations.Count > 0 ? locations : null;
        }

        public IList<DaObjects.LocationInfo> GetLocations(string destinationId,int preference)
        {
            var destinationInfo = dataAccess.GetObject<DaObjects.Destination>(destinationId, null);
            if (destinationInfo == null || destinationInfo.Locations == null || destinationInfo.Locations.Count == 0)
            {
                return null;
            }

            var locations = new List<DaObjects.LocationInfo>();
            foreach (var locationId in destinationInfo.Locations)
            {
                var locationData = dataAccess.GetObject<DaObjects.LocationInfo>(locationId, null);
                if (locationData == null)
                {
                    continue;
                }
                Random rand = new Random();
                float rating = (float)rand.Next(0, 5);
                locationData.AverageRating = rating;

                // Add location adding logic based on time, rating etc
                locations.Add(locationData);
            }

            // Sorting based on Average Rating
            var slocations = locations.OrderByDescending(x => x.AverageRating).ToList();
            int limit = 20;

            var preflocations = new List<DaObjects.LocationInfo>();
            for ( int i = limit*preference-limit+1;  i<=limit*preference; i++ )
            {
                preflocations.Add(slocations[i]);
            }
            return preflocations.Count > 0 ? preflocations : null;
        }

        [HttpGet]
        public IList<string> GetDestinations()
        {
            return new List<string>()
            {
                "Amsterdam",
                "Goa",
                "Hyderabad",
                "Santorini",
                "Sydney",
                "Paris"
            };
        }

        [HttpPost]
        public bool AddUser(string id, string fn, string ln)
        {
            var userData = new DaObjects.UserInformation()
            {
                UserId = id,
                FirstName = fn,
                LastName = ln
            };

            dataAccess.SetObject(userData);
            dataAccess.Flush();

            return true;
        }

        [HttpGet]
        public IList<DaObjects.TripInformation> GetTrips(string id)
        {
            var userInformation = dataAccess.GetObject<DaObjects.UserInformation>(id, null);
            if (userInformation == null ||
                userInformation.Trips == null ||
                userInformation.Trips.Count == 0)
            {
                return null;
            }

            var trips = new List<DaObjects.TripInformation>();
            foreach (var tripId in userInformation.Trips)
            {
                var trip = dataAccess.GetObject<DaObjects.TripInformation>(tripId, null);
                if (trip == null)
                {
                    continue;
                }

                trips.Add(trip);
            }

            return trips.Count > 0 ? trips : null;
        }

        [HttpPost]
        public bool UpdateTrip(DaObjects.TripInformation trip)
        {
            if (trip == null)
            {
                return false;
            }

            dataAccess.SetObject(trip);
            dataAccess.Flush();
            return true;
        }

        [HttpPost]
        public string CreateTrip(DaObjects.TripInformation newTrip, string userId)
        {
            var user = dataAccess.GetObject<DaObjects.UserInformation>(userId, null);
            if (user == null)
            {
                return null;
            }

            dataAccess.SetObject(newTrip);
            user.Trips = new List<string>(user.Trips);
            user.Trips.Add(newTrip.TripId);

            dataAccess.SetObject(user);
            dataAccess.Flush();
            return newTrip.TripId;
        }

        [HttpGet]
        public DaObjects.TripInformation GetTrip(string userId, string tripId)
        {
            var user = dataAccess.GetObject<DaObjects.UserInformation>(userId, null);
            if (user == null)
            {
                return null;
            }

            var isTripRegisteredWithUser = user.Trips != null && user.Trips.Contains(tripId);

            var trip = dataAccess.GetObject<DaObjects.TripInformation>(tripId, null);
            if (trip != null && !isTripRegisteredWithUser)
            {
                user.Trips = user.Trips == null ? new List<string>() : new List<string>(user.Trips);
                user.Trips.Add(tripId);
            }
            else if (trip == null && isTripRegisteredWithUser)
            {
                user.Trips.Remove(tripId);
            }

            dataAccess.SetObject(user);
            dataAccess.Flush();

            return trip;
        }

        [HttpGet]
        private string FillData()
        {
            try
            {
                /*
                Excel.Application xlApp = new Excel.Application();
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(HostingEnvironment.MapPath(@"~/Controllers/paris-attraction.csv"));
                Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                Excel.Range xlRange = xlWorksheet.UsedRange;

                int rowCount = xlRange.Rows.Count;
                IList<string> days = new List<string>();
                days.Add("Monday");
                days.Add("Tuesday");
                days.Add("Wednesday");
                days.Add("Thursday");
                days.Add("Friday");
                days.Add("Saturday");
                days.Add("Sunday");

                for (int i = 2; i <= rowCount; i++)
                {
                    var newLocation = new DaObjects.LocationInfo();

                    // 1 address 2 category 3 id 4 lat 5 long 6 city 7 name 8 id 11 details 
                    newLocation.Name = xlRange.Cells[i, 7].Value2.ToString();

                    newLocation.Address = xlRange.Cells[i, 1].Value2.ToString();

                    newLocation.City = xlRange.Cells[i, 6].Value2.ToString();

                    newLocation.Id = xlRange.Cells[i, 8].Value2.ToString();

                    newLocation.Country = "France";

                    newLocation.ContactNumber = "0909876543210";

                    newLocation.Summary = "Paris is the capital and most populous city of France. Situated on the river Seine in northern metropolitan France, it is in the centre of the Île-de-France region, also known as the région parisienne, Paris Region. ";

                    IList<string> images = new List<string>();
                    images.Add("https://www.google.co.in/search?q=paris+%2Bimages&espv=2&biw=1243&bih=567&source=lnms&tbm=isch&sa=X&ved=0ahUKEwioleKukpDOAhUfR48KHY3CCiIQ_AUIBigB#imgrc=YJE3PnhRhomodM%3A");
                    images.Add("https://www.google.co.in/search?q=paris+%2Bimages&espv=2&biw=1243&bih=567&source=lnms&tbm=isch&sa=X&ved=0ahUKEwioleKukpDOAhUfR48KHY3CCiIQ_AUIBigB#imgrc=hYmBkW9T2g2wRM%3A");

                    newLocation.Images = images;
                    newLocation.WebsiteUrl = xlRange.Cells[i, 11].Value2.ToString();

                    IDictionary<string, IList<Tuple<DateTime, DateTime>>> openSchedule = new Dictionary<string, IList<Tuple<DateTime, DateTime>>>();

                    for (int k = 0; k < 7; k++)
                    {
                        string dayOfWeek = days[k];
                        var openDateTime1 = new DateTime(2016, 7, 26, 9, 00, 00);
                        var closeDateTime1 = new DateTime(2016, 7, 26, 12, 00, 00);
                        var openDateTime2 = new DateTime(2016, 7, 26, 16, 00, 00);
                        var closeDateTime2 = new DateTime(2016, 7, 26, 22, 00, 00);
                        var temp = new List<Tuple<DateTime, DateTime>>
                        {
                             Tuple.Create(openDateTime1,closeDateTime1),
                             Tuple.Create(openDateTime2,closeDateTime2)
                        };

                        openSchedule.Add(dayOfWeek, temp);
                    }

                    newLocation.OpenSchedule = openSchedule;

                    newLocation.Reviews = new List<DaObjects.Review>();
                    for (int f = 1; f <= 5; f++)
                    {
                        var newReview = new DaObjects.Review()
                        {
                            UserId = "Anonymous",
                            Date = new DateTime(2016, 7, 26, 16, 00, 00),
                            Rating = 3.4f,
                            Statement = "Brilliant!!..Excellent"
                        };

                        newLocation.Reviews.Add(newReview);
                    }

                    newLocation.Category = DaObjects.Category.Entertainment;

                    newLocation.AverageRating = 3.4F;

                    newLocation.Latitude = xlRange.Cells[i, 4].Value2.ToString();
                    newLocation.Longitude = xlRange.Cells[i, 5].Value2.ToString();

                    dataAccess.SetObject(newLocation);
                }*/

                Excel.Application xlApp = new Excel.Application();
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(HostingEnvironment.MapPath(@"~/Controllers/DestinationLocation.csv"));
                Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                Excel.Range xlRange = xlWorksheet.UsedRange;

                int rowCount = xlRange.Rows.Count;
                var newDestination = new DaObjects.Destination();
                newDestination.Id = xlRange.Cells[1, 1].Value2.ToString();
                newDestination.Name = newDestination.Id;
                newDestination.Images = new List<string>()
                {
                    "https://www.google.co.in/search?q=paris+%2Bimages&espv=2&biw=1243&bih=567&source=lnms&tbm=isch&sa=X&ved=0ahUKEwioleKukpDOAhUfR48KHY3CCiIQ_AUIBigB#imgrc=YJE3PnhRhomodM%3A",
                    "https://www.google.co.in/search?q=paris+%2Bimages&espv=2&biw=1243&bih=567&source=lnms&tbm=isch&sa=X&ved=0ahUKEwioleKukpDOAhUfR48KHY3CCiIQ_AUIBigB#imgrc=hYmBkW9T2g2wRM%3A"
                };

                var locations = new List<string>();
                for (int i = 1; i <= rowCount; i++)
                {
                    locations.Add(xlRange.Cells[i, 2].Value2.ToString());
                }

                newDestination.Locations = locations;

                dataAccess.SetObject(newDestination);
                dataAccess.Flush();
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}