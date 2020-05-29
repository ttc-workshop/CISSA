using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces;
using System;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Builders
{
    public class QueryConditionBuilder : BaseQueryBuilder, IQueryCondition
    {
        public QueryConditionDef Condition { get; private set; }

        public QueryConditionBuilder(BaseQueryBuilder parent, QueryConditionDef condition)
            : base(parent)
        {
            Condition = condition;
        }

        public BaseExpressionBuilder GetParentExpression()
        {
            var b = Parent;

            while (b != null)
            {
                if (b is BaseExpressionBuilder) return (BaseExpressionBuilder) b;
                b = b.Parent;
            }
            return null;
        }

        public IQueryExpression Eq(object value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef {Values = new[] {value}};
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[]
                    {new QueryConditionValueDef {Name = paramName, Value = value}}
            };
            //Condition.Values = new[] { value };
            Condition.Condition = ConditionOperation.Equal;
            return GetParentExpression();
        }

        public IQueryExpression Eq(string attribute, string source)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Attribute =
                    new QuerySingleAttributeDef
                    {
                        Attribute = new QueryAttributeRef { AttributeName = attribute, Source = GetSource(source) }
                    }
            };
            Condition.Condition = ConditionOperation.Equal;
            return GetParentExpression();
        }

        public IQueryExpression Neq(object value, string paramName = null)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] { new QueryConditionValueDef { Name = paramName, Value = value } }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new[] { value } };
            //Condition.Values = new[] { value };
            Condition.Condition = ConditionOperation.NotEqual;
            return GetParentExpression();
        }

        public IQueryExpression Neq(string attribute, string source)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Attribute =
                    new QuerySingleAttributeDef
                    {
                        Attribute = new QueryAttributeRef { AttributeName = attribute, Source = GetSource(source) }
                    }
            };
            Condition.Condition = ConditionOperation.NotEqual;
            return GetParentExpression();
        }

        public IQueryExpression Ge(object value, string paramName = null)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] { new QueryConditionValueDef { Name = paramName, Value = value } }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new[] { value } };
            //Condition.Values = new[] { value };
            Condition.Condition = ConditionOperation.GreatEqual;
            return GetParentExpression();
        }

        public IQueryExpression Ge(string attribute, string source)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Attribute =
                    new QuerySingleAttributeDef
                    {
                        Attribute = new QueryAttributeRef { AttributeName = attribute, Source = GetSource(source) }
                    }
            };
            Condition.Condition = ConditionOperation.GreatEqual;
            return GetParentExpression();
        }

        public IQueryExpression Gt(object value, string paramName = null)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] { new QueryConditionValueDef { Name = paramName, Value = value } }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new[] { value } };
            //Condition.Values = new[] { value };
            Condition.Condition = ConditionOperation.GreatThen;
            return GetParentExpression();
        }

        public IQueryExpression Gt(string attribute, string source)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Attribute =
                    new QuerySingleAttributeDef
                    {
                        Attribute = new QueryAttributeRef { AttributeName = attribute, Source = GetSource(source) }
                    }
            };
            Condition.Condition = ConditionOperation.GreatThen;
            return GetParentExpression();
        }

        public IQueryExpression Le(object value, string paramName = null)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] { new QueryConditionValueDef { Name = paramName, Value = value } }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new[] { value } };
            //Condition.Values = new[] { value };
            Condition.Condition = ConditionOperation.LessEqual;
            return GetParentExpression();
        }

        public IQueryExpression Le(string attribute, string source)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Attribute =
                    new QuerySingleAttributeDef
                    {
                        Attribute = new QueryAttributeRef { AttributeName = attribute, Source = GetSource(source) }
                    }
            };
            Condition.Condition = ConditionOperation.LessEqual;
            return GetParentExpression();
        }

        public IQueryExpression Lt(object value, string paramName = null)
        {
            //Condition.Right = new QueryConditionPartDef { Values = new[] { value } };
            //Condition.Values = new[] { value };
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] { new QueryConditionValueDef { Name = paramName, Value = value } }
            };
            Condition.Condition = ConditionOperation.LessThen;
            return GetParentExpression();
        }

        public IQueryExpression Lt(string attribute, string source)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Attribute =
                    new QuerySingleAttributeDef
                    {
                        Attribute = new QueryAttributeRef { AttributeName = attribute, Source = GetSource(source) }
                    }
            };
            Condition.Condition = ConditionOperation.LessThen;
            return GetParentExpression();
        }

        public IQueryExpression Contains(string value, string paramName = null)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] { new QueryConditionValueDef { Name = paramName, Value = value } }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new object[] { value } };
            //Condition.Values = new object[] { value };
            Condition.Condition = ConditionOperation.Contains;
            return GetParentExpression();
        }

        public IQueryExpression Like(string value, string paramName = null)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] { new QueryConditionValueDef { Name = paramName, Value = value } }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new object[] { value } };
            //Condition.Values = new object[] { value };
            Condition.Condition = ConditionOperation.Like;
            return GetParentExpression();
        }

        public IQueryExpression Between(object value1, object value2, string paramName1 = null, string paramName2 = null)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[]
                    {
                        new QueryConditionValueDef { Name = paramName1, Value = value1 },
                        new QueryConditionValueDef { Name = paramName2, Value = value2 }
                    }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new[] { value1, value2 } };
            //Condition.Values = new[] { value1, value2 };
            //Condition.Value2 = value2;
            Condition.Condition = ConditionOperation.Between;
            return GetParentExpression();
        }

        public IQueryExpression NotContains(string value, string paramName = null)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] { new QueryConditionValueDef { Name = paramName, Value = value } }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new object[] { value } };
            //Condition.Values = new object[] { value };
            Condition.Condition = ConditionOperation.NotContains;
            return GetParentExpression();
        }

        public IQueryExpression NotLike(string value, string paramName = null)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] { new QueryConditionValueDef { Name = paramName, Value = value } }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new object[] { value } };
            //Condition.Values = new object[] { value };
            Condition.Condition = ConditionOperation.NotLike;
            return GetParentExpression();
        }

        public IQueryExpression NotBetween(object value1, object value2, string paramName1 = null, string paramName2 = null)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[]
                    {
                        new QueryConditionValueDef { Name = paramName1, Value = value1 },
                        new QueryConditionValueDef { Name = paramName2, Value = value2 }
                    }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new[] { value1, value2 } };
            //Condition.Values = new[] { value1, value2 };
            Condition.Condition = ConditionOperation.NotBetween;
            return GetParentExpression();
        }

        public IQueryExpression IsNull()
        {
            Condition.Condition = ConditionOperation.IsNull;
            return GetParentExpression();
        }

        public IQueryExpression IsNotNull()
        {
            Condition.Condition = ConditionOperation.IsNotNull;
            return GetParentExpression();
        }

        public IQueryExpression Is(string value)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] { new QueryConditionValueDef { Name = null, Value = value } }
            };
            //Condition.Right = new QueryConditionPartDef { Values = new object[] { value } };
            //Condition.Values = new object[] { value };
            Condition.Condition = ConditionOperation.Is;
            return GetParentExpression();
        }

        public IQueryCondition Include(string attribute)
        {
            var condition = CreateConditionDef(attribute, ExpressionOperation.And, ""); //new QueryConditionDef {AttributeName = attribute, Operation = ExpressionOperation.And};
            Condition.Conditions.Add(condition);
            Condition.Condition = ConditionOperation.Include;
            return new ConditionExpressionBuilder(this, condition);
        }

        public IQueryCondition Exp(string attribute)
        {
            var condition = CreateConditionDef(attribute, ExpressionOperation.And, "");  //new QueryConditionDef { AttributeName = attribute, Operation = ExpressionOperation.And };
            Condition.Conditions.Add(condition);
            Condition.Condition = ConditionOperation.Exp;
            return new ConditionExpressionBuilder(this, condition);
        }

        public IQueryExpression In(QueryDef subQuery)
        {
            Condition.Right = new QueryConditionPartDef {SubQuery = new SubQueryDef {QueryDef = subQuery}};
            //Condition.SubQueryDef = subQuery;
            Condition.Condition = ConditionOperation.In;
            return GetParentExpression();
        }

        public IQueryExpression In(QueryDef subQuery, string attribute)
        {
            var helper = new QueryDefHelper(subQuery);
            var subAttr = helper.AddAttribute(attribute);
            Condition.Right = new QueryConditionPartDef { SubQuery = new SubQueryDef { QueryDef = subQuery, Attribute = subAttr} };
            //Condition.SubQueryDef = subQuery;
            //Condition.SubQueryAttribute = attribute;
            Condition.Condition = ConditionOperation.In;
            return GetParentExpression();
        }

        public IQueryExpression In(string attribute, string source)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Attribute =
                    new QuerySingleAttributeDef
                    {
                        Attribute = new QueryAttributeRef { AttributeName = attribute, Source = GetSource(source) }
                    }
            };
            Condition.Condition = ConditionOperation.In;
            return GetParentExpression();
        }

        public IQueryExpression In(object[] values)
        {
            Condition.Right = new QueryConditionPartDef {Values = values};
            //Condition.Values = values;
            Condition.Condition = ConditionOperation.In;
            return GetParentExpression();
        }

        public IQueryExpression NotIn(QueryDef subQuery)
        {
            Condition.Right = new QueryConditionPartDef { SubQuery = new SubQueryDef { QueryDef = subQuery } };
            //Condition.SubQueryDef = subQuery;
            Condition.Condition = ConditionOperation.NotIn;
            return GetParentExpression();
        }

        public IQueryExpression NotIn(QueryDef subQuery, string attribute)
        {
            var helper = new QueryDefHelper(subQuery);
            var subAttr = helper.AddAttribute(attribute);
            Condition.Right = new QueryConditionPartDef { SubQuery = new SubQueryDef { QueryDef = subQuery, Attribute = subAttr } };
            //Condition.SubQueryDef = subQuery;
            //Condition.SubQueryAttribute = attribute;
            Condition.Condition = ConditionOperation.NotIn;
            return GetParentExpression();
        }

        public IQueryExpression NotIn(object[] values)
        {
            Condition.Right = new QueryConditionPartDef { Values = values };
            //Condition.Values = values;
            Condition.Condition = ConditionOperation.NotIn;
            return GetParentExpression();
        }

        internal override void AddCondition(QueryConditionDef condition)
        {
            Condition.Conditions.Add(condition);
        }
    }
}
