using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Organizations;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextOrgRepository: IOrgRepository
    {
        private IMultiDataContext DataContext { get; set; }

        private readonly IList<IOrgRepository> _repositories = new List<IOrgRepository>();

        public MultiContextOrgRepository(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Account))
                {
                    _repositories.Add(new OrgRepository(context));
                }
            }
        }

        public OrgTypeInfo FindOrgType(Guid orgTypeId)
        {
            return _repositories.Select(repo => repo.FindOrgType(orgTypeId)).FirstOrDefault(oti => oti != null);
        }

        public OrgTypeInfo GetOrgType(Guid orgTypeId)
        {
            var orgTypeInfo = FindOrgType(orgTypeId);

            if (orgTypeInfo == null)
                throw new ApplicationException(String.Format("Тип организации с Id = \"{0}\" не найден!", orgTypeId));

            return orgTypeInfo;
        }

        public OrgInfo Find(Guid orgId)
        {
            return _repositories.Select(repo => repo.Find(orgId)).FirstOrDefault(oi => oi != null);
        }

        public OrgInfo Find(string orgName)
        {
            return _repositories.Select(repo => repo.Find(orgName)).FirstOrDefault(oi => oi != null);
        }

        public OrgInfo Get(Guid orgId)
        {
            var orgInfo = Find(orgId);

            if (orgInfo == null)
                throw new ApplicationException(String.Format("Организация \"{0}\" не найдена!", orgId));

            return orgInfo;
        }

        public OrgInfo Get(string orgName)
        {
            var orgInfo = Find(orgName);

            if (orgInfo == null)
                throw new ApplicationException(String.Format("Организация \"{0}\" не найдена!", orgName));

            return orgInfo;
        }

        public bool TryGetOrgName(Guid orgId, out string orgName)
        {
            var org = Find(orgId);
            orgName = org != null ? org.Name : String.Empty;
            return org != null;
        }

        public string GetOrgName(Guid orgId)
        {
            return Get(orgId).Name;
        }

        public OrgPositionInfo FindOrgPosition(Guid orgPositionId)
        {
            return _repositories.Select(repo => repo.FindOrgPosition(orgPositionId)).FirstOrDefault(opi => opi != null);
        }

        public OrgPositionInfo GetOrgPosition(Guid orgPositionId)
        {
            var orgPosition = FindOrgPosition(orgPositionId);

            if (orgPosition != null) return orgPosition;

            throw new ApplicationException(String.Format("Организация с Id = {0} не найдена", orgPositionId));
        }

        public string GetOrgPositionName(Guid posId)
        {
            var orgPosition = FindOrgPosition(posId);

            if (orgPosition != null) return orgPosition.Name;

            throw new ApplicationException(String.Format("Организация с Id = {0} не найдена", posId));
        }

        public Guid GetOrgIdByName(string orgName)
        {
            var org = Find(orgName);

            return org != null ? org.Id : Guid.Empty;
        }

        public OrgInfo FindByCode(string orgCode)
        {
            return _repositories.Select(repo => repo.FindByCode(orgCode)).FirstOrDefault(i => i != null);
        }
        public Guid? TryGetOrgIdByCode(string orgCode)
        {
            var orgInfo = FindByCode(orgCode);

            return orgInfo != null ? orgInfo.Id : (Guid?)null;
        }

        public Guid GetOrgIdByCode(string orgCode)
        {
            var orgId = TryGetOrgIdByCode(orgCode);

            if (orgId == null)
            {
                throw new ApplicationException(
                    string.Format("Организации с кодом '{0}' не существует.", orgCode));
            }

            return (Guid) orgId;
        }

        public IEnumerable<Guid> GetOrganizations(Guid? orgTypeId)
        {
            var list = new List<Guid>();
            foreach (var repo in _repositories)
            {
                list.AddRange(repo.GetOrganizations(orgTypeId));
            }
            return list;
        }
    }
}