using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.BizService.Defs
{
    [DataContract]
    public class BizControlData
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid ProcessDefId { get; set; }
        [DataMember]
        public Guid DocumentDefId { get; set; }
        [DataMember]
        public Guid AttributeDefId { get; set; }
        [DataMember]
        public Guid EnumDefId { get; set; }
        [DataMember]
        public Guid EnumId { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public Image Image { get; set; }
        [DataMember]
        public object Value { get; set; }
    }

    [DataContract]
    public class BizControl : BizObject
    {
    }

    [DataContract]
    public class BizButton : BizControl
    {
        [DataMember]
        public Guid ProcessDefId { get; set; }
    }

    [DataContract]
    public class BizEdit : BizControl
    {
        public enum EditDataType {Int, Float, Text, DateTime, Currency}
        [DataMember]
        public EditDataType DataType { get; set; }
        [DataMember]
        public Guid AttributeDefId { get; set; }
        [DataMember]
        public object Value { get; set; }
    }

    [DataContract]
    public class BizComboBox : BizControl
    {
        [DataMember]
        public Guid AttributeDefId { get; set; }
        [DataMember]
        public Guid EnumDefId { get; set; }
        [DataMember]
        public object Value { get; set; }
    }

    [DataContract]
    public class BizLabel : BizControl
    {
        [DataMember]
        public string Text { get; set; }
    }

    [DataContract]
    public class BizImage : BizControl
    {
        [DataMember]
        public Image Image { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Width { get; set; }
    }

    [DataContract]
    public class BizGrid : BizControl
    {
        public enum BizGridType {Table, Detail}
        [DataMember]
        public Guid DocumentDefId { get; set; }
        [DataMember]
        public BizGridType GridType { get; set; }
        [DataMember]
        public List<BizControl> Items { get; set; }
    }

    [DataContract]
    public class BizCheckBox : BizControl
    {
        [DataMember]
        public Guid AttributeDefId { get; set; }
        [DataMember]
        public bool Checked { get; set; }
    }

    [DataContract]
    public class BizRadioItem : BizControl
    {
        [DataMember]
        public Guid EnumId { get; set; }
        [DataMember]
        public bool Checked { get; set; }
    }

    [DataContract]
    public class BizRadioGroup : BizControl
    {
        [DataMember]
        public Guid AttributeDefId { get; set; }
        [DataMember]
        public List<BizRadioItem> Items { get; set; }
    }

    [DataContract]
    public class BizForm : BizControl
    {
        [DataMember]
        public Guid DocumentDefId { get; set; }
        [DataMember]
        public List<BizControl> Controls { get; set; }
    }
}