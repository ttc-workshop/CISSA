using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class OrganizationAttribute : AttributeBase
    {
        public OrganizationAttribute() {}

        public OrganizationAttribute(AttrDef attrDef)
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
