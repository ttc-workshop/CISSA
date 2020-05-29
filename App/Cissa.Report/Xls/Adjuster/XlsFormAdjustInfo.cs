using System.Collections.Generic;
using System.Linq;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public abstract class XlsFormAdjustInfo
    {
        public abstract List<XlsColumnItemAdjustInfo> Columns { get; }
        public XlsColumnItemAdjustInfo FindControl(object control)
        {
            return Columns.FirstOrDefault(c => c.Control == control);
        }

        public XlsColumnItemAdjustInfo FindControl(object control, int no)
        {
            return Columns.FirstOrDefault(c => c.Control == control && c.No != null && c.No == no);
        }
        //public abstract int GetColumnSize(int count);

        internal IEnumerable<XlsFormControlSizeInfo> GetControlGreatThen(int size)
        {
            var result = 0;
            var i = 0;
            foreach (var control in Columns)
            {
                result += control.Size;
                if (result > size)
                {
                    yield return new XlsFormControlSizeInfo(i, control, result);
                    break;
                }
                i++;
            }
        }

        public void AdjustColumn(int columnNo, int size)
        {
            var total = 0;
            var prevNo = 0;
            foreach (var column in Columns)
            {
                total += column.Size;

                if (total == size)
                {
                    column.ColumnNo = columnNo;
                    column.ColSpan = columnNo - prevNo;
                    break;
                }
                if (total > size)
                    break;

                prevNo = column.ColumnNo;
            }
        }


        public int GetTotalSize()
        {
            return Columns.Sum(c => c.Size);
        }

        public abstract void SetTotalSize(int totalSize);
    }
}