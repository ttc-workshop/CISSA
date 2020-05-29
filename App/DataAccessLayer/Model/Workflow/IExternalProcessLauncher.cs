namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public interface IExternalProcessLauncher
    {
        ExternalProcessExecuteResult Launch(string serviceUrl, string userName, string password, string processName, WorkflowContextData contextData);
        ExternalProcessExecuteResult Launch(WorkflowGateRef gateRef, string processName, WorkflowContextData contextData);
    }
}