using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryJoin : SqlQueryItem
    {
        public SqlQuerySource Source { get; private set; }
        public SqlQuerySource Target { get; private set; }
        public SqlSourceJoinType JoinType { get; set; }
        public SqlQuerySourceAttribute JoinAttribute { get; private set; }
        public AttrDef JoinAttrDef { get; private set; }
        public bool IsTargetAttribute { get; private set; }

        private readonly IList<SqlQueryCondition> _conditions = new List<SqlQueryCondition>();
        public IList<SqlQueryCondition> Conditions { get { return _conditions; } }
    
        public SqlQueryJoin(SqlQuerySource source, SqlQuerySource target, SqlSourceJoinType joinType,
                            Guid attrDefId)
        {
            Source = source;
            Target = target;
            JoinType = joinType;

            SqlQuerySource attrSource;
            JoinAttrDef = Source.FindAttributeDef(attrDefId);
            if (JoinAttrDef != null) attrSource = Source;
            else
            {
                JoinAttrDef = Target.FindAttributeDef(attrDefId);
                attrSource = Target;
                IsTargetAttribute = true;
            }

//            if (JoinAttrDef.Type.Id == (short)CissaDataType.Doc)
            if (JoinAttrDef != null)
            {
                JoinAttribute = attrSource.GetAttribute(JoinAttrDef.Id);
            }
        }

        public SqlQueryJoin(SqlQuerySource target, SqlSourceJoinType joinType)
        {
            // Source = source;
            Target = target;
            JoinType = joinType;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, SqlQuerySource source, Guid attrDefId, ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, operation, attrDefId, condition, value);

            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, SqlQuerySource source, string attrDefName, ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, operation, attrDefName, condition, value);

            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, SqlQuerySource source1, string attrDefName1, ConditionOperation condition, SqlQuerySource source2, string attrDefName2, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(operation, source1, attrDefName1, condition, source2, attrDefName2);

            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        internal string BuildConditions()
        {
            return SqlQueryCondition.BuildConditions(Conditions);
        }
    }
/*
    public class SqlQueryJoinCondition
    {
        public ConditionOperation Condition { get; set; }
        public ExpressionOperation Operation { get; set; }
        public SqlQuerySourceAttribute SourceAttribute { get; set; }
        public SqlQuerySourceAttribute TargetAttribute { get; set; }

        private readonly List<SqlQueryJoinCondition> _conditions = new List<SqlQueryJoinCondition>();
        public List<SqlQueryJoinCondition> Conditions { get { return _conditions; } }

        public static string BuildConditions(SqlQueryJoin join, IEnumerable<SqlQueryJoinCondition> conditions)
        {
            if (conditions == null) return String.Empty;

            var sql = "";
            var first = true;
            foreach (var condition in conditions)
            {
                if (!first)
                    switch (condition.Operation)
                    {
                        case ExpressionOperation.And:
                            sql = sql + " and ";
                            break;
                        case ExpressionOperation.Or:
                            sql = sql + " or ";
                            break;
                        case ExpressionOperation.AndNot:
                            sql = sql + " and not ";
                            break;
                        case ExpressionOperation.OrNot:
                            sql = sql + " or not ";
                            break;
                    }

                switch (condition.Condition)
                {
                    case ConditionOperation.Include:
                        {
                            var s = (condition.Conditions != null) ? BuildConditions(join, condition.Conditions) : String.Empty;
                            if (String.IsNullOrEmpty(s)) return String.Empty;
                            sql = sql + "(" + s + ")";
                        }
                        break;
                    case ConditionOperation.Exp:
                        {
                            var s = (condition.Conditions != null) ? BuildConditions(join, condition.Conditions) : String.Empty;
                            if (String.IsNullOrEmpty(s)) return String.Empty;
                            sql = sql + "(" + s + ")";
                        }
                        break;
                    default:
                        sql = sql + condition.BuildCondition(join);
                        break;
                }

                first = false;
            }
            return sql;
        }

        private string BuildCondition(SqlQueryJoin join)
        {
            if (Condition == ConditionOperation.Include)
            {
                var s = BuildConditions(join, Conditions);
                if (String.IsNullOrEmpty(s)) return s;
                return "(" + s + ")";
            }
            if (Condition == ConditionOperation.Exp)
            {
                var s = BuildConditions(join, Conditions);
                if (String.IsNullOrEmpty(s)) return s;
                return "(" + s + ")";
            }

            var sourceAttr = String.Format("[{0}].[{1}]", join.Source.AliasName, SourceAttribute.AliasName);
            var targetAttr = String.Format("[{0}].[{1}]", join.Target.AliasName, TargetAttribute.AliasName);
            var sql = sourceAttr;

            var operation = SqlQueryCondition.ComparisonOperationToString(Condition);

            if (Condition == ConditionOperation.Between || Condition == ConditionOperation.NotBetween)
                throw new ApplicationException("Join condition operation not supported!");
            if (Condition == ConditionOperation.In || Condition == ConditionOperation.NotIn)
            {
                sql = String.Format("{0} {1} ({2})", sql, operation, targetAttr);
            }
            else if (Condition == ConditionOperation.IsNull || Condition == ConditionOperation.IsNotNull)
                sql = String.Format("{0} {1}", sql, operation);
            else if (Condition == ConditionOperation.Levenstein)
                sql = String.Format("{0}({1}, {2}) >= {3}", operation, sql, targetAttr,
                    SqlQuery.LevensteinCoefficient.ToString(CultureInfo.InvariantCulture).Replace(',', '.'));
            else
                sql = String.Format("{0} {1} {2}", sql, operation, targetAttr);

            return sql;
        }

    }*/
}