using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Organizations;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class OrgRepository : IOrgRepository
    {
        private IDataContext DataContext { get; set; }
//        private readonly bool _ownDataContext;

//        public Guid UserId { get; private set; }

        public OrgRepository(IDataContext dataContext/*, Guid userId*/)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");
            /*{
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else*/
                DataContext = dataContext;
//            UserId = userId;
        }
        public OrgRepository(IAppServiceProvider provider, IDataContext dataContext)
        {
            DataContext = dataContext;  // provider.Get<IDataContext>();
        }
//        public OrgRepository(Guid userId) : this(null, userId) {}
//        public OrgRepository() : this((IDataContext) null/*, Guid.Empty*/) { }

        public static readonly ObjectCache<OrgInfo> OrgInfoCache = new ObjectCache<OrgInfo>();
        public static readonly ObjectCache<OrgTypeInfo> OrgTypeInfoCache = new ObjectCache<OrgTypeInfo>();
        public static readonly ObjectCache<OrgPositionInfo> OrgPositionInfoCache = new ObjectCache<OrgPositionInfo>();

        public OrgTypeInfo FindOrgType(Guid orgTypeId)
        {
            var cached = OrgTypeInfoCache.Find(orgTypeId);
            if (cached != null)
                return cached.CachedObject;

            var orgType = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Org_Unit>().FirstOrDefault(o => o.Id == orgTypeId);
            if (orgType != null)
            {
                var orgTypeInfo = new OrgTypeInfo
                {
                    Id = orgType.Id,
                    Name = orgType.Full_Name,
                    ParentId = orgType.Parent_Id
                };
                OrgTypeInfoCache.Add(orgTypeInfo, orgTypeId);

                return orgTypeInfo;
            }

            return null;
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
            var cached = OrgInfoCache.Find(orgId);
            if (cached != null)
                return cached.CachedObject;

            var org = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Organization>().FirstOrDefault(o => o.Id == orgId);
            if (org != null)
            {
                var orgInfo = new OrgInfo
                                  {
                                      Id = org.Id,
                                      Code = org.Code,
                                      Name = org.Full_Name,
                                      TypeId = org.Type_Id,
                                      Type = org.Type_Id != null ? GetOrgType((Guid) org.Type_Id) : null
                                  };
                OrgInfoCache.Add(orgInfo, orgId);

                return orgInfo;
            }

            return null;
        }

        public OrgInfo Find(string orgName)
        {
            var cached =
                OrgInfoCache/*.GetItems()*/
                    .FirstOrDefault(
                        o => String.Equals(o.CachedObject.Name, orgName, StringComparison.OrdinalIgnoreCase));

            if (cached != null)
                return cached.CachedObject;

            var org =
                DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Organization>().FirstOrDefault(
                    o => o.Full_Name.ToUpper() == orgName.ToUpper());

            if (org != null)
            {
                var orgInfo = new OrgInfo
                {
                    Id = org.Id,
                    Code = org.Code,
                    Name = org.Full_Name,
                    TypeId = org.Type_Id,
                    Type = org.Type_Id != null ? GetOrgType((Guid) org.Type_Id) : null
                };
                OrgInfoCache.Add(orgInfo, org.Id);

                return orgInfo;
            }

            return null;
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

        //[SmartCache(TimeOutSeconds = 3600)]
        public bool TryGetOrgName(Guid orgId, out string orgName)
        {
/*            var org = DataContext.ObjectDefs.OfType<Organization>().Where(o => o.Id == orgId);
            if (org.Any())
            {
                orgName = org.First().Full_Name;
                return true;
            }*/
            var orgInfo = Find(orgId);

            orgName = orgInfo != null ? orgInfo.Name : String.Empty;

            return orgInfo != null;
        }

        public string GetOrgName(Guid orgId)
        {
           return Get(orgId).Name;
        }

        public OrgPositionInfo FindOrgPosition(Guid orgPositionId)
        {
            var cached = OrgPositionInfoCache.Find(orgPositionId);
            if (cached != null)
                return cached.CachedObject;

            var orgPosition = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Org_Position>().FirstOrDefault(o => o.Id == orgPositionId);
            if (orgPosition != null)
            {
                var orgPositionInfo = new OrgPositionInfo
                {
                    Id = orgPosition.Id,
                    Name = orgPosition.Full_Name,
                    OrgTypeId = orgPosition.Parent_Id
                };
                OrgPositionInfoCache.Add(orgPositionInfo, orgPositionId);

                return orgPositionInfo;
            }

            return null;
        }

        public OrgPositionInfo GetOrgPosition(Guid orgPositionId)
        {
            var orgPositionInfo = FindOrgPosition(orgPositionId);

            if (orgPositionInfo == null)
                throw new ApplicationException(String.Format("Орг. позиция с Id = \"{0}\" не найдена!", orgPositionId));

            return orgPositionInfo;
        }

        //[SmartCache(TimeOutSeconds = 3600)]
        public bool TryGetOrgPositionName(Guid posId, out string posName)
        {
            /*var pos = DataContext.ObjectDefs.OfType<Org_Position>().Where(o => o.Id == posId);
            if (pos.Any())
            {
                posName = pos.First().Full_Name;
                return true;
            }*/
            var orgPosition = FindOrgPosition(posId);

            posName = orgPosition != null ? orgPosition.Name : String.Empty;
            return orgPosition != null;
        }

        public string GetOrgPositionName(Guid posId)
        {
            string posName;

            if (TryGetOrgPositionName(posId, out posName)) return posName;

            throw new ApplicationException(String.Format("Организация с Id = {0} не найдена", posId));
        }

        public Guid? TryGetOrgIdByName(string orgName)
        {
            /*var query = from item in DataContext.ObjectDefs.OfType<Organization>()
                        where
                            item.Name.ToUpper() == orgName.ToUpper() ||
                            item.Full_Name.ToUpper() == orgName.ToUpper()
                        select item.Id;

            if (!query.Any()) return null;

            return query.First();*/
            var orgInfo = Find(orgName);

            return orgInfo != null ? orgInfo.Id : (Guid?) null;
        }

        public Guid GetOrgIdByName(string orgName)
        {
            var orgId = TryGetOrgIdByName(orgName);

            if (orgId == null)
            {
                throw new ApplicationException(
                    string.Format("Организации с именем '{0}' не существует.", orgName));
            }

            return (Guid) orgId;
        }

        public OrgInfo FindByCode(string orgCode)
        {
            var cached =
                OrgInfoCache/*.GetItems()*/
                    .FirstOrDefault(
                        o => String.Equals(o.CachedObject.Code, orgCode, StringComparison.OrdinalIgnoreCase));

            if (cached != null)
                return cached.CachedObject;

            var org =
                DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Organization>().FirstOrDefault(
                    o => o.Code.ToUpper() == orgCode.ToUpper());

            if (org != null)
            {
                var orgInfo = new OrgInfo
                {
                    Id = org.Id,
                    Code = org.Code,
                    Name = org.Full_Name,
                    TypeId = org.Type_Id,
                    Type = org.Type_Id != null ? GetOrgType((Guid)org.Type_Id) : null
                };
                OrgInfoCache.Add(orgInfo, org.Id);

                return orgInfo;
            }

            return null;
        }

        public Guid? TryGetOrgIdByCode(string orgCode)
        {
            /*var query = from item in DataContext.ObjectDefs.OfType<Organization>()
                        where item.Code.ToUpper() == orgCode.ToUpper()
                        select item.Id;

            if (!query.Any()) return null;

            return query.First();*/
            var orgInfo = FindByCode(orgCode);

            return orgInfo != null ? orgInfo.Id : (Guid?) null;
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

        public static readonly ObjectCache<IList<Guid>> OrganizationListCache = new ObjectCache<IList<Guid>>();

        public IEnumerable<Guid> GetOrganizations(Guid? orgTypeId)
        {
            var orgList = OrganizationListCache.Find(orgTypeId ?? Guid.Empty);
            if (orgList != null)
                return new List<Guid>(orgList.CachedObject);

            IList<Guid> list;
            var edc = DataContext.GetEntityDataContext();
            if (orgTypeId != null)
                list = edc.Entities.Object_Defs.OfType<Organization>()
                    .Where(o => o.Type_Id == orgTypeId && (o.Deleted == null || o.Deleted == false))
                    .OrderBy(o => o.Full_Name)
                    .Select(o => o.Id).ToList();
            else
                list = edc.Entities.Object_Defs.OfType<Organization>()
                    .Where(o => o.Deleted == null || o.Deleted == false).OrderBy(o => o.Full_Name)
                    .Select(o => o.Id).ToList();

            OrganizationListCache.Add(list, orgTypeId ?? Guid.Empty);

            return list;
        }
/*
        public void Dispose()
        {
            if (_ownDataContext && DataContext != null)
            {
                try
                {
                    DataContext.Dispose();
                    DataContext = null;
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "OrgRepository.Dispose");
                    throw;
                }
            }
        }

        ~OrgRepository()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "OrgRepository.Finalize");
                    throw;
                }
        }*/
    }
}
