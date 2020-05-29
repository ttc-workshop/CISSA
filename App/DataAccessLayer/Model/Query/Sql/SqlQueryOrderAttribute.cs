using System;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryOrderAttribute : SqlQueryAttribute
    {
        public bool Asc { get; set; }

        public SqlQueryOrderAttribute(SqlQuerySource source, SqlQuerySourceAttribute attribute, bool asc) : base(source, attribute)
        {
            Asc = asc;
        }

        public SqlQueryOrderAttribute(SqlQuerySource source, Guid attrDefId, bool asc) : base(source, attrDefId)
        {
            Asc = asc;
        }

        public SqlQueryOrderAttribute(SqlQuerySource source, string attrDefName, bool asc) : base(source, attrDefName)
        {
            Asc = asc;
        }
        public SqlQueryOrderAttribute(SqlQuerySource source, SystemIdent attrIdent, bool asc)
            : base(source, attrIdent)
        {
            Asc = asc;
        }

        public SqlQueryOrderAttribute(SqlQuerySource source, SystemIdent attrIdent, string exp, bool asc)
            : base(source, attrIdent, exp)
        {
            Asc = asc;
        }

        public SqlQueryOrderAttribute(SqlQuerySource source, string attrDefName, string expression, bool asc)
            : base(source, attrDefName, expression)
        {
            Asc = asc;
        }

        public SqlQueryOrderAttribute(SqlQuerySource source, Guid attrDefId, string expression, bool asc)
            : base(source, attrDefId, expression)
        {
            Asc = asc;
        }

        public SqlQueryOrderAttribute(SqlQuerySource source, Guid attrDefId) : this(source, attrDefId, true) { }

        public SqlQueryOrderAttribute(SqlQuerySource source, string attrDefName) : this(source, attrDefName, true) {}

        public override void Build(SqlBuilder builder)
        {
            builder.AddOrderBy(GetExpression());
        }

        public override string GetExpression()
        {
            var exp = base.GetExpression();

            if (!Asc) return exp + " desc";

            return exp;
        }
    }
}