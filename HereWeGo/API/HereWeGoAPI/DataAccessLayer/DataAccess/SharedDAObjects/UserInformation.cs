//-----------------------------------------------------------------------
// <copyright file="UserInformation.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;

    [DAClass(Namespace = "UI", TableName = TableName.UserInformation)]
    [DataContract(Name = "UI", Namespace = "")]
    public class UserInformation
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "ID")]
        public string UserId { get; set; }

        [DataMember(Order = 1, Name = "FN")]
        public string FirstName { get; set; }

        [DataMember(Order = 2, Name = "LN")]
        public string LastName { get; set; }

        [DataMember(Order = 3, Name = "TS")]
        public IList<string> Trips { get; set; }

        [DataMember(Order = 4, Name = "I")]
        public string ImageUrl { get; set; }

        [DataMember(Order = 5, Name = "E")]
        public string Email { get; set; }
    }
}
