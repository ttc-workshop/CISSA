using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class ObjectDefAttribute : AttributeBase
    {
        public ObjectDefAttribute() { }
        public ObjectDefAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember]
        public Guid? Value { get; set; }

        public override object ObjectValue
        {
            get { return Value; } 
            set
            {
                Value = (value is Guid)
                    ? (Guid) value
                    : (value != null)
                        ? Guid.Parse(value.ToString())
                        : (Guid?) null;
            }
        }
    }
}
