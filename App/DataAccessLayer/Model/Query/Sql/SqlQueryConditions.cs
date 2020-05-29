using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryConditions : List<SqlQueryCondition>
    {
        public IEnumerable<SqlQuerySourceAttribute> GetConditionAttributes()
        {
            var attrs = new List<SqlQuerySourceAttribute>();

            foreach (var sCond in
                this.SelectMany(cond => cond.Attributes.Where(sCond => !attrs.Contains(sCond.Attribute))))
            {
                attrs.Add(sCond.Attribute);
                yield return sCond.Attribute;
            }
        }

        private ICollection<SqlQueryCondition> GetExternalSourceConditions(SqlQuerySource source,
            ICollection<SqlQueryCondition> sourceConditions)
        {
            var externalConditions = new List<SqlQueryCondition>();

            FillExternalSourceConditions(source, sourceConditions, externalConditions);

            return externalConditions;
        }

        public void FillExternalSourceConditions(SqlQuerySource source, 
            ICollection<SqlQueryCondition> sourceConditions, ICollection<SqlQueryCondition> externalConditions)
        {
            foreach (var condition in this)
            {
                if (condition.Condition == ConditionOperation.Include || condition.Condition == ConditionOperation.Exp)
                {
                    if (sourceConditions.Contains(condition)) continue;

                    if (condition.Conditions == null || condition.Conditions.Count == 0)
                        continue;

                    if (condition.Conditions.HasOrConditions())
                        externalConditions.Add(condition);
                    else
                        condition.Conditions.FillExternalSourceConditions(source, sourceConditions, externalConditions);
                }
                else
                {
                    if (sourceConditions.Contains(condition)) continue;

                    var sources = new List<SqlQuerySource>();

                    FillConditionSources(sources, condition);

                    if (sources.Contains(source))
                        externalConditions.Add(condition);
                }
            }
        }

        private ICollection<SqlQueryCondition> GetSourceConditions(SqlQuerySource source)
        {
            var sourceConditions = new List<SqlQueryCondition>();

            FillSourceConditions(source, sourceConditions);

            return sourceConditions;
        }

        public bool HasOrConditions()
        {
            return this.Any(c =>
                c.Operation == ExpressionOperation.Or ||
                c.Operation == ExpressionOperation.OrNot);
        }

        public void FillSourceConditions(SqlQuerySource source, ICollection<SqlQueryCondition> sourceConditions)
        {
            if (!HasOrConditions())
                foreach (var condition in this)
                {
                    if (condition.Condition == ConditionOperation.Include || condition.Condition == ConditionOperation.Exp)
                    {
                        if (condition.Conditions == null || condition.Conditions.Count == 0)
                            continue;

                        if (condition.Conditions.HasOrConditions())
                        {
                            var sources = condition.Conditions.GetSources();

                            if (sources.Count == 1 && sources[0] == source) sourceConditions.Add(condition);
                            /*foreach (var sub in conditions)
                                sourceConditions.Add(sub);*/
                        }
                        else
                            condition.Conditions.FillSourceConditions(source, sourceConditions);
                    }
                    else
                    {
                        var sources = new List<SqlQuerySource>();

                        FillConditionSources(sources, condition);

                        if (sources.Count == 1 && sources[0] == source)
                            sourceConditions.Add(condition);
                    }
                }
            else
            {
                var sources = GetSources();

                if (sources.Count == 1 && sources[0] == source)
                    foreach (var condition in this)
                        sourceConditions.Add(condition);
            }
        }

        public List<SqlQuerySource> GetSources()
        {
            var sources = new List<SqlQuerySource>();

            foreach (var condition in this)
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

            foreach (var attrRef in condition.Left.Attributes)
                if (!sources.Contains(attrRef.Source))
                    sources.Add(attrRef.Source);
            if (condition.Right.IsAttribute)
                foreach (var attrRef in condition.Right.Attributes)
                    if (!sources.Contains(attrRef.Source))
                        sources.Add(attrRef.Source);
        }

        public IEnumerable<QueryConditionValueDef> FindParams(string paramName)
        {
                foreach (var condition in this)
                {
                    if (condition.Condition == ConditionOperation.Include ||
                        condition.Condition == ConditionOperation.Exp)
                    {
                        foreach (var param in condition.Conditions.FindParams(paramName))
                            yield return param;
                    }
                    else
                    {
                        foreach (var param in FindConditionPartParams(paramName, condition.Left))
                            yield return param;
                        foreach (var param in FindConditionPartParams(paramName, condition.Right))
                            yield return param;
                    }
                }
        }
        public IEnumerable<QueryConditionValueDef> GetParams()
        {
            foreach (var condition in this)
            {
                if (condition.Condition == ConditionOperation.Include ||
                    condition.Condition == ConditionOperation.Exp)
                {
                    foreach (var param in condition.Conditions.GetParams())
                        yield return param;
                }
                else
                {
                    foreach (var param in GetConditionPartParams(condition.Left))
                        yield return param;
                    foreach (var param in GetConditionPartParams(condition.Right))
                        yield return param;
                }
            }
        }

        private static IEnumerable<QueryConditionValueDef> FindConditionPartParams(string paramName, SqlQueryConditionPart part)
        {
            if (part.Params != null)
                foreach (var param in part.Params.Where(
                    param =>
                        param.Name != null &&
                        String.Equals(param.Name, paramName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    yield return param;
                }
            if (part.SubQuery != null)
            {
                foreach (var param in part.SubQuery.FindParams(paramName))
                    yield return param;
            }
        }
        private static IEnumerable<QueryConditionValueDef> GetConditionPartParams(SqlQueryConditionPart part)
        {
            if (part.Params != null)
                foreach (var param in part.Params.Where(param => !String.IsNullOrEmpty(param.Name)))
                {
                    yield return param;
                }
            if (part.SubQuery != null)
            {
                foreach (var param in part.SubQuery.GetAllParams())
                    yield return param;
            }
        }
    }
}