using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Defs;
using Intersoft.Cissa.Report.Utils;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Interfaces;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.Cissa.Report.Builders
{
    public class XlsDefFromReportDefBuilder : IBuilder<ReportDef, XlsDef>
    {
        public IAppServiceProvider Provider { get; private set; }

        public XlsDefFromReportDefBuilder(IAppServiceProvider provider)
        {
            Provider = provider;
        }

        public XlsDef Build(ReportDef report)
        {
            var sqlQueryBuilder = Provider.Get<IBuilder<ReportDef, SqlQuery>>();

            var query = sqlQueryBuilder.Build(report);
            try
            {
                var readerFactory = Provider.Get<ISqlQueryReaderFactory>();
                var reader = readerFactory.Create(query);
                try
                {
                    reader.Open();
                    // Logger.OutputLog(@"c:\distr\cissa\XlsDefFromReportDefSql.log", reader.GetSql());
                    if (report.HasCrossGrouping())
                    {
                        var table = CreateCrossDataTable(report, reader);

                        //table.Fill(reader);

                        var crossBuilder = new XlsCrossDataTableBuilder(report, table);
                        return crossBuilder.Build();
                    }

                    var builder = new XlsReportDefBuilder(Provider, report, reader);
                    return builder.Build();
                }
                catch
                {
                    reader.Dispose();
                    throw;
                }
            }
            catch
            {
                query.Dispose();
                throw;
            }
        }

        private CrossDataTable CreateCrossDataTable(ReportDef report, SqlQueryReader reader)
        {
            var table = new CrossDataTable();
            var sourceMap = MapSources(report, reader);
            var columnMaps = new Dictionary<CrossDataColumn, SqlQueryDataSetField>();
            var dataSet = new SqlQueryDataSet(Provider, reader);

            if (report.Columns != null)
                foreach (var column in report.Columns.OfType<ReportAttributeColumnDef>())
                {
                    AddColumn(table, report, reader, column, sourceMap, dataSet, columnMaps);
                }

            table.Fill(dataSet, columnMaps);

            return table;
        }

        private void AddColumn(CrossDataTable table, ReportDef report, SqlQueryReader reader, ReportAttributeColumnDef column, Dictionary<Guid, SqlQuerySource> sourceMap,
            SqlQueryDataSet dataSet, Dictionary<CrossDataColumn, SqlQueryDataSetField> columnMaps) {

            SqlQuerySource querySource;
            var reportSource = report.GetSourceDef(column.Attribute.SourceId);
            var reportSourceAttr = reportSource.Attributes != null
                ? reportSource.Attributes.FirstOrDefault(a => a.Id == column.Attribute.AttributeId)
                : null;

            var hasQuerySource = sourceMap.TryGetValue(column.Attribute.SourceId, out querySource);

            var attr = reportSourceAttr == null
                ? !hasQuerySource
                    ? reader.Query.FindAttribute(column.Attribute.AttributeId)
                    : reader.Query.FindAttribute(querySource, column.Attribute.AttributeId)
                : !hasQuerySource
                    ? reader.Query.FindAttribute(reportSourceAttr.Ident)
                    : reader.Query.FindAttribute(querySource, reportSourceAttr.Ident);

            if (attr != null)
            {
                var i = reader.Query.Attributes.IndexOf(attr);

                if (column.Grouping == ReportColumnGroupingType.None ||
                    column.Grouping == ReportColumnGroupingType.Group)
                {
                    var crossColumn = new CrossDataKeyColumn(i, column.Caption);
                    table.AddColumn(crossColumn);
                    columnMaps.Add(crossColumn,
                        new SqlQueryDataSetField(dataSet, attr,
                            column.Grouping == ReportColumnGroupingType.None
                                ? SqlQuerySummaryFunction.None
                                : SqlQuerySummaryFunction.Group));
                }
                else if (column.Grouping == ReportColumnGroupingType.CrossGroup)
                {
                    var crossColumn = new CrossDataGroupColumn(i, column.Caption);
                    table.AddColumn(crossColumn);
                    columnMaps.Add(crossColumn, new SqlQueryDataSetField(dataSet, attr));
                }
                else
                {
                    var crossColumn = new CrossDataFuncColumn(i, column.Caption, column.ToSqlGrouping());
                    table.AddColumn(crossColumn);
                    columnMaps.Add(crossColumn, new SqlQueryDataSetField(dataSet, attr, column.ToSqlGrouping()));
                }
            }

        }
        private Dictionary<Guid, SqlQuerySource> MapSources(ReportDef report, SqlQueryReader reader)
        {
            var result = new Dictionary<Guid, SqlQuerySource>();

            foreach (var sourceDef in report.Sources.Where(s => s.DocDef != null))
            {
                if (reader.Query.Source.IsDocDef(sourceDef.Id) && !result.ContainsValue(reader.Query.Source))
                {
                    result.Add(sourceDef.Id, reader.Query.Source);
                }
                else
                {
                    var querySource =
                        reader.Query.Sources.FirstOrDefault(
                            s => s.IsDocDef(sourceDef.Id) && !result.ContainsValue(s));
                    if (querySource != null)
                        result.Add(sourceDef.Id, querySource);
                }
            }
            return result;
        }
    }
}