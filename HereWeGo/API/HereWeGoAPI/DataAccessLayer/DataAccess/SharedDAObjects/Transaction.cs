
namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Name = "T", Namespace = "")]
    public class Transaction
    {
        [DataMember(Order = 0, Name = "TT")]
        public TransactionType TransactionType { get; set; }

        [DataMember(Order = 1, Name = "TD")]
        public DateTime TransactionDate { get; set; }

        [DataMember(Order = 2, Name = "TV")]
        public uint TransactionValue { get; set; }

        [DataMember(Order = 3, Name = "TA")]
        public IDictionary<AttributeKeys, string> TransactionAttributes { get; set; }
    }

    public enum TransactionType
    {
        SignUp,
        Earning,
        Redemption,
        AccountManagement
    }

    public enum AttributeKeys
    {
        SmsCount,
        EmailCount
    }
}
