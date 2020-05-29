using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.UserApp.ServiceReference;
//using Intersoft.CISSA.Report.Context;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class ConfigTableReport : BaseContextState
    {
        public TableReportContext Context { get; private set; }

        public Guid SourceId { get; set; }

        public ConfigTableReport(IContext context, Guid sourceDocDefId)
            : base(context)
        {
            InitializeContext(context, sourceDocDefId);
        }

        public ConfigTableReport(IContext context, ContextState previous, byte[] buf, string fileName) : base(context, previous)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.DeserializeTableReport(buf);
            SourceId = Context.Def.SourceId;
        }

        public ConfigTableReport(IContext context, ContextState previous, Guid sourceDocDefId)
            : base(context, previous)
        {
            InitializeContext(context, sourceDocDefId);
        }

        protected void InitializeContext(IContext context, Guid sourceDocDefId)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.CreateTableReport(sourceDocDefId);
            SourceId = Context.Def.SourceId;
        }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Report", "Config");
        }

        public void JoinSource(IContext context, Guid masterId, Guid joinId, Guid attrId)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.JoinTableReportSource(Context, masterId, joinId, attrId);
        }

        public void RemoveSource(IContext context, Guid sourceId)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.RemoveTableReportSource(Context, sourceId);
        }

        public void AddColumn(IContext context, Guid sourceId, Guid attrId)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.AddTableReportColumn(Context, sourceId, attrId);
        }

        public void AddColumns(IContext context, Guid sourceId, List<Guid> attrIds)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.AddTableReportColumns(Context, sourceId, attrIds);
        }

        public void RemoveColumn(IContext context, Guid columnId)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.RemoveTableReportColumn(Context, columnId);
        }

        public void MoveColumn(IContext context, Guid id, bool up)
        {
            var columnDef = Context.Def.Columns.FirstOrDefault(cd => cd.Id == id);

            if (columnDef != null)
            {
                var i = Context.Def.Columns.IndexOf(columnDef);
                if (up && i == 0) return;
                if (up) i--;
                else i++;
                if (!up && i == Context.Def.Columns.Count) return;

                Context.Def.Columns.Remove(columnDef);
                Context.Def.Columns.Insert(i, columnDef);
            }
        }

        public void AddRootExpCondition(IContext context)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.AddTableReportExpCondition(Context);
        }

        public void AddParamCondition(IContext context, Guid sourceId, Guid attrId)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.AddTableReportCondition(Context, sourceId, attrId, ReportConditionDefType.Param);
        }

        public void AddAttrCondition(IContext context, Guid sourceId, Guid attrId)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.AddTableReportCondition(Context, sourceId, attrId, ReportConditionDefType.Attribute);
        }

        public void RemoveCondition(IContext context, Guid conditionId)
        {
            var rm = context.GetReportProxy();
            Context = rm.Proxy.RemoveTableReportCondition(Context, conditionId);
        }

        public byte[] Execute(IContext context)
        {
            var rm = context.GetReportProxy();
            var file = rm.Proxy.ExecuteTableReport(Context);
            return file;
        }

        public byte[] Serialize(IContext context)
        {
            var rm = context.GetReportProxy();
            var data = rm.Proxy.SerializeTableReport(Context);
            return data;
        }

        public void SetColumnSortType(Guid id, string sortType)
        {
            if (Context == null || Context.Def == null || Context.Def.Columns == null) return;

            var columnDef = Context.Def.Columns.OfType<ReportAttributeColumnDef>().FirstOrDefault(c => c.Id == id);
            if (columnDef == null) return;

            if (SortType.None.ToString().Equals(sortType, StringComparison.OrdinalIgnoreCase))
                columnDef.SortType = SortType.None;
            else if (SortType.Ascending.ToString().Equals(sortType, StringComparison.OrdinalIgnoreCase))
                columnDef.SortType = SortType.Ascending;
            else if (SortType.Descending.ToString().Equals(sortType, StringComparison.OrdinalIgnoreCase))
                columnDef.SortType = SortType.Descending;
        }

        public void SetColumnGrouping(Guid id, string value)
        {
            if (Context == null || Context.Def == null || Context.Def.Columns == null) return;

            var columnDef = Context.Def.Columns.OfType<ReportAttributeColumnDef>().FirstOrDefault(c => c.Id == id);
            if (columnDef == null) return;

            if (ReportColumnGroupingType.None.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                columnDef.Grouping = ReportColumnGroupingType.None;
            else if (ReportColumnGroupingType.Group.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                columnDef.Grouping = ReportColumnGroupingType.Group;
            else if (ReportColumnGroupingType.CrossGroup.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                columnDef.Grouping = ReportColumnGroupingType.CrossGroup;
            else if (ReportColumnGroupingType.Count.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                columnDef.Grouping = ReportColumnGroupingType.Count;
            else if (ReportColumnGroupingType.Sum.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                columnDef.Grouping = ReportColumnGroupingType.Sum;
            else if (ReportColumnGroupingType.Avg.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                columnDef.Grouping = ReportColumnGroupingType.Avg;
            else if (ReportColumnGroupingType.Max.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                columnDef.Grouping = ReportColumnGroupingType.Max;
            else if (ReportColumnGroupingType.Min.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                columnDef.Grouping = ReportColumnGroupingType.Min;
        }

        public void SetConditionExpression(Guid id, string value)
        {
            ExpressionOperation exp;
            if (ExpressionOperation.TryParse(value, out exp))
            {
                if (Context == null || Context.Def == null || Context.Def.Conditions == null) return;

                var condition = FindConditionDef(Context.Def, id);
                if (condition == null) return;

                condition.Operation = exp;
            }
        }

        public void SetConditionOperation(Guid id, string value)
        {
            CompareOperation exp;
            if (CompareOperation.TryParse(value, out exp))
            {
                if (Context == null || Context.Def == null || Context.Def.Conditions == null) return;

                var condition = FindConditionDef(Context.Def, id) as ReportConditionDef;
                if (condition == null) return;

                condition.Condition = exp;
            }
        }

        private static ReportConditionItemDef FindConditionById(ReportConditionItemDef condition, Guid conditionId)
        {
            if (condition.Id == conditionId) return condition;

            var exp = condition as ReportExpConditionDef;
            if (exp != null && exp.Conditions != null)
                return exp.Conditions.Select(c => FindConditionById(c, conditionId)).FirstOrDefault(c => c != null);

            return null;
        }

        public static ReportConditionItemDef FindConditionDef(ReportDef def, Guid conditionId)
        {
            return def.Conditions.Select(c => FindConditionById(c, conditionId)).FirstOrDefault(c => c != null);
        }

        public bool SetColumnCaption(Guid id, string value)
        {
            if (Context != null && Context.Def != null && Context.Def.Columns != null)
            {
                var column = Context.Def.Columns.FirstOrDefault(c => c.Id == id);

                if (column != null)
                {
                    column.Caption = value;
                    return true;
                }
            }
            return false;
        }

        public bool SetConditionParamCaptionValue(Guid id, string caption, string value)
        {
            if (Context != null && Context.Def != null && Context.Def.Conditions != null)
            {
                var condition = Context.Def.Conditions.OfType<ReportConditionDef>().FirstOrDefault(c => c.Id == id && c.RightPart is ReportConditionRightParamDef);

                if (condition != null)
                {
                    var rightPart = (ReportConditionRightParamDef)condition.RightPart;
                    rightPart.Caption = caption;
                    rightPart.Value = value;
                    return true;
                }
            }
            return false;
        }

        public List<ComboBoxUpdateData> SetConditionRightSource(Guid id, string value)
        {
            var result = new List<ComboBoxUpdateData>();

            if (Context != null && Context.Def != null && Context.Def.Conditions != null)
            {
                var condition = Context.Def.Conditions.OfType<ReportConditionDef>().FirstOrDefault(c => c.Id == id && c.RightPart is ReportConditionRightAttributeDef);

                if (condition != null)
                {
                    var rightPart = (ReportConditionRightAttributeDef)condition.RightPart;
                    Guid sourceId;
                    if (Guid.TryParse(value, out sourceId))
                    {
                        var sourceDef = Context.Def.Sources.FirstOrDefault(s => s.Id == sourceId);
                        if (rightPart.Attribute != null)
                        {
                            rightPart.Attribute.SourceId = sourceId;
                            if (sourceDef != null && sourceDef.DocDef != null && sourceDef.DocDef.Attributes != null)
                            {
                                if (sourceDef.DocDef.Attributes.All(a => a.Id != rightPart.Attribute.AttributeId))
                                    rightPart.Attribute.AttributeId = Guid.Empty;
                            }
                        }
                        else
                        {
                            rightPart.Attribute = new ReportAttributeDef {AttributeId = Guid.Empty, SourceId = sourceId};
                        }

                        var data = new ComboBoxUpdateData("ra-" + id, rightPart.Attribute.AttributeId.ToString());
                        result.Add(data);
                        if (sourceDef != null && sourceDef.DocDef != null && sourceDef.DocDef.Attributes != null)
                            data.items =
                                sourceDef.DocDef.Attributes.Where(a => a.Type.Id != (short) CissaDataType.Doc &&
                                                                       a.Type.Id != (short) CissaDataType.DocList)
                                    .Select(a => new ComboBoxItem(a.Id.ToString(), a.Caption)).ToArray();

                        return result;
                    }
                }
            }
            return result;
        }

        public List<ComboBoxUpdateData> SetConditionRightAttribute(Guid id, string value)
        {
            var result = new List<ComboBoxUpdateData>();

            if (Context != null && Context.Def != null && Context.Def.Conditions != null)
            {
                var condition = Context.Def.Conditions.OfType<ReportConditionDef>().FirstOrDefault(c => c.Id == id && c.RightPart is ReportConditionRightAttributeDef);

                if (condition != null)
                {
                    var rightPart = (ReportConditionRightAttributeDef) condition.RightPart;
                    Guid attrId;
                    if (Guid.TryParse(value, out attrId))
                    {
                        if (rightPart.Attribute != null)
                        {
                            var sourceDef = Context.Def.Sources.FirstOrDefault(s => s.Id == rightPart.Attribute.SourceId);
                            if (sourceDef != null && sourceDef.DocDef != null && sourceDef.DocDef.Attributes != null && sourceDef.DocDef.Attributes.Any(a => a.Id == attrId))
                                    rightPart.Attribute.AttributeId = attrId;

                            var data = new ComboBoxUpdateData("ra-" + id, rightPart.Attribute.AttributeId.ToString());
                            result.Add(data);
                        }

                    }
                }
            }
            return result;
        }

        public void SetConditionRightParamValue(Guid id, string value)
        {
            if (Context != null && Context.Def != null && Context.Def.Conditions != null)
            {
                var condition = Context.Def.Conditions.OfType<ReportConditionDef>().FirstOrDefault(c => c.Id == id && c.RightPart is ReportConditionRightParamDef);

                if (condition != null)
                {
                    var rightPart = (ReportConditionRightParamDef) condition.RightPart;
                    rightPart.Value = value;
                }
            }
        }
    }
}