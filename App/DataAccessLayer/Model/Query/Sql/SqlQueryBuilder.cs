using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public static class SqlQueryBuilder
    {
        public static SqlQuery Build(QueryBuilder builder)
        {
            return Build(builder.Def);
        }

        public static SqlQuery Build(IDataContext dataContext, QueryDef def)
        {
            if (def.Source == null)
                throw new PropertyConstraintException("QueryDef Source not defined!");

            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var bldr = new SqlQueryBuilderTool(provider, dataContext, def.UserId);

                return bldr.Build(def);
            }
            /*using (var defRepo = new DocDefRepository(dataContext))
            {
                var docDef = def.Source.DocDefId == Guid.Empty
                                 ? defRepo.DocDefByName(def.Source.DocDefName)
                                 : defRepo.DocDefById(def.Source.DocDefId);

                var query = new SqlQuery(dataContext, docDef, def.Alias, def.UserId)
                                {
                                    DocumentId = def.DocumentId, 
                                    ListAttrDefId = def.ListAttrId
                                };
                try
                {
                    foreach (var source in def.Sources)
                    {
                        BuildSource(query, source, defRepo, dataContext);
                    }
                    foreach (var join in def.Joins)
                    {
                        BuildJoin(query, join, docDef, dataContext);
                    }
                    foreach (var condition in def.WhereConditions)
                    {
                        BuildCondition(query, condition, docDef, null);
                    }
                    foreach (var order in def.OrderAttributes)
                    {
                        query.AddOrderAttribute(order);
                    }
                    return query;
                }
                catch
                {
                    query.Dispose();
                    throw;
                }
            }*/
        }

        public static SqlQuery Build(QueryDef def)
        {
            return Build(null, def);
        }

        public static SqlQuery Build(IDataContext dataContext, QueryDef def, Guid userId)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var bldr = new SqlQueryBuilderTool(provider, dataContext, userId);

                return bldr.Build(def);
            }
            /*using (var defRepo = new DocDefRepository(dataContext, userId))
            {
                var docDef = def.DocDefId == Guid.Empty
                                 ? defRepo.DocDefByName(def.DocDefName)
                                 : defRepo.DocDefById(def.DocDefId);

                var query = new SqlQuery(dataContext, docDef, userId)
                                {
                                    DocumentId = def.DocumentId, 
                                    ListAttrDefId = def.ListAttrId
                                };
                try
                {
                    foreach (var source in def.Sources)
                    {
                        BuildSource(query, source, defRepo, dataContext);
                    }
                    foreach (var join in def.Joins)
                    {
                        BuildJoin(query, join, docDef, dataContext);
                    }
                    foreach (var condition in def.WhereConditions)
                    {
                        BuildCondition(query, condition, docDef, null);
                    }
                    foreach (var order in def.OrderAttributes)
                    {
                        query.AddOrderAttribute(order);
                    }
                    return query;
                }
                catch
                {
                    query.Dispose();
                    throw;
                }
            }*/
        }

        public static SqlQuery Build(QueryDef def, Guid userId)
        {
            return Build(null, def, userId);
        }

        private static void BuildSource(SqlQuery query, QuerySourceDef source, IDocDefRepository defRepo, IDataContext dataContext)
        {
            if (source.SubQuery != null)
            {
                var subQuery = Build(dataContext, source.SubQuery);
                query.Sources.Add(new SqlQuerySubSource(subQuery, source.Alias));
            }
            else if (source.DocDefId != Guid.Empty)
            {
                query.Sources.Add(new SqlQueryDocSource(query.Provider, source.DocDefId, source.Alias));
            }
            else if (!String.IsNullOrEmpty(source.DocDefName))
            {
                var sourceDocDef = defRepo.DocDefByName(source.DocDefName);
                query.Sources.Add(new SqlQueryDocSource(query.Provider, sourceDocDef, source.Alias));
            }
        }

        private static void BuildJoin(SqlQuery query, QueryJoinDef join, DocDef docDef, IDataContext dataContext)
        {
            var source =
                query.Sources.First(
                    s => String.Equals(s.AliasName, join.Source.Alias, StringComparison.OrdinalIgnoreCase));
            var queryJoin = new SqlQueryJoin(source, join.Operation);
            foreach (var condition in join.Conditions)
            {
                BuildJoinCondition(query, queryJoin, condition, docDef, null, dataContext);
            }
            query.SourceJoins.Add(queryJoin);
        }

        private static void BuildCondition(SqlQuery query, QueryConditionDef condition, DocDef docDef, SqlQueryCondition parentCondition)
        {
            if (condition.Condition != ConditionOperation.Include && condition.Condition != ConditionOperation.Exp)
            {
                if (condition.SubQueryDef != null)
                {
                    var subQuery = Build(query.DataContext, condition.SubQueryDef);
                    if (String.IsNullOrEmpty(condition.AttributeName))
                        query.AddCondition(condition.Operation, docDef, condition.AttributeId, condition.Condition,
                                           subQuery, condition.SubQueryAttribute, parentCondition);
                    else
                        query.AddCondition(condition.Operation, docDef, condition.AttributeName, condition.Condition,
                                           subQuery, condition.SubQueryAttribute, parentCondition);
                }
                else
                    if (String.IsNullOrEmpty(condition.AttributeName))
                        query.AddCondition(condition.Operation, docDef, condition.AttributeId, condition.Condition, condition.Values, parentCondition);
                    else
                        query.AddCondition(condition.Operation, docDef, condition.AttributeName, condition.Condition,
                                           condition.Values, parentCondition);
            }
            else if (condition.Condition == ConditionOperation.Include && condition.Conditions != null && condition.Conditions.Count > 0)
            {
                var attrDef = String.IsNullOrEmpty(condition.AttributeName)
                                  ? docDef.Attributes.First(a => a.Id == condition.AttributeId)
                                  : docDef.Attributes.First(
                                      a => String.Equals(a.Name, condition.AttributeName, StringComparison.OrdinalIgnoreCase));
                query.JoinSource(query.FindSourceByDocDef(docDef), attrDef.DocDefType.Id, SqlSourceJoinType.Inner, attrDef.Id);

                foreach(var child in condition.Conditions)
                {
                    BuildCondition(query, child, attrDef.DocDefType.Id, parentCondition);
                }
            }
            else if (condition.Condition == ConditionOperation.Exp && condition.Conditions != null)
            {
                var exp = query.AddExpCondition(condition.Operation, parentCondition);

                foreach (var child in condition.Conditions)
                {
                    BuildCondition(query, child, docDef, exp);
                }
            }
        }
        private static void BuildCondition(SqlQuery query, QueryConditionDef condition, Guid docDefId, SqlQueryCondition parentCondition)
        {
            //using (var defRepo = new DocDefRepository(query.DataContext))
            var defRepo = query.Provider.Get<IDocDefRepository>();
            {
                var docDef = defRepo.DocDefById(docDefId);

                BuildCondition(query, condition, docDef, parentCondition);
            }
        }

        private static void BuildJoinCondition(SqlQuery query, SqlQueryJoin join, QueryConditionDef condition, DocDef docDef, SqlQueryCondition parentCondition, IDataContext dataContext)
        {
            if (condition.Condition != ConditionOperation.Include && condition.Condition != ConditionOperation.Exp)
            {
                var leftPart = BuildConditionPart(query, condition.Left, dataContext);
                var rightPart = BuildConditionPart(query, condition.Right, dataContext);
                join.Conditions.Add(new SqlQueryCondition(condition.Operation, leftPart, condition.Condition, rightPart));
            }
            else if (condition.Condition == ConditionOperation.Include && condition.Conditions != null && condition.Conditions.Count > 0)
            {
                var attrDef = String.IsNullOrEmpty(condition.AttributeName)
                                  ? docDef.Attributes.First(a => a.Id == condition.AttributeId)
                                  : docDef.Attributes.First(
                                      a => String.Equals(a.Name, condition.AttributeName, StringComparison.OrdinalIgnoreCase));
                query.JoinSource(query.FindSourceByDocDef(docDef), attrDef.DocDefType.Id, SqlSourceJoinType.Inner, attrDef.Id);

                foreach (var child in condition.Conditions)
                {
                    BuildJoinCondition(query, join, child, attrDef.DocDefType.Id, parentCondition, dataContext);
                }
            }
            else if (condition.Condition == ConditionOperation.Exp && condition.Conditions != null)
            {
                var exp = new SqlQueryCondition(null, condition.Operation, Guid.Empty, ConditionOperation.Exp, null);

                foreach (var child in condition.Conditions)
                {
                    BuildJoinCondition(query, join, child, docDef, exp, dataContext);
                }
            }
        }
        private static void BuildJoinCondition(SqlQuery query, SqlQueryJoin join, QueryConditionDef condition, Guid docDefId, SqlQueryCondition parentCondition, IDataContext dataContext)
        {
            // using (var defRepo = new DocDefRepository(dataContext))
            var defRepo = query.Provider.Get<IDocDefRepository>();
            {
                var docDef = defRepo.DocDefById(docDefId);

                BuildJoinCondition(query, join, condition, docDef, parentCondition, dataContext);
            }
        }

        private static SqlQueryConditionPart BuildConditionPart(SqlQuery query, QueryConditionPartDef part, IDataContext dataContext)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (part == null) throw new ArgumentNullException("part");

            var single = part.Attribute as QuerySingleAttributeDef;
            if (single != null)
            {
                var source = query.FindSource(single.Attribute.Source);
                
                var attr = single.Attribute.AttributeId != Guid.Empty
                    ? source.GetAttribute(single.Attribute.AttributeId)
                    : source.GetAttribute(single.Attribute.AttributeName);
                var attrRef = new SqlQuerySourceAttributeRef(source, attr);
                var sqPart =  new SqlQueryConditionPart();
                sqPart.Attributes.Add(attrRef);

                return sqPart;
            }
            var exp = part.Attribute as QueryExpAttributeDef;
            if (exp != null)
            {
                var sqPart = new SqlQueryConditionPart{Expression = exp.Expression};

                foreach (var attribute in exp.Attributes)
                {
                    var source = query.FindSource(attribute.Source);

                    var attr = attribute.AttributeId != Guid.Empty
                        ? source.GetAttribute(attribute.AttributeId)
                        : source.GetAttribute(attribute.AttributeName);
                    var attrRef = new SqlQuerySourceAttributeRef(source, attr);
                    sqPart.Attributes.Add(attrRef);
                }
                return sqPart;
            }
            var sub = part.Attribute as QuerySubAttributeDef;
            if (sub != null && sub.SubQuery != null)
            {
                var sqlSub = Build(dataContext, sub.SubQuery.QueryDef);

                var sqPart = new SqlQueryConditionPart
                {
                    SubQuery = sqlSub,
                    SubQueryAttribute = BuildAttribute(sqlSub, sub.SubQueryAttribute)
                };

                return sqPart;
            }
            if (part.SubQuery != null)
            {
                var sqlSub = Build(dataContext, part.SubQuery.QueryDef);

                var sqPart = new SqlQueryConditionPart
                {
                    SubQuery = sqlSub,
                    SubQueryAttribute = BuildAttribute(sqlSub, part.Attribute)
                };

                return sqPart;
            }

            return new SqlQueryConditionPart {Params = part.Params};
        }

        public static SqlQueryAttribute BuildAttribute(SqlQuery query, QueryAttributeDef attrDef)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (attrDef == null) throw new ArgumentNullException("attrDef");

            var single = attrDef as QuerySingleAttributeDef;
            if (single != null)
            {
                var source = query.FindSource(single.Attribute.Source);
                
                return single.Attribute.AttributeId != Guid.Empty
                    ? query.GetAttribute(source, single.Attribute.AttributeId)
                    : query.GetAttribute(source, single.Attribute.AttributeName);
            }
            var exp = attrDef as QueryExpAttributeDef;
            if (exp != null)
            {
                var attrRefs = new List<SqlQuerySourceAttributeRef>();

                foreach (var attribute in exp.Attributes)
                {
                    var source = query.FindSource(attribute.Source);

                    var attr = attribute.AttributeId != Guid.Empty
                        ? source.GetAttribute(attribute.AttributeId)
                        : source.GetAttribute(attribute.AttributeName);
                    var attrRef = new SqlQuerySourceAttributeRef(source, attr);
                    attrRefs.Add(attrRef);
                }
                return query.AddAttribute(attrRefs, exp.Expression);
            }
            return null;
        }
    }
}