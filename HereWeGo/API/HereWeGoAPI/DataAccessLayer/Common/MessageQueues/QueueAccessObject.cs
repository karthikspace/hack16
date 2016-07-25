//-----------------------------------------------------------------------
// <copyright file="QueueAccessObject.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common.MessageQueues
{
    public class QueueAccessObject : IQueueData
    {
        public string SerializedQueueMessage { get; set; }

        public string MessageId { get; set; }

        public string PopReceipt { get; set; }
    }
}
