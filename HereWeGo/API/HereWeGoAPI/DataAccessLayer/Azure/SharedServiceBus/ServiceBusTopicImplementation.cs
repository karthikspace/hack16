//-----------------------------------------------------------------------
// <copyright file="ServiceBusTopicImplementation.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure.SharedServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.RewardsIntl.Platform.DataAccess.Azure.SharedServiceBus.SharedObjects;
    using Microsoft.ServiceBus.Messaging;

    public class ServiceBusTopicImplementation
    {
        private static IDictionary<string, IDictionary<string, SubscriptionClient>> topicSubscriptionClients = new Dictionary<string, IDictionary<string, SubscriptionClient>>();

        public ServiceBusMessage GetMessage(string topicName, string subscriptionKey)
        {
            if (string.IsNullOrWhiteSpace(topicName) || string.IsNullOrWhiteSpace(subscriptionKey))
            {
                Trace.TraceError("Topic Name / SubscriptionKey can't be null for reading message");
            }

            SubscriptionClient subscriptionClient = null;
            IDictionary<string, SubscriptionClient> topicSubscriptions = null;

            // If the topic itself doesn't exist add a new topic with new subscription for the key
            if (!topicSubscriptionClients.TryGetValue(topicName, out topicSubscriptions))
            {
                var newTopicSubscription = new Dictionary<string, SubscriptionClient>();
                subscriptionClient = SubscriptionClient.Create(topicName, subscriptionKey /*"FreeChargeCoupon20"*/, ReceiveMode.ReceiveAndDelete);
                newTopicSubscription[subscriptionKey] = subscriptionClient;
                topicSubscriptionClients[topicName] = newTopicSubscription;
            }
            else if (!topicSubscriptions.TryGetValue(subscriptionKey, out subscriptionClient))
            {
                // If Topic exists but subscription doesn't, add a new subscription
                subscriptionClient = SubscriptionClient.Create(topicName, subscriptionKey /*"FreeChargeCoupon20"*/, ReceiveMode.ReceiveAndDelete);
                topicSubscriptions[subscriptionKey] = subscriptionClient;
            }

            return ReceiveMessage(subscriptionClient);
        }

        private ServiceBusMessage ReceiveMessage(SubscriptionClient subscriptionClient)
        {
                //receive messages from Agent Subscription
                var brokeredMessage = subscriptionClient.Receive(TimeSpan.FromSeconds(0));
                if (brokeredMessage != null)
                {
                    return new ServiceBusMessage()
                    {
                        MessageId = brokeredMessage.MessageId,
                        MessageProperties = brokeredMessage.Properties
                    };
                }

            return null;
        }
    }
}
