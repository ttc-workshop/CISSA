using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Intersoft.CISSA.DataAccessLayer.Utils
{
    public static class ByteArrayHelper
    {
        public static byte[] ConvertFrom(object obj)
        {
            if (obj == null) return null;

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
