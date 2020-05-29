using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    [DataContract]
    [KnownType(typeof(Doc))]
    [KnownType(typeof(AttributeBase))]
    [KnownType(typeof(CurrencyAttribute))]
    [KnownType(typeof(DocAttribute))]
    [KnownType(typeof(DocListAttribute))]
    [KnownType(typeof(EnumAttribute))]
    [KnownType(typeof(FloatAttribute))]
    [KnownType(typeof(IntAttribute))]
    [KnownType(typeof(TextAttribute))]
    [KnownType(typeof(OrganizationAttribute))]
    [KnownType(typeof(DateTimeAttribute))]
    [KnownType(typeof(BoolAttribute))]
    [KnownType(typeof(DocumentStateAttribute))]
    [KnownType(typeof(WorkflowVariable))]
    [KnownType(typeof(QueryDef))]
    public class WorkflowContextData : IStringParams
    {
        [DataMember]
        public WorkflowContextData Parent { get; set; }

        [DataMember]
        public Guid ParentProcessId { get; set; }

        [DataMember]
        public Guid? ProcessId { get; set; }

        [DataMember]
        public Guid? GateId { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public string ProcessName { get; set; }

        public WorkflowContextData(Guid processId, Guid userId)
        {
            ParentProcessId = processId;
            UserId = userId;

            State = WorkflowRuntimeState.Finish;
        }

        public WorkflowContextData()
        {
            ParentProcessId = Guid.Empty;

            State = WorkflowRuntimeState.Finish;
        }

        public WorkflowContextData(WorkflowProcess process, Guid userId)
        {
            ParentProcessId = process.Id;
            UserId = userId;
            ProcessName = process.Name;

            State = WorkflowRuntimeState.Finish;
        }

        public WorkflowContextData(WorkflowProcess process, WorkflowContextData parent)
            : this(process, parent.UserId)
        {
            Parent = parent;
        }

        public WorkflowContextData(WorkflowProcess process, WorkflowContextData parent, Guid userId)
            : this(process, userId)
        {
            Parent = parent;
        }

        private List<WorkflowVariable> _variables = new List<WorkflowVariable>();

        [DataMember]
        public List<WorkflowVariable> Variables
        {
            get { return _variables ?? (_variables = new List<WorkflowVariable>()); } set { _variables = value; }
        }

        [DataMember]
        public WorkflowRuntimeState State { get; set; }

        [DataMember]
        public Guid ActivityId { get; set; }

        [DataMember]
        public string GateProcessName { get; set; }

        [DataMember]
        public List<UserAction> UserActions { get; set; }

        [DataMember]
        public Guid? CurrentFormId { get; set; }

        [DataMember]
        public string FormCaption { get; set; }

        [DataMember]
        public Guid ReportId { get; set; }
        
        [DataMember]
        public string ExceptionName { get; set; }

        [DataMember]
        public WorkflowContextData ReturnedContextData { get; set; }

        [DataMember]
        public bool ReturnedSuccessFlag { get; set; }

        [DataMember]
        public bool ReturnedExceptionFlag { get; set; }

        [DataMember]
        public bool SuccessFlag { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public Doc CurrentDocument { get; set; }

        [DataMember]
        public Guid? CurrentDocumentId { get; set; }

        [DataMember]
        public Guid? CurrentDocumentDefId { get; set; }

        [DataMember]
        public Guid? UserActionId { get; set; }

        [DataMember]
        public Doc SelectedDocument { get; set; }

        [DataMember]
        public Guid? SelectedDocumentId { get; set; }

        [DataMember]
        public Doc FilterDocument { get; set; }

        [DataMember]
        public List<Guid> DocumentList { get; set; }

        [DataMember]
        public string TemplateFileName { get; set; }

        [DataMember]
        public QueryDef CurrentQuery { get; set; }

        [DataMember]
        public List<BizControl> ControlDatas { get; set; }

        [DataMember]
        public Guid? FilterDocStateId { get; set; }

        [DataMember]
        public string LogFileName { get; set; }

        [DataMember]
        public List<ModelMessage> ErrorMessages { get; set; }

        [DataMember]
        public List<BizFormOptions> FormOptions { get; set; }

        [DataMember]
        public byte[] UploadFileData { get; set; }

        [DataMember]
        public string UploadFileName { get; set; }

        [DataMember]
        public List<FileData> DownloadFiles { get; set; }

        [DataMember]
        public List<BlobData> BlobDatas { get; set; }

        [DataMember]
        public ExternalProcessReturnData ReturnData { get; set; }

        [DataMember]
        public bool ReturnException { get; set; }

        [DataMember]
        public bool PreviewForm { get; set; }

        [DataMember]
        public Guid PreviewFormId { get; set; }

        public string Get(string name)
        {
            var data = GetVariable(name);

            return data != null ? data.ToString() : "";
        }

        private static object GetVariableFrom(WorkflowContextData data, string name)
        {
            if (data == null || data.Variables == null) return null;

            var variable = data.Variables.Find(v => String.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));

            var objectVariable = variable as ObjectVariable;
            if (objectVariable != null) return objectVariable.Value;
            var documentVariable = variable as DocumentVariable;
            if (documentVariable != null) return documentVariable.Value;
            var attributeVariable = variable as AttributeVariable;
            if (attributeVariable != null) return attributeVariable.Value;
            var valueVariable = variable as EnumValueVariable;
            if (valueVariable != null) return valueVariable.Value;
            var listVariable = variable as ObjectListVariable;
            if (listVariable != null) return listVariable.Value;
            var docListVariable = variable as DocListVariable;
            if (docListVariable != null) return docListVariable.Value;

            return null;
        }

        private object GetVariable(string name)
        {
            var value = GetVariableFrom(this, name);

            if (value != null) return value;

            var parent = Parent;
            while (parent != null)
            {
                value = GetVariableFrom(parent, name);
                if (value != null) return value;
                parent = parent.Parent;
            }

            return null;
        }
    }

    [DataContract]
    public class ExternalProcessReturnData
    {
        [DataMember]
        public ExternalProcessExecuteResultType ResultType { get; set; }

        [DataMember]
        public bool WithException { get; set; }

        [DataMember]
        public string ExceptionMessage { get; set; }

        [DataMember]
        public bool Correct { get; set; }
    }
}