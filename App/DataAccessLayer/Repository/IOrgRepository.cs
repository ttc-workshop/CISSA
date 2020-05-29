using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Organizations;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IOrgRepository //: IDisposable
    {
        OrgTypeInfo FindOrgType(Guid orgTypeId);
        OrgTypeInfo GetOrgType(Guid orgTypeId);
        OrgInfo Find(Guid orgId);
        OrgInfo Find(string orgName);
        OrgInfo Get(Guid orgId);
        OrgInfo Get(string orgName);
        bool TryGetOrgName(Guid orgId, out string orgName);
        string GetOrgName(Guid orgId);
        OrgPositionInfo FindOrgPosition(Guid orgPositionId);
        OrgPositionInfo GetOrgPosition(Guid orgPositionId);
        string GetOrgPositionName(Guid posId);
        Guid GetOrgIdByName(string orgName);
        Guid? TryGetOrgIdByCode(string orgCode);
        Guid GetOrgIdByCode(string orgCode);
        IEnumerable<Guid> GetOrganizations(Guid? orgTypeId);

        OrgInfo FindByCode(string orgCode); // Добавлено для реализации млути контекстного доступа
    }
}