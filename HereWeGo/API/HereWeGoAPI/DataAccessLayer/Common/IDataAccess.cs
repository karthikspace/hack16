// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDataAccess.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    using System;
    using System.Collections.Generic;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common.MessageQueues;

    public interface IDataAccess
    {
        #region Properties
        /// <summary>
        /// Accessor for Queue operations via DataAccess Layer
        /// </summary>
        IQueueAccess QueueAccess { get; set; }
        #endregion

        #region Methods

        /// <summary>
        /// return da object by partition key and row key.
        /// </summary>
        /// <typeparam name="T">type with DAClass attribute specified</typeparam>
        /// <param name="partitionKey">the partition key of the objects, can't be null</param>
        /// <param name="rowKey">the row key of the objects, can be null</param>
        /// <returns></returns>
        T GetObject<T>(object partitionKey, object rowKey);

        /// <summary>
        /// write daObject to session, waiting for Flush call.
        /// </summary>
        /// <param name="customerObject">the object that will be wrote, 
        /// must be a type with DAClass attribute specified.</param>
        void SetObject(object customerObject);

        /// <summary>
        /// Flush all pending write into backend storage
        /// </summary>
        void Flush();

        /// <summary>
        /// this method will query changed data based on
        /// the partitionkey range and date range.
        /// </summary>
        /// <returns></returns>
        IList<T> Query<T>(object startPartitionKey,
                          object endPartitionKey,
                          DateTime startDateTime,
                          DateTime endDateTime,
                          ref ContinuationToken continuationToken);

        /// <summary>
        /// Deletes the DA object.
        /// </summary>
        /// <param name="customObject">The object to be deleted (must be a type with DAClass
        /// attribute specified)</param>
        void DeleteObject(object customObject);

        #endregion
    }

    /// <summary>
    /// Continuation token for azure multiple page fetch
    /// </summary>
    public class ContinuationToken
    {
        public string token;
    }

    public class KeyInfo
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
    }
}
