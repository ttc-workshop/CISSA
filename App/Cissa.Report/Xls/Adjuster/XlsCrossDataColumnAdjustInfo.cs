using System;
using Intersoft.Cissa.Report.Defs;
using Intersoft.Cissa.Report.Utils;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public class XlsCrossDataColumnAdjustInfo : XlsColumnItemAdjustInfo
    {
        public CrossDataGroupColumnValue ColumnValue { get; private set; }
        public int CaptionSize { get; private set; }

        public XlsCrossDataColumnAdjustInfo(CrossDataColumnItem column)
            : base(column.Column)
        {
            Control = column.Column;
            ColumnValue = column.ColumnValues != null ? column.ColumnValues[0] : null;
            CaptionSize = GetMaxWordLength(column.Caption);

            Size = Math.Max(IntegerColumnWidth, CaptionSize);
        }
    }
}