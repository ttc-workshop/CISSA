namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public static class DataContextFactoryProvider
    {
        private static IDataContextFactory _factory = new DataContextFactory();

        public static IDataContextFactory Factory
        {
            get { return _factory; }
            internal set { _factory = value; }
        }

        public static IDataContextFactory GetFactory()
        {
            return Factory;
        }
    }
}