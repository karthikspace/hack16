//-----------------------------------------------------------------------
// <copyright file="PhoneOtp.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name = "PO", Namespace = "")]
    public class PhoneOtp
    {
        [DataMember(Order = 0, Name = "O")]
        public string Code { get; set; }

        [DataMember(Order = 1, Name = "ST")]
        public DateTime SentDateTimeUtc { get; set; }

        [DataMember(Order = 2, Name = "P")]
        public string ContactNumber { get; set; }
    }
}
