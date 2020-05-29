using System;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Builders
{
    public abstract class BaseQueryBuilder
    {
        public BaseQueryBuilder Parent { get; private set; }

        protected BaseQueryBuilder(BaseQueryBuilder parent)
        {
            Parent = parent;
        }

        internal virtual void AddCondition(QueryConditionDef condition)
        {
            throw new NotImplementedException();
        }

        internal virtual QueryConditionDef CreateConditionDef(string attrName, ExpressionOperation operation, string source)
        {
            if (Parent != null)
                return Parent.CreateConditionDef(attrName, operation, source);

            throw new ApplicationException("Не могу создать Query ConditionDef");
        }

        internal virtual QueryConditionDef CreateConditionDef(Guid attrId, ExpressionOperation operation, string source)
        {
            if (Parent != null)
                return Parent.CreateConditionDef(attrId, operation, source);

            throw new ApplicationException("Не могу создать Query ConditionDef");
        }

        internal virtual QuerySourceDef FindSource(string alias)
        {
            if (Parent != null)
                return Parent.FindSource(alias);

            return null;
        }

        internal QuerySourceDef GetSource(string alias)
        {
            var source = FindSource(alias);

            if (source == null)
                throw new ApplicationException(String.Format("Query Source \"{0}\" not found", alias));

            return source;
        }
    }
}
