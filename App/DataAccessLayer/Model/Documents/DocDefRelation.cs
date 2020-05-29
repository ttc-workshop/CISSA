using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class DocDefRelation
    {
        [DataMember]
        public Guid DocDefId { get; set; }

        [DataMember]
        public Guid AttributeId { get; set; }

        [DataMember]
        public string DocumentName { get; set; }

        [DataMember]
        public string DocumentCaption { get; set; }

        [DataMember]
        public string AttributeName { get; set; }

        [DataMember]
        public string AttributeCaption { get; set; }

        [DataMember]
        public Guid RefDocDefId { get; set; }

        [DataMember]
        public string RefDocumentName { get; set; }

        [DataMember]
        public string RefDocumentCaption { get; set; }

        [DataMember]
        public CissaDataType DataType { get; set; }
    }
}