// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataAccessImplementation.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common.MediaDataAccessor;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common.MessageQueues;
    using Microsoft.RewardsIntl.Platform.DataAccess.MessageQueues;

    /// <summary>
    /// the default implementation of IDataAccess interface
    /// </summary>
    public class DataAccessImplementation : IDataAccess
    {
        /// <summary>
        /// instance of media data accessor
        /// </summary>
        private IMediaDataAccessor MediaDataAccessor;

        /// <summary>
        /// Name of the Media Storage
        /// </summary>
        private DataAccessAdapter Adapter;

        private ContinuationTokenInternal internalContinuationToken;

        /// <summary>
        /// The constructor of Data access object.
        /// Factory class will pass the media data accessor object
        /// </summary>
        /// <param name="mediaDataAccessor"></param>
        /// <param name="adapter"></param>
        internal DataAccessImplementation(IMediaDataAccessor mediaDataAccessor, DataAccessAdapter adapter)
        {
            this.MediaDataAccessor = mediaDataAccessor;
            this.Adapter = adapter;
            this.QueueAccess = new MessageQueueAccessImplementation(mediaDataAccessor);
        }

        #region Properties

        public IQueueAccess QueueAccess { get; set; }

        #endregion

        #region Synchronous Methods

        /// <summary>
        /// return objects base on the partition key and row key
        /// </summary>
        public T GetObject<T>(object partitionKey, object rowKey)
        {
            try
            {
                DAClassInfo classInfo = DAClassInfo.GetDAClassInfo<T>();

                if (null == partitionKey)
                    throw new Exception("partitionKey can't be null.");

                String internalRowKey = classInfo.GetInternalRowKeyFromRowKey(rowKey).ToUpper(CultureInfo.InvariantCulture);
                DataAccessObject daObject = MediaDataAccessor.GetObject(Convert.ToString(partitionKey, CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture), internalRowKey, classInfo);

                T obj;

                if (null == daObject)
                    obj = default(T);
                else
                    obj = daObject.Deserialize<T>();

                return obj;
            }
            catch (Exception ex)
            {
                Trace.TraceError("GetObject({0},{1}) failed with error:({2}).", partitionKey, rowKey, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// write object
        /// </summary>
        public void SetObject(object customerObject)
        {
            DataAccessObject daObject = null;
            try
            {
                if (null == customerObject)
                {
                    throw new Exception("customerObject can't be null.");
                }

                daObject = new DataAccessObject(customerObject);
                MediaDataAccessor.SetObject(daObject);
            }
            catch (Exception ex)
            {
                if (null != daObject)
                {
                    Trace.TraceError("SetObject ({0},{1},{2}) failed with error:{3}",
                        daObject.ClassInfo.Namespace,
                        daObject.PartitionKey,
                        daObject.RowKey,
                        ex.ToString());
                }
                else
                {
                    Trace.TraceError("SetObject failed with error:{0}", ex.ToString());
                }

                throw;
            }
        }

        /// <summary>
        /// Flush all pending changes into storage
        /// </summary>
        public void Flush()
        {
            MediaDataAccessor.Flush();
        }

        public IList<T> Query<T>(object startPartitionKey, object endPartitionKey, DateTime startDateTime, DateTime endDateTime, ref ContinuationToken continuationToken)
        {
            try
            {
                DAClassInfo classInfo = DAClassInfo.GetDAClassInfo<T>();

                if (null == startPartitionKey)
                    throw new Exception("startPartitionKey can't be null.");

                if (null == endPartitionKey)
                    throw new Exception("endPartitionKey can't be null.");

                if (String.Compare(startPartitionKey.ToString(), endPartitionKey.ToString(), StringComparison.Ordinal) > 0)
                    throw new Exception("endPartitionKey must be larger than or equal to startPartitionKey");

                if (startDateTime >= endDateTime)
                    throw new Exception("endDateTime must be larger than startDateTime");

                if (null == continuationToken)
                    this.internalContinuationToken = null;

                if (null != continuationToken)
                {
                    if (null == internalContinuationToken)
                        throw new Exception("Continuation token don't match with current session. ");

                    if (string.IsNullOrEmpty(continuationToken.token))
                        throw new Exception("Invalid token. ");

                    if (continuationToken.token != internalContinuationToken.TokenId)
                        throw new Exception("Continuation token don't match with current session. ");
                }

                IList<DataAccessObject> daObjList = MediaDataAccessor.Query(startPartitionKey.ToString().ToUpper(CultureInfo.InvariantCulture),
                                         endPartitionKey.ToString().ToUpper(CultureInfo.InvariantCulture),
                                         classInfo.Namespace.ToUpper(CultureInfo.InvariantCulture),
                                         startDateTime,
                                         endDateTime,
                                         classInfo,
                                         ref internalContinuationToken);

                IList<T> objList = new List<T>();
                foreach (DataAccessObject daObj in daObjList)
                {
                    objList.Add(daObj.Deserialize<T>());
                }

                if (null != internalContinuationToken)
                {
                    if (string.IsNullOrEmpty(internalContinuationToken.TokenId))
                        internalContinuationToken.TokenId = Guid.NewGuid().ToString().ToUpper(CultureInfo.InvariantCulture);

                    continuationToken = new ContinuationToken()
                    {
                        token = internalContinuationToken.TokenId
                    };
                }
                else
                {
                    continuationToken = null;
                }

                return objList;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Query changes failed with error:{0}", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Deletes the DA object.
        /// </summary>
        /// <param name="customObject">The object to be deleted (must be a type with DAClass
        /// attribute specified)</param>
        public void DeleteObject(object customObject)
        {
            DataAccessObject daObject = null;
            try
            {
                if (null == customObject)
                {
                    throw new Exception("customObject can't be null.");
                }

                daObject = new DataAccessObject(customObject);
                MediaDataAccessor.DeleteObject(daObject);
            }
            catch (Exception ex)
            {
                if (null != daObject)
                {
                    Trace.TraceInformation("DeleteObject ({0},{1},{2}) failed with error:{3}",
                        daObject.ClassInfo.Namespace,
                        daObject.PartitionKey,
                        daObject.RowKey,
                        ex.ToString());
                }
                else
                {
                    Trace.TraceError("DeleteObject failed with error:{0}", ex.ToString());
                }
                throw;
            }
        }
        #endregion
    }
}
