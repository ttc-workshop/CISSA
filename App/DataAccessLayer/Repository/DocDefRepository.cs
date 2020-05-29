using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class DocDefRepository : IDocDefRepository
    {
        private readonly IPermissionRepository _permissionRepository;
        public Guid UserId { get; private set; }

        public IAppServiceProvider Provider { get; private set; }
        private readonly bool _ownProvider;
        public IDataContext DataContext { get; private set; }

        private readonly IEnumRepository _enumRepo;

        public DocDefRepository(IDataContext dataContext, Guid userId)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = factory.Create();
            _ownProvider = true;

            DataContext = dataContext ?? Provider.Get<IDataContext>();
            _enumRepo = Provider.Get<IEnumRepository>();
            _permissionRepository = Provider.Get<IPermissionRepository>();
            UserId = userId;
        }

        public DocDefRepository(IDataContext dataContext) : this(dataContext, Guid.Empty) {}
        public DocDefRepository(Guid userId) : this(null, userId) { }
        public DocDefRepository() : this(null, Guid.Empty) {}

        public DocDefRepository(IAppServiceProvider provider, IDataContext dataContext)
        {
            Provider = provider;
            DataContext = dataContext ?? Provider.Get<IDataContext>();
            UserId = Provider.GetCurrentUserId();
            _enumRepo = Provider.Get<IEnumRepository>();
            _permissionRepository = Provider.Get<IPermissionRepository>();
        }

        public static readonly ObjectCache<DocDef> DocDefCache = new ObjectCache<DocDef>();
        private static readonly ReaderWriterLock DocDefCacheLock = new ReaderWriterLock();
        
        public static readonly ObjectCache<IEnumerable<Guid>> DocDefDescendantCache = new ObjectCache<IEnumerable<Guid>>();
        private static readonly ReaderWriterLock DocDefDescendantCacheLock = new ReaderWriterLock();

        private static IList<DocDef> _metaobjectDefs;
      
        // TODO: Вынести метаобъекты в отдельный класс!
        private static DocDef FindMetaobjectDocDef(Guid docId)
        {
            if (_metaobjectDefs == null)
            {
                _metaobjectDefs = new List<DocDef>(MetaobjectDefs.GetMetaobjectDocDefs());
            }
            return _metaobjectDefs.FirstOrDefault(def => def.Id == docId);
        }

        /// <summary>
        /// Возвращает потомков для типа документа
        /// </summary>
        /// <param name="docDefId">Идентификатор типа документа</param>
        /// <returns>Список идентификаторов потомков</returns>
        //[SmartCache(TimeOutSeconds = 3600)]
        public IEnumerable<Guid> GetDocDefDescendant(Guid docDefId)
        {
            DocDefDescendantCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cache = DocDefDescendantCache.Find(docDefId);
                if (cache != null) return cache.CachedObject;

                var lc = DocDefDescendantCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    cache = DocDefDescendantCache.Find(docDefId);
                    if (cache != null) return cache.CachedObject;

                    var docDefs = (
                        from docDef in DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Document_Def>()
                        where docDef.Ancestor_Id == docDefId
                        select docDef.Id).ToList();

                    foreach (var id in docDefs)
                        docDefs = new List<Guid>(docDefs.Union(GetDocDefDescendant(id)));

                    docDefs = new List<Guid>(docDefs.Union(new[] {docDefId}));
                    DocDefDescendantCache.Add(docDefs, docDefId);

                    return docDefs;
                }
                finally
                {
                    DocDefDescendantCacheLock.DowngradeFromWriterLock(ref lc);
                }
            }
            finally
            {
                DocDefDescendantCacheLock.ReleaseReaderLock();
            }
        }

        public DocDef Find(string docDefName)
        {
            Guid id;

            DocDefCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cached =
                    DocDefCache
                        .FirstOrDefault(
                            o =>
                                String.Compare(o.CachedObject.Name, docDefName, StringComparison.OrdinalIgnoreCase) == 0);
                if (cached != null)
                    return cached.CachedObject;

                var query = from doc in DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Document_Def>()
                    where doc.Name.ToUpper() == docDefName.ToUpper()
                    select doc.Id;

                id = query.FirstOrDefault();

                if (id == Guid.Empty) return null;
            }
            finally
            {
                DocDefCacheLock.ReleaseReaderLock();
            }
            return Find(id);
        }

        /// <summary>
        /// Возвращает предков для типа документа
        /// </summary>
        /// <param name="docDefId">Идентификатор типа документа</param>
        /// <returns>Список идентификаторов предков</returns>
        public IEnumerable<Guid> GetAncestors(Guid docDefId)
        {
            var dd = Find(docDefId);

            while (dd != null && dd.AncestorId != null)
            {
                yield return (Guid) dd.AncestorId;
                dd = Find((Guid) dd.AncestorId);
            }
        }

        /// <summary>
        /// Возвращает предков для типа документа
        /// </summary>
        /// <param name="docDef">Тип документа</param>
        /// <returns>Список идентификаторов предков</returns>
        public IEnumerable<Guid> GetAncestors(DocDef docDef)
        {
            var dd = docDef;

            while (dd != null && dd.AncestorId != null)
            {
                yield return (Guid) dd.AncestorId;
                dd = Find((Guid)dd.AncestorId);
            }
        }

        /// <summary>
        /// Загружает тип документа по идентификатору
        /// </summary>
        /// <param name="docDefId">Идентификатор типа документа</param>
        /// <returns>Тип документа</returns>
        //[SmartCache(TimeOutSeconds = 3600)]
        public DocDef Find(Guid docDefId)
        {
            DocDefCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cachedItem = DocDefCache.Find(docDefId);
                if (cachedItem != null)
                    return cachedItem.CachedObject;

                var metaDocDef = FindMetaobjectDocDef(docDefId);
                if (metaDocDef != null)
                {
                    FillDocDefAncestorAttributes(metaDocDef, metaDocDef.AncestorId);
                    DocDefCache.Add(metaDocDef, docDefId);

                    return metaDocDef;
                }

                var lc = DocDefCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    cachedItem = DocDefCache.Find(docDefId);
                    if (cachedItem != null)
                        return cachedItem.CachedObject;

                    var query =
                        from def in DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Document_Def>()
                        where def.Id == docDefId
                        select def;

                    if (!query.Any()) return null;

                    var dbDocDef = query.First();
                    var docDef = new DocDef
                    {
                        Id = dbDocDef.Id,
                        Name = dbDocDef.Name,
                        Caption = dbDocDef.Full_Name,
                        AncestorId = dbDocDef.Ancestor_Id,
//                WithHistory = dbDocDef.WithHistory,
                        IsInline = dbDocDef.Is_Inline ?? false,
                        IsPublic = dbDocDef.Is_Public ?? false,
                        Description = dbDocDef.Description,
                        Attributes = new List<AttrDef>()
                    };

                    FillDocDefAncestorAttributes(docDef, docDef.AncestorId);
                    FillDocDefAttributes(docDef, docDef.Id);

                    docDef.Permissions = _permissionRepository.GetObjectDefPermissions(docDef.Id);
                    DocDefCache.Add(docDef, docDefId);
                    InitAttrDocDefTypes(docDef);
                    return docDef;
                }
                finally
                {
                    DocDefCacheLock.DowngradeFromWriterLock(ref lc);
                }
            }
            finally
            {
                DocDefCacheLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Загружает тип документа по идентификатору
        /// </summary>
        /// <param name="docDefId">Идентификатор типа документа</param>
        /// <returns>Тип документа</returns>
        public DocDef DocDefById(Guid docDefId)
        {
            var docDef = Find(docDefId);

            if (docDef == null)
                throw new ApplicationException(
                    string.Format("Типа документа с идентификатором {0} не существует", docDefId));

            return docDef;
        }

        /// <summary>
        /// Загружает тип документа по имени
        /// </summary>
        /// <param name="docDefName">Имя типа документа</param>
        /// <returns>Тип документа</returns>
        //[SmartCache(TimeOutSeconds = 3600)]
        public DocDef DocDefByName(string docDefName)
        {
            var docDef = Find(docDefName);

            if (docDef == null)
                throw new ApplicationException(
                    string.Format("Типа документа с именем {0} не существует", docDefName));

            return docDef;
        }

        public static readonly IDictionary<string, List<DocDefName>> DocDefNameCache = new Dictionary<string, List<DocDefName>>();
        // public readonly static object DocDefNameCacheLock = new object();
        static readonly ReaderWriterLock DocDefNameCacheLock = new ReaderWriterLock();
        private const int LockTimeout = 500000;
        private const string SelectDocDefNameSql = 
            "SELECT dd.Id, od.Name, od.Full_Name " +
            "FROM Document_Defs dd JOIN Object_Defs od ON od.Id = dd.Id " +
            "WHERE (od.Deleted is null OR od.Deleted = 0) AND dd.[WithHistory] = 0 " +
            "ORDER BY od.Full_Name";

        public IList<DocDefName> GetDocDefNames()
        {
            //lock (DocDefNameCacheLock)
            DocDefNameCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var list = FindDocDefNames(DataContext.Name);
                if (list != null) return list;

                var lc = DocDefNameCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    list = FindDocDefNames(DataContext.Name); // Убедиться, что другой процесс уже не создал список
                    if (list != null) return list;

                    list = new List<DocDefName>();

                    using (var command = DataContext.CreateCommand(SelectDocDefNameSql))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(
                                    new DocDefName
                                    {
                                        Id = reader.GetGuid(0),
                                        Name = reader.IsDBNull(1) ? String.Empty : reader.GetString(1),
                                        Caption = reader.IsDBNull(2) ? String.Empty : reader.GetString(2)
                                    });
                            }
                            DocDefNameCache.Add(DataContext.Name, list);
                        }
                    }
                    return list;
                }
                finally
                {
                    DocDefNameCacheLock.DowngradeFromWriterLock(ref lc);
                }
            }
            finally
            {
                DocDefNameCacheLock.ReleaseReaderLock();
            }
        }

        public static void ClearCaches()
        {
            DocDefCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                if (DocDefCache != null) DocDefCache.Clear();
            }
            finally
            {
                DocDefCacheLock.ReleaseWriterLock();
            }
            DocDefNameCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                if (DocDefNameCache != null) DocDefNameCache.Clear();
            }
            finally
            {
                DocDefNameCacheLock.ReleaseWriterLock();
            }
            DocDefDescendantCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                DocDefDescendantCache.Clear();
            }
            finally
            {
                DocDefDescendantCacheLock.ReleaseWriterLock();
            }
            TypeDefCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                TypeDefCache = null;
            }
            finally
            {
                TypeDefCacheLock.ReleaseWriterLock();
            }
        }

        private const string SelectDocDefRelationSql = 
            "SELECT ad.Id, od.Name, od.Full_Name, ad.Type_Id, " +
                "(SELECT Id FROM Document_Defs WHERE Id = od1.Id), (SELECT Id FROM Document_Defs WHERE Id = od2.Id), " +
                "(SELECT Id FROM Document_Defs WHERE Id = od3.Id), (SELECT Id FROM Document_Defs WHERE Id = od4.Id), (SELECT Id FROM Document_Defs WHERE Id = od5.Id) " +
            "FROM Attribute_Defs ad JOIN Object_Defs od ON od.Id = ad.Id " +
                "LEFT OUTER JOIN Object_Defs od1 ON od1.Id = od.Parent_Id " +
                "LEFT OUTER JOIN Object_Defs od2 ON od2.Id = od1.Parent_Id " +
                "LEFT OUTER JOIN Object_Defs od3 ON od3.Id = od2.Parent_Id " +
                "LEFT OUTER JOIN Object_Defs od4 ON od4.Id = od3.Parent_Id " +
                "LEFT OUTER JOIN Object_Defs od5 ON od5.Id = od4.Parent_Id " +
            "WHERE ad.Document_Id = @docId AND ad.Type_Id IN (6,7) " +
                "AND (od.Deleted is null OR od.Deleted = 0) AND (od1.Deleted is null OR od1.Deleted = 0) " +
                "AND (od2.Deleted is null OR od2.Deleted = 0)";
        public IList<DocDefRelation> GetDocDefRelations(Guid docDefId)
        {
            var docDef = Find(docDefId);

            return docDef == null ? new List<DocDefRelation>() : GetDocDefRelations(docDef);
        }

        public IList<DocDefRelation> GetDocDefRelations(DocDef docDef)
        {
            var relations = (
                from attr in docDef.Attributes
                where
                    (attr.Type.Id == (short) CissaDataType.Doc || attr.Type.Id == (short) CissaDataType.DocList) &&
                    attr.DocDefType != null
                select new DocDefRelation
                {
                    DocDefId = docDef.Id,
                    DocumentName = docDef.Name,
                    DocumentCaption = docDef.Caption,
                    AttributeId = attr.Id,
                    AttributeName = attr.Name,
                    AttributeCaption = attr.Caption,
                    RefDocDefId = attr.DocDefType.Id,
                    RefDocumentName = attr.DocDefType.Name,
                    RefDocumentCaption = attr.DocDefType.Caption,
                    DataType = (CissaDataType) attr.Type.Id
                }).ToList();

            using (var command = DataContext.CreateCommand(SelectDocDefRelationSql))
            {
                var param = command.CreateParameter();
                param.ParameterName = "@docId";
                param.Value = docDef.Id;
                command.Parameters.Add(param);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var relation = new DocDefRelation
                        {
                            AttributeId = reader.GetGuid(0),
                            AttributeName = reader.GetString(1),
                            AttributeCaption = reader.GetString(2),
                            RefDocDefId = docDef.Id,
                            RefDocumentCaption = docDef.Caption,
                            RefDocumentName = docDef.Name,
                            DataType = (CissaDataType) reader.GetInt16(3)
                        };

                        var relDocDefId = !reader.IsDBNull(4)
                            ? reader.GetGuid(4)
                            : !reader.IsDBNull(5)
                                ? reader.GetGuid(5)
                                : !reader.IsDBNull(6)
                                    ? reader.GetGuid(6)
                                    : !reader.IsDBNull(7)
                                        ? reader.GetGuid(7)
                                        : !reader.IsDBNull(8) ? reader.GetGuid(8) : Guid.Empty;

                        if (relDocDefId != Guid.Empty)
                        {
                            var relDocDef = Find(relDocDefId);

                            if (relDocDef != null)
                            {
                                relation.DocDefId = relDocDef.Id;
                                relation.DocumentName = relDocDef.Name;
                                relation.DocumentCaption = relDocDef.Caption;

                                relations.Add(relation);
                            }
                        }
                    }
                }
            }

            return relations;
        }

        private static List<DocDefName> FindDocDefNames(string dataContextName)
        {
            //lock (DocDefNameCacheLock) // 09-02-17
            {
                List<DocDefName> list;
                return DocDefNameCache.TryGetValue(dataContextName, out list) ? list : null;
            }
        }

        private void FillDocDefAncestorAttributes(DocDef def, Guid? ancestorId)
        {
            if (ancestorId != null)
            {
                var ancestor = DocDefById((Guid) ancestorId);
                    /*context.ObjectDefs.OfType<Document_Def>().FirstOrDefault(dd => dd.Id == (Guid) ancestorId);*/

                if (ancestor != null)
                {
                    //FillDocDefAncestorAttributes(def, ancestor.AncestorId);
                    FillDocDefAttributes(def, ancestor);
                }
            }
        }

        private void FillDocDefAttributes(DocDef def, Guid parentId)
        {
            var edc = DataContext.GetEntityDataContext();
            foreach (var attr in edc.Entities.Object_Defs.OfType<Attribute_Def>()
                .Where(a => a.Parent_Id == parentId && (a.Deleted == null || a.Deleted == false)))
            {
                var attrDef = CreateAttrDef(attr);
                if (attrDef == null) continue;

                def.Attributes.Add(attrDef);
                attrDef.Permissions = _permissionRepository.GetObjectDefPermissions(attrDef.Id);
            }
            foreach (var folder in edc.Entities.Object_Defs.OfType<Folder_Def>()
                .Where(f => f.Parent_Id == parentId && (f.Deleted == null || f.Deleted == false)))
            {
                FillDocDefAttributes(def, folder.Id);
            }
        }

        private AttrDef CreateAttrDef(Attribute_Def source)
        {
            if (source == null) return null;

            var result = new AttrDef
            {
                Id = source.Id,
                Name = source.Name ?? String.Empty,
                Caption = source.Full_Name ?? String.Empty,
                IsNotNull = source.Is_Not_Null ?? false,
                IsUnique = source.Is_Unique ?? false,
                MaxLength = source.Max_Length ?? 0,
                MaxValue = source.Max_Value,
                MinValue = source.Min_Value,
                DefaultValue = source.Default_Value ?? String.Empty,
                OrgTypeId = source.Org_Type_Id,
                Script = source.CalculateScript,
                BlobInfo = new BlobInfo
                {
                    MaxHeight = source.BlobMaxHeight ?? 0,
                    MaxWidth = source.BlobMaxWidth ?? 0,
                    MaxSizeBytes = source.BlobMaxSizeBytes ?? 0,
                    IsImage = source.BlobIsImage ?? false
                }
            };
            // DONE: Исключить постоянное чтение типов данных из БД
            // if (!source.Data_TypesReference.IsLoaded) source.Data_TypesReference.Load();
            // if (source.Data_Types != null)
            if (source.Type_Id != null)
            {
                var typeDefs = GetTypeDefs();
                var typeDef = typeDefs.FirstOrDefault(d => d.Id == source.Type_Id);
                if (typeDef != null)
                    result.Type = new TypeDef
                    {
                        Id = typeDef.Id,
                        Name = typeDef.Name
                    };
                else
                    result.Type = new TypeDef {Id = (short) source.Type_Id};
            }
            else
                return null;

            if (source.Enum_Id != null)
            {
                // DONE: Исключить прямое чтение справочных данных из БД
                //if (!source.Enum_DefsReference.IsLoaded) source.Enum_DefsReference.Load();
                var enumDef = _enumRepo.Find((Guid) source.Enum_Id);
                if (enumDef != null)
                    result.EnumDefType = new EnumDef
                    {
                        Id = enumDef.Id,
                        Description = enumDef.Description,
                        Name = enumDef.Name,
                        Caption = enumDef.Caption
                    };
            }
            if (source.Document_Id != null)
            {
                // DONE: Исключить прямое чтение описаний документов из БД
                //if (!source.Document_DefsReference.IsLoaded) source.Document_DefsReference.Load();
                //var attrDocDef = Find((Guid) source.Document_Id);
                //if (attrDocDef != null)
                    result.DocDefType = new DocDef
                    {
                        Id = (Guid) source.Document_Id /*, //attrDocDef.Id,
                        Name = attrDocDef.Name,
                        Caption = attrDocDef.Caption,
                        AncestorId = attrDocDef.AncestorId,
                        IsInline = attrDocDef.IsInline,
                        IsPublic = attrDocDef.IsPublic*/
                    };
            }
            return result;
        }

        private void FillDocDefAttributes(DocDef def, DocDef ancestorDef)
        {
            foreach (var attr in ancestorDef.Attributes)
            {
                var attrDef = new AttrDef(attr);

                def.Attributes.Add(attrDef);
                attrDef.Permissions = _permissionRepository.GetObjectDefPermissions(attrDef.Id);
            }
        }

        private void InitAttrDocDefTypes(DocDef def)
        {
            foreach (var attr in def.Attributes)
            {
                if (attr.DocDefType != null && attr.DocDefType.Id != Guid.Empty)
                {
                    var attrDocDef = Find(attr.DocDefType.Id);

                    if (attrDocDef != null)
                    {
                        attr.DocDefType.Name = attrDocDef.Name;
                        attr.DocDefType.Caption = attrDocDef.Caption;
                        attr.DocDefType.AncestorId = attrDocDef.AncestorId;
                        attr.DocDefType.IsInline = attrDocDef.IsInline;
                        attr.DocDefType.IsPublic = attrDocDef.IsPublic;
                    }
                }
            }
        }

        public static List<TypeDef> TypeDefCache;
        // public static readonly object TypeDefCacheLock = new object();
        static readonly ReaderWriterLock TypeDefCacheLock = new ReaderWriterLock();

        private const string SelectDataTypeSql = "SELECT Id, Name FROM Data_Types";

        private IEnumerable<TypeDef> GetTypeDefs()
        {
            //lock (TypeDefCacheLock)
            TypeDefCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                if (TypeDefCache != null) return TypeDefCache;

                var lc = TypeDefCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    if (TypeDefCache != null) return TypeDefCache; // Убедиться, что другой процесс уже не создал список

                    TypeDefCache = new List<TypeDef>();

                    using (var command = DataContext.CreateCommand(SelectDataTypeSql))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TypeDefCache.Add(new TypeDef
                                {
                                    Id = reader.GetInt16(0),
                                    Name = reader.IsDBNull(1) ? String.Empty : reader.GetString(1)
                                });
                            }
                        }
                    }
                    return TypeDefCache;
                }
                finally
                {
                    TypeDefCacheLock.DowngradeFromWriterLock(ref lc);
                }
            }
            finally
            {
                TypeDefCacheLock.ReleaseReaderLock();
            }
        }

        private static DocDef Deserialize(string data)
        {
            var read = new StringReader(data);
            var serializer = new XmlSerializer(typeof(DocDef));
            var reader = new XmlTextReader(read);
            try
            {
                return (DocDef) serializer.Deserialize(reader);
            }
            finally
            {
                reader.Close();
                read.Close();
                read.Dispose();
            }
        }

        private static string Serialize(DocDef docDef)
        {
            var serializer = new XmlSerializer(typeof(DocDef));
            var stream = new MemoryStream();
            try
            {
                serializer.Serialize(stream, docDef);
                stream.Position = 0;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
        }

        public void Dispose()
        {
            if (_ownProvider && Provider != null)
            {
                try
                {
                    Provider.Dispose();
                    Provider = null;
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "DocDefRepository.Dispose");
                    throw;
                }
            }
            /*if (_ownDataContext && DataContext != null)
            {
                try
                {
                    DataContext.Dispose();
                    DataContext = null;
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "DocDefRepository.Dispose");
                    throw;
                }
            }*/
        }

        /*~DocDefRepository()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "DocDefRepository.Finalize");
                    throw;
                }
        }*/
    }
}
