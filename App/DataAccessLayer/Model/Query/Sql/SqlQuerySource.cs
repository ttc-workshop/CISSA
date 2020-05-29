using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public abstract class SqlQuerySource : SqlQueryBase
    {
        public Guid UserId { get; set; }
        public string AliasName { get; set; }

        private readonly List<SqlQuerySourceAttribute> _attributes = new List<SqlQuerySourceAttribute>();
        public List<SqlQuerySourceAttribute> Attributes { get { return _attributes; } }

        public int AttributeAliasNo { get; private set; }
        public List<string> AttributeAliases { get; private set; }

        public bool WithNoLock { get; set; }

        protected SqlQuerySource(string alias)
        {
            AliasName = alias;
        }

        public SqlQuerySourceAttribute FindAttribute(Guid attrDefId)
        {
            return Attributes.FirstOrDefault(a => a.Def != null && a.Def.Id == attrDefId);
        }

        public SqlQuerySourceAttribute FindAttribute(SystemIdent ident)
        {
            return Attributes.FirstOrDefault(a => a.Def == null && a.Ident == ident);
        }

        public SqlQuerySourceAttribute FindAttribute(string attrDefName)
        {
            /*if (attrDefName.Length > 0 && attrDefName[0] == '&')
            {
                var ident = SystemIdentConverter.Convert(attrDefName);

                return FindAttribute(ident);
            }
            return Attributes != null
                ? Attributes.FirstOrDefault(
                    a =>
                        a.Def != null &&
                        String.Equals(a.Def.Name, attrDefName, StringComparison.OrdinalIgnoreCase))
                : null;*/
            return Attributes != null
                ? Attributes.FirstOrDefault(a => a.SameAttrName(attrDefName))
                : null;
        }

        public SqlQuerySourceAttribute GetAttribute(Guid attrDefId)
        {
            return FindAttribute(attrDefId) ?? AddAttribute(attrDefId);
        }
        public SqlQuerySourceAttribute GetAttribute(string attrDefName)
        {
            return FindAttribute(attrDefName) ?? AddAttribute(attrDefName);
        }
        public SqlQuerySourceAttribute GetAttribute(SystemIdent attrIdent)
        {
            return FindAttribute(attrIdent) ?? AddAttribute(attrIdent);
        }
        public SqlQuerySourceAttribute GetAttribute(QueryAttributeRef attrRef)
        {
            if (attrRef.AttributeId != Guid.Empty)
                return FindAttribute(attrRef.AttributeId) ?? AddAttribute(attrRef.AttributeId);
            return FindAttribute(attrRef.AttributeName) ?? AddAttribute(attrRef.AttributeName);
        }

        protected abstract SqlQuerySourceAttribute AddAttribute(Guid attrDefId);
        protected abstract SqlQuerySourceAttribute AddAttribute(string attrDefName);
        protected abstract SqlQuerySourceAttribute AddAttribute(SystemIdent attrIdent);

        public abstract AttrDef FindAttributeDef(Guid attrDefId);
        public abstract AttrDef FindAttributeDef(string attrDefName);

        public bool HasAttributeDef(Guid attrDefId)
        {
            return FindAttributeDef(attrDefId) != null;
        }

        public bool HasAttributeDef(string attrDefName)
        {
            if (attrDefName.Length > 0 && attrDefName[0] == '&')
            {
                SystemIdent ident;
                if (SystemIdentConverter.TryConvert(attrDefName, out ident))
                    return true;
            }

            return FindAttributeDef(attrDefName) != null;
        }

        public abstract DocDef GetDocDef();
        public bool IsDocDef(Guid docDefId)
        {
            var docDef = GetDocDef();
            return docDef != null && docDef.Id == docDefId;
        }

        public bool IsDocDef(DocDef docDef)
        {
            return IsDocDef(docDef.Id);
        }

        public virtual bool IsSame(QuerySourceDef sourceDef)
        {
            if (sourceDef == null) return false;

            var docDef = GetDocDef();

            return docDef != null && ((sourceDef.DocDefId != Guid.Empty && docDef.Id == sourceDef.DocDefId) ||
                                      String.CompareOrdinal(docDef.Name, sourceDef.DocDefName) == 0 &&
                                      !String.IsNullOrEmpty(sourceDef.DocDefName));
        }

        protected string GetAttributeAlias(string alias)
        {
            if (AttributeAliases == null) AttributeAliases = new List<string>();

            if (!String.IsNullOrEmpty(alias))
            {
                int i = 0;
                alias = alias.Replace(" ", "_");
                alias = alias.Replace("&", "");

                if (String.Equals(alias, "Id", StringComparison.OrdinalIgnoreCase) || AttributeAliases.Contains(alias.ToUpper()))
                {
                    do
                    {
                        i++;
                        alias = alias + i;
                    } while (AttributeAliases.Contains(alias.ToUpper()));
                }
            }
            else
            {
                do
                {
                    AttributeAliasNo++;
                    alias = "attr" + AttributeAliasNo;
                } while (AttributeAliases.Contains(alias.ToUpper()));
            }
            return alias;
        }
    }
}