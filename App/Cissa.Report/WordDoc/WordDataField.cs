using System;
using Intersoft.Cissa.Report.Common;
using Intersoft.CISSA.DataAccessLayer.Model;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordDataField : WordContentItemDef
    {
        private DataSetField Field { get; set; }
        public string Format { get; set; }
        public Func<object, object> Func { get; set; }

        public WordDataField(DataSetField field, string format = null)
        {
            Field = field;
            Format = format;
        }

        public override string GetText()
        {
            var value = Field.GetValue();
            var type = Field.GetDataType();
            if (Func != null)
            {
                value = Func(value);
                return value != null ? value.ToString() : String.Empty;
            }

            if (value != null)
                switch (type)
                {
                    case BaseDataType.Text:
                        return (string) value;
                    case BaseDataType.Int:
                        return String.IsNullOrEmpty(Format) ? ((int) value).ToString() : ((int) value).ToString(Format);
                    case BaseDataType.Float:
                        return String.IsNullOrEmpty(Format)
                            ? ((double) value).ToString("N")
                            : ((double) value).ToString(Format);
                    case BaseDataType.Currency:
                        return String.IsNullOrEmpty(Format)
                            ? ((decimal) value).ToString("N")
                            : ((decimal) value).ToString(Format);
                    case BaseDataType.DateTime:
                        return String.IsNullOrEmpty(Format)
                            ? ((DateTime) value).ToShortDateString()
                            : ((DateTime) value).ToString(Format);
                    case BaseDataType.Bool:
                        return String.IsNullOrEmpty(Format)
                            ? ((bool) value) ? "Да" : "Нет"
                            : ((double) value).ToString(Format);
                    default:
                        return value.ToString();
                }
            return String.Empty;
        }
    }
}