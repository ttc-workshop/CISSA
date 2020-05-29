using System;
using System.Runtime.CompilerServices;

namespace Intersoft.CISSA.DataAccessLayer.Core
{
    public interface IAppServiceProvider : IDisposable
    {
        T Find<T>() where T : class;
        T Find<T>(object arg) where T : class;
        T Get<T>() where T : class;
        T Get<T>(object arg) where T : class;
        int GetServiceCount();
    }

    public static class AppServiceProviderHelper
    {
        public static Guid GetCurrentUserId(this IAppServiceProvider provider)
        {
            var userDataProvider = provider.Find<IUserDataProvider>();

            if (userDataProvider == null) return Guid.Empty;

            return userDataProvider.UserId;
        }
    }
}