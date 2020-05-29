using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizEditFloat : BizEdit
    {
//        [DataMember]
//        [Obsolete("Неиспользуемое свойство")]
//        public FloatAttribute Attribute { get; set; }

        [DataMember]
        public double? Value { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? double.Parse(value.ToString()) : (double?)null; }
        }
    }
}