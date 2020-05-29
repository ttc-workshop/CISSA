using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class ShowSelectForm : BaseTableForm
    {
        public ShowSelectForm(IContext context, Guid formId, IList<UserAction> userActions = null) 
            : base(context, formId, userActions) { }
        public ShowSelectForm(IContext context, ContextState previous, Guid formId, IList<UserAction> userActions = null)
            : base(context, previous, formId, userActions) { }

        public ShowSelectForm(IContext context, Guid formId, Guid? docStateId, IList<UserAction> userActions = null)
            : base(context, formId, docStateId, userActions) { }

        public ShowSelectForm(IContext context, ContextState previous, Guid formId, Guid? docStateId, IList<UserAction> userActions = null)
            : base(context, previous, formId, docStateId, userActions) { }

        public ShowSelectForm(IContext context, Guid formId, Doc filter, IList<UserAction> userActions = null)
            : base(context, formId, filter, userActions) { }

        public ShowSelectForm(IContext context, ContextState previous, Guid formId, Doc filter, IList<UserAction> userActions = null)
            : base(context, previous, formId, filter, userActions) { }

        public ShowSelectForm(IContext context, Guid formId, IList<Guid> docIdList, IList<UserAction> userActions = null)
            : base(context, formId, docIdList, userActions) { }

        public ShowSelectForm(IContext context, ContextState previous, Guid formId, IList<Guid> docIdList, IList<UserAction> userActions = null)
            : base(context, previous, formId, docIdList, userActions) { }

        public ShowSelectForm(IContext context, Guid formId, IList<BizControl> controls, IList<UserAction> userActions = null)
            : base(context, formId, controls, userActions) { }

        public ShowSelectForm(IContext context, ContextState previous, Guid formId, IList<BizControl> controls, IList<UserAction> userActions = null)
            : base(context, previous, formId, controls, userActions) { }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Form", "Select");
        }
    }
}