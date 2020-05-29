using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class EnumAttribute : AttributeBase
    {
        public EnumAttribute() { }

        public EnumAttribute(AttrDef atrDef)
        {
            AttrDef = atrDef;
            Value = null;
        }

        [DataMember]
        public Guid? Value { get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? Guid.Parse(value.ToString()) : (Guid?)null; }
        }
    }
}