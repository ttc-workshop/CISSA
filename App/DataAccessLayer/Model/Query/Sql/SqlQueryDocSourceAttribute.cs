using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryDocSourceAttribute : SqlQuerySourceAttribute
    {
        private readonly SystemIdent _ident;
        private readonly AttrDef _def;

        public SqlQueryDocSourceAttribute(AttrDef attrDef, string alias) : base(alias)
        {
            _def = attrDef;
        }

        public SqlQueryDocSourceAttribute(SystemIdent attrIdent, string alias) : base(alias)
        {
            _def = null;
            _ident = attrIdent;
        }

        public override AttrDef Def
        {
            get { return _def; }
        }

        public override SystemIdent Ident
        {
            get { return _ident; }
        }
    }
}