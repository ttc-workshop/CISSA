using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    [TestClass]
    public class DocDynamicTests
    {
        private readonly Guid _userId = Guid.Parse("180B1E71-6CDA-4887-9F83-941A12D7C979");

        [TestMethod]
        public void Full()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("2194385f-befc-452f-ab7c-19dd5f033225");

                dynamic doc = new DynaDoc(rep.New(docDefId), Guid.Empty, dataContext);

                doc.CurAttr = 777;
                doc.DocAttr = Guid.Parse("{7A25F0EC-0874-4CEC-9130-45A2199F0000}");
                doc.EnumAttr = Guid.Parse("{7A25F0EC-0874-4CEC-9130-45A2199F0001}");
                doc.FloatAttr = 444.4;
                doc.IntAttr = 234;
                doc.StrAttr = "Some text";

                var docList = (List<Guid>) doc.DocListAttr;
                docList.Add(Guid.NewGuid());
                docList.Add(Guid.NewGuid());
                docList.Add(Guid.NewGuid());

                Assert.AreEqual(3, ((List<Guid>) doc.DocListAttr).Count);

                Assert.AreEqual(777m, doc.CurAttr);
                Assert.AreEqual(Guid.Parse("{7A25F0EC-0874-4CEC-9130-45A2199F0000}"), doc.DocAttr);
                Assert.AreEqual(Guid.Parse("{7A25F0EC-0874-4CEC-9130-45A2199F0001}"), doc.EnumAttr);
                Assert.AreEqual(444.4, doc.FloatAttr);
                Assert.AreEqual(234, doc.IntAttr);
                Assert.AreEqual("Some text", doc.StrAttr);
            }
        }

        [TestMethod]
        public void DocList()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("2194385f-befc-452f-ab7c-19dd5f033225");

                dynamic doc = new DynaDoc(rep.New(docDefId), Guid.Empty, dataContext);

                var docList = (List<Guid>) doc.DocListAttr;
                docList.Add(Guid.NewGuid());
                docList.Add(Guid.NewGuid());
                docList.Add(Guid.NewGuid());

                Assert.AreEqual(3, ((List<Guid>) doc.DocListAttr).Count);
            }
        }

        [TestMethod]
        public void SimpleTypes()
        {
            using (var dataContext = new DataContext())
            {
                var rep = new DocRepository(dataContext, _userId);
                var docDefId = Guid.Parse("2194385f-befc-452f-ab7c-19dd5f033225");

                dynamic doc = new DynaDoc(rep.New(docDefId), Guid.Empty, dataContext);

                doc.CurAttr = 777;
                doc.DocAttr = Guid.Parse("{7A25F0EC-0874-4CEC-9130-45A2199F0000}");
                doc.EnumAttr = Guid.Parse("{7A25F0EC-0874-4CEC-9130-45A2199F0001}");
                doc.FloatAttr = 444.4;
                doc.IntAttr = 234;
                doc.StrAttr = "Some text";

                Assert.AreEqual(777m, doc.CurAttr);
                Assert.AreEqual(Guid.Parse("{7A25F0EC-0874-4CEC-9130-45A2199F0000}"), doc.DocAttr);
                Assert.AreEqual(Guid.Parse("{7A25F0EC-0874-4CEC-9130-45A2199F0001}"), doc.EnumAttr);
                Assert.AreEqual(444.4, doc.FloatAttr);
                Assert.AreEqual(234, doc.IntAttr);
                Assert.AreEqual("Some text", doc.StrAttr);
            }
        }
    }
}
