using System;
using System.Collections.Generic;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public interface IWorkflowEngine
    {
        /// <summary>
        /// Запускает процесс на выполенение
        /// </summary>
        /// <param name="processId">Идентификатор процесса</param>
        // <param name="userId">Идентификатор пользователя запустившего процесс</param>
        /// <param name="processParameters">Параметры переданные в процесс</param>
        /// <returns>Результат выполения процесса (промежуточный, или конечный)</returns>
        WorkflowContextData Run(Guid processId, /*Guid userId,*/ Dictionary<String, object> processParameters);

        WorkflowContextData Run(Guid processId, WorkflowContextData contextData);

        /// <summary>
        /// Продолжение выполнения процесса
        /// </summary>
        /// <param name="contextData">Контекст процесса</param>
        /// <returns>Результат выполения процесса (промежуточный, или конечный)</returns>
        WorkflowContextData Continue(WorkflowContextData contextData);

        /// <summary>
        /// Запускает удаленный процесс по имени шлюза
        /// </summary>
        /// <param name="gateName">Имя шлюза доступа к процессу</param>
        /// <param name="contextData">Контекст клиентского процесса</param>
        /// <returns>Контекст выполнения процесса</returns>
        ExternalProcessExecuteResult RunGate(string gateName, WorkflowContextData contextData);

        WorkflowContextData RunExternalProcess(WorkflowGateRef gateRef, string processName,
            WorkflowContextData contextData);
    }
}