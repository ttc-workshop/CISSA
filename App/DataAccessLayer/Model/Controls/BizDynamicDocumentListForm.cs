using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizDynamicDocumentListForm : BizControl
    {
        [DataMember]
        public Guid? FormId { get; set; }

        [DataMember]
        public BizTableForm TableForm { get; set; }

        [DataMember]
        public Guid? ScritpId { get; set; }

        [DataMember]
        public List<Guid> DocList { get; set; }
    }
}