using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Repository
{
    public interface IFormRepository
    {
        BizForm Get(Guid formId);
//        BizForm Get(Guid formId, Guid docId);
//        BizForm Save(BizForm formForSave);

        BizForm SetDoc(BizForm form, Doc document);
        Doc GetDoc(BizForm form, Doc document);

        bool AddFieldValue(out string message, BizForm bizForm, Guid controlId, object value, bool onlyCheck);
        ManagedTableForm GetTableForm(Guid formId);

        TControl Find<TControl>(BizControl form, Guid id) where TControl : BizControl;

        //SearchParameter GetParameterForSearch(BizForm bizForm, Guid controlId, object value);
        //ManagedTableForm Search(Guid formId, List<SearchParameter> searchParameters);
    }
}