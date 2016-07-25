
namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.RewardsIntl.Platform.DataAccess.Common;

    public class MockTableContext : IAzureDATableContext
    {
        public static AzureDAEntityList EntityList = new AzureDAEntityList();
        public static List<AzureDAEntityList> ContinousEntityList = new List<AzureDAEntityList>();
        public static bool ThrowAzureError = false;

        public static void Reset()
        {
            ThrowAzureError = false;
            ContinousEntityList.Clear();
            EntityList = new AzureDAEntityList();
        }

        public static void ContinueToken(bool emptyEntityList = true)
        {
            ContinousEntityList.Add(EntityList);
            if (emptyEntityList)
            {
                EntityList = new AzureDAEntityList();
            }
        }

        public static void ContinueToken(int count)
        {
            AzureDAEntityList newEntityList = new AzureDAEntityList();
            for (int i = 0; i < count; i++)
            {
                if (EntityList.Values.Count > 0)
                {
                    AzureDAEntity entity = EntityList.Values.First<AzureDAEntity>();
                    newEntityList.Add(entity);
                    EntityList.Remove(entity);
                }
            }

            ContinousEntityList.Add(newEntityList);
        }

        #region IAzureDATableContext Members

        public AzureDAEntityList GetAllEntities(string partitionKey, string daRowKey, TableName tableName)
        {
            if (ThrowAzureError)
            {
                throw new Exception("Azure Error");
            }

            IEnumerable<KeyValuePair<AzureDAEntityKey, AzureDAEntity>> entityLinqObject =
                (from entity in EntityList
                 where entity.Value.PartitionKey.Equals(partitionKey, StringComparison.Ordinal)
                 && String.Compare(entity.Value.RowKey, daRowKey + DAConstants.KeySeparatorLeft/*"{"*/, StringComparison.Ordinal) >= 0
                 && String.Compare(entity.Value.RowKey, daRowKey + DAConstants.KeySeparatorRight/*"}"*/, StringComparison.Ordinal) <= 0
                 select entity);

            AzureDAEntityList lastReturnedEntities;

            lastReturnedEntities = new AzureDAEntityList();
            foreach (var entityKey in entityLinqObject)
            {
                entityKey.Value.RetrievedFromAzure = true;
                lastReturnedEntities.Add(entityKey.Value);
            }
            return lastReturnedEntities;
        }

        public void AddPendingChange(AzureDAEntity entity)
        {
            if (ThrowAzureError)
            {
                throw new Exception("Azure Error");
            }

            if (entity.Deleted && entity.RetrievedFromAzure)
            {
                EntityList.Remove(entity);
            }
            else if (!entity.RetrievedFromAzure && EntityList.Contains(entity))
            {
                throw new Exception("Entity Already Exists");
            }
            else
            {
                EntityList[new AzureDAEntityKey(entity)] = entity;
            }

            entity.RetrievedFromAzure = false;
        }

        public void FlushChanges(TableName tableName)
        {
            if (ThrowAzureError)
            {
                throw new Exception("Azure Error");
            }
        }

        public AzureDAEntityList QueryEntities(string startPartitionKey, string endPartitionKey, string nameSpace, TableName tableName, System.DateTime startTime, System.DateTime endTime, ref ContinuationTokenInternal continuationToken)
        {
            if (ThrowAzureError)
            {
                throw new Exception("Azure Error");
            }

            AzureDAEntityList currentEntityList = ContinousEntityList[0];
            int i = 0;

            if (null != continuationToken)
            {
                while (currentEntityList.Values.Last<AzureDAEntity>().PartitionKey != continuationToken.PartitionKey
                    || currentEntityList.Values.Last<AzureDAEntity>().RowKey != continuationToken.RowKey)
                {
                    i++;
                    currentEntityList = ContinousEntityList[i];
                }
            }

            if (i + 1 == ContinousEntityList.Count)
                continuationToken = null;
            else
            {
                continuationToken = new ContinuationTokenInternal();
                AzureDAEntityList nextList = ContinousEntityList[i + 1];
                continuationToken.PartitionKey = nextList.Values.Last<AzureDAEntity>().PartitionKey;
                continuationToken.RowKey = nextList.Values.Last<AzureDAEntity>().RowKey;
            }

            IEnumerable<AzureDAEntity> entityLinqObject;
            if (startPartitionKey.Equals(endPartitionKey, StringComparison.Ordinal))
            {
                entityLinqObject = (from entity in currentEntityList.Values
                                    where entity.PartitionKey.Equals(startPartitionKey, StringComparison.Ordinal)
                                    && String.Compare(entity.RowKey, DAConstants.KeySeparatorLeft + nameSpace + DAConstants.KeySeparatorRight + DAConstants.KeySeparatorLeft/*"}{"*/, StringComparison.Ordinal) >= 0
                                    && String.Compare(entity.RowKey, DAConstants.KeySeparatorLeft/*"{"*/ + nameSpace + DAConstants.KeySeparatorRight + DAConstants.KeySeparatorRight /*"}}"*/, StringComparison.Ordinal) <= 0
                                    && entity.LastUpdatedDateTime >= startTime
                                    && entity.LastUpdatedDateTime < endTime
                                    select entity);
            }
            else
            {
                entityLinqObject = (from entity in currentEntityList.Values
                                    where String.Compare(entity.PartitionKey, startPartitionKey, StringComparison.Ordinal) >= 0
                                                           && String.Compare(entity.PartitionKey, endPartitionKey, StringComparison.Ordinal) < 0
                                                           && String.Compare(entity.RowKey, DAConstants.KeySeparatorLeft/*"{"*/ + nameSpace + DAConstants.KeySeparatorRight + DAConstants.KeySeparatorLeft/*"}{"*/, StringComparison.Ordinal) >= 0
                                                           && String.Compare(entity.RowKey, DAConstants.KeySeparatorLeft/*"{"*/ + nameSpace + DAConstants.KeySeparatorRight + DAConstants.KeySeparatorRight/*"}}"*/, StringComparison.Ordinal) <= 0 //use "}" because } is larger than "{" in ascii
                                                           && entity.LastUpdatedDateTime >= startTime
                                                           && entity.LastUpdatedDateTime < endTime
                                    select entity);
            }

            AzureDAEntityList lastReturnedEntities;

            lastReturnedEntities = new AzureDAEntityList();
            foreach (var entityKey in entityLinqObject)
            {
                entityKey.RetrievedFromAzure = true;
                lastReturnedEntities.Add(entityKey);
            }

            return lastReturnedEntities;
        }




        #endregion
    }
}
