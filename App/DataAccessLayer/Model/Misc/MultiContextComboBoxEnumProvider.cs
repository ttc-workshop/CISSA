using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Misc
{
    public class MultiContextComboBoxEnumProvider : IComboBoxEnumProvider
    {
        private IAppServiceProvider Provider { get; set; }
        public IMultiDataContext DataContext { get; private set; }

        private readonly IDictionary<IDataContext, IComboBoxEnumProvider> _repositories = new Dictionary<IDataContext, IComboBoxEnumProvider>();

        private readonly IEnumRepository _enumRepo;
        private readonly IUserRepository _userRepo;
        private readonly IOrgRepository _orgRepo;

        public MultiContextComboBoxEnumProvider(IAppServiceProvider provider)
        {
            Provider = provider;
            DataContext = Provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Document))
                    _repositories.Add(context, new ComboBoxEnumProvider(Provider, context));
            }
            _enumRepo = Provider.Get<IEnumRepository>();
            _userRepo = Provider.Get<IUserRepository>();
            _orgRepo = Provider.Get<IOrgRepository>();
        }

        public List<EnumValue> GetEnumDocumentValues(BizComboBox combo, DocDef detailDocDef)
        {
            var result = new List<EnumValue>();
            foreach (var pair in _repositories.Where(pair => pair.Key.DataType.HasFlag(DataContextType.Document)))
            {
                result.AddRange(pair.Value.GetEnumDocumentValues(combo, detailDocDef));
            }
            return result;
        }

        public List<EnumValue> GetEnumDocumentValues(BizComboBox combo, AttrDef attrDef)
        {
            var result = new List<EnumValue>();
            foreach (var pair in _repositories.Where(pair => pair.Key.DataType.HasFlag(DataContextType.Document)))
            {
                result.AddRange(pair.Value.GetEnumDocumentValues(combo, attrDef));
            }
            return result;
        }

        public List<EnumValue> GetEnumDocumentValues(AttrDef attrDef, params string[] attrNames)
        {
            var result = new List<EnumValue>();
            foreach (var pair in _repositories.Where(pair => pair.Key.DataType.HasFlag(DataContextType.Document)))
            {
                result.AddRange(pair.Value.GetEnumDocumentValues(attrDef, attrNames));
            }
            return result;
        }

        public List<EnumValue> GetEnumUserValues()
        {
            var result = new List<EnumValue>();
            //foreach (var pair in _repositories.Where(pair => pair.Key.DataType.HasFlag(DataContextType.Account)))
            {
                result.AddRange(_repositories.Values.First().GetEnumUserValues());
            }
            return result;
        }

        public List<EnumValue> GetEnumOrganizationValues(Guid? orgTypeId)
        {
            var result = new List<EnumValue>();
            //foreach (var pair in _repositories/*.Where(pair => pair.Key.DataType.HasFlag(DataContextType.Account))*/)
            {
                result.AddRange(_repositories.Values.First().GetEnumOrganizationValues(orgTypeId));
            }
            return result;
        }

        public IList<EnumValue> GetFormComboBoxValues(BizForm form, BizComboBox comboBox)
        {
            var list = new List<EnumValue>();

            foreach (var pair in _repositories)
            {
                list.AddRange(pair.Value.GetFormComboBoxValues(form, comboBox));
            }
            return list;
        }

        public IList<EnumValue> GetFormComboBoxValues(BizComboBox comboBox, AttrDef attrDef)
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
                foreach (var pair in _repositories.Where(pair => pair.Key.DataType.HasFlag(DataContextType.Document)))
                {
                    list.AddRange(pair.Value.GetFormComboBoxValues(comboBox, attrDef));
                }
                return list;
            }
            if (attrDef.Type.Id == (short)CissaDataType.Enum && attrDef.EnumDefType != null)
                return new List<EnumValue>(_enumRepo.GetEnumItems(attrDef.EnumDefType.Id));
            if (attrDef.Type.Id == (short)CissaDataType.Organization)
                return GetEnumOrganizationValues(attrDef.OrgTypeId);
            if (attrDef.Type.Id == (short)CissaDataType.User)
                return GetEnumUserValues();

            return list;
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

            if (attrDef.Type.Id == (short) CissaDataType.Doc && attrDef.DocDefType != null)
            {
                foreach (var pair in _repositories.Where(pair => pair.Key.DataType.HasFlag(DataContextType.Document)))
                {
                    list.AddRange(pair.Value.GetFormComboBoxValues(form, comboBox, attrDef));
                }
                return list;
            }
            if (attrDef.Type.Id == (short) CissaDataType.Enum && attrDef.EnumDefType != null)
                return new List<EnumValue>(_enumRepo.GetEnumItems(attrDef.EnumDefType.Id));
            if (attrDef.Type.Id == (short) CissaDataType.Organization)
                return GetEnumOrganizationValues(attrDef.OrgTypeId);
            if (attrDef.Type.Id == (short) CissaDataType.User)
                return GetEnumUserValues();

            return list;
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
                        return GetEnumOrganizationValue((Guid)comboBox.Value);
                    case SystemIdent.UserId:
                    case SystemIdent.UserName:
                        return GetEnumUserValue((Guid)comboBox.Value);
                }
                return null;
            }

            if (attrDef.Type.Id == (short)CissaDataType.Doc && attrDef.DocDefType != null)
            {
                return
                    _repositories.Where(pair => pair.Key.DataType.HasFlag(DataContextType.Document))
                        .Select(pair => pair.Value.GetComboBoxDetailValue(comboBox, attrDef))
                        .FirstOrDefault();
            }
            if (attrDef.Type.Id == (short)CissaDataType.Enum && attrDef.EnumDefType != null)
                return _enumRepo.GetEnumValue(attrDef.EnumDefType.Id, (Guid)comboBox.Value);
            if (attrDef.Type.Id == (short)CissaDataType.Organization)
                return GetEnumOrganizationValue((Guid)comboBox.Value);
            if (attrDef.Type.Id == (short)CissaDataType.User)
                return GetEnumUserValue((Guid)comboBox.Value);

            return null;
        }

        private string GetEnumUserValue(Guid value)
        {
            var userInfo = _userRepo.FindUserInfo(value);

            if (userInfo == null) return null;

            return
                ComboBoxEnumProvider.GetFirstNotEmpty(userInfo.LastName + " " + userInfo.FirstName + " " + userInfo.MiddleName,
                    userInfo.UserName) + ", " + userInfo.OrganizationName;
        }

        private string GetEnumOrganizationValue(Guid value)
        {
            var orgInfo = _orgRepo.Find(value);

            if (orgInfo == null) return null;

            return orgInfo.Name;
        }

        private AttrDef GetComboBoxAttrDef(BizForm form, BizComboBox comboBox)
        {
            var finder = new FormHelper(Provider, form);
            var docDef = finder.FindAttributeDocDef(comboBox.AttributeDefId);
            if (docDef != null)
                return docDef.Attributes.FirstOrDefault(a => a.Id == comboBox.AttributeDefId);

            return null;
        }
    }
}