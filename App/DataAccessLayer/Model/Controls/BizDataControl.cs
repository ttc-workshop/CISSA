using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    [KnownType(typeof(BizEdit))]
    [KnownType(typeof(BizComboBox))]
    [KnownType(typeof(BizForm))]
    [KnownType(typeof(BizGrid))]
    [KnownType(typeof(BizDocumentControl))]
    [KnownType(typeof(BizDocumentListForm))]
    [KnownType(typeof(BizEditText))]
    [KnownType(typeof(BizEditCurrency))]
    [KnownType(typeof(BizEditFloat))]
    [KnownType(typeof(BizEditInt))]
    [KnownType(typeof(BizEditDateTime))]
    [KnownType(typeof(BizEditBool))]
    [KnownType(typeof(BizTableColumn))]
    [KnownType(typeof(BizDataImage))]

    [XmlInclude(typeof(BizEdit))]
    [XmlInclude(typeof(BizComboBox))]
    [XmlInclude(typeof(BizForm))]
    [XmlInclude(typeof(BizGrid))]
    [XmlInclude(typeof(BizDocumentControl))]
    [XmlInclude(typeof(BizDocumentListForm))]
    [XmlInclude(typeof(BizEditText))]
    [XmlInclude(typeof(BizEditCurrency))]
    [XmlInclude(typeof(BizEditFloat))]
    [XmlInclude(typeof(BizEditInt))]
    [XmlInclude(typeof(BizEditDateTime))]
    [XmlInclude(typeof(BizEditBool))]
    [XmlInclude(typeof(BizTableColumn))]
    [XmlInclude(typeof(BizDataImage))]
    public abstract class BizDataControl : BizControl
    {
        [DataMember]
        public Guid? AttributeDefId { get; set; }

        [DataMember]
        public bool ReadOnly { get; set; }

        [DataMember]
        public bool FormNotNull { get; set; }

        [DataMember]
        public bool DocNotNull { get; set; }

        [DataMember]
        public string AttributeName { get; set; }

        [XmlIgnore]
        public abstract object ObjectValue { get; set; }

        [DataMember]
        public bool Updatable { get; set; }
    }
}
