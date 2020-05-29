using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.Cissa.Report.Defs
{
    public static class ReportAttributeColumnDefHelper
    {
        public static SqlQuerySummaryFunction ToSqlGrouping(this ReportAttributeColumnDef def)
        {
            switch (def.Grouping)
            {
                case ReportColumnGroupingType.Min:
                    return SqlQuerySummaryFunction.Min;
                case ReportColumnGroupingType.Max:
                    return SqlQuerySummaryFunction.Max;
                case ReportColumnGroupingType.Count:
                    return SqlQuerySummaryFunction.Count;
                case ReportColumnGroupingType.Sum:
                    return SqlQuerySummaryFunction.Sum;
                case ReportColumnGroupingType.Avg:
                    return SqlQuerySummaryFunction.Avg;
                case ReportColumnGroupingType.Group:
                case ReportColumnGroupingType.CrossGroup:
                    return SqlQuerySummaryFunction.Group;
            }
            return SqlQuerySummaryFunction.None;
        }
    }
}