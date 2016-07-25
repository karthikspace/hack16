namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System.Runtime.Serialization;

    [DataContract(Name = "UB", Namespace = "")]
    public class UserBalance
    {
        [DataMember(Order = 1, Name = "AB")]
        public uint AvailableBalance { get; set; }

        [DataMember(Order = 2, Name = "RB")]
        public uint RedeemedBalance { get; set; }
    }
}
