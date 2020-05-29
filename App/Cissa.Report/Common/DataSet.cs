using System;
using Intersoft.CISSA.DataAccessLayer.Model;

namespace Intersoft.Cissa.Report.Common
{
    public abstract class DataSet: IDisposable 
    {
        public abstract bool Eof();

        public abstract void Next();

        public abstract void Reset();

        protected virtual void DoDispose(bool managed) {}

        public void Dispose()
        {
            DoDispose(true);
        }

        public abstract bool HasField(string fieldName);

        public abstract int GetRecordNo();

        public virtual DataSetField CreateField(string fieldName)
        {
            throw new ApplicationException("Cannot create DataSet field");
        }

        public bool CalcSummary { get; set; }
    }

    public abstract class DataSetField
    {
        protected DataSet DataSet { get; private set; }

        protected DataSetField(DataSet dataSet)
        {
            DataSet = dataSet;
        }

        public abstract object GetValue();

        public virtual BaseDataType GetDataType()
        {
            return BaseDataType.Text;
        }
    }
}
