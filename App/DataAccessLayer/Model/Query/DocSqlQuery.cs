using System;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    public class DocSqlQuery
    {
        public QueryDef Def { get; private set; }

        public DocSqlQuery(QueryDef def)
        {
            Def = def;
        }

        public DocSqlQuery(QueryBuilder builder)
        {
            Def = builder.Def;
        }

        public DocSqlQuery(IQueryExpression exp)
        {
            while (exp != null && !(exp is IQuery)) exp = exp.End();

            if (exp is IQuery) Def = ((IQuery) exp).GetDef();
            else
                throw new ApplicationException("Не могу создать запрос! Ошибка в выражении запроса");
        }


    }
}
