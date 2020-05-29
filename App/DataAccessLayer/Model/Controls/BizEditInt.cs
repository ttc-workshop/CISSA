using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizEditInt : BizEdit
    {
//        [DataMember]
//        [Obsolete("Неиспользуемое свойство")]
//        public IntAttribute Attribute { get; set; }

        [DataMember]
        public int? Value { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? int.Parse(value.ToString()) : (int?) null; }
        }
    }
}