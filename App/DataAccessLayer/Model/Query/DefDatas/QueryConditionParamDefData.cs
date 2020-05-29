namespace Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas
{
    public class QueryConditionParamDefData
    {
        public string ParamName { get; private set; }
        public QueryConditionDefData Condition { get; private set; }

        public QueryConditionParamDefData(string paramName, QueryConditionDefData condition)
        {
            ParamName = paramName;
            Condition = condition;
        }
    }
}