using System;
using System.Linq;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Model;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class XlsDefToWordDefConverter
    {
        public XlsDefToWordDefConverter(XlsDef xlsDef, WordDocDef wordDef)
        {
            foreach (var area in xlsDef.Areas)
            {
                BuildArea(area, wordDef);
            }
        }

        private void BuildArea(XlsArea area, WordDocDef wordDef)
        {
            var grid = area as XlsGrid;
            if (grid != null)
            {
                if (grid.Items.Count == 0) return;

                //var tableBuilder = new TableBuilder(grid.GetRows(), grid.GetCols());
                var rowNo = 0;
                var table = wordDef.AddTable();
                table.Style = grid.Style;
                while (!grid.RowDatas.Eof())
                {
                    foreach (var gridRow in grid.Items.OfType<XlsRow>())
                    {
                        BuildTable(gridRow, table, rowNo, 0);
                    }
                    grid.RowDatas.Next();
                    rowNo++;
                }
                wordDef.AddParagraph(String.Empty);
                return;
            }
            foreach (var item in area.Items)
            {
                if (item.GetRows() == 0)
                {
                    wordDef.AddParagraph(String.Empty, item.Style);
                    continue;
                }

                BuildItem(item, wordDef);
            }
        }


        private void BuildItem(XlsItem item, WordDocDef wordDef)
        {
            var row = item as XlsRow;
            if (row != null)
            {
                if (row.Items.Count == 0) return;
                
                if (row.Items.Count == 1 && row.Items[0] is XlsCell)
                {
                    var cell = (XlsCell) row.Items[0];
                    var val = cell.GetValue();
                    wordDef.AddParagraph(val != null ? val.ToString() : String.Empty, cell.Style);
                    return;
                }

                /*var tableBuilder = new TableBuilder(row.GetRows(), row.GetCols());
                RowToTableInfo(row, tableBuilder, 0, 0);*/

                var table = wordDef.AddTable();
                table.Style = row.Style;
                BuildTable(row, table, 0, 0);
                return;
            }
            var grid = item as XlsGrid;
            if (grid != null)
            {
                if (grid.Items.Count == 0) return;

                //var tableBuilder = new TableBuilder(grid.GetRows(), grid.GetCols());
                var rowNo = 0;
                var table = wordDef.AddTable();
                table.Style = grid.Style;
                while (!grid.RowDatas.Eof())
                {
                    foreach (var gridRow in grid.Items.OfType<XlsRow>())
                    {
                        BuildTable(gridRow, table, rowNo, 0);
                    }
                    grid.RowDatas.Next();
                    rowNo++;
                }
                wordDef.AddParagraph(String.Empty);
                return;
            }
            var area = item as XlsArea;
            if (area != null)
            {
                foreach (var subItem in area.Items)
                {
                    BuildItem(subItem, wordDef);
                }
            }
        }

        private void BuildTable(XlsRow row, WordTableDef table, int rowNo, int colNo)
        {
            var tableRow = table.InsertRow(rowNo);
            tableRow.Style = row.Style;

            foreach (var item in row.Items)
            {
                /*if (item is XlsDataField)
                {
                    var cell = tableRow.AddCell(colNo);
                    cell.Style = item.Style;
                    cell.ColSpan = 1;
                    var val = ((XlsCell)item).GetValue();
                    var type = ((XlsDataField)item).Field.GetDataType();
                    var s = String.Empty;
                    if (val != null)
                        switch (type)
                        {
                            case CissaDataType.Text:
                                s = (string)val;
                                break;
                            case CissaDataType.Int:
                                s = ((int)val).ToString();
                                break;
                            case CissaDataType.Float:
                                s = ((double)val).ToString("F");
                                break;
                            case CissaDataType.Currency:
                                s = ((decimal)val).ToString("N");
                                break;
                            case CissaDataType.DateTime:
                                s = ((DateTime)val).ToShortDateString();
                                break;
                            case CissaDataType.Bool:
                                s = ((bool)val) ? "Да" : "Нет";
                                break;
                            default:
                                s = val.ToString();
                                break;
                        }
                    cell.AddText(s, item.Style);
                    colNo++;
                }
                else if (item is XlsCell)
                {
                    var cell = tableRow.AddCell(colNo);
                    cell.Style = item.Style;
                    cell.ColSpan = 1;
                    var val = ((XlsCell)item).GetValue();
                    cell.AddText(val != null ? val.ToString() : String.Empty, item.Style);
                    colNo++;
                }
                else if (item is XlsNode)
                {
                    var cell = tableRow.AddCell(colNo);
                    cell.Style = item.Style;
                    cell.ColSpan = item.GetCols();
                    if (item is XlsTextNode)
                        cell.AddText(((XlsTextNode)item).Text, item.Style);
                    else if (item is XlsCellNode)
                    {
                        var val = ((XlsCellNode)item).Cell.GetValue();
                        cell.AddText(val != null ? val.ToString() : String.Empty, item.Style);
                    }

                    var node = (XlsNode)item;
                    if (node.Items.Count > 0)
                        AddNodeCell(node, table, rowNo + 1, colNo);
                    colNo += node.GetCols();
                }*/
                AddTableRowCell(table, tableRow, item, ref rowNo, ref colNo);
            }
        }

        private void AddNodeCell(XlsNode node, WordTableDef table, int rowNo, int colNo)
        {
            var row = table.InsertRow(rowNo);

            foreach (var item in node.Items)
            {
                /*if (item is XlsDataField)
                {
                    var cell = row.AddCell(colNo);
                    cell.Style = item.Style;
                    cell.ColSpan = 1;
                    var val = ((XlsCell)item).GetValue();
                    var type = ((XlsDataField) item).Field.GetDataType();
                    var s = String.Empty;
                    if (val != null)
                        switch (type)
                        {
                            case CissaDataType.Text:
                                s = (string) val;
                                break;
                            case CissaDataType.Int:
                                s = ((int)val).ToString();
                                break;
                            case CissaDataType.Float:
                                s = ((double)val).ToString("F");
                                break;
                            case CissaDataType.Currency:
                                s = ((decimal) val).ToString("N");
                                break;
                            case CissaDataType.DateTime:
                                s = ((DateTime)val).ToShortDateString();
                                break;
                            case CissaDataType.Bool:
                                s = ((bool)val) ? "Да" : "Нет";
                                break;
                            default:
                                s = val.ToString();
                                break;
                        }
                    cell.AddText(s, item.Style);
                    colNo++;
                }
                else if (item is XlsCell)
                {
                    var cell = row.AddCell(colNo);
                    cell.Style = item.Style;
                    cell.ColSpan = 1;
                    var val = ((XlsCell) item).GetValue();
                    cell.AddText(val != null ? val.ToString() : String.Empty, item.Style);
                    colNo++;
                }
                else if (item is XlsNode)
                {
                    var cell = row.AddCell(colNo);
                    cell.Style = item.Style;
                    cell.ColSpan = item.GetCols();
                    if (item is XlsTextNode)
                        cell.AddText(((XlsTextNode)item).Text, item.Style);
                    else if (item is XlsCellNode)
                    {
                        var val = ((XlsCellNode)item).Cell.GetValue();
                        cell.AddText(val != null ? val.ToString() : String.Empty, item.Style);
                    }

                    var childNode = (XlsNode)item;
                    if (childNode.Items.Count > 0)
                        AddNodeCell(childNode, table, rowNo + 1, colNo);
                    colNo += childNode.GetCols();
                }*/
                AddTableRowCell(table, row, item, ref rowNo, ref colNo);
            }
        }

        private void AddTableRowCell(WordTableDef table, WordTableRowDef row, XlsItem item, ref int rowNo, ref int colNo)
        {
            var field = item as XlsDataField;
            if (field != null)
            {
                var cell = row.AddCell(colNo);
                cell.Style = field.Style;
                cell.ColSpan = 1;
                var val = ((XlsCell) item).GetValue();
                var type = field.Field.GetDataType();
                var s = String.Empty;
                if (val != null)
                    switch (type)
                    {
                        case BaseDataType.Text:
                            s = (string) val;
                            break;
                        case BaseDataType.Int:
                            s = ((int) val).ToString();
                            break;
                        case BaseDataType.Float:
                            s = ((double) val).ToString("F");
                            break;
                        case BaseDataType.Currency:
                            s = ((decimal) val).ToString("N");
                            break;
                        case BaseDataType.DateTime:
                            s = ((DateTime) val).ToShortDateString();
                            break;
                        case BaseDataType.Bool:
                            s = ((bool) val) ? "Да" : "Нет";
                            break;
                        default:
                            s = val.ToString();
                            break;
                    }
                cell.AddText(s, field.Style);
                colNo++;
            }
            else
            {
                var xlsCell = item as XlsCell;
                if (xlsCell != null)
                {
                    var cell = row.AddCell(colNo);
                    cell.Style = xlsCell.Style;
                    cell.ColSpan = 1;
                    var val = xlsCell.GetValue();
                    cell.AddText(val != null ? val.ToString() : String.Empty, xlsCell.Style);
                    colNo++;
                }
                else
                {
                    var xlsNode = item as XlsNode;
                    if (xlsNode != null)
                    {
                        var cell = row.AddCell(colNo);
                        cell.Style = xlsNode.Style;
                        cell.ColSpan = item.GetCols();
                        var textNode = item as XlsTextNode;
                        if (textNode != null)
                            cell.AddText(textNode.Text, xlsNode.Style);
                        else
                        {
                            var cellNode = item as XlsCellNode;
                            if (cellNode != null)
                            {
                                var val = cellNode.Cell.GetValue();
                                cell.AddText(val != null ? val.ToString() : String.Empty, xlsNode.Style);
                            }
                        }

                        var node = xlsNode;
                        if (node.Items.Count > 0)
                            AddNodeCell(node, table, rowNo + 1, colNo);
                        colNo += node.GetCols();
                    }
                }
            }
        }
    }
}
