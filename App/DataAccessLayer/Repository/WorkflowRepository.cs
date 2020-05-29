using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class WorkflowRepository : IWorkflowRepository, IDisposable
    {
        public IDataContext DataContext { get; private set; }
        private readonly bool _ownDataContext;

        public WorkflowRepository(IDataContext dataContext)
        {
            if (dataContext == null)
            {
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else
                DataContext = dataContext;

            _langRepo = new LanguageRepository(DataContext);
        }
        public WorkflowRepository() : this (null) {}

        public WorkflowRepository(IAppServiceProvider provider, IDataContext dataContext)
        {
            DataContext = dataContext; //provider.Get<IDataContext>();
            _langRepo = provider.Get<ILanguageRepository>();
        }

        private readonly ILanguageRepository _langRepo;

        private static readonly ObjectCache<WorkflowActivity> ActivityCache = new ObjectCache<WorkflowActivity>();
        private static readonly ObjectCache<WorkflowProcess> ProcessCache = new ObjectCache<WorkflowProcess>();
        private static readonly ReaderWriterLock ActivityCacheLock = new ReaderWriterLock();
        private static readonly ReaderWriterLock ProcessCacheLock = new ReaderWriterLock();
        private static readonly IDictionary<Guid, Guid> ProcessStartActivities = new SortedDictionary<Guid, Guid>();
        // public static readonly object WorkflowProcessStartActivityLock = new Object();
        private static readonly ReaderWriterLock ProcessStartActivityLock = new ReaderWriterLock();

        public static object ActivityCacheCount
        {
            get
            {
                ActivityCacheLock.AcquireReaderLock(LockTimeout);
                try
                {
                    return ActivityCache.Count;
                }
                finally
                {
                    ActivityCacheLock.ReleaseReaderLock();
                }
            }
        }

        public static object ProcessCacheCount
        {
            get
            {
                ProcessCacheLock.AcquireReaderLock(LockTimeout);
                try
                {
                    return ProcessCache.Count;
                }
                finally
                {
                    ProcessCacheLock.ReleaseReaderLock();
                }
            }
        }

        public static void ClearCaches()
        {
            ActivityCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                ActivityCache.Clear();
            }
            finally
            {
                ActivityCacheLock.ReleaseWriterLock();
            }
            ProcessCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                ProcessCache.Clear();
            }
            finally
            {
                ProcessCacheLock.ReleaseWriterLock();
            }
            ProcessStartActivityLock.AcquireWriterLock(LockTimeout);
            try
            {
                ProcessStartActivities.Clear();
            }
            finally
            {
                ProcessStartActivityLock.ReleaseWriterLock();
            }
            GateCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                GateCache.Clear();
            }
            finally
            {
                GateCacheLock.ReleaseWriterLock();
            }
            GateRefCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                GateRefCache.Clear();
            }
            finally
            {
                GateRefCacheLock.ReleaseWriterLock();
            }
            UserActionCacheLock.AcquireWriterLock(LockTimeout);
            try
            {
                UserActionCache.Clear();
            }
            finally
            {
                UserActionCacheLock.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Загружает процесс по идентификатору
        /// </summary>
        /// <param name="processId">Идентификатор процесса</param>
        /// <returns>Загруженный процесс</returns>
        //[SmartCache(TimeOutSeconds = 600)]
        public WorkflowProcess LoadProcessById(Guid processId)
        {
            ProcessCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cached = ProcessCache.Find(processId);
                if (cached != null)
                    return cached.CachedObject;

                var lc = ProcessCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    cached = ProcessCache.Find(processId);
                    if (cached != null)
                        return cached.CachedObject;

                    var process =
                        DataContext.GetEntityDataContext()
                            .Entities.Object_Defs.OfType<Workflow_Process>()
                            .FirstOrDefault(p => p.Id == processId);

                    var startActivityId = GetProcessStartActivity(processId);

                    if (process != null)
                    {
                        var returnProcess = new WorkflowProcess(process, startActivityId);

                        ProcessCache.Add(returnProcess, processId);

                        return returnProcess;
                    }
                }
                finally
                {
                    ProcessCacheLock.DowngradeFromWriterLock(ref lc);
                }
                return null;
            }
            finally
            {
                ProcessCacheLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Позволяет загрузить действие по идентификатору
        /// </summary>
        /// <param name="activityId">Идентификатор действия</param>
        /// <returns>Загруженное действие</returns>
        public WorkflowActivity LoadActivityById(Guid activityId)
        {
            ActivityCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cached = ActivityCache.Find(activityId);
                if (cached != null)
                    return cached.CachedObject;

                var lc = ActivityCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    cached = ActivityCache.Find(activityId);
                    if (cached != null)
                        return cached.CachedObject;

                    var activity = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Workflow_Activity>()
                        .FirstOrDefault(p => p.Id == activityId);

                    WorkflowActivity returnActivity = null;

                    if (activity is Document_Activity)
                        returnActivity = new DocumentActivity(activity as Document_Activity);

                    else if (activity is Document_State_Activity)
                        returnActivity = new DocumentStateActivity(activity as Document_State_Activity);

                    else if (activity is Presentation_Activity)
                        returnActivity = new PresentationActivity(activity as Presentation_Activity);

                    else if (activity is Script_Activity)
                        returnActivity = new ScriptActivity(activity as Script_Activity);

                    else if (activity is Finish_Activity)
                        returnActivity = new FinishActivity(activity as Finish_Activity);

                    else if (activity is Process_Call_Activity)
                        returnActivity = new ProcessCallActivity(activity as Process_Call_Activity);

                    else if (activity is Start_Activity)
                        returnActivity = new StartActivity(activity as Start_Activity);

                    else if (activity is Gate_Call_Activity)
                        returnActivity = new GateCallActivity(activity as Gate_Call_Activity);

                    if (returnActivity != null)
                        ActivityCache.Add(returnActivity, activityId);

                    return returnActivity;
                }
                finally
                {
                    ActivityCacheLock.DowngradeFromWriterLock(ref lc);
                }
            }
            finally
            {
                ActivityCacheLock.ReleaseReaderLock();
            }
        }

        public WorkflowActivity GetActivityById(Guid activityId)
        {
            var activity = LoadActivityById(activityId);

            if (activity == null)
                throw new ApplicationException(String.Format("Workflow Activity with Id '{0}' not found!", activityId));

            return activity;
        }

        /// <summary>
        /// Загружает действия процесса
        /// </summary>
        /// <param name="processId">Идентификатор процесса</param>
        /// <returns>Список идентификаторов действий процесса</returns>
        [Obsolete("Не используется")]
        public IList<Guid> LoadProcessActivities(Guid processId)
        {
            return new List<Guid>(
                DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Workflow_Activity>()
                    .Where(a => a.Parent_Id == processId)
                    .Select(a => a.Id)
                );
        }

        /// <summary>
        /// Загружает стартовое действие процесса
        /// </summary>
        /// <remarks>
        /// Лучше стартовое действие строго не прописывать в процессе. А брать его из таблицы Start_Activities
        /// </remarks>
        /// <param name="processId">Идентификатор процесса</param>
        /// <returns>Идентификатор стартового действия</returns>
        public Guid GetProcessStartActivity(Guid processId)
        {
            // lock (WorkflowProcessStartActivityLock)
            ProcessStartActivityLock.AcquireReaderLock(LockTimeout);
            try
            {
                Guid startActivityId;
                if (ProcessStartActivities.TryGetValue(processId, out startActivityId))
                    return startActivityId;

                var lc = ProcessStartActivityLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    if (ProcessStartActivities.TryGetValue(processId, out startActivityId))
                        return startActivityId;

                    var query = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Start_Activity>()
                        .Where(a => a.Parent_Id == processId && (a.Deleted == null || a.Deleted == false))
                        .Select(a => a.Id);

                    startActivityId = query.FirstOrDefault();

                    ProcessStartActivities.Add(processId, startActivityId);
                }
                finally
                {
                    ProcessStartActivityLock.DowngradeFromWriterLock(ref lc);
                }

                return startActivityId;
            }
            finally
            {
                ProcessStartActivityLock.ReleaseReaderLock();
            }
        }

        private const int LockTimeout = 500000;

        /// <summary>
        /// Загружает последующее действие процесса через выбранное действие пользователя
        /// </summary>
        /// <param name="processId">Идентификатор процесса</param>
        /// <param name="lastActivity">Последнее (передыдущее) действие</param>
        /// <param name="userActionId">Действие пользователя</param>
        [Obsolete("Не используется!")]
        public WorkflowActivity GetActivityByUserActionId(Guid processId, Guid lastActivity, Guid userActionId)
        {
            /*var queryActivityLink = from lnk in DataContext.ObjectDefs.OfType<Activity_Link>()
                                    where //lnk.Parent_Id == processId &&
                                        lnk.User_Action_Id == userActionId &&
                                        lnk.Source_Id == lastActivity
                                    select lnk.Target_Id;*/
            Guid? targetActivityId = null;
            var activity = GetActivityById(lastActivity);

            if (activity.TargetLinks != null)
            {
                var queryActivityLink =
                    activity.TargetLinks.Where(l => l.UserActionId != null && l.UserActionId == userActionId)
                        .Select(l => l.TargetId)
                        .ToList();

                if (!queryActivityLink.Any())
                {
                    throw new ApplicationException("Не удалось найти действие для продолжения");
                }

                targetActivityId = queryActivityLink.First();
            }

            if (!targetActivityId.HasValue)
            {
                throw new ApplicationException("В Activity_Link не указано следующее действие (TargetId)");
            }

            return LoadActivityById(targetActivityId.Value);
        }

        /// <summary>
        /// Загружает список возможных действий пользователя
        /// </summary>
        /// <param name="activityId">Идентификатор действия</param>
        public IList<UserAction> GetActivityUserActions(Guid activityId)
        {
            /*var targetLinks = from l in DataContext.ObjectDefs.OfType<Activity_Link>().Include("User_Actions")
                              where l.Source_Id == activityId &&
                                    l.User_Action_Id != null &&
                                    (l.Deleted == null || l.Deleted == false)
                              orderby l.Order_Index
                              select l;*/

            var returnList = new List<UserAction>();

            var activity = LoadActivityById(activityId);
            if (activity != null && activity.TargetLinks != null)
            {
                var targetLinks = activity.TargetLinks.Where(l => l.UserActionId != null);

                returnList.AddRange(targetLinks.Select(
                    l => GetUserAction(l.UserActionId ?? Guid.Empty)
                        /*new UserAction
                    {
                        Id = l.UserActionId,
                        Name = l.UserAction.Name
                    }*/));
            }
            return returnList;
        }

        /// <summary>
        /// Загружает список возможных действий пользователя
        /// </summary>
        /// <param name="activityId">Идентификатор действия</param>
        /// <param name="languageId">Язык</param>
        public IList<UserAction> GetActivityUserActions(Guid activityId, int languageId)
        {
            var returnList = new List<UserAction>();

            var activity = LoadActivityById(activityId);
            if (activity != null && activity.TargetLinks != null)
            {
                var targetLinks = activity.TargetLinks.Where(l => l.UserActionId != null);

                returnList.AddRange(targetLinks.Select(
                    l => GetUserAction(l.UserActionId ?? Guid.Empty, languageId)));
            }
            return returnList;
        }

        public static ObjectCache<UserAction> UserActionCache = new ObjectCache<UserAction>();
        private static readonly ReaderWriterLock UserActionCacheLock = new ReaderWriterLock();

        public UserAction GetUserAction(Guid id)
        {
            UserActionCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cacheItem = UserActionCache.Find(id);
                if (cacheItem != null)
                    return new UserAction
                    {
                        Id = id,
                        Name = cacheItem.CachedObject.Name,
                        DefaultName = cacheItem.CachedObject.DefaultName
                    };

                var lc = UserActionCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    cacheItem = UserActionCache.Find(id);
                    if (cacheItem != null)
                        return new UserAction
                        {
                            Id = id,
                            Name = cacheItem.CachedObject.Name,
                            DefaultName = cacheItem.CachedObject.DefaultName
                        };

                    var action =
                        DataContext.GetEntityDataContext()
                            .Entities.Object_Defs.OfType<User_Action>()
                            .FirstOrDefault(ua => ua.Id == id);
                    if (action != null)
                    {
                        var userAction = new UserAction
                        {
                            Id = id,
                            Name = action.Full_Name,
                            DefaultName = action.Full_Name
                        };
                        UserActionCache.Add(userAction, id);

                        return new UserAction {Id = id, Name = userAction.Name, DefaultName = userAction.DefaultName};
                    }
                }
                finally
                {
                    UserActionCacheLock.DowngradeFromWriterLock(ref lc);
                }
                return null;
            }
            finally
            {
                UserActionCacheLock.ReleaseReaderLock();
            }
        }

        public UserAction GetUserAction(Guid id, int languageId)
        {
            var userAction = GetUserAction(id);

            if (languageId != 0)
                TranslateUserAction(userAction, languageId);

            return userAction;
        }

        /// <summary>
        /// Переводит список пользовательских действий на заданный язык
        /// </summary>
        /// <param name="userActions">Список пользовательских действий</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Список пользовательских действий</returns>
        public void TranslateUserActions(List<UserAction> userActions, int languageId)
        {
            foreach (var userAction in userActions)
            {
                TranslateUserAction(userAction, languageId);
            }
        }

        public static ObjectKeyCache<string, WorkflowGate> GateCache = new ObjectKeyCache<string, WorkflowGate>();
        private static ReaderWriterLock GateCacheLock = new ReaderWriterLock();

        public WorkflowGate LoadGateByName(string gateName)
        {
            GateCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cacheItem = GateCache.Find(gateName);
                if (cacheItem != null)
                    return new WorkflowGate
                    {
                        Id = cacheItem.CachedObject.Id,
                        Name = gateName,
                        Description = cacheItem.CachedObject.Description,
                        ProcessId = cacheItem.CachedObject.ProcessId
                    };

                var lc = GateCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    cacheItem = GateCache.Find(gateName);
                    if (cacheItem != null)
                        return new WorkflowGate
                        {
                            Id = cacheItem.CachedObject.Id,
                            Name = gateName,
                            Description = cacheItem.CachedObject.Description,
                            ProcessId = cacheItem.CachedObject.ProcessId
                        };

                    var gate =
                        DataContext.GetEntityDataContext()
                            .Entities.Object_Defs.OfType<Workflow_Gate>()
                            .FirstOrDefault(ua => ua.Name.Equals(gateName));
                    if (gate != null)
                    {
                        var workflowGate = new WorkflowGate
                        {
                            Id = gate.Id,
                            Name = gate.Name,
                            Description = gate.Full_Name,
                            ProcessId = gate.Process_Id ?? Guid.Empty
                        };
                        GateCache.Add(workflowGate, gateName);

                        return new WorkflowGate
                        {
                            Id = gate.Id,
                            Name = gate.Name,
                            Description = gate.Full_Name,
                            ProcessId = gate.Process_Id ?? Guid.Empty
                        };
                    }
                }
                finally
                {
                    GateCacheLock.DowngradeFromWriterLock(ref lc);
                }
                return null;
            }
            finally
            {
                GateCacheLock.ReleaseReaderLock();
            }
        }

        public static ObjectCache<WorkflowGateRef> GateRefCache = new ObjectCache<WorkflowGateRef>();
        private static readonly ReaderWriterLock GateRefCacheLock = new ReaderWriterLock();

        public WorkflowGateRef LoadGateRefById(Guid gateRefId)
        {
            GateRefCacheLock.AcquireReaderLock(LockTimeout);
            try
            {
                var cacheItem = GateRefCache.Find(gateRefId);
                if (cacheItem != null)
                    return new WorkflowGateRef
                    {
                        Id = gateRefId,
                        ServiceUrl = cacheItem.CachedObject.ServiceUrl,
                        UserName = cacheItem.CachedObject.UserName,
                        Password = cacheItem.CachedObject.Password,
                        CertificateData = new CertificateData
                        {
                            StoreLocation = cacheItem.CachedObject.CertificateData.StoreLocation,
                            StoreName = cacheItem.CachedObject.CertificateData.StoreName,
                            FindType = cacheItem.CachedObject.CertificateData.FindType,
                            FindValue = cacheItem.CachedObject.CertificateData.FindValue
                        }
                    };

                var lc = GateRefCacheLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    cacheItem = GateRefCache.Find(gateRefId);
                    if (cacheItem != null)
                        return new WorkflowGateRef
                        {
                            Id = gateRefId,
                            ServiceUrl = cacheItem.CachedObject.ServiceUrl,
                            UserName = cacheItem.CachedObject.UserName,
                            Password = cacheItem.CachedObject.Password,
                            CertificateData = new CertificateData
                            {
                                StoreLocation = cacheItem.CachedObject.CertificateData.StoreLocation,
                                StoreName = cacheItem.CachedObject.CertificateData.StoreName,
                                FindType = cacheItem.CachedObject.CertificateData.FindType,
                                FindValue = cacheItem.CachedObject.CertificateData.FindValue
                            }
                        };

                    var gateRef =
                        DataContext.GetEntityDataContext()
                            .Entities.Object_Defs.OfType<Workflow_Gate_Ref>()
                            .FirstOrDefault(ua => ua.Id == gateRefId);
                    if (gateRef != null)
                    {
                        var workflowGateRef = new WorkflowGateRef
                        {
                            Id = gateRef.Id,
                            ServiceUrl = gateRef.Service_Url,
                            UserName = gateRef.User_Name,
                            Password = gateRef.User_Password,
                            CertificateData = new CertificateData
                            {
                                StoreLocation = CertificateDataHelper.GetStoreLocation(gateRef.Store_Location),
                                StoreName = CertificateDataHelper.GetStoreName(gateRef.Store_Name),
                                FindType = CertificateDataHelper.GetFindType(gateRef.Find_Type),
                                FindValue = gateRef.Subject_Name
                            }
                        };
                        GateRefCache.Add(workflowGateRef, gateRefId);

                        return new WorkflowGateRef
                        {
                            Id = workflowGateRef.Id,
                            ServiceUrl = workflowGateRef.ServiceUrl,
                            UserName = workflowGateRef.UserName,
                            Password = workflowGateRef.Password,
                            CertificateData = new CertificateData
                            {
                                StoreLocation = workflowGateRef.CertificateData.StoreLocation,
                                StoreName = workflowGateRef.CertificateData.StoreName,
                                FindType = workflowGateRef.CertificateData.FindType,
                                FindValue = workflowGateRef.CertificateData.FindValue
                            }
                        };
                    }
                }
                finally
                {
                    GateRefCacheLock.DowngradeFromWriterLock(ref lc);
                }
                return null;
            }
            finally
            {
                GateRefCacheLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Переводит список пользовательских действий на заданный язык
        /// </summary>
        /// <param name="userAction">Ппользовательское действие</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Список пользовательских действий</returns>
        public void TranslateUserAction(UserAction userAction, int languageId)
        {
            if (userAction == null) return;

            if (languageId != 0)
            {
                userAction.Name = _langRepo.GetTranslation(userAction.Id, languageId);
                if (String.IsNullOrEmpty(userAction.Name))
                    userAction.Name = userAction.DefaultName;
            }
            else
                userAction.Name = userAction.DefaultName;
        }

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
                    Logger.OutputLog(e, "WorkflowRepository.Dispose");
                    throw;
                }
            }
        }

        ~WorkflowRepository()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "WorkflowRepository.Finalize");
                    throw;
                }
        }
    }
}
