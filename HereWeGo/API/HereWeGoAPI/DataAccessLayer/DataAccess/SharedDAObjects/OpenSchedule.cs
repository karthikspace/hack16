//-----------------------------------------------------------------------
// <copyright file="OpenSchedule.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Name = "OS", Namespace = "")]
    public class OpenSchedule
    {
        [DataMember(Order = 0, Name = "DW")]
        public IList<char> DayOfWeek { get; set; }

        [DataMember(Order = 1, Name = "OCT")]
        public IList<Tuple<DateTime, DateTime>> OpenCloseTimes { get; set; }
    }
}
