using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Intersoft.CISSA.DataAccessLayer.Model.Security
{
    public class ObjectDefPermissionCollection : IEnumerable<BizObjectPermission>
    {
        private readonly List<BizObjectPermission> _objDefPermissions;

        public int Count { get { return _objDefPermissions.Count; } }
 
        public ObjectDefPermissionCollection()
        {
            _objDefPermissions = new List<BizObjectPermission>();
        }

        public void Add(BizObjectPermission permission)
        {
            var query = from per in _objDefPermissions
                        where per.ObjectId == permission.ObjectId
                        select per;

            if (query.Any())
            {
                var findedPermission = query.First();
                findedPermission.Permission += permission.Permission;
            }
            else
            {
                _objDefPermissions.Add(permission);
            }
        }

        public IEnumerable<BizObjectPermission> GetIdByBizObjectType (BizObjectType bizObjectType)
        {
            var query = from item in _objDefPermissions
                        where item.BizObjectType == bizObjectType
                        select item;

            return query.AsEnumerable();
        }

        public BizPermission GetPermissionsForObjectId(Guid objectId)
        {
            var query = from item in _objDefPermissions
                        where item.ObjectId == objectId
                        select item;
            
            if (!query.Any())
            {
                return new BizPermission(false);
            }
            return query.First().Permission;
        }

        /// <summary>
        /// Возвращает перечислитель, выполняющий итерацию в коллекции.
        /// </summary>
        /// <returns>
        /// Интерфейс <see cref="T:System.Collections.Generic.IEnumerator`1"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<BizObjectPermission> GetEnumerator()
        {
            return _objDefPermissions.GetEnumerator();
        }

        /// <summary>
        /// Возвращает перечислитель, который осуществляет перебор элементов коллекции.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.Collections.IEnumerator"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
