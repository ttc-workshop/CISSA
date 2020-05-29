using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizDocumentControl : BizDataControl
    {
        [DataMember]
        public Guid? FormId { get; set; }

        /*[DataMember]
        public Guid? AttributeDefId { get; set; }*/

        [DataMember]
        public BizForm DocForm { get; set; }

        [DataMember]
        public Guid? Value { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set
            {
                Value = value != null ? Guid.Parse(value.ToString()) : (Guid?)null;
                if (Value == null) DocForm = null;
            }
        }
    }
}
