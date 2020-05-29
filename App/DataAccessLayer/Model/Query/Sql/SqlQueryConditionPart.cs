using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryConditionPart
    {
        private readonly List<SqlQuerySourceAttributeRef> _attributes = new List<SqlQuerySourceAttributeRef>();
        public List<SqlQuerySourceAttributeRef> Attributes { get { return _attributes; } }
        public string Expression { get; set; }

        public QueryConditionValueDef[] Params { get; set; }

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
                        Params[i] = new QueryConditionValueDef {Name = null, Value = value[i]};
                }
                else
                {
                    Params = null;
                }
            }
        }
        //public object[] Values { get; set; }
        public object Value { get { return Params[0].Value; /*Values[0]*/; } }
        public QueryConditionValueDef Param { get { return Params[0]; } }
        public SqlQuery SubQuery { get; set; }
        public SqlQueryAttribute SubQueryAttribute { get; set; }

        public bool IsAttribute { get { return Attributes.Count > 0; } }
        public bool IsSubQuery { get { return Attributes.Count == 0 && SubQuery != null && SubQueryAttribute != null; } }
        public bool IsValue { get { return Attributes.Count == 0 && SubQuery == null && SubQueryAttribute == null && /*Values*/Params != null; } }

        public SqlQuerySourceAttribute Attribute
        {
            get { return Attributes.Count > 0 ? Attributes[0].Attribute : null; }
        }
    }
}