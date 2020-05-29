using System;
using System.Collections.Generic;
using Intersoft.Cissa.Report.Defs;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public class XlsReportDefAdjustInfo : XlsFormAdjustInfo
    {
        private readonly List<XlsTableReportColumnAdjustInfo> _columns = new List<XlsTableReportColumnAdjustInfo>();
        public override List<XlsColumnItemAdjustInfo> Columns { get { return new List<XlsColumnItemAdjustInfo>(_columns); } }

        public Guid FormId { get; private set; }

        public XlsReportDefAdjustInfo(ReportDef report)
        {
            //FormId = report.Id;

            if (report.Columns != null)
                foreach (var column in report.Columns)
                    AddControlBand(column);
        }

        public int GetColumnSize(int count)
        {
            var result = 0;
            for (var i = 0; i <= Math.Min(Columns.Count - 1, count); i++)
            {
                var column = Columns[i];
                result += column.Size;
            }
            return result;
        }

        public override void SetTotalSize(int totalSize)
        {
            // Do nothing;
        }

        protected void AddControlBand(ReportColumnDef column)
        {
            AddControlColumn(column);
        }

        protected void AddControlColumn(ReportColumnDef column)
        {
            _columns.Add(new XlsTableReportColumnAdjustInfo(column));
        }
    }
}