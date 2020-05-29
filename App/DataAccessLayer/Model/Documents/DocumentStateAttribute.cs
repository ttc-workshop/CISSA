using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class DocumentStateAttribute: AttributeBase
    {
        public DocumentStateAttribute() {}

        public DocumentStateAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember]
        public Guid? Value { get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        public override object ObjectValue
        {
            get { return Value; }
            set
            {
                Value = value != null ? Guid.Parse(value.ToString()) : (Guid?)null;
            }
        }
    }
}
