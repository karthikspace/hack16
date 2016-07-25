﻿using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using HereWeGoAPI.Models;
using Microsoft.RewardsIntl.Platform.DataAccess.Azure.DataAccess;
using Microsoft.RewardsIntl.Platform.DataAccess.Common;
using DaObjects = Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects;

namespace HereWeGoAPI.Controllers
{
    public class TripHackController : ApiController
    {
        private static IDataAccess dataAccess;

        static TripHackController()
        {
            dataAccess = DataAccessFactory.CreateDataAccessObject();
        }
            
            // GET: TripHack
        public LocationData GetLocation(string locationId)
        {
            var locationData = dataAccess.GetObject<DaObjects.LocationInfo>(locationId, null);
            if (locationData == null)
            {
                return null;
            }

            return ConvertToLocation(locationData);
        }

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


                // Add location adding logic based on time, rating etc
                locations.Add(ConvertToLocation(locationData));
            }

            return locations.Count > 0 ? locations : null;
        }

        public IList<string> GetDestinations()
        {
            return new List<string>()
            {
                "Amsterdam",
                "Goa",
                "Hyderabad",
                "Santorini",
                "Sydney"
            };
        }

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

        private static LocationData ConvertToLocation(DaObjects.LocationInfo locationInfo)
        {
            return  new LocationData()
            {
                Name = locationInfo.Name,
                Address = locationInfo.Address,
                AverageRating = locationInfo.AverageRating,
                Category = locationInfo.Category,
                City = locationInfo.City,
                ContactNumber = locationInfo.ContactNumber,
                Country = locationInfo.Country,
                Id = locationInfo.Id,
                Images = locationInfo.Images,
                Latitude = locationInfo.Latitude,
                Longitude = locationInfo.Longitude,
                Reviews = locationInfo.Reviews,
                Schedule = locationInfo.Schedule,
                Summary = locationInfo.Summary,
                WebsiteUrl = locationInfo.WebsiteUrl,
            };
        }
    }
}