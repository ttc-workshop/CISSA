using System;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Repository;
using ProtoBuf;

namespace ConsoleApplication1.Tests
{
    public static class Serialization
    {
        public static string SerializeForm(BizControl form)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize<BizControl>(stream, form);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }

        public static BizControl DeserializeForm(string data)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(data);
                    writer.Flush();
                    stream.Position = 0;
                    return Serializer.Deserialize<BizControl>(stream);
                }
            }
        }

        public static void Execute()
        {
            using (var formRepo = new FormRepository((IDataContext) null))
            {
                var form = formRepo.GetForm(new Guid("{90958557-E6B0-40A8-88D8-75B71130D5FC}"));

                var s = SerializeForm(form);
                Console.WriteLine(s);

                var result = DeserializeForm(s);
                Console.WriteLine(result.GetType().Name);
            }
        }
    }
}
