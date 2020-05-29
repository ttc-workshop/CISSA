using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class FloatAttribute : AttributeBase
    {
        public FloatAttribute() {}

        public FloatAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember]
        public double? Value { get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        public override object ObjectValue
        {
            get { return Value/* ?? 0f*/; }
            set { Value = value != null ? double.Parse(value.ToString()) : (double?)null; }
        }
    }
}