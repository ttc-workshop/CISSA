using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Interfaces;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Providers;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Storage;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Utils;


namespace Intersoft.CISSA.ScriptActivityCompileChecker
{
    class Program
    {
        public const string DataContextConfigSectionName = "DataContexts";

        static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.AppSettings.Get("connectionString");
            var currentUserName = ConfigurationManager.AppSettings.Get("userName");
            var currentUserPassword = ConfigurationManager.AppSettings.Get("userPassword");
            var fileName = ConfigurationManager.AppSettings.Get("fileName");
            var columnNo = ConfigurationManager.AppSettings.Get("columnNo");

            var time = DateTime.Now;
            //var doc = new Doc {Id = Guid.NewGuid()};
            dynamic t = new object(); //new DynaDoc(doc, Guid.NewGuid());
            Console.WriteLine(time.ToShortTimeString());
            using (var connection = new SqlConnection(connectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    CreateBaseServiceFactories();
                    var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                    using (var provider = providerFactory.Create(dataContext))
                    {
                        var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();

                        var userProvider = new UserDataProvider(currentUserName, currentUserPassword, provider);
                        serviceRegistrator.AddService(userProvider);
                        

                        dynamic doc = DynaDoc.CreateNew(new Guid("{EB9E1B38-6FF4-4145-8D96-05BC54CD67E2}"), userProvider.UserId, provider);

                        CheckScriptActivities(provider, fileName, int.Parse(columnNo));
                    }
                }
            }
            /*var dataContextFactory = DataContextFactoryProvider.GetFactory();

            using (var dataContext = dataContextFactory.CreateMultiDc(DataContextConfigSectionName))
            {
            }*/
            var finishTime = DateTime.Now;
            Console.WriteLine(@"Finish at " + finishTime.ToShortTimeString());
            var dTime = finishTime - time;
            Console.WriteLine(dTime.TotalSeconds + @" s");
//            Console.ReadKey();
        }

        private static void CheckScriptActivities(IAppServiceProvider provider, string fileName, int columnNo)
        {
            var dataContext = provider.Get<IDataContext>();
            var edc = dataContext.GetEntityDataContext();
            Logger.DatabaseName = dataContext.StoreConnection.Database;
            var now = DateTime.Now;
            var logPath = ConfigurationManager.AppSettings.Get("logPath");
            if (String.IsNullOrEmpty(logPath)) logPath = @"c:\distr\cissa\";
            else if (!logPath.EndsWith(@"\")) logPath += @"\";
            var logFileName = String.Format(logPath + @"ScriptChecker-{0}{1}{2}-{3}{4}{5}.log", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            using (var reader = new CsvReader(fileName))
            {
                reader.Delimiters[0] = String.Empty + (char) 9;
                int i = 1;
                //var contextData = new WorkflowContextData();
                //var context = new WorkflowContext(contextData, provider);
                var lastMsg = false;
                while (reader.Read())
                {
                    var s = reader.Fields[columnNo];
                    Guid id;

                    if (!String.IsNullOrEmpty(s) && Guid.TryParse(s, out id))
                    {
                        CheckScriptActivity(provider, edc, id, logFileName, ref i, ref lastMsg);
                    }
                }
            }
        }

        /*private static void CheckAllScriptActivities(IAppServiceProvider provider)
        {
            var dataContext = provider.Get<IDataContext>();
            var edc = dataContext.GetEntityDataContext();
            Logger.DatabaseName = dataContext.StoreConnection.Database;
            var now = DateTime.Now;
            var logPath = ConfigurationManager.AppSettings.Get("logPath");
            if (String.IsNullOrEmpty(logPath)) logPath = @"c:\distr\cissa\";
            else if (!logPath.EndsWith(@"\")) logPath += @"\";
            var logFileName = String.Format(logPath + @"ScriptChecker-{0}{1}{2}-{3}{4}{5}.log", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            using (var reader = new CsvReader(fileName))
            {
                reader.Delimiters[0] = String.Empty + (char)9;
                int i = 1;
                //var contextData = new WorkflowContextData();
                //var context = new WorkflowContext(contextData, provider);
                var lastMsg = false;
                while (reader.Read())
                {
                    var s = reader.Fields[columnNo];
                    Guid id;

                    if (!String.IsNullOrEmpty(s) && Guid.TryParse(s, out id))
                    {
                        CheckScriptActivity(provider, edc, id, logFileName, ref i, ref lastMsg);
                    }
                }
            }
        }*/

        private const string SelectWorkflowProcessSql = "SELECT wp.Id FROM Workflow_Processes wp " +
            "JOIN Object_Defs od ON od.Id = wp.Id " +
            "WHERE wp.Deleted IS NULL OR od.Deleted = 0";
        private static IEnumerable<Guid> GetWorkflowProcesses(IAppServiceProvider provider)
        {
            var dc = provider.Get<IDataContext>();
            using (var command = dc.CreateCommand(SelectWorkflowProcessSql))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return reader.GetGuid(0);
                    }
                }
            }
        }

        private const string SelectWorkflowStartActivities = "SELECT sa.Id FROM Start_Activities sa " + 
            "JOIN Object_Defs od ON od.Id = sa.Id " +
            "WHERE od.Parent_Id = @procId AND (od.Deleted IS NULL OR od.Deleted = 0)";
        private static IEnumerable<Guid> GetWorkflowProcessStartActivities(IDataContext dc, Guid processId)
        {
            using (var command = dc.CreateCommand(SelectWorkflowStartActivities))
            {
                var param = command.CreateParameter();
                param.ParameterName = "@procId";
                param.Value = processId;
                command.Parameters.Add(param);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return reader.GetGuid(0);
                    }
                }
            }
        }

        private const string SelectWorkflowActivities = 
            "SELECT al.Id, al.Target_Id, al.Condition " +
            "FROM Activity_Links al JOIN Object_Defs od ON od.Id = al.Id " +
            "WHERE al.Source_Id = @sourceId AND al.Target_Id IS NOT NULL AND (od.Deleted IS NULL OR od.Deleted = 0)";
        private static IEnumerable<ScriptInfo> GetWorkflowProcessActivities(IDataContext dc, Guid sourceId)
        {
            using (var command = dc.CreateCommand(SelectWorkflowActivities))
            {
                var param = command.CreateParameter();
                param.ParameterName = "@sourceId";
                param.Value = sourceId;
                command.Parameters.Add(param);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new ScriptInfo {Id = reader.GetGuid(0), Script = reader.GetString(1)};
                    }
                }
            }
        }

        private const string SelectScriptActivitySql = "SELECT sa.Id, sa.Script " +
            "FROM Script_Activities sa WHERE sa.Id = @id";
        /*private static IEnumerable<ScriptInfo> GetWorkflowScriptActivities(IDataContext dc, Guid processId)
        {
            var list = new List<ScriptInfo>();

            foreach (var startId in GetWorkflowProcessStartActivities(dc, processId))
            {
                var sid = startId;
                do
                {
                    var linkInfo = GetWorkflowProcessActivities(dc, sid);
                    if (list.Any(i => i.Id == linkInfo.Id))
                    if (!string.IsNullOrWhiteSpace(linkInfo.Script))
                    {
                        yield return linkInfo;
                    }
                } while ();
            }
            using (var command = dc.CreateCommand(SelectWorkflowActivities))
            {
                var param = command.CreateParameter();
                param.ParameterName = "@sourceId";
                param.Value = sourceId;
                command.Parameters.Add(param);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new ScriptInfo { Id = reader.GetGuid(0), Script = reader.GetString(1) };
                    }
                }
            }
        }*/

        private static void CheckScriptActivity(IAppServiceProvider provider, IEntityDataContext edc, Guid id, string logFileName, ref int i, ref bool lastMsg)
        {
                var scriptActivity =
                    edc.Entities.Object_Defs.OfType<Script_Activity>().FirstOrDefault(a => a.Id == id);

                if (scriptActivity != null)
                {
                    var pathName = GetActivityPath(provider, id);

                    var scriptManager = new ScriptManager(scriptActivity.Script);

                    try
                    {
                        scriptManager.Compile("void");
                        Console.Write('.');
                        lastMsg = false;
                    }
                    catch (Exception e)
                    {
                        var msg =
                            String.Format(
                                @"Line: {5}; Path: [{6}]; Activity: [{4}]; Exception: {0}; ""{1}""; Source: {2}; Target: {3}",
                                e.GetType().Name, e.Message, e.Source,
                                e.TargetSite, id, i, pathName);
                        Logger.OutputLog(logFileName, msg + String.Format("\n -- Stack: {0}", e.StackTrace));
                        if (!lastMsg) Console.WriteLine();
                        Console.WriteLine(@"{0}. [{1}] {2}", i, id, pathName);
                        lastMsg = true;
                    }
                }
                else
                {
                    var link = edc.Entities.Object_Defs.OfType<Activity_Link>().FirstOrDefault(a => a.Id == id);

                    if (link != null)
                    {
                        var pathName = GetActivityPath(provider, id);

                        var scriptManager = new ScriptManager(link.Condition);

                        try
                        {
                            scriptManager.Compile("bool");
                            Console.Write('.');
                            lastMsg = false;
                        }
                        catch (Exception e)
                        {
                            var msg =
                                String.Format(
                                    @"Line: {5}; Path: [{6}]; Link: [{4}]; Exception: {0}; ""{1}""; Source: {2}; Target: {3}",
                                    e.GetType().Name, e.Message, e.Source,
                                    e.TargetSite, id, i, pathName);
                            Logger.OutputLog(logFileName, msg + String.Format("\n -- Stack: {0}", e.StackTrace));
                            if (!lastMsg) Console.WriteLine();
                            Console.WriteLine(@"{0}. [{1}] {2}", i, id, pathName);
                            lastMsg = true;
                        }
                    }
                    else
                    {
                        var msg = String.Format(@"[{0}]; Line: {2}; Activity: [{1}]; ""{3}""",
                            DateTime.Now.ToShortTimeString(), id, i, "Script Activity not found!");
                        Logger.OutputLog(logFileName, msg);
                        Console.WriteLine(@"{0}. [{1}] Script Activity not found!", i, id);
                    }
                }
            i++;
        }

        private static string GetActivityPath(IAppServiceProvider provider, Guid id)
        {
            var dataContext = provider.Get<IDataContext>();
            using (var command = dataContext.CreateCommand(SelectObjectPathNameSql))
            {
                var param = command.CreateParameter();
                param.ParameterName = "@id";
                param.Value = id;
                command.Parameters.Add(param);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        return reader.GetString(0);
                }
            }
            return String.Empty;
        }

        private const string SelectObjectPathNameSql = 
        @"select 
			CASE od5.Deleted WHEN 1 THEN '*!*' ELSE '' END + isnull('[' + od5.Full_Name + ']->', isnull('{' + od5.Name + '}->', '')) + 
				CASE od4.Deleted WHEN 1 THEN '*!*' ELSE '' END + isnull('[' + od4.Full_Name + ']->', isnull('{' + od4.Name + '}->', '')) + 
				CASE od3.Deleted WHEN 1 THEN '*!*' ELSE '' END + isnull('[' + od3.Full_Name + ']->', isnull('{' + od3.Name + '}->', '[-]')) + 
				CASE od2.Deleted WHEN 1 THEN '*!*' ELSE '' END + isnull('[' + od2.Full_Name + ']->', isnull('{' + od2.Name + '}->', '[-]')) + 
				CASE od1.Deleted WHEN 1 THEN '*!*' ELSE '' END + isnull('[' + od1.Full_Name + ']', isnull('{' + od1.Name + '}', '[-]')) as Object_Path
        from Object_Defs od1
			left outer join Object_Defs od2 on od1.Parent_Id = od2.Id
			left outer join Object_Defs od3 on od2.Parent_Id = od3.Id
			left outer join Object_Defs od4 on od3.Parent_Id = od4.Id
			left outer join Object_Defs od5 on od4.Parent_Id = od5.Id
        where od1.Id = @id";
        public static void CreateBaseServiceFactories()
        {
            AppServiceProvider.SetServiceFactoryFunc(typeof(IUserRepository),
                (arg) =>
                {
                    var prov = arg as IAppServiceProvider;
                    return new UserRepository(prov as IAppServiceProvider, prov.Get<IDataContext>());
                });
            AppServiceProvider.SetServiceFactoryFunc(typeof(IOrgRepository),
                prov =>
                    new OrgRepository((prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IAttributeRepository), (prov) => new AttributeRepository(prov as IAppServiceProvider));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocDefRepository),
                prov =>
                    new DocDefRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocRepository),
                prov =>
                    new DocRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocStateRepository),
                prov =>
                    new DocStateRepository((prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocumentTableMapRepository),
                (prov) =>
                    new DocumentTableMapRepository((prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(IEnumRepository),
                (prov) =>
                    new EnumRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IFormRepository),
                (prov) =>
                    new FormRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(ILanguageRepository),
                prov =>
                    new LanguageRepository(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IPermissionRepository),
                prov =>
                    new PermissionRepository(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(IWorkflowRepository), CreateWorkflowRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IWorkflowEngine), CreateWorkflowEngine);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IAttributeStorage),
                (prov, dc) =>
                    new ServiceDefInfo(new AttributeStorage(prov as IAppServiceProvider, dc as IDataContext), true));

            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocumentStorage),
                (prov, dc) =>
                    new ServiceDefInfo(new DocumentStorage(prov as IAppServiceProvider, dc as IDataContext), true));

            AppServiceProvider.SetServiceFactoryFunc(typeof(ITemplateReportGeneratorProvider),
                (prov, dc) =>
                    new ServiceDefInfo(new TemplateReportGeneratorProvider(prov as IAppServiceProvider, dc as IDataContext), true));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IControlFactory),
                (prov, dc) =>
                    new ServiceDefInfo(new ControlFactory(prov as IAppServiceProvider,
                        dc as IDataContext), false));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IComboBoxEnumProvider),
                prov =>
                    new ComboBoxEnumProvider(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilderFactory),
                prov =>
                    new SqlQueryBuilderFactory(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory),
                prov => new SqlQueryReaderFactory(prov as IAppServiceProvider,
                    (prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory),
                (prov, dc) =>
                    new ServiceDefInfo(new SqlQueryReaderFactory(prov as IAppServiceProvider,
                        dc as IDataContext), false));
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContextConfigSectionNameProvider), CreateDataContextConfigSectionNameProvider);

            //AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContext), CreateDataContext);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IMultiDataContext), CreateDataContext);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder),
                prov =>
                    new SqlQueryBuilderTool(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));
            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder), CreateSqlQueryBuilder2);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IQueryRepository),
                prov => new QueryRepository(prov as IAppServiceProvider,
                    (prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(IXlsFormDefBuilderFactory), prov => new XlsFormDefBuilderFactory(Get(prov)));
        }

        public static void CreateMdcBaseServiceFactories()
        {
            AppServiceProvider.SetServiceFactoryFunc(typeof(IUserRepository), CreateUserRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IOrgRepository), CreateOrgRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IAttributeRepository), CreateAttributeRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocDefRepository), CreateDocDefRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocRepository), CreateDocRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocStateRepository), CreateDocStateRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocumentTableMapRepository), CreateDocumentTableMapRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IEnumRepository), CreateEnumRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IFormRepository), CreateFormRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ILanguageRepository), CreateLanguageRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IPermissionRepository), CreatePermissionRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IWorkflowRepository), CreateWorkflowRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IWorkflowEngine), CreateWorkflowEngine);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IAttributeStorage), CreateAttributeStorage);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocumentStorage), CreateDocumentStorage);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ITemplateReportGeneratorProvider), CreateTemplateReportGeneratorProvider);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IControlFactory), CreateControlFactory);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IComboBoxEnumProvider), CreateComboBoxEnumProvider);

            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilderFactory), CreateSqlQueryBuilderFactory);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilderFactory), CreateSqlQueryBuilderFactory2);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory), CreateSqlQueryReaderFactory);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory), CreateSqlQueryReaderFactory2);

            AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContext), CreateDataContext);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IMultiDataContext), CreateDataContext);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder), CreateSqlQueryBuilder);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder), CreateSqlQueryBuilder2);

            AppServiceProvider.SetServiceFactoryFunc(typeof(IQueryRepository), CreateQueryRepository);

            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocDefStateListProvider), CreateMultiContextDocDefStateListProvider);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocDefStateListProvider), CreateDocDefStateListProvider);

            AppServiceProvider.SetServiceFactoryFunc(typeof(IXlsFormDefBuilderFactory), CreateXlsFormDefBuilderFactory);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IQueryRepository), CreateQueryRepository);
        }

        private static object CreateXlsFormDefBuilderFactory(object arg)
        {
            return new XlsFormDefBuilderFactory(Get(arg));
        }

        private static object CreateQueryRepository(object arg)
        {
            return new MultiContextQueryRepository(Get(arg));
        }

        private static object CreateDataContext(object arg)
        {
            var provider = Get(arg);
            var sectionNameProvider = provider.Get<IDataContextConfigSectionNameProvider>();

            var dataContextFactory = DataContextFactoryProvider.GetFactory();
            var dataContext = dataContextFactory.CreateMultiDc(sectionNameProvider.GetSectionName());

            return dataContext;
        }

        private static object CreateSqlQueryReaderFactory(object arg)
        {
            var provider = Get(arg);
            var mdc = provider.Get<IMultiDataContext>();
            var dataContext = mdc.GetDocumentContext; // mdc.Contexts.First(dc => dc.DataType.HasFlag(DataContextType.Document));

            return new SqlQueryReaderFactory(provider, dataContext);
        }
        private static ServiceDefInfo CreateSqlQueryReaderFactory2(object arg, object paramArg)
        {
            var provider = Get(arg);
            var dataContext = paramArg as IDataContext;

            return new ServiceDefInfo(new SqlQueryReaderFactory(provider, dataContext), true);
        }

        private static object CreateComboBoxEnumProvider(object arg)
        {
            return new MultiContextComboBoxEnumProvider(Get(arg));
        }

        private static ServiceDefInfo CreateControlFactory(object arg, object paramArg)
        {
            return new ServiceDefInfo(new ControlFactory(Get(arg), paramArg as IDataContext), true);
        }

        private static object CreateTemplateReportGeneratorProvider(object arg)
        {
            var provider = Get(arg);
            var mdc = provider.Get<IMultiDataContext>();
            var dataContext = mdc.GetDocumentContext; //mdc.Contexts.First(dc => dc.DataType.HasFlag(DataContextType.Document));

            return new TemplateReportGeneratorProvider(provider, dataContext);
        }

        private static ServiceDefInfo CreateDocumentStorage(object arg, object dataContext)
        {
            return new ServiceDefInfo(new DocumentStorage(Get(arg), dataContext as IDataContext), true);
        }

        private static object CreateMultiContextDocDefStateListProvider(object arg)
        {
            var provider = Get(arg);
            var mdc = provider.Get<IMultiDataContext>();

            return new MultiContextDocDefStateListProvider(provider, mdc);
        }
        private static ServiceDefInfo CreateDocDefStateListProvider(object arg, object dataContext)
        {
            return new ServiceDefInfo(new DocDefStateListProvider(Get(arg), dataContext as IDataContext), true);
        }

        private static ServiceDefInfo CreateAttributeStorage(object arg, object dataContext)
        {
            return new ServiceDefInfo(new AttributeStorage(Get(arg), dataContext as IDataContext), true);
        }

        private static object CreateWorkflowEngine(object arg)
        {
            var provider = Get(arg);
            var mdc = provider.Get<IDataContext>();
            // var dataContext = mdc.GetDocumentContext; // mdc.Contexts.First(dc => dc.DataType.HasFlag(DataContextType.Document));

            return new WorkflowEngine(provider, mdc);
        }

        private static object CreateWorkflowRepository(object arg)
        {
            return new /*MultiContext*/WorkflowRepository(Get(arg).Get<IDataContext>());
        }

        private static object CreatePermissionRepository(object arg)
        {
            return new MultiContextPermissionRepository(Get(arg));
        }

        private static object CreateFormRepository(object arg)
        {
            return new MultiContextFormRepository(Get(arg));
        }

        private static object CreateEnumRepository(object arg)
        {
            return new MultiContextEnumRepository(Get(arg));
        }

        private static object CreateDocumentTableMapRepository(object arg)
        {
            return new MultiContextDocumentTableMapRepository(Get(arg));
        }

        private static object CreateDocStateRepository(object arg)
        {
            return new MultiContextDocStateRepository(Get(arg));
        }

        private static object CreateDocRepository(object arg)
        {
            return new MultiContextDocRepository(Get(arg));
        }

        private static object CreateDocDefRepository(object arg)
        {
            return new MultiContextDocDefRepository(Get(arg));
        }

        private static object CreateAttributeRepository(object arg)
        {
            return new AttributeRepository(Get(arg));
        }

        private static object CreateOrgRepository(object arg)
        {
            return new MultiContextOrgRepository(Get(arg));
        }

        private static IAppServiceProvider Get(object arg)
        {
            var provider = arg as IAppServiceProvider;

            if (provider == null)
                throw new Exception("Service Factory method error! Cannot retreave IAppServiceProvider.");

            return provider;
        }
        private static object CreateUserRepository(object arg)
        {
            return new MultiContextUserRepository(Get(arg));
        }

        private static object CreateLanguageRepository(object arg)
        {
            return new MultiContextLanguageRepository(Get(arg));
        }

        private static object CreateSqlQueryBuilderFactory(object arg)
        {
            var provider = Get(arg);
            var mdc = provider.Get<IMultiDataContext>();
            var dataContext = mdc.GetDocumentContext; //.Contexts.First(dc => dc.DataType.HasFlag(DataContextType.Document));

            return new SqlQueryBuilderFactory(provider, dataContext);
        }
        private static ServiceDefInfo CreateSqlQueryBuilderFactory2(object arg, object paramArg)
        {
            var provider = Get(arg);
            var dataContext = paramArg as IDataContext;

            return new ServiceDefInfo(new SqlQueryBuilderFactory(provider, dataContext), true);
        }
        private static object CreateSqlQueryBuilder(object arg)
        {
            var provider = Get(arg);
            var mdc = provider.Get<IMultiDataContext>();
            var dataContext = mdc.GetDocumentContext; //.Contexts.First(dc => dc.DataType.HasFlag(DataContextType.Document));

            return new SqlQueryBuilderTool(provider, dataContext, true);
        }
        private static ServiceDefInfo CreateSqlQueryBuilder2(object arg, object paramArg)
        {
            var provider = Get(arg);
            var dataContext = paramArg as IDataContext;

            return new ServiceDefInfo(new SqlQueryBuilderTool(provider, dataContext, true), true);
        }

        private class ScriptInfo
        {
            public Guid Id { get; set; }
            public string Script { get; set; }
        }
    }
}
