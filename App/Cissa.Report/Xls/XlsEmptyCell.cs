namespace Intersoft.Cissa.Report.Xls
{
    public class XlsEmptyCell : XlsCell
    {
        public XlsEmptyCell() : base() {}
        public XlsEmptyCell(int colSpan = 0, int rowSpan = 0) : base(colSpan, rowSpan) {}

        public override object GetValue()
        {
            return null;
        }
    }
}