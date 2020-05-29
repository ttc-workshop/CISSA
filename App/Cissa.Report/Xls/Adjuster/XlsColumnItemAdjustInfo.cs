using System;
using System.Linq;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public class XlsColumnItemAdjustInfo
    {
        public const int TextColumnWidth = 20;
        public const int IntegerColumnWidth = 10;
        public const int FloatColumnWidth = 10;
        public const int DateColumnWidth = 10;
        public const int CurrencyColumnWidth = 10;
        public const int BooleanColumnWidth = 5;

        public object Control { get; protected set; }
        public int Size { get; internal set; }
        public int ColSpan { get; internal set; }
        public int ColumnNo { get; internal set; }

        public int? No { get; internal set; }

        public XlsColumnItemAdjustInfo(object column, int size, int? no)
        {
            Control = column;
            Size = size;
            No = no;
        }

        public XlsColumnItemAdjustInfo(object column, int size): this(column, size, null) {}

        public XlsColumnItemAdjustInfo(object column): this(column, 0, null) {}

        public static readonly char[] Delimiters = { ' ', '\r', '\n' };
        public static int GetMaxWordLength(string s)
        {
            if (String.IsNullOrEmpty(s)) return 0;

            var words = s.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);

            return words.Max(w => w.Length);
        }
    }
}