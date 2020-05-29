using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextDocDefRepository : IDocDefRepository
    {
        public IMultiDataContext DataContext { get; private set; }

        private readonly IList<IDocDefRepository> _repositories = new List<IDocDefRepository>();

        public MultiContextDocDefRepository(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Meta))
                    _repositories.Add(new DocDefRepository(provider, context));
            }
        }

        public void Dispose()
        {
        }

        public IEnumerable<Guid> GetDocDefDescendant(Guid docDefId)
        {
            return _repositories.SelectMany(repo => repo.GetDocDefDescendant(docDefId));
        }

        public DocDef Find(Guid docDefId)
        {
            return _repositories.Select(repo => repo.Find(docDefId)).FirstOrDefault(dd => dd != null);
        }

        public DocDef Find(string docDefName)
        {
            return _repositories.Select(repo => repo.Find(docDefName)).FirstOrDefault(dd => dd != null);
        }

        public DocDef DocDefById(Guid docDefId)
        {
            var docDef = Find(docDefId);

            if (docDef == null)
                throw new ApplicationException(
                    string.Format("Типа документа с идентификатором {0} не существует", docDefId));

            return docDef;
        }

        public DocDef DocDefByName(string docDefName)
        {
            var docDef = Find(docDefName);

            if (docDef == null)
                throw new ApplicationException(
                    string.Format("Типа документа с именем {0} не существует", docDefName));

            return docDef;
        }

        public IList<DocDefName> GetDocDefNames()
        {
            return new List<DocDefName>(_repositories.SelectMany(repo => repo.GetDocDefNames()));
        }

        public IList<DocDefRelation> GetDocDefRelations(Guid docDefId)
        {
            return new List<DocDefRelation>(_repositories.SelectMany(repo => repo.GetDocDefRelations(docDefId)));
        }

        public IList<DocDefRelation> GetDocDefRelations(DocDef docDef)
        {
            return new List<DocDefRelation>(_repositories.SelectMany(repo => repo.GetDocDefRelations(docDef)));
        }
    }
}