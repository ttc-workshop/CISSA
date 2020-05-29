using System.Drawing;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;
using NPOI.SS.UserModel;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsGrid : XlsArea
    {
        public const int WriteSummaryRow = 1;

        public DataSet RowDatas { get; private set; }

        public bool ShowSummary { get; set; }

        public XlsGrid(DataSet dataSet)
        {
            RowDatas = dataSet;
            ShowSummary = true;
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            var oldStyle = writer.MergeStyle(Style);
            try
            {
                var i = 0;
                while (!RowDatas.Eof())
                {
                    using (var rowWriter = writer.AddRowArea(GetRows(), GetCols()))
                    {
                        // rowWriter.SetBorder(BorderTop, BorderLeft, BorderRight, BorderBottom);
                        foreach (var item in Items)
                        {
                            if (item.GetRows() == 0) continue;

                            using (var colWriter = rowWriter.AddColArea())
                            {
                                item.WriteTo(colWriter, param);
                            }
                        }
                    }
                    RowDatas.Next();
                    i++;
                }
                if (i == 0)
                {
                    using (var rowWriter = writer.AddRowArea(GetRows(), GetCols()))
                    {
                        using (var colWriter = rowWriter.AddColArea())
                        {
                            var noRecord = new XlsText("Нет данных", Items != null && Items.Count > 0 ? Items[Items.Count - 1].GetCols() : 1)
                            {
                                Style = {FontColor = IndexedColors.GREY_50_PERCENT.Index, HAlign = HAlignment.Center}
                            };
                            noRecord.WriteTo(colWriter, param);
                        }
                    }
                }
                else if (ShowSummary)
                {
                    using (var rowWriter = writer.AddRowArea(GetRows(), GetCols()))
                    {
                        rowWriter.Style.FontStyle |= FontStyle.Bold;
                        // rowWriter.SetBorder(BorderTop, BorderLeft, BorderRight, BorderBottom);
                        foreach (var item in Items)
                        {
                            if (item.GetRows() == 0) continue;

                            using (var colWriter = rowWriter.AddColArea())
                            {
                                item.WriteTo(colWriter, WriteSummaryRow);
                            }
                        }
                    }
                }
            }
            finally
            {
                writer.Style = oldStyle;
            }
        }

        protected override void DoDispose()
        {
            base.DoDispose();
            if (RowDatas != null)
            {
                RowDatas.Dispose();
                RowDatas = null;
            }
        }

        ~XlsGrid()
        {
            if (RowDatas != null)
                RowDatas.Dispose();
        }
    }
}