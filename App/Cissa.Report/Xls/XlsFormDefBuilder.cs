using System;
using System.Drawing;
using System.Linq;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;
using Intersoft.Cissa.Report.Xls.Adjuster;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;
using NPOI.SS.UserModel;

namespace Intersoft.Cissa.Report.Xls
{
    public interface IXlsFormDefBuilder
    {
        BizForm Form { get; }
        XlsDef Build(Doc doc);
    }

    public interface IXlsFormDefBuilderFactory
    {
        IXlsFormDefBuilder Create(BizForm form);
    }

    public class XlsFormDefBuilder : IXlsFormDefBuilder //, IDisposable
    {
        public IDataContext DataContext { get; private set; }
        public BizForm Form { get; private set; }
        public Guid UserId { get; private set; }

        public IAppServiceProvider Provider { get; private set; }

//        private readonly IDocRepository _docRepo;
        private readonly IFormRepository _formRepo;

        private readonly ISqlQueryBuilderFactory _sqlQueryBuilderFactory;
        private readonly ISqlQueryReaderFactory _sqlQueryReaderFactory;
        private readonly IComboBoxEnumProvider _comboBoxValueProvider;

        private readonly XlsFormColumnAdjuster _adjuster = new XlsFormColumnAdjuster();

        public XlsFormDefBuilder(IDataContext dataContext, BizForm form, Guid userId)
        {
            DataContext = dataContext;
            Form = form;
            UserId = userId;

            var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = providerFactory.Create(dataContext, new UserDataProvider(UserId, ""));

            _formRepo = Provider.Get<IFormRepository>();
            _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
            _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
            _comboBoxValueProvider = Provider.Get<IComboBoxEnumProvider>();
        }

        public XlsFormDefBuilder(IAppServiceProvider provider, BizForm form)
        {
            Provider = provider;
            DataContext = provider.Get<IDataContext>();
            Form = form;
            //var userData = provider.Get<IUserDataProvider>();
            UserId = provider.GetCurrentUserId(); //userData.UserId;

            _formRepo = Provider.Get<IFormRepository>();
            _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
            _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
            _comboBoxValueProvider = Provider.Get<IComboBoxEnumProvider>();
        }

        public XlsDef Build(Doc doc)
        {
            /*GetFormRepo()*/
            _formRepo.SetFormDoc(Form, doc);

            return Build();
        }

        private int _captionColSpan = 1;
        private int _valueColSpan = 1;

        protected XlsDef Build()
        {
            var def = new XlsDef();
            try
            {
                AdjustDocListForms(Form);
                _adjuster.Adjust();
                _captionColSpan = _adjuster.GetColumnSpanForPercent(50);
                _valueColSpan = Math.Max(_adjuster.ColumnCount - _captionColSpan, 1);

                def.Style.FontName = "Arial Narrow";
                def.DefaultRowHeight = 13;

                var cell = def.AddArea().AddRow().AddText(Form.Caption);
                cell.Style.FontDSize = 2;
                cell.Style.FontStyle = FontStyle.Bold;
                cell.Style.FontColor = IndexedColors.DARK_BLUE.Index; // 18;
                cell.Style.HAlign = HAlignment.Center;
                cell.Style.AutoHeight = true;
                cell.ColSpan = _adjuster.ColumnCount;

                def.AddArea().AddEmptyRow();

//                var area = def.AddArea();

                if (Form.Children != null)
                    foreach (var control in Form.Children)
                    {
                        AddControlRow(def, /*area,*/ control);
                    }

                def.ColumnWidths = _adjuster.ColumnSizes;

                return def;
            } 
            catch
            {
                def.Dispose();
                throw;
            }
        }

        private void AdjustDocListForms(BizControl form)
        {
            if (form.Children != null)
                foreach (var control in form.Children)
                {
                    var listForm = control as BizDocumentListForm;
                    if (listForm != null)
                    {
                        if (listForm.Children != null && listForm.Children.Count > 0)
                            _adjuster.AddTableForm(listForm);
                        else if (listForm.TableForm != null)
                        {
                            _adjuster.AddTableForm(listForm.TableForm);
                        }
                    }
                    else
                        AdjustDocListForms(control);
                }
        }

        private void AddControlRow(XlsDef def, /*XlsGroup band,*/ BizControl control)
        {
            if (control.Invisible || control is BizButton) return;

            if (control is BizPanel || control is BizDocumentControl)
            {
                var cell = def.AddArea().AddRow().AddText(control.Caption); //band.AddGroup(new XlsRow()).AddText(control.Caption);
                cell.Style.FontStyle = FontStyle.Bold;
                cell.ColSpan = _adjuster.ColumnCount;
                cell.Style.FontColor = IndexedColors.DARK_BLUE.Index; // 18;
                cell.Style.HAlign = HAlignment.Center;
                cell.Style.AutoHeight = true;
                cell.Style.BgColor = IndexedColors.GREY_25_PERCENT.Index;

                foreach (var child in control.Children)
                {
                    AddControlRow(def, /*band,*/ child);
                }
            }
            else if (control is BizDocumentListForm)
            {
                var docListForm = (BizDocumentListForm) control;

                var cell = def.AddArea().AddRow().AddText(control.Caption);
                cell.Style.FontStyle = FontStyle.Bold;
                cell.ColSpan = _adjuster.ColumnCount;
                cell.Style.FontColor = IndexedColors.DARK_BLUE.Index; // 18;
                cell.Style.HAlign = HAlignment.Center;
                cell.Style.AutoHeight = true;
                cell.Style.BgColor = IndexedColors.GREY_25_PERCENT.Index;

                var tableForm = docListForm.Children != null && docListForm.Children.Count > 0
                    ? docListForm
                    : (BizControl) docListForm.TableForm;
                var docDefId = docListForm.TableForm != null
                    ? docListForm.TableForm.DocumentDefId ?? Guid.Empty
                    : Guid.Empty;

                SqlQueryDataSet dataSet;
                var sqb = _sqlQueryBuilderFactory.Create();
                if (docListForm.AttributeDefId != null)
                {
                    /*using (
                        var query = SqlQueryExBuilder.BuildAttrList(tableForm,
                            docListForm.DocumentId ?? Guid.Empty, docDefId, (Guid) docListForm.AttributeDefId, null,
                            null,
                            UserId, DataContext))*/
                    /*using (*/
                    var query = sqb.BuildAttrList(tableForm, docListForm.DocumentId ?? Guid.Empty, docDefId,
                        (Guid) docListForm.AttributeDefId, null, null);//)
                    {
                        query.WithNoLock = true;

                        dataSet = CreateDataSet(query);
                    }
                }
                else if (docListForm.FormAttributeDefId != null)
                {
                    /*using (
                        var query = SqlQueryExBuilder.BuildRefList(tableForm,
                            docListForm.DocumentId ?? Guid.Empty, docDefId, (Guid) docListForm.FormAttributeDefId, null,
                            null,
                            UserId, DataContext))*/
                    //using (
                    var query = sqb.BuildRefList(tableForm,
                        docListForm.DocumentId ?? Guid.Empty, docDefId, (Guid) docListForm.FormAttributeDefId, null,
                        null);//)
                    {
                        dataSet = CreateDataSet(query);
                    }
                }
                else
                    throw new ApplicationException(
                        "Недостаточно данных для формирования Excel-файла. Ошибка в данных формы!");

                /*int count;
                    var docs = docRepo.DocAttrListById(out count, docListForm.DocumentId ?? Guid.Empty,
                        docListForm.AttributeDefId ?? Guid.Empty, 0, 0);

                    dataSet = new DocFormDataSet(docs, tableForm, UserId, DataContext);*/

                var hRow = def.AddArea().AddRow();
                hRow.ShowAllBorders(true);
                hRow.Style.FontStyle = FontStyle.Bold;
                hRow.Style.HAlign = HAlignment.Center;
                hRow.Style.BgColor = IndexedColors.BLUE_GREY.Index; //48;
                hRow.Style.FontColor = IndexedColors.WHITE.Index;
                hRow.Style.WrapText = true;
                var dRow = def.AddGrid(dataSet).AddRow();
                dRow.ShowAllBorders(true);

                if (tableForm.Children != null)
                    foreach (var child in tableForm.Children)
                    {
                        AddControlBand(hRow, dRow, child, dataSet);
                    }
            }
            else
            {
                var row = def.AddArea().AddRow();//band.AddGroup(new XlsRow());
                var cell = row.AddText(control.Caption);
                cell.ColSpan = _captionColSpan;
                cell.Style.HAlign = HAlignment.Right;
                cell.Style.WrapText = true;

                if (control is BizDataControl && ((BizDataControl) control).ObjectValue != null)
                {
                    var value = GetDataControlValue((BizDataControl) control);
                    var dataCell = row.AddText(value);
                    dataCell.ColSpan = _valueColSpan;
                    dataCell.Style.FontColor = IndexedColors.DARK_BLUE.Index;
                    dataCell.Style.WrapText = true;
                }
                else
                {
                    var emptyCell = row.AddEmptyCell();
                    emptyCell.ColSpan = _valueColSpan;
                }
            }
        }

        private SqlQueryDataSet CreateDataSet(SqlQuery query)
        {
            var reader = _sqlQueryReaderFactory.Create(query); //new SqlQueryReader(DataContext, query);

            return Provider != null ? new SqlQueryDataSet(Provider, reader) : new SqlQueryDataSet(reader, UserId);
        }

        private string GetDataControlValue(BizDataControl control)
        {
            if (control.ObjectValue == null) return null;

            var box = control as BizComboBox;
            if (box != null && _comboBoxValueProvider != null/*&& box.Items != null*/)
            {
                /*var item =  box.Items.FirstOrDefault(i => i.Id == box.Value);
                if (item != null)
                    return item.Value;*/
                return _comboBoxValueProvider.GetComboBoxDetailValue(Form, box);
            }
            var editDateTime = control as BizEditDateTime;
            if (editDateTime != null)
            {
                return !String.IsNullOrWhiteSpace(editDateTime.Format)
                    ? ((DateTime) editDateTime.Value).ToString(editDateTime.Format)
                    : ((DateTime) editDateTime.Value).ToShortDateString();
            }
            var currencyEdit = control as BizEditCurrency;
            if (currencyEdit != null)
            {
                return !String.IsNullOrWhiteSpace(currencyEdit.Format)
                    ? ((Decimal) currencyEdit.Value).ToString(currencyEdit.Format)
                    : ((Decimal) currencyEdit.Value).ToString("N");
            }
            var floatEdit = control as BizEditFloat;
            if (floatEdit != null)
            {
                return !String.IsNullOrWhiteSpace(floatEdit.Format)
                    ? ((double) floatEdit.Value).ToString(floatEdit.Format)
                    : ((double) floatEdit.Value).ToString("N");
            }
            var intEdit = control as BizEditInt;
            if (intEdit != null)
            {
                return !String.IsNullOrWhiteSpace(intEdit.Format)
                    ? ((int) intEdit.Value).ToString(intEdit.Format)
                    : ((int) intEdit.Value).ToString("N");
            }
            var boolEdit = control as BizEditBool;
            if (boolEdit != null)
            {
                return boolEdit.Value ?? false ? "Да" : "Нет";
            }
            return control.ObjectValue.ToString();
        }

        protected void AddControlBand(XlsGroup band, XlsRow gridRow, BizControl control, SqlQueryDataSet dataSet)
        {
            if (control.Invisible || control is BizButton) return;

            if (control is BizPanel)
            {
                var node = new XlsTextNode(control.Caption);
                band.AddGroup(node);

                foreach (var child in control.Children)
                {
                    AddControlBand(node, gridRow, child, dataSet);
                }
            }
            else if (control is BizTableColumn ||
                     control is BizDocumentControl || control is BizDocumentListForm)
            {
                if (control.Children != null)
                {
                    foreach (var child in control.Children)
                    {
                        AddControlBand(band, gridRow, child, dataSet);
                    }
                }
            }
            else if (control is BizDataControl)
                AddControlColumn(band, gridRow, control, dataSet);
            else
                if (control.Children != null)
                {
                    foreach (var child in control.Children)
                    {
                        AddControlBand(band.AddGroup(new XlsTextNode(child.Caption)), gridRow, child, dataSet);
                    }
                } 
        }

        protected void AddControlColumn(XlsGroup band, XlsRow gridRow, BizControl control, SqlQueryDataSet dataSet)
        {
            if (control.Invisible) return;

            var header = new XlsTextNode(control.Caption);
            band.AddGroup(header); 
            var info = _adjuster.Find(control);
            var attr = dataSet.Reader.Query.FindAttribute(((BizDataControl)control).AttributeDefId ?? Guid.Empty);
            if (attr != null)
            {
                var field = gridRow.AddDataField(new SqlQueryDataSetField(dataSet, attr, control));
                if (info != null)
                {
                    header.ColSpan = info.ColSpan;
                    field.ColSpan = info.ColSpan;
                }
            }
/*
            var field = gridRow.AddDataField(new SqlQueryDataSetField(dataSet, control));
            if (info != null)
            {
                header.ColSpan = info.ColSpan;
                field.ColSpan = info.ColSpan;
//                if (info.ColSpan == 1)
//                    field.Width = info.Size;
            }
*/
        }

        /*public void Dispose()
        {
            if (_docRepo != null)
            {
                _docRepo.Dispose();
                _docRepo = null;
            }
        }

        ~XlsFormDefBuilder()
        {
            if (_docRepo != null)
                _docRepo.Dispose();
        }*/
    }

    public class XlsFormDefBuilderFactory : IXlsFormDefBuilderFactory
    {
        public IAppServiceProvider Provider { get; private set; }

        public XlsFormDefBuilderFactory(IAppServiceProvider provider)
        {
            Provider = provider;
        }

        public IXlsFormDefBuilder Create(BizForm form)
        {
            return new XlsFormDefBuilder(Provider, form);
        }
    }
}
