using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    public class SubQueryDef: QueryItemDef
    {
        [DataMember]
        public QueryDef QueryDef { get; set; }

        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public QueryAttributeDef Attribute { get; set; }

        [DataMember]
        public List<QueryConditionDef> Conditions { get; private set; }

        public SubQueryDef()
        {
            Conditions = new List<QueryConditionDef>();
        }
    }
}