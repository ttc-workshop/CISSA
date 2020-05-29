using System;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryExecutor
    {
        public IDataContext DataContext { get; private set; }
        public SqlQuery Query { get; private set; }

        public SqlQueryExecutor(IDataContext dataContext, SqlQuery query)
        {
            DataContext = dataContext;
            Query = query;
        }

        public SqlQueryExecutor(SqlQuery query)
        {
            DataContext = query.DataContext;
            Query = query;
        }

        public int Count()
        {
            using (var reader = new SqlQueryReader(DataContext, Query))
                return reader.GetCount();
        }

        public object Sum(Guid attrDefId)
        {
            using (var reader = new SqlQueryReader(DataContext, Query))
                return reader.GetSum(attrDefId);
        }

        public object Sum(string attrDefName)
        {
            using (var reader = new SqlQueryReader(DataContext, Query))
                return reader.GetSum(attrDefName);
        }

        public T Sum<T>(Guid attrDefId)
        {
            var result = default(T);
            using (var reader = new SqlQueryReader(DataContext, Query))
            {
                var v = reader.GetSum(attrDefId);
                if (v != null) v.TryParse(out result);
            }
            return result;
        }

        public T Sum<T>(string attrDefName)
        {
            var result = default(T);
            using (var reader = new SqlQueryReader(DataContext, Query))
            {
                var v = reader.GetSum(attrDefName);
                if (v != null) v.TryParse(out result);
            }
            return result;
        }

        public object Max(Guid attrDefId)
        {
            using (var reader = new SqlQueryReader(DataContext, Query))
                return reader.GetMax(attrDefId);
        }

        public object Max(string attrDefName)
        {
            using (var reader = new SqlQueryReader(DataContext, Query))
                return reader.GetMax(attrDefName);
        }

        public object Min(Guid attrDefId)
        {
            using (var reader = new SqlQueryReader(DataContext, Query))
                return reader.GetMin(attrDefId);
        }

        public object Min(string attrDefName)
        {
            using (var reader = new SqlQueryReader(DataContext, Query))
                return reader.GetMin(attrDefName);
        }

    }
}
