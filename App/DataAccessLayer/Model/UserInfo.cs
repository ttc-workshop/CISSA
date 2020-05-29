using System;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Organizations;

namespace Intersoft.CISSA.DataAccessLayer.Model
{
    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string MiddleName { get; set; }

        [DataMember]
        public Guid? PositionId { get; set; }
        [DataMember]
        public string PositionName { get; set; }

        [DataMember]
        public OrgPositionInfo Position { get; set; }

        [DataMember]
        public Guid? OrgUnitTypeId { get; set; }
        [DataMember]
        public string OrgUnitTypeName { get; set; }

        [DataMember]
        public Guid? OrganizationId { get; set; }
        [DataMember]
        public Guid? OrganizationTypeId { get; set; }
        [DataMember]
        public string OrganizationName { get; set; }
        [DataMember]
        public string OrganizationCode { get; set; }

        [DataMember]
        public OrgInfo Organization { get; set; }

        [DataMember]
        public int LanguageId { get; set; }
    }
}