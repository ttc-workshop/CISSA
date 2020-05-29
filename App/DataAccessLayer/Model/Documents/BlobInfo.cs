
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class BlobInfo
    {
        [DataMember]
        public int MaxHeight { get; set; }

        [DataMember]
        public int MaxWidth { get; set; }
        
        [DataMember]
        public int MaxSizeBytes { get; set; }

        [DataMember]
        public bool IsImage { get; set; }
    }
}
