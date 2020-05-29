using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    /// <summary>
    ///Это класс теста для UserRepositoryTest, в котором должны
    ///находиться все модульные тесты UserRepositoryTest
    ///</summary>
    [TestClass()]
    public class UserRepositoryTest
    {
        public const string DbConnectioinString =
            "Data Source=195.38.189.100;Initial Catalog=cissa-with-children;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        /// <summary>
        ///Тест для ChangeUserPassword
        ///</summary>
        [TestMethod]
        public void ChangeUserPasswordTest()
        {
            var factory = DataContextFactoryProvider.GetFactory();
            using (var dataContext = factory.CreateDc(DbConnectioinString))
            {
                var target = new UserRepository(dataContext);

                BizResult res = target.ChangeUserPassword("TestUser", "123", "123");

                Assert.IsNotNull(res);
                Assert.AreEqual("Смена пароля прошла успешно.", res.Message);
            }
        }

        [TestMethod]
        public void ChangeUserPasswordException()
        {
            var target = new UserRepository();

            BizResult res = target.ChangeUserPassword("xxx", "123", "123");

            Assert.IsNotNull(res);
            Assert.AreEqual("Не верное имя пользователя или старый пароль!", res.Message);
        }

        /// <summary>
        ///Тест для GetUserInfo
        ///</summary>
        [TestMethod]
        public void GetUserInfoTest()
        {
            var target = new UserRepository();
            
            UserInfo actual = target.GetUserInfo("TestUser");
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///Тест для GetUserInfo
        ///</summary>
        [TestMethod]
        public void GetUserInfoNotExistenUser()
        {
            var target = new UserRepository();

            UserInfo actual = target.GetUserInfo("NotExistenUser");
            Assert.IsNull(actual);
        }

        /// <summary>
        ///Тест для Validate
        ///</summary>
        [TestMethod]
        public void ValidateTest()
        {
            var target = new UserRepository(); 
            
            bool actual = target.Validate("TestUser", "123");

            Assert.AreEqual(true, actual);
        }

        /// <summary>
        ///Тест для Validate
        ///</summary>
        [TestMethod]
        public void ValidateFalseTest()
        {
            var target = new UserRepository();

            bool actual = target.Validate("NotExistenUser", "123");

            Assert.AreEqual(false, actual);
        }

        /// <summary>
        ///Тест для Validate
        ///</summary>
        [TestMethod]
        public void ValidateFalse2Test()
        {
            var target = new UserRepository();

            bool actual = target.Validate("TestUser", "NotExistenPass");

            Assert.AreEqual(false, actual);
        }

        /// <summary>
        /// Тест для Validate
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateNull1()
        {
            var target = new UserRepository();

            Assert.AreEqual(false, target.Validate(null, "123"));
        }

        /// <summary>
        /// Тест для Validate
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateNull2()
        {
            var target = new UserRepository();
        
            Assert.AreEqual(false, target.Validate(null, null));
        }

        /// <summary>
        /// Тест для Validate
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateNull3()
        {
            var target = new UserRepository();

            Assert.AreEqual(false, target.Validate("TestUser", null));
        }
    }
}
