using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using Intersoft.Cissa.Report.Defs;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Interfaces;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.Cissa.Report.Builders
{
    public class SqlQueryFromReportDefBuilder : IBuilder<ReportDef, SqlQuery>
    {
        public IAppServiceProvider Provider { get; private set; }
        public Guid UserId { get; set; }

        public SqlQueryFromReportDefBuilder(IAppServiceProvider provider)
        {
            Provider = provider;
        }
        public SqlQueryFromReportDefBuilder(IAppServiceProvider provider, Guid userId) : this(provider)
        {
            UserId = userId;
        }

        public SqlQuery Build(ReportDef report)
        {
            if (report.Sources == null)
                throw new PropertyConstraintException("ReportDef Sources not defined!");

            var sourceDef = report.Sources.First(s => s.Id == report.SourceId);

            var docDef = sourceDef.DocDef;

            // TODO: Доделать код для глобального провайдера
            var query = /*_globalProvider ? new SqlQuery(Provider,) :*/ 
                UserId != Guid.Empty ? new SqlQuery(Provider, docDef, UserId) : new SqlQuery(Provider, docDef);

            var sourceMapper = new Dictionary<Guid, SqlQuerySource>();
            try
            {
                sourceMapper.Add(report.SourceId, query.Source);

                foreach (var source in report.Sources)
                {
                    if (source.Id != report.SourceId)
                    {
                        sourceMapper.Add(source.Id, BuildSource(query, source));
                    }
                }
                foreach (var join in report.Joins)
                {
                    BuildJoin(query, @join, sourceMapper);
                }
                var grouping =
                    report.Columns.OfType<ReportAttributeColumnDef>()
                        .Any(c => c.Grouping != ReportColumnGroupingType.None);

                foreach (var column in report.Columns)
                {
                    var attrColumn = column as ReportAttributeColumnDef;
                    if (attrColumn != null)
                    {
                        var source = sourceMapper[attrColumn.Attribute.SourceId];
                        var reportSource = report.GetSourceDef(attrColumn.Attribute.SourceId);
                        var reportSourceAttr = reportSource.Attributes != null
                            ? reportSource.Attributes.FirstOrDefault(a => a.Id == attrColumn.Attribute.AttributeId)
                            : null;
                        if (!grouping)
                        {
                            if (reportSourceAttr == null)
                                query.AddAttribute(source, attrColumn.Attribute.AttributeId);
                            else
                                query.AddAttribute(source, reportSourceAttr.Ident);
                        }
                        else
                        {
                            if (attrColumn.Grouping != ReportColumnGroupingType.None &&
                                attrColumn.Grouping != ReportColumnGroupingType.Group &&
                                attrColumn.Grouping != ReportColumnGroupingType.CrossGroup)
                            {

                                if (reportSourceAttr == null)
                                    query.AddAttribute(source, attrColumn.Attribute.AttributeId, attrColumn.ToSqlGrouping());
                                else
                                    query.AddAttribute(source, reportSourceAttr.Ident, attrColumn.ToSqlGrouping());
                            }
                            else
                            {
                                if (reportSourceAttr == null)
                                    query.AddAttribute(source, attrColumn.Attribute.AttributeId);
                                else
                                    query.AddAttribute(source, reportSourceAttr.Ident);
                            }
                        }
                    }
                } 
                foreach (var condition in report.Conditions)
                {
                    BuildCondition(query, report, condition, null, sourceMapper, false);
                }
                if (grouping)
                    foreach (
                        var group in
                            report.Columns.OfType<ReportAttributeColumnDef>()
                                .Where(
                                    c =>
                                        c.Grouping == ReportColumnGroupingType.Group ||
                                        c.Grouping == ReportColumnGroupingType.None ||
                                        c.Grouping == ReportColumnGroupingType.CrossGroup))
                    {
                        var source = sourceMapper[group.Attribute.SourceId];
                        var reportSource = report.GetSourceDef(group.Attribute.SourceId);
                        var reportSourceAttr = reportSource.Attributes != null
                            ? reportSource.Attributes.FirstOrDefault(a => a.Id == group.Attribute.AttributeId)
                            : null;
                        if (reportSourceAttr == null)
                            query.AddGroupAttribute(source, group.Attribute.AttributeId);
                        else
                            query.AddGroupAttribute(source, reportSourceAttr.Ident);
                    }
                /*foreach (var condition in def.HavingConditions)
                {
                    BuildCondition(query, condition, docDef, null, true);
                }*/
                foreach (var order in report.Columns.OfType<ReportAttributeColumnDef>().Where(c => c.SortType != SortType.None))
                {
                    var source = sourceMapper[order.Attribute.SourceId];

                    var reportSource = report.GetSourceDef(order.Attribute.SourceId);
                    var reportSourceAttr = reportSource.Attributes != null
                        ? reportSource.Attributes.FirstOrDefault(a => a.Id == order.Attribute.AttributeId)
                        : null;
                    if (grouping &&
                        (order.Grouping != ReportColumnGroupingType.None ||
                         order.Grouping != ReportColumnGroupingType.Group ||
                         order.Grouping != ReportColumnGroupingType.CrossGroup))
                    {
                        var exp = "";
                        switch (order.Grouping)
                        {
                            case ReportColumnGroupingType.Sum:
                                exp = "sum({0})";
                                break;
                            case ReportColumnGroupingType.Min:
                                exp = "min({0})";
                                break;
                            case ReportColumnGroupingType.Max:
                                exp = "max({0})";
                                break;
                            case ReportColumnGroupingType.Count:
                                exp = "count({0})";
                                break;
                            case ReportColumnGroupingType.Avg:
                                exp = "avg({0})";
                                break;
                        }
                        if (reportSourceAttr == null)
                            query.AddOrderAttribute(source, order.Attribute.AttributeId, exp,
                                order.SortType == SortType.Ascending);
                        else
                            query.AddOrderAttribute(source, reportSourceAttr.Ident, exp, order.SortType == SortType.Ascending);
                    }
                    else
                    {
                        if (reportSourceAttr == null)
                            query.AddOrderAttribute(source, order.Attribute.AttributeId,
                                order.SortType == SortType.Ascending);
                        else
                            query.AddOrderAttribute(source, reportSourceAttr.Ident, order.SortType == SortType.Ascending);
                    }
                }
                return query;
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        private static SqlQuerySource BuildSource(SqlQuery query, ReportSourceDef source)
        {
            if (source.DocDef == null)
                throw new ApplicationException("Немогу создать запрос. ReportSourceDef.DocDef не определен!");

            var alias = query.GetSourceAlias(source.DocDef.Name);
            var result = new SqlQueryDocSource(query.Provider, source.DocDef, alias);
            query.Sources.Add(result);
            return result;
        }

        private static void BuildJoin(SqlQuery query, ReportSourceJoinDef join, IDictionary<Guid, SqlQuerySource> mapper)
        {
            var masterSource = mapper[join.MasterId];
            var jointSource = mapper[join.SourceId];

            var joinDef = new SqlQueryJoin(masterSource, jointSource, join.JoinType, join.JoinAttribute.AttributeId);
            query.SourceJoins.Add(joinDef);
        }

        private static void BuildCondition(SqlQuery query, ReportDef report, ReportConditionItemDef item, SqlQueryCondition parentCondition, IDictionary<Guid, SqlQuerySource> mapper, bool isHaving)
        {
            var condition = item as ReportConditionDef;
            if (condition != null)
            {
                var source = mapper[condition.LeftAttribute.SourceId];
                var reportSource = report.GetSourceDef(condition.LeftAttribute.SourceId);
                var reportSourceAttr = reportSource.Attributes != null
                    ? reportSource.Attributes.FirstOrDefault(a => a.Id == condition.LeftAttribute.AttributeId)
                    : null;
                var attr = reportSourceAttr == null ? source.GetAttribute(condition.LeftAttribute.AttributeId) : source.GetAttribute(reportSourceAttr.Ident);

                var attrRef = new SqlQuerySourceAttributeRef(source, attr);
                var leftPart = new SqlQueryConditionPart();
                leftPart.Attributes.Add(attrRef);

                var rightPart = BuildConditionPart(query, report, condition, mapper);

                if (parentCondition != null)
                    parentCondition.Conditions.Add(new SqlQueryCondition(item.Operation, leftPart,
                        CompareOperationConverter.CompareToCondition(condition.Condition), rightPart));
                else if (isHaving)
                    query.HavingConditions.Add(new SqlQueryCondition(item.Operation, leftPart,
                        CompareOperationConverter.CompareToCondition(condition.Condition),
                        rightPart));
                else
                    query.Conditions.Add(new SqlQueryCondition(item.Operation, leftPart,
                        CompareOperationConverter.CompareToCondition(condition.Condition),
                        rightPart));
            }
            else
            {
                var expCondition = item as ReportExpConditionDef;
                if (expCondition != null && expCondition.Conditions != null)
                {
                    var exp = query.AddExpCondition(item.Operation, parentCondition);

                    foreach (var child in expCondition.Conditions)
                    {
                        BuildCondition(query, report, child, exp, mapper, isHaving);
                    }
                }
            }
        }

        private static SqlQueryConditionPart BuildConditionPart(SqlQuery query, ReportDef report, ReportConditionDef condition, IDictionary<Guid, SqlQuerySource> mapper)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (condition == null) throw new ArgumentNullException("condition");

            var single = condition.RightPart as ReportConditionRightAttributeDef;
            if (single != null)
            {
                var source = mapper[single.Attribute.SourceId];
                var reportSource = report.GetSourceDef(single.Attribute.SourceId);
                var reportSourceAttr = reportSource.Attributes != null
                    ? reportSource.Attributes.FirstOrDefault(a => a.Id == single.Attribute.AttributeId)
                    : null;
                var attr = reportSourceAttr == null ? source.GetAttribute(single.Attribute.AttributeId) : source.GetAttribute(reportSourceAttr.Ident);

                var attrRef = new SqlQuerySourceAttributeRef(source, attr);
                var rightPart = new SqlQueryConditionPart();
                rightPart.Attributes.Add(attrRef);

                return rightPart;
            }
            var exp = condition.RightPart as ReportConditionRightParamDef;
            if (exp != null)
            {
                var sqPart = new SqlQueryConditionPart
                {
                    Params = new[] {new QueryConditionValueDef {Value = exp.Value, Name = exp.Caption}}
                };

                return sqPart;
            }
            throw new ApplicationException("Не могу создать правую часть SqlQuery условия из ReportDef условия");
        }
    }
}
