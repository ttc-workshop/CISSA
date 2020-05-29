using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents.AutoAttr
{
    [DataContract]
    public class AutoAttribute : AttributeBase
    {
        public AutoAttribute() {}

        public AutoAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember]
        public Object Value { get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}