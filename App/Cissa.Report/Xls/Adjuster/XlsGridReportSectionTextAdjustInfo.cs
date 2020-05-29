using System;
using System.Collections.Generic;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public class XlsGridReportSectionTextAdjustInfo : XlsFormAdjustInfo
    {
        public XlsGridReportSectionText Section { get; private set; }

        private readonly List<XlsColumnItemAdjustInfo> _columns = new List<XlsColumnItemAdjustInfo>();
        public override List<XlsColumnItemAdjustInfo> Columns { get { return _columns; } }

        public XlsColumnItemAdjustInfo SectionColumn { get; private set; }

        public XlsGridReportSectionTextAdjustInfo(XlsGridReportSectionText section)
        {
            Section = section;
            _columns.Add(new XlsColumnItemAdjustInfo(Section, section.LeftMargin, -1));
            SectionColumn = new XlsColumnItemAdjustInfo(Section, XlsColumnItemAdjustInfo.TextColumnWidth, 0);
            _columns.Add(SectionColumn);
            _columns.Add(new XlsColumnItemAdjustInfo(null, section.RightMargin));
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

        public override void SetTotalSize(int totalSize)
        {
            SectionColumn.Size = totalSize - (Section.LeftMargin + Section.RightMargin);
        }
    }
}