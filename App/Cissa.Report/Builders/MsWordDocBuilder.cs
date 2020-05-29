using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Intersoft.Cissa.Report.Styles;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Novacode;

namespace Intersoft.Cissa.Report.Builders
{
    public class MsWordDocBuilder
    {
        public IDataContext DataContext { get; private set; }
        public MsWordDocBuilder(IDataContext dataContext)
        {
            DataContext = dataContext;
        }

        private readonly IDictionary<ContentStyle, Formatting> _formattings = new Dictionary<ContentStyle, Formatting>();

        private ContentStyle _style;

        private readonly IDictionary<Type, Action<XlsItem, DocX>> _methods = new Dictionary<Type, Action<XlsItem, DocX>>(); 

        public void BuildFromXlsDef(XlsDef def, Stream stream)
        {
            DocX document = DocX.Create(stream);

            _methods.Add(typeof(XlsArea), BuildArea);
            _methods.Add(typeof(XlsGrid), BuildGrid);
            _methods.Add(typeof(XlsRow), BuildRow);


            _style = new ContentStyle(def.Style);
            foreach (var area in def.Areas)
            {
                BuildArea(area, document);
            }
        }

        private void BuildRow(XlsItem item, DocX document)
        {
            var row = (XlsRow) item;
            var oldStyle = MergeStyle(item.Style);
            try
            {
                var formatting = GetFormatting();
                if (/*row.Items.Count < 2 &&*/ !row.Items.Exists(i => (i is XlsGroup)))
                {
                    var para = AppendParagraph(document);
                    foreach (var child in row.Items)
                    {
                        AppendParaText(child, para);
                    }
                }
                else
                {
                    var table = document.InsertTable(row.GetRows(), row.GetCols());
                    
                }
            }
            finally
            {
                SetStyle(oldStyle);
            }
        }

        private Paragraph AppendParagraph(DocX document)
        {
            var formatting = GetFormatting();
            var para = document.InsertParagraph(string.Empty, false, formatting);
            switch (_style.HAlign)
            {
                case HAlignment.Center:
                    para.Alignment = Novacode.Alignment.center;
                    break;
                case HAlignment.FullWidth:
                    para.Alignment = Novacode.Alignment.both;
                    break;
                case HAlignment.Left:
                    para.Alignment = Novacode.Alignment.left;
                    break;
                case HAlignment.Right:
                    para.Alignment = Novacode.Alignment.right;
                    break;
            }
            return para;
        }

        private void AppendParaText(XlsItem item, Paragraph para)
        {
            if (item is XlsCell)
            {
                var value = ((XlsCell) item).GetValue();
                para.Append(value != null ? value.ToString() : String.Empty);
            }
        }

        private void BuildArea(XlsItem arg1, DocX arg2)
        {
            BuildArea((XlsArea) arg1, arg2);
        }

        private void BuildGrid(XlsItem item, DocX document)
        {
            throw new NotImplementedException();
        }

        private void BuildArea(XlsArea area, DocX document)
        {
            var oldStyle = MergeStyle(area.Style);
            try
            {
                if (area is XlsGrid)
                {
                    while (!((XlsGrid) area).RowDatas.Eof())
                    {
                        BuildItems(area, document);
                        ((XlsGrid) area).RowDatas.Next();
                    }
                }
                else
                    BuildItems(area, document);
            }
            finally
            {
                SetStyle(oldStyle);
            }
        }

        private void BuildItems(XlsArea area, DocX document)
        {
            foreach (var item in area.Items)
            {
                if (item.GetRows() == 0)
                {
                    var formatting = GetFormatting();
                    document.InsertParagraph(string.Empty, false, formatting);
                    continue;
                }

                BuildItem(item, document);
            }
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
                    
                    Size = 12 + (_style.HasFontDSize() ? _style.FontDSize : 0)
                };
                /*if (_style.HasFontColor())
                    result.FontColor = (Color) _style.FontColor;*/
                _formattings.Add(new ContentStyle(_style), result);
            }
            return result;
        }

        private void BuildItem(XlsItem item, DocX document)
        {
            var method = (from m in _methods where m.Key == item.GetType() select m.Value).First();
            method(item, document);
        }

        public void SetStyle(ContentStyle style)
        {
            _style = style;
        }

        private ContentStyle MergeStyle(ContentStyle style)
        {
            var oldStyle = new ContentStyle(_style);
            _style.MulMerge(style);
            return oldStyle;
        }
    }
}
