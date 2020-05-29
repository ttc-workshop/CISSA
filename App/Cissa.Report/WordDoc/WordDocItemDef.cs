using Intersoft.Cissa.Report.Styles;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordDocItemDef
    {
        private readonly ContentStyle _style = new ContentStyle();
        public ContentStyle Style { get { return _style; } set { _style.MulMerge(value); } }
    }
}