
namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure
{
    public class AzureTableContextFactory
    {
        public static bool UseMock = false;
        /// <summary>
        /// </summary>
        static AzureTableContextFactory()
        {
        }

        public static IAzureDATableContext CreateAzureTableContext()
        {
            if (UseMock)
                return new MockTableContext();
            else
                return new AzureDATableContext();
        }
    }
}
