using System.Runtime.Serialization;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    public class ReportConditionRightAttributeDef : ReportConditionRightPartDef
    {
        [DataMember]
        public ReportAttributeDef Attribute { get; set; }
    }
}