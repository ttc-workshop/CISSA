using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextDocRepository : IDocRepository
    {
        private IMultiDataContext DataContext { get; set; }

        private readonly IDictionary<IDataContext, IDocRepository> _repositories = new Dictionary<IDataContext, IDocRepository>();

        private readonly IDocDefRepository _docDefRepo;

        public MultiContextDocRepository(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Document))
                    _repositories.Add(context, new DocRepository(provider, context));
            }

            _docDefRepo = provider.Get<IDocDefRepository>();
        }

        public void Dispose()
        {
        }

        public Doc Save(Doc docForSave)
        {
            var repo = TryGetDocRepo(docForSave);

            return repo != null ? repo.Save(docForSave) : GetDefaultDocRepo().Save(docForSave);
        }

        public Doc New(Guid docDefId)
        {
            return New(_docDefRepo.DocDefById(docDefId));
        }
        public Doc New(DocDef docDef)
        {
            return GetDocRepo(docDef).New(docDef);
        }

        public Doc InitDocFrom(Doc doc, IStringParams prms)
        {
            return GetDocRepo(doc).InitDocFrom(doc, prms);
        }

        public Doc LoadById(Guid documentId)
        {
            return _repositories.Values.Select(repo => repo.LoadById(documentId)).FirstOrDefault(d => d != null);
        }

        public Doc LoadById(Guid documentId, DateTime forDate)
        {
            return _repositories.Values.Select(repo => repo.LoadById(documentId, forDate)).FirstOrDefault(d => d != null);
        }

        public void DeleteById(Guid documentId)
        {
            GetDocRepo(documentId).DeleteById(documentId);
        }

        public void HideById(Guid documentId)
        {
            GetDocRepo(documentId).HideById(documentId);
        }

        public List<DocState> GetDocumentStates(Guid docId)
        {
            return new List<DocState>(_repositories.Values.SelectMany(repo => repo.GetDocumentStates(docId)));
        }

        public Doc Check(Doc document)
        {
            var repo = GetDocRepoDefault(document);
            return repo.Check(document);
        }

        public Doc CalculateAutoAttributes(Doc doc)
        {
            var repo = GetDocRepoDefault(doc);
            return repo.CalculateAutoAttributes(doc);
        }

        public DocState GetDocState(Guid documentId, DateTime forDate = new DateTime())
        {
            var repo = TryGetDocRepo(documentId);
            if (repo != null) return repo.GetDocState(documentId, forDate);
            return null;
        }

        public void SetDocState(Guid documentId, Guid stateTypeId)
        {
            GetDocRepo(documentId).SetDocState(documentId, stateTypeId);
        }

        public void SetDocState(Doc document, Guid stateTypeId)
        {
            GetDocRepo(document).SetDocState(document, stateTypeId);
        }

        public Doc CreateDoc(Guid docDefId)
        {
            return CreateDoc(_docDefRepo.DocDefById(docDefId));
        }

        public Doc CreateDoc(DocDef docDef)
        {
            var repo = GetDocRepo(docDef);
            return repo.CreateDoc(docDef);
        }

        public List<Guid> List(out int pageCount, Guid docDefId, int pageNo, int pageSize = 0)
        {
            // TODO: ѕеределать - сделать мультиконтекстную выборку!
            return GetDefaultDocRepo().List(out pageCount, docDefId, pageNo, pageSize);
        }

        public List<Guid> List(out int count, Guid docDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttrId = null)
        {
            return GetDefaultDocRepo().List(out count, docDefId, pageNo, pageSize, filter, sortAttrId);
        }

        public List<Guid> List(out int count, Guid docDefId, Guid docStateId, int pageNo, int pageSize, Doc filter = null,
            Guid? sortAttrId = null)
        {
            return GetDefaultDocRepo().List(out count, docDefId, docStateId, pageNo, pageSize, filter, sortAttrId);
        }

        public List<Guid> DocAttrList(out int count, Doc document, Guid attrDefId, int pageNo, int pageSize, Doc filter = null,
            Guid? sortAttrId = null)
        {
            return GetDocRepoDefault(document).DocAttrList(out count, document, attrDefId, pageNo, pageSize, filter, sortAttrId);
        }

        public List<Guid> DocAttrList(out int count, Doc document, string attrDefName, int pageNo, int pageSize, Doc filter = null,
            Guid? sortAttrId = null)
        {
            return GetDocRepoDefault(document).DocAttrList(out count, document, attrDefName, pageNo, pageSize, filter, sortAttrId);
        }

        public List<Guid> DocAttrList(out int count, Doc document, DocListAttribute attr, int pageNo, int pageSize, Doc filter = null,
            Guid? sortAttrId = null)
        {
            return GetDocRepoDefault(document).DocAttrList(out count, document, attr, pageNo, pageSize, filter, sortAttrId);
        }

        public List<Guid> DocAttrListById(out int count, Guid docId, Guid attrDefId, int pageNo, int pageSize, Doc filter = null,
            Guid? sortAttrId = null)
        {
            return GetDocRepoDefault(docId).DocAttrListById(out count, docId, attrDefId, pageNo, pageSize, filter, sortAttrId);
        }

        public Doc GetNestingDocument(Doc document, DocAttribute docAttr)
        {
            // ƒолжен просмотреть вложенные документы, если есть - вернуть
            if (docAttr.Document != null) return docAttr.Document;
            // иначе загрузить из Ѕƒ
            return docAttr.Document = docAttr.Value != null ? LoadById((Guid)docAttr.Value) : null;
        }

        public bool DocIsStored(Doc document)
        {
            return GetDocRepoDefault(document).DocIsStored(document);
        }

        public bool DocExists(Guid docId)
        {
            return _repositories.Values.Any(repo => repo.DocExists(docId));
        }

        public bool ExistsInDocList(Guid docId, Guid attrDocId, Guid attrDefId)
        {
            return _repositories.Values.Any(repo => repo.ExistsInDocList(docId, attrDocId, attrDefId));
        }

        public Doc AddDocToList(Guid docId, Doc document, Guid attrDefId)
        {
            return GetDocRepo(document).AddDocToList(docId, document, attrDefId);
        }

        public Doc AddDocToList(Guid docId, Doc document, string attrName)
        {
            return GetDocRepo(document).AddDocToList(docId, document, attrName);
        }

        public Doc AddDocToList(Doc doc, Doc document, DocListAttribute attr)
        {
            return GetDocRepo(document).AddDocToList(doc, document, attr);
        }

        public void RemoveDocFromList(Guid docId, Doc document, DocListAttribute attr)
        {
            var repo = TryGetDocRepo(document);
            if (repo != null) repo.RemoveDocFromList(docId, document, attr);
        }

        public void RemoveDocFromList(Guid docId, Doc document, string attrName)
        {
            var repo = TryGetDocRepo(document);
            if (repo != null) repo.RemoveDocFromList(docId, document, attrName);
        }

        public void ClearAttrDocList(Guid docId, Guid attrDefId)
        {
            var repo = TryGetDocRepo(docId);
            if (repo != null) repo.ClearAttrDocList(docId, attrDefId);
        }

        public int CalcAttrDocListCount(Doc doc, DocListAttribute attr)
        {
            var repo = TryGetDocRepo(doc);
            if (repo != null) return repo.CalcAttrDocListCount(doc, attr);
            return 0;
        }

        public double? CalcAttrDocListSum(Doc doc, DocListAttribute attr, string sumAttrName)
        {
            var repo = TryGetDocRepo(doc);
            if (repo != null) return repo.CalcAttrDocListSum(doc, attr, sumAttrName);
            return null;
        }

        public object GetDocumentValue(Doc document, SystemIdent ident)
        {
            return GetDocRepoDefault(document).GetDocumentValue(document, ident);
        }

        public BlobData GetBlobAttrData(Guid docId, AttrDef attrDef)
        {
            var repo = TryGetDocRepo(docId);
            if (repo != null) return repo.GetBlobAttrData(docId, attrDef);
            return null;
        }

        public BlobData GetBlobAttrData(Guid docId, Guid attrDefId)
        {
            var repo = TryGetDocRepo(docId);
            if (repo != null) return repo.GetBlobAttrData(docId, attrDefId);
            return null;
        }

        public void SaveBlobAttrData(Guid docId, Guid attrDefId, byte[] data, string fileName)
        {
            GetDocRepo(docId).SaveBlobAttrData(docId, attrDefId, data, fileName);
        }

        protected IDocRepository GetDefaultDocRepo()
        {
            return _repositories.Values.First();
        }

        // TODO: —делать IObjectToDataContextMapper<TObject>, который возвращает DataContext хранимого объекта
        // Example: var mapper = Provider.Get<IObjectToDataContextMapper<DocDef>>();
        //  var dataContext = mapper.Get(docDef);
        protected virtual IDocRepository GetDocRepo(DocDef docDef)
        {
            // TODO: ƒолжен получить контекст метаданных дл€ docDef;
            // определить какой документный контекст назначен контексту метаданных и возвратить IDocRepository с док. контекстом
            return GetDefaultDocRepo();
        }

        private IDocRepository GetDocRepo(Doc document)
        {
            if (document != null && !String.IsNullOrEmpty(document.DataContextName))
                return _repositories[DataContext.Contexts.First(c => String.Equals(c.Name, document.DataContextName, StringComparison.InvariantCulture))];

            if (_repositories.Count == 1) return GetDefaultDocRepo();

            throw new ApplicationException("Ќе могу определить репозитарий дл€ документа");
        }
        private IDocRepository TryGetDocRepo(Doc document)
        {
            if (document == null || String.IsNullOrEmpty(document.DataContextName)) return null;

            IDocRepository repo = null;
            var dc = DataContext.Contexts.FirstOrDefault(c => String.Equals(c.Name, document.DataContextName, StringComparison.Ordinal));
            if (dc != null)
                _repositories.TryGetValue(dc, out repo);
            return repo;
        }
        private IDocRepository GetDocRepoDefault(Doc document)
        {
            var repo = TryGetDocRepo(document);
            return repo ?? GetDefaultDocRepo();
        }

        private IDocRepository GetDocRepo(Guid docId)
        {
            var cacheItem = DocRepository.DocCache.Find(docId);

            return cacheItem != null
                ? GetDocRepo(cacheItem.CachedObject)
                : _repositories.Values.First(repo => repo.DocExists(docId));
        }
        private IDocRepository GetDocRepoDefault(Guid docId)
        {
            var repo = TryGetDocRepo(docId);

            return repo ?? GetDefaultDocRepo();
        }
        private IDocRepository TryGetDocRepo(Guid docId)
        {
            var cacheItem = DocRepository.DocCache.Find(docId);

            return cacheItem != null
                ? TryGetDocRepo(cacheItem.CachedObject)
                : _repositories.Values.FirstOrDefault(repo => repo.DocExists(docId));
        }

        public void SaveBlobAttrData(Doc doc, Guid attrDefId, byte[] data, string fileName)
        {
            GetDocRepo(doc).SaveBlobAttrData(doc, attrDefId, data, fileName);
        }

        public void AssignDocTo(Doc source, Doc target)
        {
            source.Assign(target);
        }

        public void ReplaceRefToDoc(Guid docId1, Guid docId2)
        {
            GetDocRepo(docId1).ReplaceRefToDoc(docId1, docId2);
        }
    }
}