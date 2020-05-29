using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    public class QueryAttributeRef
    {
        [DataMember]
        public QuerySourceDef Source { get; set; }

        [DataMember]
        public Guid AttributeId { get; set; }
        
        [DataMember]
        public string AttributeName { get; set; }
    }
}