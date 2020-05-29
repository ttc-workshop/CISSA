using System;
using System.Collections.Generic;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQuerySelectAttribute : SqlQueryAttribute
    {
        public SqlQuerySummaryFunction Summary { get; private set; }

        public SqlQuerySelectAttribute(SqlQuerySource source, Guid attrDefId)
            : base(source, attrDefId)
        {
        }

        public SqlQuerySelectAttribute(SqlQuerySource source, string attrDefName) : base(source, attrDefName)
        {
        }

        public SqlQuerySelectAttribute(SqlQuerySource source, SystemIdent attrIdent)
            : base(source, attrIdent)
        {
        }

        public SqlQuerySelectAttribute(SqlQuerySource source, Guid attrDefId, SqlQuerySummaryFunction summary)
            : base(source, attrDefId)
        {
            Summary = summary;
        }

        public SqlQuerySelectAttribute(SqlQuerySource source, string attrDefName, SqlQuerySummaryFunction summary) : base(source, attrDefName)
        {
            Summary = summary;
        }

        public SqlQuerySelectAttribute(SqlQuerySource source, SystemIdent attrIdent, SqlQuerySummaryFunction summary)
            : base(source, attrIdent)
        {
            Summary = summary;
        }

        public SqlQuerySelectAttribute(SqlQuerySource source, Guid attrDefId, string expression)
            : base(source, attrDefId, expression)
        {
        }

        public SqlQuerySelectAttribute(SqlQuerySource source, string attrDefName, string expression) : base(source, attrDefName, expression)
        {
        }

        public SqlQuerySelectAttribute(IEnumerable<SqlQuerySourceAttributeRef> attrRefs, string expression)
            : base(attrRefs, expression)
        {
        }

        public override void Build(SqlBuilder builder)
        {
            builder.AddSelect(GetExpression());
        }

        public override string GetExpression()
        {
            var i = 0;
            var attrs = new object[Attributes.Count];

            foreach (var attr in Attributes)
            {
                var s = String.Format("[{0}].[{1}]", attr.Source.AliasName, attr.Attribute.AliasName);
                attrs.SetValue(s, i);
                i++;
            }

            var exp = (string) attrs[0] ?? String.Empty;
            if (!String.IsNullOrEmpty(Expression))
                exp = String.Format(Expression, attrs);
            else if (Summary != SqlQuerySummaryFunction.None)
                switch (Summary)
                {
                    case SqlQuerySummaryFunction.Count:
                        exp = String.Format("count({0})", attrs);
                        break;
                    case SqlQuerySummaryFunction.Sum:
                        exp = String.Format("sum({0})", attrs);
                        break;
                    case SqlQuerySummaryFunction.Avg:
                        exp = String.Format("avg({0})", attrs);
                        break;
                    case SqlQuerySummaryFunction.Max:
                        exp = String.Format("max({0})", attrs);
                        break;
                    case SqlQuerySummaryFunction.Min:
                        exp = String.Format("min({0})", attrs);
                        break;
                }
            return exp;
        }
    }
}