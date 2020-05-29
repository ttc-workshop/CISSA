using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.Cissa.Report.Common
{
    public class DocFormDataSet : DataSet
    {
        public IList<Guid> DocList { get; set; }
        public BizControl Form { get; private set; }

        protected IDataContext DataContext { get; private set; }

        private readonly bool _ownDataContext;

        public IDocRepository DocRepo { get; private set; }
        public IFormRepository FormRepo { get; private set; }

        public DocFormDataSet(IDataContext dataContext, IEnumerable<Guid> docs, BizControl form, Guid userId)
        {
            DocList = new List<Guid>(docs);
            Form = form;
            if (dataContext == null)
            {
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else
                DataContext = dataContext;

            FormRepo = new FormRepository(DataContext, userId);
            DocRepo = new DocRepository(DataContext, userId);
        }

        public DocFormDataSet(IEnumerable<Guid> docs, BizControl form, Guid userId) : this(null, docs, form, userId) { }

        public int Index { get; private set; }
        public Doc Current { get; private set; }

        public override bool Eof()
        {
            return DocList.Count <= Index;
        }

        public override void Next()
        {
            Index++;
            if (DocList.Count > Index)
            {
                Current = DocRepo.LoadById(DocList[Index]);
                FormRepo.SetFormDoc(Form, Current);
            }
        }

        public override void Reset()
        {
            Index = 0;
        }

        public BizControl GetCurrent()
        {
            if (Current == null && Index < DocList.Count)
                Current = DocRepo.LoadById(DocList[Index]);
            FormRepo.SetFormDoc(Form, Current);
            return Form;
        }

        protected override void DoDispose(bool managed)
        {
            if (_ownDataContext && DataContext != null)
            {
                try
                {
                    DataContext.Dispose();
                    DataContext = null;
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "DocFormDataSet.Dispose");
                    throw;
                }
            }
        }

        public override bool HasField(string fieldName)
        {
            throw new NotImplementedException();
        }

        public override int GetRecordNo()
        {
            return Index;
        }

        ~DocFormDataSet()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "DocFormDataSet.Finalize");
                    throw;
                }
        }
    }

    public class DocFormDataSetField : DataSetField
    {
        public BizControl Control { get; private set; }

        public DocFormDataSetField(DocFormDataSet dataSet, BizControl control)
            : base(dataSet)
        {
            Control = control;
        }

        public override object GetValue()
        {
            var form = ((DocFormDataSet)DataSet).GetCurrent();
            if (form == null) return null;

            var box = Control as BizComboBox;
            if (box != null)
            {
                if (box.Items != null)
                {
                    var value =
                        box.Items.FirstOrDefault(
                            v => v.Id == (box.Value ?? Guid.Empty));
                    if (value != null)
                        return value.Value;
                }
            }
            var time = Control as BizEditDateTime;
            if (time != null)
            {
                if (time.Value != null)
                    return ((DateTime)time.Value)/*.ToString("d")*/;
            }
            var control = Control as BizDataControl;
            if (control != null)
                return control.ObjectValue;

            return null;
        }

        public override BaseDataType GetDataType()
        {
            if (Control is BizEditInt) return BaseDataType.Int;
            if (Control is BizEditText) return BaseDataType.Text;
            if (Control is BizEditBool) return BaseDataType.Bool;
            if (Control is BizEditCurrency) return BaseDataType.Currency;
            if (Control is BizEditDateTime) return BaseDataType.DateTime;
            if (Control is BizEditFloat) return BaseDataType.Float;
            if (Control is BizComboBox) return BaseDataType.Text;
            return BaseDataType.Text;
        }
    }
}

