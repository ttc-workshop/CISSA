using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Maps;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextDocumentTableMapRepository : IDocumentTableMapRepository
    {
        public IMultiDataContext DataContext { get; private set; }

        private readonly IList<IDocumentTableMapRepository> _repositories = new List<IDocumentTableMapRepository>();

        public MultiContextDocumentTableMapRepository(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Meta))
                    _repositories.Add(new DocumentTableMapRepository(context));
            }
        }


        public DocumentTableMap Find(Guid docDefId)
        {
            return _repositories.Select(repo => repo.Find(docDefId)).FirstOrDefault(map => map != null);
        }

        public DocumentTableMap Get(Guid docDefId)
        {
            var map = Find(docDefId);

            if (map == null)
                throw new ApplicationException(String.Format("DocumentTableMap для \"{0}\" не найден", docDefId));

            return map;
        }
    }
}