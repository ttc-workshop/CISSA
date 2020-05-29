using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Intersoft.Cissa.Report.Styles;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using Font = NPOI.SS.UserModel.Font;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsWriter : IDisposable
    {
        public XlsWriter Parent { get; private set; }

        public HSSFWorkbook Workbook { get; private set; }
        public Sheet Sheet { get; private set; }
        public int StartRowIndex { get; private set; }
        public int StartColIndex { get; private set; }
        public int EndRowIndex { get; private set; }
        public int EndColIndex { get; private set; }

//        public XlsColor BgColor { get; private set; }
//        public bool? BorderTop { get; set; }
//        public bool? BorderLeft { get; set; }
//        public bool? BorderRight { get; set; }
//        public bool? BorderBottom { get; set; }

        public bool WrapText { get; set; }
        public bool ShrinkToFit { get ; set; }

        public XlsWriter(XlsWriter parent, HSSFWorkbook workbook, Sheet sheet, int startRow, int startCol)
        {
            Parent = parent;
            Workbook = workbook;
            Sheet = sheet;
            StartRowIndex = startRow;
            StartColIndex = startCol;
            CurrentRowIndex = startRow;
            CurrentColIndex = startCol;
            NextRowIndex = startRow;
            NextColIndex = startCol;

            if (Parent != null)
            {
//                BorderTop = Parent.BorderTop;
//                BorderLeft = Parent.BorderLeft;
//                BorderRight = Parent.BorderRight;
//                BorderBottom = Parent.BorderBottom;
                Style = Parent.Style;

                EndRowIndex = Math.Max(Parent.EndRowIndex, CurrentRowIndex);
                EndColIndex = Math.Max(Parent.EndColIndex, CurrentColIndex);
            }
        }

        public XlsWriter(XlsWriter parent, HSSFWorkbook workbook, Sheet sheet, int startRow, int startCol, int endRow, int endCol) : 
            this(parent, workbook, sheet, startRow, startCol)
        {
            EndRowIndex = Math.Max(endRow, CurrentRowIndex);
            EndColIndex = Math.Max(endCol, CurrentColIndex);
        }

       /* public int ColCount { get; private set; }
        public int RowCount { get; private set; }*/

        public int CurrentColIndex { get; private set; }
        public int CurrentRowIndex { get; private set; }

        public int NextColIndex { get; protected set; }
        public int NextRowIndex { get; protected set; }

        public Row CurrentRow { get; private set; }
        public Cell CurrentCell { get; private set; }

        private readonly ContentStyle _style = new ContentStyle();
        public ContentStyle Style
        {
            get { return _style; }
            set { _style.Assign(value); }
        }

        private float _defaultRowHeight = 0;
        public float DefaultRowHeight { 
            get { return _defaultRowHeight; }
            set
            {
                if (!(Math.Abs(_defaultRowHeight - value) > 0.01)) return;

                _defaultRowHeight = value;

                if (Sheet != null)
                    Sheet.DefaultRowHeightInPoints = _defaultRowHeight;
            } 
        }

        /*public TableCellBorder Borders 
        { 
            get { return _style.Borders; }
            set { _style.Borders = value; } 
        }*/

//        private readonly List<XlsWriter> _writers = new List<XlsWriter>();
//        public List<XlsWriter> Writers { get { return _writers; } }

/*
        public int GetLastRow()
        {
            var lastRow = CurrentRowIndex;
            foreach (var writer in Writers)
            {
                var writerLastRow = writer.GetLastRow();
                if (writerLastRow > lastRow) lastRow = writerLastRow;
            }
            return lastRow;
        }

        public int GetLastCol()
        {
            var lastCol = CurrentColIndex;
            foreach (var writer in Writers)
            {
                var writerLastCol = writer.GetLastCol();
                if (writerLastCol > lastCol) lastCol = writerLastCol;
            }
            return lastCol;
        }
*/

        public XlsWriter AddRowArea()
        {
            return new XlsWriter(this, Workbook, Sheet, NextRowIndex, StartColIndex);
        }

        public XlsWriter AddRowArea(int rows, int cols)
        {
            return new XlsWriter(this, Workbook, Sheet, NextRowIndex, StartColIndex,
                                 NextRowIndex + Math.Max(rows, 0), NextColIndex + Math.Max(cols, 0));
        }

        public XlsWriter AddColArea()
        {
            return new XlsWriter(this, Workbook, Sheet, StartRowIndex, NextColIndex);
        }

        public XlsWriter AddColArea(int rows, int cols)
        {
            return new XlsWriter(this, Workbook, Sheet, StartRowIndex, NextColIndex,
                                 NextRowIndex + Math.Max(rows, 0), NextColIndex + Math.Max(cols, 0));
        }

        public void AddRow()
        {
            CurrentRowIndex = /*++*/NextRowIndex++;
            CurrentRow = Sheet.GetRow(CurrentRowIndex) ?? Sheet.CreateRow(CurrentRowIndex);
        }

        protected CellStyle CreateCellStyle(Cell cell)
        {
            //if (cell.CellStyle != null) return cell.CellStyle;

            //var style = Workbook.CreateCellStyle();

            return cell.CellStyle = Workbook.CreateCellStyle();
        }

        protected CellStyle CreateCellStyle(CellRangeAddress region)
        {
            /*var firstCell = true;
            var sameStyle = true;
            CellStyle style = null;

            for (int iRow = region.FirstRow; iRow <= region.LastRow; iRow++)
            {
                var row = Sheet.GetRow(iRow) ?? Sheet.CreateRow(iRow);
                for (int iCol = region.FirstColumn; iCol <= region.LastColumn; iCol++)
                {
                    var cell = row.GetCell(iCol) ?? row.CreateCell(iCol);
                    if (cell != null && cell.CellStyle != null)
                    {
                        if (firstCell)
                            style = cell.CellStyle;
                        else
                            if (style != cell.CellStyle)
                            {
                                sameStyle = false;
                                break;
                            }
                    }
                    else
                    {
                        sameStyle = false;
                        break;
                    }
                    firstCell = false;
                }
                if (!sameStyle) break;
            }
            if (sameStyle && style != null) 
                return style;*/

            var style = Workbook.CreateCellStyle();

            for (var iRow = region.FirstRow; iRow <= region.LastRow; iRow++)
            {
                var row = Sheet.GetRow(iRow) ?? Sheet.CreateRow(iRow);
                for (var iCol = region.FirstColumn; iCol <= region.LastColumn; iCol++)
                {
                    var cell = row.GetCell(iCol) ?? row.CreateCell(iCol);
                    if (cell != null) cell.CellStyle = style;
                }
            }

            return style;
        }

        protected CellStyle GetCellStyle()
        {
            return CurrentCell != null ? CreateCellStyle(CurrentCell) : null;
        }

        protected void SetCellStyle(Cell cell, CellStyle style)
        {
            cell.CellStyle = style;
        }

        protected void SetCellStyle(CellRangeAddress region, CellStyle style)
        {
            for (var iRow = region.FirstRow; iRow <= region.LastRow; iRow++)
            {
                var row = Sheet.GetRow(iRow) ?? Sheet.CreateRow(iRow);
                for (var iCol = region.FirstColumn; iCol <= region.LastColumn; iCol++)
                {
                    var cell = row.GetCell(iCol) ?? row.CreateCell(iCol);
                    if (cell != null) cell.CellStyle = style;
                }
            }
        }

        private Font GetFont(ContentStyle style)
        {
            var curFont = Workbook.GetFontAt(0);

            for(short i = 1; i < Workbook.NumberOfFonts; i++)
            {
                var font = Workbook.GetFontAt(i);

                if (style.HasFontName())
                {
                    if (!String.Equals(font.FontName, style.FontName, StringComparison.OrdinalIgnoreCase)) continue;
                }
                else
                    if (!String.Equals(font.FontName, curFont.FontName, StringComparison.OrdinalIgnoreCase)) continue;

                if (style.HasFontStyle())
                {
                    if (style.FontStyle.HasFlag(FontStyle.Italic) && !font.IsItalic) continue;
                    if (style.FontStyle.HasFlag(FontStyle.Bold) && font.Boldweight != (short)FontBoldWeight.BOLD)
                        continue;
                    if (style.FontStyle.HasFlag(FontStyle.Underline) && font.Underline != (byte) FontUnderlineType.SINGLE)
                        continue;
                    if (style.FontStyle == FontStyle.Regular &&
                        (font.IsItalic || font.Boldweight != (short)FontBoldWeight.NORMAL || font.Underline != (byte)FontUnderlineType.NONE))
                        continue;
                }
                if (style.FontDSize == 0 && curFont.FontHeightInPoints != font.FontHeightInPoints) continue;
                if (style.FontDSize != 0)
                {
                    var fs = curFont.FontHeightInPoints + style.FontDSize;
                    if (font.FontHeightInPoints != fs) continue;
                }
                if (style.HasFontColor() && style.FontColor != font.Color) continue;

                return font;
            }
            return Workbook.CreateFont();
        }

        private readonly IDictionary<ContentStyle, CellStyle> _styleDictionary =
            new Dictionary<ContentStyle, CellStyle>(new ContentStyleEqualityComparer());

        protected CellStyle GetStyle(ContentStyle style)
        {
            var root = GetRootParent();

            var dict = root._styleDictionary;
            //CellStyle cellStyle;
            return
                (from styleTemp in dict.Keys 
                 where styleTemp.CompareTo(style) == 0 
                 select dict[styleTemp]).FirstOrDefault();
        }

        private void AssignStyle(CellStyle cellStyle, ContentStyle style)
        {
            if (style.HasFontName() || style.HasFontStyle() || style.HasFontDSize() || style.HasFontColor())
            {
                var font = GetFont(style); //Workbook.CreateFont();  // cellStyle.GetFont(Workbook);

                if (style.HasFontName())
                    font.FontName = style.FontName;
                if (style.HasFontDSize())
                    font.FontHeightInPoints += style.FontDSize;
                if (style.HasFontStyle())
                {
                    if (style.FontStyle.HasFlag(FontStyle.Bold))
                        font.Boldweight = (short)FontBoldWeight.BOLD;
                    else
                        font.Boldweight = (short)FontBoldWeight.NORMAL;
                    font.IsItalic = style.FontStyle.HasFlag(FontStyle.Italic);
                    font.Underline = style.FontStyle.HasFlag(FontStyle.Underline)
                                         ? (byte)FontUnderlineType.SINGLE
                                         : (byte)FontUnderlineType.NONE;
                }
                if (style.HasFontColor())
                    font.Color = style.FontColor;

                cellStyle.SetFont(font);
            }
            if (style.HasBgColor())
            {
                cellStyle.FillForegroundColor = style.BgColor;
                cellStyle.FillPattern = FillPatternType.SOLID_FOREGROUND;
            }
            switch (style.HAlign)
            {
                case HAlignment.Left:
                    cellStyle.Alignment = HorizontalAlignment.LEFT;
                    break;
                case HAlignment.Right:
                    cellStyle.Alignment = HorizontalAlignment.RIGHT;
                    break;
                case HAlignment.Center:
                    cellStyle.Alignment = HorizontalAlignment.CENTER;
                    break;
            }
            switch (style.VAlign)
            {
                case VAlignment.Top:
                    cellStyle.VerticalAlignment = VerticalAlignment.TOP;
                    break;
                case VAlignment.Bottom:
                    cellStyle.VerticalAlignment = VerticalAlignment.BOTTOM;
                    break;
                case VAlignment.Middle:
                    cellStyle.VerticalAlignment = VerticalAlignment.CENTER;
                    break;
            }

            if (style.Borders.HasFlag(TableCellBorder.Top) /* ?? false*/)
            {
                cellStyle.BorderTop = CellBorderType.THIN;
                cellStyle.TopBorderColor = style.BorderColor; //HSSFColor.BLACK.index;
            }
            else
                cellStyle.BorderTop = CellBorderType.NONE;

            if (style.Borders.HasFlag(TableCellBorder.Left))
            {
                cellStyle.BorderLeft = CellBorderType.THIN;
                cellStyle.LeftBorderColor = style.BorderColor; //HSSFColor.BLACK.index;
            }
            else
                cellStyle.BorderLeft = CellBorderType.NONE;

            if (style.Borders.HasFlag(TableCellBorder.Right))
            {
                cellStyle.BorderRight = CellBorderType.THIN;
                cellStyle.RightBorderColor = style.BorderColor; //HSSFColor.BLACK.index;
            }
            else
                cellStyle.BorderRight = CellBorderType.NONE;

            if (style.Borders.HasFlag(TableCellBorder.Bottom))
            {
                cellStyle.BorderBottom = CellBorderType.THIN;
                cellStyle.BottomBorderColor = style.BorderColor; //HSSFColor.BLACK.index;
            }
            else
                cellStyle.BorderBottom = CellBorderType.NONE;

            if (style.WrapText ?? false)
                cellStyle.WrapText = (bool)style.WrapText;
        }

        protected void SetStyle(CellStyle cellStyle, ContentStyle style)
        {
            AssignStyle(cellStyle, style);

            var root = GetRootParent();
            //var styleTemp = new ContentStyle(style);
            root._styleDictionary.Add(new ContentStyle(style), cellStyle);
        }

        /*private void DrawBorders(TableCellBorder borders)
        {
            var style = Style;
            style.Borders = borders;
            SetStyle(style);
        }*/

        public void AddCell(int colSpan = 0, int rowSpan = 0)
        {
            if (CurrentRow == null) AddRow(); //CurrentRow = Sheet.CreateRow(CurrentRowIndex);

            CurrentColIndex = NextColIndex++;
            CurrentCell = CurrentRow.GetCell(CurrentColIndex) ?? CurrentRow.CreateCell(CurrentColIndex);
            var regionColIndex = CurrentColIndex + (colSpan > 1 ? colSpan - 1 : 0);
            var regionRowIndex = CurrentRowIndex + (rowSpan > 1 ? rowSpan - 1 : 0);

            CellRangeAddress region = null;
            if (colSpan > 1 || rowSpan > 1)
            {
                region = new CellRangeAddress(CurrentRowIndex, regionRowIndex, CurrentColIndex, regionColIndex);
                Sheet.AddMergedRegion(region);

                if (colSpan > 1)
                    NextColIndex += colSpan - 1;
                if (rowSpan > 1)
                    NextRowIndex = Math.Max(NextRowIndex, regionRowIndex + 1);
            }
            // Style the cell with borders all around.
//            if (Style.HasValues()/* || ((int)Borders) != 0*/)
//                (BorderTop ?? false) || (BorderLeft ?? false) || (BorderRight ?? false) || (BorderBottom ?? false))
            {
                //var style = Workbook.CreateCellStyle();
                var style = GetStyle(Style);

                if (style == null)
                {
                    style = region == null ? CreateCellStyle(CurrentCell) : CreateCellStyle(region);
                    SetStyle(style, Style);
                }

                if (region == null)
                    SetCellStyle(CurrentCell, style);
                else
                    SetCellStyle(region, style);
/*
                if (Borders.HasFlag(TableCellBorder.Top)/* ?? false♥1♥)
                {
                    style.BorderTop = CellBorderType.THIN;
                    style.TopBorderColor = HSSFColor.BLACK.index;
                }
                if (Borders.HasFlag(TableCellBorder.Left))
                {
                    style.BorderLeft = CellBorderType.THIN;
                    style.LeftBorderColor = HSSFColor.BLACK.index;
                }
                if (Borders.HasFlag(TableCellBorder.Right))
                {
                    style.BorderRight = CellBorderType.THIN;
                    style.RightBorderColor = HSSFColor.BLACK.index;
                }
                if (Borders.HasFlag(TableCellBorder.Bottom))
                {
                    style.BorderBottom = CellBorderType.THIN;
                    style.BottomBorderColor = HSSFColor.BLACK.index;
                }
*/
//                style.WrapText = WrapText;
//                style.ShrinkToFit = ShrinkToFit;

                /*if (region == null)
                    CurrentCell.CellStyle = style;
                else
                {
                    for (int iRow = region.FirstRow; iRow <= region.LastRow; iRow++)
                    {
                        var row = Sheet.GetRow(iRow) ?? Sheet.CreateRow(iRow);
                        for (int iCol = region.FirstColumn; iCol <= region.LastColumn; iCol++)
                        {
                            var cell = row.GetCell(iCol) ?? row.CreateCell(iCol);
                            if (cell != null) cell.CellStyle = style;
                        }
                    }
                }*/
            }
        }

        public void SetValue(bool value)
        {
            CheckCell().SetCellValue(value);
            if (Style.AutoWidth ?? false)
                AutoWidthColumn(CurrentCell.ColumnIndex);
        }

        public void SetValue(int value)
        {
            CheckCell().SetCellValue(value);
            if (Style.AutoWidth ?? false)
                AutoWidthColumn(CurrentCell.ColumnIndex);
        }

        protected XlsWriter GetRootParent()
        {
            if (Parent == null) return this;
            var p = Parent;
            while (p.Parent != null)
            {
                p = p.Parent;
            }
            return p;
        }

        private CellStyle _dateTimeStyle;
        protected CellStyle GetDateTimeStyle()
        {
            if (Parent != null)
            {
                var style = Parent.GetDateTimeStyle();
                if (style != null) return style;
            }

            if (_dateTimeStyle != null) return _dateTimeStyle;

            _dateTimeStyle = Workbook.CreateCellStyle();
            var format = Workbook.CreateDataFormat();
            _dateTimeStyle.DataFormat = format.GetFormat("DD.MM.YYYY");
            GetRootParent()._dateTimeStyle = _dateTimeStyle;
            return _dateTimeStyle;
        }

        public void SetValue(DateTime value)
        {
            var cell = CheckCell();
            cell.SetCellValue(value);
            var style = GetDateTimeStyle();
            AssignStyle(style, Style);
                /*cell.CellStyle ?? Workbook.CreateCellStyle();
            var format = Workbook.CreateDataFormat();
            style.DataFormat = format.GetFormat("DD.MM.YYYY");*/
            cell.CellStyle = style;
            if (Style.AutoWidth ?? false)
                AutoWidthColumn(CurrentCell.ColumnIndex);
        }

        public void SetValue(string value)
        {
            CheckCell().SetCellValue(value);
            if (Style.AutoWidth ?? false)
                AutoWidthColumn(CurrentCell.ColumnIndex);
        }

        public void SetValue(double value)
        {
            CheckCell().SetCellValue(value);
            if (Style.AutoWidth ?? false)
                AutoWidthColumn(CurrentCell.ColumnIndex);
        }

        public void SetValue(object value)
        {
            if (value != null)
                CheckCell().SetCellValue(value.ToString());
//            Console.WriteLine(String.Format("Row: {0}; Col: {1}", CurrentRowIndex, CurrentColIndex));
            if (Style.AutoWidth ?? false)
                AutoWidthColumn(CurrentCell.ColumnIndex);
        }

        private Cell CheckCell()
        {
            if (CurrentCell == null)
                throw new ApplicationException("Не могу записать значение! Ячейка не добавлена!");
            return CurrentCell;
        }

        /*public void SetBorder(bool? top, bool? left, bool? right, bool? bottom)
        {
            if (top != null) Borders = TableCellBorder.Top;
            if (left != null) Borders = TableCellBorder.Left;
            if (right != null) Borders = TableCellBorder.Right;
            if (bottom != null) Borders = TableCellBorder.Bottom;
        }*/

        /*public void SetBorder(TableCellBorder borders)
        {
            Borders = borders;
        }*/

        public void AutoWidthColumn()
        {
            if (CurrentCell != null)
            {
                AutoWidthColumn(CurrentCell.ColumnIndex);
            }
        }
        public void AutoHeightRow()
        {
            if (CurrentCell != null)
            {
                AutoHeightRow(CurrentCell.RowIndex);
            }
        }

        private List<int> _autoWidthColumns;
        private List<int> _autoHeightRows;

        public void AutoWidthColumn(int colIndex)
        {
            if (Parent != null)
                Parent.AutoWidthColumn(colIndex);
            else
            {
                if (_autoWidthColumns == null)
                    _autoWidthColumns = new List<int>();

                if (!_autoWidthColumns.Contains(colIndex))
                    _autoWidthColumns.Add(colIndex);
            }
        }
        public void AutoHeightRow(int rowIndex)
        {
            if (Parent != null)
                Parent.AutoHeightRow(rowIndex);
            else
            {
                if (_autoHeightRows == null)
                    _autoHeightRows = new List<int>();

                if (!_autoHeightRows.Contains(rowIndex))
                    _autoHeightRows.Add(rowIndex);
            }
        }

        public void Dispose()
        {
            if (Parent != null)
            {
                // Установить размеры родителя
                if (Parent.NextColIndex < NextColIndex) Parent.NextColIndex = NextColIndex;
                if (Parent.NextRowIndex < NextRowIndex) Parent.NextRowIndex = NextRowIndex;
            }
        }

        public ContentStyle SetStyle(ContentStyle style)
        {
            var oldStyle = new ContentStyle(Style);
            Style.Assign(style);
            /*if (CurrentCell != null)
            {
                var cellStyle = GetStyle(Style);
                if (cellStyle == null)
                {
                    cellStyle = GetCellStyle();
                    SetStyle(cellStyle, Style);
                }
                CurrentCell.CellStyle = cellStyle;
            }*/
            return oldStyle;
        }

        public ContentStyle MergeStyle(ContentStyle style)
        {
            var oldStyle = new ContentStyle(Style);
            Style.Merge(style);
            /*if (CurrentCell != null)
            {
                var cellStyle = GetStyle(Style);
                if (cellStyle == null)
                {
                    cellStyle = GetCellStyle();
                    SetStyle(cellStyle, Style);
                }
                CurrentCell.CellStyle = cellStyle;
            }*/
            return oldStyle;
        }

        public void ApplyColumnAutoSize()
        {
            if (_autoWidthColumns != null)
            {
                foreach (var column in _autoWidthColumns)
                {
                    Sheet.AutoSizeColumn(column, true);
                }
            }
            if (_autoHeightRows != null)
                foreach (var rowIndex in _autoHeightRows)
                {
                    var row = Sheet.GetRow(rowIndex);
                    if (row != null)
                        row.Height = 0;
                }
        }

        public void SetColumnWidth(int width)
        {
            if (CurrentCell != null)
            {
                SetColumnWidth(CurrentCell.ColumnIndex, width);
            }
        }

        public void SetColumnWidth(int columnIndex, int width)
        {
            if (Parent != null)
                Parent.SetColumnWidth(columnIndex, width);
            else
            {
                Sheet.SetColumnWidth(columnIndex, width);
            }
        }
    }
}
