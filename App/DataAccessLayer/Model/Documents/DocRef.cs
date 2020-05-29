using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class DocRef
    {
        [DataMember]
        public Guid DocumentId { get; set; }

        [DataMember]
        public Guid DocumentDefId { get; set; }

        [DataMember]
        public Guid AttributeDefId { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public bool IsList { get; set; }
    }
}
