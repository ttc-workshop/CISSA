using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class BlobAttribute : AttributeBase
    {
        public BlobAttribute() {}

        public BlobAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember]
        public byte[] Value { get; set; }

        [DataMember]
        public string FileName { get; set; }
        
        [DataMember]
        public string FileExtention { get; set; }

        [DataMember]
        public bool HasValue { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = (byte[]) value; }
        }
    }
}