//-----------------------------------------------------------------------
// <copyright file="UserInformation.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;

    [DAClass(Namespace = "TI", TableName = TableName.TripInformation)]
    [DataContract(Name = "TI", Namespace = "")]
    public class TripInformation
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "TID")]
        public string TripId { get; set; }

        [DataMember(Order = 1, Name = "TD")]
        public string DestinationId { get; set; }

        [DataMember(Order = 2, Name = "DI")]
        public string DestinationImage { get; set; }

        [DataMember(Order = 3, Name = "SD")]
        public DateTime StartDateUTC { get; set; }

        [DataMember(Order = 4, Name = "ED")]
        public DateTime EndDateUTC { get; set; }

        /// <summary>
        /// List of all the locations included in the trip
        /// </summary>
        [DataMember(Order = 5, Name = "LS")]
        public IList<string> Locations { get; set; }
    }
}
