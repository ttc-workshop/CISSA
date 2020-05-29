using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    [DataContract]
    public class QueryAttributeCondition: QueryCondition
    {
        [DataMember]
        public string AttributeName { get; private set; }

        [DataMember]
        public List<QueryCondition> Conditions { get; private set; }

        private readonly static DateTime MaxDate = new DateTime(9999, 12, 31);

        public QueryAttributeCondition() {}

        public QueryAttributeCondition(string attributeName, ExpressionOperation operation) :
            base(operation)
        {
            AttributeName = attributeName;
            Conditions = new List<QueryCondition>();
        }

        public override void Add(QueryCondition condition)
        {
            Conditions.Add(condition);
        }

        public override IQueryable<Document> Build(DocDef docDef, ObjectContext context, Guid userId)
        {
            var em = (cissaEntities)context;

            var source = BuildSource(docDef, context, userId);

            if (docDef.Attributes == null)
            {
                var defRepo = new DocDefRepository(userId);

                docDef = defRepo.DocDefById(docDef.Id);
            }

            var attr = docDef.Attributes.First(a => String.Compare(a.Name, AttributeName, true) == 0);

            if (Conditions.Count == 0)
            {
                if (attr.Type.Id == (short) CissaDataType.Text)
                {
                    var text = Value != null ? Value.ToString() : "";
                    switch (Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                em.Text_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == text && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                em.Text_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != text && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.Contains:
                            return source.Intersect(
                                em.Text_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value.Contains(text) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNull:
                            return source.Intersect(
                                em.Text_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == null && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Text attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Int)
                {
                    var val = Value != null ? Convert.ToInt32(Value) : 0;
                    switch (Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                em.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                em.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatEqual:
                            return source.Intersect(
                                em.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value >= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatThen:
                            return source.Intersect(
                                em.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value > val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessEqual:
                            return source.Intersect(
                                em.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value <= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessThen:
                            return source.Intersect(
                                em.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value < val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                em.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Int attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Float)
                {
                    var val = Value != null ? Convert.ToDouble(Value) : 0;
                    switch (Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                em.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                em.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatEqual:
                            return source.Intersect(
                                em.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value >= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatThen:
                            return source.Intersect(
                                em.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value > val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessEqual:
                            return source.Intersect(
                                em.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value <= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessThen:
                            return source.Intersect(
                                em.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value < val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                em.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Float attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Currency)
                {
                    var val = Value != null ? Convert.ToDecimal(Value) : 0;
                    switch (Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                em.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                em.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatEqual:
                            return source.Intersect(
                                em.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value >= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatThen:
                            return source.Intersect(
                                em.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value > val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessEqual:
                            return source.Intersect(
                                em.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value <= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessThen:
                            return source.Intersect(
                                em.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value < val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                em.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Currency attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.DateTime)
                {
                    var val = Value != null ? Convert.ToDateTime(Value) : DateTime.MinValue;
                    switch (Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                em.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                em.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatEqual:
                            return source.Intersect(
                                em.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value >= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatThen:
                            return source.Intersect(
                                em.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value > val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessEqual:
                            return source.Intersect(
                                em.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value <= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessThen:
                            return source.Intersect(
                                em.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value < val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                em.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for DateTime attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Bool)
                {
                    var val = Value != null ? Convert.ToBoolean(Value) : false;
                    switch (Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                em.Boolean_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                em.Boolean_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                em.Boolean_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Bool attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Enum)
                {
                    var val = Value != null ? Value.ToString() : "";
                    Guid enumId;
                    if (!Guid.TryParse(val, out enumId))
                    {
                        var enumRepo = new EnumRepository();

                        enumId = enumRepo.GetEnumValueId(attr.EnumDefType.Id, val);
                    }

                    switch (Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                em.Enum_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == enumId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                em.Enum_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != enumId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                em.Enum_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Enum attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Doc)
                {
                    var val = Value != null ? Value.ToString() : "";
                    var docId = Guid.Empty;
                    var hasDocId = Guid.TryParse(val, out docId);

                    if (Condition == ConditionOperation.Is)
                    {
                        DocDef isDocDef = null;
                        var defRepo = new DocDefRepository();
                        isDocDef = hasDocId ? defRepo.DocDefById(docId) : defRepo.DocDefByName(val);

                        var descendants = defRepo.GetDocDefDescendant(isDocDef.Id);

                        var docs =
                            em.Documents.Where(
                                d =>
                                d.Def_Id != null && descendants.Contains(d.Def_Id ?? Guid.Empty) &&
                                (d.Deleted == null || d.Deleted == false)).Select(d => d.Id);

                        return source.Intersect(
                            em.Document_Attributes.Include("Documents")
                                .Where(a => a.Def_Id == attr.Id && docs.Contains(a.Value) && a.Expired >= MaxDate)
                                .Select(a => a.Document));
                    }

                    if (!hasDocId) throw new ApplicationException("Invalid argument type for Document attribute");
                    switch (Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                em.Document_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == docId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                em.Document_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != docId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                em.Document_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Document attribute");
                    }
                }
                throw new ApplicationException("Не могу сформировать запрос. Данный тип не поддерживается!");
            }

            if (attr.Type.Id != (short) CissaDataType.Doc && attr.Type.Id != (short) CissaDataType.DocList)
                throw new ApplicationException("Ошибка в запросе! Атрибут не является документным.");

            foreach(var condition in Conditions)
            {
                var sq = condition.Build(attr.DocDefType, context, userId);

                var sub = attr.Type.Id == (short) CissaDataType.DocList
                              ? em.DocumentList_Attributes.Include("Documents")
                                    .Where(a => sq.Select(d => d.Id).Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document)
                              : em.Document_Attributes.Include("Documents")
                                    .Where(a => sq.Select(d => d.Id).Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document);

                source = source.Intersect(sub);
            }
            return source;
        }
    }
}
