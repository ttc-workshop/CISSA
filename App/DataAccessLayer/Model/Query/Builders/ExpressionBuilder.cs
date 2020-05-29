using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Builders
{
    public class ExpressionBuilder: BaseExpressionBuilder
    {
        public QueryConditionDef Condition { get; private set; }
        public ExpressionBuilder(BaseQueryBuilder parent, QueryConditionDef condition)
            : base(parent)
        {
            Condition = condition;
        }

        internal override void AddCondition(QueryConditionDef condition)
        {
            Condition.Conditions.Add(condition);
        }
    }
}
