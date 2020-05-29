namespace Intersoft.CISSA.DataAccessLayer.Core
{
    public static class AppServiceProviderFactoryProvider
    {
        private static IAppServiceProviderFactory _factory = new AppServiceProviderFactory();
        public static IAppServiceProviderFactory Factory 
        { 
            get { return _factory; }
            internal set { _factory = value; }
        }

        public static IAppServiceProviderFactory GetFactory()
        {
            return Factory;
        }
    }
}