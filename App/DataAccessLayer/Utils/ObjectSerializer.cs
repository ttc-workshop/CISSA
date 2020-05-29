using System.IO;
//using ProtoBuf;

namespace Intersoft.CISSA.DataAccessLayer.Utils
{
    public static class ObjectSerializer
    {
        /*public static T Deserialize<T>(string data)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(data);
                    writer.Flush();
                    stream.Position = 0;
                    return Serializer.Deserialize<T>(stream);
                }
            }
        }

        public static string Serialize<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize<T>(stream, obj);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }*/

    }

    public interface IObjectCloner<T>
    {
        T Clone(T source);
    }
}
