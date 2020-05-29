using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizMenu : BizControl
    {
        [DataMember]
        public Guid? FormId { get; set; }

        [DataMember]
        public Guid? DocStateId { get; set; }

        [DataMember]
        public Guid? ProcessId { get; set; }

        [DataMember]
        public bool Expanded { get; set; }
    }
}
