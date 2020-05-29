namespace Intersoft.Cissa.Report.Xls
{
    public class XlsBool : XlsCell
    {
        public bool Value { get; set; }

        public XlsBool(bool value, int colSpan = 0, int rowSpan = 0)
            : base(colSpan, rowSpan)
        {
            Value = value;
        }

        public override object GetValue()
        {
            return Value;
        }

        public override int GetDefaultSize()
        {
            return 5;
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            if (param != 0) return;

//            writer.SetBorder(BorderTop, BorderLeft, BorderRight, BorderBottom);
            var oldStyle = writer.MergeStyle(Style);
            try
            {
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