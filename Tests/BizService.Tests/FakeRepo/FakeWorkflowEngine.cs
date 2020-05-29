using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.BizServiceTests.FakeRepo
{
    class FakeWorkflowEngine: IWorkflowEngine
    {
        public Guid UserId { get; private set; }

        public FakeWorkflowEngine(Guid userId)
        {
            UserId = userId;
        }

        /// <summary>
        /// Запускает процесс на выполенение
        /// </summary>
        /// <param name="processId">Идентификатор процесса</param>
        // <param name="userId">Идентификатор пользователя запустившего процесс</param>
        /// <param name="processParameters">Параметры переданные в процесс</param>
        /// <returns>Результат выполения процесса (промежуточный, или конечный)</returns>
        public WorkflowContextData Run(Guid processId, Dictionary<String, object> processParameters)
        {
            return new WorkflowContextData(processId, UserId);
        }

        /// <summary>
        /// Продолжение выполнения процесса
        /// </summary>
        /// <param name="contextData">Контекст процесса</param>
        /// <returns>Результат выполения процесса (промежуточный, или конечный)</returns>
        public WorkflowContextData Continue(WorkflowContextData contextData)
        {
            return new WorkflowContextData(Guid.Empty, UserId);
        }
    }
}
