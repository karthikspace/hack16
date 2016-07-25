//-----------------------------------------------------------------------
// <copyright file="AzureDATableContext.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Text;
    using System.Threading;
    using Microsoft.Azure.KeyVault;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class AzureDATableContext : IAzureDATableContext
    {
        private static bool isInitialized = false;
        private static object initializedLock = new object();

        private static Dictionary<string, CloudTable> CloudTables = new Dictionary<string, CloudTable>(StringComparer.OrdinalIgnoreCase);
        private static TableRequestOptions TableEncryptedRequestOptions;
        private static bool IsTableEncryptionEnabled;

        private ThreadLocal<TableBatchOperation> tableBatchOperationThreadLocal = new ThreadLocal<TableBatchOperation>(() => null);
        private ThreadLocal<bool> batchOperationInProgressThreadLocal = new ThreadLocal<bool>(() => false);

        /// <summary>
        /// Create Azure tables for first time use.
        /// </summary>
        static AzureDATableContext()
        {
            if (!isInitialized)
            {
                lock (initializedLock)
                {
                    if (!isInitialized)
                    {
                        // This sets it globally for all new ServicePoints
                        ServicePointManager.UseNagleAlgorithm = false;

                        var account =
                            CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);

                        var tableClient = account.CreateCloudTableClient();

                        // Create all registered tables
                        foreach (TableName tableName in Enum.GetValues(typeof(TableName)))
                        {
                            var cloudTable = tableClient.GetTableReference(tableName.ToString());
                            cloudTable.CreateIfNotExists();
                            CloudTables[tableName.ToString()] = cloudTable;
                        }

                        IsTableEncryptionEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableTableEncryption"].Trim(), CultureInfo.InvariantCulture);
                        if (IsTableEncryptionEnabled)
                        {
                            // Adding Encryption Request Options
                            var encryptionKeyBytes = Encoding.ASCII.GetBytes(ConfigurationManager.AppSettings["TableEncryptionKey"].Trim());
                            var key = new SymmetricKey(ConfigurationManager.AppSettings["TableEncryptionKeyId"].Trim(), encryptionKeyBytes);

                            TableEncryptedRequestOptions = new TableRequestOptions()
                            {
                                EncryptionPolicy = new TableEncryptionPolicy(key, null)
                            };
                        }

                        isInitialized = true;
                    }
                }
            }
        }

        public AzureDATableContext()
        {
        }

        void IAzureDATableContext.FlushChanges(TableName tableName)
        {
            FlushChanges(tableName);
        }

        AzureDAEntityList IAzureDATableContext.QueryEntities(string startPartitionKey, string endPartitionKey, string nameSpace, TableName tableName,
            DateTime startTime, DateTime endTime, ref ContinuationTokenInternal continuationToken)
        {
            return QueryEntities(startPartitionKey, endPartitionKey, nameSpace, tableName, startTime, endTime, ref continuationToken);
        }

        public AzureDAEntityList QueryEntities(
            string startPartitionKey,
            string endPartitionKey,
            string nameSpace,
            TableName tableName,
            DateTime startTime,
            DateTime endTime,
            ref ContinuationTokenInternal continuationToken)
        {
            Trace.TraceInformation("AzureTableContext: QueryEntities ({0},{1},{2},{3},{4})",
                                        startPartitionKey,
                                        endPartitionKey,
                                        startTime,
                                        endTime,
                                        continuationToken);

            string partitionKeyFilter = null;

            if (startPartitionKey.Equals(endPartitionKey, StringComparison.Ordinal))
            {
                partitionKeyFilter = TableQuery.GenerateFilterCondition(AzureDAEntityProperties.PartitionKey, QueryComparisons.Equal, startPartitionKey);

            }
            else
            {
                string partitionKeyLowerFilter = TableQuery.GenerateFilterCondition(AzureDAEntityProperties.PartitionKey, QueryComparisons.GreaterThanOrEqual, startPartitionKey);
                string partitionKeyUpperFilter = TableQuery.GenerateFilterCondition(AzureDAEntityProperties.PartitionKey, QueryComparisons.LessThan, endPartitionKey);
                partitionKeyFilter = TableQuery.CombineFilters(partitionKeyLowerFilter, TableOperators.And, partitionKeyUpperFilter);
            }

            string rowKeyLowerFilter = TableQuery.GenerateFilterCondition(AzureDAEntityProperties.RowKey,
                                                                          QueryComparisons.GreaterThanOrEqual,
                                                                          DAConstants.KeySeparatorLeft + nameSpace + DAConstants.KeySeparatorRight + DAConstants.KeySeparatorLeft/*"}{"*/);

            string rowKeyUpperFilter = TableQuery.GenerateFilterCondition(AzureDAEntityProperties.RowKey,
                                                                          QueryComparisons.LessThanOrEqual,
                                                                          DAConstants.KeySeparatorLeft/*"{"*/ + nameSpace + DAConstants.KeySeparatorRight + DAConstants.KeySeparatorRight /*"}}"*/);

            string rowKeyFilter = TableQuery.CombineFilters(rowKeyLowerFilter, TableOperators.And, rowKeyUpperFilter);

            string lastUpdatedDateTimeLowerFilter = TableQuery.GenerateFilterConditionForDate(AzureDAEntityProperties.LastUpdatedDateTime, 
                                                                                              QueryComparisons.GreaterThanOrEqual, 
                                                                                              DateTime.SpecifyKind(startTime, DateTimeKind.Utc));

            string lastUpdatedDateTimeUpperFilter = TableQuery.GenerateFilterConditionForDate(AzureDAEntityProperties.LastUpdatedDateTime, 
                                                                                              QueryComparisons.LessThan, 
                                                                                              DateTime.SpecifyKind(endTime, DateTimeKind.Utc));

            string lastUpdatedDateTimeFilter = TableQuery.CombineFilters(lastUpdatedDateTimeLowerFilter, TableOperators.And, lastUpdatedDateTimeUpperFilter);

            string partitionAndrowKeyFilter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter);
            string query = TableQuery.CombineFilters(partitionAndrowKeyFilter, TableOperators.And, lastUpdatedDateTimeFilter);

            return ExecuteAQuery(query, ref continuationToken, tableName);
        }

        /// <summary>
        /// get all Azure entities for a DAObject with partition key and rowkey
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="daRowKey">the rowkey representation in DA</param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public AzureDAEntityList GetAllEntities(string partitionKey, string daRowKey, TableName tableName)
        {
            Trace.TraceInformation("AzureTableContext: Retrieving entities from Azure for ({0},{1}) under TableName: {2}", partitionKey, daRowKey, tableName);

            // get all entities whith given partition key 
            // and row key start with "rowkey{"

            string partitionKeyFilter = TableQuery.GenerateFilterCondition(AzureDAEntityProperties.PartitionKey, QueryComparisons.Equal, partitionKey);

            string rowKeyLowerFilter = TableQuery.GenerateFilterCondition(AzureDAEntityProperties.RowKey, QueryComparisons.GreaterThanOrEqual, daRowKey + DAConstants.KeySeparatorLeft /*"{"*/);
            string rowKeyUpperFilter = TableQuery.GenerateFilterCondition(AzureDAEntityProperties.RowKey, QueryComparisons.LessThan, daRowKey + DAConstants.KeySeparatorRight /*"}"*/);
            string rowKeyFilter = TableQuery.CombineFilters(rowKeyLowerFilter, TableOperators.And, rowKeyUpperFilter);

            string query = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, rowKeyFilter);

            return ExecuteAQuery(query, tableName);
        }

        /// <summary>
        /// put all pending changes into AzureTableContext
        /// </summary>
        /// <param name="entity"></param>
        public void AddPendingChange(AzureDAEntity entity)
        {
            try
            {
                if (!batchOperationInProgress)
                {
                    tableBatchOperation = new TableBatchOperation();
                    batchOperationInProgress = true;
                }

                if (entity.RetrievedFromAzure)
                {
                    if (entity.Deleted)
                    {
                        tableBatchOperation.Delete(entity);
                    }
                    else
                    {
                        tableBatchOperation.Replace(entity);
                    }
                }
                else
                {
                    // the deleted objects which is not retrieved from Azure will
                    // be abandoned.
                    if (!entity.Deleted)
                    {
                        tableBatchOperation.Insert(entity);
                    }
                }

            }
            catch (Exception ex)
            {
                HandleAzureException(ex);
            }
        }

        /// <summary>
        /// Save pending changes with batch
        /// </summary>
        public void FlushChanges(TableName tableName)
        {
            try
            {
                Trace.TraceInformation("AzureTableContext: SaveChanges as Batch. Count: {0}", tableBatchOperation.Count);

                if (batchOperationInProgress)
                {
                    CloudTables[tableName.ToString()].ExecuteBatch(tableBatchOperation, GetTableEncryptedRequestOptions(tableName));

                    // get ready for next batch operation
                    tableBatchOperation = null;
                    batchOperationInProgress = false;
                }

                Trace.TraceInformation("AzureTableContext: Successfully flushed changes.");

            }
            catch (Exception ex)
            {
                HandleAzureException(ex, tableName.ToString());
            }
        }

        /// <summary>
        /// Returns encryption options for the tables mentioned
        /// </summary>
        private static TableRequestOptions GetTableEncryptedRequestOptions(TableName tableName)
        {
            if (!IsTableEncryptionEnabled)
            {
                return null;
            }

            // PII Data is stored in 3 Tables Currently
            switch (tableName)
            {
                case TableName.UserInformation:
                    return TableEncryptedRequestOptions;
                default:
                    return null;
            }
        }

        private static void HandleAzureException(Exception ex, string tableName = "")
        {
            bool isUpdateEntityConflict = false;
            string errorMsg = string.Empty;
            var se = ex as StorageException;
            if (se != null)
            {
                errorMsg += se.Message;
                if (se.RequestInformation != null && se.RequestInformation.HttpStatusCode == 412)
                {
                    isUpdateEntityConflict = true;
                }
            }
            else
            {
                errorMsg = ex.Message;
            }

            var errorCode = isUpdateEntityConflict ? "Azure Update Conflict Error" : "Azure Error";

            Trace.TraceError("AzureTableContext:" + tableName + " Exception with ErrorCode: {0}", errorCode);

            throw new Exception(errorMsg, ex);
        }

        private ContinuationTokenInternal GetContinuationTokenInternal(TableContinuationToken tableContinuationToken)
        {
            ContinuationTokenInternal continuationToken = null;

            if (tableContinuationToken != null)
            {
                continuationToken = new ContinuationTokenInternal();
                continuationToken.PartitionKey = tableContinuationToken.NextPartitionKey;
                continuationToken.RowKey = tableContinuationToken.NextRowKey;
                continuationToken.TableName = tableContinuationToken.NextTableName;
            }

            return continuationToken;
        }
        private TableContinuationToken GetTableContinuationToken(ContinuationTokenInternal continuationTokenInternal)
        {
            TableContinuationToken tableContinuationToken = null;

            if (continuationTokenInternal != null)
            {
                tableContinuationToken = new TableContinuationToken();
                tableContinuationToken.NextPartitionKey = continuationTokenInternal.PartitionKey;
                tableContinuationToken.NextRowKey = continuationTokenInternal.RowKey;
                tableContinuationToken.NextTableName = continuationTokenInternal.TableName;
            }

            return tableContinuationToken;
        }


        private AzureDAEntityList ExecuteAQuery(string query, ref ContinuationTokenInternal continuationTokenInternal, bool getAllRecordsAtSameTime, TableName tableName)
        {
            var entityList = new AzureDAEntityList();
            var cloudTable = CloudTables[tableName.ToString()];
            try
            {
                do
                {
                    var tableQuery = new TableQuery<AzureDAEntity>().Where(query);
                    TableQuerySegment<AzureDAEntity> azureResponse = cloudTable.ExecuteQuerySegmented(
                        tableQuery,
                        this.GetTableContinuationToken(continuationTokenInternal),
                        GetTableEncryptedRequestOptions(tableName));
                    List<AzureDAEntity> entities = azureResponse.Results;
                    continuationTokenInternal = this.GetContinuationTokenInternal(azureResponse.ContinuationToken);

                    foreach (var entity in entities)
                    {
                        entity.RetrievedFromAzure = true;

                        // Its internal variable. not from Azure
                        entity.TableName = tableName;

                        entityList.Add(entity);
                    }
                }
                while (continuationTokenInternal != null && getAllRecordsAtSameTime);
            }
            catch (Exception ex)
            {
                HandleAzureException(ex);
            }

            return entityList;
        }

        private AzureDAEntityList ExecuteAQuery(string query, ref ContinuationTokenInternal continuationTokenInternal, TableName tableName)
        {
            return ExecuteAQuery(query, ref continuationTokenInternal, false, tableName);
        }

        private AzureDAEntityList ExecuteAQuery(string query, TableName tableName)
        {
            ContinuationTokenInternal continuationToken = null;
            return ExecuteAQuery(query, ref continuationToken, true, tableName);
        }

        private TableBatchOperation tableBatchOperation
        {
            get
            {
                return this.tableBatchOperationThreadLocal.Value;
            }
            set
            {
                this.tableBatchOperationThreadLocal.Value = value;
            }
        }

        private bool batchOperationInProgress
        {
            get
            {
                return this.batchOperationInProgressThreadLocal.Value;
            }
            set
            {
                this.batchOperationInProgressThreadLocal.Value = value;
            }
        }
    }
}
