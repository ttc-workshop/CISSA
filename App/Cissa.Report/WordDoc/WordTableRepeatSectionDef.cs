using Intersoft.Cissa.Report.Common;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordTableRepeatSectionDef : WordTableSectionDef
    {
        public DataSet Datas { get; private set; }

        public bool ResetDatas { get; set; }

        public WordTableRepeatSectionDef(DataSet dataSet)
        {
            Datas = dataSet;
        }
    }
}