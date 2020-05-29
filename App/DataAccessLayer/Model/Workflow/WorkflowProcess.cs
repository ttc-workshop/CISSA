using System;
using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class WorkflowProcess
    {
        public WorkflowProcess(Workflow_Process process, Guid startActivityId)
        {
            Id = process.Id;
            Name = process.Name;
            StartActivityId = startActivityId;
            Script = process.Script;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public Guid StartActivityId { get; private set; }

        public string Script { get; private set; }
/*
        public void Execute(WorkflowContext context)
        {
            Guid firstId = GetFirstActivity();

            context.Engine.
        }
*/
    }
}