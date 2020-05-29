using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    [KnownType(typeof(ReportAttributeColumnDef))]
    [KnownType(typeof(ReportRowNoColumnDef))]
    [XmlInclude(typeof(ReportAttributeColumnDef))]
    [XmlInclude(typeof(ReportRowNoColumnDef))]
    public class ReportColumnDef : ReportItemDef
    {
        [DataMember]
        public bool Visible { get; set; }

        [DataMember]
        public string Caption { get; set; }
    }
}