using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizEditDateTime: BizEdit
    {
//        [DataMember]
//        [Obsolete("Неиспользуемое свойство")]
//        public DateTimeAttribute Attribute { get; set; }

        [DataMember]
        public DateTime? Value { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set
            {
                Value = value != null
                            ? DateTime.Parse(value.ToString())
                            : (DateTime?) null;
            }
        }
    }
}
