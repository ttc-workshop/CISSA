using System;
using System.Data;

namespace Intersoft.Cissa.Report.Common
{
    public class DataTableDataSet : DataSet
    {
        public DataTable Table { get; private set; }

        public int Index { get; private set; }

        public DataTableDataSet(DataTable table)
        {
            Table = table;
        }

        public override bool Eof()
        {
            return Table.Rows.Count <= Index;
        }

        public override void Next()
        {
            Index++;
        }

        public override void Reset()
        {
            Index = 0;
            _current = null;
        }

        public override bool HasField(string fieldName)
        {
            return Table.Columns.Contains(fieldName);
        }

        public override int GetRecordNo()
        {
            return Index;
        }

        public override DataSetField CreateField(string fieldName)
        {
            return new DataTableDataSetField(this, fieldName);
        }

        private DataRow _current;
        public DataRow GetCurrent()
        {
            if (_current == null && Index < Table.Rows.Count)
                _current = Table.Rows[Index];
            return _current;
        }
    }

    public class DataTableDataSetField: DataSetField
    {
        public string FieldName { get; private set; }
        public DataTableDataSetField(DataSet dataSet, string fieldName)
            : base(dataSet)
        {
            FieldName = fieldName;
        }

        public override object GetValue()
        {
            var tableSet = (DataSet as DataTableDataSet);
            if (tableSet != null)
            {
                var data = tableSet.GetCurrent();
                if (data != null)
                    return data[FieldName];
            }
            return String.Empty;
        }
    }
}