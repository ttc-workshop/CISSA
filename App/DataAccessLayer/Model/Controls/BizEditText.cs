using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizEditText : BizEdit
    {
//        [DataMember]
//        [Obsolete("Неиспользуемое свойство")]
//        public TextAttribute Attribute { get; set; }

        [DataMember]
        public string Value { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? value.ToString() : null; }
        }
    }
}