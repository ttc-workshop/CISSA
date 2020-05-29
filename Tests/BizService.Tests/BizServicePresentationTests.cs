using System;
using Intersoft.CISSA.BizServiceTests.FakeRepo;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.BizServiceTests
{
    /// <summary>
    /// Сводное описание для BizServicePresentationTests
    /// </summary>
    [TestClass]
    public class BizServicePresentationTests
    {
        private static BizService.BizService GetService()
        {
            var formRepo = new FakeFormRepository();
            return new BizService.BizService(formRepo, "TestUserName");
        }

        [TestMethod]
        public void GetForm()
        {
            var service = GetService();

//            BizForm form = service.GetForm(Guid.NewGuid(), Guid.NewGuid());
            BizForm form = service.GetAnyForm(Guid.NewGuid()/*, Guid.NewGuid()*/);

            Assert.IsNotNull(form);
        }

        [TestMethod]
        public void GetDetailForm()
        {
            var service = GetService();

            BizForm form = service.GetDetailForm(Guid.NewGuid());

            Assert.IsNotNull(form);
        }

        [TestMethod]
        public void GetTableForm()
        {
            var service = GetService();

            var form = service.GetGridForm(Guid.NewGuid()/*, null*/);

            Assert.IsNotNull(form);
        }

        //[TestMethod]
        //public void GetMainForm()
        //{
        //    var service = GetService();

        //    BizForm form = service.GetMainForm();

        //    Assert.IsNotNull(form);
        //}

        //[TestMethod]
        //public void ExecuteAction()
        //{
        //    var service = GetService();

        //    BizResult form = service.ExecuteBizAction(Guid.NewGuid());

        //    Assert.IsNotNull(form);
        //}
    }
}
