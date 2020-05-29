using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;
using Novacode;
using NPOI.HSSF.Record;
using NPOI.HSSF.UserModel;

namespace Intersoft.Cissa.Report.WordDoc
{
    public class DataSetResetEventArgs : EventArgs
    {
        public DataSet DataSet { get; set; }
    }

    public sealed class WordDocBuilder
    {
        private WordDocDef Def { get; set; }

        public WordDocBuilder(WordDocDef def)
        {
            Def = def;
            _style = new ContentStyle(def.Style);
        }

        private readonly IDictionary<ContentStyle, Formatting> _formattings = new Dictionary<ContentStyle, Formatting>();

        private ContentStyle _style;

        public void Build(Stream stream)
        {
            var document = DocX.Create(stream);
            switch (Def.PaperSize)
            {
                case PaperSize.A3:
                    document.PageHeight = WordDocDef.A4PaperHeight*2;
                    document.PageWidth = WordDocDef.A4PaperWidth*2;
                    break;
                /*case PaperSize.A4:
                    document.PageHeight = WordDocDef.A4PaperHeight;
                    document.PageWidth = WordDocDef.A4PaperWidth;
                    break;*/
                case PaperSize.A5:
                    document.PageHeight = WordDocDef.A4PaperHeight/2;
                    document.PageWidth = WordDocDef.A4PaperWidth/2;
                    break;
            }
            if (Def.Orientation != PageOrientation.Portrait && document.PageLayout != null)
                document.PageLayout.Orientation = Orientation.Landscape;

            if (Def.MarginBottom > 0.01)
                document.MarginBottom = Def.MarginBottom;
            if (Def.MarginLeft > 0.01)
                document.MarginLeft = Def.MarginLeft;
            if (Def.MarginTop > 0.01)
                document.MarginTop = Def.MarginTop;
            if (Def.MarginRight > 0.01)
                document.MarginRight = Def.MarginRight;

            BuildItems(Def.Items, document);

            document.Save();
        }

        private IDictionary<PageBreak, int> _pageBreakSteps = new Dictionary<PageBreak, int>();

        private void BuildItems(IList<WordDocItemDef> items, DocX document, Paragraph p = null)
        {
            foreach (var item in items)
            {
                var tableDef = item as WordTableDef;
                if (tableDef != null && tableDef.Sections.Count >= 0 && tableDef.GetColCount() >= 0)
                {
                    BuildTable(tableDef, document);
                }
                else if (item is WordParagraphDef)
                {
                    var paraDef = (WordParagraphDef) item;
                    var oldStyle = MergeStyle(paraDef.Style);
                    try
                    {
//                        var format = GetFormatting();
                        var para = AppendParagraph(document /*.InsertParagraph()*/);
                        foreach (var paraItem in paraDef.Items)
                        {
                            if (paraItem is WordContentItemDef)
                                AddParagraphContent(para, (WordContentItemDef) paraItem);
                            else if (paraItem is WordRepeatSectionDef)
                            {
                                var repeatSection = (WordRepeatSectionDef)paraItem;
                                if (repeatSection.ResetDatas)
                                {
                                    var args = new DataSetResetEventArgs { DataSet = repeatSection.Datas };
                                    OnBeforeDataSetReset(args);
                                    repeatSection.Datas.Reset();
                                    OnAfterDataSetReset(args);
                                }

                                while (!repeatSection.Datas.Eof())
                                {
                                    BuildItems(repeatSection.Items, document, para);
                                    repeatSection.Datas.Next();
                                }
                            }
                            else if (paraItem is WordSectionDef)
                                BuildItems(((WordSectionDef) paraItem).Items, document, para);
                        }
                    }
                    finally
                    {
                        SetStyle(oldStyle);
                    }
                }
                else if (item is WordRepeatSectionDef)
                {
                    var repeatSection = (WordRepeatSectionDef) item;
                    var oldStyle = MergeStyle(item.Style);
                    try
                    {
                        if (repeatSection.ResetDatas)
                        {
                            var args = new DataSetResetEventArgs {DataSet = repeatSection.Datas};
                            OnBeforeDataSetReset(args);
                            repeatSection.Datas.Reset();
                            OnAfterDataSetReset(args);
                        }

                        while (!repeatSection.Datas.Eof())
                        {
                            BuildItems(repeatSection.Items, document, p);
                            repeatSection.Datas.Next();
                        }
                    }
                    finally
                    {
                        SetStyle(oldStyle);
                    }
                }
                else if (item is WordSectionDef)
                {
                    BuildItems(((WordSectionDef) item).Items, document, p);
                }
                else if (item is PageBreak)
                {
                    var pageBreak = (PageBreak) item;
                    int step;
                    if (!_pageBreakSteps.TryGetValue(pageBreak, out step))
                    {
                        step = pageBreak.Step;
                        _pageBreakSteps[pageBreak] = step;
                    }
                    step--;
                    if (step <= 0)
                    {
                        document.InsertSectionPageBreak();
                        _pageBreakSteps[pageBreak] = pageBreak.Step;
                    }
                }
                else if (item is WordContentItemDef)
                {
                    if (p == null)
                        p = AppendParagraph(document);
                    AddParagraphContent(p, (WordContentItemDef) item);
                }
            }
        }

        private void OnBeforeDataSetReset(DataSetResetEventArgs e)
        {
            var handler = BeforeDataSetReset;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnAfterDataSetReset(DataSetResetEventArgs e)
        {
            var handler = AfterDataSetReset;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<DataSetResetEventArgs> BeforeDataSetReset;
        public event EventHandler<DataSetResetEventArgs> AfterDataSetReset;

        private void BuildTable(WordTableDef tableDef, DocX document)
        {
            var oldStyle = MergeStyle(tableDef.Style);
            try
            {
                var table = document.InsertTable(1 /*tableDef.GetRowCount() + 1*/, tableDef.GetColCount());
                if (_style.HasBorders && _style.Borders == TableCellBorder.All)
                    table.Design = TableDesign.TableGrid;
                else
                    table.Design = TableDesign.TableNormal;
                table.AutoFit = tableDef.FitWidth ? AutoFit.Window : AutoFit.Contents;
                var firstRow = true;

                foreach (var sectionDef in tableDef.Sections)
                {
                    var def = sectionDef as WordTableRepeatSectionDef;
                    if (def != null)
                    {
                        var repeatSection = def;
                        var oldSectionStyle = MergeStyle(def.Style);
                        try
                        {
                            if (repeatSection.ResetDatas)
                            {
                                var args = new DataSetResetEventArgs { DataSet = repeatSection.Datas };
                                OnBeforeDataSetReset(args);
                                repeatSection.Datas.Reset();
                                OnAfterDataSetReset(args);
                            }

                            while (!repeatSection.Datas.Eof())
                            {
                                BuildTableSectionRows(def, table, tableDef, ref firstRow);
                                repeatSection.Datas.Next();
                            }
                        }
                        finally
                        {
                            SetStyle(oldSectionStyle);
                        }
                    }
                    else
                    {
                        var oldSectionStyle = MergeStyle(sectionDef.Style);
                        try
                        {
                            BuildTableSectionRows(sectionDef, table, tableDef, ref firstRow);
                        }
                        finally
                        {
                            SetStyle(oldSectionStyle);
                        }
                    }
//                for (var r = 0; r <= tableDef.GetRowCount(); r++)
                }
            }
            finally
            {
                SetStyle(oldStyle);
            }
        }

        private void BuildTableSectionRows(WordTableSectionDef sectionDef, Table table, WordTableDef tableDef, ref bool firstRow)
        {
            foreach (var rowDef in sectionDef.GetRows())
            {
                //if (tableDef.Rows.ContainsKey(r))
                {
                    //var rowDef = tableDef.Rows[r];
                    var oldRowStyle = MergeStyle(rowDef.Style);
                    try
                    {
                        var row = firstRow ? table.Rows[0] : table.InsertRow();
                        firstRow = false;
                        for (var c = 0; c < tableDef.GetColCount(); c++)
                        {
                            if (rowDef.Cells.ContainsKey(c))
                            {
                                var cellDef = rowDef.Cells[c];
                                var oldCellStyle = MergeStyle(cellDef.Style);
                                try
                                {
                                    var cellPara = AppendParagraph( /*table.Rows[r]*/row.Cells[c]);
                                    //SetParagraphFormat(cellPara);

                                    foreach (var itemDef in cellDef.Items)
                                    {
                                        if (itemDef is WordContentItemDef)
                                            AddParagraphContent(cellPara, (WordContentItemDef) itemDef);
                                    }
                                }
                                finally
                                {
                                    SetStyle(oldCellStyle);
                                }
                            }
                            else
                                AppendParagraph(row.Cells[c]);
                        }
                    }
                    finally
                    {
                        SetStyle(oldRowStyle);
                    }
                }
            }
        }

        private void SetParagraphFormat(Paragraph para)
        {
            if (para != null)
            {
                switch (_style.HAlign)
                {
                    case HAlignment.Center:
                        para.Alignment = Alignment.center;
                        break;
                    case HAlignment.FullWidth:
                        para.Alignment = Alignment.both;
                        break;
                    case HAlignment.Left:
                        para.Alignment = Alignment.left;
                        break;
                    case HAlignment.Right:
                        para.Alignment = Alignment.right;
                        break;
                }
                if (_style.HasFontStyle() && _style.FontStyle.HasFlag(FontStyle.Bold))
                    para.Bold();
                if (_style.HasFontStyle() && _style.FontStyle.HasFlag(FontStyle.Italic))
                    para.Italic();

                para.FontSize(12 + (_style.HasFontDSize() ? _style.FontDSize : 0));
                if (_style.HasFontName())
                    para.Font(new FontFamily(_style.FontName));
                if (_style.HasFontColor())
                {
                    var p = new HSSFPalette(new PaletteRecord());
                    var c = p.GetColor(_style.FontColor);
                    var triplet = c.GetTriplet();
                    para.Color(Color.FromArgb(triplet[0], triplet[1], triplet[2]));
                }
            }
        }

        private void AddParagraphContent(Paragraph para, WordContentItemDef content)
        {
//            var def = content as WordParagraphTextDef;
            if (content != null)
            {
                var oldStyle = MergeStyle(content.Style);
                try
                {
                    var formatting = GetFormatting();

                    para.InsertText(content.GetText()/* + "\n"*/, false, formatting);
                    switch (_style.HAlign)
                    {
                        case HAlignment.Center:
                            para.Alignment = Alignment.center;
                            break;
                        case HAlignment.FullWidth:
                            para.Alignment = Alignment.both;
                            break;
                        case HAlignment.Left:
                            para.Alignment = Alignment.left;
                            break;
                        case HAlignment.Right:
                            para.Alignment = Alignment.right;
                            break;
                    }
                    //else
                    //    para.InsertText(def.Text, false, formatting);
                }
                finally
                {
                    SetStyle(oldStyle);
                }
            }
        }

        private Paragraph AppendParagraph(DocX document)
        {
            var formatting = GetFormatting();
            
            var para = document.InsertParagraph(string.Empty, false, formatting);
            switch (_style.HAlign)
            {
                case HAlignment.Center:
                    para.Alignment = Alignment.center;
                    break;
                case HAlignment.FullWidth:
                    para.Alignment = Alignment.both;
                    break;
                case HAlignment.Left:
                    para.Alignment = Alignment.left;
                    break;
                case HAlignment.Right:
                    para.Alignment = Alignment.right;
                    break;
            }
            return para;
        }
        private Paragraph AppendParagraph(Cell cell)
        {
            var para = cell.Paragraphs.FirstOrDefault();
            if (para == null)
            {
                var formatting = GetFormatting();

                para = cell.InsertParagraph(string.Empty, false, formatting);

                switch (_style.HAlign)
                {
                    case HAlignment.Center:
                        para.Alignment = Alignment.center;
                        break;
                    case HAlignment.FullWidth:
                        para.Alignment = Alignment.both;
                        break;
                    case HAlignment.Left:
                        para.Alignment = Alignment.left;
                        break;
                    case HAlignment.Right:
                        para.Alignment = Alignment.right;
                        break;
                }
            }
            else
                SetParagraphFormat(para);

            if (_style.HasBgColor())
            {
                var p = new HSSFPalette(new PaletteRecord());
                var c = p.GetColor(_style.BgColor);
                var triplet = c.GetTriplet();
                cell.FillColor = Color.FromArgb(triplet[0], triplet[1], triplet[2]);
            }

            return para;
        }

        private Formatting GetFormatting()
        {
            var result = (from f in _formattings where f.Key.CompareTo(_style) == 0 select f.Value).FirstOrDefault();

            if (result == null)
            {
                result = new Formatting
                {
                    Bold = _style.HasFontStyle() && _style.FontStyle.HasFlag(FontStyle.Bold),
                    Italic = _style.HasFontStyle() && _style.FontStyle.HasFlag(FontStyle.Italic),
                    UnderlineStyle = _style.HasFontStyle() && _style.FontStyle.HasFlag(FontStyle.Underline) ? UnderlineStyle.singleLine : UnderlineStyle.none,

                    Size = 12 + (_style.HasFontDSize() ? _style.FontDSize : 0)
                };
                if (_style.HasFontName())
                    result.FontFamily = new FontFamily(_style.FontName);
                if (_style.HasFontColor())
                {
                    var p = new HSSFPalette(new PaletteRecord()); 
                    var c = p.GetColor(_style.FontColor);
                    var triplet = c.GetTriplet();
                    result.FontColor = Color.FromArgb(triplet[0], triplet[1], triplet[2]);
                }
                /*if (_style.HasBgColor())
                    result.Highlight = Highlight.blue;*/

                _formattings.Add(new ContentStyle(_style), result);
            }
            return result;
        }

        private void SetStyle(ContentStyle style)
        {
            _style = style;
        }

        private ContentStyle MergeStyle(ContentStyle style)
        {
            var oldStyle = new ContentStyle(_style);
            if (style != null)
                _style.MulMerge(style);
            return oldStyle;
        }
    }
}