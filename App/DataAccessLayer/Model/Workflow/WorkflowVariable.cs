using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    [DataContract]
    [KnownType(typeof(ObjectVariable))]
    [KnownType(typeof(DocumentVariable))]
    [KnownType(typeof(AttributeVariable))]
    [KnownType(typeof(EnumValueVariable))]
    [KnownType(typeof(QueryVariable))]
    [KnownType(typeof(ObjectListVariable))]
    [KnownType(typeof(DocListVariable))]

    [XmlInclude(typeof(ObjectVariable))]
    [XmlInclude(typeof(DocumentVariable))]
    [XmlInclude(typeof(AttributeVariable))]
    [XmlInclude(typeof(EnumValueVariable))]
    [XmlInclude(typeof(QueryVariable))]
    [XmlInclude(typeof(ObjectListVariable))]
    [XmlInclude(typeof(DocListVariable))]
    public abstract class WorkflowVariable
    {
        [DataMember]
        public string Name { get; set; }

        [XmlIgnore]
        public abstract object ObjectValue { get; set; }
    }

    [DataContract]
    public class ObjectVariable : WorkflowVariable
    {
        [DataMember]
        public object Value { get; set; }

        [XmlIgnore]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value; }
        }
    }

    [DataContract]
    public class DocumentVariable : WorkflowVariable
    {
        [DataMember]
        public Doc Value { get; set; }

        [XmlIgnore]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value as Doc; }
        }
    }

    [DataContract]
    public class AttributeVariable : WorkflowVariable
    {
        [DataMember]
        public AttributeBase Value { get; set; }

        [XmlIgnore]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value as AttributeBase; }
        }
    }

    [DataContract]
    public class EnumValueVariable : WorkflowVariable
    {
        [DataMember]
        public EnumValue Value { get; set; }

        [XmlIgnore]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value as EnumValue; }
        }
    }

    [DataContract]
    public class QueryVariable : WorkflowVariable
    {
        [DataMember]
        public QueryDef Value { get; set; }

        [XmlIgnore]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value as QueryDef; }
        }
    }

    [DataContract]
    public class ObjectListVariable : WorkflowVariable
    {
        [DataMember]
        public List<object> Value { get; set; }

        [XmlIgnore]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value as List<object>; }
        }
    }

    [DataContract]
    public class DocListVariable : WorkflowVariable
    {
        [DataMember]
        public List<Doc> Value { get; set; }

        [XmlIgnore]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value as List<Doc>; }
        }
    }

}
