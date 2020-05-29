namespace Intersoft.Cissa.Report.Xls
{
    public abstract class XlsCell : XlsItem
    {
        public abstract object GetValue();

        public int ColSpan { get; set; }
        public int RowSpan { get; set; }

        public int Size { get; set; }

        public bool ShrinkToFit { get; set; }
        public bool AutoSize { get; set; }

        protected XlsCell() {}

        protected XlsCell(int colSpan = 0, int rowSpan = 0)
        {
            ColSpan = colSpan;
            RowSpan = rowSpan;
        }

        public virtual int GetDefaultSize()
        {
            return 0;
        }

        public override int GetCols()
        {
            return ColSpan > 1 ? ColSpan : 1;
        }
        public override int GetRows()
        {
            return RowSpan > 1 ? RowSpan : 1;
        }

        public float? Width { get; set; }
        public float? Height { get; set; }

        public virtual int GetSize()
        {
            if (Size > 0) return Size;
            var defSize = GetDefaultSize();
            if (defSize > 0) return defSize;

            var value = GetValue();
            if (value != null)
                return value.ToString().Length;
            return 0;
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            //writer.SetBorder(BorderTop, BorderLeft, BorderRight, BorderBottom);
            var oldStyle = writer.MergeStyle(Style);
            try
            {
//                writer.ShrinkToFit = ShrinkToFit;

                writer.AddCell(ColSpan, RowSpan);
                writer.SetValue(GetValue());
                if (Width != null)
                    writer.SetColumnWidth((int) Width);
/*
                if (Style.AutoWidth ?? false /*AutoSize♥1♥)
                {
                    if (ColSpan > 0 && RowSpan > 0)
                        writer.AutoSizeColumn(true);
                    else
                        writer.AutoSizeColumn();
                }
*/
            }
            finally
            {
                writer.Style = oldStyle;
            }
        }
    }
}