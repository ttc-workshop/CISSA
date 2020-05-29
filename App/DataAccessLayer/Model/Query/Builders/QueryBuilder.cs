using System;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Builders
{
    public class QueryBuilder: BaseExpressionBuilder, IQuery
    {
        public QueryDef Def { get; private set; }

        public QueryBuilder(Guid docDefId, Guid userId) : base(null)
        {
            Def = new QueryDef {Source = new QuerySourceDef {DocDefId = docDefId}, UserId = userId};
        }

        public QueryBuilder(Guid docDefId): this(docDefId, Guid.Empty) {}

        public QueryBuilder(string docDefName, Guid userId)
            : base(null)
        {
            Def = new QueryDef {Source = new QuerySourceDef {DocDefName = docDefName}, UserId = userId};
        }

        public QueryBuilder(string docDefName): this(docDefName, Guid.Empty) {}

        public QueryBuilder(Doc doc, Guid listAttrId, Guid userId)
            : base(null)
        {
            var attrDef = doc.DocDef.Attributes.First(a => a.Id == listAttrId);

            Def = new QueryDef
            {
                DocumentId = doc.Id,
                OwnerDocDefId = doc.DocDef.Id,
                Source = new QuerySourceDef {DocDefId = attrDef.DocDefType.Id},
                ListAttrId = listAttrId,
                ListAttrName = attrDef.Name,
                UserId = userId
            };
        }

        public QueryBuilder(Doc doc, string listAttrName, Guid userId)
            : base(null)
        {
            var attrDef =
                doc.DocDef.Attributes.First(
                    a => String.Equals(a.Name, listAttrName, StringComparison.OrdinalIgnoreCase));

            Def = new QueryDef
            {
                DocumentId = doc.Id,
                OwnerDocDefId = doc.DocDef.Id,
                //DocDefId = attrDef.DocDefType.Id, 
                Source = new QuerySourceDef {DocDefId = attrDef.DocDefType.Id, Alias = ""},
                ListAttrName = listAttrName,
                ListAttrId = attrDef.Id,
                UserId = userId
            };
        }
        public QueryBuilder(Doc doc, string listAttrName)
            : base(null)
        {
            var attrDef =
                doc.DocDef.Attributes.First(
                    a => String.Equals(a.Name, listAttrName, StringComparison.OrdinalIgnoreCase));

            Def = new QueryDef
            {
                DocumentId = doc.Id,
                OwnerDocDefId = doc.DocDef.Id,
                Source = new QuerySourceDef {DocDefId = attrDef.DocDefType.Id},
                ListAttrName = listAttrName,
                ListAttrId = attrDef.Id
            };
        }

        public QueryBuilder(QueryDef queryDef)
            : base(null)
        {
            Def = queryDef;
        }

        public string SetAlias(string alias)
        {
            var helper = new QueryDefHelper(Def);
            var a = QueryDefHelper.PrepareAlias(alias);
            if (helper.UniqueSourceAlias(a))
                Def.Source.Alias = a;
            else
                a = helper.GetUniqueSourceAlias(a);
            return a;
        }

        public IQueryCondition Where(string attribute, string source = null)
        {
            return And(attribute, source);
        }

        internal override void AddCondition(QueryConditionDef condition)
        {
            Def.WhereConditions.Add(condition);
        }

        public IQueryJoin Join(QueryDef query, string alias)
        {
            var helper = new QueryDefHelper(Def);
            var source = new QuerySourceDef {Alias = alias, SubQuery = query};
            var join = helper.AddJoin(source, SqlSourceJoinType.Inner);

            return new QueryJoinBuilder(this, join);
        }

        public IQuery Join(QueryDef query, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                SubQuery = query,
                Alias = alias
            };
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.Inner,
                Source = source
            };
            Def.Sources.Add(source);
            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQuery Join(Guid docDefId, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                DocDefId = docDefId,
                Alias = alias
            };
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.Inner,
                Source = source/*,
                Conditions = expBuilder.Conditions*/
            };
            Def.Sources.Add(source);

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQuery Join(string docDefName, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                DocDefName = docDefName,
                Alias = alias
            };
            Def.Sources.Add(source);
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.Inner,
                Source = source
            };

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQueryJoin LeftJoin(QueryDef query, string alias)
        {
            var helper = new QueryDefHelper(Def);
            var source = new QuerySourceDef { Alias = alias, SubQuery = query };
            var join = helper.AddJoin(source, SqlSourceJoinType.LeftOuter);

            return new QueryJoinBuilder(this, join);
        }

        public IQuery LeftJoin(QueryDef query, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                SubQuery = query,
                Alias = alias
            };
            Def.Sources.Add(source);
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.LeftOuter,
                Source = source
            };

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQuery LeftJoin(Guid docDefId, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                DocDefId = docDefId,
                Alias = alias
            };
            Def.Sources.Add(source);
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.LeftOuter,
                Source = source
            };

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQuery LeftJoin(string docDefName, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                DocDefName = docDefName,
                Alias = alias
            };
            Def.Sources.Add(source);
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.LeftOuter,
                Source = source
            };

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQueryJoin RightJoin(QueryDef query, string alias)
        {
            var helper = new QueryDefHelper(Def);
            var source = new QuerySourceDef { Alias = alias, SubQuery = query };
            var join = helper.AddJoin(source, SqlSourceJoinType.RightOuter);

            return new QueryJoinBuilder(this, join);
        }

        public IQuery RightJoin(QueryDef query, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                SubQuery = query,
                Alias = alias
            };
            Def.Sources.Add(source);
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.RightOuter,
                Source = source
            };

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQuery RightJoin(Guid docDefId, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                DocDefId = docDefId,
                Alias = alias
            };
            Def.Sources.Add(source);
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.RightOuter,
                Source = source
            };

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQuery RightJoin(string docDefName, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                DocDefName = docDefName,
                Alias = alias
            };
            Def.Sources.Add(source);
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.RightOuter,
                Source = source
            };

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQuery FullJoin(Guid docDefId, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                DocDefId = docDefId,
                Alias = alias
            };
            Def.Sources.Add(source);
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.FullOuter,
                Source = source
            };

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQuery FullJoin(string docDefName, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                DocDefName = docDefName,
                Alias = alias
            };
            Def.Sources.Add(source);
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.FullOuter,
                Source = source
            };

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public IQuery FullJoin(QueryDef query, string alias, Action<IQueryExpression> conditionAction)
        {
            var source = new QuerySourceDef
            {
                SubQuery = query,
                Alias = alias
            };
            Def.Sources.Add(source);
            var joinDef = new QueryJoinDef
            {
                Operation = SqlSourceJoinType.FullOuter,
                Source = source
            };

            var expBuilder = new QueryJoinBuilder(this, joinDef);
            Def.Joins.Add(joinDef);
            conditionAction.Invoke(expBuilder);
            return this;
        }

        public QueryDef GetDef()
        {
            return Def;
        }

        public IQuery AddOrderBy(string attrName, bool asc)
        {
            new QueryDefHelper(Def).AddOrderDef(attrName, asc);
            return this;

            /*var order = new QueryOrderDef {AttributeName = attrName, Asc = asc};
            Def.OrderAttributes.Add(order);
            return order;*/
        }

        public IQuery AddOrderBy(Guid attrId, bool asc)
        {
            new QueryDefHelper(Def).AddOrderDef(attrId, asc);
            return this;

            /*var order = new QueryOrderDef { AttributeId = attrId, Asc = asc };
            Def.OrderAttributes.Add(order);
            return order;*/
        }

        internal override QueryConditionDef CreateConditionDef(string attrName, ExpressionOperation operation, string source)
        {
            var def = GetDef();
            if (def != null)
            {
                var helper = new QueryDefHelper(def);

                var sourceDef = String.IsNullOrEmpty(source) ? null /*def.Source*/ : helper.FindSource(source);

                return new QueryConditionDef
                {
                    Operation = operation,
                    Left = new QueryConditionPartDef { Attribute = helper.AddAttribute(attrName, sourceDef) }
                };
            }
            return base.CreateConditionDef(attrName, operation, source);
        }
        internal override QueryConditionDef CreateConditionDef(Guid attrId, ExpressionOperation operation, string source)
        {
            var def = GetDef();
            if (def != null)
            {
                var helper = new QueryDefHelper(def);

                var sourceDef = String.IsNullOrEmpty(source) ? null /*def.Source*/ : helper.FindSource(source);

                return new QueryConditionDef
                {
                    Operation = operation,
                    Left = new QueryConditionPartDef {Attribute = helper.AddAttribute(attrId, sourceDef)}
                };
            }
            return base.CreateConditionDef(attrId, operation, source);
        }

        internal override QuerySourceDef FindSource(string alias)
        {
            var helper = new QueryDefHelper(Def);
            return helper.FindSource(alias);
        }
    }

    public class QueryJoinBuilder : BaseExpressionBuilder, IQueryJoin
    {
        public QueryJoinDef Def { get; private set; }
        public QueryJoinBuilder(BaseQueryBuilder parent, QueryJoinDef join) : base (parent)
        {
            Def = join;
        }

        internal override void AddCondition(QueryConditionDef condition)
        {
            Def.Conditions.Add(condition);
        }


        public IQueryCondition On(string source, string attrName)
        {
            var sourceDef = Parent.GetSource(source);

            var condition = new QueryConditionDef
            {
                Operation = ExpressionOperation.And,
                Left = new QueryConditionPartDef
                {
                    Attribute = new QuerySingleAttributeDef
                    {
                        Alias = attrName,
                        Attribute = new QueryAttributeRef { Source = sourceDef, AttributeName = attrName}
                    }
                }
            };
            AddCondition(condition);

            return new QueryConditionBuilder(this, condition);
        }
    }
}
