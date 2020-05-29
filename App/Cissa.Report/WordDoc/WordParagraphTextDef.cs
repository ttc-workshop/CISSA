namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordParagraphTextDef : WordContentItemDef
    {
        public string Text { get; set; }
        public bool EndOfLine { get; set; }

        public WordParagraphTextDef(string text, bool eoln = false)
        {
            Text = text;
            EndOfLine = eoln;
        }
        public override string GetText()
        {
            if (EndOfLine) return Text + "\n";
            return Text;
        }
    }
}