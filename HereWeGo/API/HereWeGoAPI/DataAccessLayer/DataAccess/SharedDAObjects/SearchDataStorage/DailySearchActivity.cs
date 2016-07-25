//-----------------------------------------------------------------------
// <copyright file="DailySearchActivity.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;

    [DAClass(Namespace = "DSA", TableName = TableName.DailySearchActivity)]
    [DataContract(Name = "DSA", Namespace = "")]
    public class DailySearchActivity
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "B")]
        public string Bruid { get; set; }

        [DARowKey]
        [DataMember(Order = 1, Name = "D")]
        public string Date { get; set; }

        [DataMember(Order = 2, Name = "SRL")]
        public IList<SearchRecord> SearchRecords { get; set; }
    }
}