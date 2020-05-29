using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas
{
    [DataContract]
    public class QuerySourceDefData : QueryItemDefData
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public Guid? DocumentId { get; set; }

        [DataMember]
        public Guid? QueryId { get; set; }

        [DataMember]
        public SqlSourceJoinType JoinType { get; set; }

        [DataMember]
        [XmlIgnore]
        public DocDef DocDef { get; set; }

        [DataMember]
        [XmlIgnore]
        public QueryDefData Query { get; set; }
    }
}