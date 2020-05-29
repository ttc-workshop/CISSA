using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    [KnownType(typeof(BizEditText))]
    [KnownType(typeof(BizEditCurrency))]
    [KnownType(typeof(BizEditFloat))]
    [KnownType(typeof(BizEditInt))]
    [KnownType(typeof(BizEditDateTime))]
    [KnownType(typeof(BizEditBool))]
    [KnownType(typeof(BizEditVar))]
    [KnownType(typeof(BizEditSysIdent))]
    [KnownType(typeof(BizEditFile))]

    [XmlInclude(typeof(BizEditText))]
    [XmlInclude(typeof(BizEditCurrency))]
    [XmlInclude(typeof(BizEditFloat))]
    [XmlInclude(typeof(BizEditInt))]
    [XmlInclude(typeof(BizEditDateTime))]
    [XmlInclude(typeof(BizEditBool))]
    [XmlInclude(typeof(BizEditVar))]
    [XmlInclude(typeof(BizEditSysIdent))]
    [XmlInclude(typeof(BizEditFile))]
    public abstract class BizEdit : BizDataControl
    {
        [DataMember]
        public SystemIdent Ident { get; set; }

        [DataMember]
        public int MaxValue { get; set; }

        [DataMember]
        public int MinValue { get; set; }

        [DataMember]
        public uint MaxLength { get; set; }

        [DataMember]
        public byte Rows { get; set; }

        [DataMember]
        public byte Cols { get; set; }

        [DataMember]
        public bool NotNull { get; set; }

        [DataMember]
        public string EditMask { get; set; }

        [DataMember]
        public string Format { get; set; }

        [DataMember]
        public Guid? ProcessId { get; set; }
    }
}
