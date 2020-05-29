using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class ShowMessage : ContextState
    {
        public string Message { get; private set; }
        public IList<UserAction> UserActions { get; private set; }

        public ShowMessage(IContext context, string message, IList<UserAction> userActions = null)
            : base(context)
        {
            Message = message;
            UserActions = userActions;
        }

        public ShowMessage(IContext context, ContextState previous, string message, IList<UserAction> userActions = null)
            : base(context, previous)
        {
            Message = message;
            UserActions = userActions;
        }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Form", "Message");
        }
    }
}