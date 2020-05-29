using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    [DataContract]
    public class AttributeSort
    {
        [DataMember]
        public Guid AttributeId { get; set; }
        [DataMember]
        public bool Asc { get; set; }
    }
}
