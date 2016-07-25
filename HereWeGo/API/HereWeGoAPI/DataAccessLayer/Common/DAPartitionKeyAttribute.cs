using System;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    /// <summary>
    /// Specify the partition key of a DA Class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DAPartitionKeyAttribute : DataAccessAttribute
    {
    }
}
