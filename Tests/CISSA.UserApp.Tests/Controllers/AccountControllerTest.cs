using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Intersoft.CISSA.UserApp.Controllers;
using Intersoft.CISSA.UserApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CISSA.UserApp.Tests.Controllers
{
    [TestClass]
    public class AccountControllerTest
    {

        [TestMethod]
        public void ChangePasswordGetReturnsView()
        {
            // Arrange
            AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.ChangePassword();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(10, ((ViewResult)result).ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void ChangePasswordPostReturnsRedirectOnSuccess()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new ChangePasswordModel
            {
                OldPassword = "goodOldPassword",
                NewPassword = "goodNewPassword",
                ConfirmPassword = "goodNewPassword"
            };

            // Act
            ActionResult result = controller.ChangePassword(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("ChangePasswordSuccess", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void ChangePasswordPostReturnsViewIfChangePasswordFails()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new ChangePasswordModel
            {
                OldPassword = "goodOldPassword",
                NewPassword = "badNewPassword",
                ConfirmPassword = "badNewPassword"
            };

            // Act
            ActionResult result = controller.ChangePassword(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual("Текущий пароль или новый пароль введены неверно. Повторите ввод еще раз.", controller.ModelState[""].Errors[0].ErrorMessage);
            Assert.AreEqual(10, viewResult.ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void ChangePasswordPostReturnsViewIfModelStateIsInvalid()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new ChangePasswordModel
            {
                OldPassword = "goodOldPassword",
                NewPassword = "goodNewPassword",
                ConfirmPassword = "goodNewPassword"
            };
            controller.ModelState.AddModelError("", "Dummy error message.");

            // Act
            ActionResult result = controller.ChangePassword(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual(10, viewResult.ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void ChangePasswordSuccessReturnsView()
        {
            // Arrange
            AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.ChangePasswordSuccess();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void LogOffLogsOutAndRedirects()
        {
            // Arrange
            AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.LogOff();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Home", redirectResult.RouteValues["controller"]);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            Assert.IsTrue(((MockFormsAuthenticationService)controller.FormsService).SignOutWasCalled);
        }

        [TestMethod]
        public void LogOnGetReturnsView()
        {
            // Arrange
            AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.LogOn();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public void LogOnPostReturnsRedirectOnSuccessWithoutReturnUrl()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new LogOnModel
            {
                UserName = "someUser",
                Password = "goodPassword",
                RememberMe = false
            };

            // Act
            ActionResult result = controller.LogOn(model, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Home", redirectResult.RouteValues["controller"]);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            Assert.IsTrue(((MockFormsAuthenticationService)controller.FormsService).SignInWasCalled);
        }

        [TestMethod]
        public void LogOnPostReturnsRedirectOnSuccessWithLocalReturnUrl()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new LogOnModel
            {
                UserName = "someUser",
                Password = "goodPassword",
                RememberMe = false
            };

            // Act
            ActionResult result = controller.LogOn(model, "/someUrl");

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            var redirectResult = (RedirectResult)result;
            Assert.AreEqual("/someUrl", redirectResult.Url);
            Assert.IsTrue(((MockFormsAuthenticationService)controller.FormsService).SignInWasCalled);
        }

        [TestMethod]
        public void LogOnPostReturnsRedirectToHomeOnSuccessWithExternalReturnUrl()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new LogOnModel
            {
                UserName = "someUser",
                Password = "goodPassword",
                RememberMe = false
            };

            // Act
            ActionResult result = controller.LogOn(model, "http://malicious.example.net");

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Home", redirectResult.RouteValues["controller"]);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            Assert.IsTrue(((MockFormsAuthenticationService)controller.FormsService).SignInWasCalled);
        }

        [TestMethod]
        public void LogOnPostReturnsViewIfModelStateIsInvalid()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new LogOnModel
            {
                UserName = "someUser",
                Password = "goodPassword",
                RememberMe = false
            };
            controller.ModelState.AddModelError("", "Dummy error message.");

            // Act
            ActionResult result = controller.LogOn(model, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
        }

        [TestMethod]
        public void LogOnPostReturnsViewIfValidateUserFails()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new LogOnModel
            {
                UserName = "someUser",
                Password = "badPassword",
                RememberMe = false
            };

            // Act
            ActionResult result = controller.LogOn(model, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual("Логин или пароль пользователя введены неверно. Повторите ввод еще раз.", controller.ModelState[""].Errors[0].ErrorMessage);
        }

        [TestMethod]
        public void RegisterGetReturnsView()
        {
            // Arrange
            AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.Register();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(10, ((ViewResult)result).ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void RegisterPostReturnsRedirectOnSuccess()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new RegisterModel
            {
                UserName = "someUser",
                Email = "goodEmail",
                Password = "goodPassword",
                ConfirmPassword = "goodPassword"
            };

            // Act
            ActionResult result = controller.Register(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            var redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Home", redirectResult.RouteValues["controller"]);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
        }

        [TestMethod]
        public void RegisterPostReturnsViewIfRegistrationFails()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new RegisterModel
            {
                UserName = "duplicateUser",
                Email = "goodEmail",
                Password = "goodPassword",
                ConfirmPassword = "goodPassword"
            };

            // Act
            ActionResult result = controller.Register(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual("Пользователь с таким именем уже существует. Пожалуйста, введите другое.", controller.ModelState[""].Errors[0].ErrorMessage);
            Assert.AreEqual(10, viewResult.ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void RegisterPostReturnsViewIfModelStateIsInvalid()
        {
            // Arrange
            AccountController controller = GetAccountController();
            var model = new RegisterModel
            {
                UserName = "someUser",
                Email = "goodEmail",
                Password = "goodPassword",
                ConfirmPassword = "goodPassword"
            };
            controller.ModelState.AddModelError("", "Dummy error message.");

            // Act
            ActionResult result = controller.Register(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual(10, viewResult.ViewData["PasswordLength"]);
        }

        private static AccountController GetAccountController()
        {
            var requestContext = new RequestContext(new MockHttpContext(), new RouteData());
            var controller = new AccountController
            {
                FormsService = new MockFormsAuthenticationService(),
                MembershipService = new MockMembershipService(),
                Url = new UrlHelper(requestContext),
            };
            controller.ControllerContext = new ControllerContext
            {
                Controller = controller,
                RequestContext = requestContext
            };
            return controller;
        }

        private class MockFormsAuthenticationService : IFormsAuthenticationService
        {
            public bool SignInWasCalled;
            public bool SignOutWasCalled;

            public void SignIn(string userName, bool createPersistentCookie)
            {
                // verify that the arguments are what we expected
                Assert.AreEqual("someUser", userName);
                Assert.IsFalse(createPersistentCookie);

                SignInWasCalled = true;
            }

            public void SignOut()
            {
                SignOutWasCalled = true;
            }
        }

        private class MockHttpContext : HttpContextBase
        {
            private readonly IPrincipal _user = new GenericPrincipal(new GenericIdentity("someUser"), null /* roles */);
            private readonly HttpRequestBase _request = new MockHttpRequest();

            public override IPrincipal User
            {
                get
                {
                    return _user;
                }
                set
                {
                    base.User = value;
                }
            }

            public override HttpRequestBase Request
            {
                get
                {
                    return _request;
                }
            }
        }

        private class MockHttpRequest : HttpRequestBase
        {
            private readonly Uri _url = new Uri("http://mysite.example.com/");

            public override Uri Url
            {
                get
                {
                    return _url;
                }
            }
        }

        private class MockMembershipService : IMembershipService
        {
            public int MinPasswordLength
            {
                get { return 10; }
            }

            public bool ValidateUser(string userName, string password)
            {
                return (userName == "someUser" && password == "goodPassword");
            }

            public MembershipCreateStatus CreateUser(string userName, string password, string email)
            {
                if (userName == "duplicateUser")
                {
                    return MembershipCreateStatus.DuplicateUserName;
                }

                // verify that values are what we expected
                Assert.AreEqual("goodPassword", password);
                Assert.AreEqual("goodEmail", email);

                return MembershipCreateStatus.Success;
            }

            public bool ChangePassword(string userName, string oldPassword, string newPassword)
            {
                return (userName == "someUser" && oldPassword == "goodOldPassword" && newPassword == "goodNewPassword");
            }
        }

    }
}
