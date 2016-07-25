//-----------------------------------------------------------------------
// <copyright file="PhoneIndex.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;

    [DAClass(Namespace = "PI", TableName = TableName.PhoneIndex)]
    [DataContract(Name = "PI", Namespace = "")]
    public class PhoneIndex
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "PN")]
        public string PhoneNumber { get; set; }

        [DataMember(Order = 1, Name = "P")]
        public HashSet<string> PuidSet { get; set; }

        [DataMember(Order = 2, Name = "L")]
        public DateTime LastUpdateDateTimeUtc { get; set; }
    }
}
