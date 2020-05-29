using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    [KnownType(typeof(CurrencyAttribute))]
    [KnownType(typeof(DocAttribute))]
    [KnownType(typeof(DocListAttribute))]
    [KnownType(typeof(EnumAttribute))]
    [KnownType(typeof(FloatAttribute))]
    [KnownType(typeof(IntAttribute))]
    [KnownType(typeof(TextAttribute))]
    [KnownType(typeof(DateTimeAttribute))]
    [KnownType(typeof(BoolAttribute))]
    [KnownType(typeof(OrganizationAttribute))]
    [KnownType(typeof(DocumentStateAttribute))]
    [KnownType(typeof(MetaInfoAttribute))]
    [KnownType(typeof(ObjectDefAttribute))]
    [KnownType(typeof(BlobAttribute))]

    [XmlInclude(typeof(CurrencyAttribute))]
    [XmlInclude(typeof(DocAttribute))]
    [XmlInclude(typeof(DocListAttribute))]
    [XmlInclude(typeof(EnumAttribute))]
    [XmlInclude(typeof(FloatAttribute))]
    [XmlInclude(typeof(IntAttribute))]
    [XmlInclude(typeof(TextAttribute))]
    [XmlInclude(typeof(DateTimeAttribute))]
    [XmlInclude(typeof(BoolAttribute))]
    [XmlInclude(typeof(OrganizationAttribute))]
    [XmlInclude(typeof(DocumentStateAttribute))]
    [XmlInclude(typeof(MetaInfoAttribute))]
    [XmlInclude(typeof(ObjectDefAttribute))]
    [XmlInclude(typeof(BlobAttribute))]
    public abstract class AttributeBase
    {
        [DataMember]
        public AttrDef AttrDef { get; set; }

        [DataMember]
        public DateTime Created { get; set; }

        [XmlIgnore]
        public abstract object ObjectValue { get; set; }
    }
}
