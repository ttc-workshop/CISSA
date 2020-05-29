using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Enums
{
    [DataContract]
    public class EnumValue
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string DefaultValue { get; set; }
    }
}
