using System;
using System.Runtime.Serialization;

namespace Intersoft.Cissa.Report.Defs
{
    public class ReportAttributeDef : ReportItemDef
    {
        [DataMember]
        public Guid SourceId { get; set; } // Id of ReportSourceDef

        [DataMember]
        public Guid AttributeId { get; set; }  // Id of AttrDef

        /*[DataMember]
        public SystemIdent Ident { get; set; }*/
    }
}