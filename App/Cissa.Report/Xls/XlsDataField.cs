using System;
using Intersoft.Cissa.Report.Common;
using Intersoft.CISSA.DataAccessLayer.Model;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsDataField : XlsCell
    {
        public DataSetField Field { get; private set; }

        public double SummaryValue { get; private set; }
        public int SummaryValueCount { get; private set; }

        public XlsDataField(DataSetField field, int colSpan = 0, int rowSpan = 0)
            : base(colSpan, rowSpan)
        {
            Field = field;
            ShrinkToFit = true;
            SummaryValue = 0.0;
            SummaryValueCount = 0;
        }

        public override object GetValue()
        {
            return Field.GetValue();
        }

        public object GetSummaryValue()
        {
            return SummaryValue;
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            var oldStyle = writer.MergeStyle(Style);
            try
            {
                var value = param == XlsGrid.WriteSummaryRow
                    ? SummaryValueCount > 0 ? GetSummaryValue() : ""
                    : GetValue();
                var type = Field.GetDataType();

                // writer.SetBorder(BorderTop, BorderLeft, BorderRight, BorderBottom);
                writer.ShrinkToFit = ShrinkToFit;
                writer.AddCell(ColSpan, RowSpan);

                if (value != null)
                {
                    var s = value.ToString();
                    if (type == BaseDataType.Int)
                    {
                        int i;
                        if (value is int)
                        {
                            writer.SetValue((int) value);
                            SummaryValue += (int) value;
                            SummaryValueCount++;
                        } 
                        else if (int.TryParse(s, out i))
                        {
                            writer.SetValue(i);
                            SummaryValue += i;
                            SummaryValueCount++;
                        }
                        else writer.SetValue(s);
                    }
                    else if (type == BaseDataType.Float || type == BaseDataType.Currency)
                    {
                        double d;
                        if (value is double)
                        {
                            writer.SetValue((double) value);
                            SummaryValue += (double) value;
                            SummaryValueCount++;
                        }
                        else if (value is float)
                        {
                            d = Convert.ToDouble(value);
                            writer.SetValue(d);
                            SummaryValue += d;
                            SummaryValueCount++;
                        }
                        else if (value is decimal)
                        {
                            d = Convert.ToDouble(value);
                            writer.SetValue(d);
                            SummaryValue += d;
                            SummaryValueCount++;
                        }
                        else if (value is int)
                        {
                            d = Convert.ToDouble(value);
                            writer.SetValue(d);
                            SummaryValue += d;
                            SummaryValueCount++;
                        }
                        else if (double.TryParse(s, out d))
                        {
                            writer.SetValue(d);
                            SummaryValue += d;
                            SummaryValueCount++;
                        }
                        else writer.SetValue(s);
                    }
                    else if (type == BaseDataType.DateTime)
                    {
                        DateTime dt;
                        if (value is DateTime) writer.SetValue((DateTime) value);
                        else if (DateTime.TryParse(s, out dt)) writer.SetValue(dt);
                        else writer.SetValue(s);
                    }
                    else if (type == BaseDataType.Bool)
                    {
                        bool b;
                        if (value is bool) writer.SetValue((bool) value);
                        else if (bool.TryParse(s, out b)) writer.SetValue(b);
                        else writer.SetValue(s);
                    }
                    else 
                        writer.SetValue(s);
                    /*{
                        case CissaDataType.Text:
                            writer.SetValue(value.ToString());
                            break;
                        case CissaDataType.Int:
                            writer.SetValue((int) value);
                            break;
                        case CissaDataType.Float:
                            writer.SetValue((double) value);
                            break;
                        case CissaDataType.Currency:
                            writer.SetValue(Convert.ToDouble(value));
                            break;
                        case CissaDataType.DateTime:
                            writer.SetValue((DateTime) value);
                            break;
                        case CissaDataType.Bool:
                            writer.SetValue((bool) value);
                            break;
                        default:
                            writer.SetValue(value);
                            break;
                    }*/
                }
                if (Width != null)
                    writer.SetColumnWidth((int)Width);
            }
            finally
            {
                writer.Style = oldStyle;
            }
        }
        public override int GetDefaultSize()
        {
            switch (Field.GetDataType())
            {
                case BaseDataType.Text:
                    return 30;
                case BaseDataType.Int:
                    return 10;
                case BaseDataType.Float:
                    return 12;
                case BaseDataType.Currency:
                    return 12;
                case BaseDataType.DateTime:
                    return 10;
                case BaseDataType.Bool:
                    return 5;
                default:
                    return 10;
            }
        }

    }
}