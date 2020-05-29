using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Providers
{
    public interface IDocDefStateListProvider
    {
        IEnumerable<DocStateType> Get(Guid docDefId);
    }
}
