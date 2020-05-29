using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    [KnownType(typeof(QuerySubAttributeDef))]
    [KnownType(typeof(QueryExpAttributeDef))]
    [KnownType(typeof(QuerySingleAttributeDef))]

    [XmlInclude(typeof(QuerySubAttributeDef))]
    [XmlInclude(typeof(QueryExpAttributeDef))]
    [XmlInclude(typeof(QuerySingleAttributeDef))]
    public class QueryAttributeDef : QueryItemDef
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public bool Invisible { get; set; }
    }
    
    [DataContract]
    public class QuerySubAttributeDef: QueryAttributeDef
    {
        [DataMember]
        public SubQueryDef SubQuery { get; set; }

        [DataMember]
        public QueryAttributeDef SubQueryAttribute { get; set; }
    }

    [DataContract]
    public class QueryExpAttributeDef: QueryAttributeDef
    {
        [DataMember]
        public List<QueryAttributeRef> Attributes { get; set; }

        [DataMember]
        public string Expression { get; set; }
    }

    [DataContract]
    public class QuerySingleAttributeDef: QueryAttributeDef
    {
        [DataMember]
        public QueryAttributeRef Attribute { get; set; }
    }
}
