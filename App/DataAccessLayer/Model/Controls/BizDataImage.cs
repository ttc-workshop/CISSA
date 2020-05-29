using System;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizDataImage : BizDataControl
    {
        [DataMember]
        public byte[] Value { get; set; }

        [DataMember]
        public Guid DocumentId { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public int Width { get; set; }

        public override object ObjectValue
        {
            get { return Value; }
            set { Value = ByteArrayHelper.ConvertFrom(value); }
        }
    }
}