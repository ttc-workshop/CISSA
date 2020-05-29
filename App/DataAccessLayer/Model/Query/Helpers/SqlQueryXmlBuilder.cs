using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Core;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers
{
    public class SqlQueryXmlBuilder
    {
        public IAppServiceProvider Provider { get; private set; }
        public IDataContext DataContext { get; private set; }
        public SqlQuery Query { get; private set; }

        private readonly IEnumRepository _enumRepo;
        private readonly IDocRepository _docRepo;
        private readonly ISqlQueryReaderFactory _readerFactory;

        public SqlQueryXmlBuilder(IAppServiceProvider provider, IDataContext dataContext, SqlQuery query)
        {
            Provider = provider;
            DataContext = dataContext;
            Query = query;

            _docRepo = Provider.Get<IDocRepository>();
            _enumRepo = Provider.Get<IEnumRepository>();
            _readerFactory = Provider.Get<ISqlQueryReaderFactory>(DataContext);
        }

        public Action<SqlQueryXmlBuilder, SqlQueryReader, IList<XElement>> BeforeEachRecord { get; set; }
        public Action<SqlQueryXmlBuilder, SqlQueryReader, IList<XElement>> AfterEachRecord { get; set; }

        private SqlQueryReader GetReader()
        {
            return _readerFactory.Create(Query); //new SqlQueryReader(DataContext, Query);
        }

        public string GetRootElementName()
        {
            return Query.Source.AliasName + "List";
        }
        public string GetRecordElementName()
        {
            return Query.Source.AliasName;
        }

        public XElement BuildAll(Action<int> progressAction)
        {
            var root = new XElement(GetRootElementName());
            using (var reader = GetReader())
            {
                var count = 0;
                reader.Open();
                while (reader.Read())
                {
                    var record = new XElement(GetRecordElementName());
                    if (BeforeEachRecord != null)
                    {
                        var list = new List<XElement>();
                        BeforeEachRecord(this, reader, list);
                        list.ForEach(i => record.Add(i));
                    }
                    record.Add(BuildCurrent(reader));
                        /*from fld in reader.Fields
                        select
                            new XElement(fld.AttributeName ?? fld.AttributeId.ToString(), reader.GetValue(fld.Index)));*/
                    if (AfterEachRecord != null)
                    {
                        var list = new List<XElement>();
                        AfterEachRecord(this, reader, list);
                        list.ForEach(i => record.Add(i));
                    }
                    root.Add(record);
                    if (progressAction != null) progressAction(count);
                    count++;
                }
            }
            return root;
        }

        public  IEnumerable<XElement> BuildCurrent(SqlQueryReader reader)
        {
            return reader.Fields.Select(fld => GetValueElement(reader, fld));
        }

        private static string GetFieldElementName(SqlQueryField field)
        {
            var name = field.AttributeName;
            if (String.IsNullOrWhiteSpace(name))
                name = field.SelectAttribute != null ? field.SelectAttribute.Alias : String.Empty;
            if (String.IsNullOrWhiteSpace(name))
                name = field.SelectAttribute != null ? field.SelectAttribute.AliasName : String.Empty;
            if (String.IsNullOrWhiteSpace(name))
                name = field.AttributeId.ToString();

            return name.Replace("&", "");
        }

        public XElement GetValueElement(SqlQueryReader reader, SqlQueryField field)
        {
            var name = GetFieldElementName(field);

            if (reader.IsDbNull(field.Index))
                return new XElement(name, "");

            if (field.AttrDef != null)
            {
                switch (field.AttrDef.Type.Id)
                {
                    case (short) CissaDataType.Enum:
                        if (field.AttrDef.EnumDefType == null) break;
                        var enumDef = _enumRepo.Get(field.AttrDef.EnumDefType.Id);
                        var enumItem = enumDef.EnumItems.FirstOrDefault(i => i.Id == reader.GetGuid(field.Index));
                        if (enumItem != null)
                            return new XElement(name, new XElement("Id", enumItem.Id),
                                new XElement("Value", enumItem.Value));
                        break;
                    case (short) CissaDataType.Doc:
                        if (PublicDocAttribute(field.AttrDef))
                        {
                            var doc = _docRepo.LoadById(reader.GetGuid(field.Index));
                            return new XElement(name, GetDocElement(doc));
                        }
                        break;
/*
                            case (short) CissaDataType.DocList:
                                if (PublicDocAttribute(field.AttrDef))
                                {
                                    DocRepo.Ge
                                    var docList = new DocList(DataContext);
                                    return new XElement(name, GetDocListElement(doc));
                                }
                                break;
*/
                }
            }
            return new XElement(name, reader.GetValue(field.Index));
        }

        private XElement GetDocElement(Doc doc)
        {
            return new XElement(doc.DocDef.Name ?? doc.DocDef.Id.ToString(),
                GetDocAttributeElements(doc));
        }

        private IEnumerable<XElement> GetDocAttributeElements(Doc doc)
        {
            yield return new XElement("Id", doc.Id);
            yield return new XElement("Created", doc.CreationTime);
            foreach (
                var attr in doc.Attributes.Where(a => a.AttrDef != null && !String.IsNullOrWhiteSpace(a.AttrDef.Name)))
                yield return
                    new XElement(attr.AttrDef.Name, attr.ObjectValue);
        }

        public bool PublicDocAttribute(AttrDef attrDef)
        {
            return false;
        }
    }
}
