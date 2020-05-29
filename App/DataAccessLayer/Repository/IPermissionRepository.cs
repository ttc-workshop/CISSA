using System;
using Intersoft.CISSA.DataAccessLayer.Model.Security;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    /// <summary>
    /// Определяет интерфейс доступа к разрешениям
    /// </summary>
    public interface IPermissionRepository
    {
//        /// <summary>
//        /// Получает список доступных объектов для пользователя
//        /// </summary>
//        /// <param name="userId">Идентификатор пользователя</param>
//        /// <returns>Список доступных объектов и их разрешения</returns>
//        ObjectDefPermissionCollection ListOfAccessibleObjects(Guid userId);

        PermissionSet GetObjectDefPermissions(Guid objectId);
        PermissionSet GetUserPermissions(Guid userId);
//        PermissionSet GetUserPermissions(Guid userId, Guid? positionId);
        PermissionSet GetPositionPermissions(Guid positionId);
        PermissionSet GetOrgUnitPermissions(Guid orgUnitId);
        PermissionSet GetRolePermissions(Guid roleId);
    }
}
