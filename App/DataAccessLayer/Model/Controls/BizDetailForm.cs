using System;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizDetailForm : BizForm
    {
        [DataMember]
        public short LayoutId { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public Guid? OrganizationId { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public string OrganizationName { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public Guid? PositionId { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public string PositionName { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public Guid UserId { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public string UserFullName { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public DateTime Created { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public DocState DocumentState { get; set; }

        [DataMember]
        public bool AllowEdit { get; set; }

        [DataMember]
        public bool AllowDelete { get; set; }
    }
}
