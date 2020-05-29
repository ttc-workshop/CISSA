using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class FileData
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public byte[] Data { get; set; }
    }
}