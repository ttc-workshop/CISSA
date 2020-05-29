using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class ShowDocForm : BaseDocForm
    {
        public ShowDocForm(IContext context, Guid formId, Guid docId, IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, formId, docId, userActions, errorMessages) {}

        public ShowDocForm(IContext context, ContextState previous, Guid formId, Guid docId, IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, previous, formId, docId, userActions) {}

        public ShowDocForm(IContext context, Guid formId, Doc document, IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, formId, document, userActions) {}

        public ShowDocForm(IContext context, ContextState previous, Guid formId, Doc document, IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, previous, formId, document, userActions) {}

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Form", "Show");
        }
    }
}