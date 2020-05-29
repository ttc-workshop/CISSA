namespace Intersoft.CISSA.DataAccessLayer.Core
{
    public interface IAppServiceProviderFactory
    {
        IAppServiceProvider Create();
        IAppServiceProvider Create(params object[] services);
    }
}