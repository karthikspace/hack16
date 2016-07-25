//-----------------------------------------------------------------------
// <copyright file="RedemptionHistory.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;

    [DAClass(Namespace = "RH", TableName = TableName.RedemptionHistory)]
    [DataContract(Name = "RH", Namespace = "")]
    public class RedemptionHistory
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "BR")]
        public string Bruid { get; set; }

        [DARowKey]
        [DataMember(Order = 1, Name = "RP")]
        public uint RedemptionPageNumber { get; set; }

        [DataMember(Order = 2, Name = "R")]
        public IList<Redemption> Redemptions { get; set; }

        public const uint RedemptionPageSize = 10;
    }
}
