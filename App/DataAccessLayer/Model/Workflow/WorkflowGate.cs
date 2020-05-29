using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    [DataContract]
    public class WorkflowGate
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid ProcessId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}