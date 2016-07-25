using System;

namespace Microsoft.RewardsIntl.Platform.DataAccess.Common
{
    public class DaSerializerFactory
    {
        public static IDaSerializer CreateSerializer(Type type, SerializerType serializerType)
        {
            switch (serializerType)
            {
                case SerializerType.DataContractorSerializer:
                    return new DaDataContractSerializer(type);
            }
            return null;
        }
    }
}