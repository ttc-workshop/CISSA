
using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces
{
    public interface IQuery
    {
        IQueryJoin Join(QueryDef query, string alias);
        IQuery Join(QueryDef query, string alias, Action<IQueryExpression> conditionAction);
        IQuery Join(Guid docDefId, string alias, Action<IQueryExpression> conditionAction);
        IQuery Join(string docDefName, string alias, Action<IQueryExpression> conditionAction);
        IQueryJoin LeftJoin(QueryDef query, string alias);
        IQuery LeftJoin(QueryDef query, string alias, Action<IQueryExpression> conditionAction);
        IQuery LeftJoin(Guid docDefId, string alias, Action<IQueryExpression> conditionAction);
        IQuery LeftJoin(string docDefName, string alias, Action<IQueryExpression> conditionAction);
        IQueryJoin RightJoin(QueryDef query, string alias);
        IQuery RightJoin(QueryDef query, string alias, Action<IQueryExpression> conditionAction);
        IQuery RightJoin(Guid docDefId, string alias, Action<IQueryExpression> conditionAction);
        IQuery RightJoin(string docDefName, string alias, Action<IQueryExpression> conditionAction);
        IQuery FullJoin(Guid docDefId, string alias, Action<IQueryExpression> conditions);
        IQuery FullJoin(string docDefName, string alias, Action<IQueryExpression> conditions);
        IQuery FullJoin(QueryDef query, string alias, Action<IQueryExpression> conditions);
        //        IQuery Include(string attributeName);
//        IQuery IncludeDef(string docDefName, string aliasName);

        QueryDef GetDef();

        IQuery AddOrderBy(string attrName, bool asc = true);
        IQuery AddOrderBy(Guid attrId, bool asc = true);
        /*
                bool Any();
                IEnumerable<Guid> All();
                IEnumerable<object> All(string attribute);
                Guid? FirstOrDefault();
                object FirstOrDefault(string attribute);
                IEnumerable<Guid> First(int count);
                IEnumerable<object> First(int count, string attribute);
                Guid? LastOrDefault();
                object LastOrDefault(string attribute);
                IEnumerable<Guid> Last(int count);
                IEnumerable<object> Last(int count, string attribute);
                IEnumerable<Guid> Take(int no, int count);
                IEnumerable<object> Take(int no, int count, string attribute);

                int Count();
                double? Sum(string attribute);
        */
    }

    public interface IQueryOrderDef
    {
        
    }
}
