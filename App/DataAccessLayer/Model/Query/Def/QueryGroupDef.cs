using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    public class QueryGroupDef: QueryItemDef
    {
        [DataMember]
        public QueryAttributeDef Attribute { get; set; }
/*
        [DataMember]
        public QuerySourceDef Source { get; set; }

        [DataMember]
        public string AttributeName { get; set; }

        [DataMember]
        public Guid AttributeId { get; set; }
*/
    }

    public enum QueryGroupmentOperation
    {
        None,
        Count,
        Sum,
        Avg,
        Max,
        Min,
        Group
    }
}