using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    [KnownType(typeof(ReportSourceDef))]
    [KnownType(typeof(ReportAttributeDef))]
    [KnownType(typeof(ReportColumnDef))]
    [KnownType(typeof(ReportConditionItemDef))]
    [KnownType(typeof(ReportFieldDef))]
    [KnownType(typeof(ReportSourceJoinDef))]
    [KnownType(typeof(ReportSourceSystemAttributeDef))]

    [XmlInclude(typeof(ReportSourceDef))]
    [XmlInclude(typeof(ReportAttributeDef))]
    [XmlInclude(typeof(ReportColumnDef))]
    [XmlInclude(typeof(ReportConditionItemDef))]
    [XmlInclude(typeof(ReportFieldDef))]
    [XmlInclude(typeof(ReportSourceJoinDef))]
    [XmlInclude(typeof(ReportSourceSystemAttributeDef))]
    public class ReportItemDef
    {
        [DataMember]
        public Guid Id { get; set; }
    }
}