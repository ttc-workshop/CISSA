using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryConditionOperationBuilder
    {
        public ExpressionOperation Operation { get; private set; }
        public SqlQueryConditionBuilder Parent { get; private set; }
        public DocDef DocDef { get; private set; }
        public AttrDef AttrDef { get; private set; }

        public SqlQueryConditionOperationBuilder(SqlQueryConditionBuilder parent, ExpressionOperation operation,
                                                 DocDef docDef, AttrDef attrDef)
        {
            Parent = parent;
            Operation = operation;
            DocDef = docDef;
            AttrDef = attrDef;
        }

        public SqlQueryConditionBuilder Eq(object value)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.Equal, value);
            return Parent;
        }
        public SqlQueryConditionBuilder Neq(object value)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.NotEqual, value);
            return Parent;
        }
        public SqlQueryConditionBuilder Gt(object value)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.GreatThen, value);
            return Parent;
        }
        public SqlQueryConditionBuilder Ge(object value)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.GreatEqual, value);
            return Parent;
        }
        public SqlQueryConditionBuilder Lt(object value)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.LessThen, value);
            return Parent;
        }
        public SqlQueryConditionBuilder Le(object value)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.LessEqual, value);
            return Parent;
        }
        public SqlQueryConditionBuilder Like(object value)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.Like, value);
            return Parent;
        }
        public SqlQueryConditionBuilder NotLike(object value)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.NotLike, value);
            return Parent;
        }
        public SqlQueryConditionBuilder IsNull()
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.IsNull, null);
            return Parent;
        }
        public SqlQueryConditionBuilder NotNull()
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.IsNotNull, null);
            return Parent;
        }
        public SqlQueryConditionBuilder In(object[] values)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.In, values);
            return Parent;
        }
        public SqlQueryConditionBuilder NotIn(object[] values)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.NotIn, values);
            return Parent;
        }
        public SqlQueryConditionBuilder In(SqlQuery query, string attrName)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.In, query, attrName);
            return Parent;
        }
        public SqlQueryConditionBuilder NotIn(SqlQuery query, string attrName)
        {
            Parent.Query.AddCondition(Operation, DocDef, AttrDef.Id, ConditionOperation.NotIn, query, attrName);
            return Parent;
        }
    }
}