using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.Cissa.Report.Defs;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;

namespace Intersoft.Cissa.Report.Xls.Adjuster
{
    public class XlsFormColumnAdjuster
    {
        public static int PanelDivideSize = 5;

        private readonly List<XlsFormAdjustInfo> _forms = new List<XlsFormAdjustInfo>();
        public List<XlsFormAdjustInfo> Forms { get { return _forms; } }

        private readonly IDictionary<int, int> _columnSizes = new Dictionary<int, int>();
        public IDictionary<int, int> ColumnSizes { get { return _columnSizes; } } 

        public void AddTableForm(BizControl form)
        {
            Forms.Add(new XlsTableFormAdjustInfo(form));
        }

        public void AddReportDef(ReportDef report)
        {
            Forms.Add(new XlsReportDefAdjustInfo(report));
        }

        public void AddReportSection(XlsGridReportSectionItem section)
        {
            var textSection = section as XlsGridReportSectionText;
            if (textSection != null)
                Forms.Add(new XlsGridReportSectionTextAdjustInfo(textSection));
            var tableSection = section as XlsGridReportSectionTable;
            if (tableSection != null)
                Forms.Add(new XlsGridReportSectionTableAdjustInfo(tableSection));
        }

        /*public void AddForm(BizControl form)
        {
            if (form is BizTableForm) AddTableForm(form);
            else Forms.Add(new XlsGridPanelAdjustInfo(form));
        }*/

        public void Adjust()
        {
            var columnNo = 1;
            var prevSize = 0;
            _columnSizes.Clear();

            if (Forms.Count == 0) return;

            var totalSize = Forms.Max(f => f.GetTotalSize());
            Forms.ForEach(f => f.SetTotalSize(totalSize));

            var controls = new List<XlsFormControlSizeInfo>(Forms.SelectMany(t => t.GetControlGreatThen(0)));

            while (controls.Count > 0)
            {
                var size = controls.Min(c => c.TotalSize);
                Forms.ForEach(t => t.AdjustColumn(columnNo, size));
                controls = new List<XlsFormControlSizeInfo>(Forms.SelectMany(t => t.GetControlGreatThen(size)));
                ColumnSizes.Add(columnNo, size - prevSize);
                columnNo++;
                prevSize = size;
            }
        }

        public XlsColumnItemAdjustInfo Find(object control, int no)
        {
            //return Forms.SelectMany(t => t.Columns).FirstOrDefault(c => c.Id == id);
            //return Forms.Where(f => f.FindControl(id) != null).Select(f => f.FindControl(id)).FirstOrDefault();
            return Forms.Select(form => form.FindControl(control, no)).FirstOrDefault(item => item != null);
        }
        public XlsColumnItemAdjustInfo Find(object control)
        {
            //return Forms.SelectMany(t => t.Columns).FirstOrDefault(c => c.Id == id);
            //return Forms.Where(f => f.FindControl(id) != null).Select(f => f.FindControl(id)).FirstOrDefault();
            return Forms.Select(form => form.FindControl(control)).FirstOrDefault(item => item != null);
        }


        public int GetColumnSize(int no)
        {
            return ColumnSizes[no];
        }

        public int ColumnCount { get { return ColumnSizes.Count; } }

        public int TotalSize { get { return ColumnSizes.Values.Sum(); } }

        public int GetColumnSpanForPercent(int percent)
        {
            var total = TotalSize;
            var size = 0;
            for (var i = 1; i <= ColumnCount; i++)
            {
                size += ColumnSizes[i];
                if ((size*100)/total == percent) 
                    return i;

                if ((size * 100) / total > percent)
                {
                    if (i == 1) return 1;
                    return i - 1;
                }
            }
            return ColumnCount > 1 ? ColumnCount - 1 : 1;
        }
    }

    public enum XlsControlLayoutType { Vertical, Horizontal, ValueOnly }
    public enum XlsColumnPartType { Value, Caption, Divider }

    public class XlsGridPanelRowAdjustInfo
    {
        private readonly List<XlsTableFormControlAdjustInfo> _controls = new List<XlsTableFormControlAdjustInfo>();
        public List<XlsTableFormControlAdjustInfo> Controls { get { return _controls; } }
        public XlsGridPanelRowAdjustInfo()
        {
        }
    }
/*
    
    public class XlsGridPanelAdjustInfo : XlsFormAdjustInfo
    {
        private readonly List<XlsGridPanelRowAdjustInfo> _rows = new List<XlsGridPanelRowAdjustInfo>();
        public List<XlsGridPanelRowAdjustInfo> Rows { get { return _rows; } }

        public XlsGridPanelAdjustInfo(BizControl form) : base(form)
        {
            if (form.Children != null)
                foreach (var control in form.Children)
                    AddControl(control, 0);
            else
                AddControl(form, 0);
        }

        private int AddControl(BizControl control, int row)
        {
            if (control.Invisible) return row;

            if (control is BizForm)
            {
                if (control.Children != null)
                    foreach (var child in control.Children)
                        row = AddControl(child, row);
            }
            else if (control is BizPanel)
            {
                var panel = (BizPanel) control;
                if (panel.LayoutId == (short) LayoutType.Default || panel.LayoutId == (short) LayoutType.Detail || panel.LayoutId == (short)LayoutType.Vertical)
                {
                    if (control.Children != null)
                        foreach (var child in control.Children)
                            row = AddControl(child, row);
                }
                else if (panel.LayoutId == (short) LayoutType.Horizontal)
                {
                    var maxRow = 0;
                    if (control.Children != null)
                        foreach (var child in control.Children)
                            maxRow = Math.Max(AddControl(child, row), maxRow);
                    row = maxRow;
                }
                else if (panel.LayoutId == (short) LayoutType.Grid)
                {
                    if (panel.Children != null)
                    {
                        var firstRowPanel = panel.Children.FirstOrDefault() as BizPanel;
                        if (firstRowPanel != null)
                            AddColumnChildren(null, firstRowPanel);
                        else
                            foreach (var child in panel.Children.Where(c => !(c is BizPanel)))
                            {
                                AddColumnChildren(null, child);
                            }

                        RowForms = new List<BizControl>();
                        var rowPanel = new BizPanel { Children = new List<BizControl>() };
                        foreach (var child in panel.Children)
                        {
                            if (child is BizPanel)
                            {
                                if (rowPanel.Children.Count > 0)
                                {
                                    RowForms.Add(rowPanel);
                                    rowPanel = new BizPanel { Children = new List<BizControl>() };
                                }
                                RowForms.Add(child);
                            }
                            else
                                rowPanel.Children.Add(child);
                        }
                        if (rowPanel.Children.Count > 0) RowForms.Add(rowPanel);
                    }
                }
            }
            else if (control is BizDocumentControl)
            {
                if (control.Children != null && control.Children.Count > 0)
                    foreach (var child in control.Children)
                        row = AddControl(child, row);
                else
                {
                    var docControl = (BizDocumentControl) control;
                    if (docControl.DocForm != null)
                        row = AddControl(docControl.DocForm, row);
                }
            }
            else 
                AddControlCell(control, row);
            return row;
        }

        private void AddControlCell(BizControl control, int row)
        {
            var rowInfo = Rows.Count >= row ? Rows[row] : null;
            if (rowInfo == null)
            {
                if (row == Rows.Count)
                {
                    rowInfo = new XlsGridPanelRowAdjustInfo();
                    Rows.Add(rowInfo);
                }
                throw new ApplicationException("Строка не найдена!");
            }
            rowInfo.Controls.Add(new XlsFormControlAdjustInfo(control));
        }
    }
*/
}