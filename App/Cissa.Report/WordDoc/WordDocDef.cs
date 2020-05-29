namespace Intersoft.Cissa.Report.WordDoc
{
    public enum PageOrientation
    {
        Portrait,
        Landscape
    }
    public enum PaperSize { A4, A3, A5 }

    public class WordDocDef : WordSectionDef
    {
        public const float A4PaperHeight = 1122;
        public const float A4PaperWidth = 793;

        public PageOrientation Orientation { get; set; }
        public PaperSize PaperSize { get; set; }

        public WordDocDef Portrait()
        {
            Orientation = PageOrientation.Portrait;
            return this;
        }
        public WordDocDef Landscape()
        {
            Orientation = PageOrientation.Landscape;
            return this;
        }

        public WordDocDef A5()
        {
            PaperSize = PaperSize.A5;
            return this;
        }
        public WordDocDef A3()
        {
            PaperSize = PaperSize.A3;
            return this;
        }
        public WordDocDef A4()
        {
            PaperSize = PaperSize.A4;
            return this;
        }

        public float MarginLeft { get; set; }
        public float MarginTop { get; set; }
        public float MarginBottom { get; set; }
        public float MarginRight { get; set; }
    }
}