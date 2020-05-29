using System;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQuerySubSourceAttribute : SqlQuerySourceAttribute
    {
        public SqlQueryAttribute Attribute { get; private set; }

        public SqlQuerySubSourceAttribute(SqlQueryAttribute attribute, string alias)
            : base(alias)
        {
            Attribute = attribute;
        }

        public override AttrDef Def
        {
            get { return Attribute.Def; }
        }

        public override SystemIdent Ident
        {
            get { return Attribute.Attribute.Ident; }
        }

        public override bool SameAttrName(string name)
        {
            if (Attribute != null &&
                String.Equals(Attribute.Alias, name, StringComparison.OrdinalIgnoreCase))
                return true;

            return base.SameAttrName(name);
        }
    }
}