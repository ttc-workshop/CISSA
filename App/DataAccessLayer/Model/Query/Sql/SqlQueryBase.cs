using System.Collections.Generic;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public abstract class SqlQueryBase : SqlQueryItem
    {
        public abstract SqlBuilder BuildSql(ICollection<SqlQueryCondition> conditions, bool isMain = false);
        public abstract SqlBuilder BuildSql(ICollection<SqlQuerySourceAttribute> attrs, ICollection<SqlQueryCondition> conditions, bool isMain = false);

        protected static List<SqlQuerySource> GetConditionListSources(IEnumerable<SqlQueryCondition> conditions)
        {
            var sources = new List<SqlQuerySource>();

            foreach (var condition in conditions)
                FillConditionSources(sources, condition);

            return sources;
        }

        protected static void FillConditionSources(ICollection<SqlQuerySource> sources, SqlQueryCondition condition)
        {
            if (condition.Condition == ConditionOperation.Include || condition.Condition == ConditionOperation.Exp)
            {
                if (condition.Conditions == null || condition.Conditions.Count == 0) return;

                foreach (var subCondition in condition.Conditions)
                    FillConditionSources(sources, subCondition);

                return;
            }

            //            if (condition.SubQuery == null)
            foreach (var attrRef in condition.Left.Attributes)
                if (!sources.Contains(attrRef.Source))
                    sources.Add(attrRef.Source);
            if (condition.Right.IsAttribute)
                foreach (var attrRef in condition.Right.Attributes)
                    if (!sources.Contains(attrRef.Source))
                        sources.Add(attrRef.Source);
        }
    }
}