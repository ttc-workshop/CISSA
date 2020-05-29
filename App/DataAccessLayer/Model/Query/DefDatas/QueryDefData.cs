using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas
{
    public class QueryDefData : QueryItemDefData
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public Guid? DocumentId { get; set; }

        [DataMember]
        [XmlIgnore]
        public DocDef DocDef { get; set; }
    }
}