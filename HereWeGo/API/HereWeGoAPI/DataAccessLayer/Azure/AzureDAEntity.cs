using System;
using System.Collections.Generic;
using Microsoft.RewardsIntl.Platform.DataAccess.Common;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure
{
    public class AzureDAEntityKey : IComparable<AzureDAEntityKey>
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }
        
        public AzureDAEntityKey(AzureDAEntity entity)
        {
            this.PartitionKey = entity.PartitionKey;
            this.RowKey = entity.RowKey;
        }

        public AzureDAEntityKey()
        {
        }

        #region IComparable<AzureDAEntityKey> Members

        // preserve same Azure Partitionkey then Rowkey, ordinal string sorting order on retrieval
        // its like with braces for Rowkey {DAObjectType}{DARowKey}{PageId}.
        // Example: {USERACTIVITYPAGE}{179}{0}
        // Its very important to preserve the order to concatenate pages split across 2 sequential query scans
        public int CompareTo(AzureDAEntityKey other)
        {
            int r1 = string.CompareOrdinal(this.PartitionKey, other.PartitionKey);
            if (0 != r1)
            {
                return r1;
            }

            return string.CompareOrdinal(this.RowKey, other.RowKey);
        }

        #endregion
    }

    public class AzureDAEntity : TableEntity
    {
        /// <summary>
        /// this page id in zure rows. this field is used 
        /// as a sequence id to assembly to a DAObject
        /// </summary>
        public int PageId { get; set; }

        [EncryptProperty]
        public string SerializedString { get; set; }

        /// <summary>
        /// put this as internal as we don't want to this to be store into azure 
        /// this flag indicate if we want to add new or update existing entity
        /// when SetObject is called.
        /// </summary>
        internal bool RetrievedFromAzure = true;

        /// <summary>
        /// The bcp work won't pickup and deleted records
        /// so we will mark the entity as deleted instead of
        /// real delete them
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// because 1 da object may be splitted into mutliple Azure
        /// entities, use this field to track the last update datetime
        /// of the whole object.
        /// </summary>
        public DateTime LastUpdatedDateTime { get; set; }

        /// <summary>
        /// the REAL row key of the whole da object.
        /// </summary>
        public String DARowKey { get; set; }
        
        /// <summary>
        /// The table name this object need to be stored
        /// </summary>
        internal TableName TableName { get; set; }

        #region constructors

        /// <summary>
        /// constructor used by serializer
        /// </summary>
        public AzureDAEntity()
        {
            Deleted = false;
        }
        #endregion
    }

    public class AzureDAEntityProperties
    {
        public const string PartitionKey = "PartitionKey";
        public const string RowKey = "RowKey";
        public const string LastUpdatedDateTime = "LastUpdatedDateTime";
    }

    public class AzureDAEntityList : SortedList<AzureDAEntityKey, AzureDAEntity>
    {
        public void Add(AzureDAEntity entity)
        {
            this.Add(new AzureDAEntityKey(entity), entity);
        }

        public bool Contains(AzureDAEntity entity)
        {
            return this.ContainsKey(new AzureDAEntityKey(entity));
        }

        public bool Remove(AzureDAEntity entity)
        {
            return this.Remove(new AzureDAEntityKey(entity));
        }
    }
}