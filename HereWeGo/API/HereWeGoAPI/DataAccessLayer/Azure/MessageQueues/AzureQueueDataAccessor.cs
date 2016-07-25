//-----------------------------------------------------------------------
// <copyright file="AzureQueueDataAccessor.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MMicrosoft.RewardsIntl.Platform.DataAccess.Azure.MessageQueues
{
    using System.Collections.Generic;
    using Microsoft.RewardsIntl.Platform.DataAccess.Azure.MessageQueues;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common.MediaDataAccessor;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common.MessageQueues;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class AzureQueueDataAccessor : IQueueMediaAccessor
    {
        #region Methods
        /// <summary>
        /// Sets one message for the specified object type T in the queue defined by the object type T
        /// </summary>
        public void SetQueueMessage(MessageQueueName queueName, QueueAccessObject queueAccessObject)
        {
            if (queueAccessObject == null || string.IsNullOrWhiteSpace(queueAccessObject.SerializedQueueMessage))
            {
                return;
            }

            AzureMessageQueueContext.SetMessage(queueName, queueAccessObject.SerializedQueueMessage);
        }

        /// <summary>
        /// Deletes one message for the specified object type T from the Queue
        /// </summary>
        public void DeleteQueueMessage(MessageQueueName queueName, QueueAccessObject queueAccessObject)
        {
            if (queueAccessObject == null ||
                string.IsNullOrWhiteSpace(queueAccessObject.MessageId) ||
                string.IsNullOrWhiteSpace(queueAccessObject.PopReceipt))
            {
                return;
            }

            AzureMessageQueueContext.DeleteMessage(queueName, queueAccessObject.MessageId, queueAccessObject.PopReceipt);
        }

        /// <summary>
        /// Gets one message for the specified object type T
        /// </summary>
        public QueueAccessObject PeekQueueMessage(MessageQueueName queueName)
        {
            var queueMessage = (CloudQueueMessage)AzureMessageQueueContext.PeekMessage(queueName);
            if (queueMessage == null)
            {
                return null;
            }

            return GetQueueAccessObject(queueMessage);
        }

        /// <summary>
        /// Gets one message for the specified object type T with updated visibility timeout
        /// </summary>
        /// <param name="queueName">Queue to retrieve data from</param>
        /// <param name="setVisibilityTimeOut">timeout in seconds</param>
        public QueueAccessObject GetQueueMessage(MessageQueueName queueName, int setVisibilityTimeOut)
        {
            var queueMessage = (CloudQueueMessage)AzureMessageQueueContext.GetMessage(queueName, setVisibilityTimeOut);
            if (queueMessage == null)
            {
                return null;
            }

            return GetQueueAccessObject(queueMessage);
        }

        /// <summary>
        /// Gets batch messages for the specified object type T and updates the visibilityTimeOut for the messages
        /// </summary>
        /// <param name="queueName">Queue to retrieve data from</param>
        /// <param name="messageCount">count of messages to be retrieved</param>
        /// <param name="setVisibilityTimeOut">time out in seconds</param>
        public IList<QueueAccessObject> GetBatchQueueMessages(MessageQueueName queueName, int messageCount, int setVisibilityTimeOut)
        {
            var queueMessages = (IList<CloudQueueMessage>)AzureMessageQueueContext.GetBatchMessages(queueName, messageCount, setVisibilityTimeOut);
            if (queueMessages == null || queueMessages.Count == 0)
            {
                return null;
            }

            var messages = new List<QueueAccessObject>();
            foreach (var queueMessage in queueMessages)
            {
                messages.Add(GetQueueAccessObject(queueMessage));
            }

            return messages;
        }

        /// <summary>
        /// Clears all the messages from the Queue of Type T messages
        /// </summary>
        public void ClearQueueMessages(MessageQueueName queueName)
        {
            AzureMessageQueueContext.ClearQueueMessages(queueName);
        }

        /// <summary>
        /// Convert Cloud Queue message to QueueAccessObject
        /// </summary>
        private static QueueAccessObject GetQueueAccessObject(CloudQueueMessage cloudQueueMessage)
        {
            return new QueueAccessObject()
            {
                MessageId = cloudQueueMessage.Id,
                PopReceipt = cloudQueueMessage.PopReceipt,
                SerializedQueueMessage = cloudQueueMessage.AsString
            };
        }
        #endregion
    }
}
