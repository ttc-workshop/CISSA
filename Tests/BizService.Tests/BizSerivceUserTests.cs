using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SqlClient;
using System.IdentityModel.Tokens;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Configuration;
using Intersoft.CISSA.BizService;
using Intersoft.CISSA.BizService.Utils;
using Intersoft.CISSA.BizServiceTests.FakeRepo;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.BizServiceTests
{
    /// <summary>
    /// Сводное описание для UnitTest1
    /// </summary>
    [TestClass]
    public class BizSerivceUserTests
    {
        private static BizService.BizService GetService()
        {
            //var userRepo = new FakeUserRepository();
            return new BizService.BizService(/*userRepo,*/ "D");
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
            Assert.AreEqual(BizResultType.Message , bizResult.Type);
            Assert.AreEqual("Смена пароля прошла успешно.", bizResult.Message);
        }

        public static readonly string ConnectionString =
        //"Data Source=195.38.189.100;Initial Catalog=cissa-with-children;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        "Data Source=localhost;Initial Catalog=asist-data;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        [TestMethod]
        public void AppServiceProviderTest()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    //AppServiceProvider.SetServiceFactoryFunc(typeof (IUserRepository), CreateFakeUserRepository);
                    using (var multiDc = new MultiDataContext(new[] {dataContext}))
                    {
                        var service = new BizService.BizService(multiDc, "D");
                        try
                        {
                            Console.WriteLine(@"Count of AppServiceProvider Factory functions: {0}",
                                AppServiceProvider.TypeFactoryFuncs.Count);
                            Console.WriteLine(@"Count of Services in AppServiceProvider: {0}",
                                service.Provider.GetServiceCount());

                            Assert.IsNotNull(service);
                            service.TryConnect();
                        }
                        finally
                        {
                            service.Dispose();
                        }
                        service = new BizService.BizService(service.Provider, "D");
                        try
                        {
                            Console.WriteLine(@"Count of AppServiceProvider Factory functions: {0}",
                                AppServiceProvider.TypeFactoryFuncs.Count);
                            Console.WriteLine(@"Count of Services in AppServiceProvider: {0}",
                                service.Provider.GetServiceCount());

                            Assert.IsNotNull(service);

                            var userInfo = service.GetUserInfo();
                            Assert.IsNotNull(userInfo);
                            Console.WriteLine(userInfo.OrganizationName);
                            foreach (var l in service.GetLanguages())
                            {
                                Console.Write(l.Name, " ");
                            }
                        }
                        finally
                        {
                            service.Dispose();
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void BizServiceEmptyConstructor()
        {
            using (var bizService = new BizService.BizService("D"))
            {
                var userInfo = bizService.GetUserInfo();
                Assert.IsNotNull(userInfo);
                Console.WriteLine(userInfo.OrganizationName, userInfo.UserName);
            }
            Console.WriteLine("OK");
        }

        [TestMethod]
        public void CheckCustomSecurityUserNameValidator()
        {
            var validator = new CustomSecurityUserNameValidator();
            validator.Validate("D", "123");
        }

        [TestMethod]
        public void ValidateSecurityUserName()
        {
            //using (var connection = new SqlConnection(ConnectionString))
            {
                //using (var dataContext = new DataContext(connection))
                {
                    BaseServiceFactory.CreateBaseServiceFactories();

                    using (var multiDc = new MultiDataContext(BaseServiceFactory.DataContextConfigSectionName /*new[] {dataContext}*/))
                    {
                        var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                        var serviceProvider = providerFactory.Create(multiDc);

                        var userRepo = serviceProvider.Get<IUserRepository>(); //new UserRepository(dataContext);

                        Console.WriteLine(userRepo.GetType().Name);
                        Assert.IsTrue(userRepo.Validate("R", "803"));
                    }
                }
            }
        }

        [TestMethod]
        public void TestMetadataWorkspace()
        {
            var asm = Assembly.GetExecutingAssembly();
            var asmType = asm.GetType();
            Console.WriteLine(asmType.Name);
            var workspace =
                new MetadataWorkspace(
                    new[]
                    {
                        @".\Model\Data\DataModel.csdl",
                        @".\Model\Data\DataModel.ssdl",
                        @".\Model\Data\DataModel.msl"
                    }, new[] {asm});
            foreach (var item in workspace.GetItemCollection(DataSpace.CSSpace))
                Console.WriteLine(item.ToString());

            const string assemblyQualifiedName = @"Intersoft.CISSA.DataAccessLayer";
            Type anyModelType = Type.GetType(assemblyQualifiedName);
            Console.WriteLine(anyModelType != null ? anyModelType.Name : "----");
            if (anyModelType != null)
            {
                Assembly modelAssembly = Assembly.GetAssembly(anyModelType);
                Console.WriteLine(modelAssembly.FullName);
            }

            foreach (var refAssembly in asm.GetReferencedAssemblies())
            {
                Console.WriteLine(refAssembly.Name);
            }
        }

        [TestMethod]
        public void ConnectionStringTest()
        {
            foreach (ConnectionStringSettings setting in ConfigurationManager.ConnectionStrings)
            {
                Console.WriteLine("* " + setting.Name);
                foreach (PropertyInformation prop in setting.ElementInformation.Properties)
                {
                    Console.WriteLine("  -- {0}={1}", prop.Name, prop.Value);
                }
            }
            var settings =
                ConfigurationManager.GetSection("DbConnectionTypes")
                    as System.Collections.Specialized.NameValueCollection;

            if (settings != null)
            {
                foreach (string key in settings.AllKeys)
                {
                    Console.WriteLine("** {0}: {1}", key, settings[key]);

                    var setting = ConfigurationManager.ConnectionStrings[key] as ConnectionStringSettings;

                    var factory = System.Data.Common.DbProviderFactories.GetFactory(setting.ProviderName);
                    using (var connection = factory.CreateConnection())
                    {
                        if (connection != null)
                        {
                            connection.ConnectionString = setting.ConnectionString;
                            connection.Open();
                        }
                    }
                }
            }
        }

        private object CreateFakeUserRepository(object arg)
        {
            return new FakeUserRepository();
        }
    }
}
