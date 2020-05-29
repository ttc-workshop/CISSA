using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryAttribute : SqlQueryItem
    {
        private readonly List<SqlQuerySourceAttributeRef> _attributes = new List<SqlQuerySourceAttributeRef>();
        public List<SqlQuerySourceAttributeRef> Attributes { get { return _attributes; } }

        public string Expression { get; private set; }
        private string _alias;
        public string Alias
        {
            get { return _alias; }
            set { if (GetName() != value) _alias = value; }
        }

        public SqlQuerySourceAttribute Attribute
        {
            get { return _attributes[0].Attribute; }
        }
        public SqlQuerySource Source
        {
            get { return _attributes[0].Source; }
        }

        protected SqlQueryAttribute(SqlQuerySource source, SqlQuerySourceAttribute attribute)
        {
            _attributes.Add(new SqlQuerySourceAttributeRef(source, attribute));
        }

        protected SqlQueryAttribute(SqlQuerySource source, Guid attrDefId)
        {
            _attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefId)));
        }

        protected SqlQueryAttribute(SqlQuerySource source, string attrDefName)
        {
            _attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefName)));
        }

        protected SqlQueryAttribute(SqlQuerySource source, SystemIdent attrIdent)
        {
            _attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrIdent)));
        }

        protected SqlQueryAttribute(SqlQuerySource source, SystemIdent attrIdent, string exp) : this(source, attrIdent)
        {
            Expression = exp;
        }

        protected SqlQueryAttribute(SqlQuerySource source, Guid attrDefId, string expression)
            : this(source, attrDefId)
        {
            Expression = expression;
        }

        protected SqlQueryAttribute(SqlQuerySource source, string attrDefName, string expression)
            : this(source, attrDefName)
        {
            Expression = expression;
        }

        protected SqlQueryAttribute(IEnumerable<SqlQuerySourceAttributeRef> attrRefs, string expression)
        {
            _attributes.AddRange(attrRefs);
            Expression = expression;
        }

        public string GetAttrDefTableName()
        {
            return Attribute.GetAttrDefTableName();
        }

        public string AliasName { get { return Attribute.AliasName; } }
        public AttrDef Def { get { return Attribute.Def; } }
        public string SourceName { get { return Source.AliasName; } }
        public bool IsSystemIdent { get { return Attribute.Def == null; } }

        public SystemIdent Ident { get { return Attribute.Ident; } }

        public bool IsMultiExpression { get { return Attributes.Count > 1; } }

        public virtual void Build(SqlBuilder builder)
        {
            throw new NotImplementedException();
        }

        public virtual string GetExpression()
        {
            var i = 0;
            object[] attrs = new object[Attributes.Count];

            foreach (var attr in Attributes)
                attrs.SetValue("[" + attr.Source.AliasName + "].[" + attr.Attribute.AliasName + "]", i++);

            var exp = attrs[0] != null ? (string) attrs[0] : String.Empty;
            if (!String.IsNullOrEmpty(Expression))
                exp = String.Format(Expression, attrs);

            return exp;
        }

        public string GetName()
        {
            return _attributes.Count > 0 ? Attribute.AliasName : "Noname";
        }
    }
}