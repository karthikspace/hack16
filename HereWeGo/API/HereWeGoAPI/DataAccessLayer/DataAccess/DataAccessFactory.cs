using Microsoft.RewardsIntl.Platform.DataAccess.Common;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Azure.DataAccess
{
    public enum DataAccessAdapter
    {
        Azure,
    }

    /// <summary>
    /// The factory class of DataAccess layer.
    /// use this class the create instance of Data access object.
    /// </summary>
    public static class DataAccessFactory
    {
        public static DataAccessAdapter CurrentAdapter = DataAccessAdapter.Azure;

        /// <summary>
        /// Create an instance of data access layer object.
        /// </summary>
        /// <returns></returns>
        public static IDataAccess CreateDataAccessObject()
        {
            return CreateDataAccessObject(CurrentAdapter);
        }

        public static IDataAccess CreateDataAccessObject(DataAccessAdapter target)
        {
            return new DataAccessImplementation(new AzureMediaDataAccessor(), target);
        }
    }
}