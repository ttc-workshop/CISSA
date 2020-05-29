using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextDocStateRepository : IDocStateRepository
    {
        public IMultiDataContext DataContext { get; private set; }

        private readonly IList<IDocStateRepository> _repositories = new List<IDocStateRepository>();

        public MultiContextDocStateRepository(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Meta))
                    _repositories.Add(new DocStateRepository(context));
            }
        }

        public DocStateType TryLoadById(Guid stateId)
        {
            return _repositories.Select(repo => repo.TryLoadById(stateId)).FirstOrDefault(dst => dst != null);
        }

        public DocStateType LoadById(Guid stateId)
        {
            var stateType = TryLoadById(stateId);

            if (stateType == null)
                throw new ApplicationException(String.Format("Состояния с идентификатором {0} не существует", stateId));

            return stateType;
        }

        public DocStateType TryLoadByName(string stateName)
        {
            return _repositories.Select(repo => repo.TryLoadByName(stateName)).FirstOrDefault(dst => dst != null);
        }

        public DocStateType LoadByName(string stateName)
        {
            var stateType = TryLoadByName(stateName);

            if (stateType == null)
                throw new ApplicationException(String.Format("Состояния с именем \"{0}\" не существует", stateName));

            return stateType;
        }

        public Guid GetDocStateTypeId(string stateName)
        {
            return LoadByName(stateName).Id;
        }
    }
}