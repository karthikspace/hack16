using System;
using System.Linq;
using System.Text;
using Microsoft.RewardsIntl.Platform.DataAccess.Common;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure
{
    /// <summary>
    /// this class contain a serials of AzureDaEntities
    /// and contruct the entities during Set operation
    /// and assembly DAObject from entities during Get operation
    /// </summary>
    public class AzureDAObject
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public TableName TableName { get; set; }

        /// <summary>
        /// the list of entites that map to current DAObject
        /// </summary>
        public AzureDAEntityList AzureDAEntityList;

        public AzureDAObject(string partitionKey, string rowKey, TableName tableName)
        {
            if (string.IsNullOrEmpty(partitionKey))
            {
                throw new Exception("PartitionKey can't be null or empty.");
            }

            if (string.IsNullOrEmpty(rowKey))
            {
                throw new Exception("RowKey can't be null or empty.");
            }

            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
            this.TableName = tableName;

            AzureDAEntityList = new AzureDAEntityList();
        }
        
        /// <summary>
        /// Assembly all entites to DAObject
        /// </summary>
        /// <returns></returns>
        public DataAccessObject GetDAObject()
        {
            int cnt = AzureDAEntityList.Count;

            if (cnt <= 0)
            {
                return null;
            }
            else
            {
                // To create DAObject from Azure Rows/Pages, it should be sorted by integer PageId
                // why? default azure sort order: ordinal string compare
                // So, Azure Order 1, 10, 2, 3, 4, 5, 6, 7, 8, 9
                // Order we need:  1, 2,  3, 4, 5, 6, 7, 8, 9, 10
                var entityListSortedByPageId= AzureDAEntityList.Values.OrderBy(entity => entity.PageId);

                StringBuilder sb = new StringBuilder();
                foreach(var entity in entityListSortedByPageId)
                {
                    if (entity.Deleted)
                    {
                        break;
                    }

                    sb.Append(entity.SerializedString);
                }

                // All entities already marked for delete
                if (string.IsNullOrEmpty(sb.ToString()))
                {
                    return null;
                }

                return new DataAccessObject()
                {
                    PartitionKey = this.PartitionKey,
                    RowKey = this.RowKey,
                    SerializedString = sb.ToString()
                };
            }
        }

        /// <summary>
        /// Construct entities from DAObject.
        /// </summary>
        /// <param name="daObj"></param>
        public void Set(DataAccessObject daObj)
        {
            if (null == daObj)
            {
                throw new Exception("DAObject can't be NULL for Set.");
            }

            if (null == daObj.SerializedString)
            {
                throw new Exception("DAObject can't be NULL for serialized string.");
            }

            if (this.PartitionKey != daObj.PartitionKey)
            {
                throw new Exception("Partition keys are not same.");
            }

            if (this.RowKey != daObj.RowKey)
            {
                throw new Exception("Row keys are not same. ");
            }

            if (this.TableName != daObj.ClassInfo.TableName)
            {
                throw new Exception("Table Names are not same. ");
            }


            DateTime dtNow = DateTime.UtcNow;

            int currentEntityPageId = 0;
            int currentStringPosition = 0;
            int stringLength = daObj.SerializedString.Length;
            int originalEntityListLength = AzureDAEntityList.Count;

            while (currentStringPosition < stringLength)
            {
                int len = DAConstants.LargestAzureDataPageSize;

                //get the string for current page, it will be a full page length or
                //the rest of the string if less then full page.
                if (DAConstants.LargestAzureDataPageSize + currentStringPosition >= stringLength)
                    len = stringLength - currentStringPosition;

                string currentPage = daObj.SerializedString.Substring(currentStringPosition, len);

                AzureDAEntityKey key = new AzureDAEntityKey()
                {
                    PartitionKey = daObj.PartitionKey,
                    RowKey = daObj.RowKey + DAConstants.KeySeparatorLeft + currentEntityPageId + DAConstants.KeySeparatorRight
                };

                if (AzureDAEntityList.ContainsKey(key))
                {
                    //update the entity if current entity with same page id existed in Azure
                    AzureDAEntityList[key].SerializedString = currentPage;
                    AzureDAEntityList[key].Deleted = false; //undelete it if it was marked as deleted before.
                    AzureDAEntityList[key].LastUpdatedDateTime = dtNow;
                    AzureDAEntityList[key].DARowKey = daObj.RowKey;
                    AzureDAEntityList[key].TableName = this.TableName;
                }
                else
                {
                    //Add new entity to Azure
                    AzureDAEntityList[key] = new AzureDAEntity()
                    {
                        PartitionKey = this.PartitionKey,
                        RowKey = key.RowKey,
                        DARowKey = daObj.RowKey,
                        PageId = currentEntityPageId,
                        SerializedString = currentPage,
                        RetrievedFromAzure = false,
                        Deleted = false,
                        TableName = this.TableName,
                        LastUpdatedDateTime = dtNow
                    };
                }

                ++currentEntityPageId;
                currentStringPosition += DAConstants.LargestAzureDataPageSize;
            }

            //delete the rest of unused entities.
            while (currentEntityPageId < originalEntityListLength)
            {
                AzureDAEntityKey key = new AzureDAEntityKey()
                {
                    PartitionKey = daObj.PartitionKey,
                    RowKey = daObj.RowKey + DAConstants.KeySeparatorLeft + currentEntityPageId + DAConstants.KeySeparatorRight
                };

                AzureDAEntityList[key].TableName = this.TableName;
                AzureDAEntityList[key].LastUpdatedDateTime = dtNow;
                AzureDAEntityList[key].SerializedString = string.Empty;
                AzureDAEntityList[key].Deleted = true;

                AzureDAEntityList[key].DARowKey = daObj.RowKey;
                currentEntityPageId++;
            }

            // Not retrieved from Azure, but delete operation requested
            if (this.AzureDAEntityList.Count == 0 && daObj.SerializedString.Length == 0)
            {
                AzureDAEntityKey key = new AzureDAEntityKey()
                {
                    PartitionKey = daObj.PartitionKey,
                    RowKey = daObj.RowKey + DAConstants.KeySeparatorLeft + currentEntityPageId + DAConstants.KeySeparatorRight,
                };

                AzureDAEntityList[key] = new AzureDAEntity()
                {
                    PartitionKey = daObj.PartitionKey,
                    RowKey = key.RowKey,
                    DARowKey = daObj.RowKey,
                    PageId = currentEntityPageId,
                    SerializedString = string.Empty,
                    RetrievedFromAzure = false,
                    Deleted = true,
                    TableName = this.TableName,
                    LastUpdatedDateTime = dtNow,
                    ETag = "*"
                };
            }
        }
    }
}
