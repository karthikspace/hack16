using System;
using Microsoft.RewardsIntl.Platform.DataAccess.Common;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure
{
    /// <summary>
    /// internal interface for unit test purpose
    /// this interface will abstract out all Azure operations
    /// unit test will provide a mock object for this.
    /// </summary>
    public interface IAzureDATableContext
    {
        /// <summary>
        /// return all entities list for given partitionkey and rowkey
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="daRowKey"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        AzureDAEntityList GetAllEntities(string partitionKey, string daRowKey, TableName tableName);

        /// <summary>
        /// add 1 pending changes into context
        /// </summary>
        /// <param name="entity"></param>
        void AddPendingChange(AzureDAEntity entity);

        /// <summary>
        /// Send chagnes to azure in 1 batch
        /// </summary>
        void FlushChanges(TableName tableName);

        /// <summary>
        /// 
        /// </summary>
        AzureDAEntityList QueryEntities(
            string startPartitionKey,
            string endPartitionKey,
            string nameSpace,
            TableName tableName,
            DateTime startTime,
            DateTime endTime,
            ref ContinuationTokenInternal continuationToken);
    }
}
