using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class DateTimeAttribute: AttributeBase
    {
        public DateTimeAttribute() {}

        public DateTimeAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }
        [DataMember]
        public DateTime? Value { get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? DateTime.Parse(value.ToString()) : (DateTime?)null; }
        }
    }
}
