using System;
using System.Linq;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsTextNode : XlsNode
    {
        public string Text { get; set; }

        public XlsTextNode(string text)
        {
            Text = text;
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
            return Math.Max(Items.Sum(item => item.GetCols()), 1);
        }

        public override int GetRows()
        {
            return (Items.Count > 0 ? Items.Max(item => item.GetRows()) : 0) + 1;
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            var oldStyle = writer.MergeStyle(Style);
            try
            {
                if (Items.Count == 0)
                    writer.AddCell(ColSpan, writer.EndRowIndex - writer.CurrentRowIndex);
                else
                    writer.AddCell(GetCols());

                // writer.SetBorder(BorderTop, BorderLeft, BorderRight, BorderBottom);
                writer.SetValue(Text);

                using (var rowWriter = writer.AddRowArea())
                {
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