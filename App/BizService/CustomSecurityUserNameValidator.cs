using System;
using System.Data.Entity.Core.EntityClient;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.IO;
using System.Threading;
using System.Web.Hosting;
using Intersoft.CISSA.BizService.Utils;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.BizService
{

    /// <summary>
    /// Класс проверки данных клиета подключенного к сервису безопасности Security
    /// </summary>
    public class CustomSecurityUserNameValidator : UserNamePasswordValidator
    {
        /// <summary>
        /// Проверяет данные подлючающегося клиента
        /// </summary>
        /// <param name="userName">Логин клиента</param>
        /// <param name="password">Пароль клиента</param>
        public override void Validate(string userName, string password)
        {
            try
            {
                //using (var connection = new EntityConnection("name=cissaEntities"))
                {
                    //using (var dataContext = new DataContext(connection))
                    {
                        BaseServiceFactory.CreateBaseServiceFactories();

                        var dataContextFactory = DataContextFactoryProvider.GetFactory();
                        using (var multiDC = dataContextFactory.CreateMultiDc(BaseServiceFactory.DataContextConfigSectionName))
                        {
                            var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                            var serviceProvider = providerFactory.Create(multiDC);

                            var userRepo = serviceProvider.Get<IUserRepository>(); //new UserRepository(dataContext);

                            if (userRepo.Validate(userName, password)) return;
                        }

                        Thread.Sleep(1000); // притормозим выполнение для сложности подбора пароля
                        throw new SecurityTokenException("Unknown Username or Password");
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    using (var writer = new StreamWriter(Logger.GetLogFileName("bizServiceValidate"), true))
                    {
                        writer.WriteLine("{0}: Log error for username: \"{1}\"; message: \"{2}\"",
                            DateTime.Now,
                            userName, e.Message);
                        if (e.InnerException != null)
                            writer.WriteLine("  - inner exception: \"{0}\"", e.InnerException.Message);
                        writer.WriteLine("  -- Stack: {0}", e.StackTrace);
                        writer.WriteLine(Directory.GetCurrentDirectory());
//                        writer.WriteLine(HostingEnvironment.MapPath("."));
                    }
                }
                catch
                {
                    // ignored
                }
                throw;
            }
        }
    }
}