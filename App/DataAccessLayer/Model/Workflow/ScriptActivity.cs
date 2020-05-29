using System;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Core;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class ScriptActivity : WorkflowActivity
    {
        public ScriptActivity(Script_Activity activity)
            : base(activity)
        {
            _script = activity.Script;
        }

        private readonly string _script;

        // private static readonly object ScriptExecLock = new object();

        public override void Execute(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            try
            {
                var scriptManager = new ScriptManager(_script);

                //lock(ScriptExecLock)
                {
                    scriptManager.Execute(context);
                }

                base.Execute(context, provider, dataContext);
            }
            catch(Exception e)
            {
                OnException(context, e);
                context.ThrowException(e);
            }
        }
    }
}
