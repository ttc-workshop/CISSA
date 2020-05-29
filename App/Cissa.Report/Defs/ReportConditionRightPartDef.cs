using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    [KnownType(typeof(ReportConditionRightAttributeDef))]
    [KnownType(typeof(ReportConditionRightParamDef))]
    [KnownType(typeof(ReportConditionRightVariableDef))]

    [XmlInclude(typeof(ReportConditionRightAttributeDef))]
    [XmlInclude(typeof(ReportConditionRightParamDef))]
    [XmlInclude(typeof(ReportConditionRightVariableDef))]
    public class ReportConditionRightPartDef
    {
    }
}