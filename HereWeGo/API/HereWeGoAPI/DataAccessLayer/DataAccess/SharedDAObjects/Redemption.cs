//-----------------------------------------------------------------------
// <copyright file="Redemption.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Runtime.Serialization;
    
    [DataContract(Name = "R", Namespace = "")]
    public class Redemption
    {
        [DataMember(Order = 0, Name = "RD")]
        public DateTime RedemptionDateTimeUtc { get; set; }

        [DataMember(Order = 1, Name = "CR")]
        public uint Credits { get; set; }

        [DataMember(Order = 2, Name = "S")]
        public string SkuId { get; set; }

        [DataMember(Order = 3, Name = "C")]
        public string CouponId { get; set; }

        [DataMember(Order = 4, Name = "O")]
        public string OrderId { get; set; }

        [DataMember(Order = 7, Name = "P")]
        public string Provider { get; set; }

        [DataMember(Order = 8, Name = "N")]
        public string Name { get; set; }
    }
}
