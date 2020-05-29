using System;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class GateCallActivity : WorkflowActivity
    {
        public GateCallActivity(Gate_Call_Activity activity)
            : base(activity)
        {
            CallGateId = activity.Gate_Id;
            ProcessName = activity.Process_Name;
        }

        public Guid? CallGateId { get; set; }

        public string ProcessName { get; set; }

        public override void Execute(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            if (CallGateId != null)
                context.CallGateProcess((Guid) CallGateId, ProcessName);
            else
                context.ThrowException("No Call Gate", "Вызываемый шлюз не указан");
        }

        public override void AfterExecution(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            try
            {
                base.Execute(context, provider, dataContext);
            }
            catch (Exception e)
            {
                context.ThrowException(e);
            }
        }
    }
}