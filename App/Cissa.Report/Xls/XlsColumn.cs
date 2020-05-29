using System;
using System.Linq;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsColumn : XlsGroup
    {
        public bool FullHeight { get; set; }

        public XlsRow AddRow()
        {
            var row = new XlsRow();
            Items.Add(row);
            return row;
        }

        public XlsRow AddRow(string text, int colSpan = 0, int rowSpan = 0)
        {
            var row = new XlsRow();
            Items.Add(row);
            row.AddText(text, colSpan, rowSpan);
            return row;
        }

        public XlsNode AddNode(XlsCell cell)
        {
            var node = new XlsCellNode(cell);
            Items.Add(node);
            return node;
        }

        public XlsNode AddNode(string text, int colSpan = 0, int rowSpan = 0)
        {
            var node = (colSpan == 0 && rowSpan == 0) ? new XlsTextNode(text) : (XlsNode) new XlsCellNode(text, colSpan, rowSpan);
            Items.Add(node);
            return node;
        }

        public override int GetCols()
        {
            return Items.Count > 0 ? Math.Max(Items.Max(item => item.GetCols()), 1) : 1;
        }

        public override int GetRows()
        {
            return Items.Sum(item => item.GetRows());
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            var oldStyle = writer.MergeStyle(Style);
            try
            {
                using (var colWriter = writer.AddColArea(GetRows(), GetCols()))
                {
                    // colWriter.SetBorder(BorderTop, BorderLeft, BorderRight, BorderBottom);

                    foreach (var item in Items)
                    {
                        if (item.GetRows() == 0) continue;

                        using (var rowWriter = colWriter.AddRowArea())
                        {
                            item.WriteTo(rowWriter, param);
                        }
                    }
                }
            }
            finally
            {
                writer.Style = oldStyle;
            }
        }
    }
}