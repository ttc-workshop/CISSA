using System.Data.Entity.Core.Objects;
using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public interface IEntityDataContext
    {
        cissaEntities Entities { get; }
        ObjectResult<TEntity> ExecQuery<TEntity>(string sql, params object[] parameters);
        ObjectSet<Document> Documents { get; }
        void SaveChanges();
    }
}