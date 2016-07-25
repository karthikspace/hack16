using System;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    /// <summary>
    /// specify that current property is the row key of the object
    /// this attribute is optional
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DARowKeyAttribute : DataAccessAttribute
    {
    }
}
