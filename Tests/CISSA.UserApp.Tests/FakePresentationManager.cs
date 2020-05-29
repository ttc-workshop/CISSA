using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;
using BizControl = Intersoft.CISSA.UserApp.ServiceReference.BizControl;
using BizDetailForm = Intersoft.CISSA.UserApp.ServiceReference.BizDetailForm;
using BizForm = Intersoft.CISSA.UserApp.ServiceReference.BizForm;
using BizMenu = Intersoft.CISSA.UserApp.ServiceReference.BizMenu;
using BizTableForm = Intersoft.CISSA.UserApp.ServiceReference.BizTableForm;

namespace CISSA.UserApp.Tests
{
    class FakePresentationManager : IPresentationManager
    {
        public BizForm GetForm(Guid formId, Guid docId)
        {
            return new BizForm();
        }

        public BizForm GetAnyForm(Guid formId, int languageId = 0)
        {
            throw new NotImplementedException();
        }

        public BizDetailForm GetDetailForm(Guid formId, int languageId = 0)
        {
            return new BizDetailForm();
        }

        public BizDetailForm GetDetailFormWithData(Guid formId, Guid docId, int languageId)
        {
            throw new NotImplementedException();
        }

        public BizTableForm GetGridForm(Guid formId, int languageId = 0)
        {
            return new BizTableForm();
        }

        public BizTableForm GetTableForm(Guid formId, List<Guid> documentsIds)
        {
            return new BizTableForm();
        }

        public BizForm SetFormDoc(BizForm form, Doc document)
        {
            return form;
        }

        public Doc GetFormDoc(BizForm form, Doc document)
        {
            return document;
        }

        public BizForm SaveForm(BizForm form)
        {
            return new BizForm();
        }

        public BizForm GetMainForm()
        {
            return new BizForm();
        }

        public BizResult ExecuteBizAction(Guid actionId)
        {
            return new BizResult();
        }

        public List<BizMenu> GetMenus(Guid workerId, int languageId = 0)
        {
            return new List<BizMenu>();
        }

        public List<LanguageType> GetLanguages()
        {
            throw new NotImplementedException();
        }

        public BizForm TranslateForm(BizForm form, int languageId)
        {
            throw new NotImplementedException();
        }

        public List<BizMenu> TranslateMenus(List<BizMenu> menus, int languageId)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, Guid? docStateId, BizForm filter, List<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRowsFromQuery(out int count, BizForm form, QueryDef def, List<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRowsFromList(BizForm form, List<Guid> docIds, List<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetDocListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetRefListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, Guid? docStateId, BizForm filter, List<Guid> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }


        public void SetControlData(BizControl control, Doc document)
        {
            return;
        }
    }
}
