//-----------------------------------------------------------------------
// <copyright file="TripSchedule.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Name = "TS", Namespace = "")]
    public class TripSchedule
    {
        [DataMember(Order = 0, Name = "S")]
        public DateTime Start { get; set; }

        [DataMember(Order = 1, Name = "E")]
        public DateTime End { get; set; }

        [DataMember(Order = 2, Name = "L")]
        public string LocationId { get; set; }
    }
}
