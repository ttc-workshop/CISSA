using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public class XlsTableFormAdjustInfo : XlsFormAdjustInfo
    {
        private readonly List<XlsTableFormControlAdjustInfo> _columns = new List<XlsTableFormControlAdjustInfo>();
        public override List<XlsColumnItemAdjustInfo> Columns { get { return new List<XlsColumnItemAdjustInfo>(_columns); } }

        public Guid FormId { get; private set; }

        public XlsTableFormAdjustInfo(BizControl form)
        {
            FormId = form.Id;

            if (form.Children != null)
                foreach (var control in form.Children)
                    AddControlBand(control);
        }

        public /*override*/ int GetColumnSize(int count)
        {
            var result = 0;
            for (var i = 0; i <= Math.Min(Columns.Count - 1, count); i++)
            {
                var column = Columns[i];
                result += column.Size;
            }
            return result;
        }

        /*internal override IEnumerable<XlsFormControlSizeInfo> GetControlGreatThen(int size)
        {
            var result = 0;
            var i = 0;
            foreach (var control in _columns)
            {
                result += control.Size;
                if (result > size)
                {
                    yield return new XlsFormControlSizeInfo(i, control, result);
                    break;
                }
                i++;
            }
        }*/

        /*public override void AdjustColumn(int columnNo, int size)
        {
            var total = 0;
            var prevNo = 0;
            foreach (var column in _columns)
            {
                total += column.Size;

                if (total == size)
                {
                    column.ColumnNo = columnNo;
                    column.ColSpan = columnNo - prevNo;
                    break;
                }
                if (total > size)
                    break;

                prevNo = column.ColumnNo;
            }
        }*/

        /*public override int GetTotalSize()
        {
            return Columns.Sum(c => c.Size);
        }*/

        public override void SetTotalSize(int totalSize)
        {
            // Do nothing;
        }

        protected void AddControlBand(BizControl control)
        {
            if (control.Invisible) return;

            if (control is BizPanel)
            {
                foreach (var child in control.Children)
                {
                    AddControlBand(child);
                }
            }
            else if (control is BizTableColumn ||
                     control is BizDocumentControl || control is BizDocumentListForm)
            {
                if (control.Children != null)
                {
                    foreach (var child in control.Children)
                    {
                        AddControlBand(child);
                    }
                }
            }
            else if (control is BizDataControl)
                AddControlColumn(control);
            else
                if (control.Children != null)
                {
                    foreach (var child in control.Children)
                    {
                        AddControlBand(child);
                    }
                }
        }

        protected void AddControlColumn(BizControl control)
        {
            if (control.Invisible) return;

            _columns.Add(new XlsTableFormControlAdjustInfo(control));
        }
    }
}