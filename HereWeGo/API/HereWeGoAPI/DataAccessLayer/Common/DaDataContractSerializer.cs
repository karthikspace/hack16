using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    public class DaDataContractSerializer:IDaSerializer
    {
        private DataContractSerializer internalSerializer;
        private static XmlWriterSettings SerializeSetting = new XmlWriterSettings()
        {
            OmitXmlDeclaration = true
        };

        public DaDataContractSerializer(Type type)
        {
            internalSerializer = new DataContractSerializer(type);
        }

        #region IDaSerializer Members

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security.Xml", "CA3054:DoNotAllowDtdOnXmlTextReader")]
        public object Deserialize(string xmlString)
        {
            Trace.TraceInformation("DaDataContractSerializer Start Deserialize");
            try
            {
                using (StringReader strReader = new StringReader(xmlString))
                {
                    using (XmlTextReader reader = new XmlTextReader(strReader))
                    {
                        object obj = internalSerializer.ReadObject(reader);
                        Trace.TraceInformation("DaDataContractSerializer Finished Deserialize");
                        return obj;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Errors happened when trying to Deserialize object.");
                throw new Exception("Errors happened when trying to Deserialize object.", ex);
            }
        }

        public string Serialize(object obj)
        {
            return Serialize(obj, null);
        }

        public string Serialize(object obj, XmlWriterSettings settings)
        {
            if (null == settings)
                settings = SerializeSetting;

            try
            {
                String serializedString;
                Trace.TraceInformation("DaDataContractSerializer Start Serialize.");

                StringBuilder sbSerialized = new StringBuilder();
                using (XmlWriter writer = XmlWriter.Create(sbSerialized, settings))
                {
                    internalSerializer.WriteObject(writer, obj);
                    writer.Flush();
                    serializedString = sbSerialized.ToString();
                }
                Trace.TraceInformation("DaDataContractSerializer Finished Serialize");

                return serializedString;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Errors happened when trying to serialize object.");
                throw new Exception("Errors happened when trying to serialize object.", ex);
            }
        }

        #endregion
    }
}
