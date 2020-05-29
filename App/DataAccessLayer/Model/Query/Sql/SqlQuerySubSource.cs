using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQuerySubSource : SqlQuerySource
    {
        public SqlQueryObject SubQuery { get; private set; }

        public SqlQuerySubSource(SqlQueryObject subQuery, string alias): base(alias)
        {
            SubQuery = subQuery;
        }


        public override SqlBuilder BuildSql(ICollection<SqlQueryCondition> conditions, bool isMain = false)
        {
            return BuildSql(Attributes, conditions, isMain);
        }

        public override SqlBuilder BuildSql(ICollection<SqlQuerySourceAttribute> attrs, ICollection<SqlQueryCondition> conditions, bool isMain = false)
        {
            var builder = new SqlBuilder();

            builder.SetFrom(String.Format("({0}) AS [{1}]", SubQuery.BuildSql(null, isMain), AliasName));

            if (attrs != null && Attributes != null)
            {
                foreach (var attr in attrs)
                {
                    if (Attributes.Contains(attr))
                        builder.AddSelect(String.Format("[{0}].[{2}] AS [{1}]", AliasName, attr.AliasName, GetSubQueryAttrAlias(attr)));
                }
            }
            return builder;
        }

        private static object GetSubQueryAttrAlias(SqlQuerySourceAttribute attr)
        {
            var subQueryAttr = attr as SqlQuerySubSourceAttribute;
            if (subQueryAttr != null && subQueryAttr.Attribute != null)
                return String.IsNullOrEmpty(subQueryAttr.Attribute.Alias)
                    ? subQueryAttr.Attribute.AliasName
                    : subQueryAttr.Attribute.Alias;
            return attr.AliasName;
        }

        protected override SqlQuerySourceAttribute AddAttribute(Guid attrDefId)
        {
            var subAttribute = SubQuery.GetAttribute(attrDefId);

            //var alias = GetAttributeAlias(subAttribute.AliasName);
            var alias = GetAttributeAlias(String.IsNullOrEmpty(subAttribute.Alias) ? subAttribute.AliasName : subAttribute.Alias);
            if (!String.IsNullOrEmpty(AliasName)) alias = AliasName + "_" + alias;
            AttributeAliases.Add(alias.ToUpper());
            var attribute = new SqlQuerySubSourceAttribute(subAttribute, alias);

            Attributes.Add(attribute);

            return attribute;
        }

        protected override SqlQuerySourceAttribute AddAttribute(string attrDefName)
        {
            var subAttribute = SubQuery.GetAttribute(attrDefName);

            var alias = GetAttributeAlias(String.IsNullOrEmpty(subAttribute.Alias) ? subAttribute.AliasName : subAttribute.Alias); //subAttribute.AliasName);
            if (!String.IsNullOrEmpty(AliasName)) alias = AliasName + "_" + alias;
            AttributeAliases.Add(alias.ToUpper());
            var attribute = new SqlQuerySubSourceAttribute(subAttribute, alias);

            Attributes.Add(attribute);

            return attribute;
        }

        protected override SqlQuerySourceAttribute AddAttribute(SystemIdent attrIdent)
        {
            var subAttribute = SubQuery.GetAttribute(attrIdent);

            //var alias = GetAttributeAlias(subAttribute.AliasName);
            var alias = GetAttributeAlias(String.IsNullOrEmpty(subAttribute.Alias) ? subAttribute.AliasName : subAttribute.Alias);
            if (!String.IsNullOrEmpty(AliasName)) alias = AliasName + "_" + alias;
            AttributeAliases.Add(alias.ToUpper());
            var attribute = new SqlQuerySubSourceAttribute(subAttribute, alias);

            Attributes.Add(attribute);

            return attribute;
        }

        public override AttrDef FindAttributeDef(Guid attrDefId)
        {
            var attribute = SubQuery.FindAttribute(attrDefId);
            if (attribute != null) return attribute.Def;
            return null;
        }

        public override AttrDef FindAttributeDef(string attrDefName)
        {
            var attribute = SubQuery.FindAttribute(attrDefName);
            if (attribute != null) return attribute.Def;
            return null;
        }

        public override bool IsSame(QuerySourceDef sourceDef)
        {
            if (sourceDef == null) return false;

            if (sourceDef.SubQuery == null) return false;

            if (String.Equals(sourceDef.Alias, AliasName)) return true;

            // TODO: Доделать сравнение, чтобы точно возвращать соответствие
            if (sourceDef.SubQuery.Source.DocDefId == SubQuery.Def.Id)
                return true;

            /*return docDef != null && ((sourceDef.DocDefId != Guid.Empty && docDef.Id == sourceDef.DocDefId) ||
                                      String.CompareOrdinal(docDef.Name, sourceDef.DocDefName) == 0 &&
                                      !String.IsNullOrEmpty(sourceDef.DocDefName));
             */
            return false;
        }

        public override DocDef GetDocDef()
        {
            return SubQuery.Def;
        }
    }
}