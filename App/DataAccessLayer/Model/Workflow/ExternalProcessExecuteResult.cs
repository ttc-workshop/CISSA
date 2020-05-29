using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using NPOI.HSSF.Util;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public enum ExternalProcessExecuteResultType
    {
        Success = 1,
        Failed = 2,
        GateNotFound,
        ProcessNotFound
    }

    [DataContract]
    public class ExternalProcessExecuteResult
    {
        [DataMember]
        public ExternalProcessExecuteResultType Type { get; set; }

        [DataMember]
        public WorkflowContextData Data { get; set; }

        [DataMember]
        public WorkflowContextData ReturnData { get; set; }

        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class ProcessProgressInfo
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Format { get; set; }

        [DataMember]
        public double Value { get; set; }

        [DataMember]
        public double MaxValue { get; set; }
        
    }

    [DataContract]
    public class ExceptionInfo
    {
        [DataMember]
        public string ExceptionName { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public ExceptionInfo InnerException { get; set; }

        [DataMember]
        public string StackTrace { get; set; }
    }

    [DataContract]
    public class WorkflowProcessExecutionTaskState
    {
        [DataMember]
        public int State { get; set; }

        [DataMember]
        public ProcessProgressInfo Progress { get; set; }

        [DataMember]
        public ExceptionInfo ExceptionInfo { get; set; }

        [DataMember]
        public Guid ProcessId { get; set; }

        [DataMember]
        public string ProcessName { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public Guid UserId { get; set; }
    }

    [DataContract]
    public class WorkflowProcessExecutionTaskResult
    {
        [DataMember]
        public int State { get; set; }

        [DataMember]
        public ExceptionInfo ExceptionInfo { get; set; }
    }

    public class WorkflowProcessExecutionTask
    {
        public const int RUNNING = 0;
        public const int SUCCESS = 1;
        public const int FAIL = -1;

        public Guid Id { get; set; }

        public DateTime Created { get; set; }

        public Guid UserId { get; set; }

        public Guid ProcessId { get; set; }

        public string ProcessName { get; set; }

        public int State { get; set; }

        public WorkflowContextData ContextData { get; set; }

        public Task Task { get; private set; }

        [DataMember]
        public ExceptionInfo ExceptionInfo { get; set; }

        public WorkflowProcessExecutionTask(Guid taskId, WorkflowContextData contextData, Action action)
        {
            Id = taskId;
            State = RUNNING;
            ContextData = contextData;
            Task = new Task(action);
            Created = DateTime.Now;
            UserId = contextData != null ? contextData.UserId : Guid.Empty;
            ProcessId = contextData != null ? contextData.ProcessId ?? Guid.Empty : Guid.Empty;
            ProcessName = contextData != null ? contextData.ProcessName : string.Empty;
        }

        public void Start()
        {
            Task.Start();
        }
    }
}
