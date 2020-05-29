using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextQueryRepository : IQueryRepository
    {
        private IAppServiceProvider Provider { get; set; }
        private IMultiDataContext DataContext { get; set; }

        private readonly IDictionary<IDataContext, IQueryRepository> _repositories = new Dictionary<IDataContext, IQueryRepository>();

        public MultiContextQueryRepository(IAppServiceProvider provider)
        {
            Provider = provider;
            DataContext = provider.Get<IMultiDataContext>();
            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Meta))
                    _repositories.Add(context, new QueryRepository(Provider, context));
            }
        }
        public QuerySourceDefData FindJoinDef(Guid id)
        {
            return _repositories.Values.Select(repo => repo.FindJoinDef(id)).FirstOrDefault();
        }

        public QueryConditionDefData FindConditionDef(Guid id)
        {
            return _repositories.Values.Select(repo => repo.FindConditionDef(id)).FirstOrDefault();
        }

        public QueryDefData FindQuery(Guid id)
        {
            return _repositories.Values.Select(repo => repo.FindQuery(id)).FirstOrDefault();
        }

        public QueryDefData GetQuery(Guid id)
        {
            var query = FindQuery(id);

            if (query == null)
                throw new ApplicationException(String.Format("Запрос с Id = \"{0}\" не найден!", id));

            return query;
        }
    }
}