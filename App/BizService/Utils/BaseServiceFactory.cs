using System;
using Intersoft.Cissa.Report.Builders;
using Intersoft.Cissa.Report.Defs;
using Intersoft.Cissa.Report.Xls;
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

namespace Intersoft.CISSA.BizService.Utils
{
    public class BaseServiceFactory
    {
        public const string DataContextConfigSectionName = "DataContexts";

        public static void CreateBaseServiceFactories()
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
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContextConfigSectionNameProvider), CreateDataContextConfigSectionNameProvider);

            AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContext), CreateDataContext);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IMultiDataContext), CreateDataContext);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder), CreateSqlQueryBuilder);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder), CreateSqlQueryBuilder2);

            AppServiceProvider.SetServiceFactoryFunc(typeof(IXlsFormDefBuilderFactory), CreateXlsFormDefBuilderFactory);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IQueryRepository), CreateQueryRepository);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IExternalProcessLauncher), CreateExternalProcessLauncher);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IBuilder<ReportDef, SqlQuery>), CreateSqlQueryFromReportDefBuilder);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IBuilder<ReportDef, XlsDef>), CreateXlsDefFromReportDefBuilder);

            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocDefStateListProvider), CreateMultiContextDocDefStateListProvider);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocDefStateListProvider), CreateDocDefStateListProvider);
        }

        private static object CreateXlsDefFromReportDefBuilder(object arg)
        {
            return new XlsDefFromReportDefBuilder(Get(arg));
        }

        private static object CreateSqlQueryFromReportDefBuilder(object arg)
        {
            var provider = Get(arg);
            var userId = provider.GetCurrentUserId();
            return new SqlQueryFromReportDefBuilder(provider, userId);
        }

        private static object CreateExternalProcessLauncher(object arg)
        {
            return new ExternalProcessLauncher();
        }

        private static object CreateQueryRepository(object arg)
        {
            return new MultiContextQueryRepository(Get(arg));
        }

        private static object CreateXlsFormDefBuilderFactory(object arg)
        {
            var provider = Get(arg);
            return new XlsFormDefBuilderFactory(provider);
        }

        private static object CreateDataContext(object arg)
        {
            var provider = Get(arg);
            var sectionNameProvider = provider.Get<IDataContextConfigSectionNameProvider>();

            var dataContextFactory = DataContextFactoryProvider.GetFactory();
            var dataContext = dataContextFactory.CreateMultiDc(sectionNameProvider.GetSectionName());

            return dataContext;
        }

        private static object CreateDataContextConfigSectionNameProvider(object arg)
        {
            return new DataContextConfigSectionNameProvider(DataContextConfigSectionName);
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
            return new MultiContextWorkflowRepository(Get(arg));
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
    }
}