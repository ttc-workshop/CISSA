using System;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizEditCurrency : BizEdit
    {
//        [DataMember]
//        [Obsolete("Неиспользуемое свойство")]
//        public CurrencyAttribute Attribute { get; set; }

        [DataMember]
        public decimal? Value { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? decimal.Parse(value.ToString()) : (decimal?)null; }
        }
    }
}