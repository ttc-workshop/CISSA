using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class DocState
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public DocStateType Type { get; set; }
        [DataMember]
        public Guid WorkerId { get; set; }

        [DataMember]
        public DateTime? Created { get; set; }

        [DataMember]
        public string WorkerName { get; set; }
    }
}
