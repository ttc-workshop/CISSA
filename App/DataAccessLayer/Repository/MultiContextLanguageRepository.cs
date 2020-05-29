using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Languages;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextLanguageRepository : ILanguageRepository
    {
        public IMultiDataContext DataContext { get; private set; }

        private readonly IList<ILanguageRepository> _repositories = new List<ILanguageRepository>();

        public MultiContextLanguageRepository(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Meta))
                    _repositories.Add(new LanguageRepository(provider, context));
            }
        }

        public IList<LanguageType> Load()
        {
            var list = new List<LanguageType>();
            foreach (var repo in _repositories)
            {
                list.AddRange(repo.Load());
            }
            return list;
        }

        public string GetTranslation(Guid defId, int languageId)
        {
            return _repositories.Select(repo => repo.GetTranslation(defId, languageId)).FirstOrDefault(s => !String.IsNullOrEmpty(s));
        }
    }
}