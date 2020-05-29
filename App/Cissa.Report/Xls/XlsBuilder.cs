using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsBuilder
    {
        public XlsDef Def { get; set; }

        public XlsBuilder(XlsDef def)
        {
            Def = def;
        }

        public HSSFWorkbook Workbook { get; private set; }
        public Sheet Sheet { get; private set; }

        public HSSFWorkbook Build()
        {
            Initialize();

            var writer = new XlsWriter(null, Workbook, Sheet, 0, 0);

            Def.WriteTo(writer);
/*
            foreach (var area in Def.Areas)
            {
                using (var areaWriter = writer.AddArea())
                {
                    BuildArea(area, areaWriter);
                }
            }
*/
            return Workbook;
        }

/*        private static void BuildArea(XlsGroup area, XlsWriter writer)
        {
            if (area.GetRows() <= 0) return;

            foreach(var row in area.Items.OfType<XlsRow>())
            {
                using (var rowWriter = writer.AddArea())
                {
                    foreach (var item in row.Items)
                    {
                        BuildItem(item, rowWriter);
                    }
                }
            }
        }

        private static void BuildItem(XlsItem item, XlsWriter writer)
        {
            if (item is XlsCell)
            {
                var cell = (XlsCell) item;

                writer.AddCell(cell.GetCols(), cell.GetRows());
                //cell.WriteTo(writer);
            }
        }*/

        internal void Initialize()
        {
            Workbook = new HSSFWorkbook();

            //Create a entry of DocumentSummaryInformation
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "Intersoft Ltd.";
            Workbook.DocumentSummaryInformation = dsi;

            //Create a entry of SummaryInformation
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Subject = "КИССП Отчет";
            Workbook.SummaryInformation = si;

            Sheet = Workbook.CreateSheet("Отчет");
        }
    }
}
