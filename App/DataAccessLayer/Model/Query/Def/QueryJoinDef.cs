using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    public class QueryJoinDef: QueryItemDef
    {
        [DataMember]
        public SqlSourceJoinType Operation { get; set; }

        [DataMember]
        public QuerySourceDef Source { get; set; }

        [DataMember]
        public List<QueryConditionDef> Conditions { get; private set; }

        public QueryJoinDef()
        {
            Conditions = new List<QueryConditionDef>();
        }
    }

    /*[DataContract]
    public class QueryJoinConditionDef
    {
        [DataMember]
        public ExpressionOperation Operation { get; set; }

        [DataMember]
        public ConditionOperation Condition { get; set; }

        [DataMember]
        public List<QueryJoinConditionDef> Conditions { get; private set; }

        [DataMember]
        public Guid MasterAttrId { get; set; }
        [DataMember]
        public string MasterAttrName { get; set; }

        [DataMember]
        public Guid SlaveAttrId { get; set; }
        [DataMember]
        public string SlaveAttrName { get; set; }
    }*/
}
