using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class DocAttribute : AttributeBase
    {
        public DocAttribute() {}

        public DocAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember]
        public Guid? Value { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public Doc Document { get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        public override object ObjectValue
        {
            get { return Value; }
            set
            {
                Value = value != null ? Guid.Parse(value.ToString()) : (Guid?)null;

                if (Value == null || (Document != null && Document.Id != (Value ?? Guid.Empty)))
                    Document = null;
            }
        }
    }
}