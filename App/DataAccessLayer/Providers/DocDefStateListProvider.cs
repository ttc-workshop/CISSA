using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Storage;

namespace Intersoft.CISSA.DataAccessLayer.Providers
{
    public class DocDefStateListProvider: IDocDefStateListProvider
    {
        private readonly IDocumentStorage _docStorage;
        private readonly IDocStateRepository _docStateRepo;

        public DocDefStateListProvider(IAppServiceProvider provider, IDataContext dataContext)
        {
            _docStorage = provider.Get<IDocumentStorage>(dataContext);
            _docStateRepo = provider.Get<IDocStateRepository>();
        }

        public IEnumerable<DocStateType> Get(Guid docDefId)
        {
            return _docStorage.GetDocDefStateTypes(docDefId).Select(stateTypeId => _docStateRepo.LoadById(stateTypeId));
        }
    }

    public class MultiContextDocDefStateListProvider : IDocDefStateListProvider
    {
        private readonly IAppServiceProvider _provider;
        private readonly IMultiDataContext _dataContext;

        public MultiContextDocDefStateListProvider(IAppServiceProvider provider, IMultiDataContext dataContext)
        {
            _provider = provider;
            _dataContext = dataContext;
        }

        public IEnumerable<DocStateType> Get(Guid docDefId)
        {
            return
                _dataContext.Contexts.Where(context => context.DataType.HasFlag(DataContextType.Document))
                    .Select(context => _provider.Get<IDocDefStateListProvider>(context))
                    .SelectMany(prov => prov.Get(docDefId));
        }
    }
}