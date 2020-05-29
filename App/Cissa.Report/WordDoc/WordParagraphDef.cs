using System;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordParagraphDef : WordGroupDef
    {
        public WordParagraphDef() { }

        public WordParagraphDef(string text, ContentStyle style = null)
        {
            AddText(text, style);
        }

        public WordParagraphDef(DataSetField field, ContentStyle style = null, string format = null)
        {
            AddDataField(field, style, format);
        }

        public WordParagraphDef(SqlQueryDataSet dataSet, string name, ContentStyle style = null, string format = null)
        {
            AddDataField(dataSet, name, style, format);
        }

        public WordParagraphTextDef AddText(string text, ContentStyle style = null)
        {
            var result = new WordParagraphTextDef(text);
            AddItem(result);
            if (style != null) result.Style = style;
            return result;
        }
        public WordParagraphTextDef AddTextLine(string text, ContentStyle style = null)
        {
            var result = new WordParagraphTextDef(text, true);
            AddItem(result);
            if (style != null) result.Style = style;
            return result;
        }

        public WordDataField AddDataField(DataSetField field, ContentStyle style = null, string format = null)
        {
            var result = new WordDataField(field, format);
            AddItem(result);
            if (style != null) result.Style = style;
            return result;
        }

        public void AddDataField(SqlQueryDataSet dataSet, string name, ContentStyle style = null, string format = null)
        {
            if (dataSet != null)
                //AddDataField(new SqlQueryDataSetField(dataSet, dataSet.Reader.Query.GetAttribute(name), null), style);
                AddDataField(dataSet.CreateField(name), style, format);
            else
                throw new ArgumentNullException("dataSet");
        }
    }
}
