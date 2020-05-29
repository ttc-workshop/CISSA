using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizTableColumn : BizDataControl
    {
        [DataMember]
        public Guid? Value { get; set; }

        [XmlIgnore]
        [DataMember]
        public Doc Document { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set
            {
                Value = value != null ? Guid.Parse(value.ToString()) : (Guid?)null;
                if (Value == null) Document = null;
            }
        }
    }
}
