//-----------------------------------------------------------------------
// <copyright file="IQueueMediaAccessor.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common.MediaDataAccessor
{
    using System.Collections.Generic;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common.MessageQueues;

    public interface IQueueMediaAccessor
    {
        /// <summary>
        /// Sets one message for the specified object type T in the queue defined by the object type T
        /// </summary>
        void SetQueueMessage(MessageQueueName queueName, QueueAccessObject queueAccessObject);

        /// <summary>
        /// Deletes one message for the specified object type T from the Queue
        /// </summary>
        void DeleteQueueMessage(MessageQueueName queueName, QueueAccessObject queueAccessObject);

        /// <summary>
        /// Gets one message for the specified object type T
        /// </summary>
        QueueAccessObject PeekQueueMessage(MessageQueueName queueName);

        /// <summary>
        /// Gets one message for the specified object type T with updated visibility timeout
        /// </summary>
        /// <param name="queueName">Queue to retrieve data from</param>
        /// <param name="setVisibilityTimeOut">timeout in seconds</param>
        QueueAccessObject GetQueueMessage(MessageQueueName queueName, int setVisibilityTimeOut);

        /// <summary>
        /// Gets batch messages for the specified object type T and updates the visibilityTimeOut for the messages
        /// </summary>
        /// <param name="queueName">Queue to retrieve data from</param>
        /// <param name="messageCount">count of messages to be retrieved</param>
        /// <param name="setVisibilityTimeOut">time out in seconds</param>
        IList<QueueAccessObject> GetBatchQueueMessages(MessageQueueName queueName, int messageCount, int setVisibilityTimeOut);

        /// <summary>
        /// Clears all the messages from the Queue of Type T messages
        /// </summary>
        void ClearQueueMessages(MessageQueueName queueName);
    }
}
