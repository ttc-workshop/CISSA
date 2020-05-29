using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class ShowParamForm : BaseDocForm
    {
        public ShowParamForm(IContext context, Guid formId, Guid docId, IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, formId, docId, userActions, errorMessages) {}

        public ShowParamForm(IContext context, ContextState previous, Guid formId, Guid docId, IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, previous, formId, docId, userActions, errorMessages) {}

        public ShowParamForm(IContext context, Guid formId, Doc document, IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, formId, document, userActions, errorMessages) {}

        public ShowParamForm(IContext context, ContextState previous, Guid formId, Doc document, IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, previous, formId, document, userActions, errorMessages) {}

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Form", "Param");
        }
    }

}