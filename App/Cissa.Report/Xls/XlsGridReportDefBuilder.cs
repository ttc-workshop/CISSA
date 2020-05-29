using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;
using Intersoft.Cissa.Report.Xls.Adjuster;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using NPOI.SS.UserModel;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsGridReportSectionItem
    {
        private readonly ContentStyle _style = new ContentStyle();
        public ContentStyle Style { get { return _style; } }

        public int LeftMargin { get; set; }
        public int RightMargin { get; set; }

        public XlsGridReportSectionItem Bold()
        {
            Style.FontStyle |= FontStyle.Bold;
            return this;
        }
        public XlsGridReportSectionItem Italic()
        {
            Style.FontStyle |= FontStyle.Italic;
            return this;
        }
        public XlsGridReportSectionItem Center()
        {
            Style.HAlign = HAlignment.Center;
            return this;
        }
        public XlsGridReportSectionItem Right()
        {
            Style.HAlign = HAlignment.Right;
            return this;
        }
        public XlsGridReportSectionItem Left()
        {
            Style.HAlign = HAlignment.Left;
            return this;
        }
        public XlsGridReportSectionItem FontDSize(short delta)
        {
            Style.FontDSize = delta;
            return this;
        }
        public XlsGridReportSectionItem Margins(int left, int right = 0)
        {
            LeftMargin = left;
            RightMargin = right;
            return this;
        }
        public XlsGridReportSectionItem Borders(bool show = true)
        {
            if (show)
                Style.Borders = TableCellBorder.All;
            else
                Style.Borders = TableCellBorder.None;
            return this;
        }

        public virtual void Build(XlsArea area, XlsFormColumnAdjuster adjuster)
        {
        }
    }

    public class XlsGridReportSectionText : XlsGridReportSectionItem
    {
        public string Text { get; set; }

        public XlsGridReportSectionText(string text)
        {
            Text = text;
        }

        public override void Build(XlsArea area, XlsFormColumnAdjuster adjuster)
        {
            var row = area.AddRow();

            var info = adjuster.Find(this, -1);
            if (LeftMargin > 0)
            {
                row.AddEmptyCell(info != null ? info.ColSpan : 0);
            }
            info = adjuster.Find(this, 0);
            var cell = row.AddText(Text, info != null ? info.ColSpan : 1);
            cell.Style = Style;
        }
    }

    public class XlsGridReportSectionTable : XlsGridReportSectionItem
    {
        private readonly List<XlsGridReportSectionTableCell> _cells = new List<XlsGridReportSectionTableCell>();
        public List<XlsGridReportSectionTableCell> Cells { get { return _cells; } }

        public XlsGridReportSectionTableCell FindCell(int row, int col)
        {
            return _cells.FirstOrDefault(c => c.Row == row && c.Col == col);
        }

        public XlsGridReportSectionTableCell Add(int row, int col, string text)
        {
            var cell = FindCell(row, col);
            if (cell == null)
            {
                cell = new XlsGridReportSectionTableCell(row, col, text);
                Cells.Add(cell);
            }
            else
                cell.Text = text;

            return cell;
        }
        public override void Build(XlsArea area, XlsFormColumnAdjuster adjuster)
        {
            var maxRow = Cells.Max(c => c.Row);
            var maxCol = Cells.Max(c => c.Col);
            
            for (var r = 0; r <= maxRow; r++)
            {
                var row = area.AddRow();

                var info = adjuster.Find(this, -1);
                if (LeftMargin > 0)
                {
                    var marginCell = row.AddEmptyCell(info != null ? info.ColSpan : 0);
                    marginCell.Style.Borders = TableCellBorder.None;
                }

                for (var c = 0; c <= maxCol; c++)
                {
                    info = adjuster.Find(this, c);
                    var cell = Cells.FirstOrDefault(i => i.Row == r && i.Col == c);
                    if (cell != null)
                    {
                        var xlsCell = row.AddText(cell.Text, info != null ? info.ColSpan : 1);
                        xlsCell.Style = ContentStyle.MergeStyles(Style, cell.Style);
                    }
                    else
                    {
                        var xlsCell = row.AddText("", info != null ? info.ColSpan : 1);
                        xlsCell.Style = Style;
                    }
                }
            }
        }
    }

    public class XlsGridReportSectionTableCell : XlsGridReportSectionText
    {
        public int Col { get; set; }
        public int Row { get; set; }

        public XlsGridReportSectionTableCell(int row, int col, string text) : base(text)
        {
            Row = row;
            Col = col;
        }
    }

    public class XlsGridReportDefBuilder
    {
        public IDataContext DataContext { get; private set; }
        public DocFormDataSet DataSet { get; private set; }
        public BizForm Form { get; private set; }
        public Guid UserId { get; private set; }

        public SqlQueryDataSet SqlDataSet { get; set; }

        private readonly List<XlsGridReportSectionItem> _headers = new List<XlsGridReportSectionItem>();
        public List<XlsGridReportSectionItem> Headers { get { return _headers; } }

        private readonly List<XlsGridReportSectionItem> _footers = new List<XlsGridReportSectionItem>();
        public List<XlsGridReportSectionItem> Footers { get { return _footers; } }

        public XlsGridReportDefBuilder(IDataContext dataContext, BizForm form, IEnumerable<Guid> docs, Guid userId)
        {
            DataContext = dataContext;
            Form = form;
            UserId = userId;
            DataSet = new DocFormDataSet(dataContext, docs, form, userId);
        }

        public XlsGridReportDefBuilder(IDataContext dataContext, BizForm form, SqlQueryReader reader, Guid userId)
        {
            DataContext = dataContext;
            Form = form;
            UserId = userId;
            SqlDataSet = new SqlQueryDataSet(reader, userId);
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

        public XlsDef BuildFromBizForm()
        {
            return Build();
        }

        public XlsDef Build()
        {
            var def = new XlsDef();
            try
            {
                AdjustDocListForms(Form);
                _adjuster.Adjust();

                def.Style.FontName = "Arial Narrow";
                /*var title = def.AddArea().AddRow().AddText(Form.Caption);
                title.Style.FontDSize = 2;
                title.Style.FontStyle = FontStyle.Bold;
                title.Style.FontColor = IndexedColors.DARK_BLUE.Index; // 18;
                title.Style.HAlign = HAlignment.Center;
                title.Style.AutoHeight = true;*/

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

                var ds = ((DataSet) DataSet ?? SqlDataSet);
                var dRow = def.AddGrid(ds).AddRow();
                dRow.ShowAllBorders(true);
                dRow.Style.WrapText = true;

                if (Form.Children != null)
                    foreach (var control in Form.Children)
                    {
                        AddControlBand(hRow, dRow, control);
                    }

                def.AddArea().AddEmptyRow();
                var footArea = def.AddArea();
                BuildSections(footArea, Footers);

                def.ColumnWidths = _adjuster.ColumnSizes;

                //title.ColSpan = dRow.GetCols();

                return def;
            }
            catch
            {
                def.Dispose();
                throw;
            }
        }

        private void BuildSections(XlsArea area, IEnumerable<XlsGridReportSectionItem> sections)
        {
            foreach (var section in sections)
            {
                section.Build(area, _adjuster);
            }
        }

        private void AdjustDocListForms(BizControl form)
        {
            Headers.ForEach(_adjuster.AddReportSection);
            _adjuster.AddTableForm(form);
            Footers.ForEach(_adjuster.AddReportSection);
        }

        protected void AddControlBand(XlsGroup band, XlsRow gridRow, BizControl control)
        {
            if (control.Invisible) return;

            if (control is BizPanel
                /*control is BizTableColumn ||
                control is BizDocumentControl || control is BizDocumentListForm*/)
            {
                var node = new XlsTextNode(control.Caption);
                band.AddGroup(node);

                foreach (var child in control.Children)
                {
                    AddControlBand(node, gridRow, child);
                }
            }
            else if (control is BizTableColumn ||
                     control is BizDocumentControl || control is BizDocumentListForm)
            {
                if (control.Children != null)
                {
                    foreach (var child in control.Children)
                    {
                        AddControlBand(band, gridRow, child);
                    }
                }
            }
            else if (control is BizDataControl)
                AddControlColumn(band, gridRow, control);
            else
                if (control.Children != null)
                {
                    foreach (var child in control.Children)
                    {
                        AddControlBand(band.AddGroup(new XlsTextNode(child.Caption)), gridRow, child);
                    }
                }
        }

        protected void AddControlColumn(XlsGroup band, XlsRow gridRow, BizControl control)
        {
            if (control.Invisible) return;

            var header = new XlsTextNode(control.Caption);
            band.AddGroup(header); 
            var info = _adjuster.Find(control);
            if (DataSet != null)
            {
                var field = gridRow.AddDataField(new DocFormDataSetField(DataSet, control));
                if (info != null)
                {
                    header.ColSpan = info.ColSpan;
                    field.ColSpan = info.ColSpan;
                }

            }
            else if (SqlDataSet != null)
            {
                if (!(control is BizDataControl)) return;

                var attr = SqlDataSet.Reader.Query.FindAttribute(((BizDataControl) control).AttributeDefId ?? Guid.Empty);
                if (attr != null)
                {
                    var field = gridRow.AddDataField(new SqlQueryDataSetField(SqlDataSet, attr, control));
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
