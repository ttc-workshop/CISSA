using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    public class DocQuery : IDisposable
    {
        private IDataContext _dataContext;
        private bool _ownDataContext = false;

        public IDataContext DataContext 
        { 
            get 
            {
                if (_dataContext == null)
                {
                    _dataContext = new DataContext();
                    _ownDataContext = true;
                }
                return _dataContext;
            } 
            private set { _dataContext = value; }
        }
        public QueryDef Def { get; private set; }

        private readonly List<DocDef> _docDefs = new List<DocDef>();

        public DocQuery(QueryDef def, IDataContext dataContext)
        {
            Def = def;
            DataContext = dataContext;
        }

        public DocQuery(QueryDef def) : this(def, null) {}

        public DocQuery(QueryBuilder builder) : this(builder.Def, null) {}

        public DocQuery(IQueryExpression exp)
        {
            while (exp != null && !(exp is IQuery)) exp = exp.End();

            if (exp is IQuery) Def = ((IQuery) exp).GetDef();
            else
                throw new ApplicationException("Не могу создать запрос! Ошибка в выражении запроса");
        }

        protected IQueryable<Document> Build()
        {
            return Build(DataContext);
        }

        protected IQueryable<Document> Build(IDataContext context)
        {
            var defRepo = new DocDefRepository(context, Def.UserId);
            var docDef = GetDocDef();

/*
            if (Def.DocDefId == Guid.Empty)
            {
                docDef = defRepo.DocDefByName(Def.DocDefName);
                Def.DocDefId = docDef.Id;
            }
            else
                docDef = defRepo.DocDefById(Def.DocDefId);
*/

            var descIds = defRepo.GetDocDefDescendant(docDef.Id).ToList();
            var edc = context as IEntityDataContext;
            IQueryable<Document> query;

            var userRepo = new UserRepository(context);
            UserInfo userInfo = null;

            if (Def.UserId != Guid.Empty)
                userInfo = userRepo.GetUserInfo(Def.UserId);

            if (Def.UserId == Guid.Empty || userInfo == null || docDef.IsPublic)
            {
                query = from doc in edc.Documents
                        where
                            descIds.Contains(doc.Def_Id ?? Guid.Empty) &&
                            (doc.Deleted == null || doc.Deleted == false)
                        select doc;
            }
            else
            {
                if (userInfo.OrgUnitTypeId != null)
                {
                    //                    var userOrgs = em.Object_Defs.OfType<Org_Unit>().Where(ou => ou.Id == userInfo.OrgUnitTypeId)
                    //                                   .SelectMany(ou => ou.Object_Defs).Select(ou => ou.Id);
                    //                    from org in em.Object_Defs.OfType<Organization>().Include("Org_Units")
                    //                               where org.Org_Units.Object_Defs.Contains(userInfo.OrganizationTypeId ?? Guid.Empty)
                    //                               select org.Org_Units.Object_Defs.OfType<O>();

                    var organizations =
                        edc.Entities.Object_Defs.OfType<Organization>().Where(
                            o => o.Org_Units_1.Any(ou => ou.Id == userInfo.OrgUnitTypeId));
                    var orgUnits =
                        edc.Entities.Object_Defs.OfType<Org_Unit>().Where(
                            o => o.Org_Units_1.Any(ou => ou.Id == userInfo.OrgUnitTypeId));

                    query = from doc in edc.Documents.Include("Organizations").Include("Organizations.Org_Units")
                            where
                                descIds.Contains(doc.Def_Id ?? Guid.Empty) &&
                                (doc.Deleted == null || doc.Deleted == false) &&
                                (organizations.Contains(doc.Organization) ||
                                 (doc.Organization != null && doc.Organization.Org_Units != null &&
                                  orgUnits.Contains(doc.Organization.Org_Units)) ||
                                 doc.Organization_Id == userInfo.OrganizationId)
                            select doc;
                }
                else
                    query = from doc in edc.Documents.Include("Organizations").Include("Organizations.Org_Units")
                            where
                                descIds.Contains(doc.Def_Id ?? Guid.Empty) &&
                                (doc.Deleted == null || doc.Deleted == false) &&
                                doc.Organization_Id == userInfo.OrganizationId
                            select doc;
            }

            if (Def.ListAttrId != Guid.Empty)
            {
                var attr = GetDocListAttr();

                query = query.Intersect(
                    edc.Entities.DocumentList_Attributes.Include("Documents")
                        .Where(a => a.Def_Id == attr.Id && a.Document_Id == Def.DocumentId && a.Expired >= MaxDate)
                        .Select(a => a.Document1));
            }

            foreach (var condition in Def.WhereConditions)
            {
                var cq = BuildCondition(condition, docDef, context, Def.UserId);

                if (cq != null)
                    switch (condition.Operation)
                    {
                        case ExpressionOperation.And:
                            query = query.Intersect(cq);
                            break;
                        case ExpressionOperation.Or:
                            query = query.Union(cq);
                            break;
                        case ExpressionOperation.AndNot:
                            query = query.Except(cq);
                            break;
                    }
            }

//            return query.OrderByDescending(doc => doc.Created);
            if (Def.OrderAttributes != null && Def.OrderAttributes.Count > 0)
            {
                var order = Def.OrderAttributes[0];
                var attrRef = new QueryAttributeDefHelper(order.Attribute).GetAttributeRef();
                var attrDef = !String.IsNullOrEmpty(attrRef.AttributeName)
                                  ? FindAttrDef(attrRef.AttributeName)
                                  : FindAttrDef(attrRef.AttributeId);

                if (attrDef != null)
                {
                    switch (attrDef.Type.Id)
                    {
                        case (short)CissaDataType.Int:
                            query = query.OrderBy(doc => doc.Int_Attributes.Select(a => a.Value));
                            break;
                        case (short)CissaDataType.Text:
                            query = query.OrderBy(doc => doc.Text_Attributes.Select(a => a.Value));
                            break;
                        case (short)CissaDataType.Float:
                            query = query.OrderBy(doc => doc.Float_Attributes.Select(a => a.Value));
                            break;
                        case (short)CissaDataType.DateTime:
                            query = query.OrderBy(doc => doc.Date_Time_Attributes.Select(a => a.Value));
                            break;
                        default:
                            query = query.OrderByDescending(doc => doc.Created);
                            break;
                    }
                }
                else
                    query = query.OrderByDescending(doc => doc.Created);
            }
            else
            {
                query = query.OrderByDescending(doc => doc.Created);
            }
            return query;
        }


        private AttrDef FindAttrDef(Guid attrDefId)
        {
            foreach(var docDef in _docDefs)
            {
                var attrDef = docDef.Attributes.FirstOrDefault(a => a.Id == attrDefId);
                if (attrDef != null) return attrDef;
            }
            return null;
        }

        private AttrDef FindAttrDef(string attrDefName)
        {
            foreach (var docDef in _docDefs)
            {
                var attrDef = docDef.Attributes.FirstOrDefault(a => String.Equals(a.Name, attrDefName, StringComparison.OrdinalIgnoreCase));
                if (attrDef != null) return attrDef;
            }
            return null;
        }

        protected IQueryable<Document> BuildCondition(QueryConditionDef condition, DocDef docDef, IDataContext context, Guid userId)
        {
            if (_docDefs.FirstOrDefault(d => d.Id == docDef.Id) == null)
                _docDefs.Add(docDef);

            SystemIdent ident;
            return SystemIdentConverter.TryConvert(condition.AttributeName, out ident)
                       ? BuildSystemCondition(ident, condition, docDef, context, userId)
                       : BuildAttributeCondition(condition, docDef, context, userId);
/*
            if (String.Compare(condition.AttributeName, "&state", true) == 0)
                return BuildSystemCondition(SystemIdent.State, condition, docDef, context, userId);
            if (String.Compare(condition.AttributeName, "&Id", true) == 0)
                return BuildSystemCondition(SystemIdent.Id, condition, docDef, context, userId);
            if (String.Compare(condition.AttributeName, "&OrgId", true) == 0)
                return BuildSystemCondition(SystemIdent.OrgId, condition, docDef, context, userId);
            if (String.Compare(condition.AttributeName, "&Created", true) == 0)
                return BuildSystemCondition(SystemIdent.Created, condition, docDef, context, userId);
            if (String.Compare(condition.AttributeName, "&UserId", true) == 0)
                return BuildSystemCondition(SystemIdent.UserId, condition, docDef, context, userId);
            if (String.Compare(condition.AttributeName, "&UserName", true) == 0)
                return BuildSystemCondition(SystemIdent.UserName, condition, docDef, context, userId);
            if (String.Compare(condition.AttributeName, "&OrgCode", true) == 0)
                return BuildSystemCondition(SystemIdent.OrgCode, condition, docDef, context, userId);
            if (String.Compare(condition.AttributeName, "&OrgName", true) == 0)
                return BuildSystemCondition(SystemIdent.OrgName, condition, docDef, context, userId);
*/
        }

        protected static readonly DateTime MaxDate = new DateTime(9999, 12, 31);

        private static IEntityDataContext GetEntityDataContext(IDataContext context)
        {
            var edc = context as IEntityDataContext;
            if (edc == null)
                throw new ApplicationException("Cannot get Entity DataContext!");
            return edc;
        }

        protected IQueryable<Document> BuildAttributeCondition(QueryConditionDef condition, DocDef docDef, IDataContext context, Guid userId)
        {
            var edc = GetEntityDataContext(context);
            var source = BuildSource(docDef, context, userId);

            if (docDef.Attributes == null || docDef.Attributes.Count == 0)
            {
                var defRepo = new DocDefRepository(context, userId);

                docDef = defRepo.DocDefById(docDef.Id);
            }

            var attr = !String.IsNullOrEmpty(condition.AttributeName)
                ? docDef.Attributes.First(
                    a => String.Equals(a.Name, condition.AttributeName, StringComparison.OrdinalIgnoreCase))
                : docDef.Attributes.First(a => a.Id == condition.AttributeId);

            if (condition.Conditions.Count == 0)
            {
                if (attr.Type.Id == (short)CissaDataType.Text)
                {
                    var text = condition.Value != null ? condition.Value.ToString() : "";
                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.Text_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == text && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.Text_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != text && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.Contains:
                            return source.Intersect(
                                edc.Entities.Text_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value.Contains(text) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNull:
                            return source.Intersect(
                                edc.Entities.Text_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == null && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.In:
                            var docQuery = new DocQuery(condition.SubQueryDef, context);
                            var q = docQuery.All<string>(context, condition.SubQueryAttribute);
                            return source.Intersect(
                                edc.Entities.Text_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Text attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Int)
                {
                    var val = condition.Value != null ? Convert.ToInt32(condition.Value) : 0;
                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatEqual:
                            return source.Intersect(
                                edc.Entities.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value >= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatThen:
                            return source.Intersect(
                                edc.Entities.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value > val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessEqual:
                            return source.Intersect(
                                edc.Entities.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value <= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessThen:
                            return source.Intersect(
                                edc.Entities.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value < val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                edc.Entities.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.In:
                            var docQuery = new DocQuery(condition.SubQueryDef, context);
                            var q = docQuery.All<int>(context, condition.SubQueryAttribute);
                            return source.Intersect(
                                edc.Entities.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Int attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Float)
                {
                    var val = condition.Value != null ? Convert.ToDouble(condition.Value) : 0;
                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && Math.Abs(a.Value - val) > 0.0001 && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatEqual:
                            return source.Intersect(
                                edc.Entities.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value >= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatThen:
                            return source.Intersect(
                                edc.Entities.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value > val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessEqual:
                            return source.Intersect(
                                edc.Entities.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value <= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessThen:
                            return source.Intersect(
                                edc.Entities.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value < val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                edc.Entities.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.In:
                            var docQuery = new DocQuery(condition.SubQueryDef, context);
                            var q = docQuery.All<double>(context, condition.SubQueryAttribute);
                            return source.Intersect(
                                edc.Entities.Float_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Float attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Currency)
                {
                    var val = condition.Value != null ? Convert.ToDecimal(condition.Value) : 0;
                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatEqual:
                            return source.Intersect(
                                edc.Entities.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value >= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatThen:
                            return source.Intersect(
                                edc.Entities.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value > val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessEqual:
                            return source.Intersect(
                                edc.Entities.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value <= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessThen:
                            return source.Intersect(
                                edc.Entities.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value < val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                edc.Entities.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.In:
                            var docQuery = new DocQuery(condition.SubQueryDef, context);
                            var q = docQuery.All<decimal>(context, condition.SubQueryAttribute);
                            return source.Intersect(
                                edc.Entities.Currency_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Currency attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.DateTime)
                {
                    var val = condition.Value != null ? Convert.ToDateTime(condition.Value) : DateTime.MinValue;
                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatEqual:
                            return source.Intersect(
                                edc.Entities.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value >= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.GreatThen:
                            return source.Intersect(
                                edc.Entities.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value > val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessEqual:
                            return source.Intersect(
                                edc.Entities.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value <= val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.LessThen:
                            return source.Intersect(
                                edc.Entities.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value < val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                edc.Entities.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.In:
                            var docQuery = new DocQuery(condition.SubQueryDef, context);
                            var q = docQuery.All<DateTime>(context, condition.SubQueryAttribute);
                            return source.Intersect(
                                edc.Entities.Date_Time_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for DateTime attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Bool)
                {
                    var val = condition.Value != null ? Convert.ToBoolean(condition.Value) : false;
                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.Boolean_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.Boolean_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != val && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                edc.Entities.Boolean_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.In:
                            var docQuery = new DocQuery(condition.SubQueryDef, context);
                            var q = docQuery.All<bool>(context, condition.SubQueryAttribute);
                            return source.Intersect(
                                edc.Entities.Boolean_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Bool attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Enum)
                {
                    var val = condition.Value != null ? condition.Value.ToString() : "";
                    Guid enumId;
                    if (!Guid.TryParse(val, out enumId))
                    {
                        var enumRepo = new EnumRepository(context);

                        enumId = enumRepo.GetEnumValueId(attr.EnumDefType.Id, val);
                    }

                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.Enum_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == enumId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.Enum_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != enumId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                edc.Entities.Enum_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.In:
                            var docQuery = new DocQuery(condition.SubQueryDef, context);
                            var q = docQuery.All<Guid>(context, condition.SubQueryAttribute);
                            return source.Intersect(
                                edc.Entities.Enum_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Enum attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Doc)
                {
                    var val = condition.Value != null ? condition.Value.ToString() : "";
                    Guid docId;
                    var hasDocId = Guid.TryParse(val, out docId);

                    if (condition.Condition == ConditionOperation.Is)
                    {
                        var defRepo = new DocDefRepository(context, userId);
                        DocDef isDocDef = hasDocId ? defRepo.DocDefById(docId) : defRepo.DocDefByName(val);

                        var descendants = defRepo.GetDocDefDescendant(isDocDef.Id);

                        var docs =
                            edc.Entities.Documents.Where(
                                d =>
                                d.Def_Id != null && descendants.Contains((Guid) d.Def_Id) &&
                                (d.Deleted == null || d.Deleted == false)).Select(d => d.Id);

                        return source.Intersect(
                            edc.Entities.Document_Attributes.Include("Documents")
                                .Where(a => a.Def_Id == attr.Id && docs.Contains(a.Value) && a.Expired >= MaxDate)
                                .Select(a => a.Document));
                    }
                    if (condition.Condition == ConditionOperation.In)
                    {
                        var docQuery = new DocQuery(condition.SubQueryDef, context);

                        var q = !String.IsNullOrEmpty(condition.SubQueryAttribute)
                                    ? docQuery.All<Guid>(context, condition.SubQueryAttribute)
                                    : docQuery.All(context).Select(d => d.Id);

                        return source.Intersect(
                            edc.Entities.Document_Attributes.Include("Documents")
                                .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                .Select(a => a.Document));
                    }

                    if (!hasDocId) throw new ApplicationException("Invalid argument type for Document attribute");
                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.Document_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == docId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.Document_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != docId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                edc.Entities.Document_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Document attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.DocList)
                {
                    var val = condition.Value != null ? condition.Value.ToString() : "";
                    Guid docId;
                    var hasDocId = Guid.TryParse(val, out docId);

                    if (condition.Condition == ConditionOperation.Is)
                    {
                        var defRepo = new DocDefRepository(context, userId);
                        DocDef isDocDef = hasDocId ? defRepo.DocDefById(docId) : defRepo.DocDefByName(val);

                        var descendants = defRepo.GetDocDefDescendant(isDocDef.Id);

                        var docs =
                            edc.Documents.Where(
                                d =>
                                d.Def_Id != null && descendants.Contains((Guid) d.Def_Id) &&
                                (d.Deleted == null || d.Deleted == false)).Select(d => d.Id);

                        return source.Intersect(
                            edc.Entities.DocumentList_Attributes.Include("Documents")
                                .Where(a => a.Def_Id == attr.Id && docs.Contains(a.Value) && a.Expired >= MaxDate)
                                .Select(a => a.Document));
                    }
                    if (condition.Condition == ConditionOperation.In)
                    {
                        var docQuery = new DocQuery(condition.SubQueryDef, context);

                        var q = !String.IsNullOrEmpty(condition.SubQueryAttribute)
                                    ? docQuery.All<Guid>(context, condition.SubQueryAttribute)
                                    : docQuery.All(context).Select(d => d.Id);

                        return source.Intersect(
                            edc.Entities.DocumentList_Attributes.Include("Documents")
                                .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                .Select(a => a.Document));
                    }

                    if (!hasDocId) throw new ApplicationException("Invalid argument type for Document attribute");
                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.DocumentList_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == docId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.DocumentList_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != docId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                edc.Entities.DocumentList_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Document attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.Organization)
                {
                    var val = condition.Value != null ? condition.Value.ToString() : "";
                    Guid orgId;
                    if (!Guid.TryParse(val, out orgId))
                    {
                        /*using (*/
                        var orgRepo = new OrgRepository(context /*, userId*/);
                            orgId = orgRepo.TryGetOrgIdByCode(val) ?? orgRepo.GetOrgIdByName(val);
                    }

                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.Org_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == orgId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.Org_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != orgId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                edc.Entities.Org_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.In:
                            var docQuery = new DocQuery(condition.SubQueryDef, context);
                            var q = docQuery.All<Guid>(context, condition.SubQueryAttribute);
                            return source.Intersect(
                                edc.Entities.Org_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Organization attribute");
                    }
                }
                if (attr.Type.Id == (short)CissaDataType.DocumentState)
                {
                    var val = condition.Value != null ? condition.Value.ToString() : "";
                    Guid docStateId;
                    if (!Guid.TryParse(val, out docStateId))
                    {
                        /*using (*/
                        var docStateRepo = new DocStateRepository(context);
                        docStateId = docStateRepo.GetDocStateTypeId(val);
                    }

                    switch (condition.Condition)
                    {
                        case ConditionOperation.Equal:
                            return source.Intersect(
                                edc.Entities.Doc_State_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value == docStateId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.NotEqual:
                            return source.Intersect(
                                edc.Entities.Doc_State_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Value != docStateId && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.IsNotNull:
                            return source.Intersect(
                                edc.Entities.Doc_State_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        case ConditionOperation.In:
                            var docQuery = new DocQuery(condition.SubQueryDef, context);
                            var q = docQuery.All<Guid>(context, condition.SubQueryAttribute);
                            return source.Intersect(
                                edc.Entities.Doc_State_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == attr.Id && q.Contains(a.Value) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                        default:
                            throw new ApplicationException("Cannot apply operator for Document state attribute");
                    }
                }
                throw new ApplicationException("Не могу сформировать запрос. Данный тип не поддерживается!");
            }

            if (attr.Type.Id != (short)CissaDataType.Doc && attr.Type.Id != (short)CissaDataType.DocList)
                throw new ApplicationException("Ошибка в запросе! Атрибут не является документным.");

            if (condition.Conditions.Count > 0)
            {
                var subQuery = BuildConditions(attr, condition.Conditions, context, userId);

                if (subQuery != null)
                    switch (condition.Conditions[0].Operation)
                    {
                        case ExpressionOperation.And:
                            source = source.Intersect(subQuery);
                            break;
                        case ExpressionOperation.Or:
                            source = source.Union(subQuery);
                            break;
                        case ExpressionOperation.AndNot:
                            source = source.Except(subQuery);
                            break;
                    }
            }
            return source;
        }

        private IQueryable<Document> BuildConditions(AttrDef attr, IReadOnlyList<QueryConditionDef> conditions, IDataContext context, Guid userId)
        {
            if (conditions.Count <= 0) return null;

            if (attr.DocDefType == null)
                throw new ApplicationException(
                    String.Format(
                        "Не могу сформировать запрос! Атрибут \"{0}\" не ссылается на класс документа",
                        attr.Name));

            var firstSub = conditions[0];
            IQueryable<Document> query = null;
            var em = GetEntityDataContext(context).Entities;
            
            foreach (var sub in conditions)
            {
                var sq = BuildCondition(sub, attr.DocDefType, context, userId);

                var subQuery = attr.Type.Id == (short) CissaDataType.DocList
                                   ? em.DocumentList_Attributes.Include("Documents")
                                         .Where(
                                             a =>
                                             a.Def_Id == attr.Id && sq.Select(d => d.Id).Contains(a.Value) &&
                                             a.Expired >= MaxDate)
                                         .Select(a => a.Document)
                                   : em.Document_Attributes.Include("Documents")
                                         .Where(
                                             a =>
                                             a.Def_Id == attr.Id && sq.Select(d => d.Id).Contains(a.Value) &&
                                             a.Expired >= MaxDate)
                                         .Select(a => a.Document);

                if (sub == firstSub) query = subQuery;
                else
                    switch (sub.Operation)
                    {
                        case ExpressionOperation.And:
                            query = query.Intersect(subQuery);
                            break;
                        case ExpressionOperation.Or:
                            query = query.Union(subQuery);
                            break;
                        case ExpressionOperation.AndNot:
                            query = query.Except(subQuery);
                            break;
                    }
            }

            return query;
        }

        protected IQueryable<Document> BuildSystemCondition(SystemIdent ident, QueryConditionDef condition, DocDef docDef, IDataContext context, Guid userId)
        {
            var em = GetEntityDataContext(context).Entities;

            var source = BuildSource(docDef, context, userId);

            var text = condition.Value != null ? condition.Value.ToString() : "";
            switch (condition.Condition)
            {
                case ConditionOperation.Equal:
                    switch (ident)
                    {
                        case SystemIdent.Id:
                            var docId = Guid.Parse(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Id == docId && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.State:
                            Guid stateId;
                            if (!Guid.TryParse(text, out stateId))
                            {
                                /*using (*/
                                var dsRepo = new DocStateRepository(context);
                                stateId = dsRepo.GetDocStateTypeId(text);
                            }
                            return source.Intersect(
                                    em.Document_States.Where(s => s.State_Type_Id == stateId && s.Expired >= MaxDate).Select(s => s.Document));
                        case SystemIdent.Created:
                            var val = Convert.ToDateTime(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Created == val && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.Modified:
                            var modified = Convert.ToDateTime(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Last_Modified == modified && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.OrgId:
                            var orgId = Guid.Parse(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Organization_Id == orgId && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.OrgName:
                            /*using (*/
                            var orgRepo = new OrgRepository(context /*, userId*/);
                            {
                                var orgId2 = orgRepo.GetOrgIdByName(text);
                                return source.Intersect(
                                    em.Documents.Where(
                                        d => d.Organization_Id == orgId2 && (d.Deleted == null || d.Deleted == false)));
                            }
                        case SystemIdent.OrgCode:
                            /*using (*/
                            var orgRepo2 = new OrgRepository(context /*, userId*/);
                            {
                                var orgId3 = orgRepo2.GetOrgIdByCode(text);
                                return source.Intersect(
                                    em.Documents.Where(
                                        d => d.Organization_Id == orgId3 && (d.Deleted == null || d.Deleted == false)));
                            }
                        case SystemIdent.UserId:
                            var userRefId = Guid.Parse(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.UserId == userRefId && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.UserName:
                            /*using (*/
                            var userRepo = new UserRepository(context);/*)*/
                            {
                                var userId2 = userRepo.GetUserId(text);
                                return source.Intersect(
                                    em.Documents.Where(
                                        d => d.UserId == userId2 && (d.Deleted == null || d.Deleted == false)));
                            }
                    }
                    break;
                case ConditionOperation.NotEqual:
                    switch (ident)
                    {
                        case SystemIdent.Id:
                            var docId = Guid.Parse(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Id != docId && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.State:
                            Guid stateId;
                            if (!Guid.TryParse(text, out stateId))
                            {
                                /*using (*/
                                var dsRepo = new DocStateRepository(context);
                                stateId = dsRepo.GetDocStateTypeId(text);
                            }
                            return source.Intersect(
                                    em.Document_States.Where(s => s.State_Type_Id != stateId && s.Expired >= MaxDate).Select(s => s.Document));
                        case SystemIdent.Created:
                            var val = Convert.ToDateTime(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Created != val && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.Modified:
                            var modified = Convert.ToDateTime(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Last_Modified != modified && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.OrgId:
                            var orgId = Guid.Parse(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Organization_Id != orgId && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.OrgName:
                            /*using (*/
                            var orgRepo = new OrgRepository(context /*, userId*/);
                            {
                                var orgId2 = orgRepo.GetOrgIdByName(text);
                                return source.Intersect(
                                    em.Documents.Where(
                                        d => d.Organization_Id != orgId2 && (d.Deleted == null || d.Deleted == false)));
                            }
                        case SystemIdent.OrgCode:
                            /*using (*/
                            var orgRepo2 = new OrgRepository(context /*, userId*/);
                            {
                                var orgId3 = orgRepo2.GetOrgIdByCode(text);
                                return source.Intersect(
                                    em.Documents.Where(
                                        d => d.Organization_Id != orgId3 && (d.Deleted == null || d.Deleted == false)));
                            }
                        case SystemIdent.UserId:
                            var userRefId = Guid.Parse(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.UserId != userRefId && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.UserName:
                            var userRepo = new UserRepository(context);
                            var userId2 = userRepo.GetUserId(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.UserId != userId2 && (d.Deleted == null || d.Deleted == false)));
                    }
                    break;
                case ConditionOperation.In:
                    var query = new DocQuery(condition.SubQueryDef, context);
                    var q = !String.IsNullOrEmpty(condition.SubQueryAttribute)
                                ? query.All<Guid>(context, condition.SubQueryAttribute)
                                : query.All(context).Select(d => d.Id);

                    switch (ident)
                    {
                        case SystemIdent.Id:
                            return source.Intersect(
                                em.Documents.Where(d => q.Contains(d.Id) && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.State:
                            return source.Intersect(
                                em.Document_States.Where(
                                    s => q.Contains(s.State_Type_Id) && s.Expired >= MaxDate).Select(
                                        s => s.Document));
//                        case SystemIdent.Created:
//                            return source.Intersect(
//                                em.Documents.Where(
//                                    d => q.Contains((object) d.Created) && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.OrgId:
                            return source.Intersect(
                                em.Documents.Where(
                                    d =>
                                    q.Contains(d.Organization_Id ?? Guid.Empty) && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.UserId:
                            return source.Intersect(
                                em.Documents.Where(
                                    d => q.Contains(d.UserId) && (d.Deleted == null || d.Deleted == false)));
                    }
                    break;
            }
            return source;
        }

        public SqlQuery BuildSql(IDataContext context)
        {
            //SqlQuery query;

            /*var em = (cissaEntities)context.Entities;

            var defRepo = new DocDefRepository(context, Def.UserId);*/
//            var docDef = GetDocDef();

//            var descIds = defRepo.GetDocDefDescendant(docDef.Id).ToList();

            var query = SqlQueryBuilder.Build(context, Def);

            /*var userRepo = new UserRepository(context);
            UserInfo userInfo = null;

            if (Def.UserId != Guid.Empty)
                userInfo = userRepo.GetUserInfo(Def.UserId);*/

/*
            if (Def.UserId != Guid.Empty && userInfo != null && !docDef.IsPublic)
            {
                if (userInfo.OrgUnitTypeId != null)
                {
                    var organizations =
                        em.Object_Defs.OfType<Organization>().Where(
                            o => o.Org_Units_1.Any(ou => ou.Id == userInfo.OrgUnitTypeId));
                    var orgUnits =
                        em.Object_Defs.OfType<Org_Unit>().Where(
                            o => o.Org_Units_1.Any(ou => ou.Id == userInfo.OrgUnitTypeId));

                    query = from doc in em.Documents.Include("Organizations").Include("Organizations.Org_Units")
                            where
                                descIds.Contains(doc.Def_Id ?? Guid.Empty) &&
                                (doc.Deleted == null || doc.Deleted == false) &&
                                (organizations.Contains(doc.Organization) ||
                                 (doc.Organization != null && doc.Organization.Org_Units != null &&
                                  orgUnits.Contains(doc.Organization.Org_Units)) ||
                                 doc.Organization_Id == userInfo.OrganizationId)
                            select doc;
                }
                else
                    query = from doc in em.Documents.Include("Organizations").Include("Organizations.Org_Units")
                            where
                                descIds.Contains(doc.Def_Id ?? Guid.Empty) &&
                                (doc.Deleted == null || doc.Deleted == false) &&
                                doc.Organization_Id == userInfo.OrganizationId
                            select doc;
            }
*/
            
            return query;
        }

        public bool Any()
        {
            return Build(DataContext).Any();
        }

        public IQueryable<Document> All(IDataContext context)
        {
            return Build(context);
        }

        public IEnumerable<Guid> All()
        {
            return All(DataContext).Select(d => d.Id);
        }

        public IEnumerable<object> All(string attribute)
        {
            return All<object>(DataContext, attribute);
        }

        public IQueryable<T> All<T>(IDataContext context, string attribute)
        {
            var en = GetEntityDataContext(context).Entities;

            var def = GetDocDef();

            var attr =
                def.Attributes.First(a => String.Equals(a.Name, attribute, StringComparison.OrdinalIgnoreCase));

            var query = Build(context);

            switch(attr.Type.Id)
            {
                case (short) CissaDataType.Text: 
                    return en.Text_Attributes.Where(
                        a => a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
                case (short) CissaDataType.Int:
                    return en.Int_Attributes.Include("Documents").Where(
                        a => a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
                case (short) CissaDataType.Float:
                    return en.Float_Attributes.Include("Documents").Where(
                        a=> a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
                case (short)CissaDataType.Currency:
                    return en.Currency_Attributes.Include("Documents").Where(
                        a => a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
                case (short)CissaDataType.DateTime:
                    return en.Date_Time_Attributes.Include("Documents").Where(
                        a => a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
                case (short)CissaDataType.Enum:
                    return en.Enum_Attributes.Include("Documents").Where(
                        a => a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
                case (short)CissaDataType.Doc:
                    return en.Document_Attributes.Include("Documents").Where(
                        a => a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
                case (short)CissaDataType.DocList:
                    return en.DocumentList_Attributes.Include("Documents").Where(
                        a => a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
                case (short)CissaDataType.Bool:
                    return en.Boolean_Attributes.Include("Documents").Where(
                        a => a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
                case (short)CissaDataType.DocumentState:
                    return en.Doc_State_Attributes.Include("Documents").Where(
                        a => a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
                case (short)CissaDataType.Organization:
                    return en.Org_Attributes.Include("Documents").Where(
                        a => a.Def_Id == attr.Id && query.Contains(a.Document) && a.Expired >= MaxDate).Select(
                            a => a.Value).Cast<T>();
            }
            throw new ApplicationException("Атрибут заданного типа не поддерживается");
        }

        public IEnumerable<Guid> First(int count)
        {
            return Build(DataContext).Select(d => d.Id).Take(count);
        }

        public IEnumerable<object> First(int count, string attribute)
        {
            return All(attribute).Take(count);
        }

        public Guid? FirstOrDefault()
        {
            var id = Build().Select(d => d.Id).FirstOrDefault();
            if (id == Guid.Empty) return null;
            return id;
        }

        public object FirstOrDefault(string attribute)
        {
            return All(attribute).FirstOrDefault();
        }

        public IEnumerable<Guid> Last(int count)
        {
            var q = Build().Select(d => d.Id);
            var i = q.Count();
            return q.Skip(i - count).Take(count);
        }

        public IEnumerable<object> Last(int count, string attribute)
        {
            var q = All(attribute);
            var i = q.Count();
            return q.Skip(i - count).Take(count);
        }

        public Guid? LastOrDefault()
        {
            return Build().Select(d => d.Id).LastOrDefault();
        }

        public object LastOrDefault(string attribute)
        {
            return All(attribute).LastOrDefault();
        }

        public IEnumerable<Guid> Take(int no, int count)
        {
            return Build().Select(d => d.Id).Skip(no).Take(count);
        }

        public IEnumerable<object> Take(int no, int count, string attribute)
        {
            return All(attribute).Skip(no).Take(count);
        }

        public int Count()
        {
            return Build().Count();
        }

        public double? Sum(string attrName)
        {
            var docDef = GetDocDef();

            var attr = docDef.Attributes.First(a => String.Equals(a.Name, attrName, StringComparison.OrdinalIgnoreCase));

            var em = GetEntityDataContext(DataContext).Entities;
            double? result = null;
            var q = Build(DataContext).Select(d => d.Id);

            switch (attr.Type.Id)
            {
                case (short) CissaDataType.Int:
                    var ri = em.Int_Attributes.Include("Documents")
                        .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate &&
                                    q.Contains(a.Document_Id))
                        .Select(a => a.Value);
                    if (ri.Any()) result = ri.Sum();
                    break;
                case (short) CissaDataType.Float:
                    var rf = em.Float_Attributes.Include("Documents")
                        .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate &&
                                    q.Contains(a.Document_Id))
                        .Select(a => a.Value);
                    if (rf.Any()) result = rf.Sum();
                    break;
                case (short) CissaDataType.Currency:
                    var rc = em.Currency_Attributes.Include("Documents")
                        .Where(a => a.Def_Id == attr.Id && a.Expired >= MaxDate &&
                                    q.Contains(a.Document_Id))
                        .Select(a => a.Value);
                    if (rc.Any()) result = (double) rc.Sum();
                    break;
                default:
                    throw new ApplicationException(
                        "Немогу вычислить сумму. Заданный тип атрибута не поддерживается!");
            }
            return result;
        }

        public DocDef GetDocDef()
        {
            var defRepo = new DocDefRepository(DataContext, Def.UserId);
            DocDef docDef;

            if (Def.Source != null && Def.DocDefId == Guid.Empty)
            {
                docDef = defRepo.DocDefByName(Def.DocDefName);
                Def.Source.DocDefId = docDef.Id;
            }
            else
                docDef = defRepo.DocDefById(Def.DocDefId);
            /*
            if (Def.ListAttrId != Guid.Empty && Def.OwnerDocDefId != Guid.Empty)
            {
                var ownerDocDef = defRepo.DocDefById(Def.OwnerDocDefId);
                var attr = ownerDocDef.Attributes.First(a => a.Id == Def.ListAttrId);

                return defRepo.DocDefById(attr.DocDefType.Id);
            }

            if (!String.IsNullOrEmpty(Def.ListAttrName) && Def.OwnerDocDefId != Guid.Empty)
            {
                var ownerDocDef = defRepo.DocDefById(Def.OwnerDocDefId);
                var attr = ownerDocDef.Attributes.First(a => String.Compare(a.Name, Def.ListAttrName, true) == 0);
                Def.ListAttrId = attr.Id;
                return defRepo.DocDefById(attr.DocDefType.Id);
            }
            */
            return docDef;
        }

        protected AttrDef GetDocListAttr()
        {
            var defRepo = new DocDefRepository(DataContext, Def.UserId);
            DocDef docDef;

            if (Def.OwnerDocDefId != Guid.Empty)
            {
                docDef = defRepo.DocDefById(Def.OwnerDocDefId);

                if (Def.ListAttrId != Guid.Empty)
                    return docDef.Attributes.First(a => a.Id == Def.ListAttrId);

                if (!String.IsNullOrEmpty(Def.ListAttrName))
                {
                    var attr = docDef.Attributes.First(a => String.Equals(a.Name, Def.ListAttrName, StringComparison.OrdinalIgnoreCase));
                    Def.ListAttrId = attr.Id;
                    return attr;
                }
            }
            throw new ApplicationException("Не могу вернуть атрибут! Атрибут не указан!");
        }

        protected static IQueryable<Document> BuildSource(DocDef docDef, IDataContext context, Guid userId)
        {
            var edc = GetEntityDataContext(context);
            var defRepo = new DocDefRepository(context, userId);

            var descIds = defRepo.GetDocDefDescendant(docDef.Id).ToList();

            return from doc in edc.Documents
                   where descIds.Contains(doc.Def_Id ?? Guid.Empty) && (doc.Deleted == null || doc.Deleted == false)
                   select doc;
        }

        public IEnumerable<QueryCondition> GetConditions()
        {
            foreach (var condition in Def.WhereConditions)
            {
                var qc = new QueryCondition(condition, GetDocDef());
                yield return qc;
            }
        }

        public void Dispose()
        {
            try
            {
                if (_ownDataContext && _dataContext != null)
                {
                    _dataContext.Dispose();
                    _dataContext = null;
                }
            }
            catch (Exception e)
            {
                Logger.OutputLog(e, "DocQuery.Dispose");
                throw;
            }
        }

        /*~DocQuery()
        {
            if (_ownDataContext && _dataContext != null)
                try
                {
                    _dataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "DocQuery.Finalize");
                    throw;
                }
        }*/
    }
}
