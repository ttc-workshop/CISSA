using System;
using System.Collections.Generic;
using Intersoft.Cissa.Report.Common;

namespace Intersoft.Cissa.Report.Xls
{
    public class XlsGroup : XlsItem
    {
        private readonly List<XlsItem> _items = new List<XlsItem>();

        public List<XlsItem> Items { get { return _items; } }

        public XlsCell AddCell(XlsCell cell)
        {
            Items.Add(cell);
            return cell;
        }

        public XlsGroup AddGroup(XlsGroup group)
        {
            Items.Add(group);
            return group;
        }

        public XlsCell AddEmptyCell(int colSpan = 0, int rowSpan = 0)
        {
            return AddCell(new XlsEmptyCell(colSpan, rowSpan));
        }

        public XlsCell AddText(string text, int colSpan = 0, int rowSpan = 0)
        {
            return AddCell(new XlsText(text, colSpan, rowSpan));
        }

        public XlsCell AddDateTime(DateTime value, int colSpan = 0, int rowSpan = 0)
        {
            return AddCell(new XlsDateTime(value, colSpan, rowSpan));
        }

        public XlsCell AddInt(int value, int colSpan = 0, int rowSpan = 0)
        {
            return AddCell(new XlsInt(value, colSpan, rowSpan));
        }

        public XlsCell AddFloat(double value, int colSpan = 0, int rowSpan = 0)
        {
            return AddCell(new XlsFloat(value, colSpan, rowSpan));
        }

        public XlsCell AddBool(bool value, int colSpan = 0, int rowSpan = 0)
        {
            return AddCell(new XlsBool(value, colSpan, rowSpan));
        }

        public XlsCell AddDataField(DataSetField field, int colSpan = 0, int rowSpan = 0)
        {
            return AddCell(new XlsDataField(field, colSpan, rowSpan));
        }

        public override void WriteTo(XlsWriter writer, int param = 0)
        {
            var oldStyle = writer.MergeStyle(Style);
            try
            {
                foreach (var item in Items) item.WriteTo(writer, param);
            }
            finally
            {
                writer.Style = oldStyle;
            }
        }

        protected override void DoDispose()
        {
            foreach (var item in Items)
            {
                item.Dispose();
            }
        }
    }
}