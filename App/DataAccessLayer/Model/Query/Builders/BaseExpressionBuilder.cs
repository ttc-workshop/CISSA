using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Builders
{
    public class BaseExpressionBuilder: BaseQueryBuilder, IQueryExpression
    {
        private readonly IList<QueryConditionDef> _conditions = new List<QueryConditionDef>();
        public IList<QueryConditionDef> Conditions { get { return _conditions; } }

        public BaseExpressionBuilder(BaseQueryBuilder parent)
            : base(parent)
        {
        }

        internal override void AddCondition(QueryConditionDef condition)
        {
            Conditions.Add(condition);
        }
        
        public virtual IQueryCondition CreateCondition(string attribute, ExpressionOperation operation, string source)
        {
            var condition = CreateConditionDef(attribute, operation, source); //new QueryConditionDef {AttributeName = attribute, Operation = operation, Exp = source};

            AddCondition(condition);

            return new QueryConditionBuilder(this, condition);
        }

        public virtual IQueryCondition CreateCondition(Guid attributeId, ExpressionOperation operation, string source)
        {
            var condition = CreateConditionDef(attributeId, operation, source); //new QueryConditionDef {AttributeId = attributeId, Operation = operation, Exp = source};

            AddCondition(condition);

            return new QueryConditionBuilder(this, condition);
        }

        public virtual IQueryCondition And(string attribute, string source = null)
        {
            return CreateCondition(attribute, ExpressionOperation.And, source);
        }

        public virtual IQueryCondition Or(string attribute, string source = null)
        {
            return CreateCondition(attribute, ExpressionOperation.Or, source);
        }

        public virtual IQueryCondition AndNot(string attribute, string source = null)
        {
            return CreateCondition(attribute, ExpressionOperation.AndNot, source);
        }

        public virtual IQueryCondition OrNot(string attribute, string source = null)
        {
            return CreateCondition(attribute, ExpressionOperation.OrNot, source);
        }

        public IQueryCondition And(Guid attributeId, string source = null)
        {
            return CreateCondition(attributeId, ExpressionOperation.And, source);
        }

        public IQueryCondition Or(Guid attributeId, string source = null)
        {
            return CreateCondition(attributeId, ExpressionOperation.Or, source);
        }

        public IQueryCondition AndNot(Guid attributeId, string source = null)
        {
            return CreateCondition(attributeId, ExpressionOperation.AndNot, source);
        }

        public IQueryCondition OrNot(Guid attributeId, string source = null)
        {
            return CreateCondition(attributeId, ExpressionOperation.OrNot, source);
        }

        public IQueryCondition AddExpCondition(ExpressionOperation operation, string attribute)
        {
            var exp = new QueryConditionDef { Operation = operation, Condition = ConditionOperation.Exp };
            AddCondition(exp);
            var condition = CreateConditionDef(attribute, ExpressionOperation.And, ""); // new QueryConditionDef { AttributeName = attribute, Operation = ExpressionOperation.And };

            exp.Conditions.Add(condition);

            var expBuilder = new ExpressionBuilder(this, exp);
            return new QueryConditionBuilder(expBuilder, condition);
        }

        public IQueryCondition AndExp(string attribute, string source = null)
        {
            return AddExpCondition(ExpressionOperation.And, attribute);
        }

        public IQueryCondition OrExp(string attribute, string source = null)
        {
            return AddExpCondition(ExpressionOperation.Or, attribute);
        }

        public IQueryCondition AndNotExp(string attribute, string source = null)
        {
            return AddExpCondition(ExpressionOperation.AndNot, attribute);
        }

        public IQueryCondition OrNotExp(string attribute, string source = null)
        {
            return AddExpCondition(ExpressionOperation.OrNot, attribute);
        }

        public virtual IQueryExpression End()
        {
            var parent = Parent;

            while (parent != null)
            {
                var end = parent as IQueryExpression;
                if (end != null) return end;
                parent = parent.Parent;
            }
            return null;
        }
    }
}
