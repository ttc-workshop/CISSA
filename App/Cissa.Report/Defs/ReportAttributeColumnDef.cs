using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    public class ReportAttributeColumnDef : ReportColumnDef
    {
        [DataMember]
        public ReportAttributeDef Attribute { get; set; }

        [DataMember]
        public SortType SortType { get; set; }

        [DataMember]
        public ReportColumnGroupingType Grouping { get; set; }

//        [DataMember]
//        public SqlQuerySummaryFunction Summary { get; set; }
    }
}