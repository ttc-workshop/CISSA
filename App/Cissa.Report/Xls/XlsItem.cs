using System;
using Intersoft.Cissa.Report.Styles;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsItem: IDisposable
    {
        private readonly ContentStyle _style = new ContentStyle();

        public ContentStyle Style
        {
            get { return _style; }
            set { _style.Assign(value); }
        }
//        public bool? BorderTop { get; set; }
//        public bool? BorderLeft { get; set; }
//        public bool? BorderRight { get; set; }
//        public bool? BorderBottom { get; set; }

        public void ShowAllBorders(bool? enable)
        {
//            BorderTop = enable;
//            BorderLeft = enable;
//            BorderRight = enable;
//            BorderBottom = enable;
            if (enable ?? false)
                Style.Borders = TableCellBorder.Left | TableCellBorder.Top | TableCellBorder.Right | TableCellBorder.Bottom;
        }

        public virtual int GetCols()
        {
            return 0;
        }
        public virtual int GetRows()
        {
            return 0;
        }

        public virtual void WriteTo(XlsWriter writer, int param = 0) {}

        public void Dispose()
        {
            DoDispose();
        }

        protected virtual void DoDispose() {}
    }
}