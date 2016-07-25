//-----------------------------------------------------------------------
// <copyright file="SearchRecord.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Microsoft.RewardsIntl.Platform.DataAccess.SharedDAObjects
{
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name = "SR", Namespace = "")]
    public class SearchRecord
    {
        [DataMember(Order = 0, Name = "B")]
        public string Bruid { get; set; }

        [DataMember(Order = 1, Name = "Q")]
        public string Query { get; set; }

        [DataMember(Order = 2, Name = "DT")]
        public DateTime DateTime { get; set; }
        
        [DataMember(Order = 3, Name = "DS")]
        public string Dataset { get; set; }

        [DataMember(Order = 4, Name = "OF")]
        public string OfferId { get; set; }

        [DataMember(Order = 5, Name = "AD")]
        public bool HasAdsTriggered { get; set; }

        [DataMember(Order = 6, Name = "IA")]
        public bool HasInstantAnswerTriggered { get; set; }

        [DataMember(Order = 7, Name = "WL")]
        public bool HasWebLinksTriggered { get; set; }

        [DataMember(Order = 8, Name = "TP")]
        public bool HasTaskPaneTriggered { get; set; }

        [DataMember(Order = 9, Name = "SQ")]
        public bool IsSpamQuery { get; set; }
    }
}