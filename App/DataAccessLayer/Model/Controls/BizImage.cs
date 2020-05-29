using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizImage : BizControl
    {
        [DataMember]
        public byte[] ImageBytes { get; set; }

        [DataMember]
        public Location Path { get; set; }

        [DataMember]
        public string LocalPath { get; set; }

        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public int Width { get; set; }

        public enum Location:byte
        {
            Local = 0,
            DataBase
        }
    }
}