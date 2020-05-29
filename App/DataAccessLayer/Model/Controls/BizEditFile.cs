using System;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizEditFile : BizEdit
    {
        [DataMember]
        public byte[] Value { get; set; } // File Data

        [DataMember]
        public Guid DocumentId { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public bool Empty { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = ByteArrayHelper.ConvertFrom(value); }
        }
    }
}