namespace HereWeGoAPI.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects;

    public class LocationData
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IList<string> Images { get; set; }

        public IDictionary<string, IList<Tuple<DateTime, DateTime>>> OpenSchedule { get; set; }

        public Category Category { get; set; }

        public float AverageRating { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public TimeSpan DurationToVisit { get; set; }
    }
}