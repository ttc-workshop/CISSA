namespace Intersoft.Cissa.Report.Xls
{
    public class XlsText : XlsCell
    {
        public string Value { get; set; }

        public XlsText(string value, int colSpan = 0, int rowSpan = 0) : base(colSpan, rowSpan)
        {
            Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }

        public override int GetDefaultSize()
        {
            return 30;
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            var oldStyle = writer.MergeStyle(Style);
            try
            {
                //            writer.SetBorder(BorderTop, BorderLeft, BorderRight, BorderBottom);
                writer.AddCell(ColSpan, RowSpan);
                writer.SetValue(Value);
                if (Width != null)
                    writer.SetColumnWidth((int)Width);
            }
            finally
            {
                writer.Style = oldStyle;
            }
        }
    }
}