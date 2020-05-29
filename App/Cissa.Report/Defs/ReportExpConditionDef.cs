using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    public class ReportExpConditionDef : ReportConditionItemDef
    {
        [DataMember]
        public List<ReportConditionItemDef> Conditions { get; set; }
    }
}