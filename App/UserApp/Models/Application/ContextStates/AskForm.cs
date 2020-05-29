using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class AskForm : BaseContextState
    {
        public string Message { get; internal set; }
//        public string YesUrl { get; private set; }
//        public string NoUrl { get; private set; }
        public ContextAction YesAction { get; private set; }
        public ContextAction NoAction { get; private set; }

/*
        public AskForm(IContext context, string message, string yesUrl, string noUrl) : base(context)
        {
            Message = message;
            YesUrl = yesUrl;
            NoUrl = noUrl;
        }

        public AskForm(IContext context, ContextState previous, string message, string yesUrl, string noUrl)
            : base(context, previous)
        {
            Message = message;
            YesUrl = yesUrl;
            NoUrl = noUrl;
        }
*/

        public AskForm(IContext context, string message, ContextAction yesAction, ContextAction noAction)
            : base(context)
        {
            Message = message;
            YesAction = yesAction;
            NoAction = noAction;
        }

        public AskForm(IContext context, ContextState previous, string message, ContextAction yesAction, ContextAction noAction)
            : base(context, previous)
        {
            Message = message;
            YesAction = yesAction;
            NoAction = noAction;
        }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Form", "Ask");
        }
    }
}