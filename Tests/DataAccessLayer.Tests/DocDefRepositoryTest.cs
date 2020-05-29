using System;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    [TestClass]
    public class DocDefRepositoryTest
    {
        [TestMethod]
        public void DocDefAttributesForEmptyUserId()
        {
            using (var repo = new DocDefRepository(Guid.Empty))
            {
                var items = repo.DocDefById(Guid.Parse("{846B1B55-F110-452F-B08F-8CEB0A112BE0}")).Attributes;
                    //repo.GetDocumentAttributes(Guid.Parse("{846B1B55-F110-452F-B08F-8CEB0A112BE0}"), Guid.Empty);

                Assert.IsNotNull(items);
                Assert.AreEqual(5, items.Count());
            }
        }

        [TestMethod]
        public void DocDefAttributes()
        {
            using (var repo = new DocDefRepository(Guid.Parse("180B1E71-6CDA-4887-9F83-941A12D7C979")))
            {
                var items = repo.DocDefById(Guid.Parse("846B1B55-F110-452F-B08F-8CEB0A112BE0")).Attributes;
                    /*repo.GetDocumentAttributes(
                    Guid.Parse("846B1B55-F110-452F-B08F-8CEB0A112BE0"),
                    Guid.Parse("180B1E71-6CDA-4887-9F83-941A12D7C979"));*/

                Assert.IsNotNull(items);
                Assert.AreEqual(4, items.Count());
            }
        }
    


        [TestMethod]
        public void GetDocDefDescendant()
        {
            using (var repo = new DocDefRepository())
            {
                var items = repo.GetDocDefDescendant(Guid.Parse("{C59A57D2-86F7-440F-BCF0-8DCC252B8C1F}"));

                Assert.IsNotNull(items);
                Assert.AreEqual(2, items.Count());
            }
        }


        [TestMethod]
        public void DocDefById()
        {
            using (var repo = new DocDefRepository())
            {
                DocDef items = repo.DocDefById(Guid.Parse("{846B1B55-F110-452F-B08F-8CEB0A112BE0}"));

                Assert.IsNotNull(items);
            }
        }

        [TestMethod]
        public void DocDefByName()
        {
            using (var repo = new DocDefRepository())
            {
                DocDef items = repo.DocDefByName("TestDoc");

                Assert.IsNotNull(items);
            }
        }
        
    }
}
