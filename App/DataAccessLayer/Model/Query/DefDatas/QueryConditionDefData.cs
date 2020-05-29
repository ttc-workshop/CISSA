using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas
{
    [DataContract]
    public class QueryConditionDefData : QueryItemDefData
    {
        [DataMember]
        public ExpressionOperation Expression { get; set; }

        [DataMember]
        public string LeftSourceName { get; set; }

        [DataMember]
        public Guid LeftSourceId { get; set; }

        [DataMember]
        [XmlIgnore]
        public QueryDefData LeftQuery { get; set; }

        [DataMember]
        public Guid? LeftAttributeId { get; set; }

        [DataMember]
        public string LeftAttributeName { get; set; }

        [DataMember]
        public string LeftValue { get; set; }

        [DataMember]
        public string LeftParamName { get; set; }

        [DataMember]
        public ConditionOperation Operation { get; set; }

        [DataMember]
        public string RightSourceName { get; set; }

        [DataMember]
        public Guid RightSourceId { get; set; }

        [DataMember]
        [XmlIgnore]
        public QueryDefData RightQuery { get; set; }

        [DataMember]
        public Guid? RightAttributeId { get; set; }

        [DataMember]
        public string RightAttributeName { get; set; }

        [DataMember]
        public string RightValue { get; set; }

        [DataMember]
        public string RightParamName { get; set; }
    }
}