using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Documents.AutoAttr;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    public class QueryEngine : IDisposable
    {
        public IDataContext DataContext { get; private set; }
        private readonly bool _ownDataContext;

        public QueryEngine(IDataContext dataContext)
        {
            if (dataContext == null)
            {
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else
                DataContext = dataContext;
        }

        public QueryEngine() : this(null) { }

        public QueryDef QueryFromDoc(Doc doc)
        {
            if (doc == null) return null;

            var query = new QueryBuilder(doc.DocDef.Id);

            foreach(var attr in doc.Attributes)
            {
                if (attr is DocAttribute)
                {
                    if (((DocAttribute)attr).Document != null && HasDocumentValue(((DocAttribute)attr).Document))
                    {
                        IncludeAttribute(query.And(attr.AttrDef.Name), (DocAttribute)attr);
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
                        var txt = ((TextAttribute) attr).Value;
                        if (!String.IsNullOrEmpty(txt))
                            query.And(attr.AttrDef.Name).Contains(txt);
                    }
                    else
                    {
                        query.And(attr.AttrDef.Name).Eq(attr.ObjectValue);
                    }
                }
            }
            return query.Def;
        }

        public QueryDef QueryFromForm(BizForm form)
        {
            if (form == null) return null;

            var query = new QueryBuilder(form.DocumentDefId ?? Guid.Empty);

            var docDef = GetDocDef(query.Def);

            foreach (var ctrl in form.Children)
            {
                if (!(ctrl is BizDataControl) || ((BizDataControl)ctrl).AttributeDefId == null) continue;

                var attributeDefId = ((BizDataControl)ctrl).AttributeDefId;
                if (attributeDefId != null)
                {
                    var attrId = (Guid)attributeDefId;

                    var attr = docDef.Attributes.FirstOrDefault(a => a.Id == attrId);
                    if (attr != null)
                    {
                        if (ctrl is BizDocumentControl)
                        {
                            IncludeDocControl(query.And(attr.Name), attr, ctrl);
                            continue;
                        }

                        if (((BizDataControl) ctrl).ObjectValue == null) continue;

                        if (attr.Type.Id == (short) CissaDataType.Text)
                        {
                            var txt = ((BizDataControl) ctrl).ObjectValue.ToString();
                            if (!String.IsNullOrEmpty(txt))
                                query.And(attr.Name).Contains(txt);
                        }
                        else
                            query.And(attr.Name).Eq(((BizDataControl) ctrl).ObjectValue);
                    }
                }
            }
            return query.Def;
        }

        public Doc FillDocFromQuery(Doc doc, QueryDef queryDef)
        {
            if (queryDef != null && doc != null)
                SetDocAttribute(doc, queryDef.WhereConditions);

            return doc;
        }

        public BizForm FillFormFromQuery(BizForm form, QueryDef queryDef)
        {
            var query = new DocQuery(queryDef, DataContext);

            /*foreach (var condition in query.GetConditions())
            {
                var attrDef = condition.AttrDef;

                if (attrDef != null)
                {
                    var ctrl =
                        form.Children.FirstOrDefault(
                            c => (c is BizDataControl) && (((BizDataControl) c).AttributeDefId == attrDef.Id));

                    if (ctrl is BizDocumentControl)
                    {
                        var docControl = (BizDocumentControl) ctrl;
                        if (condition.Condition.WhereConditions != null)
                        docControl.
                    }
                }
            }*/
            return form;
        }

        private static void SetDocAttribute(Doc doc, IEnumerable<QueryConditionDef> conditions)
        {
            foreach (var condition in conditions)
            {
                var attrHelper = new QueryAttributeDefHelper(condition.Left.Attribute);

                var attr = doc.Attributes.FirstOrDefault(a => attrHelper.IsSame(doc.DocDef, a.AttrDef));
                //var attr = doc.GetAttributeByName(condition.AttributeName);

                if (attr == null) continue;

                var attribute = attr as DocAttribute;
                if (attribute != null &&
                    attribute.Document != null && condition.Conditions != null)
                    SetDocAttribute(attribute.Document, condition.Conditions);
                else if (!(attr is DocListAttribute) && !(attr is AutoAttribute))
                    attr.ObjectValue = condition.Right.Value;
            }
        }

        public void AddConditionFromDoc(QueryDef queryDef, Doc doc)
        {
            if (queryDef == null || doc == null) return;

            var query = new QueryBuilder(queryDef);

            foreach (var attr in doc.Attributes)
            {
                if (attr is DocAttribute)
                {
                    if (((DocAttribute)attr).Document != null && HasDocumentValue(((DocAttribute)attr).Document))
                    {
                        IncludeAttribute(query.And(attr.AttrDef.Name), (DocAttribute)attr);
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
                        var txt = ((TextAttribute) attr).Value;
                        if (!String.IsNullOrEmpty(txt))
                            query.And(attr.AttrDef.Name).Contains(txt);
                    }
                    else
                    {
                        query.And(attr.AttrDef.Name).Eq(attr.ObjectValue);
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
                    var text = ((TextAttribute) attr).Value;
                    if (!String.IsNullOrEmpty(text)) return true;
                }
                else if (attr.ObjectValue != null) return true;
            }
            return false;
        }

        private static void IncludeDocControl(IQueryCondition cond, AttrDef attr, BizControl control)
        {
//            var exp = query.And(attr.Name);
            if (attr.DocDefType == null)
                throw new ApplicationException(String.Format("Не могу сформировать запрос! Атрибут \"{0}\" не ссылается на класс документа", attr.Name));

            var docDef = attr.DocDefType;

            foreach (var ctrl in control.Children)
            {
                if (!(ctrl is BizDataControl) || ((BizDataControl)ctrl).AttributeDefId == null) continue;

                var attributeDefId = ((BizDataControl)ctrl).AttributeDefId;
                if (attributeDefId != null)
                {
                    var attrId = (Guid)attributeDefId;

                    var sub = docDef.Attributes.FirstOrDefault(a => a.Id == attrId);
                    if (sub != null)
                    {
                        if (ctrl is BizDocumentControl)
                        {
                            IncludeDocControl(cond, sub, ctrl);
                            continue;
                        }

                        if (((BizDataControl) ctrl).ObjectValue == null) continue;

                        if (sub.Type.Id == (short) CissaDataType.Text)
                        {
                            var txt = ((BizDataControl) ctrl).ObjectValue.ToString();
                            if (!String.IsNullOrEmpty(txt))
                                cond.Include(sub.Name).Contains(txt);
                        }
                        else
                            cond.Include(sub.Name).Eq(((BizDataControl) ctrl).ObjectValue);
                    }
                }
            }
        }
/*
        private static void IncludeAttribute(IQueryExpression query, DocAttribute attr)
        {
            var exp = query.And(attr.AttrDef.Name);

            foreach (var sub in attr.Document.Attributes)
            {
                if (sub is DocAttribute)
                {
                    if (((DocAttribute) sub).Document != null && HasDocumentValue(((DocAttribute) sub).Document))
                    {
                        IncludeAttribute(exp, (DocAttribute) sub);
                    }
                }
                else if (sub is DocListAttribute)
                {
                    //                    if (((DocListAttribute)attr).AddedDocIds != null)
                }
                else if (sub.ObjectValue != null)
                {
                    if (sub is TextAttribute)
                    {
                        exp.Include(sub.AttrDef.Name).Contains(sub.ObjectValue.ToString());
                    }
                    else
                    {
                        exp.Include(sub.AttrDef.Name).Eq(sub.ObjectValue);
                    }
                }
            }
        }
*/
        private static void IncludeAttribute(IQueryCondition cond, DocAttribute attr)
        {
            foreach (var sub in attr.Document.Attributes)
            {
                if (sub is DocAttribute)
                {
                    if (((DocAttribute)sub).Document != null && HasDocumentValue(((DocAttribute)sub).Document))
                    {
                        IncludeAttribute(cond.Include(sub.AttrDef.Name), (DocAttribute)sub);
                    }
                }
                else if (sub is DocListAttribute)
                {
                    //                    if (((DocListAttribute)attr).AddedDocIds != null)
                }
                else if (sub.ObjectValue != null)
                {
                    if (sub is TextAttribute)
                    {
                        var txt = ((TextAttribute) sub).Value;
                        if (!String.IsNullOrEmpty(txt))
                            cond.Include(sub.AttrDef.Name).Contains(txt);
                    }
                    else
                        cond.Include(sub.AttrDef.Name).Eq(sub.ObjectValue);
                }
            }
        }

        public DocDef GetDocDef(QueryDef def)
        {
            using (var defRepo = new DocDefRepository(DataContext, def.UserId))
            {
                DocDef docDef;

                if (def.Source != null && def.DocDefId == Guid.Empty)
                {
                    docDef = defRepo.DocDefByName(def.DocDefName);
                    def.Source.DocDefId = docDef.Id;
                }
                else
                    docDef = defRepo.DocDefById(def.DocDefId);

                return docDef;
            }
        }

        public string QueryDefToSql(QueryDef def)
        {
            return "";
        }

        public QueryDef SqlToQueryDef(string SQL)
        {
            return null;
        }

        public void Dispose()
        {
            if (_ownDataContext && DataContext != null)
            {
                try
                {
                    DataContext.Dispose();
                    DataContext = null;
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "QueryEngine.Dispose");
                    throw;
                }
            }
        }

        ~QueryEngine()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "QueryEngine.Finalize");
                    throw;
                }
        }
    }
}
