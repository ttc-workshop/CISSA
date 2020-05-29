using System;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Core;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class FinishActivity: WorkflowActivity
    {
        public FinishActivity(Finish_Activity activity)
            : base(activity)
        {
            FormId = activity.Form_Id;
            Message = activity.Message;
        }

        public Guid? FormId { get; private set; }
        public string Message { get; private set; }

        public override void Execute(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            if (FormId != null)
                context.Finish((Guid) FormId);
            else if (!String.IsNullOrEmpty(Message))
                context.Finish(Message);
            else
                context.Finish();
        }
    }
}
