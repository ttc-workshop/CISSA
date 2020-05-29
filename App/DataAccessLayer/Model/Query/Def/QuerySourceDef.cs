using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    public class QuerySourceDef: QueryItemDef
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public Guid DocDefId { get; set; }

        [DataMember]
        public string DocDefName { get; set; }

        [DataMember]
        public QueryDef SubQuery { get; set; }
    }
}
