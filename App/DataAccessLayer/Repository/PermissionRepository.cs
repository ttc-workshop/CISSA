using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Security;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class PermissionRepository : IPermissionRepository//, IDisposable
    {
        public IDataContext DataContext { get; private set; }
//        private readonly bool _ownDataContext;

        private readonly IOrgRepository _orgRepo;
        protected IOrgRepository OrgRepo
        {
            get
            {
                return _orgRepo /*?? (_orgRepo = new OrgRepository(DataContext))*/;
            }
        }

        private readonly IUserRepository _userRepo;

        public PermissionRepository(IDataContext dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");
            /*{
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else */
                DataContext = dataContext;

            _userRepo = new UserRepository(DataContext);
            _orgRepo = new OrgRepository(DataContext);
        }
        public PermissionRepository(IAppServiceProvider provider, IDataContext dataContext)
        {
            DataContext = dataContext; //provider.Get<IDataContext>();

            _userRepo = provider.Get<IUserRepository>();
            _orgRepo = provider.Get<IOrgRepository>();
        }

        public readonly static ObjectCache<PermissionSet> ObjectDefPermissionCache = new ObjectCache<PermissionSet>();
        public readonly static ObjectCache<PermissionSet> UserPermissionCache = new ObjectCache<PermissionSet>();
        public readonly static ObjectCache<PermissionSet> OrgPositionPermissionCache = new ObjectCache<PermissionSet>();
        public readonly static ObjectCache<PermissionSet> OrgUnitPermissionCache = new ObjectCache<PermissionSet>();
//        public readonly static ObjectCache<PermissionSet> OrganizationPermissionCache = new ObjectCache<PermissionSet>();
        public readonly static ObjectCache<IList<Guid>> RoleListCache = new ObjectCache<IList<Guid>>();

        public PermissionSet GetObjectDefPermissions(Guid objectId)
        {
            return GetObjectDefPermissions(objectId, SecurityOperation.Access);
        }

        public PermissionSet GetObjectDefPermissions(Guid objectId, SecurityOperation operation)
        {
            var cached = ObjectDefPermissionCache.Find(objectId);
            if (cached != null)
                return cached.CachedObject;

            IQueryable<Guid> permissions;
            var edc = DataContext.GetEntityDataContext();

            if (operation == SecurityOperation.Access)
                permissions = edc.Entities.Permission_Defs.Include("Permissions")
                    .Where(d => d.Def_Id == objectId && (d.Access_Type == null || d.Access_Type == 0) &&
                                (d.Permission.Deleted == null || d.Permission.Deleted == false))
                    .Select(d => d.Permission_Id);
            else
            {
                var op = (int) operation;

                permissions = edc.Entities.Permission_Defs.Include("Permissions")
                    .Where(d => d.Def_Id == objectId && d.Access_Type == op &&
                                (d.Permission.Deleted == null || d.Permission.Deleted == false))
                    .Select(d => d.Permission_Id);
            }

            var permissionSet = new PermissionSet(permissions);

            ObjectDefPermissionCache.Add(permissionSet, objectId);

            return permissionSet;
        }

        public IList<Guid> GetObjectDefRoles(Guid id)
        {
            var cached = RoleListCache.Find(id);
            if (cached != null)
                return new List<Guid>(cached.CachedObject);

            var roles = DataContext.GetEntityDataContext().Entities.Role_Refs.Include("Roles")
                .Where(r => r.Def_Id == id && (r.Role.Deleted == null || r.Role.Deleted == false))
                .Select(r => r.Role_Id).ToList();

            RoleListCache.Add(roles, id);

            return new List<Guid>(roles);
        }

        public PermissionSet GetUserPermissions(Guid userId)
        {
            var cached = UserPermissionCache.Find(userId);
            if (cached != null)
                return cached.CachedObject;

            //using (var userRepo = new UserRepository(DataContext))
            {
                var permissions = GetUserPermissions(_userRepo.GetUserInfo(userId));

                UserPermissionCache.Add(permissions, userId);

                return permissions;
            }
        }

        protected PermissionSet GetUserPermissions(UserInfo userInfo)
        {
            return GetUserPermissions(userInfo.Id, userInfo.PositionId);
        }

        protected PermissionSet GetUserPermissions(Guid userId, Guid? positionId)
        {
            var results = GetObjectDefPermissions(userId);
                //new PermissionSet(
                //DataContext.PermissionDefs.Where(d => d.Def_Id == userId).Select(d => d.Permission_Id));

            if (positionId != null)
                results.UnionWith(GetPositionPermissions((Guid) positionId));

            var roles = GetObjectDefRoles(userId);
                /*DataContext.RoleRefs.Include("Roles")
                .Where(r => (r.Def_Id == userId && (r.Role.Deleted == null || r.Role.Deleted == false)))
                .Select(r => r.Role_Id);*/

            foreach (var roleId in roles)
                results.UnionWith(GetRolePermissions(roleId));

            return results;
        }

        public PermissionSet GetPositionPermissions(Guid positionId)
        {
            var cached = OrgPositionPermissionCache.Find(positionId);
            if (cached != null)
                return cached.CachedObject;

            var results = GetObjectDefPermissions(positionId);
                //new PermissionSet(
                //DataContext.PermissionDefs.Where(d => d.Def_Id == positionId).Select(d => d.Permission_Id));

            var position = OrgRepo.GetOrgPosition(positionId);
                //DataContext.ObjectDefs.OfType<Org_Position>().First(p => p.Id == positionId);
            if (position.OrgTypeId != null)
                results.UnionWith(GetOrgUnitPermissions((Guid) position.OrgTypeId));

            var roles = GetObjectDefRoles(positionId);
                /*DataContext.RoleRefs.Include("Roles")
                .Where(
                    r =>
                    (r.Def_Id == positionId && (r.Role.Deleted == null || r.Role.Deleted == false)))
                .Select(r => r.Role_Id);*/

            foreach (var roleId in roles)
                results.UnionWith(GetRolePermissions(roleId));

            OrgPositionPermissionCache.Add(results, positionId);

            return results;
        }

        public PermissionSet GetOrgUnitPermissions(Guid orgUnitId)
        {
            var cached = OrgUnitPermissionCache.Find(orgUnitId);
            if (cached != null)
                return cached.CachedObject;

            var results = GetObjectDefPermissions(orgUnitId);
                /*new PermissionSet(
                DataContext.PermissionDefs.Where(d => d.Def_Id == orgUnitId).Select(d => d.Permission_Id));*/

            var orgUnit = OrgRepo.FindOrgType(orgUnitId);
                //DataContext.ObjectDefs.OfType<Org_Unit>().FirstOrDefault(p => p.Id == orgUnitId);
            if (orgUnit != null && orgUnit.ParentId != null)
                results.UnionWith(GetOrgUnitPermissions((Guid) orgUnit.ParentId));

            var roles = GetObjectDefRoles(orgUnitId);
                /*DataContext.RoleRefs.Include("Roles")
                .Where(
                    r =>
                    (r.Def_Id == orgUnitId && (r.Role.Deleted == null || r.Role.Deleted == false)))
                .Select(r => r.Role_Id);*/

            foreach (var roleId in roles)
                results.UnionWith(GetRolePermissions(roleId));

            OrgUnitPermissionCache.Add(results, orgUnitId);

            return results;
        }

        public PermissionSet GetRolePermissions(Guid roleId)
        {
            var cached = ObjectDefPermissionCache.Find(roleId);
            if (cached != null)
                return cached.CachedObject;

            var permissions = GetObjectDefPermissions(roleId);
                //new PermissionSet(DataContext.PermissionDefs.Where(d => d.Def_Id == roleId).Select(d => d.Permission_Id));

            permissions.UnionWith(
                DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Permission>()
                    .Where(p => p.Parent_Id == roleId && (p.Deleted == null || p.Deleted == false))
                    .Select(p => p.Id));

            ObjectDefPermissionCache.Add(permissions, roleId);
            
            return permissions;
        }
/*
        /// <summary>
        /// Получает список доступных объектов для пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Список доступных объектов и их разрешения</returns>
        //[SmartCache(TimeOutSeconds = 600)]
        [Obsolete("Устаревший метод")]
        public ObjectDefPermissionCollection ListOfAccessibleObjects(Guid userId)
        {
            var permissionCollection = new ObjectDefPermissionCollection();

            if (userId == Guid.Empty) return permissionCollection;

            IEnumerable<Guid> allRolesId = GetUserRoles(DataContext, userId);

            // загрузка через обычные разрешения (НАЧАЛО)

            var allPerDefs = DataContext.ObjectDefs.OfType<Role>()
                .Include("Permissions")
                .Include("Permissions.Permission_Defs")
                .Where(x => allRolesId.Contains(x.Id))
                .SelectMany(f => f.Permissions.SelectMany(p => p.Permission_Defs));

            var xquery = from pd in allPerDefs
                         select new BizObjectPermission
                                    {
                                        ObjectId = pd.Def_Id,
                                        Permission = new BizPermission
                                                         {
                                                             AllowSelect = pd.AllowSelect,
                                                             AllowDelete = pd.AllowDelete,
                                                             AllowUpdate = pd.AllowUpdate,
                                                             AllowInsert = pd.AllowInsert
                                                         }
                                    };

            foreach (BizObjectPermission docDefPermission in xquery.AsEnumerable())
            {
                BizObjectPermission permission = docDefPermission;
                docDefPermission.BizObjectType =
                    DataContext.ObjectDefs.OfType<Enum_Def>().Where(e => e.Id == permission.ObjectId).Any()
                        ? BizObjectType.EnumDef
                        : BizObjectType.DocumentDef;

                permissionCollection.Add(docDefPermission);
            }

            // загрузка через обычные разрешения (КОНЕЦ)


            // загрузка через иерархические разрешения (НАЧАЛО)

            var allPerDefs2 = DataContext.ObjectDefs.OfType<Role>()
                .Include("Permissions")
                .Include("Permissions.Permission_Defs")
                .Where(x => allRolesId.Contains(x.Id))
                .SelectMany(f => f.Permissions.SelectMany(p => p.Permission_Defs));

            var orgUnitPermmisionsQuery =
                from per in allPerDefs2
                select new
                           {
                               //ObjectDefId = objectDefId,
                               OrgUnitId = per.Def_Id,
                               Permission = new BizPermission
                                                {
                                                    AllowSelect = per.AllowSelect,
                                                    AllowDelete = per.AllowDelete,
                                                    AllowUpdate = per.AllowUpdate,
                                                    AllowInsert = per.AllowInsert
                                                },
                               HierarchyLevel = (int?) 0 // per.HierarchyLevel
                           };


            foreach (var item in orgUnitPermmisionsQuery)
            {
                IEnumerable<Guid> objectDefs = GetObjectDefsForUnit(DataContext, item.OrgUnitId, item.HierarchyLevel ?? 0);

                foreach (Guid objectDefGuid in objectDefs)
                {
                    Guid guid = objectDefGuid;
                    var docPermiss = new BizObjectPermission
                                         {
                                             ObjectId = objectDefGuid,
                                             BizObjectType =
                                                 DataContext.ObjectDefs.OfType<Enum_Def>().Where(e => e.Id == guid).Any()
                                                     ? BizObjectType.EnumDef
                                                     : BizObjectType.DocumentDef,
                                             Permission = item.Permission
                                         };

                    permissionCollection.Add(docPermiss);
                }
            }

            // загрузка через обычные разрешения (КОНЕЦ)

            return permissionCollection;
        }

        [Obsolete("Устаревший метод")]
        private static IEnumerable<Guid> GetObjectDefsForUnit(DataContext context, Guid unitId, int hierarhiLevel)
        {
            if (hierarhiLevel == 0) return new List<Guid>();

            var ouQuery = from ou in context.ObjectDefs.OfType<Org_Unit>().Include("Object_Defs")
                          where ou.Id == unitId
                          select ou;

            if (!ouQuery.Any())
            {
                throw new ApplicationException(string.Format("Подразделения с кодом {0} не существует", unitId));
            }

            var orgUnit = ouQuery.First();

            var result = new List<Guid>();
            result.AddRange(orgUnit.Object_Defs.Select(o => o.Id));

            // загрузка детей
            if (orgUnit.Children != null && orgUnit.Children.Count > 0)
            {
                foreach (Subject subject in orgUnit.Children)
                {
                    var ou = subject as Org_Unit;
                    if (ou != null)
                    {
                        var objDefs = GetObjectDefsForUnit(context, ou.Id, hierarhiLevel - 1);
                        result.AddRange(objDefs);
                    }
                }
            }

            return result;
        }

        [Obsolete("Устаревший метод")]
        private static IEnumerable<Guid> GetUserRoles(DataContext context, Guid userId)
        {
            var userQuery = from u in context.ObjectDefs.OfType<Worker>()
                            where u.Id == userId
                            select u;

            if (!userQuery.Any())
            {
                throw new ApplicationException(
                    string.Format("Пользователя с идентификатором {0} не существует", userId));
            }

            var user = userQuery.First();

            if (!user.Roles.IsLoaded) user.Roles.Load();

            IEnumerable<Role> allRoles = user.Roles;

            if (user.Org_Positions != null)
            {
                if (!user.Org_Positions.Roles.IsLoaded) user.Org_Positions.Roles.Load();
                allRoles = allRoles.Union(user.Org_Positions.Roles);

                if (user.Org_Positions.Parent != null)
                {
                    if (!((Org_Unit) user.Org_Positions.Parent).Roles.IsLoaded)
                        ((Org_Unit) user.Org_Positions.Parent).Roles.Load();
                    allRoles = allRoles.Union(((Org_Unit) user.Org_Positions.Parent).Roles);
                }
            }

            return allRoles.Select(r => r.Id);
        }
*/
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
                    Logger.OutputLog(e, "PermissionRepository.Dispose");
                    throw;
                }
            }
        }

        ~PermissionRepository()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "PermissionRepository.Finalize");
                    throw;
                }
        }
*/
    }
}
