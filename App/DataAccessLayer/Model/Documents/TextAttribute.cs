using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class TextAttribute : AttributeBase
    {
        public TextAttribute() {}

        public TextAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
        }

        [DataMember]
        public String Value { get; set; }

        //EditText может рисоваться как TextArea, поэтому добавил строки и столбцы
        [DataMember]
        public ushort Rows { get; set; }
        [DataMember]
        public ushort Cols { get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        public override object ObjectValue
        {
            get { return Value; }
            set { Value = value != null ? value.ToString() : String.Empty; }
        }
    }
}