using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryBuilderTool : ISqlQueryBuilder
    {
        private IAppServiceProvider Provider { get; set; }
        private readonly bool _globalProvider = false;
        private IDataContext DataContext { get; set; }
        private Guid UserId { get; set; }

        private readonly IDocDefRepository _docDefRepo;
        private readonly IQueryRepository _queryRepo;

        public SqlQueryBuilderTool(IAppServiceProvider provider, IDataContext dataContext, bool globalProvider = false)
        {
            Provider = provider;
            _globalProvider = globalProvider;
            DataContext = dataContext;

            _docDefRepo = provider.Get<IDocDefRepository>();
            _queryRepo = provider.Get<IQueryRepository>();
            // var currentUser = provider.Get<IUserDataProvider>();
            UserId = Provider.GetCurrentUserId();
        }

        public SqlQueryBuilderTool(IAppServiceProvider provider, IDataContext dataContext, Guid userId,
            bool globalProvider = false) : this(provider, dataContext, globalProvider)
        {
            UserId = userId;
        }

        public SqlQuery Build(QueryDef def)
        {
            if (def.Source == null)
                throw new PropertyConstraintException("QueryDef Source not defined!");

            var docDef = def.Source.DocDefId == Guid.Empty
                ? _docDefRepo.DocDefByName(def.Source.DocDefName)
                : _docDefRepo.DocDefById(def.Source.DocDefId);

            // TODO: Доделать код для глобального провайдера
            var query = /*_globalProvider ? new SqlQuery(Provider,) :*/ new SqlQuery(Provider, docDef, def.Alias, 
                UserId != Guid.Empty ? UserId : def.UserId)
            {
                DocumentId = def.DocumentId,
                ListAttrDefId = def.ListAttrId
            };
            try
            {
                foreach (var source in def.Sources)
                {
                    BuildSource(query, source);
                }
                foreach (var join in def.Joins)
                {
                    BuildJoin(query, @join, docDef);
                }
                foreach (var condition in def.WhereConditions)
                {
                    BuildCondition(query, condition, docDef, null, false);
                }
                foreach (var order in def.OrderAttributes)
                {
                    query.AddOrderAttribute(order);
                }
                foreach (var group in def.GroupAttributes)
                {
                    query.AddGroupAttribute(group);
                }
                foreach (var condition in def.HavingConditions)
                {
                    BuildCondition(query, condition, docDef, null, true);
                }
                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        private void BuildSource(SqlQuery query, QuerySourceDef source)
        {
            if (source.SubQuery != null)
            {
                var subQuery = Build(source.SubQuery);
                query.Sources.Add(new SqlQuerySubSource(subQuery, source.Alias));
            }
            else if (source.DocDefId != Guid.Empty)
            {
                query.Sources.Add(new SqlQueryDocSource(query.Provider, source.DocDefId, source.Alias));
            }
            else if (!String.IsNullOrEmpty(source.DocDefName))
            {
                var sourceDocDef = _docDefRepo.DocDefByName(source.DocDefName);
                query.Sources.Add(new SqlQueryDocSource(query.Provider, sourceDocDef, source.Alias));
            }
        }
        private void BuildJoin(SqlQuery query, QueryJoinDef join, DocDef docDef)
        {
            var source =
                query.Sources.First(
                    s => String.Equals(s.AliasName, @join.Source.Alias, StringComparison.OrdinalIgnoreCase));
            var queryJoin = new SqlQueryJoin(source, @join.Operation);
            foreach (var condition in @join.Conditions)
            {
                BuildJoinCondition(query, queryJoin, condition, docDef, null);
            }
            query.SourceJoins.Add(queryJoin);
        }

        private void BuildJoinCondition(SqlQuery query, SqlQueryJoin join, QueryConditionDef condition, DocDef docDef, SqlQueryCondition parentCondition)
        {
            if (condition.Condition != ConditionOperation.Include && condition.Condition != ConditionOperation.Exp)
            {
                var leftPart = BuildConditionPart(query, condition.Left);
                var rightPart = BuildConditionPart(query, condition.Right);
                @join.Conditions.Add(new SqlQueryCondition(condition.Operation, leftPart, condition.Condition, rightPart));
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
                    BuildJoinCondition(query, @join, child, attrDef.DocDefType.Id, parentCondition);
                }
            }
            else if (condition.Condition == ConditionOperation.Exp && condition.Conditions != null)
            {
                var exp = new SqlQueryCondition(null, condition.Operation, Guid.Empty, ConditionOperation.Exp, null);

                foreach (var child in condition.Conditions)
                {
                    BuildJoinCondition(query, @join, child, docDef, exp);
                }
            }
        }
        private void BuildJoinCondition(SqlQuery query, SqlQueryJoin join, QueryConditionDef condition, Guid docDefId, SqlQueryCondition parentCondition)
        {
            var docDef = _docDefRepo.DocDefById(docDefId);

            BuildJoinCondition(query, @join, condition, docDef, parentCondition);
        }

        private SqlQueryConditionPart BuildConditionPart(SqlQuery query, QueryConditionPartDef part, DocDef docDef = null)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (part == null) throw new ArgumentNullException("part");

            var single = part.Attribute as QuerySingleAttributeDef;
            if (single != null)
            {

                // 2016-02-18: TODO: Добавить вариант для подзапроса
                var source = query.FindSource(single.Attribute.Source) ??
                             (docDef != null ? query.FindSourceByDocDef(docDef) : null) ??
                             (single.Attribute.AttributeId != Guid.Empty
                                 ? query.FindSourceByAttributeId(single.Attribute.AttributeId)
                                 : query.FindSourceByAttributeName(single.Attribute.AttributeName));

                if (source == null)
                    throw new ApplicationException("Не могу создать условие запроса!");

                var attr = single.Attribute.AttributeId != Guid.Empty
                    ? source.GetAttribute(single.Attribute.AttributeId)
                    : source.GetAttribute(single.Attribute.AttributeName);
                /*else
                    attr = single.Attribute.AttributeId != Guid.Empty
                        ? query.GetAttribute(single.Attribute.AttributeId)
                        : query.GetAttribute(single.Attribute.AttributeName);*/

                var attrRef = new SqlQuerySourceAttributeRef(source, attr);
                var sqPart = new SqlQueryConditionPart();
                sqPart.Attributes.Add(attrRef);

                return sqPart;

            }
            var exp = part.Attribute as QueryExpAttributeDef;
            if (exp != null)
            {
                var sqPart = new SqlQueryConditionPart { Expression = exp.Expression };

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
                var sqlSub = Build(sub.SubQuery.QueryDef);

                var sqPart = new SqlQueryConditionPart
                {
                    SubQuery = sqlSub,
                    SubQueryAttribute = BuildAttribute(sqlSub, part.Attribute) /*sub.SubQueryAttribute)*/
                };

                return sqPart;
            }
            if (part.SubQuery != null)
            {
                var sqlSub = Build(part.SubQuery.QueryDef);

                var sqPart = new SqlQueryConditionPart
                {
                    SubQuery = sqlSub,
                    SubQueryAttribute = BuildAttribute(sqlSub, part.SubQuery.Attribute)/*part.Attribute)*/
                };

                return sqPart;
            }

            return new SqlQueryConditionPart {Params = part.Params};
        }
        public SqlQueryAttribute BuildAttribute(SqlQuery query, QueryAttributeDef attrDef)
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
        private void BuildCondition(SqlQuery query, QueryConditionDef condition, DocDef docDef, SqlQueryCondition parentCondition, bool isHaving)
        {
            if (condition.Condition != ConditionOperation.Include && condition.Condition != ConditionOperation.Exp)
            {
                var leftPart = BuildConditionPart(query, condition.Left, docDef);
                var rightPart = BuildConditionPart(query, condition.Right, docDef);                
                
                if (parentCondition != null)
                    parentCondition.Conditions.Add(new SqlQueryCondition(condition.Operation, leftPart,
                        condition.Condition, rightPart));
                else if (isHaving)
                    query.HavingConditions.Add(new SqlQueryCondition(condition.Operation, leftPart, condition.Condition,
                        rightPart));
                else
                    query.Conditions.Add(new SqlQueryCondition(condition.Operation, leftPart, condition.Condition,
                        rightPart));
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
                    BuildCondition(query, child, attrDef.DocDefType.Id, parentCondition, isHaving);
                }
            }
            else if (condition.Condition == ConditionOperation.Exp && condition.Conditions != null)
            {
                var exp = query.AddExpCondition(condition.Operation, parentCondition);

                foreach (var child in condition.Conditions)
                {
                    BuildCondition(query, child, docDef, exp, isHaving);
                }
            }
        }
        private void BuildCondition(SqlQuery query, QueryConditionDef condition, Guid docDefId, SqlQueryCondition parentCondition, bool isHaving)
        {
            var docDef = _docDefRepo.DocDefById(docDefId);

            BuildCondition(query, condition, docDef, parentCondition, isHaving);
        }

        public SqlQuery Build(Guid docDefId)
        {
            return new SqlQuery(Provider, docDefId, UserId);
        }
        public SqlQuery BuildAttrList(Doc doc, string attrDefName, string alias)
        {
            return new SqlQuery(doc, attrDefName, UserId, alias, Provider);
        }

        public SqlQuery Build(Guid docDefId, Guid docStateId, Doc filter, IEnumerable<AttributeSort> sortAttrs)
        {
            var query = new SqlQuery(docDefId, Provider);
            try
            {
                query.AddCondition(ExpressionOperation.And, docDefId, "&State", ConditionOperation.Equal, docStateId);

                if (filter != null)
                    AddDocConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                    {
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                    }

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        public SqlQuery Build(Guid docDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs)
        {
            var query = new SqlQuery(Provider, docDefId, UserId);
            try
            {
//                query.AddAttribute("&Id");
                AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                    {
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                    }

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }
        /*public SqlQuery Build(DocDef docDef)
        {
            var query = new SqlQuery(DataContext, docDef);
            try
            {
                AddDocDefAttributes(query, query.Source, docDef);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }*/
        public SqlQuery Build(QueryDef def, BizForm form, IEnumerable<AttributeSort> sortAttrs)
        {
            var query = Build(def);
            try
            {
                query.AddAttribute("&Id");

                AddControlAttributes(query, query.Source, form);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                else
                    AddFormSortOrders(query, query.Source, form);

                AddControlSourceDatas(query, query.Source, form);
                AddControlJoinAndConditionDatas(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        public SqlQuery Build(QueryDef def, BizForm form, BizForm filter, IEnumerable<AttributeSort> sortAttrs)
        {
            var query = Build(def);
            try
            {
                query.AddAttribute("&Id");

                AddControlAttributes(query, query.Source, form);

                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                else
                    AddFormSortOrders(query, query.Source, form);

                AddControlSourceDatas(query, query.Source, form);
                AddControlJoinAndConditionDatas(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        public SqlQuery Build(BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs)
        {
            var query = new SqlQuery(Provider, form.DocumentDefId ?? Guid.Empty, UserId);
            try
            {
                query.AddAttribute("&Id");
                if (docStateId != null)
                    query.AddCondition(ExpressionOperation.And, query.Source.GetDocDef(), "&State",
                        ConditionOperation.Equal, (Guid) docStateId);

                AddControlAttributes(query, query.Source, form);
                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                else
                    AddFormSortOrders(query, query.Source, form);

                AddControlSourceDatas(query, query.Source, form);
                AddControlJoinAndConditionDatas(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        public SqlQuery BuildAttrList(BizForm form, Guid docId, Guid attrDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs)
        {
            var query = new SqlQuery(form.DocumentDefId ?? Guid.Empty, docId, attrDefId, Provider);
            try
            {
                query.AddAttribute("&Id");

                AddControlAttributes(query, query.Source, form);
                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                    {
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                    }
                else
                    AddFormSortOrders(query, query.Source, form);

                AddControlSourceDatas(query, query.Source, form);
                AddControlJoinAndConditionDatas(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        public SqlQuery BuildAttrList(BizControl form, Guid docId, Guid docDefId, Guid attrDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs)
        {
            var query = new SqlQuery(docDefId, docId, attrDefId, Provider);
            try
            {
                query.AddAttribute("&Id");

                AddControlAttributes(query, query.Source, form);
                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                else
                    AddFormSortOrders(query, query.Source, form);

                AddControlSourceDatas(query, query.Source, form);
                AddControlJoinAndConditionDatas(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        public SqlQuery BuildRefList(BizForm form, Guid docId, Guid attrDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs)
        {
            var query = new SqlQuery(Provider, form.DocumentDefId ?? Guid.Empty, UserId);
            try
            {
                query.AddAttribute("&Id");
                query.AndCondition(attrDefId, ConditionOperation.Equal, docId);

                AddControlAttributes(query, query.Source, form);
                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                    {
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                    }
                else
                    AddFormSortOrders(query, query.Source, form);

                AddControlSourceDatas(query, query.Source, form);
                AddControlJoinAndConditionDatas(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        public SqlQuery BuildRefList(BizControl form, Guid docId, Guid docDefId, Guid attrDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs)
        {
            var query = new SqlQuery(Provider, docDefId, UserId);
            try
            {
                query.AddAttribute("&Id");
                query.AndCondition(attrDefId, ConditionOperation.Equal, docId);

                AddControlAttributes(query, query.Source, form);
                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                else
                    AddFormSortOrders(query, query.Source, form);

                AddControlSourceDatas(query, query.Source, form);
                AddControlJoinAndConditionDatas(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        public SqlQuery Build(BizForm form, IEnumerable<Guid> docIds, IEnumerable<AttributeSort> sortAttrs)
        {
            if (form.DocumentDefId == null)
                throw new ApplicationException("Не могу создать запрос! Форма не связана с документом!");

            var query = new SqlQuery(Provider, (Guid)form.DocumentDefId, UserId);
            try
            {
                query.AddAttribute("&Id");
                AddControlAttributes(query, query.Source, form);
                query.AddCondition(ExpressionOperation.And, query.Source.GetDocDef(), "&Id", ConditionOperation.In,
                    docIds.Cast<object>());
                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                else
                    AddFormSortOrders(query, query.Source, form);

                AddControlSourceDatas(query, query.Source, form);
                AddControlJoinAndConditionDatas(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        public SqlQuery Build(BizForm form)
        {
            if (form.DocumentDefId == null)
                throw new ApplicationException("Не могу создать запрос! Форма не связана с документом!");

            var query = new SqlQuery(Provider, (Guid)form.DocumentDefId, UserId);
            try
            {
                query.AddAttribute("&Id");

                AddControlAttributes(query, query.Source, form);
                AddFormSortOrders(query, query.Source, form);

                AddControlSourceDatas(query, query.Source, form);
                AddControlJoinAndConditionDatas(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        private void AddFormConditions(SqlQuery query, SqlQuerySource source, BizControl filter)
        {
            if (filter == null || filter.Children == null) return;

            foreach (var control in filter.Children)
            {
                if (control is BizDocumentControl)
                {
                    var attrDefId = ((BizDocumentControl)control).AttributeDefId ?? Guid.Empty;
                    var ts = query.FindSourceByAttributeId(attrDefId);
                    var attrDef = ts == null ? source.FindAttributeDef(attrDefId) : ts.FindAttributeDef(attrDefId);

                    if (attrDef == null) continue;

                    if (((BizDocumentControl)control).DocForm != null && HasControlValue(((BizDocumentControl)control).DocForm))
                    {
                        if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                            ts = query.TryJoinSource(source, attrDef.DocDefType.Id, SqlSourceJoinType.Inner, attrDefId);

                        AddFormConditions(query, ts, ((BizDocumentControl)control).DocForm);
                    }
                    else if (control.Children != null && control.Children.Count > 0 && HasControlValue(control))
                    {
                        if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                            ts = query.TryJoinSource(source, attrDef.DocDefType.Id, SqlSourceJoinType.Inner, attrDefId);

                        AddFormConditions(query, ts, control);
                    }
                }
                else if (control is BizDocumentListForm)
                {
                }
                else if (control is BizDataControl && ((BizDataControl)control).AttributeDefId != null)
                {
                    if (control is BizEditText)
                    {
                        var txt = ((BizEditText)control).Value;
                        if (!String.IsNullOrEmpty(txt))
                        {
                            switch (control.Operation)
                            {
                                case CompareOperation.Like:
                                    if (txt.Contains("?") || txt.Contains("*"))
                                    {
                                        txt = txt.Trim().Replace('?', '_');
                                        txt = txt.Trim().Replace('*', '%');
                                    }
                                    else if (!txt.Contains("%") && !txt.Contains("_"))
                                    {
                                        txt = "%" + txt.Trim() + "%";
                                    }
                                    else
                                        txt = txt.Trim();

                                    query.AddCondition(ExpressionOperation.And, source,
                                        (Guid)((BizDataControl)control).AttributeDefId,
                                        ConditionOperation.Like, txt);
                                    break;
                                case CompareOperation.StartWith:
                                    txt = txt.Trim() + '%';
                                    query.AddCondition(ExpressionOperation.And, source,
                                        (Guid)((BizDataControl)control).AttributeDefId,
                                        ConditionOperation.Like, txt);
                                    break;
                                case CompareOperation.EndWith:
                                    txt = '%' + txt.Trim();
                                    query.AddCondition(ExpressionOperation.And, source,
                                        (Guid)((BizDataControl)control).AttributeDefId,
                                        ConditionOperation.Like, txt);
                                    break;
                                case CompareOperation.Levenstein:
                                    //if (txt.Length < 3) 
                                    query.AddCondition(ExpressionOperation.And, source,
                                        (Guid)((BizDataControl)control).AttributeDefId,
                                        ConditionOperation.Levenstein, txt.Trim());
                                    break;
                                case CompareOperation.NotApplied:
                                    break;
                                default:
                                    query.AddCondition(ExpressionOperation.And, source,
                                        (Guid)((BizDataControl)control).AttributeDefId,
                                        ConditionOperation.Equal, txt.ToUpper());
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (((BizDataControl)control).ObjectValue != null && control.Operation != CompareOperation.NotApplied)
                            query.AddCondition(ExpressionOperation.And, source, (Guid)((BizDataControl)control).AttributeDefId,
                                CompareOperationConverter.CompareToCondition(control.Operation), ((BizDataControl)control).ObjectValue);
                    }
                }
                else if (control is BizEditText || control is BizEditDateTime)
                {
                    if (((BizDataControl)control).ObjectValue != null && control.Operation != CompareOperation.NotApplied)
                        query.AndCondition(source, SystemIdentConverter.Convert(((BizEdit)control).Ident),
                            CompareOperationConverter.CompareToCondition(control.Operation), ((BizDataControl)control).ObjectValue);
                }
                else if (control is BizComboBox)
                {
                    if (((BizDataControl)control).ObjectValue != null && control.Operation != CompareOperation.NotApplied)
                        query.AndCondition(source, SystemIdentConverter.Convert(((BizComboBox)control).Ident),
                            CompareOperationConverter.CompareToCondition(control.Operation), ((BizDataControl)control).ObjectValue);
                }
                else if (control.Children != null && control.Children.Count > 0)
                    AddFormConditions(query, source, control);
            }
        }

        public void AddFormSortOrders(SqlQuery query, SqlQuerySource source, BizControl form)
        {
            if (form.Children != null)
                foreach (var control in form.Children)
                {
                    if (control is BizDocumentControl)
                    {
                        var attrDefId = ((BizDocumentControl) control).AttributeDefId ?? Guid.Empty;
                        var ts = query.FindSourceByAttributeId(attrDefId);
                        var attrDef = ts == null ? source.FindAttributeDef(attrDefId) : ts.FindAttributeDef(attrDefId);

                        if (attrDef == null) continue;

                        if (control.Children != null && control.Children.Count > 0)
                        {
                            if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                            {
                                ts =
                                    query.FindSourceByAttrDef(attrDefId) ??
                                    query.TryJoinSource(source, attrDef.DocDefType.Id, SqlSourceJoinType.Inner,
                                        attrDefId);
                            }

                            AddFormSortOrders(query, ts, control);
                        }
                        else if (((BizDocumentControl) control).DocForm != null)
                        {
                            if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                                ts = query.TryJoinSource(source,
                                    attrDef.DocDefType.Id,
                                    SqlSourceJoinType.Inner,
                                    ((BizDocumentControl) control).AttributeDefId ?? Guid.Empty);

                            AddFormSortOrders(query, ts, ((BizDocumentControl) control).DocForm);
                        }
                    }
                    else if (control is BizDataControl && ((BizDataControl) control).AttributeDefId != null &&
                             control.SortType != SortType.None)
                    {
                        query.AddOrderAttribute(source, (Guid) ((BizDataControl) control).AttributeDefId,
                            control.SortType == SortType.Ascending);
                    }
                }
        }

        public static ConditionOperation CompareToCondition(CompareOperation compare)
        {
            switch (compare)
            {
                case CompareOperation.Equal:
                    return ConditionOperation.Equal;
                case CompareOperation.NotEqual:
                    return ConditionOperation.NotEqual;
                case CompareOperation.Great:
                    return ConditionOperation.GreatThen;
                case CompareOperation.GreatEqual:
                    return ConditionOperation.GreatEqual;
                case CompareOperation.Less:
                    return ConditionOperation.LessThen;
                case CompareOperation.LessEqual:
                    return ConditionOperation.LessEqual;
                case CompareOperation.Like:
                    return ConditionOperation.Like;
                case CompareOperation.NotLike:
                    return ConditionOperation.NotLike;
                case CompareOperation.Levenstein:
                    return ConditionOperation.Levenstein;
                default:
                    return ConditionOperation.Equal;
            }
        }

        private bool HasControlValue(BizControl control)
        {
            var dataControl = control as BizDataControl;
            if (dataControl != null)
                if (dataControl.ObjectValue != null) return true;

            if (control.Children != null)
                return control.Children.Any(HasControlValue);
            return false;
        }

        private void AddControlSourceDatas(SqlQuery query, SqlQuerySource source, BizControl control)
        {
            if (control.QueryItems != null)
                foreach (var item in control.QueryItems)
                {
                    if (item is QuerySourceDefData)
                        AddQuerySourceData(query, (QuerySourceDefData) item);
                }

            if (control.Children != null)
                foreach (var child in control.Children)
                    AddControlSourceDatas(query, source, child);
        }

        private void AddControlJoinAndConditionDatas(SqlQuery query, SqlQuerySource source, BizControl control)
        {
            if (control.QueryItems != null)
                foreach (var item in control.QueryItems)
                {
                    if (item is QuerySourceDefData)
                        AddQueryJoinData(query, (QuerySourceDefData) item);
                    else if (item is QueryConditionDefData)
                        AddQueryConditionData(query, (QueryConditionDefData) item, null, null);
                }

            if (control.Children != null)
                foreach (var child in control.Children)
                    AddControlJoinAndConditionDatas(query, source, child);
        }

        private void AddControlAttributes(SqlQuery query, SqlQuerySource source, BizControl control)
        {
            if (control.Children != null)
                foreach (var child in control.Children)
                {
                    if (child.Invisible) continue;

                    if (child is BizDataControl && ((BizDataControl) child).AttributeDefId != null)
                        query.AddAttribute(source, (Guid) ((BizDataControl) child).AttributeDefId);
                    else if (child is BizEditDateTime)
                    {
                        if (((BizEdit) child).Ident == SystemIdent.Created)
                            query.AddAttribute(source, "&Created");
                        else if (((BizEdit) child).Ident == SystemIdent.Modified)
                            query.AddAttribute(source, "&Modified");
                        else if (((BizEdit)child).Ident == SystemIdent.StateDate)
                            query.AddAttribute(source, "&StateDate");
                    }
                    else if (child is BizEditText)
                    {
                        query.AddAttribute(source, SystemIdentConverter.Convert(((BizEdit) child).Ident));
                    }
                    else if (child is BizComboBox)
                        query.AddAttribute(source, SystemIdentConverter.Convert(((BizComboBox) child).Ident));

                    if (child is BizDocumentControl)
                    {
                        /*var ts; =
                            query.FindSourceByAttributeId(((BizDocumentControl) child).AttributeDefId ?? Guid.Empty);*/

                        AttrDef attrDef;
                        //if (ts == null)
                            attrDef =
                                source.FindAttributeDef(((BizDocumentControl) child).AttributeDefId ?? Guid.Empty);
                        /*else
                            attrDef = ts.FindAttributeDef(((BizDocumentControl) child).AttributeDefId ?? Guid.Empty);
                        */
                        if (attrDef == null) continue;

                        //if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                        var ts = query.JoinSource(source, attrDef.DocDefType.Id, SqlSourceJoinType.LeftOuter,
                            ((BizDocumentControl) child).AttributeDefId ?? Guid.Empty);

                        if (child.Children != null && child.Children.Count > 0)
                            AddControlAttributes(query, ts, child);
                        else if (((BizDocumentControl) child).DocForm != null)
                            AddControlAttributes(query, ts, ((BizDocumentControl) child).DocForm);
                    }
                    else if (child is BizTableColumn)
                    {
                        if (((BizTableColumn) child).AttributeDefId != null)
                        {
                            var ts = query.FindSourceByAttributeId((Guid) ((BizTableColumn) child).AttributeDefId);
                            AttrDef attrDef;
                            //if (ts == null)
                                attrDef =
                                    source.FindAttributeDef((Guid) ((BizTableColumn) child).AttributeDefId);
                            /*else
                                attrDef = ts.FindAttributeDef((Guid) ((BizTableColumn) child).AttributeDefId);
                            */
                            if (attrDef == null) continue;
                            //if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                                ts = query.TryJoinSource(source,
                                    attrDef.DocDefType.Id,
                                    SqlSourceJoinType.LeftOuter,
                                    (Guid) ((BizTableColumn) child).AttributeDefId);

                            AddControlAttributes(query, ts, child);
                        }
                    }
                    else if (child is BizDocumentListForm)
                    {
                        var ts =
                            query.FindSourceByAttributeId(((BizDocumentListForm) child).AttributeDefId ?? Guid.Empty);

                        AttrDef attrDef;
                        attrDef = ts == null
                            ? source.FindAttributeDef(((BizDocumentListForm) child).AttributeDefId ?? Guid.Empty)
                            : ts.FindAttributeDef(((BizDocumentListForm) child).AttributeDefId ?? Guid.Empty);

                        if (attrDef == null) continue;

                        if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                            ts = query.TryJoinSource(source,
                                attrDef.DocDefType.Id,
                                SqlSourceJoinType.Inner,
                                ((BizDocumentListForm) child).AttributeDefId ?? Guid.Empty);

                        if (child.Children != null && child.Children.Count > 0)
                            AddControlAttributes(query, ts, child);
                        else if (((BizDocumentListForm) child).TableForm != null)
                            AddControlAttributes(query, ts, ((BizDocumentListForm) child).TableForm);
                    }
                    else if (child.Children != null && child.Children.Count > 0)
                        AddControlAttributes(query, source, child);
                }
        }

        public void AddDocConditions(SqlQuery query, SqlQuerySource source, Doc filter)
        {
            if (filter == null) return;

            foreach (var attr in filter.Attributes)
            {
                if (attr is DocAttribute)
                {
                    if (((DocAttribute)attr).Document != null && HasDocumentValue(((DocAttribute)attr).Document))
                    {
                        AddJoinDocConditions(query, source, (DocAttribute)attr);
                    }
                }
                else if (attr is DocListAttribute)
                {
                    //                    if (((DocListAttribute)attr).AddedDocIds != null)
                }
                else if (attr.ObjectValue != null)
                {
                    if (attr is TextAttribute)
                    {
                        var txt = ((TextAttribute)attr).Value;
                        if (!String.IsNullOrEmpty(txt))
                            query.AddCondition(ExpressionOperation.And, attr.AttrDef.Id, ConditionOperation.Like, txt);
                    }
                    else
                    {
                        query.AddCondition(ExpressionOperation.And, attr.AttrDef.Id, ConditionOperation.Equal, attr.ObjectValue);
                    }
                }
            }
        }

        private static bool HasDocumentValue(Doc document)
        {
            foreach (var attr in document.Attributes)
            {
                if (attr is DocAttribute)
                {
                    if (((DocAttribute)attr).Document != null && HasDocumentValue(((DocAttribute)attr).Document))
                        return true;
                }
                else if (attr is DocListAttribute)
                {

                }
                else if (attr is TextAttribute)
                {
                    var text = ((TextAttribute)attr).Value;
                    if (!String.IsNullOrEmpty(text)) return true;
                }
                else if (attr.ObjectValue != null) return true;
            }
            return false;
        }

        private void AddJoinDocConditions(SqlQuery query, SqlQuerySource master, DocAttribute attr)
        {
            var source = query.JoinSource(master, attr.Document.DocDef.Id, SqlSourceJoinType.Inner, attr.AttrDef.Id);

            AddDocConditions(query, source, attr.Document);
        }

        private void AddQuerySourceData(SqlQuery query, QuerySourceDefData sourceData)
        {
            SqlQuerySource source;
            var queryData = GetSourceDataQuery(sourceData);
            if (queryData != null)
            {
                var subQuery = Build(queryData);
                source = new SqlQuerySubSource(subQuery, sourceData.Alias);
                query.Sources.Add(source);
            }
            else
            {
                var docDef = GetSourceDataDocDef(sourceData);
                if (docDef != null)
                {
                    source = new SqlQueryDocSource(query.Provider, docDef.Id, sourceData.Alias);
                    query.Sources.Add(source);
                }
                else
                    throw new ApplicationException("Не могу сформировать запрос из QuerySourceDefData");
            }

            if (sourceData.Items != null)
                foreach (var item in sourceData.Items)
                {
                    var subSource = item as QuerySourceDefData;
                    if (subSource != null)
                        AddQuerySourceData(query, subSource);
                }
        }

        private DocDef GetSourceDataDocDef(QuerySourceDefData sourceData)
        {
            if (sourceData.DocDef != null)
                return sourceData.DocDef;
            if (sourceData.DocumentId != null)
                return _docDefRepo.DocDefById((Guid)sourceData.DocumentId);

            return null;
        }

        private QueryDefData GetSourceDataQuery(QuerySourceDefData sourceData)
        {
            if (sourceData.Query != null)
                return sourceData.Query;
            if (sourceData.QueryId != null)
                return _queryRepo.GetQuery((Guid)sourceData.QueryId);

            return null;
        }

        private void AddQueryJoinData(SqlQuery query, QuerySourceDefData sourceData)
        {
            SqlQuerySource source =
                query.Sources.First(
                    s => String.Equals(s.AliasName, sourceData.Alias, StringComparison.OrdinalIgnoreCase));

            var queryJoin = new SqlQueryJoin(source, sourceData.JoinType);
            query.SourceJoins.Add(queryJoin);

            if (sourceData.Items != null)
                foreach (var item in sourceData.Items)
                {
                    var condition = item as QueryConditionDefData;
                    if (condition != null)
                    {
                        AddQueryConditionData(query, condition, queryJoin, null);
                    }
                    else
                    {
                        var subSource = item as QuerySourceDefData;
                        if (subSource != null)
                            AddQueryJoinData(query, subSource);
                    }
                }
        }

        private void AddQueryConditionData(SqlQuery query, QueryConditionDefData conditionData, SqlQueryJoin join, SqlQueryCondition parentCondition)
        {
            var conditionOperation = conditionData.Operation;
            if (conditionOperation != ConditionOperation.Include && conditionOperation != ConditionOperation.Exp)
            {
                var leftPart = BuildConditionPart(query, conditionData.LeftSourceName, conditionData.LeftAttributeId,
                    conditionData.LeftAttributeName, conditionData.LeftValue, conditionData.LeftParamName);
                var rightPart = BuildConditionPart(query, conditionData.RightSourceName, conditionData.RightAttributeId,
                    conditionData.RightAttributeName, conditionData.RightValue, conditionData.RightParamName);

                var condition = new SqlQueryCondition(conditionData.Expression, leftPart, conditionOperation, rightPart);

                if (parentCondition != null)
                    parentCondition.Conditions.Add(condition);
                else if (join != null)
                    join.Conditions.Add(condition);
                else
                    query.Conditions.Add(condition);
            }
            else if (conditionOperation == ConditionOperation.Exp && conditionData.Items != null && conditionData.Items.OfType<QueryConditionDefData>().Any())
            {
                var exp = query.AddExpCondition(conditionData.Expression, parentCondition);

                foreach (var child in conditionData.Items.OfType<QueryConditionDefData>())
                {
                    AddQueryConditionData(query, child, join, exp);
                }
            }
        }

        public SqlQueryConditionPart BuildConditionPart(SqlQuery query, string sourceName, Guid? attrId, string attrName,
            object value, string paramName)
        {
            QueryDefData subQueryData = null;
            var sourceId = Guid.Empty;

            if (!String.IsNullOrEmpty(sourceName))
            {
                if (Guid.TryParse(sourceName.Trim(), out sourceId))
                {
                    subQueryData = _queryRepo.FindQuery(sourceId);
                }
            }
            if (attrId != null || !String.IsNullOrEmpty(attrName))
            {
                if (subQueryData != null)
                {
                    var subQuery = Build(subQueryData);
                    var subAttr = attrId != null && attrId != Guid.Empty
                        ? subQuery.GetAttribute((Guid) attrId)
                        : subQuery.GetAttribute(attrName);
                    
                    return new SqlQueryConditionPart
                    {
                        SubQuery = subQuery,
                        SubQueryAttribute = subAttr
                    };
                }
                var source = (sourceId != Guid.Empty) ? query.FindSourceById(sourceId) : query.FindSourceByAlias(sourceName) ?? query.Source;

                var attr = attrId != null && attrId != Guid.Empty
                    ? source.GetAttribute((Guid) attrId)
                    : source.GetAttribute(attrName);
                var attrRef = new SqlQuerySourceAttributeRef(source, attr);
                var conditionPart = new SqlQueryConditionPart();
                conditionPart.Attributes.Add(attrRef);

                return conditionPart;
            }

            return new SqlQueryConditionPart
            {
                Params = new[] {new QueryConditionValueDef {Value = value, Name = paramName}}
            };
        }

        public SqlQuery Build(QueryDefData queryData)
        {
            var query = Build(queryData.DocumentId ?? Guid.Empty);
            if (!String.IsNullOrWhiteSpace(queryData.Alias)) query.Source.AliasName = queryData.Alias;

            if (queryData.Items != null)
            {
                foreach (var item in queryData.Items)
                {
                    var sourceData = item as QuerySourceDefData;
                    if (sourceData != null)
                        AddQuerySourceData(query, sourceData);
                }
                foreach (var item in queryData.Items)
                {
                    var cond = item as QueryConditionDefData;
                    if (cond != null)
                        AddQueryConditionData(query, cond, null, null);
                    else
                    {
                        var sourceData = item as QuerySourceDefData;
                        if (sourceData != null)
                            AddQueryJoinData(query, sourceData);
                    }
                }
            }
            return query;
        }

        public SqlQuery Build(BizControl control, Guid docDefId)
        {
            var query = Build(docDefId);

            if (control.QueryItems != null)
            {
                foreach (var item in control.QueryItems)
                {
                    var sourceData = item as QuerySourceDefData;
                    if (sourceData != null)
                        AddQuerySourceData(query, sourceData);
                }
            }
            if (control.Children != null)
            {
                foreach (var child in control.Children)
                {
                    AddControlSourceDatas(query, query.Source, child);
                }
            }
            if (control.QueryItems != null)
            {
                foreach (var item in control.QueryItems)
                {
                    var cond = item as QueryConditionDefData;
                    if (cond != null)
                        AddQueryConditionData(query, cond, null, null);
                    else
                    {
                        var sourceData = item as QuerySourceDefData;
                        if (sourceData != null)
                            AddQueryJoinData(query, sourceData);
                    }
                }
            }
            if (control.Children != null)
            {
                foreach (var child in control.Children)
                {
                    AddControlJoinAndConditionDatas(query, query.Source, child);
                }
            }

            return query;
        }
    }
}