using System;
using System.Collections.Generic;
using Intersoft.Cissa.Report.Utils;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public class XlsCrossDataTableAdjustInfo : XlsFormAdjustInfo
    {
        private readonly List<XlsCrossDataColumnAdjustInfo> _columns = new List<XlsCrossDataColumnAdjustInfo>();
        public override List<XlsColumnItemAdjustInfo> Columns { get { return new List<XlsColumnItemAdjustInfo>(_columns); } }

        public XlsCrossDataTableAdjustInfo(CrossDataTable table)
        {
            //FormId = report.Id;

            if (table.Columns != null)
                foreach (var column in table.ColumnItems())
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

        protected void AddControlBand(CrossDataColumnItem column)
        {
            AddControlColumn(column);
        }

        protected void AddControlColumn(CrossDataColumnItem column)
        {
            _columns.Add(new XlsCrossDataColumnAdjustInfo(column));
        }
    }
}