// ----------------------------------------------------------------------------------------
// <copyright file="IQueueAccess.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// ----------------------------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common.MessageQueues
{
    using System.Collections.Generic;

    public interface IQueueAccess
    {
        #region Methods

        /// <summary>
        /// Sets one message for the specified object type T in the queue defined by the object type T
        /// </summary>
        void SetQueueMessage(object customerObject);

        /// <summary>
        /// Deletes one message for the specified object type T from the Queue
        /// </summary>
        void DeleteQueueMessage<T>(string messageId, string popReceipt) where T: IQueueData;

        /// <summary>
        /// Gets one message for the specified object type T
        /// </summary>
        T PeekQueueMessage<T>() where T : IQueueData;

        /// <summary>
        /// Gets one message for the specified object type T
        /// </summary>
        T GetQueueMessage<T>() where T : IQueueData;

        /// <summary>
        /// Gets one message for the specified object type T with updated visibility timeout
        /// </summary>
        /// <param name="setVisibilityTimeOut">timeout in seconds</param>
        T GetQueueMessage<T>(int setVisibilityTimeOut) where T : IQueueData;

        /// <summary>
        /// Gets batch messages for the specified object type T.
        /// </summary>
        IList<T> GetBatchQueueMessages<T>(int messageCount) where T : IQueueData;

        /// <summary>
        /// Gets batch messages for the specified object type T and updates the visibilityTimeOut for the messages
        /// </summary>
        /// <param name="messageCount">count of messages to be retrieved</param>
        /// <param name="setVisibilityTimeOut">time out in seconds</param>
        IList<T> GetBatchQueueMessages<T>(int messageCount, int setVisibilityTimeOut) where T : IQueueData;

        /// <summary>
        /// Clears all the messages from the Queue of Type T messages
        /// </summary>
        void ClearQueueMessages<T>() where T: IQueueData;
        #endregion

    }
}
