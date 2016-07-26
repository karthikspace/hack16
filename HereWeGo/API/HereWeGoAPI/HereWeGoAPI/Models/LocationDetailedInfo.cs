using System;
using System.Collections.Generic;
using Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects;

namespace HereWeGoAPI.Models
{
    public class LocationDetailedInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IList<string> Images { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string Summary { get; set; }

        public string Address { get; set; }

        public string ContactNumber { get; set; }

        public string WebsiteUrl { get; set; }

        public IDictionary<string, IList<Tuple<DateTime, DateTime>>> OpenSchedule { get; set; }

        public IList<Review> Reviews { get; set; }

        public Category Category { get; set; }

        public float AverageRating { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public int DurationToVisit { get; set; }
    }

    public class Review
    {
        public UserData UserId { get; set; }

        public string Statement { get; set; }

        public DateTime Date { get; set; }

        public float Rating { get; set; }
    }
}