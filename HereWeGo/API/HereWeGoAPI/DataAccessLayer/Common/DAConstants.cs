using System.Collections.Generic;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    /// <summary>
    /// Constant values
    /// </summary>
    public class DAConstants
    {
        /// <summary>
        /// General category for Data access layer
        /// </summary>
        public static IList<string> DA_CATEGORY = new List<string>() { "General" };

        /// <summary>
        /// Azure only allow 32k of unicode string (64k bytes) for each column.
        /// </summary>
        public static int LargestAzureDataPageSize = 32 * 1024;

        /// <summary>
        /// Azure limitation of 100 entities or 4M data volumn
        /// </summary>
        public static int EntityCountForEachAzureBatch = 60; //64K each with some buffer

        public static string KeySeparatorLeft = "{";

        public static string KeySeparatorRight = "}";
    }
}
