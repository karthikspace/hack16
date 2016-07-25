namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class ContinuationTokenInternal
    {
        public string TokenId { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string TableName { get; set; }

        public override string ToString()
        {
            return TableName + ":" + PartitionKey + ":" + RowKey;
        }
    }
}
