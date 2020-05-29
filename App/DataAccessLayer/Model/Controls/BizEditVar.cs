using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizEditVar : BizEdit
    {
        [DataMember]
        public object Value { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}