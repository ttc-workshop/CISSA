using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    public class MetaInfoAttribute : AttributeBase
    {
        public MetaInfoAttribute() {}

        public MetaInfoAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember]
        public object Value { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}
