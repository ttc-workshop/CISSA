using System;
using Intersoft.CISSA.BizServiceTests.FakeRepo;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.BizServiceTests
{
    /// <summary>
    /// Сводное описание для UnitTest1
    /// </summary>
    [TestClass]
    public class BizServcieDocTests
    {
        private static BizService.BizService GetService()
        {
            var fakeDocRepo = new FakeDocRepository();
            return new BizService.BizService(fakeDocRepo, "TestUserName");
        }

        [TestMethod]
        public void DocumentLoad()
        {
            BizService.BizService service = GetService();

            Doc doc = service.DocumentLoad(Guid.NewGuid());

            Assert.IsNotNull(doc);
        }

        [TestMethod]
        public void DocumentNew()
        {
            BizService.BizService service = GetService();

            Doc doc = service.DocumentNew(Guid.NewGuid());

            Assert.IsNotNull(doc);
        }

        [TestMethod]
        public void DocumentSave()
        {
            BizService.BizService service = GetService();

            var docForSave = new Doc();

            Doc doc = service.DocumentSave(docForSave);

            Assert.IsNotNull(doc);
        }

        [TestMethod]
        public void DocumentDelete()
        {
            BizService.BizService service = GetService();

            service.DocumentDelete(Guid.NewGuid());
        }
    }
}
