using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class GenerateTemplate : BaseContextState, IDocumentContextState
    {
        public string FileName { get; private set; }
        public WorkflowContextData ProcessContext { get; private set; }

        public GenerateTemplate(IContext context, string fileName) : base(context)
        {
            FileName = fileName;
        }

        public GenerateTemplate(IContext context, string fileName, WorkflowContextData processContext)
            : base(context)
        {
            FileName = fileName;
            ProcessContext = processContext;
        }

        public GenerateTemplate(IContext context, ContextState previous, string fileName) : base(context, previous)
        {
            FileName = fileName;
        }

        public GenerateTemplate(IContext context, ContextState previous, string fileName, WorkflowContextData processContext)
            : base(context, previous)
        {
            FileName = fileName;
            ProcessContext = processContext;
        }

        public Guid? DocumentId { get; set; }
        public Doc Document { get; set; }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Report", "Template");
        }

        public Guid? GetDocumentId(IContext context)
        {
            return DocumentId;

        }

        public Doc GetDocument(IContext context)
        {
            return Document;
        }
    }
}