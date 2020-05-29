using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextWorkflowRepository : IWorkflowRepository
    {
        public IMultiDataContext DataContext { get; private set; }

        private readonly IList<IWorkflowRepository> _repositories = new List<IWorkflowRepository>();

        public MultiContextWorkflowRepository(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Meta))
                    _repositories.Add(new WorkflowRepository(provider, context));
            }
        }
        public WorkflowProcess LoadProcessById(Guid processId)
        {
            return _repositories.Select(r => r.LoadProcessById(processId)).FirstOrDefault(wp => wp != null);
        }

        public WorkflowActivity LoadActivityById(Guid activityId)
        {
            return _repositories.Select(r => r.LoadActivityById(activityId)).FirstOrDefault(wa => wa != null);
        }

        public void TranslateUserActions(List<UserAction> userActions, int languageId)
        {
            _repositories.First().TranslateUserActions(userActions, languageId);
        }

        public WorkflowGate LoadGateByName(string gateName)
        {
            return _repositories.Select(r => r.LoadGateByName(gateName)).FirstOrDefault(wa => wa != null);
        }

        public WorkflowGateRef LoadGateRefById(Guid gateRefId)
        {
            return _repositories.Select(r => r.LoadGateRefById(gateRefId)).FirstOrDefault(wa => wa != null);
        }


        public IList<UserAction> GetActivityUserActions(Guid activityId, int languageId)
        {
            return _repositories.SelectMany(repo => repo.GetActivityUserActions(activityId, languageId)).ToList();
        }
    }
}