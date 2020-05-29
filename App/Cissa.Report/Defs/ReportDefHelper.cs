using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace Intersoft.Cissa.Report.Defs
{
    public enum ReportConditionDefType
    {
        Param,
        Attribute
    }

    public static class ReportDefHelper
    {
        public static ReportDef Create(DocDef docDef)
        {
            if (docDef == null)
                throw new ApplicationException("Не могу создать отчет! Класс документа не указан!");

            var sourceDefId = Guid.NewGuid();
            var sourceDef = new ReportSourceDef {Id = sourceDefId, DocDef = docDef, Caption = docDef.Caption, Attributes = new List<ReportSourceSystemAttributeDef>()};
            sourceDef.Attributes.Add(new ReportSourceSystemAttributeDef
            {
                Id = Guid.NewGuid(),
                Ident = SystemIdent.Created,
                Caption = "Дата создания документа"
            });
            sourceDef.Attributes.Add(new ReportSourceSystemAttributeDef
            {
                Id = Guid.NewGuid(),
                Ident = SystemIdent.Modified,
                Caption = "Дата изменения документа"
            });
            sourceDef.Attributes.Add(new ReportSourceSystemAttributeDef
            {
                Id = Guid.NewGuid(),
                Ident = SystemIdent.State,
                Caption = "Статус документа"
            });
            sourceDef.Attributes.Add(new ReportSourceSystemAttributeDef
            {
                Id = Guid.NewGuid(),
                Ident = SystemIdent.StateDate,
                Caption = "Дата установки статуса документа"
            });

            return new ReportDef
            {
                Caption = docDef.Caption ?? docDef.Name ?? "Новый отчет",
                Columns = new List<ReportColumnDef>(),
                Conditions = new List<ReportConditionItemDef>(),
                Joins = new List<ReportSourceJoinDef>(),
                Sources = new List<ReportSourceDef>(new[] { sourceDef }),
                SourceId = sourceDefId
            };
        }

        public static void Check(this ReportDef def)
        {
            if (def == null)
                throw new ApplicationException("Отчет не определен!");
            if (def.Sources == null)
                def.Sources = new List<ReportSourceDef>();
            if (def.Columns == null)
                def.Columns = new List<ReportColumnDef>();
            if (def.Joins == null)
                def.Joins = new List<ReportSourceJoinDef>();
            if (def.Conditions == null)
                def.Conditions = new List<ReportConditionItemDef>();
        }

        public static ReportSourceDef GetSourceDef(this ReportDef def, Guid sourceId)
        {
            var sourceDef = def.Sources.FirstOrDefault(sd => sd.Id == sourceId);

            if (sourceDef == null)
                throw new ApplicationException("Источник данных не найден!");

            return sourceDef;
        }

        public static object CheckSourceAttribute(this ReportDef def, Guid sourceId, Guid attrDefId)
        {
            var sourceDef = GetSourceDef(def, sourceId);

            if (sourceDef.DocDef == null || sourceDef.DocDef.Attributes == null)
                throw new ApplicationException("Ошибка в источнике данных!");

            object attr = sourceDef.DocDef.Attributes.FirstOrDefault(a => a.Id == attrDefId);
            if (attr == null && sourceDef.Attributes != null)
                attr = sourceDef.Attributes.FirstOrDefault(a => a.Id == attrDefId);

            if (attr == null)
                throw new ApplicationException(String.Format("Атрибут \"{0}\" не найден в источнике данных!", attrDefId));

            return attr;
        }

        public static object GetAttribute(this ReportDef def, ReportAttributeDef attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute");

            return CheckSourceAttribute(def, attribute.SourceId, attribute.AttributeId);
        }

        public static ReportSourceJoinDef JoinDocDefSource(this ReportDef def, Guid masterSourceId, DocDef docDef,
            Guid attrDefId)
        {
            Check(def);
            var masterSourceDef = GetSourceDef(def, masterSourceId);
            var docDefCount = def.Sources.Count(s => s.DocDef.Id == docDef.Id);

            var jointSourceId = Guid.NewGuid();
            var jointSourceDef = new ReportSourceDef
            {
                Id = jointSourceId,
                DocDef = docDef,
                Caption = docDef.Caption + (docDefCount > 0 ? (docDefCount + 1).ToString() : String.Empty),
                Attributes = new List<ReportSourceSystemAttributeDef>()
            };
            jointSourceDef.Attributes.Add(new ReportSourceSystemAttributeDef
            {
                Id = Guid.NewGuid(),
                Ident = SystemIdent.Created,
                Caption = "Дата создания документа"
            });
            jointSourceDef.Attributes.Add(new ReportSourceSystemAttributeDef
            {
                Id = Guid.NewGuid(),
                Ident = SystemIdent.Modified,
                Caption = "Дата изменения документа"
            });
            jointSourceDef.Attributes.Add(new ReportSourceSystemAttributeDef
            {
                Id = Guid.NewGuid(),
                Ident = SystemIdent.State,
                Caption = "Статус документа"
            });
            jointSourceDef.Attributes.Add(new ReportSourceSystemAttributeDef
            {
                Id = Guid.NewGuid(),
                Ident = SystemIdent.StateDate,
                Caption = "Дата установки статуса документа"
            });

            def.Sources.Add(jointSourceDef);

            var jointAttrId = Guid.NewGuid();
            var attr = masterSourceDef.DocDef.Attributes.FirstOrDefault(a => a.Id == attrDefId);
            var jointAttrDef = attr != null
                ? new ReportAttributeDef { Id = jointAttrId, SourceId = masterSourceId, AttributeId = attrDefId }
                : new ReportAttributeDef { Id = jointAttrId, SourceId = jointSourceId, AttributeId = attrDefId };

            var joinDef =
                new ReportSourceJoinDef
                {
                    Id = Guid.NewGuid(),
                    JoinAttribute = jointAttrDef,
                    JoinType = SqlSourceJoinType.Inner,
                    MasterId = masterSourceId,
                    SourceId = jointSourceId
                };
            def.Joins.Add(joinDef);

            return joinDef;
        }

        public static void RemoveSource(this ReportDef def, Guid sourceId)
        {
            Check(def);
            if (def.SourceId == sourceId)
                throw new ApplicationException("Не могу удалить базовый источник из отчета!");

            def.Columns.RemoveAll(
                c =>
                    c is ReportAttributeColumnDef && ((ReportAttributeColumnDef) c).Attribute != null &&
                    ((ReportAttributeColumnDef) c).Attribute.SourceId == sourceId);

            def.Conditions.RemoveAll(c => ConditionHasSource(c, sourceId));

            def.Joins.RemoveAll(j => j.MasterId == sourceId || j.SourceId == sourceId);
            def.Sources.RemoveAll(s => s.Id == sourceId);
        }

        public static bool ConditionHasSource(ReportConditionItemDef c, Guid sourceId)
        {
            var expCondition = c as ReportExpConditionDef;
            if (expCondition != null)
            {
                if (expCondition.Conditions != null)
                    expCondition.Conditions.RemoveAll(i => ConditionHasSource(i, sourceId));
                else 
                    return true;

                return false;
            }


            return
                (c is ReportConditionDef && ((ReportConditionDef) c).LeftAttribute != null &&
                 ((ReportConditionDef) c).LeftAttribute.SourceId == sourceId) ||
                (c is ReportConditionDef && ((ReportConditionDef) c).RightPart is ReportConditionRightAttributeDef &&
                 ((ReportConditionRightAttributeDef) ((ReportConditionDef) c).RightPart).Attribute != null &&
                 ((ReportConditionRightAttributeDef) ((ReportConditionDef) c).RightPart).Attribute.SourceId ==
                 sourceId);
        }

        public static ReportAttributeColumnDef AddColumn(this ReportDef def, Guid sourceId, Guid attrDefId, string caption = null)
        {
            Check(def);
            var attr = CheckSourceAttribute(def, sourceId, attrDefId);

            var attrDef = new ReportAttributeDef { SourceId = sourceId, AttributeId = attrDefId };
            var s = !String.IsNullOrEmpty(caption)
                ? caption
                : attr is AttrDef
                    ? ((AttrDef) attr).Caption ?? ((AttrDef) attr).Name
                    : ((ReportSourceSystemAttributeDef) attr).Caption;
            var columnDef = new ReportAttributeColumnDef
            {
                Attribute = attrDef,
                Caption = s,
                Visible = true,
                Id = Guid.NewGuid()
            };
            def.Columns.Add(columnDef);
            return columnDef;
        }

        public static void RemoveColumn(this ReportDef def, Guid sourceId, Guid attrDefId)
        {
            Check(def);
            def.Columns.RemoveAll(
                c =>
                    c is ReportAttributeColumnDef && ((ReportAttributeColumnDef) c).Attribute != null &&
                    ((ReportAttributeColumnDef) c).Attribute.SourceId == sourceId &&
                    ((ReportAttributeColumnDef) c).Attribute.AttributeId == attrDefId);
        }

        public static void RemoveColumn(this ReportDef def, Guid columnId)
        {
            Check(def);
            def.Columns.RemoveAll(c => c.Id == columnId);
        }

        private static bool RemoveConditionIn(ICollection<ReportConditionItemDef> conditions, Guid id)
        {
            if (conditions != null)
                foreach (var condition in conditions)
                {
                    if (condition.Id == id)
                    {
                        conditions.Remove(condition);
                        return true;
                    }
                    var exp = condition as ReportExpConditionDef;
                    if (exp != null && exp.Conditions != null)
                        if (RemoveConditionIn(exp.Conditions, id)) return true;
                }
            return false;
        }

        public static void RemoveCondition(this ReportDef def, Guid conditionId)
        {
            Check(def);
            RemoveConditionIn(def.Conditions, conditionId);
        }

        public static ReportConditionDef AddParamCondition(this ReportDef def, Guid sourceId, Guid attrDefId, CompareOperation condition, object value, string paramCaption)
        {
            Check(def);
            GetSourceDef(def, sourceId);

            var conditionId = Guid.NewGuid();
            var conditionDef = new ReportConditionDef
            {
                Id = conditionId,
                Operation = ExpressionOperation.And,
                LeftAttribute = new ReportAttributeDef {SourceId = sourceId, AttributeId = attrDefId},
                Condition = condition,
                RightPart = new ReportConditionRightParamDef
                {
                    Caption = paramCaption,
                    Value = value
                }
            };
            def.Conditions.Add(conditionDef);
            return conditionDef;
        }

        public static ReportConditionDef AddAttributeCondition(this ReportDef def, Guid leftSourceId, Guid leftAttrId,
            CompareOperation condition, Guid rightSourceId, Guid rightAttrId)
        {
            Check(def);
            CheckSourceAttribute(def, leftSourceId, leftAttrId);
            CheckSourceAttribute(def, rightSourceId, rightAttrId);

            var conditionId = Guid.NewGuid();
            var conditionDef = new ReportConditionDef
            {
                Id = conditionId,
                Operation = ExpressionOperation.And,
                LeftAttribute = new ReportAttributeDef {SourceId = leftSourceId, AttributeId = leftAttrId},
                Condition = condition,
                RightPart = new ReportConditionRightAttributeDef
                {
                    Attribute = new ReportAttributeDef
                    {
                        SourceId = rightSourceId, 
                        AttributeId = rightAttrId
                    }
                }
            };
            def.Conditions.Add(conditionDef);
            return conditionDef;
        }

        public static ReportExpConditionDef AddExpConditionDef(this ReportDef def)
        {
            Check(def);
            var conditionId = Guid.NewGuid();
            var conditionDef = new ReportExpConditionDef
            {
                Id = conditionId,
                Operation = ExpressionOperation.And,
                Conditions = new List<ReportConditionItemDef>()
            };
            def.Conditions.Add(conditionDef);
            return conditionDef;
        }

        private static ReportConditionItemDef FindConditionById(ReportConditionItemDef condition, Guid conditionId)
        {
            if (condition.Id == conditionId) return condition;

            var exp = condition as ReportExpConditionDef;
            if (exp != null && exp.Conditions != null)
                return exp.Conditions.Select(c => FindConditionById(c, conditionId)).FirstOrDefault(c => c != null);

            return null;
        }

        public static ReportConditionItemDef FindConditionDef(this ReportDef def, Guid conditionId)
        {
            Check(def);
            return def.Conditions.Select(c => FindConditionById(c, conditionId)).FirstOrDefault(c => c != null);
        }

        public static ReportConditionItemDef GetConditionDef(this ReportDef def, Guid conditionId)
        {
            var condition = FindConditionDef(def, conditionId) as ReportConditionDef;

            if (condition == null)
                throw new ApplicationException("Условие не найдено!");
            return condition;
        }

        public static ReportConditionDef AddCondition(this ReportDef def, Guid leftSourceId, Guid leftAttrId, CompareOperation condition, ReportConditionDefType type = ReportConditionDefType.Param, string caption = null)
        {
            Check(def);
            var attrDef = CheckSourceAttribute(def, leftSourceId, leftAttrId);
            var s = !String.IsNullOrEmpty(caption)
                ? caption
                : attrDef is AttrDef
                    ? ((AttrDef) attrDef).Caption ?? ((AttrDef) attrDef).Name
                    : ((ReportSourceSystemAttributeDef) attrDef).Caption;

            var rightPart = (type == ReportConditionDefType.Param)
                ? (ReportConditionRightPartDef) new ReportConditionRightParamDef {Caption = s}
                : new ReportConditionRightAttributeDef {Attribute = new ReportAttributeDef()};

            var conditionDef = new ReportConditionDef
            {
                Id = Guid.NewGuid(),
                Operation = ExpressionOperation.And,
                LeftAttribute = new ReportAttributeDef
                {
                    SourceId = leftSourceId,
                    AttributeId = leftAttrId
                },
                Condition = condition,
                RightPart = rightPart
            };
            def.Conditions.Add(conditionDef);

            return conditionDef;
        }

        public static ReportConditionDef AssignConditionRightAttribute(this ReportDef def, Guid conditionId, Guid sourceId, Guid attrDefId)
        {
            Check(def);
            var condition = FindConditionDef(def, conditionId) as ReportConditionDef;

            if (condition == null)
                throw new ApplicationException("Условие не найдено!");
            CheckSourceAttribute(def, sourceId, attrDefId);

            condition.RightPart = new ReportConditionRightAttributeDef
            {
                Attribute = new ReportAttributeDef
                {
                    SourceId = sourceId,
                    AttributeId = attrDefId
                }
            };

            return condition;
        }

        public static ReportConditionDef AssignConditionRightParam(this ReportDef def, Guid conditionId, string caption, object value)
        {
            Check(def);
            var condition = FindConditionDef(def, conditionId) as ReportConditionDef;

            if (condition == null)
                throw new ApplicationException("Условие не найдено!");

            condition.RightPart = new ReportConditionRightParamDef
            {
                Caption = caption,
                Value = value
            };

            return condition;
        }

        public static ReportConditionDef AssignConditionRightVariable(this ReportDef def, Guid conditionId, string systemName)
        {
            Check(def);
            var condition = FindConditionDef(def, conditionId) as ReportConditionDef;

            if (condition == null)
                throw new ApplicationException("Условие не найдено!");

            condition.RightPart = new ReportConditionRightVariableDef
            {
                //Caption = caption,
                SystemValue = systemName
            };

            return condition;
        }

        public static bool HasGrouping(this ReportDef def)
        {
            Check(def);

            return def.Columns.OfType<ReportAttributeColumnDef>().Any(c => c.Grouping != ReportColumnGroupingType.None);
        }

        public static bool HasCrossGrouping(this ReportDef def)
        {
            Check(def);

            return def.Columns.OfType<ReportAttributeColumnDef>().Any(c => c.Grouping == ReportColumnGroupingType.CrossGroup);
        }
    }
}