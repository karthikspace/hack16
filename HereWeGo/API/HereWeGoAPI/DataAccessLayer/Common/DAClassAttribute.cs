using System;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    /// <summary>
    /// Specify that the class can be serialized and deserialized
    /// by DA layer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DAClassAttribute : DataAccessAttribute
    {
        /// <summary>
        /// The namespace of current class, will be concatenated into 
        /// row key.
        /// </summary>
        public String Namespace { get; set; }

        /// <summary>
        /// The table name this object need to be stored
        /// used only in Azure today
        /// </summary>
        public TableName TableName { get; set; }

        /// <summary>
        /// The queue name this object need to be stored
        /// </summary>
        public MessageQueueName QueueName { get; set; }
    }
}
