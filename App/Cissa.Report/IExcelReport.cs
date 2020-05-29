using System;
using System.Collections.Generic;
using NPOI.SS.UserModel;

namespace Intersoft.Cissa.Report
{
    public interface IExcelReport : IExcelable
    {
        int CurrentRowIndex { get; }
        Row CurrentRow { get; }
        Row GetNextRow();
        void SetColumnWidth(int columnIndex, float excelColumWidth);
        void MergeRegion(int firstRow, int lastRow, int firstCol, int lastCol);
        void AddEmptyRow();
        void AddEmptyRows(int countEmptyRows);

        Cell AddCell(Object value, int columnIndex, Row row, TextStyle textStyle, short backgroundColor = short.MinValue,
                     CellBorderType borderStyle = CellBorderType.NONE, short borderColor = short.MinValue);

        Row GetRow(int rowIndex);

        Cell AddCell(object value, int columnIndex, int rowIndex, TextStyle textStyle,
                     short backgroundColor = short.MinValue, CellBorderType borderStyle = CellBorderType.NONE,
                     short borderColor = short.MinValue);

        void AddLogo(int rowNumber, int columnIndex);

        void AddList<T>(IEnumerable<T> dataList, IEnumerable<DataColumn> columns, CellBorderType borderStyle,
                        short borderColor);

        void AddList<T>(IEnumerable<T> dataList, IEnumerable<DataColumn> columns);
        void AddList<T>(IEnumerable<T> dataList);
        void AddBusinesObject<T>(T businesObject);
        void SetRowHeight(Row row, short excelHeight);
        void AutoHeightMultiLineRow(Cell cell, int cntSymbolsInRow, int lineHeight);
        
        /// <summary>
        /// Добавляет верхний колонтитул страницы
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="position">Позиция</param>
        void AddPageHeader(String text, Position position);
        
        /// <summary>
        /// Добавляет нижний колонтитул страницы
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="position">Позиция</param>
        void AddPageFooter(String text, Position position);
    }
}