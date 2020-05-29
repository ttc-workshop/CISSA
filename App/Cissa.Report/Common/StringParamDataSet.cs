using System;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.Cissa.Report.Common
{
    public class StringParamDataSet: DataSet
    {
        public IStringParams Params { get; private set; }

        public StringParamDataSet(IStringParams stringParams)
        {
            Params = stringParams;
        }


        public override bool Eof()
        {
            return true;
        }

        public override void Next()
        {
            ;
        }

        public override void Reset()
        {
            ;
        }

        public override bool HasField(string fieldName)
        {
            return Params != null && String.IsNullOrEmpty(Params.Get(fieldName));
        }

        public override int GetRecordNo()
        {
            return 0;
        }

        public override DataSetField CreateField(string fieldName)
        {
            return new StringParamDataSetField(this, fieldName);
        }
    }

    public class StringParamDataSetField : DataSetField
    {
        public string ParamName { get; private set; }
        public StringParamDataSetField(DataSet dataSet, string paramName) : base(dataSet)
        {
            ParamName = paramName;
        }

        public override object GetValue()
        {
            var stringParams = (DataSet as StringParamDataSet);
            if (stringParams != null && stringParams.Params != null)
                return stringParams.Params.Get(ParamName);
            return String.Empty;
        }
    }
}
