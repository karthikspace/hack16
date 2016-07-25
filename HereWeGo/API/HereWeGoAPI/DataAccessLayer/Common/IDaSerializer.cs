using System;
using System.Xml;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    public interface IDaSerializer
    {
        Object Deserialize(String xmlString);

        /// <summary>
        /// use default xmlsetting to serialize, can same some space
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        String Serialize(Object obj);

        /// <summary>
        /// Serialize with a xmlsetting object, usually used in places that
        /// need display the serialized xml string
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        String Serialize(Object obj, XmlWriterSettings settings);
    } 
}
