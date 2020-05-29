using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using ConsoleApplication1.Lists;
using ConsoleApplication1.Tests;
using ConsoleApplication1.Updates;
using Intersoft.Cissa.Report.Builders;
using Intersoft.Cissa.Report.Defs;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Interfaces;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Providers;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Storage;

namespace ConsoleApplication1
{
    public class Program
    {
        public const string CissaConnectionString = "Data Source=195.38.189.100;Initial Catalog=cissa;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True;Connect Timeout=120";
        public const string AsistDataConnectionString = "Data Source=192.168.0.54;Initial Catalog=asist-data;Password=WestWood-911;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True;Connect Timeout=120";
        public const string NrszConnectionString = "Data Source=195.38.189.100;Initial Catalog=asist_nrsz;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True;Connect Timeout=120";
        public const string AsistMetaConnectionString = "Data Source=192.168.0.54;Initial Catalog=asist-meta;Password=WestWood-911;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True;Connect Timeout=120";

        // Orgs
        public static readonly Guid FirstMayRaionOrgId = new Guid("{34DDCAF2-EB08-48E7-894A-29C929D62C83}");
        public static readonly Guid BalykchiOrgId = new Guid("2FEA2DF5-24F6-4E53-BF6C-22604AC014C1");
//        public static readonly Guid KarakuldjaOrgId = new Guid("2A0481C0-FB12-4048-9153-CB7AE997A26C");
        public static readonly Guid KaraBuraOrgId = new Guid("17A6CA38-8B7B-4F5C-973B-18F31646CC03");
        public static readonly Guid KochkorOrgId = new Guid("{B631F3B0-1656-49C5-9152-0252F304D29B}");
        public static readonly Guid[] PilotKgPostOrgs = {BalykchiOrgId, KaraBuraOrgId, KochkorOrgId};

        public static readonly Guid CardOfThePatient = new Guid("{683B63E2-B0C6-470F-8A01-5D73AB145F8A}");

        private static void Main(string[] args)
        {
//            CorrectAppStates();
//            GetDuplicateOrders();

//            Tests.SqlQueryExecutorTest.Test1();
//            Console.ReadKey();
            var time = DateTime.Now;
            Console.WriteLine(time.ToShortTimeString());
            using (var connection = new SqlConnection(AsistDataConnectionString))
            {
                using (var dataContext = new DataContext(connection, "doc", DataContextType.Document))
                {
                    using (var metaConnection = new SqlConnection(AsistMetaConnectionString))
                    {
                        using (
                            var metaDataContext = new DataContext(metaConnection, "meta",
                                DataContextType.Account | DataContextType.Meta))
                        {
                            using (var mdc = new MultiDataContext(new IDataContext[] {metaDataContext, dataContext}))
                            {
                                //CreateBaseServiceFactories();

                                // AllRaionOrderToXml.Start(dataContext);
                                //LoadAsistEditHints.LoadAsistFormHints(dataContext);
                                // AllRaionOrderToXml.Start(PilotKgPostOrgs, dataContext);

                                //return;

                                using (var provider = GetMdcProvider(mdc))
                                {
                                    //            dynamic report = Reports.PrivilegeReport.Build(2012, 7, UserId);
                                    //            Reports.SocialBenefitInfoReport.Build(2012, 7, UserId);
                                    //            Reports.PoorBenefitReport.Build(2012, 12, UserId);
                                    //            Reports.ChildBirthBenefitReport.Build(2012, 4, UserId);
                                    //              Reports.AppInputStatReport.Build();
                                    //            Lists.OrderList.Build(FirstMayRaionOrgId);
                                    //            SetAppPaymentCategory();
                                    //            Reports.AppDetailInputStatReport.Build(2012, 10, Guid.Empty);
                                    //            ShowNullApplicantInApps();
                                    //            Tests.GetDocList.OutputDocListToFile(@"c:\docList.xls", CardOfThePatient, Guid.Empty);
                                    //            SetMsecOrgAttributes.Start();
                                    //            SetTariffDates.SetDates();
                                    //            SetSocialLoadedAppSuffix.Start();
                                    //            Serialization.Execute();
                                    //            GetProlongApplications();
                                    //            SetApproveStateAppFromSocial.Start();
                                    //            TestDocListSqlQuery.OutputQuery();
                                    //            SetOrderTerminationDate.Start();
                                    //            Tests.SqlQueryExecutorTest.QueryStateTest2();
                                    //            UpdateAccountStates.Execute();
                                    //            SqlQueryExecutorTest.MsecReportTest3();
                                    //            Updates.FixLoadedSocialApplication.Start();
                                    //            SocialFundUnloader.GetSql();
                                    //            Lists.OrderToXml.Build(KochkorOrgId, provider, dataContext);
                                    //            Tests.WordDocBuilder.TestDocXBuild();
                                    //            Lists.FormControls2Excel.Build(provider, dataContext);
                                    CreateDistrictNoForBankPaymentRegistry.CreateDistrictNoBankPaymentRegistries(provider);
                                    var finishTime = DateTime.Now;
                                    Console.WriteLine(@"Finish at " + finishTime.ToShortTimeString());
                                    var dTime = finishTime - time;
                                    Console.WriteLine(dTime.TotalSeconds + @" s");
                                    Console.ReadKey();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static readonly Guid UserId = new Guid("{180B1E71-6CDA-4887-9F83-941A12D7C979}"); // R
        // Document Def Id
        private static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
        private static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        private static readonly Guid AssignmentDefId = new Guid("{5D599CE4-76C5-4894-91CC-4EB3560196CE}");

        private static readonly Guid StateNewId = new Guid("{5CD9E88D-671E-4A44-AD92-9F74DA3B47F7}");
        private static readonly Guid StateApprovedId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}");
        private static readonly Guid StatePaidId = new Guid("{3DB4DB00-3A7F-4228-A9A3-A413B85C18B4}");

        public static void CorrectAppStates(IAppServiceProvider provider, IDataContext dataContext)
        {
            Console.WriteLine(@"Start >>>");

            var qbOrders = new QueryBuilder(OrderDefId, UserId);
            qbOrders.Where("&State").Eq(StateApprovedId).Or("&State").Eq(StateNewId);

            var qbApps = new QueryBuilder(AppDefId, UserId);
            qbApps.Where("&State").Neq(StatePaidId).And("&Id").In(qbOrders.Def, "Application");

            //using (var dataContext = new DataContext())
            {
                var apps = new DocQuery(qbApps.Def, dataContext);
                var orders = new DocQuery(qbOrders.Def, dataContext);

                // var docRepo = new DocRepository(dataContext, UserId);
                var docRepo = provider.Get<IDocRepository>();

                foreach (var appId in apps.All())
                {
                    var app = docRepo.LoadById(appId);
                    //                docRepo.SetDocState();
                    var state = docRepo.GetDocState(app.Id);

                    Console.WriteLine(@"Id: {0}, No: {1}, State: {2}", appId, app["RegNo"] ?? "-", state != null ? state.Type.Name : "??");
                }
            }
        }

        public static void GetDuplicateOrders(IAppServiceProvider provider, IDataContext dataContext)
        {
            Console.WriteLine(@"Start >>>");

            var qbOrders = new QueryBuilder(OrderDefId, UserId);
            qbOrders.Where("&State").Eq(StateApprovedId);

            var qbOrders2 = new QueryBuilder(OrderDefId, UserId);
            qbOrders2.Where("&State").Eq(StateApprovedId).And("Application").In(qbOrders.Def, "Application").AndNot("&Id").In(qbOrders.Def);

            // using (var dataContext = new DataContext())
            {
                var orders = new DocQuery(qbOrders2.Def, dataContext);

                // var docRepo = new DocRepository(dataContext, UserId);
                var docRepo = provider.Get<IDocRepository>();

                foreach (var orderId in orders.All())
                {
                    var order = docRepo.LoadById(orderId);
                    var state = docRepo.GetDocState(order.Id);

                    Console.WriteLine(@"Id: {0}, No: {1}, State: {2}", orderId, order["No"] ?? "-", state != null ? state.Type.Name : "??");
                }
            }
        }

        public static void GetProlongApplications(IAppServiceProvider provider, IDataContext dataContext)
        {
            Console.WriteLine(@"Start >>>");

            var qbApps = new QueryBuilder(AppDefId, UserId);
            qbApps.Where("OriginalApplication").IsNotNull();

            //using (var dataContext = new DataContext())
            {
                var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                var apps = sqlQueryBuilder.Build(qbApps.Def);
                apps.AddAttribute("&OrgId");
                apps.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
                apps.AddGroupAttribute("&OrgId");

                // var orgRepo = new OrgRepository(dataContext);
                var orgRepo = provider.Get<IOrgRepository>();

                using (var reader = new SqlQueryReader(dataContext, apps))
                {
                    while (reader.Read())
                    {
                        var orgId = reader.GetGuid(0);
                        var count = reader.GetValue(1);
                        var orgName = orgRepo.Get(orgId);

                        Console.WriteLine(@"Id: {0}; Name: {1}, Count: {2}", orgId, orgName.Name, count);
                    }
                }
            }
        }

        private static void SaveAppPaymentCategoryData(IAppServiceProvider provider, IDataContext dataContext, Guid appId, Guid categoryId)
        {
            //using (var docRepo = new DocRepository(UserId))
            var docRepo = provider.Get<IDocRepository>();
            {
                var app = docRepo.LoadById(appId);

                app["PaymentCategory"] = categoryId;

                docRepo.Save(app);
                Console.WriteLine(app["RegNo"] + @" сохранен");
            }
        }

        private static void HandleAppPaymentCategory(IAppServiceProvider provider, IDataContext dataContext, Guid appId, string regNo, Guid paymentCategoryId, 
            IReadOnlyCollection<Guid> assignmentCategories, IList<EnumValue> categories)
        {
            if (assignmentCategories.Count == 0)
            {
                Console.WriteLine(regNo + @" нет назначений!!!!!");
                return;
            }
            var category = Guid.Empty;
            foreach (var assignmentCategory in assignmentCategories)
            {
                if (category == Guid.Empty) category = assignmentCategory;
                else if (category != assignmentCategory)
                {
                    Console.WriteLine(regNo + @" категории назначения разные!!!!!");
                    return;
                }
            }
            if (paymentCategoryId != Guid.Empty && paymentCategoryId != category) 
            {
                Console.WriteLine(regNo + @" категория назначения не соответствует назначению!!!!!");
                return;
            }
            if (paymentCategoryId == Guid.Empty) SaveAppPaymentCategoryData(provider, dataContext, appId, category);
            else Console.WriteLine(regNo + @" Ok");
        }

        public static void SetAppPaymentCategory(IAppServiceProvider provider, IDataContext dataContext)
        {
            var apps = new SqlQuery(AppDefId, provider);
            var assigns = apps.JoinSource(apps.Source, AssignmentDefId, SqlSourceJoinType.Inner, "Assignments");
//            apps.AddCondition(ExpressionOperation.And, apps.Source.GetDocDef(), "&OrgId", ConditionOperation.Equal,
//                              FirstMayRaionOrgId);
            apps.AddAttribute("&Id");
            apps.AddAttribute("PaymentCategory");
            apps.AddAttribute(assigns, "Category");
            apps.AddAttribute("RegNo");
            apps.AddOrderAttribute("&Id");
            apps.AddCondition(ExpressionOperation.And, AppDefId, "PaymentCategory", ConditionOperation.IsNull, (object) null);

            var currentAppId = Guid.Empty;
            var currentRegNo = "none";
            var paymentCategory = Guid.Empty;
            var assignmentNo = 0;
            var categoryEqual = false;
            var diffCategories = 0;
            var equalCategories = 0;
            var noCategories = 0;
            var assignCategories = new List<Guid>();

            try
            {
                using (var reader = new SqlQueryReader(dataContext, apps))
                {
                    reader.Open();

                    try
                    {
                        var enumRepo = provider.Get<IEnumRepository>(); // new EnumRepository();
                        var categories = enumRepo.GetEnumItems(new Guid("{9FF88649-11F9-4842-BD05-E0568F552724}"));

                        while (reader.Read())
                        {
                            var id = reader.GetGuid(0);
                            if (currentAppId == id) assignmentNo++;
                            else
                            {
                                if (currentAppId != Guid.Empty)
                                    HandleAppPaymentCategory(provider, dataContext, currentAppId, currentRegNo, paymentCategory,
                                                             assignCategories,
                                                             categories);

                                currentAppId = id;
                                currentRegNo = reader.IsDbNull(3) ? "???????" : reader.GetString(3);
                                assignmentNo = 0;
                                assignCategories.Clear();
                            }

                            paymentCategory = !reader.IsDbNull(1) ? reader.GetGuid(1) : Guid.Empty;
                            var assignmentCategory = !reader.IsDbNull(2) ? reader.GetGuid(2) : Guid.Empty;
                            assignCategories.Add(assignmentCategory);

                            categoryEqual = paymentCategory == assignmentCategory;
                            if (categoryEqual) equalCategories++;
                            else diffCategories++;
                            /*
                                                Console.Write(assignmentNo.ToString() + @";" + id.ToString());

                                                var enumValue = categories.FirstOrDefault(v => v.Id == paymentCategory);
                                                if (enumValue != null)
                                                    Console.Write(@";" + enumValue.Value);
                                                else
                                                    Console.Write(@";");
                                                enumValue = categories.FirstOrDefault(v => v.Id == assignmentCategory);
                                                if (enumValue != null)
                                                    Console.Write(@";" + enumValue.Value);
                                                else
                                                    Console.Write(@";");

                                                if (categoryEqual)
                                                    Console.WriteLine(@";1"); else
                                                    Console.WriteLine(@";0");*/
                        }
                        HandleAppPaymentCategory(provider, dataContext, currentAppId, currentRegNo, paymentCategory, assignCategories,
                                                 categories);
                        Console.WriteLine(@"Equal: {0}; Diff: {1}", equalCategories, diffCategories);
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            } 
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public static void ShowNullApplicantInApps(IAppServiceProvider provider, IDataContext dataContext)
        {
            var qb = new QueryBuilder(AppDefId, UserId);

            qb.Where("Applicant").IsNull();

            var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
            var sql = sqlQueryBuilder.Build(qb.Def);

            sql.AddAttributes("&OrgName", "RegNo", "RegDate", "&Created", "Applicant");
            sql.AddOrderAttribute("&Created", false);

            using (var r = new SqlQueryReader(dataContext, sql))
            {
                var i = 1;
                while (r.Read())
                {
                    var s = i.ToString();
                    if (!r.IsDbNull(0)) s += ";" + r.GetValue(0);
                    if (!r.IsDbNull(1)) s += ";" + r.GetValue(1);
                    if (!r.IsDbNull(2)) s += ";" + r.GetValue(2);
                    if (!r.IsDbNull(3)) s += ";" + r.GetValue(3);
                    if (!r.IsDbNull(4)) s += ";" + r.GetValue(4);
                    Console.WriteLine(s);
                    i++;
                }
            }
        }

        private static IAppServiceProvider GetProvider(IDataContext dataContext)
        {
            CreateBaseServiceFactories();
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            //var currentUser = new UserDataProvider(new Guid("{D24F7555-FF16-4AB9-B322-58DA57384EDC}"), "ФирузГуломов"); // Нурек
            //var currentUser = new UserDataProvider(new Guid("{0536B958-B9A6-45C8-9D1E-6827F9D32FA0}"), "ШахнозТохирова");  // Гиссар
            //var currentUser = new UserDataProvider(new Guid("{2D6819C9-DB76-43FC-8D9F-EC940539B014}"), "d"); // ASIST
            var provider = factory.Create(dataContext);

            // var currentUser = new UserDataProvider("МаритаТилемишова", "401", provider); // Балыкчи
            // var currentUser = new UserDataProvider("БактыгульКеримкулова", "231", provider); // УСР Кара-Буринского района
            //var currentUser = new UserDataProvider("ВенераМедербекова", "1709", provider); // УСР Кочкорского района
            var currentUser = new UserDataProvider("d", "123", provider);
            var regs = provider as IAppServiceProviderRegistrator;
            if (regs != null) regs.AddService(currentUser);
            return provider;
        }

        private static IAppServiceProvider GetMdcProvider(IDataContext dataContext)
        {
            CreateMdcBaseServiceFactories();
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            //var currentUser = new UserDataProvider(new Guid("{D24F7555-FF16-4AB9-B322-58DA57384EDC}"), "ФирузГуломов"); // Нурек
            //var currentUser = new UserDataProvider(new Guid("{0536B958-B9A6-45C8-9D1E-6827F9D32FA0}"), "ШахнозТохирова");  // Гиссар
            //var currentUser = new UserDataProvider(new Guid("{2D6819C9-DB76-43FC-8D9F-EC940539B014}"), "d"); // ASIST
            var provider = factory.Create(dataContext);

            // var currentUser = new UserDataProvider("МаритаТилемишова", "401", provider); // Балыкчи
            // var currentUser = new UserDataProvider("БактыгульКеримкулова", "231", provider); // УСР Кара-Буринского района
            //var currentUser = new UserDataProvider("ВенераМедербекова", "1709", provider); // УСР Кочкорского района
            var currentUser = new UserDataProvider("admin", "A10701", provider);
            var regs = provider as IAppServiceProviderRegistrator;
            if (regs != null) regs.AddService(currentUser);
            return provider;
        }

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

            //AppServiceProvider.SetServiceFactoryFunc(typeof(IWorkflowRepository), CreateWorkflowRepository);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IWorkflowEngine), CreateWorkflowEngine);
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

            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilderFactory), CreateSqlQueryBuilderFactory2);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory),
                prov => new SqlQueryReaderFactory(prov as IAppServiceProvider,
                    (prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof (ISqlQueryReaderFactory),
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

            AppServiceProvider.SetServiceFactoryFunc(typeof(IXlsFormDefBuilderFactory), CreateXlsFormDefBuilderFactory);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IQueryRepository), CreateQueryRepository);
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
