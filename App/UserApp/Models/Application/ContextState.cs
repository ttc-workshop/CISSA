using System;

namespace Intersoft.CISSA.UserApp.Models.Application
{
    public class ContextState
    {
        public ContextState Previous { get; internal set; }
        public Guid? MenuId { get; set; }

        public ContextState(IContext context, ContextState previous)
        {
            Previous = previous;
            MenuId = null;
            context.Set(this);
        }

        public ContextState(IContext context) : this(context, context.Get()) { }

        public virtual ContextAction GetAction(IContext context)
        {
           return new ContextAction("Form", "Error"); 
        }

        public ContextState Return(IContext context)
        {
            if (context.Get() == this)
            {
                context.Set(Previous);
                return Previous;
            }
            throw new ApplicationException(Resources.Base.AppContextChangeError
                /*"Ошибка при попытке изменения состояния контекста! Состояние не является конечным!"*/);
        }

        protected virtual void Initialize(IContext context)
        {
        }
    }
}