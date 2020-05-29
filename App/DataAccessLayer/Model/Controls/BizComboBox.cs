using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizComboBox : BizDataControl
    {
        /*[DataMember]
        public Guid? AttributeDefId { get; set; }*/

//        [DataMember]
//        [Obsolete("Неиспользуемое свойство")]
//        public EnumAttribute Attribute { get; set; }

        [DataMember]
        public bool IsRadio { get; set; }

        [DataMember]
        public byte Rows { get; set; }

        [DataMember]
        public Guid? Value { get; set; }

        [DataMember]
        [XmlIgnore]
        public List<EnumValue> Items { get; set; }

        [DataMember]
        public SystemIdent Ident { get; set; }

        [DataMember]
        public Guid? DetailAttributeId { get; set; }

        [DataMember]
        public string DetailAttributeName { get; set; }

        [DataMember]
        [XmlIgnore]
        public string DetailText { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? Guid.Parse(value.ToString()) : (Guid?)null; }
        }
    }
}