using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Security;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas
{
    [DataContract]
    [KnownType(typeof(QuerySourceDefData))]
    [KnownType(typeof(QueryConditionDefData))]
    [KnownType(typeof(QueryDefData))]
    [XmlInclude(typeof(QuerySourceDefData))]
    [XmlInclude(typeof(QueryConditionDefData))]
    [XmlInclude(typeof(QueryDefData))]
    public class QueryItemDefData
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public List<QueryItemDefData> Items { get; set; }

        [DataMember]
        public PermissionSet Permissions { get; set; }
    }
}
