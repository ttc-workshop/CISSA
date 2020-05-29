using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Organizations
{
    [DataContract]
    public class OrgInfo
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public Guid? TypeId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public OrgTypeInfo Type { get; set; }
    }

    [DataContract]
    public class OrgTypeInfo
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Guid? ParentId { get; set; }
    }

    [DataContract]
    public class OrgPositionInfo
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Guid? OrgTypeId { get; set; }
    }
}
