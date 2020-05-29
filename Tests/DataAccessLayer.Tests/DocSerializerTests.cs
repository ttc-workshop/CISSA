using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    [TestClass]
    public class DocSerializerTests
    {
        private static Doc GetDoc()
        {
            var doc = new Doc
            {
                Id = Guid.Parse("{D9713AEE-6857-4A01-B3BA-E160B8844B91}"),
                //всегда пишем в один и тот же документ
                DocDef = new DocDef { Id = Guid.Parse("ED9D1436-B4C4-4AC9-97CD-F142BA48F43A"), Name = "DocWithHistory" },
                Attributes = new List<AttributeBase>()
            };

            var floatAttribute = new FloatAttribute(
                new AttrDef { Id = Guid.Parse("{9B1C8BA9-EC8A-4D7E-8542-178302452EE8}"), Name = "FloatAttr" })
            {
                Value = (new Random()).Next() / 3.333
            };

            var intAttribute =
                new IntAttribute(new AttrDef
                {
                    Id = Guid.Parse("{7F7C106F-A7C4-4802-BC4C-EA69DCF23A9A}"),
                    Name = "IntAttr"
                })
                {
                    Value = (new Random()).Next()
                };

            var textAttribute = new TextAttribute(
                new AttrDef { Id = Guid.Parse("{6DA77C5F-3727-4DDF-86A7-BE8248C4AC23}"), Name = "TextAttr" })
            {
                Value = "Som random text and ranodm value is " + (new Random()).Next()
            };


            var dateTimeAttribute = new DateTimeAttribute(
                new AttrDef { Id = Guid.Parse("2C43F420-3875-43CE-A7C6-9C8811B7C824"), Name = "DateTimeAttr" })
            {
                Value = DateTime.Now.AddYears(5)
            };

            doc.Attributes.Add(floatAttribute);
            doc.Attributes.Add(intAttribute);
            doc.Attributes.Add(textAttribute);
            doc.Attributes.Add(dateTimeAttribute);

            return doc;
        }

        [TestMethod]
        public void Serialize()
        {
            using (var dataContext = new DataContext())
            {
                var doc = GetDoc();

                var serializer = new DocSerializer(dataContext);
                string serDoc = serializer.Serialize(new List<Doc> {doc});

                Console.WriteLine(serDoc);

                Assert.IsNotNull(serDoc);
                Assert.AreNotEqual("", serDoc);
            }
        }

        [TestMethod]
        public void DeSerialize()
        {
            using (var dataContext = new DataContext())
            {
                var doc = GetDoc();

                var serializer = new DocSerializer(dataContext);
                string xml = serializer.Serialize(new List<Doc> {doc});

                IList<Doc> doc2 = serializer.DeSerialize(xml);

                Assert.IsNotNull(doc2);
            }
        }

        [TestMethod]
        public void DeSerialize2()
        {
            const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                               "   <Docs> " +
                               "       <Doc Id=\"ed9d1436-b4c4-4ac9-97cd-f142ba48f43a\" Name=\"DocWithHistory\"> " +
                               "       <Attribute Id=\"9b1c8ba9-ec8a-4d7e-8542-178302452ee8\" Name=\"FloatAttr\">24327465,6465647</Attribute> " +
                               "       <Attribute Id=\"7f7c106f-a7c4-4802-bc4c-ea69dcf23a9a\" Name=\"IntAttr\">81083443</Attribute> " +
                               "       <Attribute Id=\"6da77c5f-3727-4ddf-86a7-be8248c4ac23\" Name=\"TextAttr\">Som random text and ranodm value is 81083443</Attribute> " +
                               "       <Attribute Id=\"2c43f420-3875-43ce-a7c6-9c8811b7c824\" Name=\"DateTimeAttr\">08.06.2016 23:44:42</Attribute> " +
                               "       </Doc> " +
                               "   </Docs> ";

            using (var dataContext = new DataContext())
            {
                var serializer = new DocSerializer(dataContext);

                IList<Doc> doc2 = serializer.DeSerialize(xml);

                Assert.IsNotNull(doc2);
            }
        }

        [TestMethod]
        public void DeSerializeWithoutAttributeId()
        {
            const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                               "   <Docs> " +
                               "       <Doc Id=\"ed9d1436-b4c4-4ac9-97cd-f142ba48f43a\" Name=\"DocWithHistory\"> " +
                               "       <Attribute Name=\"FloatAttr\">24327465,6465647</Attribute> " +
                               "       <Attribute Name=\"IntAttr\">81083443</Attribute> " +
                               "       <Attribute Name=\"TextAttr\">Som random text and ranodm value is 81083443</Attribute> " +
                               "       <Attribute Name=\"DateTimeAttr\">08.06.2016 23:44:42</Attribute> " +
                               "       </Doc> " +
                               "   </Docs> ";

            using (var dataContext = new DataContext())
            {
                var serializer = new DocSerializer(dataContext);

                IList<Doc> doc2 = serializer.DeSerialize(xml);

                Assert.IsNotNull(doc2);
            }
        }


        [TestMethod]
        public void DeSerializeWithoutDocId()
        {
            const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                               "   <Docs> " +
                               "       <Doc Name=\"DocWithHistory\"> " +
                               "       <Attribute Name=\"FloatAttr\">24327465,6465647</Attribute> " +
                               "       <Attribute Name=\"IntAttr\">81083443</Attribute> " +
                               "       <Attribute Name=\"TextAttr\">Som random text and ranodm value is 81083443</Attribute> " +
                               "       <Attribute Name=\"DateTimeAttr\">08.06.2016 23:44:42</Attribute> " +
                               "       </Doc> " +
                               "   </Docs> ";

            using (var dataContext = new DataContext())
            {
                var serializer = new DocSerializer(dataContext);

                IList<Doc> doc2 = serializer.DeSerialize(xml);

                Assert.IsNotNull(doc2);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Тип документа не указан")]
        public void DeSerializeWithoutTypeOfDoc()
        {
            const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                               "   <Docs> " +
                               "       <Doc> " +
                               "       <Attribute Id=\"9b1c8ba9-ec8a-4d7e-8542-178302452ee8\" Name=\"FloatAttr\">24327465,6465647</Attribute> " +
                               "       <Attribute Id=\"7f7c106f-a7c4-4802-bc4c-ea69dcf23a9a\" Name=\"IntAttr\">81083443</Attribute> " +
                               "       <Attribute Id=\"6da77c5f-3727-4ddf-86a7-be8248c4ac23\" Name=\"TextAttr\">Som random text and ranodm value is 81083443</Attribute> " +
                               "       <Attribute Id=\"2c43f420-3875-43ce-a7c6-9c8811b7c824\" Name=\"DateTimeAttr\">08.06.2016 23:44:42</Attribute> " +
                               "       </Doc> " +
                               "   </Docs> ";

            using (var dataContext = new DataContext())
            {
                var serializer = new DocSerializer(dataContext);

                IList<Doc> doc2 = serializer.DeSerialize(xml);
            }
        }

        [TestMethod]
        public void DeSerializeByDocDefName()
        {
            const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                               "   <Docs> " +
                               "       <Doc Name=\"DocWithHistory\"> " +
                               "       <Attribute Id=\"9b1c8ba9-ec8a-4d7e-8542-178302452ee8\" Name=\"FloatAttr\">24327465,6465647</Attribute> " +
                               "       <Attribute Id=\"7f7c106f-a7c4-4802-bc4c-ea69dcf23a9a\" Name=\"IntAttr\">81083443</Attribute> " +
                               "       <Attribute Id=\"6da77c5f-3727-4ddf-86a7-be8248c4ac23\" Name=\"TextAttr\">Som random text and ranodm value is 81083443</Attribute> " +
                               "       <Attribute Id=\"2c43f420-3875-43ce-a7c6-9c8811b7c824\" Name=\"DateTimeAttr\">08.06.2016 23:44:42</Attribute> " +
                               "       </Doc> " +
                               "   </Docs> ";

            using (var dataContext = new DataContext())
            {
                var serializer = new DocSerializer(dataContext);

                IList<Doc> doc2 = serializer.DeSerialize(xml);

                Assert.IsNotNull(doc2);
            }
        }

        [TestMethod]
        public void DeSerializeEmpty()
        {
            const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                               "   <Docs> " +
                               "   </Docs> ";

            using (var dataContext = new DataContext())
            {
                var serializer = new DocSerializer(dataContext);

                IList<Doc> doc2 = serializer.DeSerialize(xml);

                Assert.IsNotNull(doc2);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Невозможно определить атрибут")]
        public void DeSerializeEmty()
        {
            const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                               "   <Docs> " +
                               "       <Doc Name=\"DocWithHistory\"> " +
                               "         <Attribute>24327465,6465647</Attribute> " + 
                               "       </Doc> " +
                               "   </Docs> ";

            using (var dataContext = new DataContext())
            {
                var serializer = new DocSerializer(dataContext);

                IList<Doc> doc2 = serializer.DeSerialize(xml);

                Assert.IsNotNull(doc2);
            }
        }
    }
}
