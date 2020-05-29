using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.Cissa.Report.Common
{
    public class SqlQueryDataSet : DataSet
    {
        public SqlQueryReader Reader { get; private set; }

        private IAppServiceProvider Provider { get; set; }

        public SqlQueryDataSet(SqlQueryReader reader, Guid userId)
        {
            Reader = reader;
        }

        public SqlQueryDataSet(IAppServiceProvider provider, SqlQueryReader reader)
        {
            Provider = provider;
            Reader = reader;
        }

        private int Index { get; set; }
        private bool _isEof;
        private bool _isRead;

        public override bool Eof()
        {
            if (!_isRead)
            {
                _isEof = !Reader.Read();
                if (!_isEof) Index++;
                _isRead = true;
            }
            return _isEof;
        }

        public override void Next()
        {
            //Index++;
            if (!_isEof)
            {
                _isEof = !Reader.Read();
                if (!_isEof) Index++;
                _isRead = true;
            }
        }

        public override void Reset()
        {
            Reader.Close();
            _isRead = false;
            Index = 0;
        }

        public override bool HasField(string fieldName)
        {
            return Reader.Query.FindAttribute(fieldName) != null;
        }

        public override int GetRecordNo()
        {
            return Index;
        }

        public override DataSetField CreateField(string fieldName)
        {
            if (String.Equals(fieldName, "_recordNo", StringComparison.InvariantCultureIgnoreCase))
                return new SqlQueryDataSetRecordNoField(this);
            return new SqlQueryDataSetField(this, Reader.Query.GetAttribute(fieldName), null);
        }

        internal IComboBoxEnumProvider GetComboBoxEnumProvider()
        {
            if (Provider != null)
                return Provider.Get<IComboBoxEnumProvider>();

            return new ComboBoxEnumProvider(Reader.DataContext, Reader.Query.UserId);
        }

        internal IList<EnumValue> GetEnumItems(Guid enumDefId)
        {
            var enumRepo = Provider != null ? Provider.Get<IEnumRepository>() : new EnumRepository(Reader.DataContext);
            return new List<EnumValue>(enumRepo.GetEnumItems(enumDefId));
        }

        internal IEnumRepository GetEnumRepository()
        {
            if (Provider != null)
                return Provider.Get<IEnumRepository>();

            return null;
        }

        internal IDocStateRepository GetDocStateRepository()
        {
            return Provider != null ? Provider.Get<IDocStateRepository>() : null;
        }

        internal IOrgRepository GetOrgRepository()
        {
            return Provider != null ? Provider.Get<IOrgRepository>() : null;
        }
        internal IUserRepository GetUserRepository()
        {
            return Provider != null ? Provider.Get<IUserRepository>() : null;
        }
    }

    public class SqlQueryDataSetField : DataSetField
    {
        private AttrDef AttrDef { get; set; }
        private SystemIdent Ident { get; set; }
        private int AttrIndex { get; set; }
        private BizControl Control { get; set; }
        private SqlQuerySummaryFunction Grouping { get; set; }

        private IList<EnumValue> _enumItems;

        private double SummaryValue { get; set; }

        public SqlQueryDataSetField(SqlQueryDataSet dataSet, SqlQuerySelectAttribute attr, BizControl control)
            : base(dataSet)
        {
            AttrIndex = dataSet.Reader.Query.Attributes.IndexOf(attr);
            AttrDef = attr.Def;
            if (AttrDef == null)
                Ident = attr.Ident;
            Control = control;
            Grouping = SqlQuerySummaryFunction.None;
            SummaryValue = 0.0;

            var comboBoxValueProvider = dataSet.GetComboBoxEnumProvider();
            if (AttrDef != null)
            {
                if (Control is BizComboBox)
                {
                    switch (AttrDef.Type.Id)
                    {
                        case (short) CissaDataType.Enum:
                            _enumItems = dataSet.GetEnumItems(AttrDef.EnumDefType.Id);
                            break;
                        case (short) CissaDataType.Organization:
                            _enumItems = comboBoxValueProvider.GetEnumOrganizationValues(null);
                            break;
                        case (short) CissaDataType.User:
                            _enumItems = comboBoxValueProvider.GetEnumUserValues();
                            break;
                        case (short) CissaDataType.Doc:
                            _enumItems = comboBoxValueProvider.GetEnumDocumentValues((BizComboBox) Control, AttrDef);
                            break;
                    }
                }
                else if (AttrDef.Type.Id == (short) CissaDataType.Enum && AttrDef.EnumDefType != null)
                    _enumItems = dataSet.GetEnumItems(AttrDef.EnumDefType.Id);
                else if (AttrDef.Type.Id == (short) CissaDataType.Doc && AttrDef.DocDefType != null)
                    _enumItems = comboBoxValueProvider.GetEnumDocumentValues(AttrDef, "Name");
                else if (AttrDef.Type.Id == (short) CissaDataType.Organization)
                    _enumItems = comboBoxValueProvider.GetEnumOrganizationValues(null);
                else if (AttrDef.Type.Id == (short) CissaDataType.User)
                    _enumItems = comboBoxValueProvider.GetEnumUserValues();
            }
        }

        public SqlQueryDataSetField(SqlQueryDataSet dataSet, SqlQuerySelectAttribute attr, SqlQuerySummaryFunction grouping = SqlQuerySummaryFunction.None)
            : base(dataSet)
        {
            AttrIndex = dataSet.Reader.Query.Attributes.IndexOf(attr);
            AttrDef = attr.Def;
            if (AttrDef == null)
                Ident = attr.Ident;
            Control = null;
            Grouping = grouping;

            var comboBoxValueProvider = dataSet.GetComboBoxEnumProvider();
            if (AttrDef != null && (Grouping == SqlQuerySummaryFunction.None || Grouping == SqlQuerySummaryFunction.Group))
            {
                if (AttrDef.Type.Id == (short)CissaDataType.Enum && AttrDef.EnumDefType != null)
                    _enumItems = dataSet.GetEnumItems(AttrDef.EnumDefType.Id);
                else if (AttrDef.Type.Id == (short)CissaDataType.Doc && AttrDef.DocDefType != null)
                    _enumItems = comboBoxValueProvider.GetEnumDocumentValues(AttrDef, "Name");
                else if (AttrDef.Type.Id == (short)CissaDataType.Organization)
                    _enumItems = comboBoxValueProvider.GetEnumOrganizationValues(null);
                else if (AttrDef.Type.Id == (short)CissaDataType.User)
                    _enumItems = comboBoxValueProvider.GetEnumUserValues();
            }
        }
        // private readonly IList<EnumValue> _enumValues;

        public override object GetValue()
        {
            // ((SqlQueryDataSet) DataSet).Reader.Open();

            var isNull = ((SqlQueryDataSet) DataSet).Reader.IsDbNull(AttrIndex);
            if (isNull) return null;
            var value = ((SqlQueryDataSet) DataSet).Reader.GetValue(AttrIndex);
            if (value == null) return null;
            if (Grouping == SqlQuerySummaryFunction.Count || Grouping == SqlQuerySummaryFunction.Avg || Grouping == SqlQuerySummaryFunction.Sum)
                return value;
            /*if (_enumValues != null && !isNull)
            {
                var enumVal = _enumValues.FirstOrDefault(e => e.Id == Guid.Parse(value.ToString()));
                return (enumVal != null) ? enumVal.Value : null;
            }*/
            var comboBox = Control as BizComboBox;
            if (comboBox != null && _enumItems != null)
            {
                Guid guidValue;
                if (Guid.TryParse(value.ToString(), out guidValue))
                {
                    // comboBox.Value = guidValue;
                    var enumValue = _enumItems.FirstOrDefault(i => i.Id == guidValue);
                    if (enumValue != null)
                        return enumValue.Value;
                }
            }
            if (AttrDef != null && AttrDef.Type.Id == (short) CissaDataType.Enum && _enumItems != null)
            {
                var enumValue = _enumItems.FirstOrDefault(i => i.Id == (Guid) value);
                if (enumValue != null)
                    return enumValue.Value;
            }
            if (AttrDef == null)
            {
                Guid guid;
                if (Ident == SystemIdent.State)
                {
                    if (Guid.TryParse(value.ToString(), out guid))
                    {
                        var enumValue = GetDocStateEnumValue(guid);
                        if (enumValue != null) return enumValue.Value;
                    }
                    else
                        return value;
                }
                else if (Ident == SystemIdent.OrgId && Guid.TryParse(value.ToString(), out guid))
                {
                    var enumValue = GetOrgInfoEnumValue(guid);
                    if (enumValue != null) return enumValue.Value;
                }
                else if (Ident == SystemIdent.UserId && Guid.TryParse(value.ToString(), out guid))
                {
                    var enumValue = GetUserInfoEnumValue(guid);
                    if (enumValue != null) return enumValue.Value;
                }
                else if (Ident == SystemIdent.UserName && Guid.TryParse(value.ToString(), out guid))
                {
                    var enumValue = GetUserInfoEnumValue(guid);
                    if (enumValue != null) return enumValue.Value;
                }
            }

            return value;
        }

        public override BaseDataType GetDataType()
        {
            if (Grouping == SqlQuerySummaryFunction.Count)
                return BaseDataType.Int;

            return AttrDef != null
                ? CissaDataTypeHelper.ConvertToBase(AttrDef.Type.Id)
                : SystemIdentConverter.ToBaseType(Ident);
        }

        private EnumValue FindEnumValue(Guid value)
        {
            return _enumItems != null ? _enumItems.FirstOrDefault(e => e.Id == value) : null;
        }

        private EnumValue GetDocStateEnumValue(Guid docStateId)
        {
            var enumValue = FindEnumValue(docStateId);

            if (enumValue != null) return enumValue;

            var docStateRepo = ((SqlQueryDataSet) DataSet).GetDocStateRepository();
            if (docStateRepo != null)
            {
                var stateType = docStateRepo.TryLoadById(docStateId);
                if (stateType != null)
                {
                    enumValue = new EnumValue {Id = stateType.Id, Value = stateType.Name};
                    if (_enumItems == null) _enumItems = new List<EnumValue>();
                    _enumItems.Add(enumValue);
                }
            }
            return enumValue;
        }
        private EnumValue GetOrgInfoEnumValue(Guid orgId)
        {
            var enumValue = FindEnumValue(orgId);

            if (enumValue != null) return enumValue;

            var orgRepo = ((SqlQueryDataSet)DataSet).GetOrgRepository();
            if (orgRepo != null)
            {
                var orgInfo = orgRepo.Find(orgId);
                if (orgInfo != null)
                {
                    enumValue = new EnumValue { Id = orgInfo.Id, Value = orgInfo.Name };
                    if (_enumItems == null) _enumItems = new List<EnumValue>();
                    _enumItems.Add(enumValue);
                }
            }
            return enumValue;
        }
        private EnumValue GetUserInfoEnumValue(Guid userId)
        {
            var enumValue = FindEnumValue(userId);

            if (enumValue != null) return enumValue;

            var userRepo = ((SqlQueryDataSet)DataSet).GetUserRepository();
            if (userRepo != null)
            {
                var userInfo = userRepo.FindUserInfo(userId);
                if (userInfo != null)
                {
                    enumValue = new EnumValue { Id = userInfo.Id, Value = userInfo.LastName + " " + userInfo.FirstName + ", " + userInfo.PositionName + ", " + userInfo.OrganizationName };
                    if (_enumItems == null) _enumItems = new List<EnumValue>();
                    _enumItems.Add(enumValue);
                }
            }
            return enumValue;
        }
    }

    public class SqlQueryDataSetRecordNoField : DataSetField
    {
        public SqlQueryDataSetRecordNoField(SqlQueryDataSet dataSet) : base(dataSet) { }

        public override object GetValue()
        {
            return DataSet.GetRecordNo();
        }

        public override BaseDataType GetDataType()
        {
            return BaseDataType.Int;
        }
    }
}
