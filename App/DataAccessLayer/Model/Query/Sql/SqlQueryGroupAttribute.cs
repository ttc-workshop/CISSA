using System;
using System.Collections.Generic;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryGroupAttribute : SqlQueryAttribute
    {
        public SqlQueryGroupAttribute(SqlQuerySource source, SqlQuerySourceAttribute attribute)
            : base(source, attribute) {}

        public SqlQueryGroupAttribute(SqlQuerySource source, Guid attrDefId) : base(source, attrDefId) {}

        public SqlQueryGroupAttribute(SqlQuerySource source, string attrDefName) : base(source, attrDefName) {}

        public SqlQueryGroupAttribute(SqlQuerySource source, SystemIdent attrIdent) : base(source, attrIdent) { }

        public SqlQueryGroupAttribute(SqlQuerySource source, string attrDefName, string expression) : base(source, attrDefName, expression) { }
        public SqlQueryGroupAttribute(IEnumerable<SqlQuerySourceAttributeRef> attrRefs, string expression) : base(attrRefs, expression) { }

        public override void Build(SqlBuilder builder)
        {
            builder.AddOrderBy(GetExpression());
        }
    }
}