using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizDocumentListForm : BizDataControl
    {
        [DataMember]
        public Guid? FormId { get; set; }

        [DataMember]
        public Guid? FormAttributeDefId { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public BizTableForm TableForm { get; set; }

        /*[DataMember]
        public DocListAttribute Attribute { get; set; }*/

        [DataMember]
        public Guid? DocDefId { get; set; }

        [DataMember]
        public Guid? DocumentId { get; set; }

        [DataMember]
        public List<Guid> DocList { get; set; }

        public override object ObjectValue
        {
            get { return null; }
            set { var val = value; }
        }
    }
}