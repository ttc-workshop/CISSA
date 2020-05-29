using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class ExceptionState : ContextState
    {
        public string Message { get; private set; }
        public IList<UserAction> UserActions { get; private set; }

        public ExceptionState(IContext context, ContextState previous, string message, IList<UserAction> userActions = null)
            : base(context, previous)
        {
            Message = message;
            UserActions = userActions;
            if (Previous is RunProcess) Previous = Previous.Previous;
        }

        public ExceptionState(IContext context, string message, IList<UserAction> userActions = null)
            : this(context, context.Find<MainForm>(), message, userActions)
        {
            Message = message;
            UserActions = userActions;
        }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Form", "Error");
        }
    }
}