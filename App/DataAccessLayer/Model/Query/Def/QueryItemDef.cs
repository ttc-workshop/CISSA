using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    [KnownType(typeof(QueryDef))]
    [KnownType(typeof(QueryAttributeDef))]
    [KnownType(typeof(QueryGroupDef))]
    [KnownType(typeof(QueryConditionDef))]
    [KnownType(typeof(QueryJoinDef))]
    [KnownType(typeof(QueryOrderDef))]
    [KnownType(typeof(QuerySourceDef))]
    [KnownType(typeof(SubQueryDef))]
    public class QueryItemDef
    {
    }
}
