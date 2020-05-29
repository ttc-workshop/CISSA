using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Enums
{
    [DataContract]
    public class EnumDef
    {
        [DataMember]
        public Guid Id { get; set; }
        
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Caption { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public List<EnumValue> EnumItems { get; set; }
    }
}
