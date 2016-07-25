// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataAccessObject.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// This is a intermedia object class inside Data Access Layer, 
    /// it contains the PartitionKey, RowKey and serialized string of the customer object.  
    /// Different impletmentation (Azure or Pegasus) will translate this object into their 
    /// specific object like AzureDAObject.
    /// </summary>
    public class DataAccessObject
    {
        #region properties
        /// <summary>
        /// The partition key of the object, directly copied from outside.
        /// </summary>
        public String PartitionKey
        {
            get
            {
                return this.partitionKey;
            }
            set
            {
                this.partitionKey = value.ToUpper(CultureInfo.InvariantCulture);
            }
        }

        private string partitionKey;

        /// <summary>
        /// This rowkey inlcude the NameSpace of the customer class
        /// </summary>
        public String RowKey
        {
            get
            {
                return this.rowKey;
            }
            set
            {
                this.rowKey = value.ToUpper(CultureInfo.InvariantCulture);
            }
        }

        private string rowKey;

        /// <summary>
        /// Serialized string
        /// </summary>
        public String SerializedString { get; set; }

        /// <summary>
        /// the type of the customer class
        /// </summary>
        public DAClassInfo ClassInfo { get; set; }
        #endregion

        #region constructors
        /// <summary>
        /// constructor used by Set operation.
        /// </summary>
        /// <param name="customerObject"></param>
        public DataAccessObject(object customerObject)
        {
            Trace.TraceInformation("Construct DAObject from customer object.");

            this.ClassInfo = DAClassInfo.GetDAClassInfo(customerObject);
            this.PartitionKey = Convert.ToString(
                ClassInfo.PartitionKeyProperty.GetValue(customerObject, null),
                CultureInfo.InvariantCulture);

            if (PartitionKey.Contains(DAConstants.KeySeparatorLeft) || PartitionKey.Contains(DAConstants.KeySeparatorRight))
                throw new Exception(String.Format(CultureInfo.InvariantCulture, "PartitonKey can't contain {0} and {1}", DAConstants.KeySeparatorLeft, DAConstants.KeySeparatorRight));

            RowKey = GetInternalRowKeyFromCustomerObject(customerObject);
            Serialize(customerObject);
        }

        private void Serialize(object customerObject)
        {
            SerializedString = this.ClassInfo.InternalSerializer.Serialize(customerObject);
        }

        /// <summary>
        /// constructor used in get scenario
        /// </summary>
        public DataAccessObject()
        {
            Trace.TraceInformation("Default constructor of Data Access Object.");
        }
        #endregion

        #region public methods

        /// <summary>
        /// return the row key that used by data access layer internal
        /// </summary>
        /// <param name="customerObject"></param>
        /// <returns></returns>
        public String GetInternalRowKeyFromCustomerObject(object customerObject)
        {
            return this.ClassInfo.GetInternalRowKeyFromRowKey(null == this.ClassInfo.RowKeyProperty ? null :
                this.ClassInfo.RowKeyProperty.GetValue(customerObject, null));
        }

        /// <summary>
        /// Deserialize daobject into specified customer object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Deserialize<T>()
        {
            this.ClassInfo = DAClassInfo.GetDAClassInfo<T>();
            object obj = ClassInfo.InternalSerializer.Deserialize(SerializedString);

            return (T)obj;
        }

        #endregion
    }
}
