//-----------------------------------------------------------------------
// <copyright file="UserProfile.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;
    using System;
    using System.Runtime.Serialization;

    public enum UserStatus
    {
        Active = 0,
        Optout = 1,
        Suspended = 2
    }

    [DAClass(Namespace = "UP", TableName = TableName.UserProfile)]
    [DataContract(Name = "UP", Namespace = "")]
    public class UserProfile
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "P")]
        public string Puid { get; set; }

        [DataMember(Order = 1, Name = "L")]
        public DateTime LastUpdateDateTimeUtc { get; set; }

        [DataMember(Order = 2, Name = "FN")]
        public string FirstName { get; set; }
        
        [DataMember(Order = 3, Name = "LN")]
        public string LastName { get; set; }
        
        [DataMember(Order = 4, Name = "CE")]
        public string ContactEmail { get; set; }
        
        [DataMember(Order = 5, Name = "PN")]
        public string PhoneNumber { get; set; }

        [DataMember(Order = 6, Name = "C")]
        public DateTime CreationionDateTimeUtc { get; set; }

        [DataMember(Order = 7, Name = "S")]
        public UserStatus Status { get; set; }

        [DataMember(Order = 8, Name = "B")]
        public string Bruid { get; set; }

        [DataMember(Order = 9, Name = "PO")]
        public PhoneOtp OtpDetails { get; set; }

        [DataMember(Order = 10, Name = "MS")]
        public bool MessageSubscriptionEnabled { get; set; }

        [DataMember(Order = 11, Name = "PA")]
        public bool IsProfileNotAccessed { get; set; }
    }
}
