
namespace Microsoft.RewardsIntl.Platform.DataAccess.Common.MediaDataAccessor
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// this is the adaptor interface for different implementation 
    /// of storages like memory, Azure
    /// </summary>
    public interface IMediaDataAccessor
    {
        /// <summary>
        /// Accessor for Messages queues related media
        /// </summary>
        IQueueMediaAccessor QueueMediaAccessor { get; set; }

        #region Methods

        /// <summary>
        /// return the object which partition key and row key
        /// in backend storage
        /// </summary>
        DataAccessObject GetObject(String partitionKey, String rowKey, DAClassInfo classInfo);

        /// <summary>
        /// Write a DAObject into storage
        /// </summary>
        void SetObject(DataAccessObject daObject);

        /// <summary>
        /// Flush all pending changes into storage
        /// </summary>
        void Flush();

        /// <summary>
        /// this method will query changed data based on
        /// the partitionkey range and date range.
        /// </summary>
        /// <returns></returns>
        IList<DataAccessObject> Query(String startPartitionKey,
                          String endPartitionKey,
                          String nameSpace,
                          DateTime startDateTime,
                          DateTime endDateTime,
                          DAClassInfo classInfo,
                          ref ContinuationTokenInternal continuationToken);

        /// <summary>
        /// Delete the DA object.
        /// </summary>
        void DeleteObject(DataAccessObject daObject);
        #endregion
    }
}
