using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Misc
{
    public class ComboBoxEnumProvider : IComboBoxEnumProvider
    {
        private IAppServiceProvider Provider { get; set; }
        private IDataContext DataContext { get; set; }

        private Guid UserId { get; set; }

        private readonly IOrgRepository _orgRepo;
        private readonly IDocDefRepository _docDefRepo;
        private readonly IUserRepository _userRepo;
        private readonly IEnumRepository _enumRepo;

        private readonly ISqlQueryBuilderFactory _sqlQueryBuilderFactory;
        private readonly ISqlQueryReaderFactory _sqlQueryReaderFactory;

        // TODO: Удалить конструктор
        public ComboBoxEnumProvider(IDataContext context, Guid userId)
        {
            DataContext = context;
            UserId = userId;

            var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
            var userData = new UserDataProvider(userId, "");
            Provider = providerFactory.Create(context, userData);

            _orgRepo = new OrgRepository(context);
            _docDefRepo = new DocDefRepository(context, UserId);
            _userRepo = new UserRepository(DataContext);
            _enumRepo = new EnumRepository(Provider, context);
        }

        public ComboBoxEnumProvider(IAppServiceProvider provider, IDataContext dataContext)
        {
            Provider = provider;
            DataContext = dataContext; 
            UserId = provider.GetCurrentUserId();

            _orgRepo = Provider.Get<IOrgRepository>();
            _docDefRepo = Provider.Get<IDocDefRepository>();
            _userRepo = Provider.Get<IUserRepository>();
            _enumRepo = Provider.Get<IEnumRepository>();

            _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
            _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
        }

        public List<EnumValue> GetEnumDocumentValues(BizComboBox combo, DocDef detailDocDef)
        {
            return GetEnumDocumentValues(combo, detailDocDef, UserId);
        }

        public List<EnumValue> GetEnumDocumentValues(BizComboBox combo, AttrDef attrDef)
        {
            return GetEnumDocumentValues(combo, attrDef.DocDefType, UserId);
        }

        public List<EnumValue> GetEnumDocumentValues(AttrDef attrDef, params string[] attrNames)
        {
            return GetEnumDocumentValues(attrDef.DocDefType, attrNames);
        }

        public List<EnumValue> GetEnumDocumentValues(BizComboBox combo, DocDef detailDocDef, Guid? userId)
        {
            var list = new List<EnumValue>();

            if (detailDocDef == null) return list;
            //var defRepo = new DocDefRepository(context);
            var docDef = _docDefRepo.DocDefById(detailDocDef.Id);

            AttrDef detailAttrDef;
            if (combo.DetailAttributeId != null)
                detailAttrDef = docDef.Attributes.FirstOrDefault(ad => ad.Id == combo.DetailAttributeId);
            else
            {
                detailAttrDef =
                    docDef.Attributes.FirstOrDefault(
                        ad => String.Equals(ad.Name, combo.DetailAttributeName, StringComparison.OrdinalIgnoreCase));
                if (detailAttrDef == null)
                    detailAttrDef =
                        docDef.Attributes.FirstOrDefault(
                            ad => ad.Type.Id == (int)CissaDataType.Text);
            }

            if (detailAttrDef == null) return list;

            var sqb = _sqlQueryBuilderFactory.Create();
            // using (var query = userId != null ? new SqlQuery(context, docDef, (Guid)userId) : new SqlQuery(context, docDef))
            // using (var query = sqb.Build(docDef.Id, null, null))
            using (var query = sqb.Build(combo, docDef.Id))
            {
                query.AddAttribute("&Id");
                query.AddAttribute(detailAttrDef.Id);
                query.AddOrderAttribute(detailAttrDef.Id);
                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(context, query))
                {
                    while (reader.Read())
                    {
                        var detail = !reader.IsDbNull(1) ? reader.GetString(1) : String.Empty;

                        list.Add(new EnumValue
                        {
                            Id = reader.GetGuid(0),
                            Value = detail,
                            DefaultValue = detail
                        });
                    }
                }
            }

            return list;
        }

        private /*static*/ List<EnumValue> GetEnumDocumentValues(DocDef detailDocDef, params string[] attrNames)
        {
            var list = new List<EnumValue>();

            if (detailDocDef == null) return list;
            //var defRepo = new DocDefRepository(context);
            var docDef = _docDefRepo.DocDefById(detailDocDef.Id);
            AttrDef detailAttrDef = null;

            foreach (var attrName in attrNames)
            {
                detailAttrDef =
                    docDef.Attributes.FirstOrDefault(
                        ad => String.Equals(ad.Name, attrName, StringComparison.OrdinalIgnoreCase));

                if (detailAttrDef != null) break;
            }
            if (detailAttrDef == null)
                detailAttrDef = docDef.Attributes.FirstOrDefault(ad => ad.Type.Id == (int) CissaDataType.Text);

            if (detailAttrDef == null) return list;
            var sqb = _sqlQueryBuilderFactory.Create();

            //using (var query = userId != null ? new SqlQuery(context, docDef, (Guid)userId) : new SqlQuery(context, docDef))
            using(var query = sqb.Build(docDef.Id, null, null))
            {
                query.AddAttribute("&Id");
                query.AddAttribute(detailAttrDef.Id);
                query.AddOrderAttribute(detailAttrDef.Id);
                using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(context, query))
                {
                    while (reader.Read())
                    {
                        var detail = !reader.IsDbNull(1) ? reader.GetString(1) : String.Empty;

                        list.Add(new EnumValue
                        {
                            Id = reader.GetGuid(0),
                            Value = detail,
                            DefaultValue = detail
                        });
                    }
                }
            }

            return list;
        }

        public IList<EnumValue> GetFormComboBoxValues(BizForm form, BizComboBox comboBox)
        {
            /*var masters = new List<BizControl>();
            if (comboBox.Masters != null && comboBox.Masters.Count > 0)
            {
                var finder = new ControlFinder(form);
                foreach (var masterId in comboBox.Masters)
                {
                    var master = finder.Find(masterId);
                    if (master != null)
                        masters.Add(master);
                }
            }*/

            return GetFormComboBoxValues(form, comboBox, GetComboBoxAttrDef(form, comboBox));
        }

        public IList<EnumValue> GetFormComboBoxValues(BizComboBox comboBox, AttrDef attrDef)
        {
            return GetFormComboBoxValues(null, comboBox, attrDef);
        }

        public IList<EnumValue> GetFormComboBoxValues(BizForm form, BizComboBox comboBox, AttrDef attrDef)
        {
            var list = new List<EnumValue>();
            if (attrDef == null)
            {
                switch (comboBox.Ident)
                {
                    case SystemIdent.OrgId:
                    case SystemIdent.OrgCode:
                    case SystemIdent.OrgName:
                        return GetEnumOrganizationValues(comboBox.DetailAttributeId);
                    case SystemIdent.UserId:
                    case SystemIdent.UserName:
                        return GetEnumUserValues();
                }
                return list;
            }

            if (attrDef.Type.Id == (short)CissaDataType.Doc && attrDef.DocDefType != null)
            {
                var detailDocDef = _docDefRepo.DocDefById(attrDef.DocDefType.Id);

                AttrDef detailAttrDef;
                if (comboBox.DetailAttributeId != null)
                    detailAttrDef = detailDocDef.Attributes.FirstOrDefault(ad => ad.Id == comboBox.DetailAttributeId);
                else
                {
                    detailAttrDef =
                        detailDocDef.Attributes.FirstOrDefault(
                            ad =>
                                String.Equals(ad.Name, comboBox.DetailAttributeName, StringComparison.OrdinalIgnoreCase))
                        ??
                        detailDocDef.Attributes.FirstOrDefault(ad => ad.Type.Id == (int)CissaDataType.Text);
                }
                if (detailAttrDef == null) return list;

                var sqlQueryBuilder = _sqlQueryBuilderFactory.Create();
                using (var query = sqlQueryBuilder.Build(comboBox, detailDocDef.Id))
                {
                    query.AddAttribute("&Id");
                    query.AddAttribute(detailAttrDef.Id);
                    query.AddOrderAttribute(detailAttrDef.Id);

                    if (form != null)
                    {
                        var helper = new FormHelper(Provider, form);
                        helper.SetQueryParams(query);
                    }
                    using (var reader = _sqlQueryReaderFactory.Create(query)) //new SqlQueryReader(context, query))
                    {
                        while (reader.Read())
                        {
                            var detail = !reader.IsDbNull(1) ? reader.GetString(1) : String.Empty;

                            list.Add(new EnumValue
                            {
                                Id = reader.GetGuid(0),
                                Value = detail,
                                DefaultValue = detail
                            });
                        }
                    }
                }
            }
            else if (attrDef.Type.Id == (short)CissaDataType.Enum && attrDef.EnumDefType != null)
                return new List<EnumValue>(_enumRepo.GetEnumItems(attrDef.EnumDefType.Id));
            else if (attrDef.Type.Id == (short)CissaDataType.Organization)
                return GetEnumOrganizationValues(attrDef.OrgTypeId);
            else if (attrDef.Type.Id == (short)CissaDataType.User)
                return GetEnumUserValues();

            return list;
        }

        private AttrDef GetComboBoxAttrDef(BizForm form, BizComboBox comboBox)
        {
            var finder = new FormHelper(Provider, form);
            var docDef = finder.FindAttributeDocDef(comboBox.AttributeDefId);
            if (docDef != null)
                return docDef.Attributes.FirstOrDefault(a => a.Id == comboBox.AttributeDefId);

            return null;
        }

        internal static string GetFirstNotEmpty(params string[] strings)
        {
            return strings.FirstOrDefault(i => !String.IsNullOrWhiteSpace(i));
        }

        public List<EnumValue> GetEnumUserValues()
        {
            return GetEnumUserValues(UserId);
        }

        private List<EnumValue> GetEnumUserValues(Guid? userId)
        {
            var users = _userRepo.GetUserAccessUsers(userId);

            return users.Select(_userRepo.GetUserInfo).Select(info =>
                new EnumValue
                {
                    Id = info.Id,
                    Value =
                        GetFirstNotEmpty(info.LastName + " " + info.FirstName + " " + info.MiddleName, info.UserName) +
                        ", " + info.OrganizationName,
                    DefaultValue =
                        GetFirstNotEmpty(info.LastName + " " + info.FirstName + " " + info.MiddleName, info.UserName) +
                        ", " + info.OrganizationName
                }).OrderBy(i => i.Value).ToList();
        }

        public List<EnumValue> GetEnumOrganizationValues( /*IDataContext context,*/ Guid? orgTypeId)
        {
            var orgs = _userRepo.GetUserAccessOrgs(UserId);

            return orgs.Select(_orgRepo.Get).Select(org =>
                new EnumValue
                {
                    Id = org.Id,
                    Code = org.Code,
                    Value = org.Name,
                    DefaultValue = org.Name
                }).OrderBy(o => o.Value).ToList();
        }

        public string GetComboBoxDetailValue(BizForm form, BizComboBox comboBox)
        {
            var attrDef = GetComboBoxAttrDef(form, comboBox);
            
            return GetComboBoxDetailValue(comboBox, attrDef);
        }

        public string GetComboBoxDetailValue(BizComboBox comboBox, AttrDef attrDef)
        {
            if (comboBox.Value == null) return null;

            if (attrDef == null)
            {
                switch (comboBox.Ident)
                {
                    case SystemIdent.OrgId:
                    case SystemIdent.OrgCode:
                    case SystemIdent.OrgName:
                        return GetEnumOrganizationValue((Guid) comboBox.Value);
                    case SystemIdent.UserId:
                    case SystemIdent.UserName:
                        return GetEnumUserValue((Guid) comboBox.Value);
                }
                return null;
            }

            if (attrDef.Type.Id == (short)CissaDataType.Doc && attrDef.DocDefType != null)
            {
                var detailDocDef = _docDefRepo.DocDefById(attrDef.DocDefType.Id);

                AttrDef detailAttrDef;
                if (comboBox.DetailAttributeId != null)
                    detailAttrDef = detailDocDef.Attributes.FirstOrDefault(ad => ad.Id == comboBox.DetailAttributeId);
                else
                {
                    detailAttrDef =
                        detailDocDef.Attributes.FirstOrDefault(
                            ad =>
                                String.Equals(ad.Name, comboBox.DetailAttributeName, StringComparison.OrdinalIgnoreCase))
                        ??
                        detailDocDef.Attributes.FirstOrDefault(ad => ad.Type.Id == (int)CissaDataType.Text);
                }
                if (detailAttrDef == null) return null;

                var sqlQueryBuilder = _sqlQueryBuilderFactory.Create();
                using (var query = sqlQueryBuilder.Build(comboBox, detailDocDef.Id))
                {
                    query.AddAttribute("&Id");
                    query.AddAttribute(detailAttrDef.Id);
                    query.AndCondition("&Id", ConditionOperation.Equal, comboBox.Value);
                    using (var reader = _sqlQueryReaderFactory.Create(query))
                    {
                        if (reader.Read())
                            return !reader.IsDbNull(1) ? reader.GetString(1) : String.Empty;
                    }
                }
            }
            else if (attrDef.Type.Id == (short)CissaDataType.Enum && attrDef.EnumDefType != null)
                return _enumRepo.GetEnumValue(attrDef.EnumDefType.Id, (Guid) comboBox.Value);
            else if (attrDef.Type.Id == (short)CissaDataType.Organization)
                return GetEnumOrganizationValue((Guid) comboBox.Value);
            else if (attrDef.Type.Id == (short)CissaDataType.User)
                return GetEnumUserValue((Guid) comboBox.Value);

            return null;
        }

        private string GetEnumUserValue(Guid value)
        {
            var userInfo = _userRepo.FindUserInfo(value);

            if (userInfo == null) return null;

            return
                GetFirstNotEmpty(userInfo.LastName + " " + userInfo.FirstName + " " + userInfo.MiddleName,
                    userInfo.UserName) + ", " + userInfo.OrganizationName;
        }

        private string GetEnumOrganizationValue(Guid value)
        {
            var orgInfo = _orgRepo.Find(value);

            if (orgInfo == null) return null;

            return orgInfo.Name;
        }
    }
}
