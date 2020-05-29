using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models
{
    public class ManagedTableForm
    {
        public Guid Id { get; private set; }
        public BizTableForm Form { get; private set; }
        public Guid? DocumentDefId { get; private set; }
        public Guid? AttributeDefId { get; private set; }
        public BizDocumentListForm Owner { get; private set; }

        public Guid? FormId { get; private set; }
        public String Caption { get; private set; }
        public Guid? DocId { get; private set; }

        public BizDetailForm FilterForm { get; set; }
//        private readonly IDocManager _docManager;
//        private readonly IPresentationManager _presentationManager;

        public IList<Column> Columns { get; private set; }
        public List<Guid> RowDocs { get; set; }
        public List<BizControl> RowForms { get; set; }
        public int HeaderRowCount { get; set; }

        public int PageSize { get; set; }
        public int PageNo { get; set; }
        public int PageCount { get; set; }
        public int RowCount { get; set; }

//        public IList<Row> Rows { get; private set; }

        public Guid? DocStateId { get; set; }

        public bool IsGridPanel { get; private set; }

        public ManagedTableForm(BizTableForm bizTableForm, BizDocumentListForm owner)
        {
            Owner = owner;
            Id = Owner.Id;
            Form = bizTableForm;
            AttributeDefId = Owner.AttributeDefId;

            Caption = Owner.Caption;
            FormId = bizTableForm.FormId;

            PageSize = bizTableForm.PageSize;
            PageCount = bizTableForm.PageSize;
            PageNo = bizTableForm.PageNo;
            //PageCount = 1;
            HeaderRowCount = 1;

            if (bizTableForm.Children != null)
            {
                Columns = new List<Column>();
                AddColumnChildren(null, Form);
            }
        }

        public ManagedTableForm(BizTableForm bizTableForm, BizDocumentListForm owner, Guid? docId, int pageNo, int pageSize)
        {
            Owner = owner;
            Id = Owner.Id;
            Form = bizTableForm;
            AttributeDefId = Owner.AttributeDefId;

            Caption = Owner.Caption;
            FormId = bizTableForm.FormId;
            DocId = docId;

            PageNo = pageNo;
            PageSize = pageSize;
            PageCount = 1;
            HeaderRowCount = 1;

            if (bizTableForm.Children != null)
            {
                Columns = new List<Column>();
                AddColumnChildren(null, Form);
            }
        }

        public ManagedTableForm(BizTableForm bizTableForm)
        {
            Id = bizTableForm.Id;
            Form = bizTableForm;

            DocumentDefId = bizTableForm.DocumentDefId;
            AttributeDefId = null;

            Caption = bizTableForm.Caption;
            FormId = bizTableForm.FormId;

            PageSize = bizTableForm.PageSize;

            PageCount = bizTableForm.PageSize;//1;
            PageNo = bizTableForm.PageNo;
            HeaderRowCount = 1;

            if (bizTableForm.Children != null)
            {
                Columns = new List<Column>();
                AddColumnChildren(null, Form);
            }

            RowDocs = new List<Guid>();
        }

        public ManagedTableForm(BizPanel panel)
        {
            Id = panel.Id;
            IsGridPanel = true;
            //Form = bizTableForm;

            //DocumentDefId = bizTableForm.DocumentDefId;
            //AttributeDefId = null;

            Caption = panel.Caption;
            //FormId = bizTableForm.FormId;

            PageSize = 0;
            PageNo = 0;
            PageCount = 1;
            HeaderRowCount = 1;

            Columns = new List<Column>();
            // Форимрование шапки
            if (panel.Children != null)
            {
                var firstRowPanel = panel.Children.FirstOrDefault() as BizPanel;
                if (firstRowPanel != null)
                    AddColumnChildren(null, firstRowPanel);
                else
                    foreach (var control in panel.Children.Where(c => !(c is BizPanel)))
                    {
                        AddColumnChildren(null, control);
                    }

                RowForms = new List<BizControl>();
                var rowPanel = new BizPanel {Children = new List<BizControl>()};
                foreach (var child in panel.Children)
                {
                    if (child is BizPanel)
                    {
                        if (rowPanel.Children.Count > 0)
                        {
                            RowForms.Add(rowPanel);
                            rowPanel = new BizPanel {Children = new List<BizControl>()};
                        }
                        RowForms.Add(child);
                    }
                    else
                        rowPanel.Children.Add(child);
                }
                if (rowPanel.Children.Count > 0) RowForms.Add(rowPanel);
            }
        }

        public ICollection<Column> GetColumnForRow(int rowNo)
        {
            var list = new List<Column>();

            FillRowColumns(list, Columns, 0, rowNo);

            return list;
        }

        public void SetRowForm(BizControl row)
        {
            Columns = new List<Column>();
            /*foreach (var control in row.Children)
            {
                if (control.Invisible) continue;
                if (control is BizTableColumn) AddColumnChildren(null, control);
                else if (control is BizDocumentControl && control.Children != null &&
                         control.Children.Count > 0)
                    AddColumnChildren(null, control);
                else 
                    Columns.Add(new Column {Control = control, Id = control.Id});
            }*/
            AddColumnChildren(null, row);
        }
//        public void SetDoc(Guid docId)
//        {
//            SetDoc(_docManager.DocumentLoad(docId));
//        }
//
//        public void SetDoc(Doc doc)
//        {
//            Form = (BizTableForm) _presentationManager.SetFormDoc(Form, doc);
//
//            Columns = new List<Column>();
//            foreach (var control in Form.Children)
//            {
//                if (control.Invisible) continue;
//                if (control is BizTableColumn) AddColumnChildren(control);
//                else if (control is BizDocumentControl && control.Children != null && control.Children.Count > 0)
//                    AddColumnChildren(control);
//                else
//                    Columns.Add(new Column {Control = control, Id = control.Id});
//            }
//        }

        internal void AddColumnChildren(Column parent, BizControl control, int nestCount = 1)
        {
            if (control.Children != null)
            {
                if (HeaderRowCount < nestCount) HeaderRowCount = nestCount;

                foreach (var child in control.Children)
                {
                    if (child.Invisible || child.Options.HasFlag(BizControlOptionFlags.Hidden)) continue;

                    if (child is BizTableColumn) AddColumnChildren(parent, child, nestCount);
                    else if (child is BizDocumentControl && child.Children != null && child.Children.Count > 0)
                        AddColumnChildren(parent, child, nestCount);
                    else if (child is BizDocumentListForm && child.Children != null && child.Children.Count > 0)
                        AddColumnChildren(parent, child, nestCount);
                    else if (child is BizPanel && (((BizPanel)child).LayoutId == 2 || ((BizPanel)child).LayoutId == 0) &&
                            child.Children != null && child.Children.Count > 0)
                    {
                        var panelColumn = new Column { Control = child, Id = child.Id, RowNo = nestCount - 1 };
                        if (parent == null)
                            Columns.Add(panelColumn);
                        else
                            parent.Children.Add(panelColumn);
                        AddColumnChildren(panelColumn, child, nestCount + 1);
                    }
                    else
                    {
                        var childColumn = new Column {Control = child, Id = child.Id, RowNo = nestCount - 1};

                        if (parent == null)
                            Columns.Add(childColumn);
                        else 
                            parent.Children.Add(childColumn);
                    }
                }

                SetColumnRowSpans(Columns, HeaderRowCount);
            }
        }

        private static void FillRowColumns(ICollection<Column> results, IEnumerable<Column> columns, int curRow, int rowNo)
        {
            if (columns == null) return;
            if (curRow > rowNo) return;

            foreach(var column in columns)
            {
                if (curRow == rowNo)
                    results.Add(column);
                else
                {
                    FillRowColumns(results, column.Children, curRow + 1, rowNo);
                }
            }
        }

        private static int SetColumnRowSpans(IEnumerable<Column> columns, int rowCount)
        {
            var maxRows = 0;

            foreach(var column in columns)
            {
                int rows;

                if (column.Children == null || column.Children.Count == 0)
                {
                    column.RowSpan = rowCount;
                    rows = rowCount;
                }
                else
                {
                    rows = SetColumnRowSpans(column.Children, rowCount - 1) + 1;
//                    if (rows < (rowCount - 1))
//                        column.RowSpan = rowCount - rows;
                }
                if (maxRows < rows) maxRows = rows;
            }

            return maxRows;
        }

        public class Row
        {
            public Guid DocId { get; set; }
            public IList<Cell> Cells { get; set; }
        }

        public class Cell
        {
            public Guid ColumnId { get; set; }
            public BizControl Value { get; set; }
        }

        public class Column
        {
            public Guid Id { get; set; }
            public BizControl Control { get; set; }
            private readonly List<Column> _children = new List<Column>();
            public List<Column> Children { get { return _children; } }
            public int RowNo { get; set; }
            public int RowSpan { get; set; }

            public string GetHeaderAttributes()
            {
                string res = "";

                if (Children != null && Children.Count > 0)
                    res = String.Format("colspan={0}", Children.Sum(c => c.GetChildCount()));

                if (RowSpan > 1)
                {
                    if (res.Length > 0) res += " ";
                    res += String.Format("rowspan={0}", RowSpan);
                }

                return res;
            }

            private int GetChildCount()
            {
                if (Children != null && Children.Count > 0)
                    return Children.Sum(c => c.GetChildCount());
                return 1;
            }
        }
    }

    public class ManagedTableFormRow
    {
        public Guid Id { get; private set; }
        public BizForm Form { get; private set; }

        public IList<ManagedTableForm.Column> Columns { get; private set; }

        public ManagedTableFormRow(BizForm form)
        {
            Id = form.Id;
            Form = form;

            if (form.Children != null)
            {
                Columns = new List<ManagedTableForm.Column>();

                /*foreach (var control in Form.Children)
                {
                    if (control.Invisible) continue;
                    if (control is BizTableColumn) AddColumnChildren(null, control);
                    else if (control is BizDocumentControl && control.Children != null && control.Children.Count > 0)
                        AddColumnChildren(null, control);
                    else
                        Columns.Add(new ManagedTableForm.Column { Control = control, Id = control.Id });
                }*/
                AddColumnChildren(null, form);
            }
        }

        private void AddColumnChildren(ManagedTableForm.Column parent, BizControl control)
        {
            if (control.Children != null)
                foreach (var child in control.Children)
                {
                    if (child.Invisible || child.Options.HasFlag(BizControlOptionFlags.Hidden)) continue;

                    if (child is BizTableColumn) AddColumnChildren(parent, child);
                    else if (child is BizDocumentControl && child.Children != null && child.Children.Count > 0)
                        AddColumnChildren(parent, child);
                    else if (child is BizPanel && ((BizPanel)child).LayoutId == 2 &&
                            child.Children != null && child.Children.Count > 0)
                    {
                        var panelColumn = new ManagedTableForm.Column { Control = child, Id = child.Id };
                        Columns.Add(panelColumn);
                        AddColumnChildren(panelColumn, child);
                    }
                    else
                    {
                        var childColumn = new ManagedTableForm.Column { Control = child, Id = child.Id };

                        if (parent == null)
                            Columns.Add(childColumn);
                        else
                            parent.Children.Add(childColumn);
                    }
                }
        }
    }
}