using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class BlobData
    {
        [DataMember]
        public Guid DocumentId { get; set; }

        [DataMember]
        public Guid AttributeDefId { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public byte[] Data { get; set; }
    }
}
