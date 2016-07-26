using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects;

namespace HereWeGoAPI.Models
{
    public class TripData
    {
        public string TripId { get; set; }

        public string DestinationId { get; set; }

        public DateTime StartDateUTC { get; set; }

        public DateTime EndDateUTC { get; set; }

        /// <summary>
        /// List of all the locations included in the trip
        /// </summary>
        public IList<TripSchedule> Locations { get; set; }

        public TripStatus TripStatus { get; set; }
    }
}