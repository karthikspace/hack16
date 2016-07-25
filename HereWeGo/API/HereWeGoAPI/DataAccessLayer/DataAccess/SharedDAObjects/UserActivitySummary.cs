//-----------------------------------------------------------------------
// <copyright file="UserActivitySummary.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;
    
    [DAClass(Namespace = "UA", TableName = TableName.UserActivitySummary)]
    [DataContract(Name = "UA", Namespace = "")]
    public class UserActivitySummary
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "BR")]
        public string Bruid { get; set; }

        [DataMember(Order = 1, Name = "UB")]
        public UserBalance Balance { get; set; }

        [DataMember(Order = 2, Name = "P")]
        public IDictionary<string, OfferInfo> OfferProgress { get; set; }

        // We store the hashcode of the last five queries
        [DataMember(Order = 3, Name = "Q")]
        public IList<int> LastFiveQueries { get; set; }

        [DataMember(Order = 4, Name = "LT")]
        public uint LastTransactionPageNumber { get; set; }

        [DataMember(Order = 5, Name = "LR")]
        public uint LastRedemptionPageNumber { get; set; }

        [DataMember(Order = 6, Name = "L")]
        public DateTime LastUpdateDateTimeUtc { get; set; }

        [DataMember(Order = 7, Name="RIG")]
        public IList<KeyValuePair<string, uint>> RecentImpressionGuids { get; set; }
    }
}
