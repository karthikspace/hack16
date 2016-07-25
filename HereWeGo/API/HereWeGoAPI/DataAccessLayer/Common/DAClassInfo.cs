namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Store all reflection information needed for DALayer of each
    /// customer object, the class will be cached in DALayer to boost 
    /// the performance.
    /// </summary>
    public class DAClassInfo
    {
        #region properties
        /// <summary>
        /// type of customer object
        /// </summary>
        public Type ClassType { get; set; }

        /// <summary>
        /// the name space of the customer object,
        /// it's read from the namspace attribute of the class
        /// </summary>
        public String Namespace { get; set; }

        /// <summary>
        /// the property info of PartitionKey in customer object,
        /// which has PartitionKey attribute
        /// </summary>
        public PropertyInfo PartitionKeyProperty { get; set; }

        /// <summary>
        /// The property info of RowKey in customer object,
        /// which has RowKey attribute
        /// </summary>
        public PropertyInfo RowKeyProperty { get; set; }

        /// <summary>
        /// Cache the xmlserializer instance for each type
        /// </summary>
        public IDaSerializer InternalSerializer { get; set; }

        /// <summary>
        /// allow user to choose serializer
        /// </summary>
        public SerializerType SerializerType { get; set; }

        /// <summary>
        /// The table name this object need to be stored
        /// used only in Azure today
        /// </summary>
        public TableName TableName { get; set; }

        /// <summary>
        /// Represents informaiton needed to manipulate Azure storage queue
        /// </summary>
        public PropertyInfo QueueMessageInfoProperty { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// construct the DAClass info for specific class type.
        /// </summary>
        /// <param name="classType"></param>
        public DAClassInfo(Type classType)
        {
            Trace.TraceInformation("Constructing class info of {0}", classType);
            ExtractTypeInformation(classType);
        }

        private void ExtractTypeInformation(Type classType)
        {
            this.ClassType = classType;

            PropertyInfo[] PIs = classType.GetProperties();
            foreach (var pi in PIs)
            {
                DataAccessAttribute daAttribute = (DataAccessAttribute)Attribute.GetCustomAttribute(pi,
                    typeof(DataAccessAttribute), true);

                if (daAttribute is DAPartitionKeyAttribute)
                {
                    if (null != this.PartitionKeyProperty)
                    {
                        throw new Exception("More than one Partition Key");
                    }

                    Trace.TraceInformation("PartitionKeyInfo:{0}", pi);
                    this.PartitionKeyProperty = pi;
                    continue;
                }
                if (daAttribute is DARowKeyAttribute)
                {
                    if (null != this.RowKeyProperty)
                        throw new Exception("More than one row key");

                    Trace.TraceInformation("RowKeyInfo:{0}", pi);
                    this.RowKeyProperty = pi;
                    continue;
                }
            }

            object[] attrs = classType.GetCustomAttributes(typeof(DAClassAttribute), true);

            if (null != attrs && attrs.Length != 1)
            {
                throw new Exception("You must specify 1 and only 1 DAClassAttribute in your custom object.");
            }

            if (attrs[0] != null && attrs[0] is DAClassAttribute)
            {
                DAClassAttribute classAttr = (DAClassAttribute)attrs[0];
                Trace.TraceInformation(
                    "Namespace:{0}, TableName:{1} ",
                    classAttr.Namespace,
                    classAttr.TableName);

                this.Namespace = classAttr.Namespace;
                this.SerializerType = SerializerType.DataContractorSerializer;
                this.TableName = classAttr.TableName;
            }

            if (null == this.PartitionKeyProperty)
            {
                throw new Exception("Partition Key not specified");
            }

            if (string.IsNullOrEmpty(this.Namespace))
            {
                throw new Exception("Namespace not specified");
            }

            this.InternalSerializer = DaSerializerFactory.CreateSerializer(classType, this.SerializerType);

        }
        #endregion

        #region public methods

        /// <summary>
        /// used in set scenario, concatenate the namespace and rowkey into internal rowkey
        /// </summary>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public String GetInternalRowKeyFromRowKey(object rowKey)
        {
            string rowKeyStr = Convert.ToString(rowKey, CultureInfo.InvariantCulture);

            if (rowKeyStr.Contains(DAConstants.KeySeparatorLeft) || rowKeyStr.Contains(DAConstants.KeySeparatorRight))
                throw new Exception(String.Format(CultureInfo.InvariantCulture, "RowKey can't contain {0} and {1}", DAConstants.KeySeparatorLeft, DAConstants.KeySeparatorRight));

            return String.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}{4}{5}", DAConstants.KeySeparatorLeft, this.Namespace, DAConstants.KeySeparatorRight,
                                                       DAConstants.KeySeparatorLeft, rowKeyStr, DAConstants.KeySeparatorRight);
        }
        #endregion

        #region static methods

        /// <summary>
        /// local cache of DAClass info to boost performance
        /// </summary>
        private static Dictionary<Type, DAClassInfo> DAClassCache = new Dictionary<Type, DAClassInfo>();

        /// <summary>
        /// Get the class info of a customer object
        /// </summary>
        public static DAClassInfo GetDAClassInfo(object customerObject)
        {
            Type T = customerObject.GetType();

            return GetDAClassInfo(T);
        }

        /// <summary>
        /// Get the class info of a type
        /// </summary>
        public static DAClassInfo GetDAClassInfo<T>()
        {
            return GetDAClassInfo(typeof(T));
        }

        /// <summary>
        /// Get the calss info of a type
        /// </summary>
        public static DAClassInfo GetDAClassInfo(Type T)
        {
            DAClassInfo classInfo;

            if (DAClassCache.ContainsKey(T))
            {
                Trace.TraceInformation("Get class info from cache:{0}", T);
                classInfo = DAClassCache[T];
            }
            else
            {
                lock (DAClassCache)
                {
                    if (DAClassCache.ContainsKey(T))
                    {
                        Trace.TraceInformation("Get class info from cache 2nd try:{0}", T);
                        classInfo = DAClassCache[T];
                    }
                    else
                    {
                        classInfo = new DAClassInfo(T);
                        DAClassCache[T] = classInfo;
                    }
                }
            }
            return classInfo;
        }
        #endregion
    }

    public enum SerializerType
    {
        DataContractorSerializer = 1
    }

    public enum TableName
    {
        DAObject,
        UserInformation,
        TripInformation,
        LocationInfo,
        Destination // Table to keep a map of all the locations with a desination
    }

    public enum MessageQueueName
    {
        DAQueue
        /*ProcessedSearchData = 1,
        DailyUserActivity*/
    }
}
