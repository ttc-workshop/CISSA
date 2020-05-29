namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public interface IDataContextFactory
    {
        IDataContext CreateDc(string connectionString);
/*    }

    public interface IMultiDataContextFactory
    {*/
        IMultiDataContext CreateMultiDc(string configSectionName);
    }
}