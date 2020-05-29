using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class BaseDocForm : BaseForm, IDocumentContextState
    {
        private readonly Guid? _documentId = null;

        public Doc Document { get; set; }
        public Guid? DocumentId
        {
            get
            {
                if (Document != null) return Document.Id;
                return _documentId;
            }
        }

        public IList<UserAction> UserActions { get; private set; }

        public IList<ModelMessage> ErrorMessages { get; private set; }

        public BaseDocForm(IContext context, Guid formId, Guid docId, IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, formId)
        {
            _documentId = docId;
//            _docManager = context.GetDocManager();
            UserActions = userActions;
            ErrorMessages = errorMessages;
        }

        public BaseDocForm(IContext context, ContextState previous, Guid formId, Guid docId,
            IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, previous, formId)
        {
            _documentId = docId;
//            _docManager = context.GetDocManager();
            UserActions = userActions;
            ErrorMessages = errorMessages;
        }

        public BaseDocForm(IContext context, Guid formId, Doc document, IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, formId)
        {
            Document = document;
            if (document != null) _documentId = document.Id;
//            _docManager = context.GetDocManager();
            UserActions = userActions;
            ErrorMessages = errorMessages;
        }

        public BaseDocForm(IContext context, ContextState previous, Guid formId, Doc document,
            IList<UserAction> userActions = null, IList<ModelMessage> errorMessages = null)
            : base(context, previous, formId)
        {
            Document = document;
            if (document != null) _documentId = document.Id;
//            _docManager = context.GetDocManager();
            UserActions = userActions;
            ErrorMessages = errorMessages;
        }

//        private readonly IDocManager _docManager;

        public Doc GetCurrentDocument(IContext context)
        {
            if (Document != null) return Document;

            if (_documentId != null)
            {
                Document = FindDocumentById((Guid) _documentId, context);

                if (Document != null) return Document;

                var dm = context.GetDocumentProxy();
                return Document = dm.Proxy.DocumentLoad((Guid) _documentId);
            }
            return null;
        }

        public Guid? DocListId { get; set; }

        public Guid? GetDocumentId(IContext context)
        {
            return DocumentId;
        }

        public Doc GetDocument(IContext context)
        {
            return Document;
        }

        public override void Update(IContext context)
        {
            if (Document != null)
            {
                var dm = context.GetDocumentProxy();
                {
                    Document = dm.Proxy.DocumentLoad(Document.Id);
                }
                UpdateDocument(Document);
            }
        }

        public override void UpdateDocument(Doc document)
        {
            if (Document != null && document != null && 
                Document != document && Document.Id == document.Id) Document = document;
            
            base.UpdateDocument(document);
        }

        public override void CheckFormLanguage(IContext context)
        {
            base.CheckFormLanguage(context);

            if (UserActions != null && UserActions.Count > 0)
            {
                var pm = context.GetPresentationProxy();
                UserActions = pm.Proxy.TranslateUserActions(new List<UserAction>(UserActions), context.GetLanguage());
            }
        }
    }
}