using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    // TODO: Переименовать в Helper class
    public class AttributeRepository: IAttributeRepository
    {
//        public IDataContext DataContext { get; private set; }

//        [Obsolete("Неправильная констркуция")]
//        private readonly IDocDefRepository _docDefRepository;

        public AttributeRepository(/*IDataContext dataContext*/)
        {
//            DataContext = dataContext;
        }
        public AttributeRepository(IAppServiceProvider provider)
        {
//            DataContext = provider.Get<IDataContext>();
        }

        public AttributeBase CreateAttribute(AttrDef def)
        {
            switch ((CissaDataType) def.Type.Id)
            {
                case CissaDataType.Int:
                    return new IntAttribute(def);
                case CissaDataType.Text:
                    return new TextAttribute(def);
                case CissaDataType.Float:
                    return new FloatAttribute(def);
                case CissaDataType.Currency:
                    return new CurrencyAttribute(def);
                case CissaDataType.Enum:
                    return new EnumAttribute(def);
                case CissaDataType.DateTime:
                    return new DateTimeAttribute(def);
                case CissaDataType.Bool:
                    return new BoolAttribute(def);
                case CissaDataType.Doc:
                    return new DocAttribute(def);
                case CissaDataType.DocList:
                    return new DocListAttribute(def);
                case CissaDataType.Organization:
                    return new OrganizationAttribute(def);
                case CissaDataType.DocumentState:
                    return new DocumentStateAttribute(def);
                case CissaDataType.DocumentId:
                    return new MetaInfoAttribute(def);
            }
            throw new ApplicationException(String.Format("Неизвестный тип атрибута \"{0}\"", def.Caption));
        }
/*

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attributeDefId"></param>
        /// <param name="docId">Идентификатор документа</param>
        /// <remarks>
        /// Если передать docId = Guid.Empty выдаст атрибуты без инициализации
        /// </remarks>
        /// <returns></returns>
        [Obsolete("Устаревший, неиспользуемый метод")]
        public AttributeBase GetAttributeById(Guid attributeDefId, Guid docId)
        {
            var en = DataContext.Entities;
            var attrDefQuery =
                en.Object_Defs.OfType<Attribute_Def>().Include("Document_Defs").Where(a => a.Id == attributeDefId);
            if (!attrDefQuery.Any())
            {
                return null;
            }

            var dbAttrDef = attrDefQuery.First();
            var attrDef = new AttrDef
                              {
                                  Id = dbAttrDef.Id,
                                  Name = dbAttrDef.Name,
                                  Type = new TypeDef
                                             {
                                                 Id = dbAttrDef.Data_Types.Id,
                                                 Name = dbAttrDef.Data_Types.Name
                                             }
                              };

            switch ((CissaDataType) attrDef.Type.Id)
            {
                case CissaDataType.Int:
                    var intAttribute = new IntAttribute(attrDef);
                    var intQuery = en.Int_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (intQuery.Any())
                    {
                        var dbatr = intQuery.First();
                        intAttribute.Value = dbatr.Value;
                    }
                    return intAttribute;


                case CissaDataType.Float:
                    var floatAttribute = new FloatAttribute(attrDef);
                    var floatQuery =
                        en.Float_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (floatQuery.Any())
                    {
                        var dbatr = floatQuery.First();
                        floatAttribute.Value = dbatr.Value;
                    }
                    return floatAttribute;


                case CissaDataType.Currency:
                    var currencyAttribute = new CurrencyAttribute(attrDef);
                    var currencyQuery =
                        en.Currency_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (currencyQuery.Any())
                    {
                        var dbatr = currencyQuery.First();
                        currencyAttribute.Value = dbatr.Value;
                    }
                    return currencyAttribute;


                case CissaDataType.Text:
                    var txtAttribute = new TextAttribute(attrDef);
                    var txtQuery =
                        en.Text_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (txtQuery.Any())
                    {
                        var dbatr = txtQuery.First();
                        txtAttribute.Value = dbatr.Value;
                    }
                    return txtAttribute;

                case CissaDataType.Bool:
                    var boolAttribute = new BoolAttribute(attrDef);
                    var boolQuery =
                        en.Boolean_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (boolQuery.Any())
                    {
                        var dbAttr = boolQuery.First();
                        boolAttribute.Value = dbAttr.Value;
                    }
                    return boolAttribute;


                case CissaDataType.Doc:
                    attrDef.DocDefType = new DocDef
                                             {
                                                 Id = dbAttrDef.Document_Defs.Id,
                                                 Name = dbAttrDef.Document_Defs.Name,
                                                 Description = dbAttrDef.Document_Defs.Description,
                                                 IsPublic = dbAttrDef.Document_Defs.Is_Public ?? false
                                             };
                    var docAttribute = new DocAttribute(attrDef);
                    var docQuery =
                        en.Document_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (docQuery.Any())
                    {
                        var dbatr = docQuery.First();
                        docAttribute.Value = dbatr.Value;
                    }
                    return docAttribute;

                case CissaDataType.DocList:
                    attrDef.DocDefType = new DocDef
                                             {
                                                 Id = dbAttrDef.Document_Defs.Id,
                                                 Name = dbAttrDef.Document_Defs.Name,
                                                 Description = dbAttrDef.Document_Defs.Description
                                             };
                    var doclistAttribute = new DocListAttribute(attrDef);
                    var doclistQuery =
                        en.DocumentList_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (doclistQuery.Any())
                    {
                        doclistAttribute.ItemsDocId = new List<Guid>();
                        foreach (DocumentList_Attribute item in doclistQuery)
                        {
                            doclistAttribute.ItemsDocId.Add(item.Value);
                        }
                    }
                    return doclistAttribute;

                case CissaDataType.Enum:
                    using (var enumRepo = new EnumRepository(DataContext))
                    {
                        attrDef.EnumDefType = new EnumDef
                                              {
                                                  Id = dbAttrDef.Enum_Defs.Id,
                                                  Name = dbAttrDef.Enum_Defs.Name,
                                                  Description = dbAttrDef.Enum_Defs.Description,
                                                  EnumItems =
                                                      new List<EnumValue>(
                                                      enumRepo.GetEnumItems(dbAttrDef.Enum_Defs.Id))
                                              };
                    }
                    var enumAttribute = new EnumAttribute {AttrDef = attrDef};
                    var enumQuery =
                        en.Enum_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (enumQuery.Any())
                    {
                        var dbatr = enumQuery.First();
                        enumAttribute.Value = dbatr.Value;
                    }
                    return enumAttribute;

                case CissaDataType.DateTime:
                    var dateAttribute = new DateTimeAttribute(attrDef);
                    var dateQuery =
                        en.Date_Time_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (dateQuery.Any())
                    {
                        var dbAttr = dateQuery.First();
                        dateAttribute.Value = dbAttr.Value;
                    }
                    return dateAttribute;

                case CissaDataType.Organization:
                    var orgAttribute = new OrganizationAttribute(attrDef);
                    var orgQuery = en.Org_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (orgQuery.Any())
                    {
                        var dbatr = orgQuery.First();
                        orgAttribute.Value = dbatr.Value;
                    }
                    return orgAttribute;

                case CissaDataType.DocumentState:
                    var docStateAttribute = new DocumentStateAttribute(attrDef);
                    var docStateQuery =
                        en.Doc_State_Attributes.Where(a => a.Document_Id == docId && a.Def_Id == attributeDefId);
                    if (docStateQuery.Any())
                    {
                        var dbatr = docStateQuery.First();
                        docStateAttribute.Value = dbatr.Value;
                    }
                    return docStateAttribute;
            }
            return null;
        }
*/

        public static bool SameAttributeValue(AttributeBase attr1, AttributeBase attr2)
        {
            if (attr1 == null) throw new ArgumentNullException("attr1");
            if (attr2 == null) throw new ArgumentNullException("attr2");

            var val1 = attr1.ObjectValue;
            var val2 = attr2.ObjectValue;

//            if (val1 == null || val2 == null) return false;
            if (attr1.AttrDef.Type.Id != attr2.AttrDef.Type.Id) return false;

            if (attr1.AttrDef.Type.Id == (short)CissaDataType.DocList)
            {
                var dlAttr1 = (DocListAttribute) attr1;
                var dlAttr2 = (DocListAttribute) attr2;

                var list1 = dlAttr1.AddedDocIds != null ? new List<Guid>(dlAttr1.AddedDocIds) : new List<Guid>();
                var list2 = dlAttr2.AddedDocIds != null ? new List<Guid>(dlAttr2.AddedDocIds) : new List<Guid>();
                foreach (var id in list1)
                {
                    if (!list2.Contains(id)) return false;
                    list2.Remove(id);
                }

                if (list2.Count > 0) return false;

                var list11 = dlAttr1.AddedDocs != null ? new List<Doc>(dlAttr1.AddedDocs) : new List<Doc>();
                var list22 = dlAttr2.AddedDocs != null ? new List<Doc>(dlAttr2.AddedDocs) : new List<Doc>();
                foreach (var doc in list11)
                {
                    var doc1 = doc;
                    var doc2 = list22.FirstOrDefault(d => d.Id == doc1.Id);
                    if (doc2 == null) return false;
                    list22.Remove(doc2);
                }

                if (list22.Count > 0) return false;

                return true;
            }

            if (val1 == null && val2 == null) return true;

            if (attr1.AttrDef.Type.Id == (short)CissaDataType.Text)
            {
                var txt1 = val1 != null ? val1.ToString() : String.Empty;
                var txt2 = val2 != null ? val2.ToString() : String.Empty;
                if (String.IsNullOrEmpty(txt1) && String.IsNullOrEmpty(txt2)) return true;
                return String.CompareOrdinal(txt1, txt2) == 0;
            }
            if (attr1.AttrDef.Type.Id == (short) CissaDataType.Blob)
            {
                var blobAttr1 = attr1 as BlobAttribute;
                var blobAttr2 = attr2 as BlobAttribute;

                if (blobAttr1 == null || blobAttr2 == null) return false;
                if (String.CompareOrdinal(blobAttr1.FileName, blobAttr2.FileName) != 0) return false;
                if (blobAttr1.HasValue != blobAttr2.HasValue) return false;
                if (blobAttr1.Value == null && blobAttr2.Value == null) return true;
                if (blobAttr1.Value != null && blobAttr2.Value != null) return blobAttr1.Value.SequenceEqual(blobAttr2.Value);
                return false;
            }

            if (val1 == null || val2 == null) return false;
            switch (attr1.AttrDef.Type.Id)
            {
                case (short) CissaDataType.Int:
                    return ((int) val1) == ((int) val2);
                case (short)CissaDataType.Float:
                    return Math.Abs(((double) val1) - ((double) val2)) < 0.00000000001;
                case (short)CissaDataType.Currency:
                    return ((decimal) val1) == ((decimal) val2);
                case (short) CissaDataType.DateTime:
                    return ((DateTime) val1) == ((DateTime) val2);
                case (short)CissaDataType.Doc:
                case (short) CissaDataType.Enum:
                case (short) CissaDataType.Organization:
                case (short) CissaDataType.DocumentState:
                    return ((Guid) val1) == ((Guid) val2);
            }
            return false;
        }
/*
        [Obsolete("Неиспользуемый метод")]
        public AttributeBase GetAttributeByName(string attributeName, Guid docDefId)
        {
            var attrDefQuery = _docDefRepository.GetDocumentAttributes(docDefId, Guid.Empty).Where(a => a.Name == attributeName);
            var docDef = _docDefRepository.DocDefById(docDefId);
            var attrDefQuery = docDef.Attributes.Where(a => String.Compare(a.Name, attributeName, true) == 0);

            if (!attrDefQuery.Any())
            {
                throw new ApplicationException(
                    string.Format("У документа типа {0} атрибута с именем {1} не существует", docDefId, attributeName)
                    );
            }

            return CreateAttribute(attrDefQuery.FirstOrDefault());    //GetAttributeById(attrDefQuery.First().Id, Guid.Empty);
        }
*/
        public AttributeBase GetAttributeById(Guid attributeDefId, Doc document)
        {
            return document.Attributes.FirstOrDefault(a => a.AttrDef.Id == attributeDefId);

 /*           var queryCurrency = document.AttrCurrency.Where(atr => atr.AttrDef.Id == attributeDefId);

            if (queryCurrency.Any())
            {
                return queryCurrency.First();
            }

            var queryDoc = document.AttrDoc.Where(atr => atr.AttrDef.Id == attributeDefId);

            if (queryDoc.Any())
            {
                return queryDoc.First();
            }

            var queryDocList = document.AttrDocList.Where(atr => atr.AttrDef.Id == attributeDefId);

            if (queryDocList.Any())
            {
                return queryDocList.First();
            }

            var queryEnum = document.AttrEnum.Where(atr => atr.AttrDef.Id == attributeDefId);

            if (queryEnum.Any())
            {
                return queryEnum.First();
            }

            var queryFloat = document.AttrFloat.Where(atr => atr.AttrDef.Id == attributeDefId);

            if (queryFloat.Any())
            {
                return queryFloat.First();
            }

            var queryInt = document.AttrInt.Where(atr => atr.AttrDef.Id == attributeDefId);

            if (queryInt.Any())
            {
                return queryInt.First();
            }

            var queryText = document.AttrText.Where(atr => atr.AttrDef.Id == attributeDefId);

            if (queryText.Any())
            {
                return queryText.First();
            }
            var queryDateTime = document.AttrDateTime.Where(atr => atr.AttrDef.Id == attributeDefId);

            if(queryDateTime.Any())
            {
                return queryDateTime.First();
            }

            var queryBool = document.AttrDateTime.Where(atr => atr.AttrDef.Id == attributeDefId);

            if (queryBool.Any())
            {
                return queryBool.First();
            }

            /*foreach(var attr in document.AttrDoc)
            {
                if (attr.Document != null)
                {
                    var result = GetAttributeById(attributeDefId, attr.Document);

                    if (result != null) return result;
                }
                else
                {
                    attr.AttrDef.DocDefType.Attributes.Where(a => a.Id == attributeDefId);
                }
            }#1#
            return null;*/
        }

        /*[Obsolete("Не используемый метод")]
        public List<Guid> GetAttributeDocList(out int count, Guid docId, Guid attributeDefId, int pageNo, int pageSize = 0)
        {
            var en = DataContext.GetEntityDataContext().Entities;
            var result = from attr in en.DocumentList_Attributes
                         orderby attr.Created descending
                         where attr.Def_Id == attributeDefId && attr.Document_Id == docId &&
                               attr.Expired >= new DateTime(9999, 12, 31)
                         select attr.Value;

            count = result.Count();

            if (pageSize <= 0)
            {
                return result.ToList();
            }
            /*pageCount = count / pageSize;
                    if (count % pageSize > 0) pageCount++;
                    #1#
            return result.Skip(pageNo*pageSize).Take(pageSize).ToList();
        }*/
    }
}
