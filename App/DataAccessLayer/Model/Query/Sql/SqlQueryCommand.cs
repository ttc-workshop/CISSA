using System.Data.Entity.Core.Objects;
using Intersoft.CISSA.DataAccessLayer.Model.Context;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryCommand
    {
        public SqlQuery Query { get; private set; }
        public IDataContext DataContext { get; private set; }

        public SqlQueryCommand(SqlQuery query, IDataContext dataContext)
        {
            Query = query;
            DataContext = dataContext;
        }

        public ObjectResult<TEntity> ExecuteQuery<TEntity>() where TEntity : class
        {
            var sql = Query.BuildSql();
            return DataContext.GetEntityDataContext().Entities.ExecuteStoreQuery<TEntity>(sql.ToString());
        }

        public int ExecuteCommand() 
        {
            var sql = Query.BuildSql();
            return DataContext.GetEntityDataContext().Entities.ExecuteStoreCommand(sql.ToString());
        }
    }
}
