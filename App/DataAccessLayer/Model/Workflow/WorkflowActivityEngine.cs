using System;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class WorkflowActivityEngine
    {
        public IAppServiceProvider Provider { get; private set; }
        public IDataContext DataContext { get; private set; }

        public WorkflowActivity Activity { get; private set; }

        public WorkflowActivityEngine(WorkflowActivity activity, IAppServiceProvider provider, IDataContext dataContext)
        {
            Provider = provider;

            DataContext = dataContext;

            Activity = activity;
        }

        public void Execute(WorkflowContext context)
        {
            if (Activity is ScriptActivity)
            {
                var repo = Provider.Get<IWorkflowRepository>();
                var process = Activity.ProcessId != Guid.Empty ? (repo.LoadProcessById(Activity.ProcessId)) : null;
                var processName = process != null ? process.Name : "-";

                using (new Monitor("Workflow activities execution",
                    string.Format("[{0}][{1}] - [{2}/{3}]", processName, Activity.Name, Activity.ProcessId, Activity.Id)))
                    Activity.Execute(context, Provider, DataContext);
            } else 
                Activity.Execute(context, Provider, DataContext);
        }

        public void AfterExecution(WorkflowContext context)
        {
            Activity.AfterExecution(context, Provider, DataContext);
        }
    }
}
