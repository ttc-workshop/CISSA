namespace Intersoft.Cissa.Report.Xls
{
    public abstract class XlsNode : XlsGroup
    {
        public int ColSpan { get; set; }

        public XlsColumn AddColumn()
        {
            var column = new XlsColumn();
            Items.Add(column);
            return column;
        }

        public XlsColumn AddColumn(string text, int colSpan = 0, int rowSpan = 0)
        {
            var column = new XlsColumn();
            Items.Add(column);
            column.AddText(text, colSpan, rowSpan);
            return column;
        }

        public abstract XlsNode AddNode(XlsCell cell);
        public abstract XlsNode AddNode(string text, int colSpan = 0, int rowSpan = 0);

        public override int GetCols()
        {
            return ColSpan > 1 ? ColSpan : 1;
        }
    }
}