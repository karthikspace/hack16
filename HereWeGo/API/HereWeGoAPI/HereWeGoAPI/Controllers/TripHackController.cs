using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Hosting;
using System.Web.Http;
using HereWeGoAPI.Models;
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
        public LocationDetailedInfo GetLocation(string locationId)
        {
            var locationInfo = dataAccess.GetObject<DaObjects.LocationInfo>(locationId, null);
            return ConvertToLocationDetailedData(locationInfo);
        }

        [HttpGet]
        public IList<LocationData> GetLocations(string destinationId, DateTime start, DateTime end)
        {
            var destinationInfo = dataAccess.GetObject<DaObjects.Destination>(destinationId, null);
            if (destinationInfo == null || destinationInfo.Locations == null || destinationInfo.Locations.Count == 0)
            {
                return null;
            }

            var locations = new List<LocationData>();
            foreach (var locationId in destinationInfo.Locations)
            {
                var locationData = dataAccess.GetObject<DaObjects.LocationInfo>(locationId, null);
                if (locationData == null)
                {
                    continue;
                }
                for (var day = start.Date; day.Date <= end.Date; day = day.AddDays(1))
                {
                    var dayofweek = day.DayOfWeek.ToString();
                    var schedule = locationData.OpenSchedule[dayofweek];
                    // Add location adding logic based on time, rating etc
                    if (schedule.Count != 0)
                    {
                        locations.Add(ConvertToLocationData(locationData));
                        break;
                    }
                }
            }

            var sortedLocations = locations.OrderByDescending(x => x.AverageRating).Take(20).ToList();
            return sortedLocations.Count > 0 ? sortedLocations : null;
        }

        [HttpGet]
        public IList<LocationData> GetLocations(string destinationId, int preference)
        {
            if (preference < 1)
            {
                preference = 1;
            }

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

                locations.Add(locationData);
            }

            // Sorting based on Average Rating
            var sortedLocations = locations.OrderByDescending(x => x.AverageRating).ToList();
            var limit = 20;
            var preferredLocations = new List<LocationData>();
            for (var i = limit * (preference - 1); i < limit * preference; i++)
            {
                preferredLocations.Add(ConvertToLocationData(sortedLocations[i]));
            }

            return preferredLocations.Count > 0 ? preferredLocations : null;
        }

        [HttpGet]
        public IList<string> GetDestinations()
        {
            return new List<string>()
            {
                "Amsterdam",
                "Goa",
                "Hyderabad",
                "Paris",
                "Sydney"
            };
        }

        [HttpPost]
        public bool AddUser(UserData newUser)
        {
            var userData = new DaObjects.UserInformation()
            {
                UserId = newUser.UserId,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName
            };

            dataAccess.SetObject(userData);
            dataAccess.Flush();

            return true;
        }

        [HttpGet]
        public IList<TripData> GetTrips(string id)
        {
            var userInformation = dataAccess.GetObject<DaObjects.UserInformation>(id, null);
            if (userInformation == null ||
                userInformation.Trips == null ||
                userInformation.Trips.Count == 0)
            {
                return null;
            }

            var trips = new List<TripData>();
            foreach (var tripId in userInformation.Trips)
            {
                var trip = dataAccess.GetObject<DaObjects.TripInformation>(tripId, null);
                if (trip == null)
                {
                    continue;
                }

                trips.Add(ConvertToTripData(trip));
            }

            return trips.Count > 0 ? trips : null;
        }

        [HttpPost]
        public bool UpdateTrip(TripData newTrip)
        {
            if (newTrip == null)
            {
                return false;
            }

            var existingTrip = dataAccess.GetObject<DaObjects.TripInformation>(newTrip.TripId, null);
            if (existingTrip == null)
            {
                return false;
            }

            existingTrip.Locations = GetTripScheduleForServer(newTrip.Locations);
            existingTrip.EndDateUTC = newTrip.EndDateUTC;
            existingTrip.StartDateUTC = newTrip.StartDateUTC;
            existingTrip.TripStatus = newTrip.TripStatus;
            existingTrip.DestinationId = newTrip.DestinationId;

            dataAccess.SetObject(existingTrip);
            dataAccess.Flush();
            return true;
        }

        [HttpPost]
        public string CreateTrip(TripData newTrip)
        {
            var user = dataAccess.GetObject<DaObjects.UserInformation>(newTrip.UserId, null);
            if (user == null)
            {
                return null;
            }

            var newTripInfo = new DaObjects.TripInformation()
            {
                Locations = GetTripScheduleForServer(newTrip.Locations),
                TripId = newTrip.TripId,
                EndDateUTC = newTrip.EndDateUTC,
                StartDateUTC = newTrip.StartDateUTC,
                TripStatus = newTrip.TripStatus,
                DestinationId = newTrip.DestinationId
            };

            dataAccess.SetObject(newTripInfo);
            user.Trips = user.Trips == null ? new List<string>() : new List<string>(user.Trips);
            user.Trips.Add(newTrip.TripId);

            dataAccess.SetObject(user);
            dataAccess.Flush();
            return newTrip.TripId;
        }

        [HttpGet]
        public TripData GetTrip(string userId, string tripId)
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

            return ConvertToTripData(trip);
        }

        [HttpGet]
        public string FillData()
        {
            try
            {
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

                IList<string> users = new List<string>()
                {
                    "newuser1",
                    "newuser2",
                    "newuser3",
                    "anonymous"
                };

                IList<string> statements = new List<string>()
                {
                    "Very nice place!!!",
                    "Amazing Place!!..a must visit",
                    "Pathetic ambience :(",
                    "Awesome time spent"
                };

                Random rnd = new Random();
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

                    var numberOfDays = rnd.Next(3, 8);
                    for (int k = 0; k < numberOfDays; k++)
                    {
                        var dayOfWeek = days[k];
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
                            UserId = users[rnd.Next(4)],
                            Date = new DateTime(2016, 7, 26, 16, 00, 00),
                            Rating = (float)(rnd.NextDouble() * 5.0),
                            Statement = statements[rnd.Next(4)]
                        };

                        newLocation.Reviews.Add(newReview);
                    }

                    newLocation.Category = DaObjects.Category.Entertainment;

                    newLocation.AverageRating = (float)(rnd.NextDouble() * 5.0);

                    newLocation.Latitude = xlRange.Cells[i, 4].Value2.ToString();
                    newLocation.Longitude = xlRange.Cells[i, 5].Value2.ToString();
                    newLocation.DurationToVisit = new TimeSpan(rnd.Next(1, 4), 0, 0);
                    dataAccess.SetObject(newLocation);
                }

                xlWorkbook.Close();

                // Populate city -> Location mapping
                xlWorkbook = xlApp.Workbooks.Open(HostingEnvironment.MapPath(@"~/Controllers/DestinationLocation.csv"));
                xlWorksheet = xlWorkbook.Sheets[1];
                xlRange = xlWorksheet.UsedRange;

                rowCount = xlRange.Rows.Count;
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

                xlWorkbook.Close();

                dataAccess.SetObject(newDestination);
                dataAccess.Flush();
                return "Success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [HttpGet]
        public string FillTripData()
        {
            var newTripId = Guid.NewGuid().ToString();
            var newUser = new DaObjects.UserInformation()
            {
                UserId = "NewUser1",
                FirstName = "Abhishek",
                LastName = "Gupta",
                Trips = new List<string>() { newTripId }
            };

            dataAccess.SetObject(newUser);

            var newTrip = new DaObjects.TripInformation()
            {
                DestinationId = "Paris",
                TripId = newTripId,
                TripStatus = DaObjects.TripStatus.Upcoming,
                StartDateUTC = new DateTime(2016, 12, 25, 10, 0, 0),
                EndDateUTC = new DateTime(2016, 12, 25, 10, 0, 0),
                Locations = new List<DaObjects.TripSchedule>()
                {
                    new DaObjects.TripSchedule()
                    {
                        LocationId = "4ADCDA03F964A520C03121E3",
                        Start = new DateTime(2016, 12, 25, 12, 0, 0),
                        End = new DateTime(2016, 12, 25, 14, 0, 0)
                    },
                    new DaObjects.TripSchedule()
                    {
                        LocationId = "4ADCDA05F964A520963221E3",
                        Start = new DateTime(2016, 12, 25, 16, 0, 0),
                        End = new DateTime(2016, 12, 25, 17, 0, 0)
                    }
                }
            };

            dataAccess.SetObject(newTrip);
            dataAccess.Flush();
            return "success";
        }

        private static LocationData ConvertToLocationData(DaObjects.LocationInfo locationInfo)
        {
            return new LocationData()
            {
                Id = locationInfo.Id,
                Images = locationInfo.Images,
                Longitude = locationInfo.Longitude,
                Name = locationInfo.Name,
                Category = locationInfo.Category,
                Latitude = locationInfo.Latitude,
                AverageRating = locationInfo.AverageRating,
                OpenSchedule = locationInfo.OpenSchedule,
                DurationToVisit = locationInfo.DurationToVisit
            };
        }

        private static LocationDetailedInfo ConvertToLocationDetailedData(DaObjects.LocationInfo locationInfo)
        {
            return new LocationDetailedInfo()
            {
                Id = locationInfo.Id,
                Images = locationInfo.Images,
                Longitude = locationInfo.Longitude,
                Name = locationInfo.Name,
                Category = locationInfo.Category,
                Latitude = locationInfo.Latitude,
                AverageRating = locationInfo.AverageRating,
                OpenSchedule = locationInfo.OpenSchedule,
                City = locationInfo.City,
                Reviews = GetReviews(locationInfo.Reviews),
                Country = locationInfo.Country,
                ContactNumber = locationInfo.ContactNumber,
                Address = locationInfo.Address,
                Summary = locationInfo.Summary,
                WebsiteUrl = locationInfo.WebsiteUrl,
                DurationToVisit = locationInfo.DurationToVisit
            };
        }

        private static TripData ConvertToTripData(DaObjects.TripInformation tripDetails)
        {
            return new TripData()
            {
                Locations = GetTripScheduleForClient(tripDetails.Locations),
                EndDateUTC = tripDetails.EndDateUTC,
                StartDateUTC = tripDetails.StartDateUTC,
                TripStatus = tripDetails.TripStatus,
                TripId = tripDetails.TripId,
                DestinationId = tripDetails.DestinationId
            };
        }

        private static IList<Review> GetReviews(IList<DaObjects.Review> reviews)
        {
            var newReviews = new List<Review>();
            foreach (var review in reviews)
            {
                var user = dataAccess.GetObject<DaObjects.UserInformation>(review.UserId, null);
                var newReview = new Review()
                {
                    UserId = user.FirstName + " " + user.LastName,
                    Statement = review.Statement,
                    Date = review.Date,
                    Rating = review.Rating
                };

                newReviews.Add(newReview);
            }

            return newReviews;
        }

        private static IList<DaObjects.TripSchedule> GetTripScheduleForServer(IList<TripSchedule> tripSchedule)
        {
            var serverTripSchedule = new List<DaObjects.TripSchedule>();
            foreach (var tripLocation in tripSchedule)
            {
                var newEntry = new DaObjects.TripSchedule()
                {
                    End = tripLocation.End,
                    Start = tripLocation.Start,
                    LocationId = tripLocation.LocationId
                };

                serverTripSchedule.Add(newEntry);
            }

            return serverTripSchedule;
        }

        private static IList<TripSchedule> GetTripScheduleForClient(IList<DaObjects.TripSchedule> tripSchedule)
        {
            var clientTripSchedule = new List<TripSchedule>();
            foreach (var tripLocation in tripSchedule)
            {
                var newEntry = new TripSchedule()
                {
                    End = tripLocation.End,
                    Start = tripLocation.Start,
                    LocationId = tripLocation.LocationId
                };

                clientTripSchedule.Add(newEntry);
            }

            return clientTripSchedule;
        }
    }
}