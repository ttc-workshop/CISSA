using System;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IDocumentNumberGenerator
    {
        long GetNewId(Guid orgId, Guid docDefId);
    }
}