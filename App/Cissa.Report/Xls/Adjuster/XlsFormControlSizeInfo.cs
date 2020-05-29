namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    internal class XlsFormControlSizeInfo
    {
        public int ColumnNo { get; private set; }
        public XlsColumnItemAdjustInfo Control { get; private set; }
//        public XlsColumnPartType PartType { get; private set; }
        public int TotalSize { get; private set; }

        public XlsFormControlSizeInfo(int no, XlsColumnItemAdjustInfo control, int totalSize/*, XlsColumnPartType partType*/)
        {
            ColumnNo = no;
            Control = control;
            TotalSize = totalSize;
//            PartType = partType;
        }
    }
}