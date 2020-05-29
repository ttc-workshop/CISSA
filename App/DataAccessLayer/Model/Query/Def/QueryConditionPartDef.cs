using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    public class QueryConditionPartDef
    {
        [DataMember]
        public QueryAttributeDef Attribute { get; set; }

        [DataMember]
        public SubQueryDef SubQuery { get; set; }

        [DataMember]
        public QueryConditionValueDef[] Params { get; set; }

        //[DataMember]
        [XmlIgnore]
        public object[] Values
        {
            get { return Params != null ? Params.Select(p => p.Value).ToArray() : null; }
            set
            {
                if (value != null)
                {
                    var count = value.Length;
                    Params = new QueryConditionValueDef[count];
                    for (var i = 0; i < count; i++)
                    {
                        Params[i] = new QueryConditionValueDef {Name = null, Value = value[i]};
                    }
                }
                else
                {
                    Params = null;
                }
            }
        }

        [XmlIgnore]
        public object Value { get { return Params != null && Params.Length > 0 ? Params[0].Value : null; } }

        [XmlIgnore]
        public object Value2 { get { return Params != null && Params.Length > 1 ? Params[1].Value : null; } }
    }

    [DataContract]
    public class QueryConditionValueDef
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public object Value { get; set; }
    }
}