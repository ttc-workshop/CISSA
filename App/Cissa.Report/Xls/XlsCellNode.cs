using System;
using System.Linq;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsCellNode : XlsNode
    {
        public XlsCell Cell { get; set; }

        public XlsCellNode(XlsCell cell)
        {
            Cell = cell;
        }

        public XlsCellNode(string text, int colSpan = 0, int rowSpan = 0)
        {
            Cell = new XlsText(text, colSpan, rowSpan);
        }

        public override XlsNode AddNode(XlsCell cell)
        {
            var node = new XlsCellNode(cell);
            Items.Add(node);
            return node;
        }

        public override XlsNode AddNode(string text, int colSpan = 0, int rowSpan = 0)
        {
            var node = (colSpan == 0 && rowSpan == 0) ? new XlsTextNode(text) : (XlsNode)new XlsCellNode(text, colSpan, rowSpan);
            Items.Add(node);
            return node;
        }

        public override int GetCols()
        {
            return Math.Max(Items.Sum(item => item.GetCols()), (Cell != null ? Cell.GetCols() : 1));
        }

        public override int GetRows()
        {
            return (Items.Count > 0 ? Items.Max(item => item.GetRows()) : 0) + (Cell != null ? Cell.GetRows() : 0);
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            var oldStyle = writer.MergeStyle(Style);
            try
            {
                if (Cell != null)
                {
                    Cell.ColSpan = GetCols();
                    Cell.WriteTo(writer, param);
                }

                using (var rowWriter = writer.AddRowArea())
                {
                    //rowWriter.SetBorder(BorderTop, BorderLeft, BorderRight, BorderBottom);

                    foreach (var item in Items)
                    {
                        if (item.GetRows() == 0) continue;

                        using (var colWriter = rowWriter.AddColArea())
                        {
                            item.WriteTo(colWriter, param);
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