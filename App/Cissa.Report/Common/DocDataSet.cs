using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.Cissa.Report.Common
{
    public class DocDataSet : DataSet
    {
        public IAppServiceProvider Provider { get; private set; }
        private readonly bool _ownProvider = false;
        public IDataContext DataContext { get; private set; }
        public IList<Guid> DocList { get; private set; }
        public IDocRepository DocRepo { get; private set; }

        public DocDataSet(IDataContext dataContext, IEnumerable<Guid> docs, Guid userId)
        {
            DataContext = dataContext;
            DocList = new List<Guid>(docs);
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = factory.Create();
            _ownProvider = true;
            DocRepo = Provider.Get<IDocRepository>(); // new DocRepository(DataContext, userId);
        }
        public DocDataSet(IAppServiceProvider provider, IDataContext dataContext, IEnumerable<Guid> docs, Guid userId)
        {
            Provider = provider;
            _ownProvider = false;
            DataContext = dataContext;
            DocList = new List<Guid>(docs);
            DocRepo = Provider.Get<IDocRepository>(); // new DocRepository(DataContext, userId);
        }

        public int Index { get; private set; }
        public Doc Current { get; private set; }

        public override bool Eof()
        {
            return DocList.Count <= Index;
        }

        public override void Next()
        {
            Index++;
            if (DocList.Count > Index)
            {
                Current = DocRepo.LoadById(DocList[Index]);
            }
        }

        public override void Reset()
        {
            Index = 0;
        }

        public Doc GetCurrent()
        {
            if (Current == null && Index < DocList.Count)
                Current = DocRepo.LoadById(DocList[Index]);
            return Current;
        }

        protected override void DoDispose(bool managed)
        {
            if (Provider != null && _ownProvider)
            {
                Provider.Dispose();
                Provider = null;
            }
        }

        public override bool HasField(string fieldName)
        {
            var curDoc = GetCurrent();
            if (curDoc != null)
                return curDoc.Attributes.FirstOrDefault(a => String.Equals(a.AttrDef.Name, fieldName, StringComparison.OrdinalIgnoreCase)) != null;
            return false;
        }

        public override int GetRecordNo()
        {
            return Index;
        }

        ~DocDataSet()
        {
            /*if (DocRepo != null)
                DocRepo.Dispose();*/
            DoDispose(false);
        }
    }

    public class DocDataSetField : DataSetField
    {
        public AttrDef AttrDef { get; private set; }

        public DocDataSetField(DocDataSet dataSet, AttrDef attrDef)
            : base(dataSet)
        {
            AttrDef = attrDef;
            // var dataContext = dataSet.DataContext;
            var provider = dataSet.Provider;
            var prov = provider.Get<IComboBoxEnumProvider>(); // new ComboBoxEnumProvider(dataContext, null);

            switch (AttrDef.Type.Id)
            {
                case (short) CissaDataType.Enum:
                    // using (var enumRepo = new EnumRepository(dataContext))
                    var enumRepo = provider.Get<IEnumRepository>();
                    _enumValues = new List<EnumValue>(enumRepo.GetEnumItems(AttrDef.EnumDefType.Id));
                    break;
                case (short)CissaDataType.Organization:
                    _enumValues = prov.GetEnumOrganizationValues(null);
                    break;
                case (short)CissaDataType.User:
                    _enumValues = prov.GetEnumUserValues();
                    break;
                case (short)CissaDataType.Doc:
                    _enumValues = prov.GetEnumDocumentValues(AttrDef, "Name");
                    break;
            }
        }

        // TODO: Добавить вывод списка документов (как в ComboBox)

        private readonly List<EnumValue> _enumValues;

        public override object GetValue()
        {
            var doc = ((DocDataSet) DataSet).GetCurrent();
            if (doc == null) return null;

            var attr = doc.Attributes.FirstOrDefault(a => a.AttrDef.Id == AttrDef.Id);
            if (attr is EnumAttribute && _enumValues != null)
            {
                var enumVal = _enumValues.FirstOrDefault(e => e.Id == ((EnumAttribute) attr).Value);
                return (enumVal != null) ? enumVal.Value : null;
            }
            if (attr is DocAttribute)
            {
                
            }
            return attr != null ? attr.ObjectValue : null;
        }

        public override BaseDataType GetDataType()
        {
            return CissaDataTypeHelper.ConvertToBase(AttrDef.Type.Id);
        }
    }
}
