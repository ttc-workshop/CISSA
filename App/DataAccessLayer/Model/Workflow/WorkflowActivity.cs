using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Core;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class WorkflowActivity
    {
        private readonly List<ActivityLink> _targetLinks = new List<ActivityLink>();

        public WorkflowActivity(Workflow_Activity activity)
        {
            Id = activity.Id;
            Name = activity.Name;
            Operation = activity.Type_Id ?? 0;
            ProcessId = activity.Parent_Id ?? Guid.Empty;

            foreach (var link in activity.Target_Links.Where(l => l.Deleted == null || l.Deleted == false).OrderBy(l => l.Order_Index))
            {
                _targetLinks.Add(new ActivityLink(link));
            }
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Guid ProcessId { get; private set; }
        public int Operation { get; private set; }

        public List<ActivityLink> TargetLinks
        {
            get { return _targetLinks; }
        }

        protected virtual Guid? GetNextActivityId(WorkflowContext context)
        {
            if (TargetLinks.Count == 1 && !TargetLinks[0].HasCondition())
                return TargetLinks[0].TargetId;

            ActivityLink defaultLink = null;

            foreach (var link in TargetLinks)
            {
                if (link.HasCondition())
                {
                    if (link.CheckCondition(context)) return link.TargetId;
                }
                else
                    defaultLink = link;
            }

            if (defaultLink != null) return defaultLink.TargetId;
            
            return null;
        }

        public virtual void BeforeExecution(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
        }

        public virtual void Execute(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            try
            {
                Guid? id = GetNextActivityId(context);

                if (id != null) context.RunActivity((Guid) id);
                else
                    context.ThrowException("Unexpected end of process", "Неожиданное завершение процесса");
            }
            catch (Exception e)
            {
                context.ThrowException(e);
            }
        }

        public virtual void AfterExecution(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            context.ThrowException("NotImplementedException", "Вызов нереализован");
        }

        public virtual void HandleException(WorkflowContext context, string exceptionName, string message)
        {
            context.ThrowException(exceptionName, message);
        }

        protected void OnException(WorkflowContext context, Exception e)
        {
            try
            {
                using (var writer = new StreamWriter(Logger.GetLogFileName("WorkflowActivity"), true))
                {
                    var userInfo = context.GetUserInfo();

                    writer.WriteLine("{0}: \"{1}\", \"{2}\"; {3}: '{4}'; \"{5}\"\n   message: \"{6}\"", DateTime.Now,
                        userInfo.UserName, userInfo.OrganizationName, GetType().Name, Id, context.ProcessName, e.Message);
                    if (e.InnerException != null)
                        writer.WriteLine("  - inner exception: \"{0}\"", e.InnerException.Message);
                    writer.WriteLine("  -- Stack: {0}", e.StackTrace);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}