using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    public class ReportDef
    {
        [DataMember]
        public List<ReportSourceDef> Sources { get; set; }

        [DataMember]
        public Guid SourceId { get; set; }

        [DataMember]
        public List<ReportSourceJoinDef> Joins { get; set; }

        [DataMember]
        public List<ReportColumnDef> Columns { get; set; }

        [DataMember]
        public List<ReportConditionItemDef> Conditions { get; set; }

        [DataMember]
        public string Caption { get; set; }
    }
}
