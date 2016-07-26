//-----------------------------------------------------------------------
// <copyright file="LocationInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;

    [DAClass(Namespace = "LI", TableName = TableName.LocationInfo)]
    [DataContract(Name = "LI", Namespace = "")]
    public class LocationInfo
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "ID")]
        public string Id { get; set; }

        [DataMember(Order = 1, Name = "N")]
        public string Name { get; set; }

        [DataMember(Order = 2, Name = "I")]
        public IList<string> Images { get; set; }

        [DataMember(Order = 3, Name = "C")]
        public string City { get; set; }

        [DataMember(Order = 4, Name = "CN")]
        public string Country{ get; set; }

        [DataMember(Order = 5, Name = "S")]
        public string Summary { get; set; }

        [DataMember(Order = 6, Name = "A")]
        public string Address { get; set; }

        [DataMember(Order = 7, Name = "PH")]
        public string ContactNumber { get; set; }

        [DataMember(Order = 8, Name = "WU")]
        public string WebsiteUrl { get; set; }

        [DataMember(Order = 9, Name = "OSD")]
        public IDictionary<string, IList<Tuple<DateTime, DateTime>>> OpenSchedule { get; set; }

        [DataMember(Order = 10, Name = "RS")]
        public IList<Review> Reviews { get; set; }

        [DataMember(Order = 11, Name = "CY")]
        public Category Category { get; set; }

        [DataMember(Order = 12, Name = "AR")]
        public float AverageRating { get; set; }

        [DataMember(Order = 13, Name = "LT")]
        public string Latitude { get; set; }

        [DataMember(Order = 14, Name = "LD")]
        public string Longitude { get; set; }

        [DataMember(Order = 15, Name = "DN")]
        public int DurationToVisit { get; set; }
    }

    public enum Category
    {
        Entertainment,
        Religious
    }
}
