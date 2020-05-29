using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class RunProcess : BaseContextState, IProcessContextState
    {
        public Guid ProcessId { get; private set; }
        public Guid? DocumentId { get; private set; }

        public RunProcess(IContext context, Guid processId)
            : base(context)
        {
            ProcessId = processId;

            var wm = context.GetWorkflowProxy();
            Run(wm.Proxy);
        }

        public RunProcess(IContext context, Guid processId, Guid docId)
            : base(context)
        {
            ProcessId = processId;
            DocumentId = docId;

            var wm = context.GetWorkflowProxy();
            Run(wm.Proxy);
        }

        public RunProcess(IContext context, ContextState previous, Guid processId)
            : base(context, previous)
        {
            ProcessId = processId;

            var wm = context.GetWorkflowProxy();
            Run(wm.Proxy);
        }

        public RunProcess(IContext context, ContextState previous, Guid processId, Guid docId)
            : base(context, previous)
        {
            ProcessId = processId;
            DocumentId = docId;

            var wm = context.GetWorkflowProxy();
            Run(wm.Proxy);
        }

        public WorkflowContextData ProcessContext { get; internal set; }

        protected void Run(IWorkflowManager workflowManager)
        {
            var param = new Dictionary<string, object>();
            
            if (DocumentId != null)
            {
                /*var document = FindDocumentById((Guid) DocumentId);

                if (document != null)
                    param.Add("InputDocument", document);*/

                param.Add("InputDocumentId", DocumentId);
            }
            if (MenuId != null)
            {
                param.Add("InputAttributeId", MenuId);
            }

            ProcessContext = workflowManager.WorkflowExecute(ProcessId, param);
        }

        public WorkflowContextData ContinueWithUserAction(Guid actionId, IWorkflowManager workflowManager)
        {
            return ProcessContext = workflowManager.WorkflowContinueWithUserAction(ProcessContext, actionId);
        }

        public WorkflowContextData ContinueWithDocument(Guid docId, IWorkflowManager workflowManager)
        {
            return ProcessContext = workflowManager.WorkflowContinueWithDocumentId(ProcessContext, docId);
        }

        public WorkflowContextData ContinueWithDocument(Doc document, IWorkflowManager workflowManager)
        {
            return ProcessContext = workflowManager.WorkflowContinueWithDocument(ProcessContext, document);
        }

        public WorkflowContextData Continue(IWorkflowManager workflowManager)
        {
            return ProcessContext = workflowManager.WorkflowContinue(ProcessContext);
        }

        public WorkflowContextData ContinueWithUploadFile(byte[] fileData, string fileName, IWorkflowManager workflowManager)
        {
            return ProcessContext = workflowManager.WorkflowContinueWithUploadFile(ProcessContext, fileData, fileName);
        }

        public override ContextAction GetAction(IContext context)
        {
            if (ProcessContext == null) return new ContextAction("Process", "ShowException");

            switch (ProcessContext.State)
            {
                case WorkflowRuntimeState.ShowForm:
                    return new ContextAction("Process", "ShowForm");

                case WorkflowRuntimeState.ShowSelectForm:
                    return new ContextAction("Process", "ShowSelectForm");

                case WorkflowRuntimeState.ShowParamForm:
                    return new ContextAction("Process", "ShowParamForm");

                case WorkflowRuntimeState.Finish:
                    return new ContextAction("Process", "Finish");

                case WorkflowRuntimeState.ShowMessage:
                    return new ContextAction("Process", "ShowMessage");

                case WorkflowRuntimeState.ShowTemplateReport:
                    return new ContextAction("Process", "ShowTemplateReport");

                case WorkflowRuntimeState.SendFile:
                    return new ContextAction("Process", "SendFile");

                case WorkflowRuntimeState.UploadFile:
                    return new ContextAction("Process", "UploadFile");

                default:
                    return new ContextAction("Process", "ShowException");
            }
        }

        public Guid GetProcessId()
        {
            return ProcessId;
        }

        public WorkflowContextData GetWorkflowContext()
        {
            return ProcessContext;
        }

        public void DownloadComplete(IContext context)
        {
            if (ProcessContext != null &&
                (ProcessContext.State == WorkflowRuntimeState.SendFile ||
                 ProcessContext.State == WorkflowRuntimeState.ShowTemplateReport))
            {
                var wm = context.GetWorkflowProxy();
                Continue(wm.Proxy);
            }
        }
    }
}