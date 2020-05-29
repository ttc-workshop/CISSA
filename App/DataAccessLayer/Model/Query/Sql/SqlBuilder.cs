using System;
using System.Collections.Generic;
using System.Linq;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlSourceJoin
    {
        public SqlSourceJoinType Type { get; set; }
        public string Source { get; set; }

        public SqlSourceJoin(SqlSourceJoinType type, string source)
        {
            Type = type;
            Source = source;
        }
    }

    public class SqlConditionItem
    {
        public ExpressionOperation Operation { get; set; }

        public SqlConditionItem(ExpressionOperation operation)
        {
            Operation = operation;
        }

        public SqlConditionItem() : this(ExpressionOperation.And) { }
    }

    public class SqlCondition : SqlConditionItem
    {
        public string Expression { get; set; }

        public SqlCondition(ExpressionOperation operation, string expression)
            : base(operation)
        {
            Expression = expression;
        }

        public SqlCondition(string expression)
        {
            Expression = expression;
        }
    }

    public class SqlExpression : SqlConditionItem
    {
        public List<SqlConditionItem> Conditions { get; private set; }

        public SqlExpression(ExpressionOperation operation) : base(operation) { }

        public SqlConditionItem AddCondition(SqlConditionItem condition)
        {
            if (Conditions == null) Conditions = new List<SqlConditionItem>();
            Conditions.Add(condition);
            return condition;
        }

        public SqlExpression GetParentOf(SqlExpression exp)
        {
            if (Conditions != null)
            {
                if (Conditions.Contains(exp)) return this;

                return Conditions.OfType<SqlExpression>().Select(item => (item).GetParentOf(exp)).FirstOrDefault(parent => parent != null);
            }
            return null;
        }
    }

    public class SqlBuilder
    {
        public int TopNo { get; set; }
        public int SkipNo { get; set; }
        public List<string> Select { get; private set; }
        public string From { get; private set; }
        public List<SqlSourceJoin> Joins { get; private set; }
        private SqlExpression _currentWhereExp;
        public List<SqlConditionItem> WhereConditions { get; private set; }
        public List<string> GroupsBy { get; private set; }
        public List<SqlConditionItem> HavingConditions { get; private set; }
        private SqlExpression _currentHavingExp;
        public List<string> OrdersBy { get; private set; }
        public string Params { get; set; }

        public void SetFrom(string from)
        {
            From = from;
        }

        public void SetFrom(string from, params object[] args)
        {
            From = String.Format(from, args);
        }

        public SqlBuilder AddSelect(string attrs)
        {
            if (Select == null) Select = new List<string>();
            
            Select.Add(attrs);

            return this;
        }

        public SqlBuilder AddSelect(string attrs, params object[] args)
        {
            return AddSelect(String.Format(attrs, args));
        }

        public SqlBuilder AddSelect(ScriptStringBuilder attrs)
        {
            return AddSelect(attrs.ToString());
        }

        public SqlBuilder AddJoin(SqlSourceJoinType type, string source, params object[] args)
        {
            AddJoin(type, String.Format(source, args));
            return this;
        }

        public SqlBuilder AddJoin(SqlSourceJoinType type, string source)
        {
            if (Joins == null) Joins = new List<SqlSourceJoin>();

            Joins.Add(new SqlSourceJoin(type, source));

            return this;
        }

        public SqlExpression AddWhereExp(ExpressionOperation operation)
        {
            if (WhereConditions == null) WhereConditions = new List<SqlConditionItem>();

            var condition = new SqlExpression(operation);

            if (_currentWhereExp != null)
                _currentWhereExp.AddCondition(condition);
            else
                WhereConditions.Add(condition);

            _currentWhereExp = condition;

            return condition;
        }

        public SqlExpression EndWhereExp()
        {
            if (_currentWhereExp == null) throw new ApplicationException("Ошибка! Вложенного выражения не существует");

            var exp = _currentWhereExp;
            _currentWhereExp = FindExpParent(_currentWhereExp);
            if (exp.Conditions != null && exp.Conditions.Count == 0)
            {
                if (_currentWhereExp != null) _currentWhereExp.Conditions.Remove(exp);
                else
                    WhereConditions.Remove(exp);
            }

            return _currentWhereExp;
        }

        public SqlExpression AddHavingExp(ExpressionOperation operation)
        {
            if (HavingConditions == null) HavingConditions = new List<SqlConditionItem>();

            var condition = new SqlExpression(operation);

            if (_currentHavingExp != null)
                _currentHavingExp.AddCondition(condition);
            else
                HavingConditions.Add(condition);

            _currentHavingExp = condition;

            return condition;
        }

        public SqlExpression EndHavingExp()
        {
            if (_currentHavingExp == null) throw new ApplicationException("Ошибка! Вложенного выражения не существует");

            _currentHavingExp = FindExpParent(_currentHavingExp);

            return _currentHavingExp;
        }

        public SqlCondition AddCondition(ExpressionOperation operation, string exp, params object[] args)
        {
            return AddCondition(operation, String.Format(exp, args));
        }
        public SqlCondition AddCondition(ExpressionOperation operation, string exp)
        {
            var condition = new SqlCondition(operation, exp);

            if (_currentWhereExp != null)
                _currentWhereExp.AddCondition(condition);
            else
            {
                if (WhereConditions == null) WhereConditions = new List<SqlConditionItem>();
                WhereConditions.Add(condition);
            }

            return condition;
        }

        public SqlBuilder AddGroupBy(string attrs, params object[] args)
        {
            return AddGroupBy(String.Format(attrs, args));
        }
        public SqlBuilder AddGroupBy(string attrs)
        {
            if (GroupsBy == null) GroupsBy = new List<string>();

            GroupsBy.Add(attrs);

            return this;
        }

        public SqlCondition AddHaving(ExpressionOperation operation, string exp, params object[] args)
        {
            return AddHaving(operation, String.Format(exp, args));
        }
        public SqlCondition AddHaving(ExpressionOperation operation, string exp)
        {
            var condition = new SqlCondition(operation, exp);

            if (_currentHavingExp != null)
                _currentHavingExp.AddCondition(condition);
            else
            {
                if (HavingConditions == null) HavingConditions = new List<SqlConditionItem>();
                HavingConditions.Add(condition);
            }

            return condition;
        }

        public SqlBuilder AddOrderBy(string attrs, params object[] args)
        {
            return AddOrderBy(String.Format(attrs, args));
        }
        public SqlBuilder AddOrderBy(string attrs)
        {
            if (OrdersBy == null) OrdersBy = new List<string>();

            OrdersBy.Add(attrs);

            return this;
        }

        private ScriptStringBuilder AddSkipStatement(ScriptStringBuilder mainSql, int skipNo, int? topNo)
        {
            var sql = new ScriptStringBuilder();

            sql.AppendLine("SELECT * ");
            sql.AppendLine("FROM");
            sql.BeginBlock();
            try
            {
                sql.AppendLine(mainSql.ToString());
            }
            finally
            {
                sql.EndBlock();
            }
            return sql;
        }

        public ScriptStringBuilder Build()
        {
            var sql = new ScriptStringBuilder();

            if (TopNo > 0 && SkipNo == 0)
                sql.AppendLine("SELECT TOP " + TopNo);
            else
                sql.AppendLine("SELECT");

            var rowNoAlias = "RowNo";
            var rowNoIndex = 0;
            var first = true;
            if (Select != null && Select.Count > 0)
            {
                sql.BeginBlock();
                try
                {
                    foreach (var s in Select)
                    {
                        if (!first) sql.AppendLine(",");
                        else
                            first = false;

                        sql.Append(s);

                        while (s.IndexOf(rowNoAlias, StringComparison.Ordinal) >= 0)
                        {
                            rowNoIndex++;
                            rowNoAlias = "RowNo" + rowNoIndex;
                        }
                    }

                    if (SkipNo > 0)
                    {
                        if (!first) sql.AppendLine(",");

                        var orderByStatement = new ScriptStringBuilder();
                        FillOrderBy(orderByStatement);
                        sql.AppendFormat("Row_Number() over (order by {0}) as {1}", orderByStatement, rowNoAlias);
                    }
                    sql.AppendLine();
                }
                finally
                {
                    sql.EndBlock();
                }
            }
            sql.AppendLine("FROM");
            sql.BeginBlock();
            try
            {
                sql.AppendLine(From);
                if (Joins != null && Joins.Count > 0)
                {
                    foreach (var join in Joins)
                    {
                        switch (join.Type)
                        {
                            case SqlSourceJoinType.Inner:
                                sql.Append("INNER JOIN ");
                                break;
                            case SqlSourceJoinType.LeftOuter:
                                sql.Append("LEFT OUTER JOIN ");
                                break;
                            case SqlSourceJoinType.RightOuter:
                                sql.Append("RIGHT OUTER JOIN ");
                                break;
                        }

                        sql.AppendLine(join.Source);
                    }
                }
            } 
            finally
            {
                sql.EndBlock();
            }
            first = true;
            RemoveEmptyWhereExpConditions(WhereConditions);
            if (WhereConditions != null && WhereConditions.Count > 0)
            {
                sql.AppendLine("WHERE");
                sql.BeginBlock();
                try
                {
                    foreach (var condition in WhereConditions)
                    {
                        if (!first)
                            switch (condition.Operation)
                            {
                                case ExpressionOperation.And:
                                    sql.AppendLine(" AND");
                                    break;
                                case ExpressionOperation.Or:
                                    sql.AppendLine(" OR");
                                    break;
                                case ExpressionOperation.AndNot:
                                    sql.AppendLine(" AND NOT");
                                    break;
                                case ExpressionOperation.OrNot:
                                    sql.AppendLine(" OR NOT");
                                    break;
                            }
                        else
                            first = false;

                        AppendCondition(sql, condition);
                    }
                } 
                finally
                {
                    sql.EndBlock();
                }
            }

            first = true;
            if (GroupsBy != null && GroupsBy.Count > 0)
            {
                sql.AppendLine("GROUP BY");
                sql.BeginBlock();
                try
                {
                    foreach (var groupBy in GroupsBy)
                    {
                        if (!first)
                            sql.AppendLine(",");
                        else
                            first = false;

                        sql.Append(groupBy);
                    }
                } 
                finally
                {
                    sql.EndBlock();
                }

                first = true;
                if (HavingConditions != null && HavingConditions.Count > 0)
                {
                    sql.AppendLine("HAVING");
                    sql.BeginBlock();
                    try
                    {
                        foreach (var condition in HavingConditions)
                        {
                            if (!first)
                                switch (condition.Operation)
                                {
                                    case ExpressionOperation.And:
                                        sql.AppendLine(" AND");
                                        break;
                                    case ExpressionOperation.Or:
                                        sql.AppendLine(" OR");
                                        break;
                                    case ExpressionOperation.AndNot:
                                        sql.AppendLine(" AND NOT");
                                        break;
                                    case ExpressionOperation.OrNot:
                                        sql.AppendLine(" OR NOT");
                                        break;
                                }
                            else
                                first = false;

                            AppendCondition(sql, condition);
                        }
                    }
                    finally
                    {
                        sql.EndBlock();
                    }
                }
            }

            if (SkipNo <= 0)
            {
                first = true;
                if (OrdersBy != null && OrdersBy.Count > 0)
                {
                    sql.AppendLine("ORDER BY");
                    sql.BeginBlock();
                    try
                    {
                        foreach (var orderBy in OrdersBy)
                        {
                            if (!first)
                                sql.AppendLine(",");
                            else
                                first = false;

                            sql.Append(orderBy);
                        }
                    }
                    finally
                    {
                        sql.EndBlock();
                    }
                }
            }
            else
            {
                var offsetSql = new ScriptStringBuilder();

                offsetSql.AppendLine("SELECT * ");
                offsetSql.AppendLine("FROM (");
                offsetSql.BeginBlock();
                try
                {
                    offsetSql.AppendLine(sql.ToString());
                }
                finally
                {
                    offsetSql.EndBlock();
                }
                offsetSql.AppendLine(") t");
                if (TopNo > 0)
                    offsetSql.AppendFormat("WHERE {0} BETWEEN {1} AND {2}", rowNoAlias, SkipNo + 1, SkipNo + TopNo);
                else
                    offsetSql.AppendFormat("WHERE {0} > {1}", rowNoAlias, SkipNo);

                if (String.IsNullOrEmpty(Params))
                    offsetSql.AppendLine("\n" + Params);

                return offsetSql;
            }

            if (String.IsNullOrEmpty(Params))
            {
                sql.AppendLine(Params);
            }

            return sql;
        }

        protected void FillOrderBy(ScriptStringBuilder sql)
        {
            var first = true;
            if (OrdersBy != null && OrdersBy.Count > 0)
            {
                sql.BeginBlock();
                try
                {
                    foreach (var orderBy in OrdersBy)
                    {
                        if (!first)
                            sql.AppendLine(",");
                        else
                            first = false;

                        sql.Append(orderBy);
                    }
                }
                finally
                {
                    sql.EndBlock();
                }
            }
        }

        public override string ToString()
        {
            return Build().ToString();
        }

        private static void AppendCondition(ScriptStringBuilder sql, SqlConditionItem condition)
        {
            var first = true;

            var expression = condition as SqlExpression;
            if (expression != null)
            {
                if (expression.Conditions != null && expression.Conditions.Count > 0)
                {
                    sql.AppendLine("(");
                    sql.BeginBlock();
                    foreach (var sub in expression.Conditions)
                    {
                        if (!first)
                            switch (sub.Operation)
                            {
                                case ExpressionOperation.And:
                                    sql.AppendLine(" AND");
                                    break;
                                case ExpressionOperation.Or:
                                    sql.AppendLine(" OR");
                                    break;
                                case ExpressionOperation.AndNot:
                                    sql.AppendLine(" AND NOT");
                                    break;
                                case ExpressionOperation.OrNot:
                                    sql.AppendLine(" OR NOT");
                                    break;
                            }
                        else
                            first = false;

                        AppendCondition(sql, sub);
                    }
                    sql.EndBlock();
                    sql.Append(")");
                }
            }
            else
            {
                sql.Append(((SqlCondition)condition).Expression);
            }
        }

        private SqlExpression FindExpParent(SqlExpression exp)
        {
            if (WhereConditions == null) return null;
            foreach (var item in WhereConditions)
            {
                var expression = item as SqlExpression;
                if (expression != null)
                {
                    var parent = expression.GetParentOf(exp);
                    if (parent != null) return parent;
                }
            }

            return null;
        }

        private static void RemoveEmptyWhereExpConditions(ICollection<SqlConditionItem> conditions)
        {
            if (conditions != null && conditions.Count > 0)
            {
                var removingConds = new List<SqlExpression>();

                foreach (var item in conditions)
                {
                    var cond = item as SqlExpression;
                    if (cond != null)
                    {
                        RemoveEmptyWhereExpConditions(cond.Conditions);
                        if (cond.Conditions == null || cond.Conditions.Count == 0)
                            removingConds.Add(cond);
                    }
                }

                foreach (var cond in removingConds)
                    conditions.Remove(cond);
            }
        }
    }
}
