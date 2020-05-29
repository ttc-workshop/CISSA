namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public enum WorkflowDocumentOperation
    {
        CreateNew = 1,
        SaveCurrent = 2,
        CreateNewById = 3,
        DeleteCurrent = 4,
        DeleteById = 5,
        LoadById = 6,
        DefineDocDefId = 7,
        FindDoc = 8
    }
}