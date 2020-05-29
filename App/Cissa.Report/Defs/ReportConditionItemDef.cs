using System.Runtime.Serialization;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Query;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    [KnownType(typeof(ReportConditionDef))]
    [KnownType(typeof(ReportExpConditionDef))]

    [XmlInclude(typeof(ReportConditionDef))]
    [XmlInclude(typeof(ReportExpConditionDef))]
    public class ReportConditionItemDef : ReportItemDef
    {
        [DataMember]
        public ExpressionOperation Operation { get; set; }
    }
}