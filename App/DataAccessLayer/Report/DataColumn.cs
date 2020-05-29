using System;

namespace Intersoft.CISSA.DataAccessLayer.Report
{
    public class DataColumn
    {
        private string _fieldName;
        private bool _rowNumberColumn;

        public DataColumn()
        {
        }

        public DataColumn(string mFieldName, string mFieldAlias, double mExcelColumnWidth)
        {
            FieldName = mFieldName;
            FieldAlias = mFieldAlias;
            ExcelColumWidth = mExcelColumnWidth;
        }

        public DataColumn(string mFieldName, string mFieldAlias)
        {
            FieldName = mFieldName;
            FieldAlias = mFieldAlias;
            ExcelColumWidth = -1;
        }

        public string FieldName
        {
            get { return _fieldName; }
            set
            {
                _fieldName = value;
                if (String.IsNullOrEmpty(FieldAlias)) FieldAlias = _fieldName;
            }
        }

        public string FieldAlias { get; set; }
        public double ExcelColumWidth { get; set; }

        public bool IsRowNumberColumn
        {
            get { return _rowNumberColumn; }
        }

        public static DataColumn GetRowNumberColumn(string mFieldAlias, double mExcelColumnWidth)
        {
            var rowColumn = new DataColumn(
                "DATA_ROW_NUMBER",
                mFieldAlias,
                mExcelColumnWidth) {_rowNumberColumn = true};
            return rowColumn;
        }

        public static DataColumn GetRowNumberColumn(string mFieldAlias)
        {
            var rowColumn = new DataColumn("DATA_ROW_NUMBER", mFieldAlias, -1);
            rowColumn._rowNumberColumn = true;
            return rowColumn;
        }
    }
}