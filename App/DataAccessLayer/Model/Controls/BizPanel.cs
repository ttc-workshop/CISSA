using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizPanel: BizControl
    {
        [DataMember]
        [Obsolete("Устаревший атрибут")]
        public bool IsHorizontal { get; set; }

        [DataMember]
        public short LayoutId { get; set; }

        [DataMember]
        public bool ReadOnly { get; set; }
    }
}
