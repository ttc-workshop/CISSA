using System.Collections.Generic;
using System.Linq;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordTableRowDef: WordDocItemDef
    {
        private readonly IDictionary<int, WordTableCellDef> _cells = new Dictionary<int, WordTableCellDef>();
        public IDictionary<int, WordTableCellDef> Cells { get { return _cells; } }

        public WordTableCellDef AddCell(int colNo)
        {
            if (_cells.ContainsKey(colNo)) return _cells[colNo];

            var cell = new WordTableCellDef();
            _cells.Add(colNo, cell);
            return cell;
        }

        public virtual int GetColCount()
        {
            return Cells.Count > 0 ? Cells.Keys.Max() + 1 : -1;
        }
    }
}