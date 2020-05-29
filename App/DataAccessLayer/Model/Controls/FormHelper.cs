using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    public class FormHelper
    {
        public BizForm Form { get; private set; }

        private readonly IDocDefRepository _docDefRepo;
        private readonly IFormRepository _formRepo;

        public FormHelper(IAppServiceProvider provider, BizForm form)
        {
            Form = form;

            _docDefRepo = provider.Get<IDocDefRepository>();
            _formRepo = provider.Get<IFormRepository>();
        }

        public DocDef FindAttributeDocDef(Guid? attrId)
        {
            if (attrId == null) return null;

            var docDef = GetFormDocDefId(Form);

            if (docDef == null) return null;
            if (docDef.Attributes.FirstOrDefault(a => a.Id == attrId) != null)
                return docDef;

            if (Form.Children != null)
                foreach (var child in Form.Children)
                {
                    var controlDocDef = FindControlAttributeDocDef(child, docDef, (Guid) attrId);
                    if (controlDocDef != null)
                        return controlDocDef;
                }

            return null;
        }

        public void InitDependency()
        {
            InitControlDependency(Form);
        }

        public void SetQueryParams(SqlQuery query)
        {
            if (query == null) return;

            foreach (var param in query.GetAllParams())
            {
                if (!String.IsNullOrEmpty(param.Name) && param.Name.Length > 1 && param.Name[0] == '@')
                {
                    var controlRef = param.Name.Substring(1);

                    FindControlAndSetParam(controlRef, param);
                }
            }
        }

        private void FindControlAndSetParam(string controlRef, QueryConditionValueDef param)
        {
            var masterControl = FindControlByRef(controlRef);

            var dataControl = masterControl as BizDataControl;
            if (dataControl != null)
                param.Value = dataControl.ObjectValue;
        }

        private void InitControlDependency(BizControl control)
        {
            if (control == null) return;

            if (control.QueryItems != null)
            {
                foreach (var item in control.QueryItems)
                {
                    foreach (var conditionParam in item.FindParamConditions())
                    {
                        if (conditionParam.ParamName.Length > 0 && conditionParam.ParamName[0] == '@')
                        {
                            var controlRef = conditionParam.ParamName.Substring(1);
                            FindControlAndSetDependent(controlRef, control);
                        }
                    }
                }
            }

            if (control.Children != null)
                foreach (var child in control.Children)
                    InitControlDependency(child);
        }

        private BizControl FindControlByRef(string controlRef)
        {
            var finder = new ControlFinder(Form);
            Guid id;

            if (Guid.TryParse(controlRef, out id))
            {
                return
                    finder.FirstOrDefault(
                        c => c is BizDataControl && (c.Id == id || ((BizDataControl)c).AttributeDefId == id));
            }
            return
                finder.FirstOrDefault(
                    c =>
                    {
                        var dataControl = c as BizDataControl;
                        if (dataControl == null) return false;
                        if (controlRef.Equals(dataControl.AttributeName, StringComparison.OrdinalIgnoreCase))
                            return true;
                        if (controlRef.Equals(dataControl.Name, StringComparison.OrdinalIgnoreCase))
                            return true;
                        var docDef = FindAttributeDocDef(dataControl.AttributeDefId);
                        if (docDef == null) return false;
                        var attrDef =
                            docDef.Attributes.FirstOrDefault(
                                a => String.Equals(a.Name, controlRef, StringComparison.OrdinalIgnoreCase));
                        if (attrDef == null) return false;
                        return controlRef.Equals(attrDef.Name);
                    });
        }

        private void FindControlAndSetDependent(string controlRef, BizControl control)
        {
            var masterControl = FindControlByRef(controlRef);

            if (masterControl != null)
            {
                if (masterControl.Dependents == null) masterControl.Dependents = new List<Guid>();
                masterControl.Dependents.Add(control.Id);

                if (control.Masters == null) control.Masters = new List<Guid>();
                control.Masters.Add(masterControl.Id);
            }
        }

        private DocDef FindControlAttributeDocDef(BizControl control, DocDef parentDocDef, Guid attrId)
        {
            var docControl = control as BizDocumentControl;
            if (docControl != null)
            {
                if (docControl.AttributeDefId != null)
                {
                    var docControlAttrDef = parentDocDef.Attributes.FirstOrDefault(a => a.Id == (Guid) docControl.AttributeDefId);
                    if (docControlAttrDef != null && docControlAttrDef.DocDefType != null)
                    {
                        var docDef = _docDefRepo.Find(docControlAttrDef.DocDefType.Id);
                        if (docDef != null)
                        {
                            if (docDef.Attributes.FirstOrDefault(a => a.Id == attrId) != null)
                                return docDef;

                            if (docControl.Children != null)
                                foreach (var child in control.Children)
                                {
                                    var attrDocDef = FindControlAttributeDocDef(child, docDef, attrId);
                                    if (attrDocDef != null) return attrDocDef;
                                }
                        }
                    }
                }
            }
            else
            {
                var docList = control as BizDocumentListForm;
                if (docList != null)
                {
                    if (docList.AttributeDefId != null)
                    {
                        var docListAttrDef = parentDocDef.Attributes.FirstOrDefault(a => a.Id == (Guid) docList.AttributeDefId);
                        if (docListAttrDef != null && docListAttrDef.DocDefType != null)
                        {
                            var docDef = _docDefRepo.Find(docListAttrDef.DocDefType.Id);
                            if (docDef != null)
                            {
                                if (docDef.Attributes.FirstOrDefault(a => a.Id == attrId) != null)
                                    return docDef;

                                if (docList.Children != null)
                                    foreach (var child in control.Children)
                                    {
                                        var attrDocDef = FindControlAttributeDocDef(child, docDef, attrId);
                                        if (attrDocDef != null) return attrDocDef;
                                    }
                            }
                        }
                    }
                    else
                    {
                        var dlForm = docList.TableForm ??
                                     (docList.FormId != null ? _formRepo.FindTableForm((Guid) docList.FormId) : null);

                        if (dlForm != null && dlForm.DocumentDefId != null)
                        {
                            var docDef = _docDefRepo.Find((Guid) dlForm.DocumentDefId);
                            if (docDef != null)
                            {
                                if (docDef.Attributes.FirstOrDefault(a => a.Id == attrId) != null)
                                    return docDef;

                                if (dlForm.Children != null)
                                    foreach (var child in dlForm.Children)
                                    {
                                        var attrDocDef = FindControlAttributeDocDef(child, docDef, attrId);
                                        if (attrDocDef != null) return attrDocDef;
                                    }
                            }
                        }
                    }
                }
                else
                {
                    var tableColumn = control as BizTableColumn;
                    if (tableColumn != null && tableColumn.AttributeDefId != null)
                    {
                        var tableColumnAttrDef = parentDocDef.Attributes.FirstOrDefault(a => a.Id == (Guid) tableColumn.AttributeDefId);
                        if (tableColumnAttrDef != null && tableColumnAttrDef.DocDefType != null)
                        {
                            var docDef = _docDefRepo.Find(tableColumnAttrDef.DocDefType.Id);
                            if (docDef != null)
                            {
                                if (docDef.Attributes.FirstOrDefault(a => a.Id == attrId) != null)
                                    return docDef;

                                if (tableColumn.Children != null)
                                    foreach (var child in tableColumn.Children)
                                    {
                                        var attrDocDef = FindControlAttributeDocDef(child, docDef, attrId);
                                        if (attrDocDef != null) return attrDocDef;
                                    }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private DocDef GetFormDocDefId(BizForm form)
        {
            return form.DocumentDefId != null ? _docDefRepo.Find((Guid) form.DocumentDefId) : null;
        }
    }
}
