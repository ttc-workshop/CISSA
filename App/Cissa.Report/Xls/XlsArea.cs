namespace Intersoft.Cissa.Report.Xls
{
    public class XlsArea : XlsGroup
    {
        public XlsRow AddRow()
        {
            var row = new XlsRow();
            Items.Add(row);
            return row;
        }

        public XlsRow AddEmptyRow(int colSpan = 0, int rowSpan = 0)
        {
            var row = new XlsRow();
            Items.Add(row);
            row.AddEmptyCell(colSpan, rowSpan);
            return row;
        }

        public XlsTextLineRow AddTextRow(string text, int colSpan = 0, int rowSpan = 0)
        {
            var row = new XlsTextLineRow();
            Items.Add(row);
            row.AddText(text, colSpan, rowSpan);
            return row;
        }

        public virtual bool Continue()
        {
            return false;
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
//            using (var rowWriter = writer.AddRowArea())
            var oldStyle = writer.MergeStyle(Style);
            try
            {
                foreach (var item in Items)
                {
                    if (item.GetRows() == 0) continue;

     //               using (var colWriter = rowWriter.AddColArea())
                    using (var rowWriter = writer.AddRowArea())
                    {
                        item.WriteTo(rowWriter, param);
                    }
                }
            }
            finally
            {
                writer.Style = oldStyle;
            }
        }
    }
}