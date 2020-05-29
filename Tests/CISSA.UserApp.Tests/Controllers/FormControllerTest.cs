using System;
using System.Web.Mvc;
using System.Web.Routing;
using CISSA.UserApp.Tests.Mock;
using Intersoft.CISSA.UserApp.Controllers;
using Intersoft.CISSA.UserApp.ServiceReference;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CISSA.UserApp.Tests.Controllers
{
    [TestClass]
    public class FormControllerTest
    {
        private static FormController GetController()
        {
            var fakePresentationManager = new FakeFormRepository();
            var fakeDocManager = new FakeDocManager();

            var controller = new FormController(/*fakePresentationManager, fakeDocManager*/);
            
            var controllerContext = new ControllerContext(
                new MockHttpContext(), new RouteData(), controller);

            controller.ControllerContext = controllerContext;

            return controller;
        }

        //[TestMethod]
        //public void Show()
        //{
        //    // Arrange
        //    var controller = GetController();

        //    // Act
        //    var result = controller.ShowDoc(Guid.NewGuid(), Guid.NewGuid()) as ViewResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //}

        [TestMethod]
        public void TableShow()
        {
            // Arrange
            var controller = GetController();

            // Act
            var result = controller.ShowList(Guid.NewGuid(), null) as RedirectResult; //ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        //[TestMethod]
        //public void Save()
        //{
        //    // Arrange
        //    var controller = GetController();

        //    controller.Session["Form"] = new BizForm { Id = Guid.NewGuid() };

        //    var fields = new FormCollection
        //                     {
        //                         {"00000000-0000-0000-0000-000000000001", "123"},
        //                         {"00000000-0000-0000-0000-000000000002", "dfff"},
        //                         {"00000000-0000-0000-0000-000000000003", "dsfasd"}
        //                     };
        //    // Act
        //    var result = controller.Save(fields) as RedirectToRouteResult;

        //    // Assert
        //    Assert.IsNotNull(result);
        //}

        [TestMethod]
        public void CheckField()
        {
            // Arrange
            var controller = GetController();

            // Act
            JsonResult result = controller.CheckField("00000000-0000-0000-0000-000000000001", "123") ;

            // Assert
            Assert.IsNotNull(result);
        }

    }
}
