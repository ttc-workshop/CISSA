using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.Cissa.Report.Defs;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.Cissa.Report.Context
{
    [DataContract]
    public class TableReportContext
    {
        [DataMember]
        public ReportDef Def { get; set; }

        [DataMember]
        public List<ReportSourceRelations> SourceRelations { get; set; }
    }

    [DataContract]
    public class ReportSourceRelations
    {
        [DataMember]
        public Guid SourceId { get; set; } // Id of ReportSourceDef

        [DataMember]
        public List<DocDefRelation> Relations { get; set; }
    }

    public static class TableReportContextHelper
    {
    }
}
