using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizEditBool: BizEdit
    {
        [DataMember]
        [Obsolete("Неиспользуемое свойство")]
        public BoolAttribute Attribute { get; set; }

        [DataMember]
        public bool? Value { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? bool.Parse(value.ToString()) : (bool?)null; }
        }
    }
}