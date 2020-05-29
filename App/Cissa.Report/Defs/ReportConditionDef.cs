using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    public class ReportConditionDef : ReportConditionItemDef
    {
        [DataMember]
        public ReportAttributeDef LeftAttribute { get; set; }

        [DataMember]
        public CompareOperation Condition { get; set; }

        [DataMember]
        public ReportConditionRightPartDef RightPart { get; set; }

        /*[DataMember]
        public ReportAttributeDef RightAttribute { get; set; }

        [DataMember]
        public string Caption { get; set; }*/
    }
}