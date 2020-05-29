using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class WordRepeatSectionDef : WordSectionDef
    {
        public DataSet Datas { get; private set; }

        public bool ResetDatas { get; set; }

        public WordRepeatSectionDef(DataSet dataSet)
        {
            Datas = dataSet;
        }

        public WordRepeatSectionDef(DataSet dataSet, ContentStyle style)
        {
            Datas = dataSet;
            Style = style;
        }
    }
}