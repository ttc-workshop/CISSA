using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using Intersoft.CISSA.BizService.Interfaces;
using Intersoft.CISSA.BizService.Utils;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Monitor = Intersoft.CISSA.DataAccessLayer.Model.Misc.Monitor;

//using NLog;

namespace Intersoft.CISSA.BizService
{
    /// <summary>
    /// Основной класс сервиса бизнес-логики КИССП
    /// </summary>
    public partial class BizService : IUserManager, IPresentationManager, IDocManager, IWorkflowManager, IReportManager, IQueryManager, IAsyncWorkflowManager,
        IDisposable
    {
        protected static readonly List<ServiceProcessInfo> Processes = new List<ServiceProcessInfo>();
        protected static int ProcessId = 1;
        // protected static readonly object ProcessInfoLock = new object();
        private static readonly ReaderWriterLock ProcessInfoLock = new ReaderWriterLock();
        static BizService()
        {
            BaseServiceFactory.CreateBaseServiceFactories();
        }

        /// <summary>
        /// Класс подключения к сущностям БД
        /// </summary>
        /*private EntityConnection Connection { get; set; }

        private readonly bool _ownConnection = false;*/

        private readonly int _id;
        private readonly IMultiDataContext _dataContext;
        private readonly bool _ownDataContext;
        /// <summary>
        /// Контекст подключения к БД
        /// </summary>
        protected IMultiDataContext DataContext { get { return _dataContext; } }

        /// <summary>
        /// Логин текущего пользователя
        /// </summary>
        private string CurrentUserName { get; set; }
        private Guid? _userId;

        /// <summary>
        /// Идентификатор текущего пользователя
        /// </summary>
        private Guid CurrentUserId
        {
            get
            {
                if (_userId == null)
                    _userId = UserRepo.GetUserInfo(CurrentUserName).Id;
                return (Guid) _userId;
            }
        }

        public IAppServiceProvider Provider { get; private set; }

        private readonly Monitor _monitor;

//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public BizService()
        {
            try
            {
                _monitor = new Monitor("BizService session");

                /*Connection = new EntityConnection("name=cissaEntities");
                _ownConnection = true;*/
                CurrentUserName = ServiceSecurityContext.Current.PrimaryIdentity.Name;
//              Log.Info(string.Format("Система запущена пользователем {0}", CurrentUserName));

                //var dc = new DataContext(Connection);
                var dataContextFactory = DataContextFactoryProvider.GetFactory();

                _dataContext = dataContextFactory.CreateMultiDc(BaseServiceFactory.DataContextConfigSectionName); //new MultiDataContext(/*new[] {dc}*/);
                _ownDataContext = true;

                var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                Provider = providerFactory.Create(DataContext);

                UserRepo = Provider.Get<IUserRepository>(); // new UserRepository(Provider);

                var serviceRegistrator = Provider.Get<IAppServiceProviderRegistrator>();
                serviceRegistrator.AddService(new UserDataProvider(CurrentUserId, CurrentUserName));

                EnumRepo = Provider.Get<IEnumRepository>(); //new EnumRepository(Provider);
                DocRepo = Provider.Get<IDocRepository>(); //new DocRepository(DataContext, CurrentUserId);
                DocDefRepo = Provider.Get<IDocDefRepository>();
                FormRepo = Provider.Get<IFormRepository>(); //new FormRepository(DataContext, CurrentUserId);
                var workflowRepo = Provider.Get<IWorkflowRepository>(); //new WorkflowRepository(DataContext);
                WorkflowRepo = workflowRepo;
                WorkflowEngine = Provider.Get<IWorkflowEngine>();
                //new WorkflowEngine(DataContext, workflowRepo, CurrentUserId);

                ReportGeneratorProvider = Provider.Get<ITemplateReportGeneratorProvider>();
                //new TemplateReportGeneratorProvider(DataContext, CurrentUserId);
                //PdfTempRepo = new PdfTemplateRepository(DataContext, CurrentUserId);
                //ExcelTempRepo = new ExcelTemplateRepository(DataContext, CurrentUserId);
                LangRepo = Provider.Get<ILanguageRepository>();

                _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
                _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
                _comboBoxValueProvider = Provider.Get<IComboBoxEnumProvider>();

                _id = RegisterProcess(CurrentUserId, CurrentUserName);
            }
            catch (Exception e)
            {
                Logger.OutputLog(e, "BizService construction");
                throw;
            }
        }

        private static int RegisterProcess(Guid userId, string currentUserName)
        {
            // lock (ProcessInfoLock)
            ProcessInfoLock.AcquireWriterLock(LockTimeout);
            try
            {
                var id = ProcessId;
                Processes.Add(
                    new ServiceProcessInfo
                    {
                        Id = id,
                        UserId = userId,
                        UserName = currentUserName,
                        StartTime = DateTime.Now
                    });
                ProcessId++;
                return id;
            }
            finally
            {
                ProcessInfoLock.ReleaseWriterLock();
            }
        }

        private void UnregisterProcess(int id)
        {
            // lock (ProcessInfoLock)
            ProcessInfoLock.AcquireWriterLock(LockTimeout);
            try
            {
                Processes.RemoveAll(info => info.Id == id);
            }
            finally
            {
                ProcessInfoLock.ReleaseWriterLock();
            }
        }

        public BizService(string userName)
        {
            _monitor = new Monitor("BizService session");

            /*Connection = new EntityConnection("name=cissaEntities");
            _ownConnection = true;*/
            CurrentUserName = userName;

            //var dc = new DataContext(Connection);
            var dataContextFactory = DataContextFactoryProvider.GetFactory();

            _dataContext = dataContextFactory.CreateMultiDc(BaseServiceFactory.DataContextConfigSectionName); //new MultiDataContext(/*new[] { dc }*/);
            _ownDataContext = true;

            var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = providerFactory.Create(DataContext);

            UserRepo = Provider.Get<IUserRepository>(); // new UserRepository(Provider);

            var serviceRegistrator = Provider.Get<IAppServiceProviderRegistrator>();
            serviceRegistrator.AddService(new UserDataProvider(CurrentUserId, CurrentUserName));

            EnumRepo = Provider.Get<IEnumRepository>(); //new EnumRepository(Provider);
            DocRepo = Provider.Get<IDocRepository>(); //new DocRepository(DataContext, CurrentUserId);
            DocDefRepo = Provider.Get<IDocDefRepository>();
            FormRepo = Provider.Get<IFormRepository>(); //new FormRepository(DataContext, CurrentUserId);
            var workflowRepo = Provider.Get<IWorkflowRepository>(); //new WorkflowRepository(DataContext);
            WorkflowRepo = workflowRepo;
            WorkflowEngine = Provider.Get<IWorkflowEngine>(); //new WorkflowEngine(DataContext, workflowRepo, CurrentUserId);

            ReportGeneratorProvider = Provider.Get<ITemplateReportGeneratorProvider>();
            //new TemplateReportGeneratorProvider(DataContext, CurrentUserId);
            //PdfTempRepo = new PdfTemplateRepository(DataContext, CurrentUserId);
            //ExcelTempRepo = new ExcelTemplateRepository(DataContext, CurrentUserId);
            LangRepo = Provider.Get<ILanguageRepository>();

            _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
            _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
            _comboBoxValueProvider = Provider.Get<IComboBoxEnumProvider>();

            _id = RegisterProcess(CurrentUserId, CurrentUserName);
        }

        public BizService(IAppServiceProvider provider, string currentUserName)
        {
            _monitor = new Monitor("BizService session");

            _dataContext = provider.Get<IMultiDataContext>();
            CurrentUserName = currentUserName;

            Provider = provider;
            UserRepo = Provider.Get<IUserRepository>();

            var serviceRegistrator = Provider.Get<IAppServiceProviderRegistrator>();
            serviceRegistrator.AddService(new UserDataProvider(CurrentUserId, CurrentUserName));

            EnumRepo = Provider.Get<IEnumRepository>();
            DocRepo = Provider.Get<IDocRepository>();
            DocDefRepo = Provider.Get<IDocDefRepository>();
            FormRepo = Provider.Get<IFormRepository>();
            var workflowRepo = Provider.Get<IWorkflowRepository>();
            WorkflowRepo = workflowRepo;
            WorkflowEngine = Provider.Get<IWorkflowEngine>();

            ReportGeneratorProvider = Provider.Get<ITemplateReportGeneratorProvider>();
            LangRepo = Provider.Get<ILanguageRepository>();

            _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
            _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
            _comboBoxValueProvider = Provider.Get<IComboBoxEnumProvider>();

            _id = RegisterProcess(CurrentUserId, CurrentUserName);
        }

        public BizService SetUserRepository(IUserRepository userRepository)
        {
            UserRepo = userRepository;
            return this;
        }

        protected virtual void Dispose(bool managed)
        {
            if (managed)
            {
                try
                {
                    if (_ownDataContext && _dataContext != null) _dataContext.Dispose();
                    //if (_ownConnection && Connection != null) Connection.Dispose();
                    if (_monitor != null) _monitor.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "BizService.Dispose");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            UnregisterProcess(_id);
        }
    }
}
