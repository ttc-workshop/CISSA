using Intersoft.CISSA.BizServiceTests.FakeRepo;
using Intersoft.CISSA.DataAccessLayer.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.BizServiceTests
{
    /// <summary>
    /// Сводное описание для UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        private static BizService.BizService GetService()
        {
            var userRepo = new FakeUserRepository();
            return new BizService.BizService(userRepo, "TestUserName");
        }

        [TestMethod]
        public void UserInfo()
        {
            var service = GetService();

            UserInfo userInfo = service.GetUserInfo();

            Assert.IsNotNull(userInfo);
        }

        [TestMethod]
        public void ChangePassword()
        {
            var service = GetService();

            BizResult bizResult = service.ChangeUserPassword("123", "123");

            Assert.IsNotNull(bizResult);
            Assert.AreEqual(BizResult.BizResultType.Message , bizResult.Type);
            Assert.AreEqual("Ok.", bizResult.Message);
        }
    }
}
