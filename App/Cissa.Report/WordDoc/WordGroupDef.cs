using System.Collections.Generic;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordGroupDef : WordDocItemDef
    {
        private readonly IList<WordDocItemDef> _items = new List<WordDocItemDef>();
        public IList<WordDocItemDef> Items { get { return _items; } }

        public void AddItem(WordDocItemDef item)
        {
            _items.Add(item);
        }

        public void RemoveItem(WordDocItemDef item)
        {
            _items.Remove(item);
        }
    }
}