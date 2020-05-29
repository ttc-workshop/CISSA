
namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces
{
    public interface IQueryJoin
    {
        IQueryCondition On(string attrName, string sourceAlias);
    }
}