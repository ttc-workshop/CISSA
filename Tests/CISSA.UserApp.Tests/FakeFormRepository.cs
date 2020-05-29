using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.Models;
using Intersoft.CISSA.UserApp.Models.Repository;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace CISSA.UserApp.Tests
{
    class FakeFormRepository: IFormRepository
    {
        public BizForm Get(Guid formId)
        {
            return new BizForm();
        }

        public BizForm Get(Guid formId, Guid docId)
        {
            return new BizForm();
        }

        public BizForm Save(BizForm formForSave)
        {
            return formForSave;
        }

        public BizForm SetDoc(BizForm form, Doc document)
        {
            return form;
        }

        public Doc GetDoc(BizForm form, Doc document)
        {
            return document;
        }

        public bool AddFieldValue(out string message, BizForm bizForm, Guid controlId, object value, bool onlyCheck)
        {
            message = "";
            return true;
        }

        public ManagedTableForm GetTableForm(Guid formId)
        {
            return new ManagedTableForm(new BizTableForm {Id = formId}/*, (IDocManager) null, (IPresentationManager) null*/);
        }

        /*public SearchParameter GetParameterForSearch(BizForm bizForm, Guid controlId, object value)
        {
            return new SearchParameter();
        }*/

        /*public ManagedTableForm Search(Guid formId, List<SearchParameter> searchParameters)
        {
            return new ManagedTableForm(new BizTableForm { Id = formId }, (IDocManager)null);
        }*/
    }
}