using System;
using System.Web.Mvc;
using Intersoft.CISSA.UserApp.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CISSA.UserApp.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        private static HomeController GetController()
        {
            var fakePresentationManager = new FakePresentationManager();
            
            return new HomeController(/*fakePresentationManager*/);
        }

        [TestMethod]
        public void Index()
        {
            // Arrange
            var controller = GetController();

            // Act
            var result = controller.Index() as RedirectResult; //ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void About()
        {
            var controller = GetController();

            var result = controller.About() as ViewResult;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Edit()
        {
            var controller = GetController();

            var result = controller.ShowAbout() as ViewResult;

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Edit2()
        {
            var controller = GetController();

//            var result = controller.Edit(Guid.NewGuid(), "Some string value", DateTime.Now) as ViewResult;
            var result = controller.ShowAbout() as ViewResult;

            Assert.IsNotNull(result);
        }
    }
}
