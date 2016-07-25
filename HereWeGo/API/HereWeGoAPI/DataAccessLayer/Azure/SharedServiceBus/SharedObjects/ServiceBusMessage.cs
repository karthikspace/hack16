//-----------------------------------------------------------------------
// <copyright file="ServiceBusMessage.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure.SharedServiceBus.SharedObjects
{
    using System.Collections.Generic;

    public class ServiceBusMessage
    {
        // Message Id will correspond to Coupon Code in out usage for now
        public string MessageId { get; set; }

        // Properties for the message
        public IDictionary<string, object> MessageProperties { get; set; }
    }
}
