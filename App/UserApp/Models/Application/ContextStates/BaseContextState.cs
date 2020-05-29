using System;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class BaseContextState : ContextState
    {
        public BaseContextState(IContext context) : base(context) { }
        public BaseContextState(IContext context, ContextState previous) : base(context, previous) { }

        public bool IsProcessState { get; set; }
        internal IProcessContextState FindProcessContextState()
        {
            var prev = Previous;

            while (prev != null)
            {
                if (prev is IProcessContextState)
                    return (IProcessContextState)prev;

                prev = prev.Previous;
            }
            return null;
        }

        public ContextState FindProcessContextState(WorkflowContextData context)
        {
            var prev = Previous;

            while (prev != null)
            {
                if (prev is IProcessContextState)
                {
                    var processContext = ((IProcessContextState) prev).GetWorkflowContext();

                    if (processContext.ParentProcessId == context.ParentProcessId)
                      return prev;
                }

                prev = prev.Previous;
            }
            return null;
        }

        public bool InProcess
        {
            get { return FindProcessContextState() != null; }
        }

        public WorkflowContextData LastContext
        {
            get
            {
                var runProcess = FindProcessContextState();

                return runProcess != null ? runProcess.GetWorkflowContext() : null;
            }
        }

        public virtual BizForm FindFormById(Guid formId)
        {
            var prev = (ContextState)this;

            while (prev != null)
            {
                if (prev is IFormContextState)
                {
                    var form = ((IFormContextState) prev).GetForm();

                    if (form != null && form.Id == formId)
                        return form;
                }
                prev = prev.Previous;
            }
            return null;
        }

        public Doc FindDocumentById(Guid docId, IContext context)
        {
            ContextState prev = this;

            while (prev != null)
            {
                if (prev is IDocumentContextState)
                {
                    var document = ((IDocumentContextState) prev).GetDocument(context);

                    if (document != null && document.Id == docId)
                        return document;
                }
                prev = prev.Previous;
            }
            return null;
        }

        public virtual void Update(IContext context)
        {
        }

        public virtual void UpdateDocument(Doc document)
        {
            if (Previous != null && Previous is BaseContextState)
                ((BaseContextState)Previous).UpdateDocument(document);
        }
    }
}