using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace Intersoft.CISSA.DataAccessLayer.Report
{
    public class ExcelReport : IExcelReport
    {
        public ExcelReport(string mTitle, string mSubject, string sheetName)
        {
            Hssfworkbook = new HSSFWorkbook();
            Sheet = (HSSFSheet) Hssfworkbook.CreateSheet(sheetName);
            CurrentRowIndex = -1;

            Styels = ExcelStyles.CreateDefaultStyels(Hssfworkbook);

            ////create a entry of DocumentSummaryInformation
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
//            dsi.Company = Resources.CompanyName;
            //dsi.Manager = "Manager";
            Hssfworkbook.DocumentSummaryInformation = dsi;

            ////create a entry of SummaryInformation
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Title = mTitle;
            si.Subject = mSubject;
//            si.ApplicationName = Resources.ReportApplicationName;
            si.CreateDateTime = DateTime.Now;
            si.EditTime = 0;
            si.LastSaveDateTime = si.CreateDateTime;
//            si.LastAuthor = Resources.ReportApplicationName;
            Hssfworkbook.SummaryInformation = si;

            // настройки по-умолчанию
            Sheet.PrintSetup.PaperSize = (short) PaperSizeType.A4;

            Sheet.FitToPage = false;
            Sheet.PrintSetup.FooterMargin = 0.25;
            Sheet.PrintSetup.HeaderMargin = 0.25;
            Sheet.PrintSetup.Scale = 100;
            Sheet.PrintSetup.FitHeight = 0;
            Sheet.PrintSetup.FitWidth = 0;
            Sheet.PrintSetup.CellError = DisplayCellErrorType.ErrorAsBlank;

            // параметры полей страницы по-умолчанию
            Sheet.SetMargin(MarginType.LeftMargin, 0.8);
            Sheet.SetMargin(MarginType.RightMargin, 0.3);
            Sheet.SetMargin(MarginType.TopMargin, 0.8);
            Sheet.SetMargin(MarginType.BottomMargin, 0.8);
        }

        private HSSFWorkbook Hssfworkbook { get; set; }
        private HSSFSheet Sheet { get; set; }
        private Dictionary<TextStyle, CellStyle> Styels { get; set; }

        private int NextRowIndex
        {
            get { return ++CurrentRowIndex; }
        }

        private Row NextRow
        {
            get
            {
                CurrentRow = Sheet.CreateRow(NextRowIndex);
                return CurrentRow;
            }
        }

        
        public int CurrentRowIndex { get; private set; }
        public Row CurrentRow { get; private set; }

        public Row GetNextRow()
        {
            return NextRow;
        }

        public MemoryStream ExcelGetMemoryStream()
        {
            //Write the stream data of workbook to the root directory
            var file = new MemoryStream();
            Hssfworkbook.Write(file);
            return file;
        }

        public byte[] ExcelGetBytes()
        {
            return ExcelGetMemoryStream().ToArray();
        }

        public void ExcelSaveToFile(String path)
        {
            //Write the stream data of workbook to the root directory
            var file = new FileStream(path, FileMode.Create);
            Hssfworkbook.Write(file);

            file.Close();
        }

        public void SetColumnWidth(int columnIndex, float excelColumWidth)
        {
            Sheet.SetColumnWidth(columnIndex, (int) (256*excelColumWidth));
        }

        public void SetRowHeight(Row row, short excelHeight)
        {
            row.Height = (short) (excelHeight*20);
        }

        public void MergeRegion(int firstRow, int lastRow, int firstCol, int lastCol)
        {
            var range = new CellRangeAddress(firstRow, lastRow, firstCol, lastCol);
            Sheet.AddMergedRegion(range);
        }

        public void AddEmptyRow()
        {
            CurrentRow = NextRow;
        }

        public void AddEmptyRows(int countEmptyRows)
        {
            for (int i = 0; i < countEmptyRows; i++)
            {
                AddEmptyRow();
            }
        }

        
        public Cell AddCell(Object value, int columnIndex, Row row, TextStyle textStyle, short backgroundColor,
                            CellBorderType borderStyle, short borderColor)
        {
            Cell cell = row.GetCell(columnIndex) ?? row.CreateCell(columnIndex);

            CellStyle cellStyle = Styels[textStyle];

            if (value == null)
            {
                cell.SetCellValue("");
            }
            else if (value.GetType() == typeof(DateTime))
            {
                cell.SetCellValue((DateTime)value);
                cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy");
            }
            else if (value.GetType() == typeof(double) ||
                     value.GetType() == typeof(float) ||
                     value.GetType() == typeof(decimal))
            {
                cell.SetCellValue((double)value);

                DataFormat format = Hssfworkbook.CreateDataFormat();
                cellStyle.DataFormat = format.GetFormat("# ### ### ##0.00");
            }
            else if (value.GetType() == typeof(Int16) ||
                     value.GetType() == typeof(Int32) ||
                     value.GetType() == typeof(Int64) ||
                     value.GetType() == typeof(UInt16) ||
                     value.GetType() == typeof(UInt32) ||
                     value.GetType() == typeof(UInt64) ||
                     value.GetType() == typeof(Byte) ||
                     value.GetType() == typeof(SByte))
            {
                cell.SetCellValue((int)value);
                cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("0");
            }
            else if (value.GetType() == typeof(bool))
            {
                cell.SetCellValue((bool)value);
            }
            else
            {
                cell.SetCellValue(value.ToString());
            }

            if (backgroundColor != short.MinValue)
            {
                cellStyle.FillForegroundColor = backgroundColor;
                cellStyle.FillPattern = FillPatternType.SOLID_FOREGROUND;
            }

            if (borderColor != short.MinValue)
            {
                cellStyle.BorderBottom = borderStyle;
                cellStyle.BorderLeft = borderStyle;
                cellStyle.BorderRight = borderStyle;
                cellStyle.BorderTop = borderStyle;

                cellStyle.BottomBorderColor = borderColor;
                cellStyle.LeftBorderColor = borderColor;
                cellStyle.RightBorderColor = borderColor;
                cellStyle.TopBorderColor = borderColor;
            }

            cell.CellStyle = cellStyle;

            return cell;
        }

        public void AddLogo(int rowNumber, int columnIndex)
        {
            var stream = new MemoryStream();
            Resources.logo.Save(stream, ImageFormat.Png);

            int logoId = Hssfworkbook.AddPicture(stream.ToArray(), PictureType.PNG);

            Drawing patriarch = Sheet.CreateDrawingPatriarch();

            // create the anchor
            // var anchor = new HSSFClientAnchor();
            // anchor.AnchorType = 2;
            //var anchor = //new HSSFClientAnchor(0, 0, 1023, 255, columnIndex, rowNumber, columnIndex, rowNumber);
            var anchor = new HSSFClientAnchor
                             {
                                 Dx1 = 0,
                                 Dx2 = 0,
                                 Dy1 = 0,
                                 Dy2 = 0,
                                 AnchorType = 3,
                                 Row1 = rowNumber,
                                 Row2 = rowNumber,
                                 Col1 = columnIndex,
                                 Col2 = columnIndex
                             };


            //load the picture and get the picture index in the workbook
            Picture picture = patriarch.CreatePicture(anchor, logoId);

            //patriarch.CreatePicture(anchor, logoId);

            //Reset the image to the original size.
            picture.Resize();

            /*
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[file.Length];
            file.Read(buffer, 0, (int)file.Length);
            return wb.AddPicture(buffer, HSSFWorkbook.PICTURE_TYPE_JPEG);
            */
        }

        public void AddList<T>(IEnumerable<T> dataList, IEnumerable<DataColumn> columns, CellBorderType borderStyle,
                               short borderColor)
        {
            //PropertyInfo[] propertys = typeof (T).GetProperties(); 

            // шапка таблицы
            Row tableHederRow = NextRow;
            int index = 0;
            foreach (DataColumn column in columns)
            {
                if (column.ExcelColumWidth >= 0)
                    Sheet.SetColumnWidth(index, (int) (256*column.ExcelColumWidth));

                AddCell(column.FieldAlias, index++, tableHederRow, TextStyle.BoldTextAlignCenter,
                        HSSFColor.GREY_25_PERCENT.index, borderStyle, borderColor);
            }

            // заполнение данными
            int listRowIndex = 1;
            foreach (T cls in dataList)
            {
                int indexData = 0;
                Row dataRow = NextRow;
                foreach (DataColumn column in columns)
                {
                    if (column.IsRowNumberColumn)
                    {
                        AddCell(listRowIndex++, indexData++, dataRow, TextStyle.NormalText, HSSFColor.WHITE.index,
                                borderStyle, borderColor);
                    }
                    else
                    {
                        PropertyInfo property = typeof (T).GetProperty(column.FieldName);
                        object value = property.GetValue(cls, null);
                        AddCell(value, indexData++, dataRow, TextStyle.NormalText, HSSFColor.WHITE.index, borderStyle,
                                borderColor);
                    }
                }
            }
        }

        public void AddList<T>(IEnumerable<T> dataList, IEnumerable<DataColumn> columns)
        {
            // шапка таблицы
            Row tableHederRow = NextRow;
            int index = 0;
            foreach (DataColumn column in columns)
            {
                if (column.ExcelColumWidth >= 0)
                    Sheet.SetColumnWidth(index, (int) (256*column.ExcelColumWidth));

                AddCell(column.FieldAlias, index++, tableHederRow, TextStyle.BoldTextAlignCenter, short.MinValue, CellBorderType.NONE, short.MinValue);
            }

            // заполнение данными
            int listRowIndex = 1;
            foreach (T cls in dataList)
            {
                int indexData = 0;
                Row dataRow = NextRow;
                foreach (DataColumn column in columns)
                {
                    if (column.IsRowNumberColumn)
                    {
                        AddCell(listRowIndex++, indexData++, dataRow, TextStyle.NormalText, short.MinValue, CellBorderType.NONE, short.MinValue);
                    }
                    else
                    {
                        PropertyInfo property = typeof (T).GetProperty(column.FieldName);
                        object value = property.GetValue(cls, null);
                        AddCell(value, indexData++, dataRow, TextStyle.NormalText, short.MinValue, CellBorderType.NONE, short.MinValue);
                    }
                }
            }
        }

        public void AddList<T>(IEnumerable<T> dataList)
        {
            PropertyInfo[] propertys = typeof (T).GetProperties();

            // шапка таблицы
            Row tableHederRow = NextRow;
            int index = 0;
            foreach (PropertyInfo property in propertys)
            {
                AddCell(property.Name, index++, tableHederRow, TextStyle.BoldTextAlignCenter,
                        HSSFColor.GREY_25_PERCENT.index, CellBorderType.THIN, HSSFColor.GREY_80_PERCENT.index);
            }

            // заполнение данными
            foreach (T cls in dataList)
            {
                int indexData = 0;
                Row dataRow = NextRow;
                foreach (PropertyInfo property in cls.GetType().GetProperties())
                {
                    AddCell(property.GetValue(cls, null), indexData++, dataRow, TextStyle.NormalText,
                            HSSFColor.WHITE.index, CellBorderType.THIN, HSSFColor.GREY_80_PERCENT.index);
                }
            }

            // авто ширина заполненых столбцов
            for (int i = 0; i < index; i++)
            {
                Sheet.AutoSizeColumn(i);
                // Гребанс ошибко! NPOI не правильно устанавливает автоширину столбцов.
                int oldColumnWidth = Sheet.GetColumnWidth(i);
                Sheet.SetColumnWidth(i, (int) (oldColumnWidth*2.25));
            }
        }

        public void AddBusinesObject<T>(T businesObject)
        {
            PropertyInfo[] propertys = typeof (T).GetProperties();

            foreach (PropertyInfo property in propertys)
            {
                AddCell(property.Name, 0, NextRow, TextStyle.BoldTextAlignCenter, HSSFColor.GREY_25_PERCENT.index,
                        CellBorderType.THIN, HSSFColor.GREY_80_PERCENT.index);
                AddCell(property.GetValue(businesObject, null).ToString(), 1, NextRow, TextStyle.NormalText,
                        HSSFColor.WHITE.index, CellBorderType.THIN, HSSFColor.GREY_80_PERCENT.index);
            }
        }

        public void AutoHeightMultiLineRow(Cell cell, int cntSymbolsInRow, int lineHeight)
        {
            string str = cell.StringCellValue;

            float cntLines = (float) str.Length/cntSymbolsInRow;

            if (cntLines < 0.90)
                cntLines = 0;

            var height = (short) ((cntLines)*lineHeight + lineHeight);

            SetRowHeight(cell.Row, height);
        }

        /// <summary>
        /// Добавляет верхний колонтитул страницы
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="position">Позиция</param>
        public void AddPageHeader(string text, Position position)
        {
            switch (position)
            {
                case Position.Left:
                    Sheet.Header.Left = text;
                    break;

                case Position.Right:
                    Sheet.Header.Right = text;
                    break;

                case Position.Centre:
                    Sheet.Header.Center = text;
                    break;
            }
        }

        /// <summary>
        /// Добавляет нижний колонтитул страницы
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="position">Позиция</param>
        public void AddPageFooter(string text, Position position)
        {
            switch (position)
            {
                case Position.Left:
                    Sheet.Footer.Left = text;
                    break;

                case Position.Right:
                    Sheet.Footer.Right = text;
                    break;

                case Position.Centre:
                    Sheet.Footer.Center = text;
                    break;
            }
        }

        public Row GetRow(int rowIndex)
        {
            if (CurrentRowIndex < rowIndex)
            {
                AddEmptyRows(rowIndex - CurrentRowIndex);
            }
            return Sheet.GetRow(rowIndex);
        }

        public Cell AddCell(object value, int columnIndex, int rowIndex, TextStyle textStyle,
            short backgroundColor = short.MinValue, CellBorderType borderStyle = CellBorderType.NONE, short borderColor = short.MinValue)
        {
            Row row = GetRow(rowIndex);

            return AddCell(value, columnIndex, row, textStyle, backgroundColor, borderStyle, borderColor);
        }
    }
}