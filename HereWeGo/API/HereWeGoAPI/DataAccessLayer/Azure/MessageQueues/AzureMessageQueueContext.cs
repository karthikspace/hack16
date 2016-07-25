//-----------------------------------------------------------------------
// <copyright file="AzureMessageQueueContext.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure.MessageQueues
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class AzureMessageQueueContext
    {
        private static bool isInitialized = false;
        private static object initializedLock = new object();

        private static Dictionary<string, CloudQueue> CloudQueues = new Dictionary<string, CloudQueue>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes the user data queue for all the processed information from the offline process to evaluate user queries
        /// </summary>
        static AzureMessageQueueContext()
        {
            if (!isInitialized)
            {
                lock (initializedLock)
                {
                    if (!isInitialized)
                    {
                        var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
                        var queueClient = account.CreateCloudQueueClient();

                        // Create all registered queues
                        foreach (MessageQueueName queueName in Enum.GetValues(typeof(MessageQueueName)))
                        {
                            var queueNameString = queueName.ToString();
                            var cloudQueue = queueClient.GetQueueReference(queueNameString.ToLower(CultureInfo.InvariantCulture));
                            cloudQueue.CreateIfNotExists();
                            CloudQueues[queueNameString] = cloudQueue;
                        }

                        isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the message in the queue mentioned
        /// </summary>
        public static void SetMessage(MessageQueueName queueName, string message)
        {
            var cloudQueue = CloudQueues[queueName.ToString()];
            if (cloudQueue == null)
            {
                throw new Exception(queueName.ToString() + " is not a valid queue");
            }

            var cloudMessage = new CloudQueueMessage(message);
            cloudQueue.AddMessage(cloudMessage);
        }

        /// <summary>
        /// Fetches a single record from the queue mentioned
        /// setVisibilityTimeOut can be provided by the consumer to add more time to message invisibility in the queue once popped
        /// </summary>
        public static object GetMessage(MessageQueueName queueName, int setVisibilityTimeOut = 30)
        {
            var cloudQueue = CloudQueues[queueName.ToString()];
            if (cloudQueue == null)
            {
                return null;
            }

            var cloudMessage = setVisibilityTimeOut == 30 ?
                cloudQueue.GetMessage() :
                cloudQueue.GetMessage(new TimeSpan(0, 0, setVisibilityTimeOut));

            if (cloudMessage == null || string.IsNullOrWhiteSpace(cloudMessage.AsString))
            {
                return null;
            }

            return cloudMessage;
        }

        /// <summary>
        /// Fetches a batch of records from the queue
        /// </summary>
        public static object GetBatchMessages(MessageQueueName queueName, int messageCount, int setVisibilityTimeOut = 30)
        {
            var cloudQueue = CloudQueues[queueName.ToString()];
            if (cloudQueue == null)
            {
                return null;
            }

            // Minimum messages to be fetched are 1
            if (messageCount <= 0)
            {
                messageCount = 1;
            }

            // Maximum messages fetched are 32
            if (messageCount > 32)
            {
                messageCount = 32;
            }

            var cloudQueueMessages = setVisibilityTimeOut == 30 ?
                cloudQueue.GetMessages(messageCount) :
                cloudQueue.GetMessages(messageCount, new TimeSpan(0, 0, setVisibilityTimeOut));
            if (cloudQueueMessages == null || !cloudQueueMessages.Any())
            {
                return null;
            }

            var cloudMessages = new List<CloudQueueMessage>();
            foreach (var cloudQueueMessage in cloudQueueMessages)
            {
                if (cloudQueueMessage == null || string.IsNullOrWhiteSpace(cloudQueueMessage.AsString))
                {
                    continue;
                }

                cloudMessages.Add(cloudQueueMessage);
            }

            return cloudMessages;
        }

        /// <summary>
        /// Delete message from the queue based on the ID and Pop Receipt generated by GetMessage call
        /// Pop Receipt needs to be valid, otherwise it will throw PopReceiptMisMatch Error
        /// </summary>
        public static void DeleteMessage(MessageQueueName queueName, string messageId, string popReceipt)
        {
            var cloudQueue = CloudQueues[queueName.ToString()];
            if (cloudQueue == null)
            {
                throw new Exception(queueName.ToString() + " is not a valid queue");
            }

            // Catch the exceptions
            cloudQueue.DeleteMessage(messageId, popReceipt);
        }

        /// <summary>
        /// Peeks the message at the front of the queue
        /// Pop Receipt is not generated for Peeked message hence these can't be deleted
        /// </summary>
        public static object PeekMessage(MessageQueueName queueName)
        {
            var cloudQueue = CloudQueues[queueName.ToString()];
            if (cloudQueue == null)
            {
                return null;
            }

            var cloudMessage = cloudQueue.PeekMessage();
            if (cloudMessage == null || string.IsNullOrWhiteSpace(cloudMessage.AsString))
            {
                return null;
            }

            return cloudMessage;
        }

        /// <summary>
        /// Clears all the messages from the queue mentioned
        /// </summary>
        public static void ClearQueueMessages(MessageQueueName queueName)
        {
            var cloudQueue = CloudQueues[queueName.ToString()];
            if (cloudQueue == null)
            {
                return;
            }

            cloudQueue.Clear();
        }
    }
}
