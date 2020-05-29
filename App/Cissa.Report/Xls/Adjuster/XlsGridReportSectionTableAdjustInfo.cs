using System;
using System.Collections.Generic;
using System.Linq;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public class XlsGridReportSectionTableAdjustInfo : XlsFormAdjustInfo
    {
        public XlsGridReportSectionTable Section { get; private set; }

        private readonly List<XlsColumnItemAdjustInfo> _columns = new List<XlsColumnItemAdjustInfo>();
        public override List<XlsColumnItemAdjustInfo> Columns { get { return _columns; } }

        private readonly int _colCount = 0;

        private int _avgSize = 10;

        public XlsGridReportSectionTableAdjustInfo(XlsGridReportSectionTable section)
        {
            Section = section;
            _columns.Add(new XlsColumnItemAdjustInfo(section, Section.LeftMargin, -1));
            var colMax = section.Cells.Max(c => c.Col);
            _colCount = colMax + 1;
            for (var i = 0; i < _colCount; i++)
            {
                var i1 = i;
                var cells = section.Cells.Where(c => c.Col == i1);
                var maxSize = cells.Select(cell => XlsTableFormControlAdjustInfo.GetMaxWordLength(cell.Text)).Concat(new[] {_avgSize}).Max();
                _columns.Add(new XlsColumnItemAdjustInfo(section, maxSize, i));
            }
            _columns.Add(new XlsColumnItemAdjustInfo(null, Section.RightMargin));
        }

        public override void SetTotalSize(int totalSize)
        {
            if (_colCount > 0)
            {
                _avgSize = (totalSize - (Section.LeftMargin + Section.RightMargin))/_colCount;
                _columns.ForEach(c => { if (c.No != null && c.No >= 0) c.Size = _avgSize; });
                var lastCell = _columns.LastOrDefault(c => c.No != null && c.No >= 0 && c.Control != null);
                if (lastCell != null)
                    lastCell.Size = totalSize - (_avgSize*(_colCount - 1)) - (Section.LeftMargin + Section.RightMargin);
            }
        }

        public int GetColumnSize(int count)
        {
            throw new NotImplementedException();
        }

        /*internal override IEnumerable<XlsFormControlSizeInfo> GetControlGreatThen(int size)
        {
            if (Size > size)
                return 
        }

        public override void AdjustColumn(int columnNo, int size)
        {
            ;
        }

        public override int GetTotalSize()
        {
            return Math.Max(XlsTableFormControlAdjustInfo.GetMaxWordLength(Section.Text) + 1, 30);
        }*/
    }
}