using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class CurrencyAttribute : AttributeBase
    {
        public CurrencyAttribute() {}

        public CurrencyAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember]
        public decimal? Value { get; set; }

        [XmlIgnore()]
        public override object ObjectValue
        {
            get { return Value /*?? 0m*/; }
            set { Value = value != null ? decimal.Parse(value.ToString()) : (decimal?)null; }
        }
    }
}