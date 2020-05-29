using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class DocListItem
    {
        [DataMember]
        public DateTime Created { get; set; }

        [DataMember]
        public Guid Value { get; set; }
    }
}
