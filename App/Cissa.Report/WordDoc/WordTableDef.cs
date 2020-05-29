using System.Collections.Generic;
using System.Linq;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordTableDef : WordDocItemDef
    {
        private readonly IList<WordTableSectionDef> _sections = new List<WordTableSectionDef>();
        public IList<WordTableSectionDef> Sections { get { return _sections; } }

        public bool FitWidth { get; set; }

        public WordTableRowDef AddRow(/*int rowNo*/)
        {
            var section = _sections.LastOrDefault();
            if (section == null)
            {
                section = new WordTableSectionDef();
                _sections.Add(section);
            }

            return section.AddRow(/*rowNo*/);
        }
        public WordTableRowDef InsertRow(int rowNo)
        {
            var section = _sections.LastOrDefault();
            if (section == null)
            {
                section = new WordTableSectionDef();
                _sections.Add(section);
            }

            return section.InsertRow(rowNo);
        }
        public WordTableCellDef AddCell(int rowNo, int colNo)
        {
            var row = InsertRow(rowNo);
            return row.AddCell(colNo);
        }

        public WordTableSectionDef AddSection(WordTableSectionDef section)
        {
            _sections.Add(section);
            return section;
        }

        public int GetColCount()
        {
            return _sections.Count > 0 ? _sections.Max(s => s.GetColCount()) : -1;
        }
    }
}