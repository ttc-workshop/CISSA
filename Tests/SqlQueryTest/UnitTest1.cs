using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlQueryTest
{
    [TestClass]
    public class UnitTest1
    {
        public const string ChatkalConnectionString =
            "Data Source=localhost;Initial Catalog=cissa-4atkal;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        public const string AsistConnectionString =
            "Data Source=localhost;Initial Catalog=asist_db;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        [TestMethod]
        public void FormWithQuerySourceDefData()
        {
            var assignNotificationTableFormId = new Guid("{DEDFA7AF-B042-4266-8C60-CFAAB3637F52}");

            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var formRepo = provider.Get<IFormRepository>(); // new FormRepository(dataContext);
                        var form = formRepo.GetTableForm(assignNotificationTableFormId);

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var query = sqlQueryBuilder.Build(form);

                        var sql = query.BuildSql();
                        using (var writer = new StreamWriter(@"c:\distr\cissa\testTableFormWithQueryDefData.sql", false))
                        {
                            writer.WriteLine(sql);
                        } 
                    }
                }
            }
        }

        [TestMethod]
        public void ComboBoxWithQuerySourceDefData()
        {
            var raionComboBoxId = new Guid("{F990047F-DD15-437C-B477-D067188FC1E3}");
            var raionDocDefId = new Guid("{4D029337-C025-442E-8E93-AFD1852073AC}");

            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var controlFactory = provider.Get<IControlFactory>(dataContext); // new FormRepository(dataContext);
                        var edc = dataContext.GetEntityDataContext().Entities;
                        var combo = controlFactory.Create(edc.Object_Defs.OfType<Combo_Box>().First(c => c.Id == raionComboBoxId));

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var query = sqlQueryBuilder.Build(combo, raionDocDefId);
                        query.AddAttribute("Name");

                        var sql = query.BuildSql();
                        using (var writer = new StreamWriter(@"c:\distr\cissa\testComboBoxWithQueryDefData.sql", false))
                        {
                            writer.WriteLine(sql);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void VillageFilterFormCascadeComboBoxTest()
        {
            var villageFilterFormId = new Guid("{DA4355F6-9B89-403A-94EE-55455F6C8802}");
            var oblastComboBoxId = new Guid("{D596DCF5-3BD1-45DC-8F3D-A1AEC274A58A}");
            var raionComboBoxId = new Guid("{CBA8FD77-520D-4B37-98E7-0CE70CC84246}");
            var djamoatComboBoxId = new Guid("{AE2810E3-F34A-414B-8084-E97252E716B9}");

            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var formRepo = provider.Get<IFormRepository>(); // new FormRepository(dataContext);
                        var form = formRepo.GetDetailForm(villageFilterFormId);

                        var comboBoxValueProvider = provider.Get<IComboBoxEnumProvider>();

                        var controlFinder = new ControlFinder(form);
                        var oblastComboBox = controlFinder.Find(oblastComboBoxId) as BizComboBox;
                        var raionComboBox = controlFinder.Find(raionComboBoxId) as BizComboBox;
                        var djamoatComboBox = controlFinder.Find(djamoatComboBoxId) as BizComboBox;

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var query = sqlQueryBuilder.Build(form);

                        var sql = query.BuildSql();
                        using (var writer = new StreamWriter(@"c:\distr\cissa\testVillageFilterFormCascadeComboBox.txt", false))
                        {
                            var oblastItems = comboBoxValueProvider.GetFormComboBoxValues(form, oblastComboBox);
                            var raionItems = comboBoxValueProvider.GetFormComboBoxValues(form, raionComboBox);
                            writer.WriteLine("1.");
                            writer.WriteLine(" - Oblast value = null");
                            writer.WriteLine(" - Raion items: {0}", raionItems.Count);
                            foreach (var item in raionItems)
                                writer.WriteLine("   - {0} - \"{1}\"", item.Id, item.Value);
                            writer.WriteLine(" - Raion value = null");
                            var djamoatItems = comboBoxValueProvider.GetFormComboBoxValues(form, djamoatComboBox);
                            writer.WriteLine(" - Djamoat items: {0}", djamoatItems.Count);
                            foreach (var item in djamoatItems)
                                writer.WriteLine("   - {0} - \"{1}\"", item.Id, item.Value);

                            writer.WriteLine("2.");
                            oblastComboBox.Value = oblastItems[4].Id;
                            writer.WriteLine(" - Oblast value = {0}, \"{1}\"", oblastComboBox.Value, oblastItems[4].Value);
                            raionItems = comboBoxValueProvider.GetFormComboBoxValues(form, raionComboBox);
                            writer.WriteLine(" - Raion items: {0}", raionItems.Count);
                            foreach (var item in raionItems)
                                writer.WriteLine("   - {0} - \"{1}\"", item.Id, item.Value);
                            raionComboBox.Value = raionItems[7].Id;
                            writer.WriteLine(" - Raion value = {0}, \"{1}\"", raionComboBox.Value, raionItems[7].Value);
                            djamoatItems = comboBoxValueProvider.GetFormComboBoxValues(form, djamoatComboBox);
                            writer.WriteLine(" - Djamoat items: {0}", djamoatItems.Count);
                            foreach (var item in djamoatItems)
                                writer.WriteLine("   - {0} - \"{1}\"", item.Id, item.Value);
                        } 
                    }
                }
            }
        }

        [TestMethod]
        public void GetComboBoxEnumList()
        {
            var raionComboBoxId = new Guid("{F990047F-DD15-437C-B477-D067188FC1E3}");
            var raionDocDefId = new Guid("{4D029337-C025-442E-8E93-AFD1852073AC}");
            var appFilterFormId = new Guid("{3F505704-729D-47E0-9201-07109818A9B9}");

            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var formRepo = provider.Get<IFormRepository>();
                        var form = formRepo.GetForm(appFilterFormId);
                        var finder = new ControlFinder(form);
                        var comboBoxValueProvider = provider.Get<IComboBoxEnumProvider>();
                        
                        var combo = finder.Find(raionComboBoxId) as BizComboBox;

                        var enumList = comboBoxValueProvider.GetFormComboBoxValues(form, combo);

                        using (var writer = new StreamWriter(@"c:\distr\cissa\testComboBoxEnumList.txt", false))
                        {
                            foreach (var item in enumList)
                            {
                                writer.WriteLine("- " + item.Id + " - " + item.Value);
                            }
                        }
                    }
                }
            }
        }

        private static readonly Guid BankAccTernDefId = new Guid("{982C14E1-3F9C-4559-8835-314C612AB021}"); //Оборот банковского счета
        private static readonly Guid BankTurnowerRepoDefId = new Guid("{BF01AAF9-4838-42C9-8F47-0171DCCD9C3D}");//отчет об оборотах
        private static readonly Guid AppDefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}");
        private static readonly Guid BankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}"); //Банковский счет
        private static readonly Guid AppStateDefId = new Guid("{547BBA55-2281-4388-A1FC-EE890168AC2D}");

        [TestMethod]
        public void TestQueryBuilderIncludeOperation()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var userRepo = provider.Get<IUserRepository>();
                        var userId = provider.GetCurrentUserId();
                        var userInfo = userRepo.GetUserInfo(userId);

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var qb = new QueryBuilder(BankAccTernDefId, userId);
                        qb.Where("&OrgId")
                            .Eq(userInfo.OrganizationId)
                            .And("Report")
                            .Include("LoadDate")
                            .Le(DateTime.Today)
                            .And("LoadDate")
                            .Ge(new DateTime(2015, 1, 1))
                            .End();
                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var bankAccountSrc = query.JoinSource(query.Source, BankAccountDefId,
                                SqlSourceJoinType.Inner,
                                "BankAccount");
                            var appSrc = query.JoinSource(bankAccountSrc, AppDefId, SqlSourceJoinType.Inner,
                                "Application");
                            var appStateSrc = query.JoinSource(appSrc, AppStateDefId, SqlSourceJoinType.Inner,
                                "Application_State");
                            /*var bankTurnOverReportSrc = query.JoinSource(query.Source, BankTurnowerRepoDefId,
                                SqlSourceJoinType.Inner, "Report");*/
                            var bankTurnOverReportSrc = query.FindSourceById(BankTurnowerRepoDefId);
                            var regAttr = query.AddAttribute(appStateSrc, "RegionId");
                            var disAttr = query.AddAttribute(appStateSrc, "DistrictId");
                            var djamAttr = query.AddAttribute(appStateSrc, "DjamoatId");
                            var dateAttr = query.AddAttribute(bankTurnOverReportSrc, "LoadDate");
                            query.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
                            query.AddGroupAttribute(regAttr);
                            query.AddGroupAttribute(disAttr);
                            query.AddGroupAttribute(djamAttr);
                            query.AddGroupAttribute(dateAttr);
                            //query.AndCondition(appSource, "&State", ConditionOperation.Equal, OnPaymentStateId);

                            var sql = query.BuildSql();
                            using (var writer = new StreamWriter(@"c:\distr\cissa\testQueryBuilderIncludeOperation.sql", false))
                            {
                                writer.WriteLine(sql);
                            }
                        }
                    }
                }
            }
        }

        public static IAppServiceProvider GetProvider(IDataContext dataContext)
        {
            CreateBaseServiceFactories();
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            //var currentUser = new UserDataProvider(new Guid("{D24F7555-FF16-4AB9-B322-58DA57384EDC}"), "ФирузГуломов"); // Нурек
            //var currentUser = new UserDataProvider(new Guid("{0536B958-B9A6-45C8-9D1E-6827F9D32FA0}"), "ШахнозТохирова");  // Гиссар
            var currentUser = new UserDataProvider(new Guid("{2D6819C9-DB76-43FC-8D9F-EC940539B014}"), "d");

            var provider = factory.Create(dataContext, currentUser);
            return provider;
        }

        private static void CreateBaseServiceFactories()
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

            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory), CreateSqlQueryReaderFactory2);
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
    }
}
