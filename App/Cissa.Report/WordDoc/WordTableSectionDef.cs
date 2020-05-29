using System.Collections.Generic;
using System.Linq;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordTableSectionDef : WordDocItemDef
    {
        private readonly IList<WordTableRowDef> _rows = new List<WordTableRowDef>();
        public IList<WordTableRowDef> Rows { get { return _rows; } }

        public WordTableRowDef InsertRow(int rowNo)
        {
            if (_rows.Count > rowNo) return _rows[rowNo];

            var row = new WordTableRowDef();
            _rows.Add(/*rowNo, */row);
            return row;
        }
        public WordTableCellDef AddCell(int rowNo, int colNo)
        {
            var row = InsertRow(rowNo);
            return row.AddCell(colNo);
        }

/*
        public int GetRowCount()
        {
            return _rows.Count > 0 ? _rows.Keys.Max() : -1;
        }
*/

        public virtual int GetColCount()
        {
            return _rows.Count > 0 ? _rows./*Values.*/Max(r => r.GetColCount()) : -1;
        }

        public virtual IEnumerable<WordTableRowDef> GetRows()
        {
            return Rows/*.OrderBy(rp => rp.Key).Select(rp => rp.Value)*/;
        }

        public WordTableRowDef AddRow()
        {
            var row = new WordTableRowDef();
            _rows.Add(/*rowNo, */row);
            return row;
        }
    }
}