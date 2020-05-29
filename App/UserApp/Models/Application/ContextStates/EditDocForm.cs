using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class EditDocForm: BaseDocForm
    {
        public EditDocForm(IContext context, Guid formId, Guid docId, IList<UserAction> userActions = null, IList<ModelMessage> errors = null)
            : base(context, formId, docId, userActions, errors)
        {}

        public EditDocForm(IContext context, ContextState previous, Guid formId, Guid docId, IList<UserAction> userActions = null, IList<ModelMessage> errors = null)
            : base(context, previous, formId, docId, userActions, errors)
        {}

        public EditDocForm(IContext context, Guid formId, Doc document, IList<UserAction> userActions = null, IList<ModelMessage> errors = null)
            : base(context, formId, document, userActions, errors)
        {}

        public EditDocForm(IContext context, ContextState previous, Guid formId, Doc document, IList<UserAction> userActions = null, IList<ModelMessage> errors = null)
            : base(context, previous, formId, document, userActions, errors)
        {}

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Form", "Edit");
        }
    }
}