using System;
using Intersoft.CISSA.DataAccessLayer.Model.Maps;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IDocumentTableMapRepository
    {
        DocumentTableMap Find(Guid docDefId);
        DocumentTableMap Get(Guid docDefId);
    }
}
