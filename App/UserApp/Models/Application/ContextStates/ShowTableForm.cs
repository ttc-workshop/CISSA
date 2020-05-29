using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class ShowTableForm : BaseTableForm
    {
        public bool NoLoad { get; set; }

        public ShowTableForm(IContext context, Guid formId, IList<UserAction> userActions = null) 
            : base(context, formId, userActions) { }

        public ShowTableForm(IContext context, ContextState previous, Guid formId, IList<UserAction> userActions = null) 
            : base(context, previous, formId, userActions) { }

        public ShowTableForm(IContext context, Guid formId, Guid? docStateId, IList<UserAction> userActions = null)
            : base(context, formId, docStateId, userActions) { }

        public ShowTableForm(IContext context, ContextState previous, Guid formId, Guid? docStateId, IList<UserAction> userActions = null)
            : base(context, previous, formId, docStateId, userActions) { }

        public ShowTableForm(IContext context, Guid formId, Doc filter, IList<UserAction> userActions = null)
            : base(context, formId, filter, userActions) { }

        public ShowTableForm(IContext context, ContextState previous, Guid formId, Doc filter, IList<UserAction> userActions = null)
            : base(context, previous, formId, filter, userActions) { }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Form", "List");
        }
    }
}