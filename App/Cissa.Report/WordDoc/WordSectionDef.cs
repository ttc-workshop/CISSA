using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordSectionDef : WordGroupDef
    {
        public WordParagraphDef AddParagraph(string text, ContentStyle style = null)
        {
            var result = new WordParagraphDef(text, style);
            AddItem(result);
            if (style != null) result.Style = style;
            result.AddText(text);
            return result;
        }

        public WordTableDef AddTable(ContentStyle style = null)
        {
            var result = new WordTableDef();
            AddItem(result);
            if (style != null) result.Style = style;
            return result;
        }

        public WordRepeatSectionDef AddRepeatSection(DataSet dataSet, ContentStyle style = null)
        {
            var result = new WordRepeatSectionDef(dataSet);
            AddItem(result);
            if (style != null) result.Style = style;
            return result;
        }

        public void AddPageBreak()
        {
            AddItem(new PageBreak());
        }
    }
}