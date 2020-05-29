using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Security;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextPermissionRepository : IPermissionRepository
    {
        public IMultiDataContext DataContext { get; private set; }

        private readonly IList<IPermissionRepository> _repositories = new List<IPermissionRepository>();

        public MultiContextPermissionRepository(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Account))
                    _repositories.Add(new PermissionRepository(provider, context));
            }
        }
        public PermissionSet GetObjectDefPermissions(Guid objectId)
        {
            var set = new PermissionSet();
            foreach (var repo in _repositories)
            {
                set.UnionWith(repo.GetObjectDefPermissions(objectId));
            }
            return set;
        }

        public PermissionSet GetUserPermissions(Guid userId)
        {
            var set = new PermissionSet();
            foreach (var repo in _repositories)
            {
                set.UnionWith(repo.GetUserPermissions(userId));
            }
            return set;
        }

        public PermissionSet GetPositionPermissions(Guid positionId)
        {
            var set = new PermissionSet();
            foreach (var repo in _repositories)
            {
                set.UnionWith(repo.GetPositionPermissions(positionId));
            }
            return set;
        }

        public PermissionSet GetOrgUnitPermissions(Guid orgUnitId)
        {
            var set = new PermissionSet();
            foreach (var repo in _repositories)
            {
                set.UnionWith(repo.GetOrgUnitPermissions(orgUnitId));
            }
            return set;
        }

        public PermissionSet GetRolePermissions(Guid roleId)
        {
            var set = new PermissionSet();
            foreach (var repo in _repositories)
            {
                set.UnionWith(repo.GetRolePermissions(roleId));
            }
            return set;
        }
    }
}