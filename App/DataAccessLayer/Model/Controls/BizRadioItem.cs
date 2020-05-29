using System;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizRadioItem : BizDataControl
    {
        [DataMember]
        public Guid? EnumId { get; set; }

        [DataMember]
        public bool? Value { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? (bool) value : (bool?) null; }
        }
    }
}