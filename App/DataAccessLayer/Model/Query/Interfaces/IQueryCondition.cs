using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces
{
    public interface IQueryCondition
    {
//        ExpressionOperation GetOperation();
        IQueryExpression Eq(object value, string paramName = null);
        IQueryExpression Eq(string attribute, string source);
        IQueryExpression Neq(object value, string paramName = null);
        IQueryExpression Neq(string attribute, string source);
        IQueryExpression Ge(object value, string paramName = null);
        IQueryExpression Ge(string attribute, string source);
        IQueryExpression Gt(object value, string paramName = null);
        IQueryExpression Gt(string attribute, string source);
        IQueryExpression Le(object value, string paramName = null);
        IQueryExpression Le(string attribute, string source);
        IQueryExpression Lt(object value, string paramName = null);
        IQueryExpression Lt(string source, string attribute);
//        IQueryExpression In(IQuery query, string attribute);
//        IQueryExpression In(List<object> values);
//        IQueryExpression In(object[] values);
        IQueryExpression Contains(string value, string paramName = null);
        IQueryExpression Like(string value, string paramName = null);
        IQueryExpression Between(object value1, object value2, string paramName1 = null, string paramName2 = null);
        IQueryExpression NotContains(string value, string paramName = null);
        IQueryExpression NotLike(string value, string paramName = null);
        IQueryExpression NotBetween(object value1, object value2, string paramName1 = null, string paramName2 = null);
        IQueryExpression IsNull();
        IQueryExpression IsNotNull();
        IQueryExpression Is(string value);
        IQueryCondition Include(string attribute);
        IQueryExpression In(QueryDef subQuery);
        IQueryExpression In(QueryDef subQuery, string attribute);
        IQueryExpression In(string attribute, string source);
        IQueryExpression In(object[] values);
        IQueryExpression NotIn(QueryDef subQuery);
        IQueryExpression NotIn(QueryDef subQuery, string attribute);
        IQueryExpression NotIn(object[] values);
        IQueryCondition Exp(string attribute);

        // Новые функции
//        IQueryExpression InState(string state);
//        IQueryExpression InStates(string[] states);
//        IQueryExpression BelongTo(string orgName);

//        IQueryable<Document> Build(ObjectContext context);
    }
}