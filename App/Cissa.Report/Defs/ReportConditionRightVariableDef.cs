using System.Runtime.Serialization;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    public class ReportConditionRightVariableDef : ReportConditionRightPartDef
    {
        [DataMember]
        public string Caption { get; set; }

        [DataMember]
        public string SystemValue { get; set; }
    }
}