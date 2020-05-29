using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    public class ReportSourceDef : ReportItemDef
    {
        [DataMember]
        public DocDef DocDef { get; set; }

        [DataMember]
        public string Caption { get; set; }

        [DataMember]
        public List<ReportSourceSystemAttributeDef> Attributes { get; set; }
    }
}