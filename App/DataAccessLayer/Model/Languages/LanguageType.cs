using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Languages
{
    [DataContract]
    public class LanguageType
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string CultureName { get; set; }
    }
}
