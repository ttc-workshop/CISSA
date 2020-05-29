using System;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public interface IProcessContextState
    {
        Guid GetProcessId();
        WorkflowContextData GetWorkflowContext();
    }
}
