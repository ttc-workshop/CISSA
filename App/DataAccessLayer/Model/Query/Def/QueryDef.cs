using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    public class QueryDef: QueryItemDef
    {
        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public Guid DocumentId { get; set; }

        [DataMember]
        public Guid OwnerDocDefId { get; set; }
        [DataMember]
        public Guid ListAttrId { get; set; }
        [DataMember]
        public string ListAttrName { get; set; }

        [DataMember]
        public QuerySourceDef Source { get; set; }

        [DataMember]
        public List<QueryAttributeDef> Attributes { get; set; }

        [DataMember]
        public List<QueryAttributeDef> SelectAttributes { get; set; }

        [DataMember]
        public List<QueryOrderDef> OrderAttributes { get; set; }

        [DataMember]
        public List<QuerySourceDef> Sources { get; set; }

        [DataMember]
        public List<QueryJoinDef> Joins { get; set; }

        [DataMember]
        public List<QueryConditionDef> WhereConditions { get; set; }

        [DataMember]
        public List<QueryGroupDef> GroupAttributes { get; set; }

        [DataMember]
        public List<QueryConditionDef> HavingConditions { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int PageNo { get; set; }

        public QueryDef()
        {
            Attributes = new List<QueryAttributeDef>();
            Sources = new List<QuerySourceDef>();
            Joins = new List<QueryJoinDef>();
            WhereConditions = new List<QueryConditionDef>();
            GroupAttributes = new List<QueryGroupDef>();
            HavingConditions = new List<QueryConditionDef>();
            OrderAttributes = new List<QueryOrderDef>();
        }

        [XmlIgnore]
        public Guid DocDefId { get { return Source != null ? Source.DocDefId : Guid.Empty; } }

        [XmlIgnore]
        public string DocDefName { get { return Source != null ? Source.DocDefName : String.Empty; } }

        [XmlIgnore]
        public string Alias { get { return Source != null ? Source.Alias : String.Empty; } }
    }
}
