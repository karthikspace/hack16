//-----------------------------------------------------------------------
// <copyright file="Review.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name = "R", Namespace = "")]
    public class Review
    {
        [DataMember(Order = 0, Name = "U")]
        public string UserId { get; set; }

        [DataMember(Order = 1, Name = "ST")]
        public string Statement { get; set; }

        [DataMember(Order = 2, Name = "DT")]
        public DateTime Date { get; set; }

        [DataMember(Order = 3, Name = "RT")]
        public float Rating { get; set; }
    }
}
