﻿namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class AboutForm : BaseContextState
    {
        public AboutForm(IContext context) : base(context)
        {
        }

        public AboutForm(IContext context, ContextState previous) : base(context, previous)
        {
        }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Home", "About");
        }
    }
}