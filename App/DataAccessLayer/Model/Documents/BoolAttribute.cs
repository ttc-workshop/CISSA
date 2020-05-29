using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class BoolAttribute: AttributeBase
    {
        public BoolAttribute() {}

        public BoolAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember]
        public bool? Value { get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? bool.Parse(value.ToString()) : (bool?)null; }
        }
    }
}
