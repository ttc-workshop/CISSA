using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryExBuilder
    {
        /*public static SqlQuery Build(Guid docDefId, Guid docStateId, Doc filter, IEnumerable<AttributeSort> sortAttrs, IDataContext dataContext = null)
        {
            var query = new SqlQuery(docDefId, dataContext);
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
        }*/

        /*public static SqlQuery Build(Guid docDefId, Doc filter, IEnumerable<AttributeSort> sortAttrs, Guid userId,
            IDataContext dataContext = null)
        {
            var query = new SqlQuery(dataContext, docDefId, userId);
            try
            {
                AddDocDefAttributes(query, query.Source, query.Source.GetDocDef());

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
        }*/

        public static SqlQuery Build(Guid docDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, Guid userId, IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var sqb = new SqlQueryBuilderTool(provider, dataContext, userId);
                return sqb.Build(docDefId, filter, sortAttrs);
            }
            /*var query = new SqlQuery(dataContext, docDefId, userId);
            try
            {
                query.AddAttribute("&Id");
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
            }*/
        }

        public static SqlQuery Build(QueryDef def, BizForm form, IEnumerable<AttributeSort> sortAttrs, Guid userId, IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var sqb = new SqlQueryBuilderTool(provider, dataContext, userId);
                return sqb.Build(def, form, sortAttrs);
            }
            /*var query = SqlQueryBuilder.Build(dataContext, def, userId);
            try
            {
                query.AddAttribute("&Id");
                AddFormAttributes(query, query.Source, form);
                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                else
                    AddFormSortOrders(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }*/
        }

        public static SqlQuery Build(QueryDef def, BizForm form, BizForm filter, IEnumerable<AttributeSort> sortAttrs, Guid userId, IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var sqb = new SqlQueryBuilderTool(provider, dataContext, userId);
                return sqb.Build(def, form, filter, sortAttrs);
            }
            /*var query = SqlQueryBuilder.Build(dataContext, def, userId);
            try
            {
                query.AddAttribute("&Id");
                AddFormAttributes(query, query.Source, form);
                if (filter != null)
                {
                    AddFormConditions(query, query.Source, filter);
                }
                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                else
                    AddFormSortOrders(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }*/
        }

       /* public static SqlQuery Build(Guid docDefId, Guid docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, IDataContext dataContext = null)
        {
            var query = new SqlQuery(docDefId, dataContext);
            try
            {
                query.AddAttribute("&Id");
                query.AddCondition(ExpressionOperation.And, query.Source.GetDocDef(), "&State", ConditionOperation.Equal,
                                   docStateId);
                if (filter != null)
                {
                    AddFormAttributes(query, query.Source, filter);
                    AddFormConditions(query, query.Source, filter);
                }
                else
                    AddDocDefAttributes(query, query.Source, query.Source.GetDocDef());

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
        }*/

        /*[Obsolete("Не завершенный метод!")]
        private static void AddDocDefAttributes(SqlQuery query, SqlQuerySource source, DocDef docDef)
        {
            foreach (var attrDef in docDef.Attributes)
            {
                if (attrDef.Type.Id == (short) CissaDataType.Doc || attrDef.Type.Id == (short)CissaDataType.DocList)
                {
//                    var slave = query.JoinSource(source, attrDef.DocDefType.Id, SqlSourceJoinType.Inner, attrDef.Id);

                    query.AddAttribute(source, attrDef.Id);
                }
                else
                {
                    query.AddAttribute(source, attrDef.Id);
                }
            }
        }*/

        public static SqlQuery Build(BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, Guid userId, IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var sqb = new SqlQueryBuilderTool(provider, dataContext, userId);
                return sqb.Build(form, docStateId, filter, sortAttrs);
            }
            /*var query = new SqlQuery(dataContext, form.DocumentDefId ?? Guid.Empty, userId);
            try
            {
                query.AddAttribute("&Id");
                if (docStateId != null)
                    query.AddCondition(ExpressionOperation.And, query.Source.GetDocDef(), "&State",
                                       ConditionOperation.Equal,
                                       (Guid) docStateId);
                AddFormAttributes(query, query.Source, form);
                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                else
                    AddFormSortOrders(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }*/
        }

        public static SqlQuery BuildAttrList(BizForm form, Guid docId, Guid attrDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, Guid userId, IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var sqb = new SqlQueryBuilderTool(provider, dataContext, userId);
                return sqb.BuildAttrList(form, docId, attrDefId, filter, sortAttrs);
            }
            /*var query = new SqlQuery(form.DocumentDefId ?? Guid.Empty, docId, attrDefId, dataContext);
            try
            {
                query.AddAttribute("&Id");
                AddFormAttributes(query, query.Source, form);
                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                    {
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                    }
                else
                    AddFormSortOrders(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }*/
        }

        public static SqlQuery BuildAttrList(BizControl form, Guid docId, Guid docDefId, Guid attrDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, Guid userId, IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var sqb = new SqlQueryBuilderTool(provider, dataContext, userId);
                return sqb.BuildAttrList(form, docId, docDefId, attrDefId, filter, sortAttrs);
            }
            /*var query = new SqlQuery(docDefId, docId, attrDefId, dataContext);
            try
            {
                query.AddAttribute("&Id");
                AddFormAttributes(query, query.Source, form);
                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                    {
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                    }
                else
                    AddFormSortOrders(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }*/
        }

        public static SqlQuery BuildRefList(BizForm form, Guid docId, Guid attrDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, 
            Guid userId, IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var sqb = new SqlQueryBuilderTool(provider, dataContext, userId);
                return sqb.BuildRefList(form, docId, attrDefId, filter, sortAttrs);
            }
            /*var query = new SqlQuery(dataContext, form.DocumentDefId ?? Guid.Empty, userId);
            try
            {
                query.AddAttribute("&Id");
                query.AndCondition(attrDefId, ConditionOperation.Equal, docId);

                AddFormAttributes(query, query.Source, form);
                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                    {
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                    }
                else
                    AddFormSortOrders(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }*/
        }

        public static SqlQuery BuildRefList(BizControl form, Guid docId, Guid docDefId, Guid attrDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs,
            Guid userId, IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var sqb = new SqlQueryBuilderTool(provider, dataContext, userId);
                return sqb.BuildRefList(form, docId, docDefId, attrDefId, filter, sortAttrs);
            }
            /*var query = new SqlQuery(dataContext, docDefId, userId);
            try
            {
                query.AddAttribute("&Id");
                query.AndCondition(attrDefId, ConditionOperation.Equal, docId);

                AddFormAttributes(query, query.Source, form);
                if (filter != null)
                    AddFormConditions(query, query.Source, filter);

                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                    {
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                    }
                else
                    AddFormSortOrders(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }*/
        }

        /*public static SqlQuery Build(Doc doc, IDataContext dataContext = null)
        {
            var query = new SqlQuery(doc.DocDef.Id, dataContext);
            try
            {
                AddDocConditions(query, query.Source, doc);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }*/

        /*public static SqlQuery Build(DocDef docDef, IDataContext dataContext = null)
        {
            var query = new SqlQuery(dataContext, docDef);
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

        public static SqlQuery Build(BizForm form, IEnumerable<Guid> docIds, IEnumerable<AttributeSort> sortAttrs, Guid userId, 
            IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var sqb = new SqlQueryBuilderTool(provider, dataContext, userId);
                return sqb.Build(form, docIds, sortAttrs);
            }
            /*if (form.DocumentDefId == null)
                throw new ApplicationException("Не могу создать запрос! Форма не связана с документом!");

            var query = new SqlQuery(dataContext, (Guid) form.DocumentDefId, userId);
            try
            {
                query.AddAttribute("&Id");
                AddFormAttributes(query, query.Source, form);
                query.AddCondition(ExpressionOperation.And, query.Source.GetDocDef(), "&Id", ConditionOperation.In,
                    docIds.Cast<object>());
                if (sortAttrs != null)
                    foreach (var attr in sortAttrs)
                        query.AddOrderAttribute(attr.AttributeId, attr.Asc);
                else
                    AddFormSortOrders(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }*/
        }

        public static SqlQuery Build(BizForm form, Guid userId, IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create(dataContext))
            {
                var sqb = new SqlQueryBuilderTool(provider, dataContext, userId);
                return sqb.Build(form);
            }
            /*if (form.DocumentDefId == null)
                throw new ApplicationException("Не могу создать запрос! Форма не связана с документом!");

            var query = new SqlQuery(dataContext, (Guid)form.DocumentDefId, userId);
            try
            {
                query.AddAttribute("&Id");
                AddFormAttributes(query, query.Source, form);
                AddFormSortOrders(query, query.Source, form);

                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }*/
        }

        /*private static void AddFormControls(SqlQuery query, SqlQuerySource source, IEnumerable<BizControl> children)
        {
            if (children == null) return;

            foreach(var control in children)
            {
                if (control is BizDataControl && ((BizDataControl)control).AttributeDefId != null)
                {
                    if (control is BizDocumentControl && control.Children != null && control.Children.Count > 0)
                    {
                        var docControl = (BizDocumentControl) control;
                        var attr = source.GetDocDef().Attributes.First(a => a.Id == (Guid) docControl.AttributeDefId);

                        var slave = query.JoinSource(source, attr.DocDefType, SqlSourceJoinType.Inner, attr.Id);

                        AddFormControls(query, slave, control.Children);
                    }
                    else
                        query.AddAttribute(source, (Guid) ((BizDataControl) control).AttributeDefId);
                }

            }
        }*/

        public static void AddDocConditions(SqlQuery query, SqlQuerySource source, Doc filter)
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
        /*
        public static void AddFormConditions(SqlQuery query, SqlQuerySource source, BizControl filter)
        {
            if (filter == null) return;

            foreach (var control in filter.Children)
            {
                if (control is BizDocumentControl)
                {
                    var attrDefId = ((BizDocumentControl) control).AttributeDefId ?? Guid.Empty;
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
                                    } else if (!txt.Contains("%") && !txt.Contains("_"))
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
                                        (Guid) ((BizDataControl) control).AttributeDefId,
                                        ConditionOperation.Levenstein, txt.Trim());
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
                        if (((BizDataControl)control).ObjectValue != null)
                            query.AddCondition(ExpressionOperation.And, source, (Guid) ((BizDataControl) control).AttributeDefId,
                                               CompareToCondition(control.Operation), ((BizDataControl) control).ObjectValue);
                    }
                }
                else if (control is BizEditText || control is BizEditDateTime)
                {
                    if (((BizDataControl)control).ObjectValue != null)
                        query.AndCondition(source, SystemIdentConverter.Convert(((BizEdit) control).Ident),
                                           CompareToCondition(control.Operation), ((BizDataControl) control).ObjectValue);
                }
                else if (control is BizComboBox)
                {
                    if (((BizDataControl)control).ObjectValue != null)
                        query.AndCondition(source, SystemIdentConverter.Convert(((BizComboBox) control).Ident),
                                           CompareToCondition(control.Operation), ((BizDataControl) control).ObjectValue);
                }
                else if (control.Children != null && control.Children.Count > 0)
                    AddFormConditions(query, source, control);
            }

        }

        public static void AddFormSortOrders(SqlQuery query, SqlQuerySource source, BizControl form)
        {
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
                                query.TryJoinSource(source, attrDef.DocDefType.Id, SqlSourceJoinType.Inner, attrDefId);
                        }

                        AddFormSortOrders(query, ts, control);
                    } 
                    else if (((BizDocumentControl)control).DocForm != null)
                    {
                        if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                            ts = query.TryJoinSource(source,
                                                  attrDef.DocDefType.Id,
                                                  SqlSourceJoinType.Inner,
                                                  ((BizDocumentControl)control).AttributeDefId ?? Guid.Empty);

                        AddFormSortOrders(query, ts, ((BizDocumentControl)control).DocForm);
                    }
                }
                else if (control is BizDataControl && ((BizDataControl)control).AttributeDefId != null && control.SortType != SortType.None)
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

        private static bool HasControlValue(BizControl control)
        {
            var dataControl = control as BizDataControl;
            if (dataControl != null)
                if (dataControl.ObjectValue != null) return true;

            if (control.Children != null)
                return control.Children.Any(HasControlValue);
            return false;
        }

        public static void AddFormAttributes(SqlQuery query, SqlQuerySource source, BizControl form)
        {
            foreach (var control in form.Children)
            {
                if (control.Invisible) continue;

                if (control is BizDataControl && ((BizDataControl)control).AttributeDefId != null)
                    query.AddAttribute(source, (Guid) ((BizDataControl) control).AttributeDefId);
                else if (control is BizEditDateTime)
                {
                    if (((BizEdit)control).Ident == SystemIdent.Created)
                        query.AddAttribute(source, "&Created");
                    else if (((BizEdit)control).Ident == SystemIdent.Modified)
                        query.AddAttribute(source, "&Modified");
                }
                else if (control is BizEditText)
                {
                    query.AddAttribute(source, SystemIdentConverter.Convert(((BizEdit)control).Ident));
                }
                else if (control is BizComboBox)
                    query.AddAttribute(source, SystemIdentConverter.Convert(((BizComboBox)control).Ident));

                if (control is BizDocumentControl)
                {
                    var ts = query.FindSourceByAttributeId(((BizDocumentControl)control).AttributeDefId ?? Guid.Empty);

                    AttrDef attrDef;
                    if (ts == null)
                        attrDef =
                            source.FindAttributeDef(((BizDocumentControl)control).AttributeDefId ?? Guid.Empty);
                    else
                        attrDef = ts.FindAttributeDef(((BizDocumentControl)control).AttributeDefId ?? Guid.Empty);

                    if (attrDef == null) continue;

                    if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                        ts = query.JoinSource(source,
                                              attrDef.DocDefType.Id,
                                              SqlSourceJoinType.LeftOuter,
                                              ((BizDocumentControl) control).AttributeDefId ?? Guid.Empty);

                    if (control.Children != null && control.Children.Count > 0)
                        AddFormAttributes(query, ts, control); 
                    else if (((BizDocumentControl)control).DocForm != null)
                        AddFormAttributes(query, ts, ((BizDocumentControl)control).DocForm);
                }
                else if (control is BizTableColumn)
                {
                    if (((BizTableColumn)control).AttributeDefId != null)
                    {
                        var ts = query.FindSourceByAttributeId((Guid) ((BizTableColumn) control).AttributeDefId);
                        AttrDef attrDef;
                        if (ts == null)
                            attrDef =
                                source.FindAttributeDef((Guid) ((BizTableColumn) control).AttributeDefId);
                        else
                            attrDef = ts.FindAttributeDef((Guid) ((BizTableColumn) control).AttributeDefId);

                        if (attrDef == null) continue;
                        if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                            ts = query.TryJoinSource(source,
                                                  attrDef.DocDefType.Id,
                                                  SqlSourceJoinType.LeftOuter,
                                                  (Guid) ((BizTableColumn) control).AttributeDefId);

                        AddFormAttributes(query, ts, control);
                    }
                }
                else if (control is BizDocumentListForm)
                {
                    var ts = query.FindSourceByAttributeId(((BizDocumentListForm)control).AttributeDefId ?? Guid.Empty);

                    AttrDef attrDef;
                    attrDef = ts == null
                        ? source.FindAttributeDef(((BizDocumentListForm) control).AttributeDefId ?? Guid.Empty)
                        : ts.FindAttributeDef(((BizDocumentListForm) control).AttributeDefId ?? Guid.Empty);

                    if (attrDef == null) continue;

                    if (ts == null || attrDef.DocDefType.Id != ts.GetDocDef().Id)
                        ts = query.TryJoinSource(source,
                                              attrDef.DocDefType.Id,
                                              SqlSourceJoinType.Inner,
                                              ((BizDocumentListForm) control).AttributeDefId ?? Guid.Empty);

                    if (control.Children != null && control.Children.Count > 0)
                        AddFormAttributes(query, ts, control);
                    else if (((BizDocumentListForm)control).TableForm != null)
                        AddFormAttributes(query, ts, ((BizDocumentListForm)control).TableForm);
                }
                else if (control.Children != null && control.Children.Count > 0)
                    AddFormAttributes(query, source, control);
            }

        }
        */
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

        private static SqlQuerySource AddJoinDocConditions(SqlQuery query, SqlQuerySource master, DocAttribute attr)
        {
            var source = query.JoinSource(master, attr.Document.DocDef.Id, SqlSourceJoinType.Inner, attr.AttrDef.Id);

            AddDocConditions(query, source, attr.Document);

            return source;
        }
        /*
        private static SqlQuerySource AddQueryJoin(SqlQuery query, SqlQuerySource master, Guid docDefId, Guid attrDefId)
        {
            return query.TryJoinSource(master, docDefId, SqlSourceJoinType.Inner, attrDefId);
        }*/
    }
}
