namespace Intersoft.CISSA.DataAccessLayer.Core
{
    public class AppServiceProviderFactory: IAppServiceProviderFactory
    {
        public IAppServiceProvider Create()
        {
            return new AppServiceProvider();
        }

        public IAppServiceProvider Create(params object[] services)
        {
            return new AppServiceProvider(services);
        }
    }
}