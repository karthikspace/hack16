namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Runtime.Serialization;
    
    [DataContract(Name = "OI", Namespace = "")]
    public class OfferInfo
    {
        [DataMember(Order = 0, Name = "P")]
        public uint CurrentProgress { get; set; }

        [DataMember(Order = 1, Name = "L")]
        public DateTime LastUpdatedDateTimeClientTimeZone { get; set; }
    }
}
