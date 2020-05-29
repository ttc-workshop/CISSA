using System;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    public class ReportSourceJoinDef : ReportItemDef
    {
        [DataMember]
        public Guid MasterId { get; set; }  // Id of ReportSourceDef

        [DataMember]
        public SqlSourceJoinType JoinType { get; set; }

        [DataMember]
        public Guid SourceId { get; set; }  // Id of ReportSourceDef

        [DataMember]
        public ReportAttributeDef JoinAttribute { get; set; }
    }
}