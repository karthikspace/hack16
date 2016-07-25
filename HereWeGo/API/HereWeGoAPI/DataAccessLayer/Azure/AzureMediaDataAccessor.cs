using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.RewardsIntl.Platform.DataAccess.Common;
using Microsoft.RewardsIntl.Platform.DataAccess.Common.MediaDataAccessor;
using MMicrosoft.RewardsIntl.Platform.DataAccess.Azure.MessageQueues;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure
{

    /// <summary>
    /// this is the key object used in SortedDictionary  for index and sort purpose
    /// to effectively retrieve da objects from memory
    /// </summary>
    public class PartitionKeyAndRowKey : IComparable<PartitionKeyAndRowKey>
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        internal PartitionKeyAndRowKey(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        /// <summary>
        /// concatecate the partition key and row key then caculate the hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (this.PartitionKey + this.RowKey).GetHashCode();
        }

        #region IComparable<PartitionKeyAndRowKey> Members

        /// <summary>
        /// Compare the partition key then row key
        /// </summary>
        /// <returns></returns>
        public int CompareTo(PartitionKeyAndRowKey other)
        {
            int i = string.CompareOrdinal(this.PartitionKey, other.PartitionKey);

            if (i == 0)
            {
                return string.CompareOrdinal(this.RowKey, other.RowKey);
            }

            return i;
        }

        #endregion
    }


    /// <summary>
    /// the Azure adaptor class.
    /// this class is not thread safe, it's suppose to be used in 
    /// single thread, caller need do thread safty if use this IDataAccess
    /// object in multi threads.
    /// </summary>
    public class AzureMediaDataAccessor : IMediaDataAccessor
    {
        /// <summary>
        /// The memory cache of all get/set objects in this DA session
        /// </summary>
        private ThreadLocal<SortedDictionary<PartitionKeyAndRowKey, AzureDAObject>> AzureDAObjectListThreadLocal =
            new ThreadLocal<SortedDictionary<PartitionKeyAndRowKey, AzureDAObject>>(() => new SortedDictionary<PartitionKeyAndRowKey, AzureDAObject>());

        /// <summary>
        /// the memory cache of all changed object in this DA session.
        /// all and only objects in this list will be sent to Azure
        /// during flush call.
        /// </summary>
        private ThreadLocal<SortedDictionary<PartitionKeyAndRowKey, AzureDAObject>> AzureDAObjectPendingChangedListThreadLocal =
            new ThreadLocal<SortedDictionary<PartitionKeyAndRowKey, AzureDAObject>>(() => new SortedDictionary<PartitionKeyAndRowKey, AzureDAObject>());

        private ThreadLocal<AzureDAEntityList> BufferedEntitiesThreadLocal = new ThreadLocal<AzureDAEntityList>(() => null);

        public AzureMediaDataAccessor()
        {
            this.QueueMediaAccessor = new AzureQueueDataAccessor();
        }
        
        #region IMediaDataAccessor Members

        #region Properties

        /// <summary>
        /// Accessor for Messages queues related media
        /// </summary>
        public IQueueMediaAccessor QueueMediaAccessor { get; set; }

        #endregion
        #region Synchronous Methods

        /// <summary>
        /// return the DAObject from Azure by partitionKey and rowKey
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <param name="classInfo"></param>
        /// <returns></returns>
        public DataAccessObject GetObject(string partitionKey, string rowKey, DAClassInfo classInfo)
        {
            if (string.IsNullOrEmpty(partitionKey))
            {
                Trace.TraceError("Partition key can't be null");
                return null;
            }

            if (string.IsNullOrEmpty(rowKey)) //row key in Azure can't be null
            {
                Trace.TraceError("RowKey can't be null or empty.");
            }

            PartitionKeyAndRowKey key = new PartitionKeyAndRowKey(partitionKey, rowKey);
            if (this.AzureDAObjectPendingChangedList.ContainsKey(key))
            {
                Trace.TraceInformation("GetObject from pending memory cache for set");
                AzureDAObject azureObj = this.AzureDAObjectPendingChangedList[key];
                return azureObj.GetDAObject();
            }
            else if (AzureDAObjectList.ContainsKey(key))
            {
                Trace.TraceInformation("GetObject({0},{1}) from memory cache.", partitionKey, rowKey);
                AzureDAObject azureObj = AzureDAObjectList[key];
                return azureObj.GetDAObject();
            }
            else
            {
                Trace.TraceInformation("GetObject({0},{1}) from memory Azure.", partitionKey, rowKey);

                IAzureDATableContext context = AzureTableContextFactory.CreateAzureTableContext();
                AzureDAEntityList entityList = context.GetAllEntities(partitionKey, rowKey, classInfo.TableName);

                //no entity mean the object is not stored in Azure
                if (entityList.Count <= 0)
                    return null;

                AzureDAObject daObj = new AzureDAObject(partitionKey, rowKey, classInfo.TableName)
                {
                    AzureDAEntityList = entityList,
                    PartitionKey = partitionKey,
                    RowKey = rowKey
                };

                //cached this item in memory so that we don't need
                //to go to Azure again next time
                this.AzureDAObjectList.Add(key, daObj);

                return daObj.GetDAObject();
            }
        }

        /// <summary>
        /// set the pending 
        /// </summary>
        /// <param name="daObject"></param>
        public void SetObject(DataAccessObject daObject)
        {
            if (null == daObject)
                throw new Exception("Can't set a null object.");

            Trace.TraceInformation("SetObject in Azure start:({0},{1})", daObject.PartitionKey, daObject.RowKey);
            PartitionKeyAndRowKey key = new PartitionKeyAndRowKey(daObject.PartitionKey, daObject.RowKey);

            AzureDAObject azureObj = null;
            if (this.AzureDAObjectPendingChangedList.ContainsKey(key))
            {
                Trace.TraceInformation("GetObject from pending memory cache for set");
                azureObj = this.AzureDAObjectPendingChangedList[key];
            }
            else if (this.AzureDAObjectList.ContainsKey(key))
            {
                Trace.TraceInformation("GetObject from memory cache for set");
                //retrieve the exiting object from memory cache
                azureObj = this.AzureDAObjectList[key];
            }
            else
            {
                Trace.TraceInformation("Set a new object");
                azureObj = new AzureDAObject(daObject.PartitionKey, daObject.RowKey, daObject.ClassInfo.TableName);
                this.AzureDAObjectList.Add(key, azureObj);
            }

            azureObj.Set(daObject);

            //Add changed object into pending list.
            if (!this.AzureDAObjectPendingChangedList.ContainsKey(key))
                this.AzureDAObjectPendingChangedList.Add(key, azureObj);
        }

        /// <summary>
        /// send all pending changed objects to Azure.
        /// </summary>
        public void Flush()
        {
            try
            {
                Trace.TraceInformation("Start flush all pending changes to Azure.");

                string currentPartitionKey = string.Empty;

                // TODO: Put correct table name once decided
                TableName currentTableName = TableName.TransactionHistory;

                int entityCount = 0;
                IAzureDATableContext context = AzureTableContextFactory.CreateAzureTableContext();

                // sort all object base on tableName, partition key and row key
                // because we need separate the objects with different partition key into 
                // different batch based on the batch operation requirement from Azure
                IEnumerable<KeyValuePair<PartitionKeyAndRowKey, AzureDAObject>> keys = from item in AzureDAObjectPendingChangedList
                                                                                       orderby item.Value.TableName, item.Key
                                                                                       select new KeyValuePair<PartitionKeyAndRowKey, AzureDAObject>(item.Key, item.Value);

                foreach (KeyValuePair<PartitionKeyAndRowKey, AzureDAObject> key in keys)
                {
                    AzureDAObject obj = key.Value;

                    if (string.IsNullOrEmpty(currentPartitionKey))
                    {
                        currentPartitionKey = obj.PartitionKey;
                        currentTableName = obj.TableName;
                    }


                    // FlushChanges and start a new batch when the table/partition key changed
                    if (currentTableName != obj.TableName || currentPartitionKey != obj.PartitionKey)
                    {
                        Trace.TraceInformation("Flush changes as Table/partition key changed from {0}:{1} to {2}:{3}",
                                                 currentTableName,
                                                 currentPartitionKey,
                                                 obj.TableName,
                                                 obj.PartitionKey);

                        context.FlushChanges(currentTableName);
                        entityCount = 0;
                        currentPartitionKey = obj.PartitionKey;
                        currentTableName = obj.TableName;
                    }

                    // if current object can't fit into current batch
                    // start a new batch
                    if (entityCount + obj.AzureDAEntityList.Count >= DAConstants.EntityCountForEachAzureBatch
                        && entityCount >= 1)
                    {
                        Trace.TraceInformation("Flush changes as reached the batch count and make sure that all entities for one object are in same batch.");
                        context.FlushChanges(currentTableName);
                        entityCount = 0;
                    }

                    foreach (AzureDAEntity entity in obj.AzureDAEntityList.Values)
                    {
                        ++entityCount;
                        context.AddPendingChange(entity);

                        if (entityCount >= DAConstants.EntityCountForEachAzureBatch)
                        {
                            Trace.TraceInformation("Flush changes as reached the batch count.");
                            context.FlushChanges(currentTableName);
                            entityCount = 0;
                        }
                    }
                }

                if (entityCount > 0)
                {
                    Trace.TraceInformation("Flush rest of the pending changed objects.");
                    context.FlushChanges(currentTableName);
                }
            }
            //Clear the pending change list after all pending changes
            //sent to Azure.
            finally
            {
                AzureDAObjectPendingChangedList.Clear();
                AzureDAObjectList.Clear();
            }
        }

        public IList<DataAccessObject> Query(String startPartitionKey, String endPartitionKey, String nameSpace, DateTime startDateTime, DateTime endDateTime, DAClassInfo classInfo, ref ContinuationTokenInternal continuationToken)
        {
            //clear the memory buffer each time before query to save memory.
            this.AzureDAObjectList.Clear();

            List<DataAccessObject> daObjList = new List<DataAccessObject>();

            if (null == continuationToken || null == BufferedEntities)
            {
                BufferedEntities = new AzureDAEntityList();
            }

            IAzureDATableContext context = AzureTableContextFactory.CreateAzureTableContext();
            AzureDAEntityList entityList =
                context.QueryEntities(startPartitionKey,
                                      endPartitionKey,
                                      nameSpace,
                                      classInfo.TableName,
                                      startDateTime,
                                      endDateTime,
                                      ref continuationToken);
            string currentPk = string.Empty;
            string currentRk = string.Empty;

            foreach (AzureDAEntity entity in entityList.Values)
            {
                currentPk = entity.PartitionKey;
                currentRk = entity.DARowKey;

                if (BufferedEntities.Count > 0)
                {
                    string pkInBuffer = BufferedEntities.Values[0].PartitionKey;
                    string rkInBuffer = BufferedEntities.Values[0].DARowKey;
                    if (currentPk != pkInBuffer || currentRk != rkInBuffer)
                    {
                        ConvertBufferEntitiesToObject(daObjList);
                    }
                }

                BufferedEntities.Add(entity);
            }

            // If we still have continuation token, all the pages/rows for DAObject might not retrieved 
            // so, keep in buffer for next query iteration
            // This code, internally assumes, AzureDAEntityList preserves the same order as CoreAzure scan order.
            // That is Azure Partitionkey then Rowkey, ordinal string compare sorting order
            if (null == continuationToken && BufferedEntities.Count > 0)
            {
                ConvertBufferEntitiesToObject(daObjList);
            }

            return daObjList;
        }

        private void ConvertBufferEntitiesToObject(List<DataAccessObject> daObjList)
        {
            string pkInBuffer = BufferedEntities.Values[0].PartitionKey;
            string rkInBuffer = BufferedEntities.Values[0].DARowKey;
            TableName tableNameInBuffer = BufferedEntities.Values[0].TableName;

            AzureDAObject azureDAObj = new AzureDAObject(pkInBuffer, rkInBuffer, tableNameInBuffer)
            {
                AzureDAEntityList = BufferedEntities,
                PartitionKey = pkInBuffer,
                RowKey = rkInBuffer
            };

            PartitionKeyAndRowKey key = new PartitionKeyAndRowKey(pkInBuffer, rkInBuffer);
            this.AzureDAObjectList.Add(key, azureDAObj);

            DataAccessObject daObject = azureDAObj.GetDAObject();

            // check for  entire object is marked for Delete or not
            if (daObject != null)
            {
                daObjList.Add(daObject);
            }

            BufferedEntities = new AzureDAEntityList();
        }

        /// <summary>
        /// Delete the DA object.
        /// </summary>
        public void DeleteObject(DataAccessObject daObject)
        {
            daObject.SerializedString = string.Empty;
            this.SetObject(daObject);
        }
        #endregion

        #endregion

        private SortedDictionary<PartitionKeyAndRowKey, AzureDAObject> AzureDAObjectList
        {
            get
            {
                return this.AzureDAObjectListThreadLocal.Value;
            }
            set
            {
                this.AzureDAObjectListThreadLocal.Value = value;
            }
        }

        private SortedDictionary<PartitionKeyAndRowKey, AzureDAObject> AzureDAObjectPendingChangedList
        {
            get
            {
                return this.AzureDAObjectPendingChangedListThreadLocal.Value;
            }
            set
            {
                this.AzureDAObjectPendingChangedListThreadLocal.Value = value;
            }
        }

        private AzureDAEntityList BufferedEntities
        {
            get
            {
                return this.BufferedEntitiesThreadLocal.Value;
            }
            set
            {
                this.BufferedEntitiesThreadLocal.Value = value;
            }
        }
    }
}
