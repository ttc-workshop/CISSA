using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Intersoft.Cissa.Report.Common;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.Cissa.Report.Utils
{
    public class CrossDataTable
    {
        private readonly List<CrossDataRow> _rows = new List<CrossDataRow>();
        public List<CrossDataRow> Rows { get { return _rows; } }

        private readonly List<CrossDataColumn> _columns = new List<CrossDataColumn>();
        public List<CrossDataColumn> Columns { get { return _columns; } }

        public void AddColumn(CrossDataColumn column)
        {
            _columns.Add(column);
        }

        public void Fill(SqlQueryReader reader)
        {
            if (!reader.Active) reader.Open();

            Fill(reader.Reader);
        }

        public void Fill(IDataReader reader)
        {
            while (reader.Read())
            {
                var row = new CrossDataRow();
                foreach (var column in Columns.OfType<CrossDataKeyColumn>())
                {
                    var value = reader.IsDBNull(column.Key) ? null : reader.GetValue(column.Key);
                    row.AddValue(column, value);
                }

                var existsRow = Find(row);
                if (existsRow != null) row = existsRow;
                else
                    Rows.Add(row);

                var i = 0;
                CrossDataGroupColumnValue columnValue = null;
                foreach (var column in Columns.OfType<CrossDataGroupColumn>())
                {
                    var value = reader.IsDBNull(column.Key) ? null : reader.GetValue(column.Key);

                    var currColumnValue = i == 0 ? column.GetValue(value) : column.GetValue(columnValue, value);
                    // row.AddValue(column, columnValue, value);
                    columnValue = currColumnValue;
                    i++;
                }

                foreach (var column in Columns.OfType<CrossDataFuncColumn>())
                {
                    var value = reader.IsDBNull(column.Key) ? null : reader.GetValue(column.Key);

                    row.AddValue(column, columnValue, value);
                }
            }
        }

        public void Fill(SqlQueryDataSet dataSet, Dictionary<CrossDataColumn, SqlQueryDataSetField> columnFieldMaps)
        {
            while (!dataSet.Eof())
            {
                var row = new CrossDataRow();
                foreach (var column in Columns.OfType<CrossDataKeyColumn>())
                {
                    var field = columnFieldMaps[column];
                    var value = field.GetValue();
                    row.AddValue(column, value);
                }

                var existsRow = Find(row);
                if (existsRow != null) row = existsRow;
                else
                    Rows.Add(row);

                var i = 0;
                CrossDataGroupColumnValue columnValue = null;
                foreach (var column in Columns.OfType<CrossDataGroupColumn>())
                {
                    var field = columnFieldMaps[column];
                    var value = field.GetValue();

                    var currColumnValue = i == 0 ? column.GetValue(value) : column.GetValue(columnValue, value);
                    // row.AddValue(column, columnValue, value);
                    columnValue = currColumnValue;
                    i++;
                }

                foreach (var column in Columns.OfType<CrossDataFuncColumn>())
                {
                    var field = columnFieldMaps[column];
                    var value = field.GetValue();

                    row.AddValue(column, columnValue, value);
                }

                dataSet.Next();
            }
        }

        public CrossDataRow Find(CrossDataRow aRow)
        {
            foreach (var row in Rows)
            {
                var b = true;
                foreach (var column in Columns.OfType<CrossDataKeyColumn>())
                {
                    var val1 = row.GetValue(column);
                    var val2 = aRow.GetValue(column);

                    if (val1 == null && val2 == null) continue;
                    if (val1 == null || val2 == null)
                    {
                        b = false;
                        break;
                    }
                    if (val1.ValueEquals(val2)) continue;
                    b = false;
                    break;
                }
                if (b) return row;
            }
            return null;
        }

        public IEnumerable<CrossDataColumnItem> ColumnItems()
        {
            foreach (var column in Columns.OfType<CrossDataKeyColumn>())
            {
                yield return new CrossDataColumnItem(column);
            }
            var lastGroup = Columns.OfType<CrossDataGroupColumn>().LastOrDefault();
            if (lastGroup != null)
            {
                foreach (var val in lastGroup.Values)
                {
                    foreach (var column in Columns.OfType<CrossDataFuncColumn>())
                    {
                        yield return new CrossDataColumnItem(column, val);
                    }
                }
            }
        }

        public IEnumerable<object> GetRowColumnDatas(CrossDataRow row)
        {
            return
                ColumnItems()
                    .Select(
                        colItem =>
                            colItem.ColumnValues != null && colItem.ColumnValues.Count > 0
                                ? row.Find(colItem.Column, colItem.ColumnValues[0])
                                : row.Find(colItem.Column))
                    .Select(rowValue => rowValue != null ? rowValue.Value : null);
        }

        public IEnumerable<object> GetColumnSummaries()
        {
            var summaries = new List<object>();

            foreach (var colItem in ColumnItems())
            {
                if (colItem.ColumnValues == null || colItem.ColumnValues.Count == 0)
                {
                    summaries.Add(null);
                }
                else
                {
                    var column = colItem.Column as CrossDataFuncColumn;
                    var func = column != null ? column.Function : SqlQuerySummaryFunction.None;

                    var s = 0.0;
                    var dt = DateTime.MinValue;
                    var valCount = 0;
                    var isDateTime = false;
                    if (func != SqlQuerySummaryFunction.None && func != SqlQuerySummaryFunction.Group)
                        foreach (var row in Rows)
                        {
                            var rowValue = row.Find(colItem.Column, colItem.ColumnValues[0]);
                            if (rowValue != null)
                            {
                                if (func == SqlQuerySummaryFunction.Sum || func == SqlQuerySummaryFunction.Count ||
                                    func == SqlQuerySummaryFunction.Avg)
                                {
                                    if (rowValue.Value is int)
                                    {
                                        s += (int) rowValue.Value;
                                        valCount++;
                                    }
                                    else if (rowValue.Value is double)
                                    {
                                        s += (double) rowValue.Value;
                                        valCount++;
                                    }
                                    else if (rowValue.Value is decimal)
                                    {
                                        s += Convert.ToDouble((decimal) rowValue.Value);
                                        valCount++;
                                    }
                                }
                                else if (func == SqlQuerySummaryFunction.Min)
                                {
                                    if (rowValue.Value is int)
                                    {
                                        if (valCount == 0 || s > (int) rowValue.Value) s = (int) rowValue.Value;
                                        valCount++;
                                    }
                                    else if (rowValue.Value is double)
                                    {
                                        if (valCount == 0 || s > (double) rowValue.Value) s = (double) rowValue.Value;
                                        valCount++;
                                    }
                                    else if (rowValue.Value is decimal)
                                    {
                                        if (valCount == 0 || s > Convert.ToDouble((decimal) rowValue.Value))
                                            s = Convert.ToDouble((decimal) rowValue.Value);
                                        valCount++;
                                    }
                                    else if (rowValue.Value is DateTime)
                                    {
                                        if (valCount == 0 || dt > (DateTime) rowValue.Value)
                                            dt = (DateTime) rowValue.Value;
                                        isDateTime = true;
                                        valCount++;
                                    }
                                }
                                else if (func == SqlQuerySummaryFunction.Max)
                                {
                                    if (rowValue.Value is int)
                                    {
                                        if (valCount == 0 || s < (int) rowValue.Value) s = (int) rowValue.Value;
                                        valCount++;
                                    }
                                    else if (rowValue.Value is double)
                                    {
                                        if (valCount == 0 || s < (double) rowValue.Value) s = (double) rowValue.Value;
                                        valCount++;
                                    }
                                    else if (rowValue.Value is decimal)
                                    {
                                        if (valCount == 0 || s < Convert.ToDouble((decimal) rowValue.Value))
                                            s = Convert.ToDouble((decimal) rowValue.Value);
                                        valCount++;
                                    }
                                    else if (rowValue.Value is DateTime)
                                    {
                                        if (valCount == 0 || dt < (DateTime) rowValue.Value)
                                            dt = (DateTime) rowValue.Value;
                                        isDateTime = true;
                                        valCount++;
                                    }
                                }
                            }
                        }

                    if (valCount > 0)
                    {
                        if (func == SqlQuerySummaryFunction.Avg)
                            summaries.Add(s / valCount);
                        else if (func == SqlQuerySummaryFunction.Sum || func == SqlQuerySummaryFunction.Count) 
                            summaries.Add(s);
                        else if (func == SqlQuerySummaryFunction.Min || func == SqlQuerySummaryFunction.Max)
                        {
                            if (isDateTime)
                                summaries.Add(dt);
                            else 
                                summaries.Add(s);
                        }
                        else 
                            summaries.Add(null);
                    }
                    else summaries.Add(null);
                }
            }

            return summaries;
        }
    }

    public class CrossDataColumnItem
    {
        public string Caption { get; private set; }
        public CrossDataColumn Column { get; private set; }
        public List<CrossDataGroupColumnValue> ColumnValues { get; private set; }

        public CrossDataColumnItem(CrossDataKeyColumn column)
        {
            Caption = column.Caption;
            Column = column;
            ColumnValues = null;
        }

        public CrossDataColumnItem(CrossDataFuncColumn column, CrossDataGroupColumnValue value)
        {
            Caption = value.Value != null ? value.Value.ToString() : "";
            Column = column;
            ColumnValues = new List<CrossDataGroupColumnValue> {value};
            var parent = value.Parent;
            while (parent != null)
            {
                ColumnValues.Add(parent);
                parent = parent.Parent;
            }
        }
    }

    public class CrossDataRow
    {
        private readonly List<CrossDataRowValue> _values = new List<CrossDataRowValue>();
        public List<CrossDataRowValue> Values { get { return _values; } }

        public void AddValue(CrossDataKeyColumn column, object value)
        {
            var data = Find(column);
            if (data == null)
                _values.Add(new CrossDataRowValue(column, value));
            else
                data.Value = value;
        }

        public void AddValue(CrossDataColumn column, CrossDataGroupColumnValue columnValue, object value)
        {
            var data = Find(column, columnValue);
            if (data == null)
                _values.Add(new CrossDataRowGroupColumnValue(column, columnValue, value));
            else
                data.Value = value;
        }

        public CrossDataRowValue Find(CrossDataColumn column)
        {
            return _values.FirstOrDefault(v => !(v is CrossDataRowGroupColumnValue) && v.Column == column);
        }
        public CrossDataRowValue Find(CrossDataColumn column, CrossDataGroupColumnValue groupValue)
        {
            return _values.OfType<CrossDataRowGroupColumnValue>().FirstOrDefault(v => v.Column == column && v.GroupValue == groupValue);
        }

        public CrossDataRowValue GetValue(CrossDataColumn column)
        {
            return _values.First(v => v.Column == column);
        }
    }

    public class CrossDataColumn
    {
        public int Key { get; private set; }

        //public abstract bool IsSame(object key);

        public string Caption { get; private set; }

        protected CrossDataColumn(int key, string caption)
        {
            Key = key;
            Caption = caption;
        }

        public virtual int GetSubColumnCount()
        {
            return 1;
        }
    }

    public class CrossDataKeyColumn : CrossDataColumn
    {
        //public int Key { get; private set; }

        /*public override bool IsSame(object key)
        {
            if (key == null) return false; //Key == null;

            return key.Equals(Key);
        }*/
        public CrossDataKeyColumn(int key, string caption) : base(key, caption) {}
    }

    public class CrossDataGroupColumn/*<TKey>*/ : CrossDataColumn
    {
        //public /*TKey*/int Key { get; private set; }
        public CrossDataGroupColumn(int key, string caption) : base(key, caption) {}

        private readonly List<CrossDataGroupColumnValue> _values = new List<CrossDataGroupColumnValue>();
        public List<CrossDataGroupColumnValue> Values { get { return _values; } }

        /*public override bool IsSame(object key)
        {
            if (key == null) return false; //Key == null;

            return key.Equals(Key);
        }*/

        public CrossDataGroupColumnValue FindValue(object value)
        {
            return Values.Where(cv => cv.Parent == null).FirstOrDefault(colValue => colValue.ValueEquals(value));
        }
        public CrossDataGroupColumnValue FindValue(CrossDataGroupColumnValue parent, object value)
        {
            return Values.Where(cv => cv.Parent == parent).FirstOrDefault(colValue => colValue.ValueEquals(value));
        }

        public CrossDataGroupColumnValue GetValue(object value)
        {
            var colValue = FindValue(value);
            if (colValue != null) return colValue;
            colValue = new CrossDataGroupColumnValue(null, value);
            Values.Add(colValue);
            return colValue;
        }

        public CrossDataGroupColumnValue GetValue(CrossDataGroupColumnValue parent, object value)
        {
            var colValue = FindValue(parent, value);
            if (colValue != null) return colValue;
            colValue = new CrossDataGroupColumnValue(parent, value);
            Values.Add(colValue);
            return colValue;
        }

        public override int GetSubColumnCount()
        {
            return _values.Sum(v => v.ChildValues.Count > 0 ? v.ChildValues.Count : 1);
        }
    }

    public class CrossDataFuncColumn : CrossDataColumn
    {
        public SqlQuerySummaryFunction Function { get; set; }

        public CrossDataFuncColumn(int key, string caption, SqlQuerySummaryFunction func) : base(key, caption)
        {
            Function = func;
        }
    }

    public class CrossDataRowValue
    {
        public CrossDataColumn Column { get; private set; }
        public object Value { get; set; }

        public CrossDataRowValue(CrossDataColumn column, object value)
        {
            Column = column;
            Value = value;
        }

        public bool ValueEquals(CrossDataRowValue val2)
        {
            if (val2 == null) return false;
            if (Value == null && val2.Value == null) return true;
            if (Value == null || val2.Value == null) return false;
            return Value.Equals(val2.Value);
        }
    }

    public class CrossDataGroupColumnValue
    {
        public CrossDataGroupColumnValue Parent { get; private set; }
        public object Value { get; private set; }

        private readonly List<CrossDataGroupColumnValue> _childValues = new List<CrossDataGroupColumnValue>();

        public List<CrossDataGroupColumnValue> ChildValues
        {
            get { return _childValues; }
        }

        public CrossDataGroupColumnValue(CrossDataGroupColumnValue parent, object value)
        {
            Parent = parent;
            Value = value;
            if (parent != null)
                Parent._childValues.Add(this);
        }

        public bool ValueEquals(object value)
        {
            if (value == null && Value == null) return true;
            if (Value != null) return Value.Equals(value);
            return false;
        }
    }


    public class CrossDataRowGroupColumnValue: CrossDataRowValue
    {
        public CrossDataGroupColumnValue GroupValue { get; private set; }

        public CrossDataRowGroupColumnValue(CrossDataColumn column, CrossDataGroupColumnValue groupValue, object value) : base(column, value)
        {
            GroupValue = groupValue;
        }
    }

}
