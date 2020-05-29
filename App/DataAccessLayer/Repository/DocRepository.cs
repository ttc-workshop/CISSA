using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Documents.AutoAttr;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Storage;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class DocRepository: IDocRepository
    {
        public IAppServiceProvider Provider { get; private set; }
        /// <summary>
        /// Контекст данных
        /// </summary>
        public IDataContext DataContext { get; private set; }

        private readonly bool _ownDataContext;

        private readonly IDocDefRepository _docDefRepo;
        private readonly IDocStateRepository _docStateRepo;
//        private readonly IPermissionRepository _permissionRepo;
        private readonly IAttributeStorage _attrStorage;
        private readonly IDocumentStorage _docStorage;
        private readonly IDocumentTableMapRepository _tableMapRepo;
        private readonly IOrgRepository _orgRepo;
        private readonly IUserRepository _userRepo;

        private readonly static DateTime MaxDate = new DateTime(9999, 12, 31);

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; private set; }

        public DocRepository(IDataContext dataContext, Guid userId)
        {
            if (dataContext == null)
            {
                DataContext = new DataContext();
                _ownDataContext = true;
            } 
            else
                DataContext = dataContext;
            UserId = userId;

            _docDefRepo = new DocDefRepository(DataContext, UserId);
            _docStateRepo = new DocStateRepository(DataContext);
//            _permissionRepo = new PermissionRepository(DataContext);
            _attrStorage = new AttributeStorage(DataContext); //(SqlConnection) DataContext.Connection.StoreConnection);
            _docStorage = new DocumentStorage(DataContext); //(SqlConnection)DataContext.Connection.StoreConnection);
            _tableMapRepo = new DocumentTableMapRepository(DataContext);
            var userRepo = new UserRepository(DataContext);
            _userRepo = userRepo;
            _orgRepo = userRepo.OrgRepo ?? new OrgRepository(DataContext);
        }

        public DocRepository(IAppServiceProvider provider, IDataContext dataContext)
        {
            Provider = provider;
            DataContext = dataContext; //provider.Get<IDataContext>();
            // var userDataProvider = provider.Get<IUserDataProvider>();
            UserId = provider.GetCurrentUserId();

            _docDefRepo = provider.Get<IDocDefRepository>();
            _docStateRepo = provider.Get<IDocStateRepository>();
            _attrStorage = provider.Get<IAttributeStorage>(DataContext);
            _docStorage = provider.Get<IDocumentStorage>(DataContext);
            _tableMapRepo = provider.Get<IDocumentTableMapRepository>();
            _userRepo = provider.Get<IUserRepository>();
            _orgRepo = provider.Get<IOrgRepository>();
        }

        public DocRepository(Guid userId) : this(null, userId) {}
        public DocRepository(IDataContext dataContext) : this(dataContext, Guid.Empty) { }
        public DocRepository() : this(null, Guid.Empty) { }

        /// <summary>
        /// Кэш документов
        /// </summary>
        public static readonly ObjectCache<Doc> DocCache = new ObjectCache<Doc>(0, 500000);
        //public static readonly object DocSaveLock = new object();
        //private static readonly object DocLoadLock = new object();

        public Doc CreateDoc(Guid docDefId)
        {
            return CreateDoc(_docDefRepo.DocDefById(docDefId));
        }

        public Doc CreateDoc(DocDef docDef)
        {
            var userInfo = _userRepo.GetUserInfo(UserId);

            var returnDoc = new Doc
            {
                Id = Guid.NewGuid(),
                DocDef = docDef,
                Attributes = new List<AttributeBase>(),
                IsNew = true,
                UserId = UserId,
                OrganizationId = userInfo != null ? userInfo.OrganizationId : null,
                PositionId = userInfo != null ? userInfo.PositionId : null,
                CreationTime = DateTime.Now,
                DataContextName = DataContext.Name
            };

            CreateDocAttributes(returnDoc, docDef);

            return returnDoc;
        }

        protected T CreateDocumentAttribute<T>(AttrDef attrDef) where T : AttributeBase
        {
            var newAttr = (T)Activator.CreateInstance(typeof(T), attrDef);

            if (attrDef.Type.Id != (short) CissaDataType.Doc || attrDef.DocDefType == null) return newAttr;

            var docAttr = newAttr as DocAttribute;
            if (docAttr == null) return newAttr;

            var docDefType = _docDefRepo.DocDefById(attrDef.DocDefType.Id);
            if (docDefType != null && docDefType.IsInline)
            {
                docAttr.Document = CreateDoc(docDefType.Id);
            }
            return newAttr;
        }

        private void CreateDocumentAttributes<T>(Doc doc, DocDef docDef, CissaDataType dataType) where T : AttributeBase
        {
            docDef.Attributes.Where(a => a.Type.Id == (short)dataType)
                .ToList().ForEach(item =>
                {
                    var attr = doc.Attributes.FirstOrDefault(a1 => a1.AttrDef.Id == item.Id);
                    if (attr == null)
                    {
                        T newAttr = CreateDocumentAttribute<T>(item);
                        doc.Attributes.Add(newAttr);
                    }
                    else
                        attr.AttrDef = item;
                });
        }
        private void CreateNewDocumentAttributes<T, TValue>(Doc doc, DocDef docDef, CissaDataType dataType) where T : AttributeBase
        {
            docDef.Attributes.Where(a => a.Type.Id == (short)dataType)
                .ToList().ForEach(item =>
                {
                    var attr = doc.Attributes.FirstOrDefault(a1 => a1.AttrDef.Id == item.Id);
                    if (attr == null)
                    {
                        T newAttr = CreateDocumentAttribute<T>(item);
                        doc.Attributes.Add(newAttr);
                        if (!String.IsNullOrEmpty(item.DefaultValue))
                        {
                            TValue d;
                            if (item.DefaultValue.TryParse(out d))
                                newAttr.ObjectValue = d;
                        }
                    }
                    else
                        attr.AttrDef = item;
                });
        }

        internal void CreateDocAttributes(Doc doc, DocDef docDef)
        {
            CreateDocumentAttributes<CurrencyAttribute>(doc, docDef, CissaDataType.Currency);
            CreateDocumentAttributes<IntAttribute>(doc, docDef, CissaDataType.Int);
            CreateDocumentAttributes<TextAttribute>(doc, docDef, CissaDataType.Text);
            CreateDocumentAttributes<BlobAttribute>(doc, docDef, CissaDataType.Blob);
            CreateDocumentAttributes<FloatAttribute>(doc, docDef, CissaDataType.Float);
            CreateDocumentAttributes<EnumAttribute>(doc, docDef, CissaDataType.Enum);
            CreateDocumentAttributes<DocAttribute>(doc, docDef, CissaDataType.Doc);
            CreateDocumentAttributes<DocListAttribute>(doc, docDef, CissaDataType.DocList);
            CreateDocumentAttributes<BoolAttribute>(doc, docDef, CissaDataType.Bool);
            CreateDocumentAttributes<DateTimeAttribute>(doc, docDef, CissaDataType.DateTime);
            CreateDocumentAttributes<AutoAttribute>(doc, docDef, CissaDataType.Auto);
            CreateDocumentAttributes<OrganizationAttribute>(doc, docDef, CissaDataType.Organization);
            CreateDocumentAttributes<DocumentStateAttribute>(doc, docDef, CissaDataType.DocumentState);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.DocumentDef);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.User);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.OrgPosition);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.OrgUnit);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.EnumDef);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.Form);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.Process);
        }

        internal void CheckDocumentAttributes(Doc doc, DocDef docDef)
        {
            CreateDocumentAttributes<CurrencyAttribute>(doc, docDef, CissaDataType.Currency);
            CreateDocumentAttributes<IntAttribute>(doc, docDef, CissaDataType.Int);
            CreateDocumentAttributes<TextAttribute>(doc, docDef, CissaDataType.Text);
            CreateDocumentAttributes<BlobAttribute>(doc, docDef, CissaDataType.Blob);
            CreateDocumentAttributes<FloatAttribute>(doc, docDef, CissaDataType.Float);
            CreateDocumentAttributes<EnumAttribute>(doc, docDef, CissaDataType.Enum);
            CreateDocumentAttributes<DocAttribute>(doc, docDef, CissaDataType.Doc);
            CreateDocumentAttributes<DocListAttribute>(doc, docDef, CissaDataType.DocList);
            CreateDocumentAttributes<BoolAttribute>(doc, docDef, CissaDataType.Bool);
            CreateDocumentAttributes<DateTimeAttribute>(doc, docDef, CissaDataType.DateTime);
            CreateDocumentAttributes<AutoAttribute>(doc, docDef, CissaDataType.Auto);
            CreateDocumentAttributes<OrganizationAttribute>(doc, docDef, CissaDataType.Organization);
            CreateDocumentAttributes<DocumentStateAttribute>(doc, docDef, CissaDataType.DocumentState);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.DocumentDef);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.User);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.OrgPosition);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.OrgUnit);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.EnumDef);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.Form);
            CreateDocumentAttributes<ObjectDefAttribute>(doc, docDef, CissaDataType.Process);
        }

        public bool HasLockedRefs(Guid docId)
        {
            // DONE: Remove IEntityDataContext access
            /*var refDocIds =
                DataContext.GetEntityDataContext().Entities.Document_Attributes.Where(a => a.Value == docId && a.Expired == MaxDate)
                    .Select(
                        a => a.Document_Id);*/
            var docRefs = _attrStorage.GetDocRefs(docId, DocRefSourceType.DocAttribute);

            foreach (var docRef in docRefs)
            {
                var state = GetDocState(docRef.DocumentId);
                if (state != null && state.Type.ReadOnly) return true;
                if (HasLockedRefs(docRef.DocumentId)) return true;
            }
            /*
            refDocIds =
                DataContext.DocumentListAttributes.Where(a => a.Value == docId && a.Expired == MaxDate)
                    .Select(
                        a => a.Document_Id);

            foreach (var refDocId in refDocIds)
            {
                var state = GetDocState(refDocId);
                if (state != null && state.Type.ReadOnly) return true;
                if (HasLockedRefs(refDocId)) return true;
            }*/
            return false;
        }

        public bool HasRefs(Guid docId)
        {
            return _attrStorage.HasRefToDoc(docId);
        }

        public void CheckToSave(Doc document)
        {
            var state = GetDocState(document.Id);

            if (state != null && state.Type.ReadOnly)
                throw new ApplicationException(String.Format("Документ находится в состоянии \"{0}\" и не может быть изменен!", state.Type.Name));

            foreach (var attr in document.Attributes)
            {
                if (attr.AttrDef.IsNotNull && attr.ObjectValue == null) 
                    throw new ApplicationException(String.Format("Атрибут \"{0}\" не может быть пустым!", attr.AttrDef.Caption ?? attr.AttrDef.Id.ToString()));
            }

            foreach (var attr in document.Attributes.Where(a => a.AttrDef.IsUnique))
            {
                if (attr.ObjectValue != null)
                {
                    if (!_attrStorage.CheckUniqueness(document.Id, attr.AttrDef, attr.ObjectValue))
                        throw new ApplicationException(String.Format("Значение \"{0}\" атрибута \"{1}\" не является уникальным!", attr.ObjectValue, attr.AttrDef.Caption));
                }
            }

            //if (HasLockedRefs(document.Id)) 
            //    throw new ApplicationException("Документ заблокирован другим документом и не может быть изменен!");
        }

        public bool CanBeChanged(Doc document)
        {
            var state = GetDocState(document.Id);

            if (state.Type.ReadOnly) return false;
            if (HasLockedRefs(document.Id)) return false;

            return true;
        }

        public List<AttributeBase> CompareAttributes(Doc source, Doc target)
        {
            return (from attr in target.Attributes
                let attr1 = attr
                let dbAttr = source.Attributes.FirstOrDefault(a => a.AttrDef.Id == attr1.AttrDef.Id)
                where !AttributeRepository.SameAttributeValue(attr1, dbAttr)
                select attr).ToList();
        }

        public List<AttributeBase> GetChangedAttributes(Doc doc)
        {
            if (DocIsStored(doc))
            {
                var dbDoc = LoadById(doc.Id);

                return CompareAttributes(dbDoc, doc);
            }
            return new List<AttributeBase>(doc.Attributes);
        }

        public void Validate(Doc doc)
        {
            
        }

        public void SaveDocAttributes(Doc doc, List<AttributeBase> attributes, DateTime saveTime = new DateTime())
        {
            var monitor = new Monitor("Save Document Attributes To Database");
            try
            {
                if (attributes == null || attributes.Count == 0) return;

                var now = saveTime == DateTime.MinValue ? DateTime.Now : saveTime;

                _docStorage.Save(doc, GetUserInfo(), now);

                foreach (var attr in attributes)
                {
                    if (attr.AttrDef.Type.Id == (short) CissaDataType.DocList)
                    {
                        var docListAttr = (DocListAttribute) attr;

                        if (docListAttr.AddedDocs != null)
                            foreach (var itemDoc in docListAttr.AddedDocs)
                            {
                                Save(itemDoc, now);
                            }
                    }
                    _attrStorage.Save(doc.Id, attr, UserId, now);
                }
                monitor.Complete();
            }
            catch (Exception e)
            {
                monitor.CompleteWithException(e);
                throw;
            }
        }

        private UserInfo _userInfo;

        private UserInfo GetUserInfo()
        {
            return _userInfo ?? (_userInfo = _userRepo.GetUserInfo(UserId));
        }

        /// <summary>
        /// Сохраняет документ в БД
        /// </summary>
        /// <param name="docForSave">Сохраняемый документ</param>
        /// <returns>Сохраненный документ</returns>
        public Doc Save(Doc docForSave)
        {
            return Save(docForSave, DateTime.Now);
        }

        /// <summary>
        /// Сохраняет документ в БД
        /// </summary>
        /// <param name="docForSave">Сохраняемый документ</param>
        /// <param name="saveTime">Время сохранения</param>
        /// <returns>Сохраненный документ</returns>
        public Doc Save(Doc docForSave, DateTime saveTime)
        {
            using (var saveMonitor = new Monitor("Save Document"))
            {
                CheckToSave(docForSave); // Проверка документа на возможность сохранения

                var changedAttributes = GetChangedAttributes(docForSave);

                DataContext.BeginTransaction();
                try
                {
                    if (changedAttributes != null)
                        SaveDocAttributes(docForSave, changedAttributes, saveTime);

                    if (docForSave.State != null)
                    {
                        var docState = GetDocState(docForSave.Id);

                        if (docState == null || docState.Type.Id != docForSave.State.Type.Id)
                        {
                            SetDocState(docForSave.Id, docForSave.State.Type.Id);
                        }
                    }

                    using(new Monitor(saveMonitor, "Write Document Content"))
                    {
                        WriteDocContent(docForSave);
                    }
                    WriteDocToTables(docForSave);

                    //DataContext.SaveChanges();

                    docForSave.IsNew = false;

                    DocCache.Add(CloneDoc(docForSave), docForSave.Id);

                    DataContext.Commit();
                }
                catch
                {
                    DataContext.Rollback();
                    throw;
                }
                return docForSave;
            }
        }

        private void WriteDocContent(Doc doc)
        {
            var monitor = new Monitor("External Document Cache", "Write Document Content");
            try
            {
                var content = Serialize(doc);

                var docDefMonitor = new Monitor(monitor, doc.DocDef.Name);
                try
                {
                    DataContext.SaveToCache(doc.Id, content);
                    docDefMonitor.Complete();
                }
                catch (Exception e)
                {
                    docDefMonitor.CompleteWithException(e);
                    throw;
                }
                monitor.Complete();
            }
            catch(Exception e)
            {
                Logger.OutputLog(e, "Ошибка при сохранении документа в кеше");
                monitor.CompleteWithException(e);
            }
        }

        private void WriteDocToTables(Doc doc)
        {
            WriteDocToTable(doc, doc.DocDef);
        }

        private void WriteDocToTable(Doc doc, DocDef docDef)
        {
            var tableMap = _tableMapRepo.Find(docDef.Id);

            if (tableMap != null && !tableMap.IsView)
            {
                var monitor = new Monitor("Save Document", "Write Document To Table", docDef.Name);
                try
                {
                    _docStorage.WriteDocToTable(tableMap, doc);
                    monitor.Complete();
                }
                catch (Exception e)
                {
                    monitor.CompleteWithException(e);
                    throw;
                }
            }

            if (docDef.AncestorId != null)
            {
                var ancestorDef = _docDefRepo.DocDefById((Guid) docDef.AncestorId);
                WriteDocToTable(doc, ancestorDef);
            }
        }

        /// <summary>
        /// Создает новый документ
        /// </summary>
        /// <param name="docDefName">Наименование класса документа</param>
        /// <returns>Новый не инициализированный документ</returns>
        public Doc New(string docDefName)
        {
            var docDef = _docDefRepo.DocDefByName(docDefName);

            return New(docDef.Id);
        }

        /// <summary>
        /// Создает новый документ
        /// </summary>
        /// <param name="docDefId">Идентификатор описания документа</param>
        /// <returns>Новый не инициализированный документ</returns>
        public Doc New(Guid docDefId)
        {
            /*
                        var permissions = _permissionRepo.ListOfAccessibleObjects(UserId).GetPermissionsForObjectId(docDefId);
                        if (!permissions.AllowInsert && !permissions.AllowUpdate && UserId != Guid.Empty)
                        {
                            throw new ApplicationException(
                                string.Format("Пользователь {0} не имеет разрешений для создания документа типа {1}", UserId, docDefId));
                        }
            */

            /*var documentDefs = from def in em.Object_Defs.OfType<Document_Def>()
                                   where def.Id == docDefId && (def.Deleted == null || def.Deleted == false)
                                   select def;
            
                if (!documentDefs.Any())
                {
                    throw new ApplicationException(
                        string.Format("Тип документа {0} не существует.", docDefId));
                }*/

            var docDef = _docDefRepo.DocDefById(docDefId);

            return New(docDef);
        }

        public Doc New(DocDef docDef)
        {
            var userInfo = GetUserInfo();

            var returnDoc = new Doc
                                {
                                    Id = Guid.NewGuid(),
                                    DocDef = docDef
                                    /*new DocDef
                                                         {
                                                             Id = docDef.Id,
                                                             Name = docDef.Name,
                                                             WithHistory = docDef.WithHistory
                                                         }*/,
                                    Attributes = new List<AttributeBase>(),
                                    IsNew = true,
                                    UserId = UserId,
                                    OrganizationId = userInfo != null ? userInfo.OrganizationId : null,
                                    PositionId = userInfo != null ? userInfo.PositionId : null,
                                    CreationTime = DateTime.Now,
                                    DataContextName = DataContext.Name
                                };

            var allDocAttributes = from attrDef in docDef.Attributes
                                   //defRepo.GetDocumentAttributes(docDefId, UserId)
                                   //where attrDef.Permissions.AllowInsert || attrDef.Permissions.AllowUpdate
                                   select attrDef;

            var docAttributes = allDocAttributes as AttrDef[] ?? allDocAttributes.ToArray();

            docAttributes.Where(a => a.Type.Id == (short) CissaDataType.Currency)
                .ToList().ForEach(item => 
                {
                    var attr = new CurrencyAttribute(item);
                    if (!String.IsNullOrEmpty(item.DefaultValue))
                    {
                        decimal d;
                        if (Decimal.TryParse(item.DefaultValue, out d))
                            attr.Value = d;
                    }
                    returnDoc.Attributes.Add(attr);
                }
            );

            docAttributes.Where(a => a.Type.Id == (short)CissaDataType.Int)
                .ToList().ForEach(item =>
                    {
                        var attr = new IntAttribute(item);
                        returnDoc.Attributes.Add(attr);
                        if (!String.IsNullOrEmpty(item.DefaultValue))
                        {
                            int d;
                            if (Int32.TryParse(item.DefaultValue, out d))
                                attr.Value = d;
                        }
                    });

            docAttributes.Where(a => a.Type.Id == (short)CissaDataType.Text)
                .ToList().ForEach(item =>
                {
                    var attr = new TextAttribute(item);
                    returnDoc.Attributes.Add(attr);
                    if (!String.IsNullOrEmpty(item.DefaultValue))
                    {
                        attr.Value = item.DefaultValue;
                    }
                });

            docAttributes.Where(a => a.Type.Id == (short) CissaDataType.Blob)
                .ToList().ForEach(item => returnDoc.Attributes.Add(new BlobAttribute(item)));

            docAttributes.Where(a => a.Type.Id == (short)CissaDataType.Float)
                .ToList().ForEach(item =>
                    {
                        var attr = new FloatAttribute(item);
                        returnDoc.Attributes.Add(attr);
                        if (!String.IsNullOrEmpty(item.DefaultValue))
                        {
                            double d;
                            if (Double.TryParse(item.DefaultValue, out d))
                                attr.Value = d;
                        }
                    });

            docAttributes.Where(a => a.Type.Id == (short) CissaDataType.Enum)
                .ToList().ForEach(item => returnDoc.Attributes.Add(new EnumAttribute(item)));

            docAttributes.Where(a => a.Type.Id == (short)CissaDataType.Doc)
                .ToList().ForEach(item =>
                                      {
                                          var docAttr = new DocAttribute(item);

                                          returnDoc.Attributes.Add(docAttr);

                                          if (item.DocDefType != null)
                                          {
                                              var attrDocDef = _docDefRepo.DocDefById(item.DocDefType.Id);
                                              if (attrDocDef != null && attrDocDef.IsInline)
                                                  docAttr.Document = New(item.DocDefType.Id);
                                          }
                                      });

            docAttributes.Where(a => a.Type.Id == (short) CissaDataType.DocList)
                .ToList().ForEach(item => returnDoc.Attributes.Add(new DocListAttribute(item)));

            docAttributes.Where(a => a.Type.Id == (short)CissaDataType.Bool)
                .ToList().ForEach(item =>
                {
                    var attr = new BoolAttribute(item);
                    returnDoc.Attributes.Add(attr);
                    if (!String.IsNullOrEmpty(item.DefaultValue))
                    {
                        bool d;
                        if (Boolean.TryParse(item.DefaultValue, out d))
                            attr.Value = d;
                    }
                });

            docAttributes.Where(a => a.Type.Id == (short)CissaDataType.DateTime)
                .ToList().ForEach(item =>
                    {
                        var attr = new DateTimeAttribute(item);
                        returnDoc.Attributes.Add(attr);
                        if (!String.IsNullOrEmpty(item.DefaultValue))
                        {
                            DateTime d;
                            if (DateTime.TryParse(item.DefaultValue, out d))
                                attr.Value = d;
                        }
                    });

            docAttributes.Where(a => a.Type.Id == (short) CissaDataType.Auto)
                .ToList().ForEach(item => returnDoc.Attributes.Add(new AutoAttribute(item)));

            docAttributes.Where(a => a.Type.Id == (short) CissaDataType.Organization)
                .ToList().ForEach(item => returnDoc.Attributes.Add(new OrganizationAttribute(item)));

            docAttributes.Where(a => a.Type.Id == (short) CissaDataType.DocumentState)
                .ToList().ForEach(item => returnDoc.Attributes.Add(new DocumentStateAttribute(item)));

            docAttributes.Where(a => a.Type.Id == (short)CissaDataType.User)
                .ToList().ForEach(item => returnDoc.Attributes.Add(new ObjectDefAttribute(item)));

//            DocCache.Add(returnDoc, returnDoc.Id);

            return returnDoc;
        }

        public Doc InitDocFrom(Doc doc, IStringParams prms)
        {
            if (prms == null) return doc;

            foreach (var attr in doc.Attributes)
            {
                var value = prms.Get(attr.AttrDef.Name);

                if (!String.IsNullOrEmpty(value))
                {
                    var oldValue = attr.ObjectValue;
                    try
                    {
                        attr.ObjectValue = value;
                    }
                    catch
                    {
                        attr.ObjectValue = oldValue;
                    }
                }
            }
            return doc;
        }

        public void AssignDocTo(Doc source, Doc target)
        {
            /*foreach(var attr in source.Attributes)
            {
                var sAttr = attr;
                var tAttr = target.Attributes.FirstOrDefault(a => a.AttrDef.Id == sAttr.AttrDef.Id);

                if (tAttr != null)
                {
                    tAttr.ObjectValue = sAttr.ObjectValue;
                }
            }*/
            source.Assign(target);
        }

        private static List<AttributeBase> ExcludeVirtualAttributes(IEnumerable<AttributeBase> attributes)
        {
            return attributes.Where(attr => !(attr is MetaInfoAttribute)).ToList();
        }

        /// <summary>
        /// Загружает документ из БД по имени класса документа
        /// </summary>
        /// <param name="docDefName">Имя класса загружаемого документа</param>
        /// <returns>Загруженный документ</returns>
        public Doc LoadByDefName(string docDefName)
        {
            var docDef = _docDefRepo.DocDefByName(docDefName);

            if (docDef != null)
                return LoadById(docDef.Id, new DateTime(9999, 12, 31));

            throw new ApplicationException(string.Format("Документ \"{0}\" не существует.", docDefName));
        }

        /// <summary>
        /// Загружает документ из БД по идентификатору документа
        /// </summary>
        /// <param name="documentId">Идентификатор загружаемого документа</param>
        /// <returns>Загруженный документ</returns>
        public Doc LoadById(Guid documentId)
        {
            return LoadById(documentId, MaxDate);
        }

        /// <summary>
        /// Загружает документ из БД по идентификатору документа
        /// </summary>
        /// <param name="documentId">Идентификатор загружаемого документа</param>
        /// <param name="forDate"></param>
        /// <returns>Загруженный документ</returns>
        public Doc LoadById(Guid documentId, DateTime forDate)
        {
            using (var loadMonitor = new Monitor("Load Document"))
            {
                Doc returnDoc;
                if (forDate == MaxDate)
                {
                    var cached = DocCache.Find(documentId);
                    if (cached != null)
                    {
                        returnDoc = CloneDoc(cached.CachedObject);

                        if (returnDoc.DocDef == null)
                            throw new ApplicationException("Ошибка в документе! Класс документа не определен!");

                        var defId = returnDoc.DocDef.Id;

                        returnDoc.DocDef = _docDefRepo.DocDefById(defId);
                        returnDoc.State = GetDocState(documentId);
                        returnDoc.DataContextName = DataContext.Name;

                        CheckDocumentAttributes(returnDoc, returnDoc.DocDef);
                        CreateVirtualAttributes(returnDoc);

                        CalculateAutoAttributes(returnDoc);

                        return returnDoc;
                    }
                }

                if (forDate == MaxDate)
                {
                    try
                    {
                        string content;
                        using (new Monitor(loadMonitor, "Load From Cache"))
                        {
                            content = DataContext.LoadFromCache(documentId);
                        }
 
                        if (!String.IsNullOrEmpty(content))
                        {
                            returnDoc = Deserialize(content);
                            if (returnDoc.DocDef == null)
                                throw new ApplicationException("Ошибка в документе! Класс документа не определен!");

                            var defId = returnDoc.DocDef.Id;

                            returnDoc.DocDef = _docDefRepo.DocDefById(defId);
                            returnDoc.State = GetDocState(documentId);
                            returnDoc.DataContextName = DataContext.Name;

                            CheckDocumentAttributes(returnDoc, returnDoc.DocDef);
                            CreateVirtualAttributes(returnDoc);

                            CalculateAutoAttributes(returnDoc);

                            if (forDate == MaxDate)
                                DocCache.Add(CloneDoc(returnDoc), documentId);

                            return returnDoc;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                DocData document;
                using (new Monitor(loadMonitor, "Read Document Data"))
                {
                    document = _docStorage.Load(documentId);
                }

                if (document == null)
                    throw new ApplicationException(string.Format("Документ {0} не существует.", documentId));

                returnDoc = new Doc
                {
                    IsNew = false,
                    Id = documentId,
                    CreationTime = document.Created ?? DateTime.MinValue,
                    DocDef = _docDefRepo.DocDefById(document.DefId ?? Guid.Empty),
                    OrganizationId = document.OrganizationId,
                    PositionId = document.PositionId,
                    UserId = document.UserId,
                    Attributes = new List<AttributeBase>(),
                    ModifiedTime = document.LastModified ?? DateTime.MinValue,
                    State = GetDocState(document.Id),
                    DataContextName = DataContext.Name
                };

                // только атрибуты с доступом на выборку - SELECT
                var defAttrWithSelect = from attrDef in returnDoc.DocDef.Attributes
                    //defRepo.GetDocumentAttributes(returnDoc.DocDef.Id, UserId)
                    //                                    where attrDef.Permissions.AllowSelect
                    select attrDef;


                using (new Monitor(loadMonitor, "Read Attributes"))
                {
                    LoadAttributes(returnDoc, defAttrWithSelect.ToList(), forDate);
                }
                CheckDocumentAttributes(returnDoc, returnDoc.DocDef);

                using(new Monitor(loadMonitor, "Write Document Content"))
                {
                    WriteDocContent(returnDoc);
                }

                CreateVirtualAttributes(returnDoc);

                CalculateAutoAttributes(returnDoc);

                if (forDate == MaxDate)
                    DocCache.Add(CloneDoc(returnDoc), documentId);

                return returnDoc;
            }
        }

        private void LoadAttributes(Doc doc, ICollection<AttrDef> attrDefs, DateTime forDate)
        {
            // TODO: Add IAttributeStorage method LoadAll(docId, forDate)
            foreach (var data in _attrStorage.LoadAll(doc.Id))
            {
                var attrDef = attrDefs.FirstOrDefault(a => data.DefId == a.Id);

                if (attrDef == null) continue;

                switch ((CissaDataType)attrDef.Type.Id)
                {
                    case CissaDataType.Text:
                        if (data.DataType != 3) continue;
                        doc.Attributes.Add(new TextAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                        ? data.Value.ToString()
                                        : String.Empty
                        });
                        break;
                    case CissaDataType.Currency:
                        if (data.DataType != 2) continue;
                        doc.Attributes.Add(new CurrencyAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                        ? Convert.ToDecimal(data.Value)
                                        : (decimal?)null
                        });
                        break;
                    case CissaDataType.Int:
                        if (data.DataType != 1) continue;
                        doc.Attributes.Add(new IntAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                        ? Convert.ToInt32(data.Value)
                                        : (int?)null
                        });
                        break;
                    case CissaDataType.Float:
                        if (data.DataType != 4) continue;
                        doc.Attributes.Add(new FloatAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                        ? Convert.ToDouble(data.Value)
                                        : (double?)null
                        });
                        break;
                    case CissaDataType.DateTime:
                        if (data.DataType != 9) continue;
                        doc.Attributes.Add(new DateTimeAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                        ? Convert.ToDateTime(data.Value)
                                        : (DateTime?)null
                        });
                        break;
                    case CissaDataType.Bool:
                        if (data.DataType != 8) continue;
                        doc.Attributes.Add(new BoolAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                        ? Convert.ToBoolean(data.Value)
                                        : (bool?)null
                        });
                        break;
                    case CissaDataType.Doc:
                        if (data.DataType != 6) continue;
                        doc.Attributes.Add(new DocAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                        ? Guid.Parse(data.Value.ToString())
                                        : (Guid?)null
                        });
                        break;
                    case CissaDataType.Enum:
                        if (data.DataType != 5) continue;
                        doc.Attributes.Add(new EnumAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                        ? Guid.Parse(data.Value.ToString())
                                        : (Guid?)null
                        });
                        break;
                    case CissaDataType.Organization:
                        if (data.DataType != 12) continue;
                        doc.Attributes.Add(new OrganizationAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                        ? Guid.Parse(data.Value.ToString())
                                        : (Guid?)null
                        });
                        break;
                    case CissaDataType.DocumentState:
                        if (data.DataType != 13) continue;
                        doc.Attributes.Add(new DocumentStateAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                        ? Guid.Parse(data.Value.ToString())
                                        : (Guid?)null
                        });
                        break;
                    case CissaDataType.Blob:
                        if (data.DataType != 15) continue;
                        doc.Attributes.Add(new BlobAttribute(attrDef)
                        {
                            Created = data.Created,
                            FileName = data.Value != null
                                ? data.Value.ToString()
                                : String.Empty,
                            HasValue = true
                        });
                        break;
                    case CissaDataType.User:
                    case CissaDataType.OrgUnit:
                    case CissaDataType.OrgPosition:
                    case CissaDataType.DocumentDef:
                    case CissaDataType.EnumDef:
                    case CissaDataType.Form:
                    case CissaDataType.Process:
                        if (data.DataType != 14) continue;
                        doc.Attributes.Add(new ObjectDefAttribute(attrDef)
                        {
                            Created = data.Created,
                            Value = data.Value != null
                                ? Guid.Parse(data.Value.ToString())
                                : (Guid?)null
                        });
                        break;
                    /*case CissaDataType.DocList:
                        doc.Attributes.Add(new DocListAttribute(attrDef));
                        break;*/
                }
            }

        }

        private static Doc CloneDoc(Doc doc)
        {
            return Deserialize(Serialize(doc));
        }

        private static Doc Deserialize(string data)
        {
            using (var read = new StringReader(data))
            {
                var serializer = new XmlSerializer(typeof(Doc));
                using (var reader = new XmlTextReader(read))
                {
                    return (Doc)serializer.Deserialize(reader);
                }
            }
        }

/*
        private static Doc FastDeserialize(string data)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(data);
                    writer.Flush();
                    stream.Position = 0;
                    return Serializer.Deserialize<Doc>(stream);
                }
            }
        }
*/
/*
        private static string FastSerialize(Doc doc)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, doc);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
*/

        private static string Serialize(Doc doc)
        {
            var serializer = new XmlSerializer(typeof(Doc));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, doc);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }

        private void CreateVirtualAttributes(Doc doc)
        {
            foreach (var attrDef in doc.DocDef.Attributes)
            {
                var aDef = attrDef;
                var attr = doc.Attributes.FirstOrDefault(a => a.AttrDef.Id == aDef.Id) as MetaInfoAttribute;

                switch (attrDef.Type.Id)
                {
                    case (short) CissaDataType.OrganizationOfDocument:
                        if (attr == null) attr = new MetaInfoAttribute(attrDef);
                        if (doc.OrganizationId != null)
                        {
                            var orgInfo = _orgRepo.Get((Guid) doc.OrganizationId);
                            attr.Value = orgInfo.Name;
                        }
                        break;
                    case (short) CissaDataType.StateOfDocument:
                        if (attr == null) attr = new MetaInfoAttribute(attrDef);
                        attr.Value = (doc.State != null && doc.State.Type != null)
                            ? doc.State.Type.Name
                            : String.Empty;  //"[Статус не установлен]";
                        break;
                    case (short) CissaDataType.ClassOfDocument:
                        if (attr == null) attr = new MetaInfoAttribute(attrDef);
                        attr.Value = doc.DocDef.Name;
                        break;
                    case (short) CissaDataType.DocumentId:
                        if (attr == null) attr = new MetaInfoAttribute(attrDef);
                        attr.Value = doc.Id;
                        break;
                    case (short) CissaDataType.CreateTimeOfDocument:
                        if (attr == null) attr = new MetaInfoAttribute(attrDef);
                        attr.Value = doc.CreationTime;
                        break;
                    case (short) CissaDataType.AuthorOfDocument:
                        var userInfo = _userRepo.FindUserInfo(doc.UserId);

                        if (attr == null) attr = new MetaInfoAttribute(attrDef);
                        attr.Value = (userInfo != null)
                                         ? String.Format("{0} {1} {2}", userInfo.LastName, userInfo.FirstName,
                                                         userInfo.MiddleName)
                                         : String.Empty;
                        break;
                    case (short) CissaDataType.OrgUnitOfDocument:
                        if (attr == null) attr = new MetaInfoAttribute(attrDef);
                        attr.Value = (doc.PositionId != null)
                                         ? _orgRepo.GetOrgPositionName((Guid) doc.PositionId)
                                         : String.Empty;
                        break;
                }
                if (attr != null) doc.Attributes.Add(attr);
            }
        }
/*

        /// <summary>
        /// Загружает список документов из БД по идентификатору формы
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <returns>Список загруженных документов</returns>
        [Obsolete("Устаревший метод")]
        public IList<Doc> LoadByFormId(Guid formId)
        {
            var formQuery = DataContext.Entities.Object_Defs.OfType<Table_Form>().Where(f => f.Id == formId);

            if (!formQuery.Any())
                throw new ApplicationException(string.Format("Формы {0} не существует.", formId));

            var dbForm = formQuery.First();

            var docsQuery = from doc in dbForm.Document_Defs.Documents
                            where doc.Deleted != true
                            select doc.Id;

            var docs = docsQuery.Select(LoadById).ToList();
            return docs.Any() ? docs : null;
        }
*/
        public bool TryHideDoc(Guid documentId)
        {
            if (HasRefs(documentId)) return false;

            if (DocExists(documentId))
            {
                /*var docForDelete = DataContext.Documents.FirstOrDefault(doc => doc.Id == documentId);

                if (docForDelete == null)
                    throw new ApplicationException(string.Format("Документа с идентификатором {0} не существует.",
                        documentId));*/

                _docStorage.Hide(documentId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Скрывает документ, делает его невидимым
        /// </summary>
        /// <param name="documentId">Ключ документа</param>
        public void HideById(Guid documentId)
        {
            if (!TryHideDoc(documentId))
                throw new ApplicationException("Не могу удалить документ! Имеются ссылки на документ!");
        }

        /// <summary>
        /// Удаляет документ из БД по идентификатору документа
        /// </summary>
        /// <param name="documentId">Идентификатор загружаемого документа</param>
        public void DeleteById(Guid documentId)
        {
            DataContext.BeginTransaction();
            try
            {
                _attrStorage.DeleteAttributes(documentId);
                _docStorage.DeleteDocStates(documentId);
                _docStorage.Delete(documentId);

                DataContext.Commit();
            }
            catch
            {
                DataContext.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Возвращает историю состояний документа
        /// </summary>
        /// <param name="docId">Идентификатор документа</param>
        /// <returns>Список состояний документа</returns>
        public List<DocState> GetDocumentStates(Guid docId)
        {
            // DONE: Remove IEntityDataContext retreave. Change with DocumentStorage method call
            /*var edc = DataContext.GetEntityDataContext();
            
            var states =
                edc.Entities.Document_States.Include("Document_State_Types").Include("Workers")
                    .Where(s => s.Document_Id == docId).OrderBy(s => s.Created);
            var stateList = states.Select(state => new DocState
                                                       {
                                                           Id = state.Id,
                                                           Created = state.Created,
                                                           WorkerId = state.Worker_Id,
                                                           Type = new DocStateType
                                                                      {
                                                                          Id = state.State_Type_Id,
                                                                          Name =
                                                                              state.Document_State_Types.Full_Name,
                                                                          ReadOnly =
                                                                              state.Document_State_Types.Read_Only ??
                                                                              false
                                                                      },
                                                           WorkerName =
                                                               (state.Worker.Last_Name ?? "") + " " +
                                                               (state.Worker.First_Name ?? "") + " " +
                                                               (state.Worker.Middle_Name ?? "")
                                                       }).ToList();*/
            var docStateDatas = new List<DocStateData>();
            _docStorage.FillDocStates(docId, docStateDatas);

            return (from state in docStateDatas
                let docStateType = _docStateRepo.LoadById(state.StateTypeId)
                let userInfo = _userRepo.FindUserInfo(state.UserId)
                select new DocState
                {
                    Id = state.Id, Created = state.Created, WorkerId = state.UserId, 
                    Type = new DocStateType
                    {
                        Id = state.StateTypeId, Name = docStateType.Name, ReadOnly = docStateType.ReadOnly
                    },
                    WorkerName = userInfo != null ? userInfo.LastName + " " + userInfo.FirstName + " " + userInfo.MiddleName : ""
                }).ToList();
        }

        /// <summary>
        /// Выполняет проверку переданного документа 
        /// </summary>
        /// <param name="document">Проверяемый документ</param>
        /// <returns>Проверенный и по возможности исправленный документ</returns>
        public Doc Check(Doc document)
        {
            // Добавить логику проверки документа если необходимо

            return document;
        }

        /// <summary>
        /// Подсчитывает автовычисляемые атрибуты
        /// </summary>
        /// <param name="doc">Документ в котором необходимо вычислить автовычисляемые атрибуты</param>
        /// <returns>Документ с вычисленными авто атрибутами</returns>
        public Doc CalculateAutoAttributes(Doc doc)
        {
            var context = new AutoAttributeContext
                              {
                                  CurrentDoc = doc, 
                                  CurrentUserId = UserId
                              };

            if (doc.AttrAuto.Any())
                foreach (var autoAttribute in doc.AttrAuto)
                {
                    var script = new AutoAttributeScriptManager(autoAttribute.AttrDef.Script);
                    autoAttribute.Value = script.Execute(context);
                }

            return doc;
        }

        public void ReplaceRefToDoc(Guid docId1, Guid docId2)
        {
            var docRefs = _attrStorage.GetDocRefs(docId1, DocRefSourceType.All);

            if (docRefs != null && docRefs.Count > 0)
            {
                var now = DateTime.Now;
                DataContext.BeginTransaction();
                try
                {
                    foreach (var docRef in docRefs)
                    {
                        if (!docRef.IsList)
                        {
                            var docDef = _docDefRepo.DocDefById(docRef.DocumentDefId);
                            var attrDef = docDef.Attributes.FirstOrDefault(a => a.Id == docRef.AttributeDefId);

                            _attrStorage.DirectSave(docRef.DocumentId, attrDef, docId2, UserId, now, String.Empty);
                        }
                        else
                        {
                            _attrStorage.RemoveDocListItem(docRef.DocumentId, docRef.AttributeDefId, docId1, now);
                            _attrStorage.AddDocListItem(docRef.DocumentId, docRef.AttributeDefId, docId2, UserId, now);
                        }
                    }
                    DataContext.Commit();
                }
                catch 
                {
                    DataContext.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <returns>Список документов</returns>
        public List<Guid> List(out int count, Guid docDefId, int pageNo, int pageSize = 0)
        {
            var builder = new QueryBuilder(docDefId, UserId);

            var sqlQueryBuilder = Provider.Get<ISqlQueryBuilder>();
            var sqlReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
            using (var query = sqlQueryBuilder.Build(builder.Def))
            {
                query.AddAttribute("&Id");
                if (pageSize > 0)
                {
                    query.TopNo = pageSize;
                    query.SkipNo = pageNo * pageSize;
                }
                using (var reader = sqlReaderFactory.Create(query))
                {
                    var list = new List<Guid>();
                    count = reader.GetCount();

                    while (reader.Read())
                    {
                        list.Add(reader.GetGuid(0));
                    }
                    return list;
                }
            }
        }

        public List<Guid> List(out int count, Guid docDefId, int pageNo, int pageSize, IDictionary<string, object> filter, Guid? sortAttrId = null)
        {
            if (filter != null)
            {
                var filterDoc = New(docDefId);

                foreach (var item in filter)
                {
                    filterDoc[item.Key] = item.Value;
                }
                return List(out count, docDefId, pageNo, pageSize, filterDoc, sortAttrId);
            }
            return List(out count, docDefId, pageNo, pageSize, (Doc) null, sortAttrId);
        }

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        public List<Guid> List(out int count, Guid docDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttrId = null)
        {
            var queryDef = new QueryBuilder(docDefId, UserId).Def;

            if (filter != null)
                using (var engine = new QueryEngine(DataContext))
                    engine.AddConditionFromDoc(queryDef, filter);

            var sqlQueryBuilder = Provider.Get<ISqlQueryBuilder>();
            var sqlReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
            using (var query = sqlQueryBuilder.Build(queryDef))
            {
                query.AddAttribute("&Id");
                if (pageSize > 0)
                {
                    query.TopNo = pageSize;
                    query.SkipNo = pageNo*pageSize;
                }
                using (var reader = sqlReaderFactory.Create(query))
                {
                    var list = new List<Guid>();
                    count = reader.GetCount();

                    while (reader.Read())
                    {
                        list.Add(reader.GetGuid(0));
                    }
                    return list;
                }
            }
        }

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="docStateId">Статус документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        public List<Guid> List(out int count, Guid docDefId, Guid docStateId, int pageNo, int pageSize, Doc filter = null, Guid? sortAttrId = null)
        {
            var builder = new QueryBuilder(docDefId, UserId);

            builder.Where("&State").Eq(docStateId);
            if (sortAttrId != null)
                builder.AddOrderBy((Guid) sortAttrId, true);

            if (filter != null)
                using (var engine = new QueryEngine(DataContext))
                    engine.AddConditionFromDoc(builder.Def, filter);

            var sqlQueryBuilder = Provider.Get<ISqlQueryBuilder>();
            var sqlReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
            using (var query = sqlQueryBuilder.Build(builder.Def))
            {
                query.AddAttribute("&Id");
                if (pageSize > 0)
                {
                    query.TopNo = pageSize;
                    query.SkipNo = pageNo*pageSize;
                }
                using (var reader = sqlReaderFactory.Create(query))
                {
                    var list = new List<Guid>();
                    count = reader.GetCount();

                    while (reader.Read())
                        list.Add(reader.GetGuid(0));

                    return list;
                }
            }
        }

        [Obsolete("Устаревший метод")]
        public Guid? FirstDocId(Guid docDefId, IDictionary<string, object> filter)
        {
            if (filter != null)
            {
                var filterDoc = New(docDefId);

                foreach (var item in filter) filterDoc[item.Key] = item.Value;

                return FirstDocId(docDefId, filterDoc);
            }
            return FirstDocId(docDefId, (Doc) null);
        }

        [Obsolete("Устаревший метод")]
        public Guid? FirstDocId(Guid docDefId, Doc filter)
        {
            var en = DataContext.GetEntityDataContext().Entities;
            var defIds = _docDefRepo.GetDocDefDescendant(docDefId).ToList();

            var result = from doc in en.Documents
                             .Include("Text_Attributes")
                             .Include("Int_Attributes")
                             .Include("Float_Attributes")
                             .Include("Currency_Attributes")
                             .Include("Date_Time_Attributes")
                             .Include("Boolean_Attributes")
                             .Include("Enum_Attributes")
                             .Include("Org_Attributes")
                             .Include("Doc_State_Attributes")
                         where defIds.Contains(doc.Def_Id ?? Guid.Empty)
                               && (doc.Deleted == null || doc.Deleted == false)
                         select doc;

            if (filter != null)
                foreach (var attr in filter.Attributes)
                {
                    var defId = attr.AttrDef.Id;
                    var value = attr.ObjectValue;

                    if (attr is TextAttribute && value != null && !String.IsNullOrEmpty(value.ToString()))
                    {
                        var text = value.ToString();

                        result =
                            result.Intersect(
                                en.Text_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == defId && a.Value.Contains(text) && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                    }
                    else if (attr is IntAttribute && value != null)
                        result =
                            result.Intersect(
                                en.Int_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == defId && a.Value == (int) value && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                    else if (attr is FloatAttribute && value != null)
                        result =
                            result.Intersect(
                                en.Float_Attributes.Include("Documents")
                                    .Where(
                                        a => a.Def_Id == defId && Math.Abs(a.Value - (double) value) < 0.0001 && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                    else if (attr is CurrencyAttribute && value != null)
                        result =
                            result.Intersect(
                                en.Currency_Attributes.Include("Documents")
                                    .Where(
                                        a => a.Def_Id == defId && a.Value == (decimal) value && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                    else if (attr is EnumAttribute && value != null)
                        result =
                            result.Intersect(
                                en.Enum_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == defId && a.Value == (Guid) value && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                    else if (attr is DateTimeAttribute && value != null)
                        result =
                            result.Intersect(
                                en.Date_Time_Attributes.Include("Documents")
                                    .Where(
                                        a =>
                                        a.Def_Id == defId && a.Value == (DateTime) value && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                    else if (attr is OrganizationAttribute && value != null)
                        result =
                            result.Intersect(
                                en.Org_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == defId && a.Value == (Guid) value && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                    else if (attr is DocumentStateAttribute && value != null)
                        result =
                            result.Intersect(
                                en.Doc_State_Attributes.Include("Documents")
                                    .Where(a => a.Def_Id == defId && a.Value == (Guid) value && a.Expired >= MaxDate)
                                    .Select(a => a.Document));
                }

            var r = result.Select(d => d.Id);

            if (r.Any()) return r.First();
            return null;
        }

        [Obsolete("Устаревший метод")]
        public Doc FirstDoc(Guid docDefId, IDictionary<string, object> filter)
        {
            var docId = FirstDocId(docDefId, filter);

            return docId != null ? LoadById((Guid)docId) : null;
        }

        [Obsolete("Устаревший метод")]
        public Doc FirstDoc(Guid docDefId, Doc filter)
        {
            var docId = FirstDocId(docDefId, filter);

            return docId != null ? LoadById((Guid) docId) : null;
        }

        [Obsolete("Устаревший метод")]
        public IEnumerable<DocListItem> GetDocListAttrItems(Doc doc, AttrDef attrDef)
        {
            return GetDocListAttrItems(doc.Id, attrDef.Id);
        }

        [Obsolete("Устаревший метод")]
        public IEnumerable<DocListItem> GetDocListAttrItems(Guid docId, Guid attrDefId)
        {
            return DataContext.GetEntityDataContext().ExecQuery<DocListItem>(
                "SELECT [Created], [Value] FROM DocumentList_Attributes " +
                "WHERE [Document_Id] = {0} AND [Def_Id] = {1} AND [Expired] = '99991231' " +
                "ORDER BY [Created]", docId, attrDefId);
        }

        [Obsolete("Устаревший метод")]
        public IEnumerable<DocListItem> GetDocListAttrItems(Doc doc, string attrDefName)
        {
            var attr =
                doc.Attributes.First(
                    a => String.Equals(a.AttrDef.Name, attrDefName, StringComparison.OrdinalIgnoreCase));

            return GetDocListAttrItems(doc.Id, attr.AttrDef.Id);
        }

        /// <summary>
        /// Постраничная загрузка документов из атрибута-списка
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="document">Документ, содержащий атрибут-список</param>
        /// <param name="attrDefName">Наименование класса атрибута</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="filter">Фильтр</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <returns>Список документов</returns>
        public List<Guid> DocAttrList(out int count, Doc document, string attrDefName, int pageNo, int pageSize, Doc filter = null, Guid? sortAttrId = null)
        {
            var attr = document.Get<DocListAttribute>(attrDefName);

            return DocAttrList(out count, document, attr, pageNo, pageSize, filter, sortAttrId);
        }

        /// <summary>
        /// Постраничная загрузка документов из атрибута-списка
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="document">Документ, содержащий атрибут-список</param>
        /// <param name="attrDefId">Класс атрибута</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="filter">Фильтр</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <returns>Список документов</returns>
        public List<Guid> DocAttrList(out int count, Doc document, Guid attrDefId, int pageNo, int pageSize, Doc filter = null, Guid? sortAttrId = null)
        {
            var docAttr = document.AttrDocList.FirstOrDefault(a => a.AttrDef.Id == attrDefId);

            return DocAttrList(out count, document, docAttr, pageNo, pageSize, filter, sortAttrId);
        }

        public List<Guid> DocAttrList(out int count, Doc document, DocListAttribute docAttr, int pageNo, int pageSize, Doc filter = null, Guid? sortAttrId = null)
        {
            if (docAttr == null)
            {
                count = 0;
                return new List<Guid>();
            }
            if (docAttr.AttrDef.DocDefType == null)
            {
                var docDef = _docDefRepo.DocDefById(document.DocDef.Id);
                docAttr.AttrDef = docDef.Attributes.FirstOrDefault(a => a.Id == docAttr.AttrDef.Id);
                if (docAttr.AttrDef == null || docAttr.AttrDef.DocDefType == null)
                {
                    count = 0;
                    return new List<Guid>();
                }
            }
            // BEGIN: Добавлено 25012013
            using (var query = new SqlQuery(document, docAttr, UserId, Provider)) // DONE: Провайдер должен быть установлен в конструкторах
            {
                query.AddAttribute("&Id");
                if (filter != null)
                    SqlQueryExBuilder.AddDocConditions(query, query.Source, filter);
                if (sortAttrId != null)
                    query.AddOrderAttribute((Guid) sortAttrId);

                var resList = new List<Guid>();

                var sqlReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
                using (var reader = sqlReaderFactory.Create(query))
                {
                    count = reader.GetCount();

                    while (reader.Read())
                        resList.Add(reader.GetGuid(0));
                }
                return resList;
            }
            // END: Добавлено 25012013
        }

        /// <summary>
        /// Постраничная загрузка документов из атрибута-списка
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docId">ID Документа</param>
        /// <param name="attrDefId">Класс атрибута</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="filter">Фильтр</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <returns>Список документов</returns>
        public List<Guid> DocAttrListById(out int count, Guid docId, Guid attrDefId, int pageNo, int pageSize, Doc filter = null, Guid? sortAttrId = null)
        {
            // BEGIN: Добавлено 25012013
            var document = LoadById(docId);
            if (document == null)
            {
                count = 0;
                return new List<Guid>();
            }
            var docAttr = document.AttrDocList.First(a => a.AttrDef.Id == attrDefId);

            if (docAttr == null)
            {
                count = 0;
                return new List<Guid>();
            }
            return DocAttrList(out count, document, docAttr, pageNo, pageSize, filter, sortAttrId);
            // END: Добавлено 25012013
        }

        // TODO: Вывести метод в Service/Helper class >> DocValueProvider
        public object GetDocumentValue(Doc document, SystemIdent ident)
        {
            if (document == null)
                return null;

            switch (ident)
            {
                case SystemIdent.Id:
                    return document.Id;
                case SystemIdent.UserId:
                    return document.UserId;
                case SystemIdent.Created:
                    return document.CreationTime;
                case SystemIdent.OrgId:
                    return document.OrganizationId;
                case SystemIdent.UserName:
                    var userInfo = _userRepo.FindUserInfo(document.UserId);
                    if (userInfo != null)
                        return String.Format("{0} {1} {2}", userInfo.LastName, userInfo.FirstName,
                                             userInfo.MiddleName);
                    return String.Empty;
                case SystemIdent.OrgName:
                    return document.OrganizationId != null
                               ? _orgRepo.GetOrgName((Guid) document.OrganizationId)
                               : "[Не указана]";
                case SystemIdent.DefId:
                    return document.DocDef.Id;
                case SystemIdent.State:
                    return (document.State != null) ? document.State.Type.Name : String.Empty;
                case SystemIdent.StateDate:
                    return (document.State != null) ? document.State.Created : null;
                default:
                    return null;
            }
        }

        public List<Guid> GetDocListRefTo(Guid docId, Guid targetDefId, string attrName)
        {
            var docDef = _docDefRepo.DocDefById(targetDefId);
            var attrDef = docDef.Attributes.FirstOrDefault(a => String.Equals(a.Name, attrName, StringComparison.OrdinalIgnoreCase));
            if (attrDef == null)
            {
                return new List<Guid>();
            }

            var sqlReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
            using (var query = new SqlQuery(DataContext, docDef, UserId))
            {
                query.AddAttribute("&Id");
                query.AddCondition(ExpressionOperation.And, query.Source.GetDocDef(), attrDef.Id,
                                   ConditionOperation.Equal,
                                   docId);

                var resList = new List<Guid>();

                using (var reader = sqlReaderFactory.Create(query))
                {
                    while (reader.Read())
                        resList.Add(reader.GetGuid(0));
                }
                return resList;
            }
        }

        /// <summary>
        /// Получает текущее состояние документа для пользователя
        /// </summary>
        /// <param name="documentId">Идентификатор документа</param>
        /// <param name="forDate">Дата на которую необходимо получить состояние документа</param>
        /// <returns>Состояние документа</returns>
        public DocState GetDocState(Guid documentId, DateTime forDate = new DateTime())
        {
            if (forDate == DateTime.MinValue) forDate = MaxDate;

            if (forDate == MaxDate)
            {
                var cached = DocCache.Find(documentId);

                if (cached != null && cached.CachedObject.State != null)
                    return new DocState
                               {
                                   Id = cached.CachedObject.State.Id,
                                   Created = cached.CachedObject.State.Created,
                                   Type = new DocStateType
                                              {
                                                  Id = cached.CachedObject.State.Type.Id,
                                                  Name = cached.CachedObject.State.Type.Name,
                                                  ReadOnly = cached.CachedObject.State.Type.ReadOnly
                                              },
                                   WorkerId = cached.CachedObject.State.WorkerId,
                                   WorkerName = cached.CachedObject.State.WorkerName
                               };
            }

            var docState = _docStorage.LoadDocState(documentId, forDate);

            if (docState != null)
            {
                return new DocState
                {
                    Id = docState.Id,
                    Type = _docStateRepo.LoadById(docState.StateTypeId),
                    WorkerId = docState.UserId,
                    Created = docState.Created,
                    WorkerName = GetUserName(docState.UserId)
                };
            }

            return null;
        }

        /// <summary>
        /// Устанавливает состояние документа для пользователя
        /// </summary>
        //// <param name="workerId">Идентификатор пользователя</param>
        /// <param name="doc">Документ</param>
        /// <param name="stateTypeId">Идентификатор состояния</param>
        /// <returns></returns>
        public void SetDocState(Doc doc, Guid stateTypeId)
        {
            if (doc == null)
                throw new ApplicationException("Не могу установить статус документу. Документ не назначен!");

            var currentDate = DateTime.Now;
            var stateType = _docStateRepo.LoadById(stateTypeId);

            if (DocIsStored(doc))
            {
                var docStateId = Guid.NewGuid();
                DataContext.BeginTransaction();
                try
                {
                    _docStorage.SaveDocState(docStateId, doc.Id, stateTypeId, UserId, currentDate);
                    DataContext.Commit();
                }
                catch
                {
                    DataContext.Rollback();
                    throw;
                }
                var cached = DocCache.Find(doc.Id);
                if (cached != null)
                {
                    cached.CachedObject.State = new DocState
                                                    {
                                                        Id = docStateId,
                                                        Type = stateType,
                                                        WorkerId = UserId,
                                                        Created = currentDate,
                                                        WorkerName = GetUserName(UserId)
                                                    };
                }
                doc.State = new DocState
                                {
                                    Id = docStateId,
                                    Type = stateType,
                                    WorkerId = UserId,
                                    Created = currentDate,
                                    WorkerName = GetUserName(UserId)
                                };
            }
            else
            {
                if (doc.State != null && doc.State.Type != null && doc.State.Type.Id == stateTypeId) return;

                doc.State = new DocState
                                {
                                    Id = Guid.NewGuid(),
                                    Type = stateType, 
                                    WorkerId = UserId,
                                    Created = DateTime.Now,
                                    WorkerName = GetUserName(UserId)
                                };
            }
        }

        /// <summary>
        /// Устанавливает состояние документа для пользователя
        /// </summary>
        //// <param name="workerId">Идентификатор пользователя</param>
        /// <param name="documentId">Идентификатор документа</param>
        /// <param name="stateTypeId">Идентификатор состояния</param>
        /// <returns></returns>
        public void SetDocState(Guid documentId, Guid stateTypeId)
        {
            // TODO: должен возвращать документ
            var currentDate = DateTime.Now;

            var stateType = _docStateRepo.LoadById(stateTypeId);
            var docStateId = Guid.NewGuid();
            _docStorage.SaveDocState(docStateId, documentId, stateTypeId, UserId, currentDate);

            var cached = DocCache.Find(documentId);
            if (cached != null)
            {
                cached.CachedObject.State = new DocState
                {
                    Id = docStateId,
                    Type = stateType,
                    WorkerId = UserId,
                    Created = currentDate,
                    WorkerName = GetUserName(UserId)
                };
            }
        }

        private string GetUserName(Guid userId)
        {
            var userInfo = _userRepo.FindUserInfo(userId);

            return userInfo != null
                       ? String.Format("{0} {1} {2}", userInfo.LastName, userInfo.FirstName,
                                       userInfo.MiddleName)
                       : "";
        }


        /// <summary>
        /// Возвращает вложенный документ
        /// </summary>
        /// <param name="document">Текущий документ</param>
        /// <param name="docAttr">Атрибут ссылающийся на вложенный документ</param>
        /// <returns></returns>
        public Doc GetNestingDocument(Doc document, DocAttribute docAttr)
        {
            // Должен просмотреть вложенные документы, если есть - вернуть
            if (docAttr.Document != null) return docAttr.Document;
            // иначе загрузить из БД
            return docAttr.Document = docAttr.Value != null ? LoadById((Guid)docAttr.Value) : null;
        }

        /// <summary>
        /// Возвращает вложенный документ
        /// </summary>
        /// <param name="document">Текущий документ</param>
        /// <param name="nestingId">Идентификатор вложенного документа</param>
        /// <returns></returns>
        public Doc GetNestingDocument(Doc document, Guid nestingId)
        {
            // Должен просмотреть вложенные документы, если есть - вернуть
            foreach (var attr in document.AttrDoc)
            {
                if (attr.Document != null && attr.Document.Id == nestingId)
                    return attr.Document;
            }
            // иначе загрузить из БД
            return LoadById(nestingId);
        }

        /// <summary>
        /// Возвращает вложенный документ
        /// </summary>
        /// <param name="document">Текущий документ</param>
        /// <param name="nestingId">Идентификатор вложенного документа</param>
        /// <param name="forDate">Дата</param>
        /// <returns></returns>
        public Doc GetNestingDocument(Doc document, Guid nestingId, DateTime forDate)
        {
            // Должен просмотреть вложенные документы, если есть - вернуть
            // иначе загрузить из БД
            return LoadById(nestingId, forDate);
        }

        /// <summary>
        /// Проверяет сохранен ли документ в БД
        /// </summary>
        /// <param name="document">Документ</param>
        /// <returns>Истина - сохранен</returns>
        public bool DocIsStored(Doc document)
        {
            if (DocCache.Find(document.Id) != null) return true;

            return _docStorage.IsExists(document.Id);
        }

        public bool DocExists(Guid docId)
        {
            if (DocCache.Find(docId) != null) return true;

            return _docStorage.IsExists(docId);
        }

        /// <summary>
        /// Проверяет содержится ли документ в списке
        /// </summary>
        /// <param name="docId">Идентификатор документа, который нужно проверить на вхождение в список</param>
        /// <param name="attrDocId">Идентификатор документа которому принадлежит список</param>
        /// <param name="attrDefId">Идентификатор атрибута-списка</param>
        /// <returns>Истина - имеется в списке</returns>
        public bool ExistsInDocList(Guid docId, Guid attrDocId, Guid attrDefId)
        {
            return _attrStorage.ExistsDocInList(attrDocId, attrDefId, docId);
        }

        public void AddDocToList(Guid docId, Guid attrDocId, Guid attrDefId)
        {
            if (_attrStorage.ExistsDocInList(attrDocId, attrDefId, docId)) return;

            _attrStorage.AddDocListItem(attrDocId, attrDefId, docId, UserId, DateTime.Now);
        }

        /// <summary>
        /// Добавить документ в список
        /// </summary>
        /// <param name="docId">Идентификатор документа, который необходимо добавить в список</param>
        /// <param name="document">Документ содержащий список</param>
        /// <param name="attrDefId">Идентификатор списка</param>
        //// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Текущий документ</returns>
        public Doc AddDocToList(Guid docId, Doc document, Guid attrDefId)
        {
            if (!DocIsStored(document))
            {
                var attr = document.AttrDocList.First(a => a.AttrDef.Id == attrDefId);
                if (attr.AddedDocIds == null)
                    attr.AddedDocIds = new List<Guid>();
                if (!attr.AddedDocIds.Contains(docId)) attr.AddedDocIds.Add(docId);
            }
            else 
                AddDocToList(docId, document.Id, attrDefId);

            return document;
        }

        public BlobData GetBlobAttrData(Guid docId, AttrDef attrDef)
        {
            var data = _attrStorage.Load(docId, attrDef);

            return data != null
                ? new BlobData
                {
                    DocumentId = docId,
                    AttributeDefId = attrDef.Id,
                    Data = data.Value as byte[],
                    FileName = data.Value2
                }
                : null;
        }

        public BlobData GetBlobAttrData(Guid docId, Guid attrDefId)
        {
            if (DocExists(docId))
            {
                var doc = LoadById(docId);
                var attrDef = doc.DocDef.Attributes.FirstOrDefault(ad => ad.Id == attrDefId);
                var data = _attrStorage.Load(docId, attrDef);
                return data != null
                    ? new BlobData
                    {
                        DocumentId = docId,
                        AttributeDefId = attrDefId,
                        Data = data.Value as byte[],
                        FileName = data.Value2
                    }
                    : null;
            }
            return null;
        }

        public void SaveBlobAttrData(Doc doc, Guid attrDefId, byte[] data, string fileName)
        {
            if (doc == null) throw new ApplicationException("Не могу сохранить Blob объект. Документ не найден!");

            var attrDef = doc.DocDef.Attributes.FirstOrDefault(a => a.Id == attrDefId);
            if (attrDef == null)
                throw new ApplicationException("Ошибка при сохранении Blob атрибута. Атрибут не найден!");

            _attrStorage.DirectSave(doc.Id, attrDef, data, UserId, DateTime.Now, fileName);
        }

        public void SaveBlobAttrData(Guid docId, AttrDef attrDef, byte[] data, string fileName)
        {
            var doc = FindCacheDoc(docId);

            if (doc != null) // Документ находится в кэше
            {
                var attr = doc.Attributes.FirstOrDefault(a => a.AttrDef.Id == attrDef.Id) as BlobAttribute;
                if (attr == null)
                    throw new ApplicationException("Ошибка при сохранении Blob атрибута. Атрибут не найден!");

                attr.Value = null;
                attr.HasValue = data != null;
            }
            _attrStorage.DirectSave(docId, attrDef, data, UserId, DateTime.Now, fileName);
        }

        public void SaveBlobAttrData(Guid docId, Guid attrDefId, byte[] data, string fileName)
        {
            var doc = LoadById(docId);
            SaveBlobAttrData(doc, attrDefId, data, fileName);
        }

        private Doc FindCacheDoc(Guid docId)
        {
            var cached = DocCache.Find(docId);
            if (cached != null)
            {
                var returnDoc = CloneDoc(cached.CachedObject);

                if (returnDoc.DocDef == null)
                    throw new ApplicationException("Ошибка в документе! Класс документа не определен!");

                var defId = returnDoc.DocDef.Id;

                returnDoc.DocDef = _docDefRepo.DocDefById(defId);

                CheckDocumentAttributes(returnDoc, returnDoc.DocDef);
                CreateVirtualAttributes(returnDoc);

                CalculateAutoAttributes(returnDoc);

                return returnDoc;
            }
            return null;
        }

        public Doc AddDocToList(Guid docId, Doc document, DocListAttribute attr)
        {
            if (!DocIsStored(document))
            {
                if (attr.AddedDocIds == null)
                    attr.AddedDocIds = new List<Guid>();
                if (!attr.AddedDocIds.Contains(docId)) attr.AddedDocIds.Add(docId);
            }
            else
                AddDocToList(docId, document.Id, attr.AttrDef.Id);

            return document;
        }

        public Doc AddDocToList(Doc doc, Doc owner, DocListAttribute attr)
        {
            if (!DocIsStored(owner) || !DocIsStored(doc))
            {
                if (attr.AddedDocs == null)
                    attr.AddedDocs = new List<Doc>();
                attr.AddedDocs.RemoveAll(d => d.Id == doc.Id);
                attr.AddedDocs.Add(doc);
            }
            else
                AddDocToList(doc.Id, owner.Id, attr.AttrDef.Id);

            return owner;
        }

        public Doc AddDocToList(Doc doc, Doc owner, string attrName)
        {
            var attr = owner.Get<DocListAttribute>(attrName);

            return AddDocToList(doc, owner, attr);
        }

        public Doc AddDocToList(Doc doc, Doc owner, Guid attrDefId)
        {
            var attr = owner.AttrDocList.First(a => a.AttrDef.Id == attrDefId);

            return AddDocToList(doc, owner, attr);
        }

        /// <summary>
        /// Добавить документ в список
        /// </summary>
        /// <param name="docId">Идентификатор документа, который необходимо добавить в список</param>
        /// <param name="document">Документ содержащий список</param>
        /// <param name="attrDefName">Идентификатор списка</param>
        //// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Текущий документ</returns>
        public Doc AddDocToList(Guid docId, Doc document, string attrDefName)
        {
            var attr = document.AttrDocList.FirstOrDefault(a => String.Equals(a.AttrDef.Name, attrDefName, StringComparison.OrdinalIgnoreCase));

            if (attr == null)
                throw new ApplicationException(String.Format("Атрибут с именем \"{0}\" не найден", attrDefName));

            if (!DocIsStored(document))
            {
                if (attr.AddedDocIds == null)
                    attr.AddedDocIds = new List<Guid>();
                if (!attr.AddedDocIds.Contains(docId)) attr.AddedDocIds.Add(docId);
            }
            else
                AddDocToList(docId, document.Id, attr.AttrDef.Id);

            return document;
        }

        /// <summary>
        /// Очистить атрибут - список документов
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="attrDefId"></param>
        public void ClearAttrDocList(Guid docId, Guid attrDefId)
        {
            var currentDate = DateTime.Now;

            _attrStorage.ClearDocList(docId, attrDefId, currentDate);
        }

        /// <summary>
        /// Удалить документ из списка
        /// </summary>
        /// <param name="docId">Идентификатор удаляемого из списка документа</param>
        /// <param name="attrDocId">Идентификатор документа содержащего список</param>
        /// <param name="attrDefId">Идентификатор спискового атрибута</param>
        public void RemoveDocFromList(Guid docId, Guid attrDocId, Guid attrDefId)
        {
            var currentDate = DateTime.Now;

            if (!_attrStorage.ExistsDocInList(attrDocId, attrDefId, docId)) return;
            

            _attrStorage.RemoveDocListItem(attrDocId, attrDefId, docId, currentDate);
        }

        public void RemoveDocFromList(Guid docId, Guid attrDocId, string attrDefName)
        {
            var docDef = _docDefRepo.DocDefById(attrDocId);

            var attrDef = docDef.Attributes.First(a => String.Equals(a.Name, attrDefName, StringComparison.OrdinalIgnoreCase));

            RemoveDocFromList(docId, attrDocId, attrDef.Id);
        }

        public void RemoveDocFromList(Guid docId, Doc document, DocListAttribute attr)
        {
            if (attr.AddedDocIds != null) attr.AddedDocIds.Remove(docId);
            if (attr.AddedDocs != null)
            {
                attr.AddedDocs.RemoveAll(d => d.Id == docId);
            }
            if (DocIsStored(document))
                RemoveDocFromList(docId, document.Id, attr.AttrDef.Id);
        }

        public void RemoveDocFromList(Guid docId, Doc document, string attrDefName)
        {
            var attr = document.Attributes.First(a => String.Equals(a.AttrDef.Name, attrDefName, StringComparison.OrdinalIgnoreCase));

            RemoveDocFromList(docId, document.Id, attr.AttrDef.Id);
        }

        /// <summary>
        /// Подсчет строк в списковом атрибует документа
        /// </summary>
        /// <param name="document">Документ</param>
        /// <param name="attr">Списковый аттрибут</param>
        /// <returns>Количество строк</returns>
        // TODO: Исключить EF, переделать через прямой SQL
        public int CalcAttrDocListCount(Doc document, DocListAttribute attr)
        {
//            return CalcAttrDocListCount(document, attr, DateTime.Now);
            var count =
                DataContext.GetEntityDataContext().Entities.DocumentList_Attributes
                    .Count(a => a.Document_Id == document.Id && a.Def_Id == attr.AttrDef.Id &&
                                a.Expired == MaxDate);

            return count;
        }

        // TODO: Исключить EF, переделать через прямой SQL
        public int CalcAttrDocListCount(Doc document, DocListAttribute attr, DateTime forDate)
        {
            var count =
                DataContext.GetEntityDataContext().Entities.DocumentList_Attributes
                    .Count(a => a.Document_Id == document.Id && a.Def_Id == attr.AttrDef.Id &&
                                a.Expired >= forDate && a.Created <= forDate);

            return count;
        }

        /// <summary>
        /// Подсчет строк в списковом атрибует документа
        /// </summary>
        /// <param name="document">Документ</param>
        /// <param name="attrId">Идентификатор спискового аттрибута</param>
        /// <returns>Количество строк</returns>
        public int CalcAttrDocListCount(Doc document, Guid attrId)
        {
            var attr = document.Attributes.OfType<DocListAttribute>().First(a => a.AttrDef.Id == attrId);

            return CalcAttrDocListCount(document, attr);
        }

        public int CalcAttrDocListCount(Doc document, string attrName)
        {
            var attr = document.Attributes.OfType<DocListAttribute>().First(a => String.Equals(a.AttrDef.Name, attrName, StringComparison.OrdinalIgnoreCase));

            return CalcAttrDocListCount(document, attr);
        }

        /// <summary>
        /// Подсчитывает сумму значений атрибута в списке документа
        /// </summary>
        /// <param name="document">Документ</param>
        /// <param name="attr">Списочный атрибут</param>
        /// <param name="sumAttrName">Наименование суммируемого атрибута</param>
        /// <returns>Сумма</returns>
        // TODO: Исключить EF, переделать через прямой SQL
        public double? CalcAttrDocListSum(Doc document, DocListAttribute attr, string sumAttrName)
        {
//            return CalcAttrDocListSum(document, attr, sumAttrName, DateTime.Now);
            if (attr.AttrDef.DocDefType == null)
                throw new ApplicationException(String.Format("Атрибут \"{0}\" не ссылается на класс документа",
                                                             attr.AttrDef.Name));

            var sumDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);

            var sumAttr = sumDocDef.Attributes.First(a => String.Equals(a.Name, sumAttrName, StringComparison.OrdinalIgnoreCase));

            var en = DataContext.GetEntityDataContext().Entities;
            var attrDocList = en.DocumentList_Attributes
                .Where(a => a.Document_Id == document.Id && a.Def_Id == attr.AttrDef.Id &&
                            a.Expired >= MaxDate)
                .Select(a => a.Value);

            double? result = null;

            switch (sumAttr.Type.Id)
            {
                case (short) CissaDataType.Int:
                    var ri = en.Int_Attributes.Include("Documents")
                        .Where(a => a.Def_Id == sumAttr.Id && a.Expired >= MaxDate &&
                                    attrDocList.Contains(a.Document_Id))
                        .Select(a => a.Value);
                    if (ri.Any()) result = ri.Sum();
                    break;
                case (short) CissaDataType.Float:
                    var rf = en.Float_Attributes.Include("Documents")
                        .Where(a => a.Def_Id == sumAttr.Id && a.Expired >= MaxDate &&
                                    attrDocList.Contains(a.Document_Id))
                        .Select(a => a.Value);
                    if (rf.Any()) result = rf.Sum();
                    break;
                case (short) CissaDataType.Currency:
                    var rc = en.Currency_Attributes.Include("Documents")
                        .Where(a => a.Def_Id == sumAttr.Id && a.Expired >= MaxDate &&
                                    attrDocList.Contains(a.Document_Id))
                        .Select(a => a.Value);
                    if (rc.Any()) result = (double) rc.Sum();
                    break;
            }
            return result;
        }

        public double? CalcAttrDocListSum(Doc document, string attrName, string sumAttrName)
        {
            var attr = document.Attributes.OfType<DocListAttribute>().First(a => String.Equals(a.AttrDef.Name, attrName, StringComparison.OrdinalIgnoreCase));

            return CalcAttrDocListSum(document, attr, sumAttrName);
        }

        public double? CalcAttrDocListSum(Doc document, Guid attrId, string sumAttrName)
        {
            var attr = document.Attributes.OfType<DocListAttribute>().First(a => a.AttrDef.Id == attrId);

            return CalcAttrDocListSum(document, attr, sumAttrName);
        }

        // TODO: Исключить EF, переделать через прямой SQL
        public double? CalcAttrDocListSum(Doc document, DocListAttribute attr, string sumAttrName, DateTime forDate)
        {
            if (attr.AttrDef.DocDefType == null)
                throw new ApplicationException(String.Format("Атрибут \"{0}\" не ссылается на класс документа",
                                                             attr.AttrDef.Name));

            var sumDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);

            var sumAttr = sumDocDef.Attributes.First(a => String.Equals(a.Name, sumAttrName, StringComparison.OrdinalIgnoreCase));

            var en = DataContext.GetEntityDataContext().Entities;
            var attrDocList = en.DocumentList_Attributes
            // ReSharper disable ImplicitlyCapturedClosure
                .Where(a => a.Document_Id == document.Id && a.Def_Id == attr.AttrDef.Id &&
                            a.Created < forDate && a.Expired >= forDate)
                .Select(a => a.Value);

            double? result = null;

            switch (sumAttr.Type.Id)
            {
                case (short) CissaDataType.Int:
                    var ri = en.Int_Attributes.Include("Documents")
                        .Where(a => a.Def_Id == sumAttr.Id &&
                                    a.Created < forDate && a.Expired >= forDate &&
                                    attrDocList.Contains(a.Document_Id))
                        .Select(a => a.Value);
                    if (ri.Any()) result = ri.Sum();
                    break;
                case (short) CissaDataType.Float:
                    var rf = en.Float_Attributes.Include("Documents")
                        .Where(a => a.Def_Id == sumAttr.Id &&
                                    a.Created < forDate && a.Expired >= forDate &&
                                    attrDocList.Contains(a.Document_Id))
                        .Select(a => a.Value);
                    if (rf.Any()) result = rf.Sum();
                    break;
                case (short) CissaDataType.Currency:
                    var rc = en.Currency_Attributes.Include("Documents")
                        .Where(a => a.Def_Id == sumAttr.Id &&
                                    a.Created < forDate && a.Expired >= forDate &&
                                    attrDocList.Contains(a.Document_Id))
                        .Select(a => a.Value);
                    if (rc.Any()) result = (double) rc.Sum();
                    break;
            }
            // ReSharper restore ImplicitlyCapturedClosure
            return result;
        }

        public void Dispose()
        {
            if (_ownDataContext && DataContext != null)
            {
                try
                {
                    DataContext.Dispose();
                    DataContext = null;
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "DocRepository.Dispose");
                    throw;
                }
            }
        }

        ~DocRepository()
        {
            if (_ownDataContext && DataContext != null)
                try 
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "DocRepository.Finalize");
                    throw;
                }
        }
    }
}
