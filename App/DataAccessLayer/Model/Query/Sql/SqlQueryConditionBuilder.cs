using System;
using System.Linq;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryConditionBuilder
    {
        public SqlQuery Query { get; private set; }
        public SqlQuerySource Source { get; private set; }
        public SqlQueryConditionBuilder Parent { get; private set; }

        public SqlQueryConditionBuilder(SqlQueryConditionBuilder parent, SqlQuery query, SqlQuerySource source)
        {
            Parent = parent;
            Query = query;
            Source = source;
        }

        public SqlQueryConditionOperationBuilder And(string attrDefName)
        {
            var attrDef =
                Source.GetDocDef()
                    .Attributes.First(a => String.Equals(a.Name, attrDefName, StringComparison.OrdinalIgnoreCase));

            return new SqlQueryConditionOperationBuilder(this, ExpressionOperation.And, Source.GetDocDef(), attrDef);
        }

        public SqlQueryConditionOperationBuilder AndNot(string attrDefName)
        {
            var attrDef =
                Source.GetDocDef()
                    .Attributes.First(a => String.Equals(a.Name, attrDefName, StringComparison.OrdinalIgnoreCase));

            return new SqlQueryConditionOperationBuilder(this, ExpressionOperation.AndNot, Source.GetDocDef(), attrDef);
        }

        public SqlQueryConditionOperationBuilder Or(string attrDefName)
        {
            var attrDef =
                Source.GetDocDef()
                    .Attributes.First(a => String.Equals(a.Name, attrDefName, StringComparison.OrdinalIgnoreCase));

            return new SqlQueryConditionOperationBuilder(this, ExpressionOperation.Or, Source.GetDocDef(), attrDef);
        }

        public SqlQueryConditionOperationBuilder OrNot(string attrDefName)
        {
            var attrDef =
                Source.GetDocDef()
                    .Attributes.First(a => String.Equals(a.Name, attrDefName, StringComparison.OrdinalIgnoreCase));

            return new SqlQueryConditionOperationBuilder(this, ExpressionOperation.OrNot, Source.GetDocDef(), attrDef);
        }
    }
}