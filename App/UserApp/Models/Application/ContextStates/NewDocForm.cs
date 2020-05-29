using System;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class NewDocForm : BaseForm, IDocumentContextState
    {
        public NewDocForm(IContext context, Guid formId) : base(context, formId)
        {
//            _docManager = context.GetDocManager();
//            _presentationManager = context.GetPresentationManager();
        }

        public NewDocForm(IContext context, ContextState previous, Guid formId) : base(context, previous, formId)
        {
//            _docManager = context.GetDocManager();
//            _presentationManager = context.GetPresentationManager();
        }
/*
        public NewDocForm(IContext context, Guid formId, Doc document) : base(context, formId, document)
        {}

        public NewDocForm(IContext context, ContextState previous, Guid formId, Doc document) : base(context, previous, formId, document)
        {}
*/
        public Doc Document { get; set; }
        public Doc GetCurrentDocument(IContext context) 
        {
            return Document ?? CreateDoc(context); 
        }

//        private readonly IDocManager _docManager;
//        private readonly IPresentationManager _presentationManager;

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Form", "New");
        }

        protected Doc CreateDoc(IContext context)
        {
            if (Document == null)
            {
                var form = GetCurrentForm(context);

                if (form.DocumentDefId != null)
                {
                    var dm = context.GetDocumentProxy();
                    Document = dm.Proxy.DocumentNew((Guid) form.DocumentDefId);

                    var processContext = FindProcessContextState();
                    if (processContext != null)
                    {
                        var workflowContext = processContext.GetWorkflowContext();
                        if (workflowContext == null) return Document;

                        Document = dm.Proxy.DocumentInit(Document, workflowContext);
                    }

                    return Document;
                }
            }
            return Document;
        }

        public Guid? GetDocumentId(IContext context)
        {
            var document = GetCurrentDocument(context);

            if (document != null) return document.Id;

            return null;
        }

        public Doc GetDocument(IContext context)
        {
            return GetCurrentDocument(context);
        }
    }
}