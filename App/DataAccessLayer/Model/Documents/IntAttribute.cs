using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class IntAttribute : AttributeBase
    {
        public IntAttribute() {}

        public IntAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember] 
        public int? Value { get; set; }

        [DataMember]
        public int MinValue { get; set; }

        [DataMember]
        public int MaxValue { get; set; }

        [XmlIgnore()]
        public override object ObjectValue
        {
            get { return Value /* ?? 0*/; } // Если null должен возвращать NULL!!!
            set { Value = value != null ? int.Parse(value.ToString()) : (int?)null; }
        }
    }
}