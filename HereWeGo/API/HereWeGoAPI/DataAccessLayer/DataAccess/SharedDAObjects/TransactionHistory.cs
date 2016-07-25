//-----------------------------------------------------------------------
// <copyright file="TransactionHistory.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;

    [DAClass(Namespace = "TH", TableName = TableName.TransactionHistory)]
    [DataContract(Name = "TH", Namespace = "")]
    public class TransactionHistory
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "BR")]
        public string Bruid { get; set; }

        [DARowKey]
        [DataMember(Order = 1, Name = "TP")]
        public uint TransactionPageNumber { get; set; }

        [DataMember(Order = 2, Name = "T")]
        public IList<Transaction> Transactions { get; set; }

        [DataMember(Order = 3, Name = "L")]
        public DateTime LastUpdateDateTimeUtc { get; set; }

        public const uint TransactionsPerPage = 50;
    }
}
