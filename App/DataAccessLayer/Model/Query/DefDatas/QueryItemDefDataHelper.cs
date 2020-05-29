using System;
using System.Collections.Generic;
using System.Linq;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas
{
    public static class QueryItemDefDataHelper
    {
        public static IEnumerable<QueryConditionParamDefData> FindParamConditions(this QueryItemDefData item)
        {
            if (item == null) yield break;

            var condition = item as QueryConditionDefData;
            if (condition != null)
            {
                if (condition.LeftAttributeId == null && String.IsNullOrEmpty(condition.LeftAttributeName))
                {
                    if (!String.IsNullOrEmpty(condition.LeftParamName))
                        yield return new QueryConditionParamDefData(condition.LeftParamName, condition);
                }
                if (condition.RightAttributeId == null && String.IsNullOrEmpty(condition.RightAttributeName))
                {
                    if (!String.IsNullOrEmpty(condition.RightParamName))
                        yield return new QueryConditionParamDefData(condition.RightParamName, condition);
                }
            }

            if (item.Items == null) yield break;

            foreach (var data in item.Items.SelectMany(FindParamConditions))
            {
                yield return data;
            }
        }
    }
}