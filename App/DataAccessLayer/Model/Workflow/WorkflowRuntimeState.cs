namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public enum WorkflowRuntimeState
    {
        Run,
        Exception,
        ProcessCall,
        ProcessReturn,
        ShowForm,
        ShowSelectForm,
        ShowParamForm,
        ShowReport,
        ShowTemplateReport,
        SendFile,
        ShowMessage,
        ShowReturn,
        ShowSelectReturn,
        UploadFile,
        UploadFileReturn,
        Finish,
        GateProcessCall
    }
}