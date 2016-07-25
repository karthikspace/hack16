//-----------------------------------------------------------------------
// <copyright file="Destination.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;

    [DAClass(Namespace = "D", TableName = TableName.Destination)]
    [DataContract(Name = "D", Namespace = "")]
    public class Destination
    {
        [DAPartitionKey]
        [DataMember(Order = 0, Name = "ID")]
        public string Id { get; set; }

        [DataMember(Order = 1, Name = "N")]
        public string Name { get; set; }

        [DataMember(Order = 2, Name = "I")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// List of all the locations associated with a destination
        /// </summary>
        [DataMember(Order = 5, Name = "LS")]
        public IList<string> Locations { get; set; }
    }
}
