namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public class DataContextFactory : IDataContextFactory/*, IMultiDataContextFactory*/
    {
        public virtual IDataContext CreateDc(string connectionString)
        {
            return new DataContext(connectionString);
        }

        public virtual IMultiDataContext CreateMultiDc(string configSectionName)
        {
            return new MultiDataContext(configSectionName);
        }
    }
}