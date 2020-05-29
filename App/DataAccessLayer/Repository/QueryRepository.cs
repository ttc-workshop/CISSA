using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Security;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class QueryRepository : IQueryRepository
    {
        private IDataContext DataContext { get; set; }

        private readonly Guid _userId;

        private readonly IDocDefRepository _docDefRepo;
        private readonly IPermissionRepository _permissionRepo;

        public QueryRepository(IAppServiceProvider provider, IDataContext dataContext)
        {
            DataContext = dataContext;

            _userId = provider.GetCurrentUserId();

            _docDefRepo = provider.Get<IDocDefRepository>();
            _permissionRepo = provider.Get<IPermissionRepository>();
        }

        public static readonly ConcurrentDictionary<string, ObjectCache<QueryItemDefData>> QueryDefCache = new ConcurrentDictionary<string, ObjectCache<QueryItemDefData>>();
        // public readonly static object QueryDefCacheLock = new object();
        public static readonly ReaderWriterLock QueryDefCacheLock = new ReaderWriterLock();
        private const int LockTimeout = 500000;

        public QuerySourceDefData FindJoinDef(Guid id)
        {
            var result = FindInCache<QuerySourceDefData>(id);
            if (result == null)
            {
                // lock (QueryDefCacheLock)
                QueryDefCacheLock.AcquireWriterLock(LockTimeout);
                try
                {
                    result = FindInCache<QuerySourceDefData>(id);
                    if (result == null)
                    {
                        var edc = DataContext.GetEntityDataContext().Entities;
                        var querySource = edc.Object_Defs.OfType<Query_Source>().FirstOrDefault(c => c.Id == id);

                        if (querySource == null) return null;
                        result = CreateQuerySource(querySource);
                        AddToCache(result);
                    }
                }
                finally
                {
                    QueryDefCacheLock.ReleaseWriterLock();
                }
            }

            result = (QuerySourceDefData) /*CheckItemPermissions(*/ Clone(result);
            /*result.DocDef = result.DocumentId != null
                ? _docDefRepo.DocDefById((Guid) result.DocumentId)
                : null;
            result.Query = result.QueryId != null ? FindQuery((Guid) result.QueryId) : null;*/
            InitQueryItem(result);

            return result;
        }

        public QueryConditionDefData FindConditionDef(Guid id)
        {
            var result = FindInCache<QueryConditionDefData>(id);
            if (result == null)
            {
                QueryDefCacheLock.AcquireWriterLock(LockTimeout);
                try
                {
                    result = FindInCache<QueryConditionDefData>(id);
                    if (result == null)
                    {
                        var edc = DataContext.GetEntityDataContext().Entities;
                        var condition = edc.Object_Defs.OfType<Condition>().FirstOrDefault(c => c.Id == id);

                        if (condition == null) return null;

                        result = CreateQueryCondition(condition);
                        AddToCache(result);
                    }
                }
                finally
                {
                    QueryDefCacheLock.ReleaseWriterLock();
                }
            }
            return (QueryConditionDefData) /*CheckItemPermissions(*/Clone(result);
        }

        public QueryDefData FindQuery(Guid id)
        {
            var result = FindInCache<QueryDefData>(id);
            if (result == null)
            {
                QueryDefCacheLock.AcquireWriterLock(LockTimeout);
                try
                {
                    result = FindInCache<QueryDefData>(id);
                    if (result == null)
                    {
                        var edc = DataContext.GetEntityDataContext().Entities;
                        var query = edc.Object_Defs.OfType<Query>().FirstOrDefault(c => c.Id == id);

                        if (query == null) return null;
                        result = CreateQuery(query);
                        AddToCache(result);
                    }
                }
                finally
                {
                    QueryDefCacheLock.ReleaseWriterLock();
                }
            }
            result = (QueryDefData) /*CheckItemPermissions(*/Clone(result);

            //result.DocDef = result.DocumentId != null ? _docDefRepo.Find(result.DocumentId ?? Guid.Empty) : null;
            InitQueryItem(result);

            return result;
        }

        public QueryDefData GetQuery(Guid id)
        {
            var query = FindQuery(id);

            if (query == null)
                throw new ApplicationException(String.Format("Запрос с Id = \"{0}\" не найден!", id));

            return query;
        }

        private void InitQueryItem(QueryItemDefData item)
        {
            var queryData = item as QueryDefData;
            if (queryData != null)
            {
                queryData.DocDef = queryData.DocumentId != null
                    ? _docDefRepo.Find(queryData.DocumentId ?? Guid.Empty)
                    : null;
            }
            else
            {
                var sourceData = item as QuerySourceDefData;
                if (sourceData != null)
                {
                    sourceData.DocDef = sourceData.DocumentId != null
                        ? _docDefRepo.DocDefById((Guid) sourceData.DocumentId)
                        : null;
                    sourceData.Query = sourceData.QueryId != null ? FindQuery((Guid) sourceData.QueryId) : null;
                }
                else
                {
                    var conditionData = item as QueryConditionDefData;
                    if (conditionData != null)
                    {
                        Guid id;
                        if (!String.IsNullOrEmpty(conditionData.LeftSourceName))
                        {
                            if (Guid.TryParse(conditionData.LeftSourceName.Trim(), out id))
                            {
                                conditionData.LeftSourceId = id;

                                conditionData.LeftQuery = FindQuery(id);
                            }
                        }
                        if (!String.IsNullOrEmpty(conditionData.RightSourceName))
                        {
                            if (Guid.TryParse(conditionData.RightSourceName.Trim(), out id))
                            {
                                conditionData.RightSourceId = id;

                                conditionData.RightQuery = FindQuery(id);
                            }
                        }
                    }
                }
            }

            if (item.Items != null)
                foreach(var sub in item.Items)
                    InitQueryItem(sub);
        }

        private T FindInCache<T>(Guid id) where T : QueryItemDefData
        {
            // lock (QueryDefCacheLock)
            QueryDefCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                ObjectCache<QueryItemDefData> cache;
                if (QueryDefCache.TryGetValue(DataContext.Name, out cache))
                {
                    var item = cache.Find(id);
                    if (item != null)
                    {
                        return item.CachedObject as T;
                    }
                }
                return null;
            }
            finally
            {
                QueryDefCacheLock.ReleaseReaderLock();
            }
        }

        private QueryDefData CreateQuery(Query query)
        {
            var result = new QueryDefData
            {
                Id = query.Id,
                Alias = query.Alias,
                DocumentId = query.Document_Id,
                Permissions = _permissionRepo.GetObjectDefPermissions(query.Id)
            };

            AddQuerySourceChildren(result, query.Id);

            return result;
        }

        private QuerySourceDefData CreateQuerySource(Query_Source querySource)
        {
            var result = new QuerySourceDefData
            {
                Id  = querySource.Id,
                Alias = querySource.Alias,
                DocumentId = querySource.Document_Id,
                QueryId = querySource.Query_Id,
                // DONE: Сделать отдельный метод преобразования!
                JoinType = ToJoinType(querySource.Join_Type),
                Permissions = _permissionRepo.GetObjectDefPermissions(querySource.Id)
            };

            AddQuerySourceChildren(result, querySource.Id);

            return result;
        }

//        private 

        private void AddQuerySourceChildren(QueryItemDefData result, Guid parentId)
        {
            if (result == null) return;

            if (result.Items == null) result.Items = new List<QueryItemDefData>();

            var en = DataContext.GetEntityDataContext().Entities;

            var children = en.Object_Defs_View
                .Where(o => o.Parent_Id == parentId && (o.Deleted == null || o.Deleted == false))
                .OrderBy(o => o.Order_Index).Select(o => o.Id).ToList();

            foreach (var childId in children)
            {
                var querySource = en.Object_Defs.OfType<Query_Source>().FirstOrDefault(c => c.Id == childId);
                if (querySource != null)
                {
                    var sub = CreateQuerySource(querySource);
                    if (sub != null) result.Items.Add(sub);
                }

                var queryCondition = en.Object_Defs.OfType<Condition>().FirstOrDefault(c => c.Id == childId);
                if (queryCondition != null)
                {
                    var sub = CreateQueryCondition(queryCondition);
                    if (sub != null) result.Items.Add(sub);
                }
            }
        }

        private QueryConditionDefData CreateQueryCondition(Condition condition)
        {
            var result = new QueryConditionDefData
            {
                Id = condition.Id,
                Expression = (ExpressionOperation)(condition.Expression ?? 0),
                Operation = ToConditionOperation(condition.Operation),
                LeftSourceName = condition.Left_Source,
                LeftAttributeId = condition.Left_Attribute_Id,
                LeftAttributeName = condition.Left_Attribute_Name,
                LeftValue = condition.Left_Value,
                LeftParamName = condition.Left_Param_Name,
                RightSourceName = condition.Right_Source,
                RightAttributeId = condition.Right_Attribute_Id,
                RightAttributeName = condition.Right_Attribute_Name,
                RightValue = condition.Right_Value,
                RightParamName = condition.Right_Param_Name,
                Permissions = _permissionRepo.GetObjectDefPermissions(condition.Id)
            };

            AddConditionChildren(result, condition);

            return result;
        }

        public QueryItemDefData CheckItemPermissions(QueryItemDefData item)
        {
            var initItem = CheckItemChildrenPermissions(item, GetPermissions());

            return initItem;
        }

        private PermissionSet _userPermissions = null;
        private PermissionSet GetPermissions()
        {
            return _userPermissions ?? (_userPermissions = _permissionRepo.GetUserPermissions(_userId));
        }

        public QueryItemDefData CheckItemChildrenPermissions(QueryItemDefData item, PermissionSet permissions)
        {
            if (item == null) return null;

            if (item.Permissions == null) return item;
            if (permissions == null)
            {
                return item.Permissions.Items.Count > 0 ? null : item;
            }

            if (!permissions.IsSupersetOf(item.Permissions)) return null;

            if (item.Items == null) return item;

            var removedItems = new List<QueryItemDefData>();

            foreach (var child in item.Items)
            {
                if (!permissions.IsSupersetOf(child.Permissions))
                {
                    removedItems.Add(child);
                }
                else
                {
                    CheckItemChildrenPermissions(child, permissions);
                }
            }

            foreach (var removing in removedItems)
                item.Items.Remove(removing);

            return item;
        }
        
        private ConditionOperation ToConditionOperation(short? op)
        {
            if (op == null) return ConditionOperation.Exp;
            return CompareOperationConverter.CompareToCondition((short) op);
        }

        private SqlSourceJoinType ToJoinType(short? op)
        {
            if (op == null) return SqlSourceJoinType.Inner;
            switch ((short) op)
            {
                case 1: return SqlSourceJoinType.Inner;
                case 2: return SqlSourceJoinType.LeftOuter;
                case 3: return SqlSourceJoinType.RightOuter;
                case 4: return SqlSourceJoinType.FullOuter;
                default: return SqlSourceJoinType.Inner;
            }
        }

        private void AddConditionChildren(QueryConditionDefData result, Condition condition)
        {
            if (result == null) return;

            if (result.Items == null) result.Items = new List<QueryItemDefData>();

            var en = DataContext.GetEntityDataContext().Entities;

            var children = en.Object_Defs.OfType<Condition>()
                .Where(o => o.Parent_Id == condition.Id && (o.Deleted == null || o.Deleted == false))
                .OrderBy(o => o.Order_Index).ToList();

            foreach (var child in children)
            {
                var sub = CreateQueryCondition(child);
                if (sub != null) result.Items.Add(sub);
            }
        }

        private void AddToCache(QueryItemDefData result)
        {
            ObjectCache<QueryItemDefData> cache;
            if (!QueryDefCache.TryGetValue(DataContext.Name, out cache))
            {
                cache = new ObjectCache<QueryItemDefData>();
                QueryDefCache.TryAdd(DataContext.Name, cache);
            }
            cache.Add(result, result.Id);
        }

        private QueryItemDefData Clone(QueryItemDefData item)
        {
            return Deserialize(Serialize(item));
        }

        private static QueryItemDefData Deserialize(string data)
        {
            using (var read = new StringReader(data))
            {
                var serializer = new XmlSerializer(typeof(QueryItemDefData));
                using (var reader = new XmlTextReader(read))
                {
                    return (QueryItemDefData)serializer.Deserialize(reader);
                }
            }
        }

        private static string Serialize(QueryItemDefData item)
        {
            var serializer = new XmlSerializer(typeof(QueryItemDefData));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, item);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
    }
}
