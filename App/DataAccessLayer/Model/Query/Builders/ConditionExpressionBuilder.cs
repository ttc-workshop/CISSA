using System;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces;
using System.Collections.Generic;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Builders
{
    public class ConditionExpressionBuilder: BaseExpressionBuilder, IQueryCondition
    {
        public QueryConditionDef Condition { get; private set; }

        public ConditionExpressionBuilder(BaseQueryBuilder parent, QueryConditionDef condition)
            : base(parent)
        {
            Condition = condition;
        }

        public IQueryCondition Where(string attribute)
        {
            return CreateCondition(attribute, ExpressionOperation.And, String.Empty);
        }

        public override IQueryCondition CreateCondition(string attribute, ExpressionOperation operation, string source)
        {
            var condition = CreateConditionDef(attribute, ExpressionOperation.And, source); // new QueryConditionDef {AttributeName = attribute, Operation = operation, Exp = source};

            Parent.AddCondition(condition);
            //Condition.WhereConditions.Add(condition);

            return new QueryConditionBuilder(this /*Parent*/, condition);
        }

        public override IQueryCondition And(string attribute, string source = null)
        {
            return CreateCondition(attribute, ExpressionOperation.And, source);
        }

        public override IQueryCondition Or(string attribute, string source = null)
        {
            return CreateCondition(attribute, ExpressionOperation.Or, source);
        }

        public override IQueryCondition AndNot(string attribute, string source = null)
        {
            return CreateCondition(attribute, ExpressionOperation.AndNot, source);
        }

        public override IQueryCondition OrNot(string attribute, string source = null)
        {
            return CreateCondition(attribute, ExpressionOperation.OrNot, source);
        }

        public override IQueryExpression End()
        {
            var parent = Parent;

            while (parent != null)
            {
                if (parent is IQueryExpression) return (IQueryExpression)parent;
                parent = parent.Parent;
            }
            return null;
        }

        internal override void AddCondition(QueryConditionDef condition)
        {
            Condition.Conditions.Add(condition);
        }

        private void CheckNotExpression()
        {
            if (Condition.Condition == ConditionOperation.Exp)
                throw new ApplicationException("Ошибка в запросе!");
        }

        public IQueryExpression Eq(object value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef {Values = new[] {value}}; //Values = new[] { value };
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[]
                    {
                        new QueryConditionValueDef {Name = paramName, Value = value}
                    }
            };
            Condition.Condition = ConditionOperation.Equal;
            return this;
        }

        public IQueryExpression Eq(string attribute, string source)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Attribute =
                    new QuerySingleAttributeDef
                    {
                        Attribute = new QueryAttributeRef {AttributeName = attribute, Source = GetSource(source)}
                    }
            }; //Values = new[] { value };
            Condition.Condition = ConditionOperation.Equal;
            return this;
        }

        public IQueryExpression Neq(object value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef { Values = new[] { value } }; //Values = new[] { value };
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] {new QueryConditionValueDef {Name = paramName, Value = value}}
            };
            Condition.Condition = ConditionOperation.NotEqual;
            return this;
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
            return this;
        }

        public IQueryExpression Ge(object value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef { Values = new[] { value } }; //Values = new[] { value };
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] {new QueryConditionValueDef {Name = paramName, Value = value}}
            };
            Condition.Condition = ConditionOperation.GreatEqual;
            return this;
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
            return this;
        }

        public IQueryExpression Gt(object value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef { Values = new[] { value } }; //Values = new[] { value };
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] {new QueryConditionValueDef {Name = paramName, Value = value}}
            };
            Condition.Condition = ConditionOperation.GreatThen;
            return this;
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
            return this;
        }

        public IQueryExpression Le(object value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef { Values = new[] { value } }; //Values = new[] { value };
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] {new QueryConditionValueDef {Name = paramName, Value = value}}
            };
            Condition.Condition = ConditionOperation.LessEqual;
            return this;
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
            return this;
        }

        public IQueryExpression Lt(object value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef { Values = new[] { value } }; //Values = new[] { value };
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] {new QueryConditionValueDef {Name = paramName, Value = value}}
            };
            Condition.Condition = ConditionOperation.LessThen;
            return this;
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
            return this;
        }

        public IQueryExpression Contains(string value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef { Values = new[] { value } }; //Values = new[] { value };
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] {new QueryConditionValueDef {Name = paramName, Value = value}}
            };
            Condition.Condition = ConditionOperation.Contains;
            return this;
        }

        public IQueryExpression Like(string value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef { Values = new[] { value } }; //Values = new[] { value };
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] {new QueryConditionValueDef {Name = paramName, Value = value}}
            };
            Condition.Condition = ConditionOperation.Like;
            return this;
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
            //Condition.Right = new QueryConditionPartDef { Values = new[] { value1, value2 } }; //Values = new[] { value1, value2 };
            //Condition.Value2 = value2;
            Condition.Condition = ConditionOperation.Between;
            return this;
        }

        public IQueryExpression NotContains(string value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef { Values = new[] { value } }; //Values = new[] { value };
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] {new QueryConditionValueDef {Name = paramName, Value = value}}
            };
            Condition.Condition = ConditionOperation.NotContains;
            return this;
        }

        public IQueryExpression NotLike(string value, string paramName = null)
        {
            /*if (String.IsNullOrEmpty(paramName))
                Condition.Right = new QueryConditionPartDef { Values = new[] { value } }; //Values = new[] { value };
            else*/
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[] {new QueryConditionValueDef {Name = paramName, Value = value}}
            };
            Condition.Condition = ConditionOperation.NotLike;
            return this;
        }

        public IQueryExpression NotBetween(object value1, object value2, string paramName1 = null, string paramName2 = null)
        {
            //Condition.Right = new QueryConditionPartDef { Values = new[] { value1, value2 } }; //Values = new[] { value1, value2 };
            Condition.Right = new QueryConditionPartDef
            {
                Params =
                    new QueryConditionValueDef[]
                    {
                        new QueryConditionValueDef { Name = paramName1, Value = value1 },
                        new QueryConditionValueDef { Name = paramName2, Value = value2 }
                    }
            };
            Condition.Condition = ConditionOperation.NotBetween;
            return this;
        }

        public IQueryExpression IsNull()
        {
            Condition.Condition = ConditionOperation.IsNull;
            return this;
        }

        public IQueryExpression IsNotNull()
        {
            Condition.Condition = ConditionOperation.IsNotNull;
            return this;
        }

        public IQueryExpression Is(string value)
        {
            Condition.Right = new QueryConditionPartDef
            {
                Params = new QueryConditionValueDef[] {new QueryConditionValueDef {Value = value}}
            }; //Values = new object[] { value };
            Condition.Condition = ConditionOperation.Is;
            return this;
        }

        public IQueryExpression In(QueryDef subQuery)
        {
            Condition.Right = new QueryConditionPartDef { SubQuery = new SubQueryDef{QueryDef = subQuery}}; //SubQueryDef = subQuery;
            Condition.Condition = ConditionOperation.In;
            return this;
        }

        public IQueryExpression In(QueryDef subQuery, string attribute)
        {
            var helper = new QueryDefHelper(subQuery);
            var attr = helper.AddAttribute(attribute);
            Condition.Right = new QueryConditionPartDef
            {
                SubQuery = new SubQueryDef {QueryDef = subQuery, Attribute = attr}
            };
            //SubQueryDef = subQuery;
            //Condition.SubQueryAttribute = attribute;
            Condition.Condition = ConditionOperation.In;
            return this;
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
            return this;

        }

        public IQueryExpression In(object[] values)
        {
            Condition.Right = new QueryConditionPartDef { Values = values }; //Values = values;
            Condition.Condition = ConditionOperation.In;
            return this;
        }

        public IQueryExpression NotIn(QueryDef subQuery)
        {
            Condition.Right = new QueryConditionPartDef { SubQuery = new SubQueryDef{QueryDef = subQuery}}; //SubQueryDef = subQuery;
            Condition.Condition = ConditionOperation.NotIn;
            return this;
        }

        public IQueryExpression NotIn(QueryDef subQuery, string attribute)
        {
            var helper = new QueryDefHelper(subQuery);
            var attr = helper.AddAttribute(attribute);
            Condition.Right = new QueryConditionPartDef
            {
                SubQuery = new SubQueryDef {QueryDef = subQuery, Attribute = attr}
            };
            //Condition.SubQueryDef = subQuery;
            //Condition.SubQueryAttribute = attribute;
            Condition.Condition = ConditionOperation.NotIn;
            return this;
        }

        public IQueryExpression NotIn(object[] values)
        {
            Condition.Right = new QueryConditionPartDef { Values = values }; //Values = values;
            Condition.Condition = ConditionOperation.NotIn;
            return this;
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
            var condition = CreateConditionDef(attribute, ExpressionOperation.And, ""); //new QueryConditionDef { AttributeName = attribute, Operation = ExpressionOperation.And };
            Condition.Conditions.Add(condition);
            Condition.Condition = ConditionOperation.Exp;
            return new ConditionExpressionBuilder(this, condition);
        }
    }
}
