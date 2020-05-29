using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Defs;
using Intersoft.Cissa.Report.Styles;
using Intersoft.Cissa.Report.Xls.Adjuster;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using NPOI.SS.UserModel;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsReportDefBuilder
    {
        public ReportDef Report { get; private set; }
        public SqlQueryDataSet SqlDataSet { get; set; }

        private readonly List<XlsGridReportSectionItem> _headers = new List<XlsGridReportSectionItem>();
        public List<XlsGridReportSectionItem> Headers { get { return _headers; } }

        private readonly List<XlsGridReportSectionItem> _footers = new List<XlsGridReportSectionItem>();
        public List<XlsGridReportSectionItem> Footers { get { return _footers; } }

        public XlsReportDefBuilder(IAppServiceProvider provider, ReportDef report, SqlQueryReader reader)
        {
            Report = report;
            SqlDataSet = new SqlQueryDataSet(provider, reader);

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
                AdjustDocListForms(Report);
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

                var dRow = def.AddGrid(SqlDataSet).AddRow();
                dRow.ShowAllBorders(true);
                dRow.Style.WrapText = true;

                var sourceMap = MapSources();
                if (Report.Columns != null)
                    foreach (var column in Report.Columns)
                    {
                        AddColumn(hRow, dRow, column, sourceMap);
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

        private Dictionary<Guid, SqlQuerySource> MapSources()
        {
            var result = new Dictionary<Guid, SqlQuerySource>();

            if (SqlDataSet == null) return result;

            foreach (var sourceDef in Report.Sources.Where(s => s.DocDef != null))
            {
                if (SqlDataSet.Reader.Query.Source.IsDocDef(sourceDef.Id) && !result.ContainsValue(SqlDataSet.Reader.Query.Source))
                {
                    result.Add(sourceDef.Id, SqlDataSet.Reader.Query.Source);
                }
                else
                {
                    var querySource =
                        SqlDataSet.Reader.Query.Sources.FirstOrDefault(
                            s => s.IsDocDef(sourceDef.Id) && !result.ContainsValue(s));
                    if (querySource != null)
                        result.Add(sourceDef.Id, querySource);
                }
            }
            return result;
        }

        private void BuildSections(XlsArea area, IEnumerable<XlsGridReportSectionItem> sections)
        {
            foreach (var section in sections)
            {
                section.Build(area, _adjuster);
            }
        }

        private void AdjustDocListForms(ReportDef report)
        {
            Headers.ForEach(_adjuster.AddReportSection);
            _adjuster.AddReportDef(report);
            Footers.ForEach(_adjuster.AddReportSection);
        }

        protected void AddColumn(XlsGroup band, XlsRow gridRow, ReportColumnDef column, Dictionary<Guid, SqlQuerySource> sourceMap)
        {
            var header = new XlsTextNode(column.Caption);
            band.AddGroup(header);
            var info = _adjuster.Find(column);
            var attrColumn = column as ReportAttributeColumnDef;
            if (attrColumn != null && SqlDataSet != null)
            {
                SqlQuerySource querySource;
                var reportSource = Report.GetSourceDef(attrColumn.Attribute.SourceId);
                var reportSourceAttr = reportSource.Attributes != null
                    ? reportSource.Attributes.FirstOrDefault(a => a.Id == attrColumn.Attribute.AttributeId)
                    : null;

                var hasQuerySource = sourceMap.TryGetValue(attrColumn.Attribute.SourceId, out querySource);

                var attr = reportSourceAttr == null
                    ? !hasQuerySource
                        ? SqlDataSet.Reader.Query.FindAttribute(attrColumn.Attribute.AttributeId)
                        : SqlDataSet.Reader.Query.FindAttribute(querySource, attrColumn.Attribute.AttributeId)
                    : !hasQuerySource
                        ? SqlDataSet.Reader.Query.FindAttribute(reportSourceAttr.Ident)
                        : SqlDataSet.Reader.Query.FindAttribute(querySource, reportSourceAttr.Ident);
                if (attr != null)
                {
                    var field = gridRow.AddDataField(new SqlQueryDataSetField(SqlDataSet, attr, attrColumn.ToSqlGrouping()));
                    if (info != null)
                    {
                        header.ColSpan = info.ColSpan;
                        field.ColSpan = info.ColSpan;
                    }
                }
            }
        }
    }
}