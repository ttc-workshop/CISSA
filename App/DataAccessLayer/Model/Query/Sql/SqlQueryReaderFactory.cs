using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public interface ISqlQueryReaderFactory
    {
        SqlQueryReader Create(SqlQuery query);

        SqlQueryReader Create(QueryDef def);
    }

    public interface ISqlQueryBuilderFactory
    {
        ISqlQueryBuilder Create();
    }

    public class MultiContextSqlQueryReaderFactory : ISqlQueryReaderFactory
    {
        public ISqlQueryReaderFactory Factory { get; private set; }

        public MultiContextSqlQueryReaderFactory(IAppServiceProvider provider)
        {
            var multiDC = provider.Get<IMultiDataContext>();
            var dataContext = multiDC.Contexts.First(dc => dc.DataType.HasFlag(DataContextType.Document));

            Factory = new SqlQueryReaderFactory(provider, dataContext);
        }

        public SqlQueryReader Create(SqlQuery query)
        {
            return Factory.Create(query);
        }

        public SqlQueryReader Create(QueryDef def)
        {
            return Factory.Create(def);
        }
    }

    public class SqlQueryReaderFactory : ISqlQueryReaderFactory
    {
        public IAppServiceProvider Provider { get; private set; }
        public IDataContext DataContext { get; private set; }

        public SqlQueryReaderFactory(IAppServiceProvider provider, IDataContext dataContext)
        {
            Provider = provider;
            DataContext = dataContext;
        }

        public SqlQueryReader Create(SqlQuery query)
        {
            return new SqlQueryReader(DataContext, query);
        }

        public SqlQueryReader Create(QueryDef def)
        {
            var sqb = new SqlQueryBuilderTool(Provider, DataContext);
            var query = sqb.Build(def);
            return Create(query);
        }
    }

    public class SqlQueryBuilderFactory : ISqlQueryBuilderFactory
    {
        public IAppServiceProvider Provider { get; private set; }
        public IDataContext DataContext { get; private set; }

        public SqlQueryBuilderFactory(IAppServiceProvider provider, IDataContext dataContext)
        {
            Provider = provider;
            DataContext = dataContext;
        }

        public ISqlQueryBuilder Create()
        {
            return new SqlQueryBuilderTool(Provider, DataContext);
        }
    }
}
