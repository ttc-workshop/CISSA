using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.BizService.Interfaces
{
    /// <summary>
    /// Интерфейс выполнения бизнес-процессов КИССП
    /// </summary>
    [ServiceContract]
    interface IWorkflowManager
    {
        /// <summary>
        /// Запуск бизнес-процесса
        /// </summary>
        /// <param name="processId">Идентификатор бизнес-процесса</param>
        /// <param name="processParameters">Входные параметры для бизнес-процесса</param>
        /// <returns>Контекст исполнения бизнес-процесса</returns>
        [OperationContract]
        WorkflowContextData WorkflowExecute(Guid processId, Dictionary<String, object> processParameters);

        /// <summary>
        /// Продолжение выполнения бизнес-процесса с учетом пользовательского действия
        /// </summary>
        /// <param name="contextData">Контекст выполнения бизнес-процесса</param>
        /// <param name="userAction">Идентификатор пользовательского действия</param>
        /// <returns>Контекст выполнения бизнес-процесса</returns>
        [OperationContract]
        WorkflowContextData WorkflowContinueWithUserAction(WorkflowContextData contextData, Guid userAction);

        /// <summary>
        /// Продолжение выполнения бизнес-процесса с учетом документа
        /// </summary>
        /// <param name="contextData">Контекст выполнения бизнес-процесса</param>
        /// <param name="docId">Идентификатор документа</param>
        /// <returns>Контекст выполнения бизнес-процесса</returns>
        [OperationContract]
        WorkflowContextData WorkflowContinueWithDocumentId(WorkflowContextData contextData, Guid docId);

        /// <summary>
        /// Продолжение выполнения бизнес-процесса с учетом документа
        /// </summary>
        /// <param name="contextData">Контекст выполнения бизнес-процесса</param>
        /// <param name="document">Документ</param>
        /// <returns>Контекст выполнения бизнес-процесса</returns>
        [OperationContract]
        WorkflowContextData WorkflowContinueWithDocument(WorkflowContextData contextData, Doc document);

        /// <summary>
        /// Продолжение выполнения бизнес-процесса
        /// </summary>
        /// <param name="contextData">Контекст выполнения бизнес-процесса</param>
        /// <returns>Контекст выполнения бизнес-процесса</returns>
        [OperationContract]
        WorkflowContextData WorkflowContinue(WorkflowContextData contextData);

        ///  <summary>
        /// Продолжение выполнения процесса после операции загрузки файла
        ///  </summary>
        ///  <param name="contextData">Контекст выполнения процесса</param>
        ///  <param name="uploadData">Массив данных загружаемого файла</param>
        /// <param name="fileName">Имя файла</param>
        /// <returns>Контекст процесса</returns>
        [OperationContract]
        WorkflowContextData WorkflowContinueWithUploadFile(WorkflowContextData contextData, byte[] uploadData, string fileName);

        /// <summary>
        /// Вызов процесса по имени внешнего канала
        /// </summary>
        /// <param name="gateName">Имя канала</param>
        /// <param name="contextData">Данные контекста</param>
        /// <returns>Контекст процесса</returns>
        [OperationContract]
        ExternalProcessExecuteResult WorkflowGateExecute(string gateName, WorkflowContextData contextData);
    }

    [ServiceContract]
    public interface IAsyncWorkflowManager
    {
        [OperationContract]
        Guid ExecuteProcess(Guid processId, Dictionary<String, object> processParameters);

        [OperationContract]
        Guid ContinueProcess(WorkflowContextData contextData);

        [OperationContract]
        Guid ContinueProcessWithUserAction(WorkflowContextData contextData, Guid userAction);

        [OperationContract]
        Guid ContinueProcessWithDocumentId(WorkflowContextData contextData, Guid docId);

        [OperationContract]
        Guid ContinueProcessWithDocument(WorkflowContextData contextData, Doc document);

        [OperationContract]
        Guid ContinueProcessWithUploadFile(WorkflowContextData contextData, byte[] uploadData, string fileName);

        [OperationContract]
        Guid ExecuteGate(string gateName, WorkflowContextData contextData);

        [OperationContract]
        WorkflowProcessExecutionTaskState GetProcessTaskState(Guid taskId);

        [OperationContract]
        WorkflowContextData EndProcessTask(Guid taskId);

        [OperationContract]
        void TerminateProcessTask(Guid taskId);

        [OperationContract]
        List<WorkflowProcessExecutionTaskState> GetProcessTaskStates();
    }

}
