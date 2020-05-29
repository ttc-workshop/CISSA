using System;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model;
using NPOI.HSSF.Record.Formula.Functions;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsRow : XlsGroup
    {
        public XlsColumn AddColumn()
        {
            var column = new XlsColumn();
            Items.Add(column);
            return column;
        }

        public XlsColumn AddColumn(string text, int colSpan = 0, int rowSpan = 0)
        {
            var column = new XlsColumn();
            Items.Add(column);
            column.AddText(text, colSpan, rowSpan);
            return column;
        }

        public XlsNode AddNode(XlsCell cell)
        {
            var node = new XlsCellNode(cell);
            Items.Add(node);
            return node;
        }

        public XlsNode AddNode(string text, int colSpan = 0, int rowSpan = 0)
        {
            var node = (colSpan == 0 && rowSpan == 0) ? new XlsTextNode(text) : (XlsNode)new XlsCellNode(text, colSpan, rowSpan);
            Items.Add(node);
            return node;
        }

        public override int GetCols()
        {
            return Items.Sum(item => item.GetCols());
        }

        public override int GetRows()
        {
            return Items.Count > 0 ? Math.Max(Items.Max(item => item.GetRows()), 1) : 1;
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            var oldStyle = writer.MergeStyle(Style);
            try
            {
                using (var rowWriter = writer.AddRowArea(GetRows(), GetCols()))
                {
                    // rowWriter.SetBorder(BorderTop, BorderLeft, BorderRight, BorderBottom);

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

    public class XlsTextLineRow : XlsGroup
    {
        public override int GetCols()
        {
            return Items.Sum(item => item.GetCols());
        }

        public override int GetRows()
        {
            return Items.Count > 0 ? Math.Max(Items.Max(item => item.GetRows()), 1) : 1;
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            var oldStyle = writer.MergeStyle(Style);
            try
            {
                using (var rowWriter = writer.AddRowArea(GetRows(), GetCols()))
                {
                    var s = String.Empty;

                    foreach (var item in Items)
                    {
                        if (item is XlsDataField)
                        {
                            var field = (XlsDataField) item;
                            var val = field.GetValue();
                            var type = field.Field.GetDataType();
                            if (val != null)
                                switch (type)
                                {
                                    case BaseDataType.Text:
                                        s += (string)val;
                                        break;
                                    case BaseDataType.Int:
                                        s += ((int)val).ToString();
                                        break;
                                    case BaseDataType.Float:
                                        s += ((double)val).ToString("F");
                                        break;
                                    case BaseDataType.Currency:
                                        s += ((decimal)val).ToString("N");
                                        break;
                                    case BaseDataType.DateTime:
                                        s += ((DateTime)val).ToShortDateString();
                                        break;
                                    case BaseDataType.Bool:
                                        s += ((bool)val) ? "Да" : "Нет";
                                        break;
                                    default:
                                        s += val.ToString();
                                        break;
                                }
                        }
                        else if (item is XlsCell)
                        {
                            var val = ((XlsCell)item).GetValue();
                            s += val != null ? val.ToString() : String.Empty;
                        }
                        else if (item is XlsNode)
                        {
                            if (item is XlsTextNode)
                                s += ((XlsTextNode)item).Text;
                            else if (item is XlsCellNode)
                            {
                                var val = ((XlsCellNode)item).Cell.GetValue();
                                s += val != null ? val.ToString() : String.Empty;
                            }
                        }
                    }
                    rowWriter.AddCell(GetCols(), GetRows());
                    writer.SetValue(s);
                }
            }
            finally
            {
                writer.Style = oldStyle;
            }
        }
    }
}