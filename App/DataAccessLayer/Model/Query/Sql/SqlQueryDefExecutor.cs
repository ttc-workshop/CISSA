using System;
using System.Collections.Generic;
using System.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryDefExecutor
    {
        public QueryDef Def { get; private set; }
        public IDataContext DataContext { get; private set; }

        public SqlQueryDefExecutor(QueryDef def, IDataContext dataContext)
        {
            Def = def;
            DataContext = dataContext;
        }

        public SqlQueryDefExecutor(QueryBuilder builder, IDataContext dataContext)
        {
            Def = builder.Def;
            DataContext = dataContext;
        }

        public int Count()
        {
            var count = 0;
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute("&Id", SqlQuerySummaryFunction.Count);

            using(var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                        count = reader.GetInt32(0);
                }
                reader.Close();
            }
            return count;
        }

        public T Sum<T>(Guid attrDefId)
        {
            T result = default(T);
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefId, SqlQuerySummaryFunction.Sum);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        var v = reader.GetValue(0);

                        v.TryParse(out result);
                    }
                }
                reader.Close();
            }
            return result;
        }

        public T Sum<T>(string attrDefName)
        {
            T result = default(T);
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefName, SqlQuerySummaryFunction.Sum);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        var v = reader.GetValue(0);

                        v.TryParse(out result);
                    }
                }
                reader.Close();
            }
            return result;
        }

        public T Max<T>(Guid attrDefId)
        {
            T result = default(T);
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefId, SqlQuerySummaryFunction.Max);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        var v = reader.GetValue(0);

                        v.TryParse(out result);
                    }
                }
                reader.Close();
            }
            return result;
        }

        public T Max<T>(string attrDefName)
        {
            T result = default(T);
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefName, SqlQuerySummaryFunction.Max);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        var v = reader.GetValue(0);

                        v.TryParse(out result);
                    }
                }
                reader.Close();
            }
            return result;
        }

        public T Min<T>(Guid attrDefId)
        {
            T result = default(T);
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefId, SqlQuerySummaryFunction.Min);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        var v = reader.GetValue(0);

                        v.TryParse(out result);
                    }
                }
                reader.Close();
            }
            return result;
        }

        public T Min<T>(string attrDefName)
        {
            T result = default(T);
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefName, SqlQuerySummaryFunction.Min);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        var v = reader.GetValue(0);

                        v.TryParse(out result);
                    }
                }
                reader.Close();
            }
            return result;
        }

        public T Avg<T>(Guid attrDefId)
        {
            T result = default(T);
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefId, SqlQuerySummaryFunction.Avg);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        var v = reader.GetValue(0);

                        v.TryParse(out result);
                    }
                }
                reader.Close();
            }
            return result;
        }

        public T Avg<T>(string attrDefName)
        {
            T result = default(T);
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefName, SqlQuerySummaryFunction.Avg);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        var v = reader.GetValue(0);

                        v.TryParse(out result);
                    }
                }
                reader.Close();
            }
            return result;
        }

        public decimal SumAsDecimal(Guid attrDefId)
        {
            var count = 0m;
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefId, SqlQuerySummaryFunction.Sum);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        if (reader.Reader.GetFieldType(0) == typeof(double))
                            count = (decimal)reader.GetDouble(0);
                        else if (reader.Reader.GetFieldType(0) == typeof(decimal))
                            count = reader.GetDecimal(0);
                    }
                }
                reader.Close();
            }
            return count;
        }

        public decimal SumAsDecimal(string attrDefName)
        {
            var count = 0m;
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefName, SqlQuerySummaryFunction.Sum);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        if (reader.Reader.GetFieldType(0) == typeof(double))
                            count = (decimal) reader.GetDouble(0);
                        else if (reader.Reader.GetFieldType(0) == typeof(decimal))
                            count = reader.GetDecimal(0);
                    }
                }
                reader.Close();
            }
            return count;
        }

        public double SumAsDouble(Guid attrDefId)
        {
            double count = 0;
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefId, SqlQuerySummaryFunction.Sum);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        if (reader.Reader.GetFieldType(0) == typeof(double))
                            count = reader.GetDouble(0);
                        else if (reader.Reader.GetFieldType(0) == typeof(decimal))
                            count = (double)reader.GetDecimal(0);
                    }
                }
                reader.Close();
            }
            return count;
        }

        public double SumAsDouble(string attrDefName)
        {
            double count = 0;
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefName, SqlQuerySummaryFunction.Sum);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                if (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        if (reader.Reader.GetFieldType(0) == typeof(double))
                            count = reader.GetDouble(0);
                        else if (reader.Reader.GetFieldType(0) == typeof(decimal))
                            count = (double) reader.GetDecimal(0);
                    }
                }
                reader.Close();
            }
            return count;
        }

        public List<T> All<T>(Guid attrDefId)
        {
            var list = new List<T>();
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefId);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                while (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        T item;
                        var v = reader.GetValue(0);
                        list.Add(v.TryParse(out item) ? item : default(T));
                    }
                    else 
                        list.Add(default(T));
                }
                reader.Close();
            }
            return list;
        }
        public List<T> All<T>(string attrDefName)
        {
            var list = new List<T>();
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            sql.AddAttribute(attrDefName);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                reader.Open();
                while (reader.Read())
                {
                    if (!reader.IsDbNull(0))
                    {
                        T item;
                        var v = reader.GetValue(0);
                        list.Add(v.TryParse(out item) ? item : default(T));
                    }
                    else
                        list.Add(default(T));
                }
                reader.Close();
            }
            return list;
        }

        public DataTable All()
        {
            var sql = SqlQueryBuilder.Build(DataContext, Def);

            var table = new DataTable(sql.Source.AliasName);

            using (var reader = new SqlQueryReader(DataContext, sql))
            {
                table.Load(reader.Reader);
            }
            return table;
        }
    }
}
