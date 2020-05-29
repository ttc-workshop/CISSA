using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Maps;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryCondition : SqlQueryItem
    {
        private readonly SqlQueryConditionPart _left = new SqlQueryConditionPart();
        public SqlQueryConditionPart Left { get { return _left; } }

        private readonly SqlQueryConditionPart _right = new SqlQueryConditionPart();
        public SqlQueryConditionPart Right { get { return _right; } }

        // private readonly List<SqlQuerySourceAttributeRef> _attributes = new List<SqlQuerySourceAttributeRef>();
        public List<SqlQuerySourceAttributeRef> Attributes { get { return Left.Attributes; } }

        public SqlQuerySource Source 
        {
            get { return Left.Attributes.Count > 0 ? Left.Attributes[0].Source : null; } 
        }

        public SqlQuerySourceAttribute Attribute
        {
            get { return Left.Attributes.Count > 0 ? Left.Attributes[0].Attribute : null; }
        }

        // public string Expression { get { return Left.Expression; } set { Left.Expression = value;} }
        // public SystemIdent Ident { get; private set; }
        public ConditionOperation Condition { get; set; }
        public ExpressionOperation Operation { get; set; }
        public object[] Values 
        { 
            get { return Right.Values; } 
        }
        public object Value { get { return Right.Params != null ? Right.Params[0].Value : null; } }

        public SqlQuery SubQuery
        {
            get { return Right.SubQuery; }
        }

        public SqlQueryAttribute SubQueryAttribute
        {
            get { return Right.SubQueryAttribute; }
        }

        private readonly SqlQueryConditions _conditions = new SqlQueryConditions();
        public SqlQueryConditions Conditions { get { return _conditions; } }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, SqlQuerySourceAttribute attribute, 
                                 ConditionOperation condition, object value)
        {
            // Source = source;
            // Attribute = attribute;
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, attribute));
            Operation = operation;
            Condition = condition;
            var values = value as object[];
            if (values == null && value is IEnumerable<object>)
                values = ((IEnumerable<object>) value).ToArray();
            Right.Values = values ?? new[] { value };
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, SqlQuerySourceAttribute attribute,
                                 ConditionOperation condition, object[] values)
        {
            // Source = source;
            // Attribute = attribute;
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, attribute));
            Operation = operation;
            Condition = condition;
            Right.Values = values;
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, SqlQuerySourceAttribute attribute,
                                 ConditionOperation condition, SqlQuery subQuery, SqlQueryAttribute subQueryAttribute)
        {
            // Source = source;
            // Attribute = attribute;
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, attribute));
            Operation = operation;
            Condition = condition;
            Right.SubQuery = subQuery;
            Right.SubQueryAttribute = subQueryAttribute;
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, SqlQuerySourceAttribute attribute,
                                 ConditionOperation condition, SqlQuery subQuery, Guid subQueryAttrId)
        {
            // Source = source;
            // Attribute = attribute;
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, attribute));
            Operation = operation;
            Condition = condition;
            Right.SubQuery = subQuery;
            Right.SubQueryAttribute = subQuery.FindAttribute(subQueryAttrId) ?? subQuery.AddAttribute(subQueryAttrId);
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, SqlQuerySourceAttribute attribute,
                                 ConditionOperation condition, SqlQuery subQuery, string subQueryAttrName)
        {
            // Source = source;
            // Attribute = attribute;
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, attribute));
            Operation = operation;
            Condition = condition;
            Right.SubQuery = subQuery;
            Right.SubQueryAttribute = subQuery.FindAttribute(subQueryAttrName) ?? subQuery.AddAttribute(subQueryAttrName);
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, Guid attrDefId, 
                                 ConditionOperation condition, object value)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefId)));
            Operation = operation;
            Condition = condition;
            var values = value as object[];
            if (values == null && value is IEnumerable<object>)
                values = ((IEnumerable<object>)value).ToArray();
            Right.Values = values ?? new[] { value };
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, Guid attrDefId,
                                 ConditionOperation condition, object value, string expression)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefId)));
            Operation = operation;
            Condition = condition;
            Left.Expression = expression;
            var values = value as object[];
            if (values == null && value is IEnumerable<object>)
                values = ((IEnumerable<object>)value).ToArray();
            Right.Values = values ?? new[] { value };
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, Guid attrDefId,
                                 ConditionOperation condition, object[] values)
        {
            // Source = source;
            // Attribute = attribute;
            if (source != null && attrDefId != Guid.Empty)
                Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefId)));
            Operation = operation;
            Condition = condition;
            Right.Values = values;
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, Guid attrDefId, 
                                 ConditionOperation condition, object[] values, string expression)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefId)));
            Operation = operation;
            Condition = condition;
            Left.Expression = expression;
            Right.Values = values;
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, Guid attrDefId,
                                 ConditionOperation condition, SqlQuery subQuery, SqlQueryAttribute subQueryAttribute)
        {
            // Source = source;
            // Attribute = attribute;
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefId)));
            Operation = operation;
            Condition = condition;
            Right.SubQuery = subQuery;
            Right.SubQueryAttribute = subQueryAttribute;
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, Guid attrDefId,
                                 ConditionOperation condition, SqlQuery subQuery, Guid subQueryAttrId)
        {
            // Source = source;
            // Attribute = attribute;
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefId)));
            Operation = operation;
            Condition = condition;
            Right.SubQuery = subQuery;
            Right.SubQueryAttribute = subQuery.FindAttribute(subQueryAttrId) ?? subQuery.AddAttribute(subQueryAttrId);
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, Guid attrDefId,
                                 ConditionOperation condition, SqlQuery subQuery, string subQueryAttrName)
        {
//            Source = source;
//            Attribute = Source.GetAttribute(attrDefId);
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefId)));
            Operation = operation;
            Condition = condition;
            Right.SubQuery = subQuery;
            Right.SubQueryAttribute = subQuery.FindAttribute(subQueryAttrName) ?? subQuery.AddAttribute(subQueryAttrName);
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, string attrDefName, 
                                 ConditionOperation condition, object value)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefName)));
            Operation = operation;
            Condition = condition;
            var values = value as object[];
            if (values == null && value is IEnumerable<object>)
                values = ((IEnumerable<object>)value).ToArray();
            Right.Values = values ?? new[] { value };
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, string attrDefName,
                                 ConditionOperation condition, object value, string expression)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefName)));
            Operation = operation;
            Condition = condition;
            Left.Expression = expression;
            var values = value as object[];
            if (values == null && value is IEnumerable<object>)
                values = ((IEnumerable<object>)value).ToArray();
            Right.Values = values ?? new[] { value };
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, IEnumerable<string> attrDefNames,
                                 ConditionOperation condition, object value, string expression)
        {
            foreach (var attrDefName in attrDefNames)
            {
                var attr = source.GetAttribute(attrDefName);
                Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, attr));
            }
            Left.Expression = expression;
            Operation = operation;
            Condition = condition;
            var values = value as object[];
            if (values == null && value is IEnumerable<object>)
                values = ((IEnumerable<object>)value).ToArray();
            Right.Values = values ?? new[] { value };
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, IEnumerable<string> attrDefNames,
                                 ConditionOperation condition, object[] values, string expession)
        {
            foreach (var attrDefName in attrDefNames)
            {
                var attr = source.GetAttribute(attrDefName);
                Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, attr));
            }
            Left.Expression = expession;
            Operation = operation;
            Condition = condition;
            Right.Values = values;
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, string attrDefName,
                                 ConditionOperation condition, object[] values)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefName)));
            Operation = operation;
            Condition = condition;
            Right.Values = values;
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, string attrDefName,
                                 ConditionOperation condition, IEnumerable<object> values)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefName)));
            Operation = operation;
            Condition = condition;
            Right.Values = values.ToArray();
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, string attrDefName,
                                 ConditionOperation condition, SqlQuery subQuery, SqlQueryAttribute subQueryAttribute)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefName)));
            Operation = operation;
            Condition = condition;
            Right.SubQuery = subQuery;
            Right.SubQueryAttribute = subQueryAttribute;
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, string attrDefName,
                                 ConditionOperation condition, SqlQuery subQuery, Guid subQueryAttrId)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefName)));
            Operation = operation;
            Condition = condition;
            Right.SubQuery = subQuery;
            Right.SubQueryAttribute = subQuery.FindAttribute(subQueryAttrId) ?? subQuery.AddAttribute(subQueryAttrId);
        }

        public SqlQueryCondition(SqlQuerySource source, ExpressionOperation operation, string attrDefName,
                                 ConditionOperation condition, SqlQuery subQuery, string subQueryAttrName)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source, source.GetAttribute(attrDefName)));
            Operation = operation;
            Condition = condition;
            Right.SubQuery = subQuery;
            Right.SubQueryAttribute = subQuery.FindAttribute(subQueryAttrName) ?? subQuery.AddAttribute(subQueryAttrName);
        }

        public SqlQueryCondition(ExpressionOperation operation, SqlQuerySource source1, string attrDefName1,
                                 ConditionOperation condition, SqlQuerySource source2, string attrDefName2)
        {
            Left.Attributes.Add(new SqlQuerySourceAttributeRef(source1, source1.GetAttribute(attrDefName1)));
            Operation = operation;
            Condition = condition;
            Right.Attributes.Add(new SqlQuerySourceAttributeRef(source2, source2.GetAttribute(attrDefName2)));
        }
        public SqlQueryCondition(ExpressionOperation operation, SqlQueryConditionPart leftPart,
                                 ConditionOperation condition, SqlQueryConditionPart rightPart)
        {
            _left = leftPart;
            Operation = operation;
            Condition = condition;
            _right = rightPart;
        }

        private string AliasName { get { return Attribute.AliasName; } }
        //  public AttrDef Def { get { return Attribute.IsSameAttrDef; } }
        // public string SourceName { get { return Source.AliasName; } }

        // TODO: 1. Добавить поддержку параметров в запросах
        // TODO: 2. Добавить поддержку связей Master-Slave в запросных контейнерах-средах исполнения запросов (например DataSet в Xls библиотеке)
        private string BuildCondition(string alias = null)
        {
            if (Condition == ConditionOperation.Include)
            {
                var s = BuildConditions(Conditions, alias);
                if (String.IsNullOrEmpty(s)) return s;
                return "(" + s + ")";
            }
            if (Condition == ConditionOperation.Exp)
            {
                var s = BuildConditions(Conditions, alias);
                if (String.IsNullOrEmpty(s)) return s;
                return "(" + s + ")";
            }

            var sql = Condition == ConditionOperation.Levenstein
                ? BuildLevensteinConditionLeftPart(alias)
                : BuildConditionLeftPart(alias);

            var operation = ComparisonOperationToString(Condition);

            if (Condition == ConditionOperation.Between || Condition == ConditionOperation.NotBetween)
                sql = String.Format("{0} {1} {2} and {3}", sql, operation, GetValueExp(0), GetValueExp(1));
            else if (Condition == ConditionOperation.In || Condition == ConditionOperation.NotIn)
            {
                if (!Right.IsAttribute)
                {
                    if (Right.Params == null && Right.SubQuery != null && Right.SubQueryAttribute != null)
                    {
                        operation  = Condition == ConditionOperation.In ? "EXISTS" : "NOT EXISTS";

                        /*sql = String.Format("{0} {1} (SELECT [sub].[{2}] FROM ({3}) as [sub])", sql, operation,
                            SubQueryAttribute.AliasName, SubQuery.BuildSql());*/
                        sql = String.Format("{1}(SELECT [sub].[{2}] FROM ({3}) as [sub] WHERE [sub].[{2}] = {0})", sql, operation,
                            Right.SubQueryAttribute.AliasName, Right.SubQuery.BuildSql());
                    }
                    else
                        sql = String.Format("{0} {1} ({2})", sql, operation, Right.Params != null ? GetValueSetExp() : "NULL");
                }
            }
            else if (Condition == ConditionOperation.IsNull || Condition == ConditionOperation.IsNotNull)
                sql = String.Format("{0} {1}", sql, operation);
            else if (Condition == ConditionOperation.Levenstein)
            {
                if (Right.IsValue)
                {
                    sql = String.Format("{0}({1}, {2}) >= {3}", operation, sql, GetLevensteinValueExp(),
                        SqlQuery.LevensteinCoefficient.ToString(CultureInfo.InvariantCulture).Replace(',', '.'));
                }
                else
                {
                    sql = String.Format("{0}({1}, {2}) >= {3}", operation, sql, BuildConditionRightPart(alias),
                        SqlQuery.LevensteinCoefficient.ToString(CultureInfo.InvariantCulture).Replace(',', '.'));
                }
            }
            else
                sql = String.Format("{0} {1} {2}", sql, operation, BuildConditionRightPart(alias));

            return sql;
        }

        public static string BuildConditions(IEnumerable<SqlQueryCondition> conditions, string alias = null)
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
                        var s = (condition.Conditions != null) ? BuildConditions(condition.Conditions, alias) : String.Empty;
                        if (String.IsNullOrEmpty(s)) return String.Empty;
                        sql = sql + "(" + s + ")"; 
                    }
                        break;
                    case ConditionOperation.Exp:
                    {
                        var s = (condition.Conditions != null) ? BuildConditions(condition.Conditions, alias) : String.Empty;
                        if (String.IsNullOrEmpty(s)) return String.Empty;
                        sql = sql + "(" + s + ")";
                    }
                        break;
                    default:
                        sql = sql + condition.BuildCondition(alias);
                        break;
                }

                first = false;
            }
            return sql;
        }

        private string GetValueExp()
        {
            if (Right.Params == null) return "NULL";

            return ValueToStr(Value);
        }

        /*private string GetStringValueExp()
        {
            if (Right.Params == null) return "''";

            var value = Right.Params.Length > 0 ? Right.Params[0].Value : null;
            return value != null ? ValueToStr(value) : "''";
        }*/

        private string GetLevensteinValueExp()
        {
            if (Right.Params == null) return "''";
            var value = Right.Params.Length > 0 ? Right.Params[0].Value : null;
            return value != null ? ValueToStr(value, true) : "''";
        }

        private string GetValueExp(int index)
        {
            if (Right.Params == null) return "NULL";

            return ValueToStr(Right.Params[index].Value);
        }

        private string ValueToStr(object value, bool levensteinFormat = false)
        {
            if (Attribute.Def != null)
            {
                switch ((CissaDataType) Attribute.Def.Type.Id)
                {
                    case CissaDataType.Text:
                        var s = Convert.ToString(value);
                        return "'" + s.ToUpper() + "'";
                    case CissaDataType.Int:
                    case CissaDataType.Float:
                    case CissaDataType.Currency:
                    case CissaDataType.Bool:
                        return value != null ? value.ToString().Replace(',', '.').ToUpper() : "NULL";
                    case CissaDataType.Enum:
                    case CissaDataType.Doc:
                    case CissaDataType.DocList:
                        return value != null ? "'" + value + "'" : "NULL";
                    case CissaDataType.DateTime:
                        if (value != null)
                        {
                            var dt = Convert.ToDateTime(value);
                            if (levensteinFormat) return "'" + dt.ToString("yyyyMMdd") + "'";
                            return "convert(date, '" + dt.ToString("dd'/'MM'/'yyyy") + "', 103)";
                        }
                        return "NULL";
                    case CissaDataType.Organization:
                    case CissaDataType.DocumentState:
                    case CissaDataType.User:
                    case CissaDataType.Process:
                    case CissaDataType.Form:
                    case CissaDataType.DocumentDef:
                    case CissaDataType.EnumDef:
                    case CissaDataType.OrgPosition:
                    case CissaDataType.OrgUnit:
                        return value != null ? "'" + value + "'" : "NULL";
                }
                throw new ApplicationException("Типы данных атрибутов не поддерживаются!");
            }

            switch (Attribute.Ident)
            {
                case SystemIdent.Id:
                case SystemIdent.OrgId:
                case SystemIdent.UserId:
                case SystemIdent.DefId:
                case SystemIdent.State:
                case SystemIdent.InState:
                    return value != null ? "'" + value + "'" : "NULL";
                case SystemIdent.Created:
                case SystemIdent.Modified:
                case SystemIdent.StateDate:
                    if (value != null)
                    {
                        var dt = Convert.ToDateTime(value);
                        if (levensteinFormat)
                            return dt.ToString("'yyyyMMdd'");
                        return "convert(date, '" + dt.ToString("dd'/'MM'/'yyyy") + "', 103)";
                    }
                    return "NULL";
                default:
                    return value != null ? "'" + value + "'" : "''";
            }
        }

        private string GetValueSetExp()
        {
            if (Right.Params == null) return "NULL";
            var s = "";
            foreach (var param in Right.Params)
            {
                if (s.Length > 0) s += ", ";
                s += ValueToStr(param.Value);
            }
            return s;
        }

        public static string ComparisonOperationToString(ConditionOperation condition)
        {
            switch (condition)
            {
                case ConditionOperation.Equal:
                    return "=";
                case ConditionOperation.NotEqual:
                    return "<>";
                case ConditionOperation.GreatThen:
                    return ">";
                case ConditionOperation.GreatEqual:
                    return ">=";
                case ConditionOperation.LessThen:
                    return "<";
                case ConditionOperation.LessEqual:
                    return "<=";
                case ConditionOperation.Like:
                    return "LIKE";
                case ConditionOperation.Contains:
                    return "LIKE";
                case ConditionOperation.NotLike:
                    return "NOT LIKE";
                case ConditionOperation.NotContains:
                    return "NOT LIKE";
                case ConditionOperation.IsNull:
                    return "IS NULL";
                case ConditionOperation.IsNotNull:
                    return "IS NOT NULL";
                case ConditionOperation.In:
                    return "IN";
                case ConditionOperation.NotIn:
                    return "NOT IN";
                case ConditionOperation.Between:
                    return "BETWEEN";
                case ConditionOperation.NotBetween:
                    return "NOT BETWEEN";
                case ConditionOperation.Levenstein:
                    return "dbo.Levenstein";
            }
            throw new ApplicationException("Операция сравнения не поддерживается!");
        }

        private string GetOperationStr(string leftPartExp = "")
        {
            var value = (Right.IsAttribute)
                ? BuildConditionRightPart()
                : (/*Right.Params == null &&*/ Right.SubQuery != null && Right.SubQueryAttribute != null)
                    ? String.Format("SELECT [sub].[{0}] FROM ({1}) AS [sub]",
                        Right.SubQueryAttribute.AliasName, Right.SubQuery.BuildSql())
                    : GetValueExp();

            switch (Condition)
            {
                case ConditionOperation.Equal:
                    return " = " + value;
                case ConditionOperation.NotEqual:
                    return " <> " + value;
                case ConditionOperation.GreatThen:
                    return " > " + value;
                case ConditionOperation.GreatEqual:
                    return " >= " + value;
                case ConditionOperation.LessThen:
                    return " < " + value;
                case ConditionOperation.LessEqual:
                    return " <= " + value;
                case ConditionOperation.Like:
                    return " LIKE " + value;
                case ConditionOperation.NotLike:
                    return " NOT LIKE " + value;
/*
                case ConditionOperation.Contains:
                    return " LIKE " + value;
                case ConditionOperation.NotLike:
                    return " NOT LIKE " + value;
*/
                case ConditionOperation.IsNull:
                    return " IS NULL";
                case ConditionOperation.IsNotNull:
                    return " IS NOT NULL";
                case ConditionOperation.In:
                    var s = (Right.IsValue ? GetValueSetExp() : value);
                    return " IN (" + s + ")";
                case ConditionOperation.NotIn:
                    var s1 = (Right.IsValue ? GetValueSetExp() : value);
                    return " NOT IN (" + s1 + ")";
                case ConditionOperation.Between:
                    if (Right.Params == null || Right.Params.Length < 2)
                        throw new ApplicationException("Ошибка в значениях операции сравнения Between");
                    return " BETWEEN " + value + " AND " + GetValueExp(1);
                case ConditionOperation.NotBetween:
                    if (Right.Params == null || Right.Params.Length < 2)
                        throw new ApplicationException("Ошибка в значениях операции сравнения Between");
                    return " NOT BETWEEN " + value + " AND " + GetValueExp(1);
                case ConditionOperation.Levenstein:
                    return "dbo.Levenstein(" + leftPartExp + "," + value + ") >= " +
                           SqlQuery.LevensteinCoefficient.ToString(CultureInfo.InvariantCulture).Replace(',', '.');

            }
            throw new ApplicationException("Операция сравнения не поддерживается!");
        }

        private const string ExistsDocStateConditionSql =
            "EXISTS(SELECT [Document_Id] FROM [Document_States] ds {0}\n" +
            "WHERE [ds].[State_Type_Id]{1} AND [ds].[Document_Id] = [{2}].[Id])";

        public string BuildConditionLeftPart(string alias = null)
        {
            if (Attributes != null && Attributes.Count > 0)
            {
                var i = 0;
                var conds = new object[Attributes.Count];

                foreach (var attr in Attributes)
                    if (String.IsNullOrEmpty(alias))
                        conds.SetValue("[" + attr.Source.AliasName + "].[" + attr.Attribute.AliasName + "]", i++);
                    else
                        conds.SetValue(alias, i++);

                var exp = (string) conds[0];
                if (!String.IsNullOrEmpty(Left.Expression))
                    exp = String.Format(Left.Expression, conds);
                return exp;
            }
            return "NULL";
        }

        public string BuildLevensteinConditionLeftPart(string alias = null)
        {
            if (Attributes != null && Attributes.Count > 0)
            {
                var i = 0;
                var conds = new object[Attributes.Count];

                foreach (var attr in Attributes)
                {
                    if (String.IsNullOrEmpty(alias))
                        conds.SetValue("[" + attr.Source.AliasName + "].[" + attr.Attribute.AliasName + "]", i);
                    else
                        conds.SetValue(alias, i);
                    if (attr.Attribute.Def != null &&
                        CissaDataTypeHelper.ConvertToBase(attr.Attribute.Def.Type.Id) == BaseDataType.DateTime)
                        conds.SetValue(String.Format("convert(varchar(8), {0}, 112)", conds[i]), i);
                    else if (attr.Attribute.Def == null &&
                             SystemIdentConverter.ToBaseType(attr.Attribute.Ident) == BaseDataType.DateTime)
                        conds.SetValue(String.Format("convert(varchar(8), {0}, 112)", conds[i]), i);

                        i++;
                }

                var exp = (string)conds[0];
                if (!String.IsNullOrEmpty(Left.Expression))
                    exp = String.Format(Left.Expression, conds);
                return exp;
            }
            return "NULL";
        }
        public string BuildConditionRightPart(string alias = null)
        {
            if (Right.Attributes != null && Right.Attributes.Count > 0)
            {
                var i = 0;
                var conds = new object[Right.Attributes.Count];

                foreach (var attr in Right.Attributes)
                    if (String.IsNullOrEmpty(alias))
                        conds.SetValue("[" + attr.Source.AliasName + "].[" + attr.Attribute.AliasName + "]", i++);
                    else
                        conds.SetValue(alias, i++);

                var exp = (string) conds[0];
                if (!String.IsNullOrEmpty(Right.Expression))
                    exp = String.Format(Right.Expression, conds);
                return exp;
            }

            var value = (Right.Params == null && Right.SubQuery != null && Right.SubQueryAttribute != null)
                            ? String.Format("SELECT [sub].[{0}] FROM ({1}) AS [sub]",
                                            SubQueryAttribute.AliasName, SubQuery.BuildSql())
                            : GetValueExp();
            return value;
        }

        // Вызывается из SqlQuery для условий в разделе Where и Having
        public string GetConditionExp(string alias = null)
        {
            var i = 0;
            var conds = new object[Left.Attributes.Count];

            foreach (var attr in Left.Attributes)
                if (String.IsNullOrEmpty(alias))
                    conds.SetValue("[" + attr.Source.AliasName + "].[" + attr.Attribute.AliasName + "]", i++);
                else
                    conds.SetValue(alias, i++);

            var exp = (string) conds[0];
            if (!String.IsNullOrEmpty(Left.Expression))
                exp = String.Format(Left.Expression, conds);
//            var s = "[" + SourceName + "].[" + AliasName + "]";
            if (Left.Attribute != null && Left.Attribute.Ident == SystemIdent.InState)
            {
                var withNoLock = Source.WithNoLock ? "WITH(NOLOCK)" : "";

                return String.Format(ExistsDocStateConditionSql, withNoLock, GetOperationStr(), Source.AliasName);
            }
            if (Condition == ConditionOperation.Levenstein)
            {
                if (Left.IsAttribute && Left.Attributes.Count == 1)
                {
                    var attr = Left.Attribute;
                    if (attr != null && attr.Def != null && attr.Def.Type.Id == (short) CissaDataType.DateTime)
                        exp = String.Format("convert(date, {0})", exp);
                }
                return GetOperationStr(exp);
            }
            return exp + GetOperationStr();
        }

        private const string SubQueryConditionSql =
            "(SELECT [Document_Id] FROM [{0}] {5}\n" +
            "WHERE [Def_Id] = '{1}' AND [Expired] = '99991231' AND [Value] {2}) as [{3}_sc{4}] ON [d].Id = [{3}_sc{4}].[Document_Id]";

        public string GetConditionSubQuery(int index)
        {
            var table = Attribute.GetAttrDefTableName();

            var withNoLock = Source.WithNoLock ? "WITH(NOLOCK)" : "";
            var s = String.Format(SubQueryConditionSql,
                table, Attribute.Def.Id, GetOperationStr(), AliasName, index, withNoLock);

            return s;
        }

        public string GetConditionInnerQuery(int index)
        {
            var withNoLock = Source.WithNoLock ? "WITH(NOLOCK)" : "";

            if (Attribute.Def != null)
            {
                var table = Attribute.GetAttrDefTableName();

                var valueFieldName = Attribute.Def.Type.Id == (short) CissaDataType.Text ? "Upper_" : "";

                var s = String.Format(
                    "(select [Document_Id] from [{0}] {5}\n" +
                    " where [Def_Id] = '{1}' and [{6}Value]{2} and [Expired] = '99991231') as [{3}_sc{7}] on [{4}].Id = [{3}_sc{7}].[Document_Id]",
                    table, Attribute.Def.Id, GetOperationStr(), AliasName, Source.AliasName, withNoLock, valueFieldName,
                    index);

                return s;
            }
            if (Attribute.Ident == SystemIdent.State)
            {
                var s = String.Format(
                    "(select [Document_Id] from [Document_States] {3}\n" +
                    " where [State_Type_Id]{0} and [Expired] = '99991231') as [{1}_sc{4}] on [{2}].Id = [{1}_sc{4}].[Document_Id]",
                    GetOperationStr(), AliasName, Source.AliasName, withNoLock, index);

                return s;
            }
            // DONE: Доделать InState!!! 
            // DONE: Исключить данный функционал, т.к. дает не правильный результат - Переделан в Exists
            /*if (Attribute.Ident == SystemIdent.InState)
            {
                var s = String.Format(
                    "(select [Document_Id] from [Document_States] {3}\n" +
                    " where [State_Type_Id]{0}) as [{1}_sc{4}] on [{2}].Id = [{1}_sc{4}].[Document_Id]",
                    GetOperationStr(), AliasName, Source.AliasName, withNoLock, index);

                return s;
            }*/
            throw new ApplicationException("Не могу сформировать вложенное условие выборки!");
        }

        public string GetAttributeExp(SqlQueryConditionPart part, IDictionary<SqlQuerySourceAttribute, int> attrIndexDictionary, 
            DocumentTableMap tableMap, string fromAlias, string orgAlias, string createdAlias, string modifiedAlias, string userAlias,
            string defAlias)
        {
            if (part.Attributes == null || part.Attributes.Count == 0)
            {
                return (part.Params == null && part.SubQuery != null && part.SubQueryAttribute != null)
                    ? String.Format("SELECT [sub].[{0}] FROM ({1}) AS [sub]",
                        part.SubQueryAttribute.AliasName, part.SubQuery.BuildSql())
                    : GetValueExp();
            }

            var attrs = new object[part.Attributes.Count];
            var i = 0;
            foreach (var attribute in part.Attributes)
            {
                var attr = attribute.Attribute;

                int n;
                var hasN = attrIndexDictionary.TryGetValue(attr, out n);
                var errMsg = String.Format("При формировании условия выборки, индекс атрибута \"{0}\" не найден",
                                           attr.AliasName);

                if (attr.Def != null)
                {
                    if (!hasN) throw new ApplicationException(errMsg);

                    if (attr.Def.Type.Id == (short) CissaDataType.DocList)
                        attrs[i] = String.Format("[dla{0}].[Value]", n);
                    else if (attr.Def.Type.Id == (short) CissaDataType.Text)
                        attrs[i] = String.Format("[a{0}].[Upper_Value]", n);
                    else if (attr.Def.Type.Id == (short)CissaDataType.DateTime && Condition == ConditionOperation.Levenstein)
                        attrs[i] = String.Format("convert(date, [a{0}].[Value])", n);
                    else
                        attrs[i] = String.Format("[a{0}].[Value]", n);
                }
                else if (attr.Ident == SystemIdent.State || attr.Ident == SystemIdent.InState)
                {
                    var stateTableField = tableMap != null ? tableMap.FindIdentField("State") : null;

                    if (stateTableField != null)
                        attrs[i] = String.Format("[{0}].[State]", fromAlias);
                    else
                    {
                        if (!hasN) throw new ApplicationException(errMsg);
                        attrs[i] = String.Format("[ds{0}].[State_Type_Id]", n);
                    }
                }
                else if (attr.Ident == SystemIdent.StateDate)
                {
                    if (!hasN) throw new ApplicationException(errMsg);
                    attrs[i] = String.Format("[ds{0}].[Created]", n);
                }
                else if (attr.Ident == SystemIdent.Id)
                {
                    attrs[i] = String.Format("[{0}].[Id]", fromAlias);
                }
                else if (attr.Ident == SystemIdent.OrgId)
                {
                    attrs[i] = String.Format("[{0}].[Organization_Id]", orgAlias);
                }
                else if (attr.Ident == SystemIdent.Created)
                {
                    attrs[i] = String.Format("[{0}].[Created]", createdAlias);
                }
                else if (attr.Ident == SystemIdent.Modified)
                {
                    attrs[i] = String.Format("[{0}].[Last_Modified]", modifiedAlias);
                }
                else if (attr.Ident == SystemIdent.UserId)
                {
                    attrs[i] = String.Format("[{0}].[UserId]", userAlias);
                }
                else if (attr.Ident == SystemIdent.DefId)
                {
                    attrs[i] = String.Format("[{0}].[Def_Id]", defAlias);
                }
                else if (attr.Ident == SystemIdent.UserName)
                {
                    if (!hasN) throw new ApplicationException(errMsg);
                    attrs[i] = String.Format("[w{0}].[User_Name]", n);
                }
                else if (attr.Ident == SystemIdent.OrgCode)
                {
                    if (!hasN) throw new ApplicationException(errMsg);
                    attrs[i] = String.Format("[o{0}].[Code]", n);
                }
                else if (attr.Ident == SystemIdent.OrgName)
                {
                    if (!hasN) throw new ApplicationException(errMsg);
                    attrs[i] = String.Format("[od{0}].[Full_Name]", n);
                }
                else
                    throw new ApplicationException("Не известный атрибут в условии запроса!");
                i++;
            }
            var exp = (string)attrs[0];
            if (!String.IsNullOrEmpty(part.Expression))
                exp = String.Format(part.Expression, attrs);
            return exp;
        }

        public string BuildSourceCondition(IDictionary<SqlQuerySourceAttribute, int> attrIndexDictionary, DocumentTableMap tableMap,
            bool needToJoinDocument)
        {
            if (Condition == ConditionOperation.Include || Condition == ConditionOperation.Exp)
            {
                if (Conditions != null && Conditions.Count > 0)
                    return /*"(" +*/ BuildSourceConditions(Conditions, attrIndexDictionary, tableMap, needToJoinDocument) /*+
                           ")"*/;
                return String.Empty;
            }

            var fromAlias = tableMap != null ? "t" : "d";
            var orgAlias = (tableMap != null && needToJoinDocument) ? "t" : "d";
            var createdAlias = (tableMap != null && tableMap.FindIdentField("Created") != null) ? "t" : "d";
            var modifiedAlias = (tableMap != null && tableMap.FindIdentField("Last_Modified") != null) ? "t" : "d";
            var userAlias = (tableMap != null && tableMap.FindIdentField("User_Id") != null) ? "t" : "d";
            var defAlias = (tableMap != null && tableMap.FindIdentField("Def_Id") != null) ? "t" : "d";

            var exp = GetAttributeExp(Left, attrIndexDictionary, tableMap, fromAlias, orgAlias, createdAlias,
                modifiedAlias, userAlias, defAlias);
            /*object[] attrs = new object[Attributes.Count];
            var i = 0;
            foreach (var attribute in Left.Attributes)
            {
                var attr = attribute.Attribute;

                int n;
                var hasN = attrIndexDictionary.TryGetValue(attr, out n);
                var errMsg = String.Format("При формировании условия выборки, индекс атрибута \"{0}\" не найден",
                                           attr.AliasName);

                if (attr.Def != null)
                {
                    if (!hasN) throw new ApplicationException(errMsg);

                    if (attr.Def.Type.Id == (short)CissaDataType.DocList)
                        attrs[i] = String.Format("[dla{0}].[Value]", n);
                    else
                        attrs[i] = String.Format("[a{0}].[Value]", n);
                }
                else if (attr.Ident == SystemIdent.State || attr.Ident == SystemIdent.InState)
                {
                    var stateTableField = tableMap != null ? tableMap.FindIdentField("State") : null;

                    if (stateTableField != null)
                        attrs[i] = String.Format("[{0}].[State]", fromAlias);
                    else
                    {
                        if (!hasN) throw new ApplicationException(errMsg);
                        attrs[i] = String.Format("[ds{0}].[State_Type_Id]", n);
                    }
                }
                else if (attr.Ident == SystemIdent.Id)
                {
                    attrs[i] = String.Format("[{0}].[Id]", fromAlias);
                }
                else if (attr.Ident == SystemIdent.OrgId)
                {
                    attrs[i] = String.Format("[{0}].[Organization_Id]", orgAlias);
                }
                else if (attr.Ident == SystemIdent.Created)
                {
                    attrs[i] = String.Format("[{0}].[Created]", createdAlias);
                }
                else if (attr.Ident == SystemIdent.Modified)
                {
                    attrs[i] = String.Format("[{0}].[Last_Modified]", modifiedAlias);
                }
                else if (attr.Ident == SystemIdent.UserId)
                {
                    attrs[i] = String.Format("[{0}].[UserId]", userAlias);
                }
                else if (attr.Ident == SystemIdent.DefId)
                {
                    attrs[i] = String.Format("[{0}].[Def_Id]", defAlias);
                }
                else if (attr.Ident == SystemIdent.UserName)
                {
                    if (!hasN) throw new ApplicationException(errMsg);
                    attrs[i] = String.Format("[w{0}].[User_Name]", n);
                }
                else if (attr.Ident == SystemIdent.OrgCode)
                {
                    if (!hasN) throw new ApplicationException(errMsg);
                    attrs[i] = String.Format("[o{0}].[Code]", n);
                }
                else if (attr.Ident == SystemIdent.OrgName)
                {
                    if (!hasN) throw new ApplicationException(errMsg);
                    attrs[i] = String.Format("[od{0}].[Full_Name]", n);
                }
                else
                    throw new ApplicationException("Не известный атрибут в условии запроса!");
                i++;
            }
            var exp = (string) attrs[0];
            if (!String.IsNullOrEmpty(Left.Expression))
                exp = String.Format(Left.Expression, attrs);*/

            var operation = ComparisonOperationToString(Condition);

            if (Condition == ConditionOperation.Between || Condition == ConditionOperation.NotBetween)
                exp = String.Format("{0} {1} {2} and {3}", exp, operation, GetValueExp(0), GetValueExp(1));
            else if (Condition == ConditionOperation.In || Condition == ConditionOperation.NotIn)
            {
                if (!Right.IsAttribute)
                {
                    /*if (Right.Params == null && SubQuery != null && SubQueryAttribute != null)*/
                    if (Right.Params == null && Right.SubQuery != null && Right.SubQueryAttribute != null)
                    {
                        operation = Condition == ConditionOperation.In ? "EXISTS" : "NOT EXISTS";

                        /*exp = String.Format("{0} {1} (SELECT [sub].[{2}] FROM ({3}) as [sub])", exp, operation,
                            SubQueryAttribute.AliasName, SubQuery.BuildSql());*/
                        exp = String.Format("{1}(SELECT [sub].[{2}] FROM ({3}) as [sub] WHERE [sub].[{2}] = {0})", exp, operation,
                            Right.SubQueryAttribute.AliasName, Right.SubQuery.BuildSql());
                    }
                    else
                        exp = String.Format("{0} {1} ({2})", exp, operation, Right.Params != null ? GetValueSetExp() : "NULL");
                }
                else
                {
                    exp = String.Format("{0} {1} ({2})", exp, operation,
                        GetAttributeExp(Right, attrIndexDictionary, tableMap, fromAlias, orgAlias, createdAlias,
                            modifiedAlias, userAlias, defAlias));
                }
            }
            else if (Condition == ConditionOperation.IsNull || Condition == ConditionOperation.IsNotNull)
                exp = String.Format("{0} {1}", exp, operation);
            else if (Condition == ConditionOperation.Levenstein)
                exp = "dbo.Levenstein(" + exp + "," + GetValueExp(0) + ") >= " +
                           SqlQuery.LevensteinCoefficient.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
            else
                exp = String.Format("{0} {1} {2}", exp, operation,
                    GetAttributeExp(Right, attrIndexDictionary, tableMap, fromAlias, orgAlias, createdAlias,
                        modifiedAlias, userAlias, defAlias));

            return exp;
        }

        private static string BuildSourceConditions(IEnumerable<SqlQueryCondition> conditions, IDictionary<SqlQuerySourceAttribute, int> attrIndexDictionary, DocumentTableMap tableMap, 
            bool needToJoinDocument)
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

                if (condition.Condition == ConditionOperation.Include || condition.Condition == ConditionOperation.Exp)
                {
                    var s = BuildSourceConditions(condition.Conditions, attrIndexDictionary, tableMap,
                        needToJoinDocument);
                    if (!String.IsNullOrEmpty(s)) sql = sql + "(" + s + ")";
                }
                else
                    sql = sql + condition.BuildSourceCondition(attrIndexDictionary, tableMap, needToJoinDocument);

                first = false;
            }
            return sql;
        }

        internal SqlQueryCondition AddExpCondition(ExpressionOperation operation)
        {
            var item = new SqlQueryCondition(null, operation, Guid.Empty, ConditionOperation.Exp, null);

            Conditions.Add(item);

            return item;
        }
    }
}