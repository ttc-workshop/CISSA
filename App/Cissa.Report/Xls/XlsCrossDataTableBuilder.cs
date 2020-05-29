using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Intersoft.Cissa.Report.Defs;
using Intersoft.Cissa.Report.Styles;
using Intersoft.Cissa.Report.Utils;
using Intersoft.Cissa.Report.Xls.Adjuster;
using NPOI.SS.UserModel;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsCrossDataTableBuilder
    {
        public ReportDef Report { get; private set; }
        //public SqlQueryDataSet SqlDataSet { get; set; }
        public CrossDataTable Table { get; set; }

        private readonly List<XlsGridReportSectionItem> _headers = new List<XlsGridReportSectionItem>();
        public List<XlsGridReportSectionItem> Headers { get { return _headers; } }

        private readonly List<XlsGridReportSectionItem> _footers = new List<XlsGridReportSectionItem>();
        public List<XlsGridReportSectionItem> Footers { get { return _footers; } }

        public XlsCrossDataTableBuilder(ReportDef report, CrossDataTable table)
        {
            Report = report;
            Table = table;

            Headers.Add(new XlsGridReportSectionText(Report.Caption));
        }

        public XlsGridReportSectionText AddHeaderText(string text)
        {
            var section = new XlsGridReportSectionText(text);

            Headers.Add(section);
            return section;
        }

        public XlsGridReportSectionText AddFooterText(string text)
        {
            var section = new XlsGridReportSectionText(text);

            Footers.Add(section);
            return section;
        }

        public XlsGridReportSectionTable AddHeaderTable()
        {
            var section = new XlsGridReportSectionTable();

            Headers.Add(section);
            return section;
        }

        public XlsGridReportSectionTable AddFooterTable()
        {
            var section = new XlsGridReportSectionTable();

            Footers.Add(section);
            return section;
        }

        private readonly XlsFormColumnAdjuster _adjuster = new XlsFormColumnAdjuster();

        public XlsDef Build()
        {
            var def = new XlsDef();
            try
            {
                AdjustReportColumns(Table);
                _adjuster.Adjust();

                def.Style.FontName = "Arial Narrow";
                def.AddArea().AddEmptyRow();

                var headArea = def.AddArea();
                BuildSections(headArea, Headers);

                def.AddArea().AddEmptyRow();

                var hRow = def.AddArea().AddRow();
                hRow.ShowAllBorders(true);
                hRow.Style.FontStyle = FontStyle.Bold;
                hRow.Style.HAlign = HAlignment.Center;
                hRow.Style.BgColor = IndexedColors.BLUE_GREY.Index; //48;
                hRow.Style.FontColor = IndexedColors.WHITE.Index;
                hRow.Style.WrapText = true;

                var dRow = def.AddArea().AddRow();
                dRow.ShowAllBorders(true);
                dRow.Style.WrapText = true;

                if (Report.Columns != null)
                {
                    BuildHeaderColumns(hRow, dRow);

                    foreach (var row in Table.Rows)
                    {
                        dRow = def.AddArea().AddRow();
                        dRow.ShowAllBorders(true);
                        dRow.Style.WrapText = true;
                        AddRow(dRow, row);
                    }
                    // Add Summary Row
                    dRow = def.AddArea().AddRow();
                    dRow.ShowAllBorders(true);
                    dRow.Style.WrapText = true;
                    dRow.Style.FontStyle = FontStyle.Bold;

                    var summaries = Table.GetColumnSummaries();
                    foreach (var data in summaries)
                    {
                        if (data is int)
                            dRow.AddInt((int)data);
                        else if (data is double)
                            dRow.AddFloat((double)data);
                        else
                            dRow.AddEmptyCell();
                    }
                    dRow.Style.AutoHeight = true;
                }
                def.AddArea().AddEmptyRow();
                var footArea = def.AddArea();
                BuildSections(footArea, Footers);

                def.ColumnWidths = _adjuster.ColumnSizes;

                return def;
            }
            catch
            {
                def.Dispose();
                throw;
            }
        }

        private void AddRow(XlsRow dRow, CrossDataRow row)
        {
            foreach (var data in Table.GetRowColumnDatas(row))
            {
                if (data is int)
                    dRow.AddInt((int) data);
                else if (data is double)
                    dRow.AddFloat((double) data);
                else if (data is bool)
                    dRow.AddBool((bool) data);
                else if (data is DateTime)
                    dRow.AddDateTime((DateTime) data);
                else
                    dRow.AddText(data != null ? data.ToString() : "");
            }
            dRow.Style.AutoHeight = true;
        }

        private void BuildSections(XlsArea area, IEnumerable<XlsGridReportSectionItem> sections)
        {
            foreach (var section in sections)
            {
                section.Build(area, _adjuster);
            }
        }

        private void AdjustReportColumns(CrossDataTable table)
        {
            Headers.ForEach(_adjuster.AddReportSection);
            _adjuster.Forms.Add(new XlsCrossDataTableAdjustInfo(table));
            Footers.ForEach(_adjuster.AddReportSection);
        }

        protected void BuildHeaderColumns(XlsGroup band, XlsRow gridRow)
        {
            XlsTextNode header;

            foreach (var col in Table.Columns.OfType<CrossDataKeyColumn>())
            {
                header = new XlsTextNode(col.Caption);
                band.AddGroup(header);
                var info = _adjuster.Find(col);
                if (info != null)
                {
                    header.ColSpan = info.ColSpan;
                }
            }
            var group = Table.Columns.OfType<CrossDataGroupColumn>().FirstOrDefault();
            if (group != null)
            {
                header = new XlsTextNode(group.Caption);
                band.AddGroup(header);
                if (group.Values != null && group.Values.Count > 0)
                    foreach (var val in group.Values)
                    {
                        var h = new XlsTextNode(val.Value != null ? val.Value.ToString() : "");
                        header.AddGroup(h);
                        AddGroupBandColumns(h, val);
                    }
                else 
                    AddGroupBandColumns(header, null);
            }
        }

        private void AddGroupBandColumns(XlsTextNode header, CrossDataGroupColumnValue child)
        {
            if (child != null && child.ChildValues != null && child.ChildValues.Count > 0)
                foreach (var val in child.ChildValues)
                {
                    var hb = new XlsTextNode(val.Value != null ? val.Value.ToString() : "-");
                    header.AddGroup(hb);
                    AddGroupBandColumns(hb, val);
                }
            else
                foreach (var func in Table.Columns.OfType<CrossDataFuncColumn>())
                {
                    header.AddNode(func.Caption);
//                    var info = _adjuster.Find(func);
                }
        }
    }
}