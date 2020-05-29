using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model;

namespace Intersoft.Cissa.Report.Defs
{
    [DataContract]
    public class ReportSourceSystemAttributeDef : ReportItemDef
    {
        [DataMember]
        public SystemIdent Ident { get; set; }

        [DataMember]
        public string Caption { get; set; }
    }
}