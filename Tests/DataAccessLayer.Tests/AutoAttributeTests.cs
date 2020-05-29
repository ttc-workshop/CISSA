using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Documents.AutoAttr;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    [TestClass]
    public class AutoAttributeTests
    {
        private readonly Guid _userId = Guid.Parse("180b1e71-6cda-4887-9f83-941a12d7c979");

        [TestMethod]
        public void AutoAttributeScriptManager()
        {
            const string script = "{ return 2 + 2; }";
            var scriptManager = new AutoAttributeScriptManager(script);
            var context = new AutoAttributeContext();

            Assert.AreEqual(4, scriptManager.Execute(context));
        }

        [TestMethod]
        public void ScriptTest()
        {
            var doc = new Doc
            {
                Id = Guid.Parse("{D9713AEE-6857-4A01-B3BA-E160B8844B91}"),
                //всегда пишем в один и тот же документ
                DocDef = new DocDef { Id = Guid.Parse("ED9D1436-B4C4-4AC9-97CD-F142BA48F43A") },
                Attributes = new List<AttributeBase>()
            };

            var floatAttribute = new FloatAttribute(
                                             new AttrDef
                                                 {
                                                     Id = Guid.Parse("{9B1C8BA9-EC8A-4D7E-8542-178302452EE8}"),
                                                     Name = "SomeFloatAttribute"
                                                 })
                                             {
                                                 Value = 10
                                             };

            var intAttribute = new IntAttribute(
                new AttrDef
                    {
                        Id = Guid.Parse("{7F7C106F-A7C4-4802-BC4C-EA69DCF23A9A}"),
                        Name = "SomeIntAttribute"
                    })
            {
                Value = 20
            };

            var autoAttr = new AutoAttribute(
                new AttrDef
                    {
                        Id = Guid.Parse("{00000000-0000-0000-0000-000000000000}"),
                        Name = "SomeAutoAttribute",
                        Script = "{ return context.CurrentDoc[\"SomeFloatAttribute\"].ToString() + '-' + context.CurrentDoc[\"SomeIntAttribute\"].ToString(); }"
                    }
                );
            
            doc.Attributes.Add(floatAttribute);
            doc.Attributes.Add(intAttribute);
            doc.Attributes.Add(autoAttr);

            using (var rep = new DocRepository(_userId))
                rep.CalculateAutoAttributes(doc);

            Assert.AreEqual("10-20", doc["SomeAutoAttribute"]);
        }

        [TestMethod]
        public void DocNew()
        {
            using (var rep = new DocRepository(_userId))
            {
                var docDefId = Guid.Parse("{846B1B55-F110-452F-B08F-8CEB0A112BE0}");

                Doc doc = rep.New(docDefId);

                Assert.IsNotNull(doc);
                Assert.IsNotNull(doc.AttrAuto);
                Assert.AreNotEqual(0, doc.AttrAuto.Count());
            }
        }


        [TestMethod]
        public void DocLoad()
        {
            using (var rep = new DocRepository(_userId))
            {
                var docId = Guid.Parse("B6B8968D-0837-45F5-8B7D-E1682DAEFF81");

                Doc doc = rep.LoadById(docId, new DateTime(2011, 6, 14));

                Assert.IsNotNull(doc);
                Assert.IsNotNull(doc.AttrAuto);
                Assert.AreNotEqual(0, doc.AttrAuto.Count());
                Assert.AreEqual("3-asdf afsdf", doc["MyAuto"]);
            }
        }
    }
}
