using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class DocDefName
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Caption { get; set; }
    }
}