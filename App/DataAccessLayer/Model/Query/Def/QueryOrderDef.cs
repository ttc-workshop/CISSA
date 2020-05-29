using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    public class QueryOrderDef: QueryItemDef
    {
        [DataMember]
        public QueryAttributeDef Attribute { get; set; }

        /*[DataMember]
        public QuerySourceDef Source { get; set; }

        [DataMember]
        public string AttributeName { get; set; }

        [DataMember]
        public Guid AttributeId { get; set; }*/

        [DataMember]
        public bool Asc { get; set; }
    }
}
