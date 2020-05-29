using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.BizService.Utils;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.BizService
{
    public partial class BizService
    {
        private IUserRepository UserRepo { get; set; }

        public BizService(IUserRepository userRepo, string currentUserName)
        {
            UserRepo = userRepo;
            CurrentUserName = currentUserName;
        }

        public BizService(IMultiDataContext dataContext, string currentUserName)
        {
            CurrentUserName = currentUserName;

            _dataContext = dataContext;

            var factory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = factory.Create(DataContext); //new AppServiceProvider(DataContext);

            UserRepo = Provider.Get<IUserRepository>();

            var serviceRegistrator = Provider.Get<IAppServiceProviderRegistrator>();
            serviceRegistrator.AddService(new UserDataProvider(CurrentUserId, CurrentUserName));

            EnumRepo = Provider.Get<IEnumRepository>();
            DocRepo = Provider.Get<IDocRepository>(); 
            FormRepo = Provider.Get<IFormRepository>();
            var workflowRepo = Provider.Get<IWorkflowRepository>();
            WorkflowRepo = workflowRepo;
            WorkflowEngine = Provider.Get<IWorkflowEngine>();

            ReportGeneratorProvider = Provider.Get<ITemplateReportGeneratorProvider>();
            LangRepo = Provider.Get<ILanguageRepository>();

            _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
            _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
        }

        /// <summary>
        /// Получает данные учетной записи пользователя
        /// </summary>
        /// <returns>Сведения о пользователе</returns>
        public UserInfo GetUserInfo()
        {
            return UserRepo.GetUserInfo(CurrentUserName);
        }

        /// <summary>
        /// Метод для изменения пароля пользователя
        /// </summary>
        /// <param name="oldPassword">Старый пароль</param>
        /// <param name="newPassword">Новый пароль</param>
        /// <returns>Возвращает результат изменения пароля пользователя</returns>
        public BizResult ChangeUserPassword(string oldPassword, string newPassword)
        {
            return UserRepo.ChangeUserPassword(CurrentUserName, oldPassword, newPassword);
        }

        /// <summary>
        /// Метод для проверки соединения с сервисом бизнес-логики
        /// </summary>
        /// <returns>Возвращает истина - при успешном подключении к сервису</returns>
        public bool TryConnect()
        {
            return true;
        }

        /// <summary>
        /// Метод установки языка пользовательского интерфейса для текущего пользователя
        /// </summary>
        /// <param name="languageId">Код устанавливаемого языка</param>
        public void SetUserLanguage(int languageId)
        {
            UserRepo.SetUserLanguage(CurrentUserId, languageId);
        }

        /// <summary>
        /// Метод для очищения кэша
        /// </summary>
        public void ClearCache()
        {
            DocRepository.DocCache.Clear();
            // DocDefRepository.DocDefCache.Clear();
            DocDefRepository.DocDefDescendantCache.Clear();
            /*lock(DocDefRepository.DocDefNameCacheLock)
                DocDefRepository.DocDefNameCache.Clear();*/
            // DocDefRepository.ClearDocDefNameCache();
            // lock(DocDefRepository.TypeDefCacheLock)
                //DocDefRepository.TypeDefCache.Clear();
            // DocDefRepository.ClearTypeDefCache();
            DocDefRepository.ClearCaches();
            UserRepository.UserInfoCache.Clear();
            UserRepository.UserOrgCache.Clear();
            OrgRepository.OrgInfoCache.Clear();
            OrgRepository.OrgTypeInfoCache.Clear();
            OrgRepository.OrgPositionInfoCache.Clear();
            OrgRepository.OrganizationListCache.Clear();
            /*FormRepository.DetailFormCache.Clear();
            FormRepository.TableFormCache.Clear();*/
            FormRepository.ClearCaches();
            // WorkflowRepository.ActivityCache.Clear();
            // WorkflowRepository.ProcessCache.Clear();
            // lock (WorkflowRepository.WorkflowProcessStartActivityLock)
            //    WorkflowRepository.ProcessStartActivities.Clear();
            // WorkflowRepository.GateCache.Clear();
            // WorkflowRepository.GateRefCache.Clear();
            WorkflowRepository.ClearCaches();
            PermissionRepository.ObjectDefPermissionCache.Clear();
            PermissionRepository.UserPermissionCache.Clear();
            PermissionRepository.OrgPositionPermissionCache.Clear();
            PermissionRepository.OrgUnitPermissionCache.Clear();
            PermissionRepository.RoleListCache.Clear();
            EnumRepository.EnumDefCache.Clear();
            LanguageRepository.ClearCache();
            DocStateRepository.DocStateTypeCache.Clear();
            // lock (ScriptManager.ScriptLoadLock)
            ScriptManager.ClearCaches();
            DocumentTableMapRepository.ClearMaps();
            // lock (QueryRepository.QueryDefCacheLock)
            QueryRepository.QueryDefCache.Clear();
        }

        /// <summary>
        /// Получает сведения о состоянии кэша
        /// </summary>
        /// <returns>Строка описания состояния кэша</returns>
        public string GetCacheInfo()
        {
            var taskCount = 0;
            var sessionProcessCount = 0;
            ProcessTaskLock.AcquireReaderLock(LockTimeout);
            try
            {
                taskCount = ProcessTasks.Count;
            }
            finally
            {
                ProcessTaskLock.ReleaseReaderLock();
            }
            ProcessInfoLock.AcquireReaderLock(LockTimeout);
            try
            {
                sessionProcessCount = Processes.Count;
            }
            finally
            {
                ProcessInfoLock.ReleaseReaderLock();
            }

            return String.Format(
                @"Кол-во классов документов: {0},
Кол-во детальных форм: {1},
Кол-во табличных форм: {2},
Кол-во действий: {3},
Кол-во процесов: {4},
Кол-во прав объектов: {5},
Кол-во прав пользователей: {6},
Кол-во списков: {7},
Кол-во пользователей: {8},
Кол-во сведений о пользователях: {9},
Кол-во организаций: {10},
Кол-во документов: {11},
Кол-во сборок: {12},
Кол-во исполняемых задач: {13},
Кол-во исполняемых сессий: {14}",
                new object[]
                    {
                        DocDefRepository.DocDefCache.Count,
                        FormRepository.DetailFormCache.Count,
                        FormRepository.TableFormCache.Count,
                        WorkflowRepository.ActivityCacheCount,
                        WorkflowRepository.ProcessCacheCount,
                        PermissionRepository.ObjectDefPermissionCache.Count,
                        PermissionRepository.UserPermissionCache.Count,
                        EnumRepository.EnumDefCache.Count,
                        UserRepository.UserAccountCount,
                        UserRepository.UserInfoCache.Count,
                        OrgRepository.OrgInfoCache.Count,
                        DocRepository.DocCache.Count,
                        ScriptManager.ScriptCache.Count,
                        taskCount,
                        sessionProcessCount
                    }
                );
        }

        public List<MonitorNode> GetMonitorNodes()
        {
            return Monitor.GetItems();
        }

        public int? GetLanguageId(string cultureName)
        {
            var languages = LangRepo.Load();

            if (languages == null || languages.Count == 0) return null;
            var langInfo =
                languages.FirstOrDefault(
                    l => String.Equals(l.CultureName, cultureName, StringComparison.OrdinalIgnoreCase));

            return langInfo == null ? (int?) null : langInfo.Id;
        }

        public string GetLanguageCultureName(int languageId)
        {
            var languages = LangRepo.Load();

            if (languages == null || languages.Count == 0) return null;
            var langInfo =
                languages.FirstOrDefault(l => l.Id == languageId);

            return langInfo == null ? String.Empty : langInfo.CultureName;
        }

        public List<ProcessInfo> GetProcessesInfo()
        {
            var list = new List<ProcessInfo>();

            // lock (ProcessInfoLock)
            ProcessInfoLock.AcquireReaderLock(LockTimeout);
            try
            {
                list.AddRange(Processes.Select(info =>
                    new ProcessInfo
                    {
                        Id = info.Id,
                        UserId = info.UserId,
                        UserName = info.UserName,
                        StartTime = info.StartTime,
                        Duration = DateTime.Now.Subtract(info.StartTime)
                    }));
            }
            finally
            {
                ProcessInfoLock.ReleaseReaderLock();
            }

            list.ForEach(info =>
            {
                var user = UserRepo.FindUserInfo(info.UserId);
                info.OrgName = user != null ? user.OrganizationName : "?";
            });

            return list;
        }
    }
}