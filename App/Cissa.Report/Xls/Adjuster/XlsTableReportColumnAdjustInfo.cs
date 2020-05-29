using System;
using Intersoft.Cissa.Report.Defs;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public class XlsTableReportColumnAdjustInfo : XlsColumnItemAdjustInfo
    {
        public Guid Id { get; private set; }
        public int CaptionSize { get; private set; }

        public XlsTableReportColumnAdjustInfo(ReportColumnDef column)
            : base(column)
        {
            Control = column;
            Id = column.Id;
            CaptionSize = GetMaxWordLength(column.Caption);

            if (column is ReportRowNoColumnDef)
                Size = Math.Max(IntegerColumnWidth, CaptionSize);
            else
                Size = Math.Max(TextColumnWidth, CaptionSize);
        }
    }
}