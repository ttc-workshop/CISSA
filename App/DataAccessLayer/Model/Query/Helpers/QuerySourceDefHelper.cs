using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers
{
    public class QuerySourceDefHelper
    {
        public QuerySourceDef Def { get; private set; }

        public QuerySourceDefHelper(QuerySourceDef def)
        {
            Def = def;
        }

        public bool IsSame(DocDef docDef)
        {
            return Def != null &&
                   ((Def.DocDefId == Guid.Empty && String.CompareOrdinal(Def.DocDefName, docDef.Name) == 0) ||
                    Def.DocDefId == docDef.Id);
        }
    }

    public class QueryDefHelper
    {
        public QueryDef Def { get; private set; }

        private readonly IDictionary<string, QueryAttributeDef> _attributeAlias = new Dictionary<string, QueryAttributeDef>();
        private readonly IDictionary<string, QuerySourceDef> _sourceAliases = new Dictionary<string, QuerySourceDef>();
        private int _aliasNo;
        private int _sourceAliasNo;

        public QueryDefHelper(QueryDef def)
        {
            Def = def;

            Def.Attributes.ForEach(a => { if (!String.IsNullOrEmpty(a.Alias)) _attributeAlias.Add(a.Alias, a); });
            if (Def.Source != null)
            {
                if (!String.IsNullOrEmpty(Def.Source.Alias)) _sourceAliases.Add(Def.Source.Alias, Def.Source);
            }
            Def.Sources.ForEach(
                s => { if (!String.IsNullOrEmpty(s.Alias)) _sourceAliases.Add(s.Alias, s); });
        }
        public QueryAttributeDef FindAttribute(string name)
        {
            if (Def.Attributes != null)
            {
                return
                    Def.Attributes.FirstOrDefault(
                        a => String.Equals(a.Alias, name, StringComparison.OrdinalIgnoreCase)) ??
                    Def.Attributes.FirstOrDefault(a => new QueryAttributeDefHelper(a).IsSame(name));
            }
            return null;
        }
        public QueryAttributeDef FindAttribute(string attrName, string source)
        {
            if (Def.Attributes != null)
            {
                return
                    Def.Attributes.FirstOrDefault(a => new QueryAttributeDefHelper(a).IsSame(attrName, source));
            }
            return null;
        }
        public QueryAttributeDef FindAttribute(string attrName, QuerySourceDef source)
        {
            if (Def.Attributes != null)
            {
                return
                    Def.Attributes.FirstOrDefault(a => new QueryAttributeDefHelper(a).IsSame(attrName, source));
            }
            return null;
        }
        public QueryAttributeDef FindAttribute(Guid id)
        {
            return Def.Attributes != null ? Def.Attributes.FirstOrDefault(a => new QueryAttributeDefHelper(a).IsSame(id)) : null;
        }
        public QueryAttributeDef FindAttribute(Guid id, string source)
        {
            return Def.Attributes != null ? Def.Attributes.FirstOrDefault(a => new QueryAttributeDefHelper(a).IsSame(id, source)) : null;
        }

        public QueryAttributeDef AddAttribute(string attrName)
        {
            var attr = FindAttribute(attrName);
            if (attr == null)
            {
                var alias = GetUniqueAttributeAlias(attrName);
                attr = QueryAttributeDefHelper.Create(attrName, alias);
                Def.Attributes.Add(attr);
            }
            return attr;
        }
        public QueryAttributeDef AddAttribute(string attrName, string source)
        {
            var attr = FindAttribute(attrName, source);
            if (attr == null)
            {
                var alias = GetUniqueAttributeAlias(attrName);
                var sourceDef = FindSource(source);
                attr = QueryAttributeDefHelper.Create(attrName, alias, sourceDef);
                Def.Attributes.Add(attr);
            }
            return attr;
        }
        public QueryAttributeDef AddAttribute(string attrName, QuerySourceDef source)
        {
            var attr = FindAttribute(attrName, source);
            if (attr == null)
            {
                var alias = GetUniqueAttributeAlias(attrName);
                attr = QueryAttributeDefHelper.Create(attrName, alias, source);
                Def.Attributes.Add(attr);
            }
            return attr;
        }
        public QueryAttributeDef AddAttribute(Guid id)
        {
            var attr = FindAttribute(id);
            if (attr == null)
            {
                var alias = GetUniqueAttributeAlias(DefaultAlias);
                attr = QueryAttributeDefHelper.Create(id, alias);
                Def.Attributes.Add(attr);
            }
            return attr;
        }
        public QueryAttributeDef AddAttribute(Guid id, string source)
        {
            var attr = FindAttribute(id);
            if (attr == null)
            {
                var alias = GetUniqueAttributeAlias(DefaultAlias);
                var sourceDef = FindSource(source);
                attr = QueryAttributeDefHelper.Create(id, alias, sourceDef);
                Def.Attributes.Add(attr);
            }
            return attr;
        }
        public QueryAttributeDef AddAttribute(Guid id, QuerySourceDef source)
        {
            var attr = FindAttribute(id);
            if (attr == null)
            {
                var alias = GetUniqueAttributeAlias(DefaultAlias);
                attr = QueryAttributeDefHelper.Create(id, alias, source);
                Def.Attributes.Add(attr);
            }
            return attr;
        }

        public QuerySourceDef FindSource(string alias)
        {
            if (String.IsNullOrWhiteSpace(alias))
                return null;

            if (Def.Source != null && String.Equals(Def.Source.Alias, alias, StringComparison.OrdinalIgnoreCase))
                return Def.Source;

            return
                Def.Sources.FirstOrDefault(s => String.Equals(s.Alias, alias, StringComparison.OrdinalIgnoreCase));
        }

        public QueryJoinDef AddJoin(QuerySourceDef source, SqlSourceJoinType joinType)
        {
            Def.Sources.Add(source);
            if (!String.IsNullOrWhiteSpace(source.Alias)) _sourceAliases.Add(source.Alias, source);
            else
            {
                _sourceAliasNo++;
                while (_sourceAliases.ContainsKey("source" + _sourceAliasNo)) _sourceAliasNo++;

                source.Alias = "source" + _sourceAliasNo;
                _sourceAliases.Add(source.Alias, source);
            }
            var join = new QueryJoinDef {Source = source, Operation = joinType};
            Def.Joins.Add(join);
            return join;
        }

        public QueryOrderDef AddOrderDef(string name, bool asc)
        {
            var attr = AddAttribute(name);
            var orderDef = new QueryOrderDef {Attribute = attr, Asc = asc};
            Def.OrderAttributes.Add(orderDef);
            return orderDef;
        }
        public QueryOrderDef AddOrderDef(Guid id, bool asc)
        {
            var attr = AddAttribute(id);
            var orderDef = new QueryOrderDef { Attribute = attr, Asc = asc };
            Def.OrderAttributes.Add(orderDef);
            return orderDef;
        }

        public static readonly string DefaultAlias = "attr";

        public bool UniqueSourceAlias(string alias)
        {
            return !_sourceAliases.ContainsKey(alias);
        }

        public bool UniqueAttributeAlias(string alias)
        {
            return !_attributeAlias.ContainsKey(alias);
        }

        public string GetUniqueAttributeAlias(string name)
        {
            var alias = PrepareAlias(name);
            do
            {
                _aliasNo++;
            } while (_attributeAlias.ContainsKey(alias + "_" + _aliasNo));

            return alias + "_" + _aliasNo;
        }

        public static string PrepareAlias(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return DefaultAlias;

            var i = 0;
            var alias = "";
            foreach (var c in name.Replace(' ', '_'))
            {
                if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c == '_')
                    alias += c;
                else if (i > 0 && c >= '0' || c <= '9')
                    alias += c;
                i++;
            }
            if (alias.Length == 0) return DefaultAlias;
            return alias;
        }

        public string GetUniqueSourceAlias(string name)
        {
            var alias = PrepareAlias(name);
            do
            {
                _sourceAliasNo++;
            } while (_sourceAliases.ContainsKey(alias + "_" + _aliasNo));

            return alias + "_" + _aliasNo;
        }
    }

    public class QueryAttributeDefHelper
    {
        public QueryAttributeDef Def { get; private set; }

        public string AttributeName
        {
            get
            {
                var single = Def as QuerySingleAttributeDef;
                if (single != null)
                    return single.Attribute != null
                        ? single.Attribute.AttributeName
                        : String.Empty;

                var exp = Def as QueryExpAttributeDef;

                return exp != null && exp.Attributes != null && exp.Attributes.Count > 0
                    ? exp.Attributes[0].AttributeName
                    : String.Empty;
            }
        }

        public Guid AttributeId
        {
            get
            {
                var single = Def as QuerySingleAttributeDef;
                if (single != null)
                    return single.Attribute != null
                        ? single.Attribute.AttributeId
                        : Guid.Empty;

                var exp = Def as QueryExpAttributeDef;
                return exp != null && exp.Attributes != null && exp.Attributes.Count > 0
                    ? exp.Attributes[0].AttributeId
                    : Guid.Empty;
            }
        }

        public QueryAttributeDefHelper(QueryAttributeDef def)
        {
            Def = def;
        }

        public bool IsSame(string name)
        {
            var single = Def as QuerySingleAttributeDef;

            return single != null && single.Attribute != null &&
                   String.Equals(single.Attribute.AttributeName, name, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsSame(string attrName, string source)
        {
            var single = Def as QuerySingleAttributeDef;

            return single != null && single.Attribute != null &&
                   (String.Equals(single.Attribute.AttributeName, attrName, StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(single.Alias, attrName, StringComparison.OrdinalIgnoreCase)) &&
                   single.Attribute.Source != null &&
                   String.Equals(single.Attribute.Source.Alias, source, StringComparison.OrdinalIgnoreCase);
        }
        public bool IsSame(string attrName, QuerySourceDef source)
        {
            var single = Def as QuerySingleAttributeDef;

            return single != null && single.Attribute != null &&
                   (String.Equals(single.Attribute.AttributeName, attrName, StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(single.Alias, attrName, StringComparison.OrdinalIgnoreCase)) &&
                   single.Attribute.Source == source;
        }

        public bool IsSingle()
        {
            return Def is QuerySingleAttributeDef;
        }

        public bool IsSame(Guid id)
        {
            var single = Def as QuerySingleAttributeDef;

            return single != null && single.Attribute != null && single.Attribute.AttributeId == id;
        }
        public bool IsSame(Guid id, string source)
        {
            var single = Def as QuerySingleAttributeDef;

            return single != null && single.Attribute != null && single.Attribute.AttributeId == id &&
                   single.Attribute.Source != null &&
                   String.Compare(single.Attribute.Source.Alias, source, StringComparison.OrdinalIgnoreCase) == 0;
        }
        public bool IsSame(Guid id, QuerySourceDef source)
        {
            var single = Def as QuerySingleAttributeDef;

            return single != null && single.Attribute != null && single.Attribute.AttributeId == id &&
                   single.Attribute.Source != source;
        }

        public static QueryAttributeDef Create(string name, string alias)
        {
            return new QuerySingleAttributeDef {Alias = alias, Attribute = new QueryAttributeRef {AttributeName = name}};
        }
        public static QueryAttributeDef Create(string name, string alias, QuerySourceDef source)
        {
            return new QuerySingleAttributeDef
            {
                Alias = alias,
                Attribute = new QueryAttributeRef {AttributeName = name, Source = source}
            };
        }


        public static QueryAttributeDef Create(Guid id, string alias)
        {
            return new QuerySingleAttributeDef { Alias = alias, Attribute = new QueryAttributeRef { AttributeId = id } };
        }
        public static QueryAttributeDef Create(Guid id, string alias, QuerySourceDef source)
        {
            return new QuerySingleAttributeDef
            {
                Alias = alias,
                Attribute = new QueryAttributeRef {AttributeId = id, Source = source}
            };
        }


        public bool IsSame(DocDef docDef, string attrName)
        {
            var single = Def as QuerySingleAttributeDef;

            return single != null && single.Attribute != null &&
                   String.CompareOrdinal(single.Attribute.AttributeName, attrName) == 0 &&
                   new QuerySourceDefHelper(single.Attribute.Source).IsSame(docDef);
        }
        public bool IsSame(DocDef docDef, Guid attrId)
        {
            var single = Def as QuerySingleAttributeDef;

            return single != null && single.Attribute != null &&
                   single.Attribute.AttributeId == attrId &&
                   new QuerySourceDefHelper(single.Attribute.Source).IsSame(docDef);
        }
        public bool IsSame(DocDef docDef, AttrDef attrDef)
        {
            var single = Def as QuerySingleAttributeDef;

            return single != null && single.Attribute != null &&
                   (single.Attribute.AttributeId == attrDef.Id ||
                    String.CompareOrdinal(single.Attribute.AttributeName, attrDef.Name) == 0) &&
                   new QuerySourceDefHelper(single.Attribute.Source).IsSame(docDef);
        }

        public bool HasSource()
        {
            var single = Def as QuerySingleAttributeDef;
            if (single != null)
            {
                return single.Attribute != null && single.Attribute.Source != null &&
                       (single.Attribute.Source.DocDefId != Guid.Empty ||
                        !String.IsNullOrEmpty(single.Attribute.Source.DocDefName));
            }
            var exp = Def as QueryExpAttributeDef;
            if (exp != null)
            {
                return exp.Attributes != null &&
                       exp.Attributes.Exists(a => a.Source != null && (a.Source.DocDefId != Guid.Empty ||
                                                                       !String.IsNullOrEmpty(a.Source.DocDefName)));
            }
            return false;
        }

        public QueryAttributeRef GetAttributeRef()
        {
            var single = Def as QuerySingleAttributeDef;
            if (single != null) return single.Attribute;
            var exp = Def as QueryExpAttributeDef;
            if (exp != null && exp.Attributes != null) return exp.Attributes[0];
            return null;
        }

        public QuerySourceDef GetSourceDef()
        {
            var single = Def as QuerySingleAttributeDef;
            if (single != null) return single.Attribute.Source;
            var exp = Def as QueryExpAttributeDef;
            if (exp != null && exp.Attributes != null) return exp.Attributes[0].Source;
            return null;
        }
    }
}
