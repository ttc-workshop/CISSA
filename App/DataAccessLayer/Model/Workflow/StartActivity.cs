using System;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Core;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class StartActivity: WorkflowActivity
    {
        public StartActivity(Start_Activity activity) : base(activity) { }

        public override void Execute(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            /*
             Действие (Execute) StartActivity заключается в том, 
             что выполняется переход к следующему действию. 
             */
            /*var queryTargetId = from l in dataContext.ObjectDefs.OfType<Activity_Link>()
                                where l.Source_Id == Id //context.ActivityId //&& l.Parent_Id == context.ProcessId
                                      && (l.Deleted == null || l.Deleted == false)
                                select l.Target_Id;*/

            var link = TargetLinks != null ? TargetLinks.FirstOrDefault(l => l.UserActionId == null) : null;

            if (link == null) //(!queryTargetId.Any())
            {
                context.ThrowException("", "Не удалось найти действие для продолжения");
                return;
            }

            Guid? newGuid = link.TargetId; //queryTargetId.First();

            if (!newGuid.HasValue)
            {
                context.ThrowException("", "В Activity_Link не указано следующее действие (TargetId)");
                return;
            }

            context.RunActivity(newGuid.Value);
        }
    }
}
