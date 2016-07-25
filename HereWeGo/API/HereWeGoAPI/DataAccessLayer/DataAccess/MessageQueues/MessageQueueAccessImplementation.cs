//-----------------------------------------------------------------------
// <copyright file="MessageQueueAccessImplementation.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.MessageQueues
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common.MediaDataAccessor;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common.MessageQueues;

    public class MessageQueueAccessImplementation : IQueueAccess
    {
        private const string PopReceiptPropertyName = "PopReceipt";
        private const string MessageIdPropertyName = "MessageId";

        private IMediaDataAccessor mediaDataAccessor;

        public MessageQueueAccessImplementation(IMediaDataAccessor mediaDataAccessor)
        {
            this.mediaDataAccessor = mediaDataAccessor;
        }

        #region properties
        /// <summary>
        /// allow user to choose serializer
        /// </summary>
        public SerializerType SerializerType { get; private set; }

        /// <summary>
        /// The table name this object need to be stored
        /// used only in Azure today
        /// </summary>
        public MessageQueueName QueueName { get; private set; }
        #endregion

        #region IQueueAccess 
        /// <summary>
        /// Sets one message for the specified object type T in the queue defined by the object type T
        /// </summary>
        public void SetQueueMessage(object customerObject)
        {
            Type customerObjectType = customerObject.GetType();
            this.ExtractRequestInfo(customerObjectType);

            // Serialize the data before setting it in queue
            var serializer = DaSerializerFactory.CreateSerializer(customerObjectType, this.SerializerType);
            var serializedString = serializer.Serialize(customerObject);

            var queueAccessObject = new QueueAccessObject()
            {
                MessageId = string.Empty,
                PopReceipt = string.Empty,
                SerializedQueueMessage = serializedString
            };

            this.mediaDataAccessor.QueueMediaAccessor.SetQueueMessage(this.QueueName, queueAccessObject);
        }

        /// <summary>
        /// Deletes one message for the specified object type T from the Queue
        /// </summary>
        public void DeleteQueueMessage<T>(string messageId, string popReceipt) where T : IQueueData
        {
            var customerObjectType = typeof(T);
            this.ExtractRequestInfo(customerObjectType);
            var queueAccessObject = new QueueAccessObject()
            {
                MessageId = messageId,
                PopReceipt = popReceipt,
                SerializedQueueMessage = string.Empty
            };

            this.mediaDataAccessor.QueueMediaAccessor.DeleteQueueMessage(this.QueueName, queueAccessObject);
        }

        /// <summary>
        /// Gets one message for the specified object type T
        /// </summary>
        public T PeekQueueMessage<T>() where T : IQueueData
        {
            var customerObjectType = typeof(T);
            this.ExtractRequestInfo(customerObjectType);

            var queueMessage = this.mediaDataAccessor.QueueMediaAccessor.PeekQueueMessage(this.QueueName);
            if (queueMessage == null)
            {
                return default(T);
            }

            var serializer = DaSerializerFactory.CreateSerializer(customerObjectType, this.SerializerType);
            var customerObject = serializer.Deserialize(queueMessage.SerializedQueueMessage);

            return (T)SetProperties(customerObject, GeneratePropertyKeyValuePairs(queueMessage));
        }
        
        /// <summary>
        /// Gets one message for the specified object type T
        /// </summary>
        public T GetQueueMessage<T>() where T : IQueueData
        {
            return this.GetQueueMessage<T>(30);
        }

        /// <summary>
        /// Gets one message for the specified object type T with updated visibility timeout
        /// </summary>
        /// <param name="setVisibilityTimeOut">timeout in seconds</param>
        public T GetQueueMessage<T>(int setVisibilityTimeOut) where T : IQueueData
        {
            var customerObjectType = typeof(T);
            this.ExtractRequestInfo(customerObjectType);
            var queueMessage = this.mediaDataAccessor.QueueMediaAccessor.GetQueueMessage(this.QueueName, setVisibilityTimeOut);
            if (queueMessage == null)
            {
                return default(T);
            }

            var serializer = DaSerializerFactory.CreateSerializer(customerObjectType, this.SerializerType);
            var customerObject = serializer.Deserialize(queueMessage.SerializedQueueMessage);

            return (T)SetProperties(customerObject, GeneratePropertyKeyValuePairs(queueMessage));
        }
        
        /// <summary>
        /// Gets batch messages for the specified object type T.
        /// </summary>
        public IList<T> GetBatchQueueMessages<T>(int messageCount) where T : IQueueData
        {
            return this.GetBatchQueueMessages<T>(messageCount, 30);
        }

        /// <summary>
        /// Gets batch messages for the specified object type T and updates the visibilityTimeOut for the messages
        /// </summary>
        /// <param name="messageCount">count of messages to be retrieved</param>
        /// <param name="setVisibilityTimeOut">time out in seconds</param>
        public IList<T> GetBatchQueueMessages<T>(int messageCount, int setVisibilityTimeOut) where T : IQueueData
        {
            Type customerObjectType = typeof(T);
            this.ExtractRequestInfo(customerObjectType);
            var queueMessages = this.mediaDataAccessor.QueueMediaAccessor.GetBatchQueueMessages(this.QueueName, messageCount, setVisibilityTimeOut);
            if (queueMessages == null || queueMessages.Count == 0)
            {
                return default(IList<T>);
            }

            var serializer = DaSerializerFactory.CreateSerializer(customerObjectType, this.SerializerType);
            var messages = new List<T>();
            foreach (var queueMessage in queueMessages)
            {
                var message = serializer.Deserialize(queueMessage.SerializedQueueMessage);
                var updatedMessage = (T)SetProperties(message, GeneratePropertyKeyValuePairs(queueMessage));
                messages.Add(updatedMessage);
            }

            return messages;
        }

        /// <summary>
        /// Clears all the messages from the Queue of Type T messages
        /// </summary>
        public void ClearQueueMessages<T>() where T : IQueueData
        {
            this.ExtractRequestInfo(typeof(T));
            this.mediaDataAccessor.QueueMediaAccessor.ClearQueueMessages(this.QueueName);
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Sets the specific property of the object and returns the updated object
        /// </summary>
        private static object SetPropertyValue(object customerObject, string propertyName, object propertyValue)
        {
            var property = customerObject.GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(customerObject, propertyValue);
            }

            return customerObject;
        }

        /// <summary>
        /// Sets the list of properties for the object and returns the updated object
        /// </summary>
        private static object SetProperties(object customerObject, IEnumerable<KeyValuePair<string, object>> properiesInfo)
        {
            foreach (var propertyInfo in properiesInfo)
            {
                customerObject = SetPropertyValue(customerObject, propertyInfo.Key, propertyInfo.Value);
            }

            return customerObject;
        }

        /// <summary>
        /// Generate a key value paired list for properties to be added in respective object
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static IEnumerable<KeyValuePair<string, object>> GeneratePropertyKeyValuePairs(QueueAccessObject message)
        {
            return new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>(MessageIdPropertyName, message.MessageId),
                new KeyValuePair<string, object>(PopReceiptPropertyName, message.PopReceipt)
            };

        }

        /// <summary>
        /// Extract the class information related to the requested object type
        /// </summary>
        private void ExtractRequestInfo(Type customerObjectType)
        {
            object[] attrs = customerObjectType.GetCustomAttributes(typeof(DAClassAttribute), true);
            if (null != attrs && attrs.Length != 1)
            {
                throw new Exception("You must specify 1 and only 1 MQClassAttribute in your custom object.");
            }

            var classAttr = attrs[0] as DAClassAttribute;
            if (classAttr == null)
            {
                throw new Exception("You must specify " + customerObjectType.ToString() + " as MQClass Attribute.");
            }

            Trace.TraceInformation("Namespace:{0}, QueueName:{1} ", classAttr.Namespace, classAttr.QueueName);

            this.SerializerType = SerializerType.DataContractorSerializer;
            this.QueueName = classAttr.QueueName;
        }

        #endregion
    }
}
