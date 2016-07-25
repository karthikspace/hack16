//-----------------------------------------------------------------------
// <copyright file="BruidIndex.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;

    [DAClass(Namespace = "BI", TableName = TableName.BruidIndex)]
    [DataContract(Name = "BI", Namespace = "")]
    public class BruidIndex
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "B")]
        public string Bruid { get; set; }

        [DataMember(Order = 1, Name = "P")]
        public string Puid { get; set; }

        [DataMember(Order = 2, Name = "L")]
        public DateTime LastUpdateDateTimeUtc { get; set; }
    }
}
