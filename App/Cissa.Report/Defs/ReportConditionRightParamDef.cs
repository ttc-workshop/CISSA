using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    public class ReportConditionRightParamDef : ReportConditionRightPartDef
    {
        [DataMember]
        public string Caption { get; set; }

        [DataMember]
        public object Value { get; set; }

        [DataMember]
        public List<EnumValue> Values { get; set; }
    }
}