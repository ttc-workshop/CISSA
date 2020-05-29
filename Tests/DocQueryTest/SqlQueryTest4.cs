using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Storage;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace DocQueryTest
{
    [TestClass]
    public class SqlQueryTest4
    {
        public static readonly string ConnectionString =
                "Data Source=192.168.0.11;Initial Catalog=cissa;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        //    "Data Source=195.38.189.100;Initial Catalog=cissa;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        public static readonly string ChatkalConnectionString =
            "Data Source=localhost;Initial Catalog=cissa-4atkal;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        // Document Defs Id
        public static readonly Guid OrderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        public static readonly Guid ReportDefId = new Guid("{0E05462C-4A4C-4729-972E-5074DB1DED4E}");
        public static readonly Guid ReportItemDefId = new Guid("{3C1A7B35-8300-4E4D-9BD9-F9AC0D4C81D6}");
        public static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        public static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");

        public static readonly Guid OrderNoDefId = new Guid("{7CD9DD2A-139C-4984-8C02-7A029D2D76DD}");
        public static readonly Guid OrderApplicationDefId = new Guid("{D70E4319-8264-4044-B791-3C1F5F1B2DB5}");
        public static readonly Guid AppRegDateDefId = new Guid("{E36E02FA-BDD3-4E3B-978D-B5BB50B7BCB7}");

        // Asist
        public static readonly string AsistConnectionString =
            "Data Source=192.168.0.11;Initial Catalog=asist-meta;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        public static readonly string LAsistConnectionString =
            "Data Source=localhost;Initial Catalog=asist_db2;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        public static readonly Guid ScheduleDefId = new Guid("{31890BE3-B9C9-49B4-ADA3-4BAAB42D58F0}");
        public static readonly Guid DistrictScheduleDefId = new Guid("{B214C045-95EC-48C2-AD61-72F9ABAB19C8}");
        public static readonly Guid DistrictScheduleItemDefId = new Guid("{B43C8F14-78D5-481E-B613-652E1D96B221}");

        public static readonly Guid App2DefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}");
        public static readonly Guid AppStateDefId = new Guid("{547BBA55-2281-4388-A1FC-EE890168AC2D}");
        public static readonly Guid BankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}");
        public static readonly Guid AssignmentDefId = new Guid("{51935CC6-CC48-4DAC-8853-DA8F57C057E8}");

        public static readonly Guid AssignedStateId = new Guid("{ACB44CC8-BF44-44F4-8056-723CED22536C}");
        public static readonly Guid OnPaymentStateId = new Guid("{78C294B5-B6EA-4075-9EEF-52073A6A2511}");

        [TestMethod]
        public void TestMethod1()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        //var appQb = new QueryBuilder(AppDefId);
                        var orderQb = new QueryBuilder(OrderDefId);
                        orderQb.SetAlias("orders");
                        orderQb.Join(AppDefId, "app", on => on.And("Application", "orders").Eq("&Id", "app"));
                        orderQb.Where("postCode", "app").Eq("10");

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        using (var query = sqlQueryBuilder.Build(orderQb.Def))
                        {
                            var s = query.BuildSql();
                            Console.WriteLine(s);
                            using (var reader = new SqlQueryReader(dataContext, query))
                            {
                                reader.Open();
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestQueryDefJoin()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var docRepo = provider.Get<IDocRepository>(); // new DocRepository(dataContext);
                        var items = new List<AsistUnitTest1.SItem>();
                        var year = 2015;

                        var qb = new QueryBuilder(AssignmentDefId);
                        qb.SetAlias("assignment");
                        qb.Join(App2DefId, "app", on => on.And("Application", "assignment").Eq("&Id", "app"));
                        qb.And("Year", "assignment").Eq(year);
                        qb.And("Month", "assignment").In(new object[] {3, 5, 7});
                        qb.And("Date", "app").Lt(new DateTime(2016, 1, 1));

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var s = query.BuildSql();
                            Console.WriteLine(s);
                            using (var reader = new SqlQueryReader(dataContext, query))
                            {
                                reader.Open();
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestQueryDefLeftJoin()
        {
            var bankAccountDepositDefId = new Guid("{42EFADF1-2366-4B88-B1FC-540F29470520}");
            var noticeDefId = new Guid("{05A93EA0-1E7F-489D-B3A7-7024B31C6D50}"); //Уведомление о депонировании
            var accountTernoverDefId = new Guid("{982C14E1-3F9C-4559-8835-314C612AB021}"); // Оборот банковского счета 
            var bankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}");
            var appDefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}"); //Заявление на АСП
            var appStateDefId = new Guid("{547BBA55-2281-4388-A1FC-EE890168AC2D}");

            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var aqb = new QueryBuilder(noticeDefId);
                        aqb.SetAlias("notice");
                        var bqb = new QueryBuilder(bankAccountDepositDefId);
                        //bqb.Where("Bank_Account").IsNotNull();
                        //aqb.Where("&Id").NotIn(bqb.Def, "NoticeOfDeposit");
                        //aqb.LeftJoin(bqb.Def, "NoticeOfDeposit");
                        aqb.LeftJoin(bqb.Def, "bad", on => on.And("NoticeOfDeposit", "bad").Eq("&Id", "notice"));
                        //aqb.LeftJoin(bankAccountDepositDefId, "bad", on => on.And("NoticeOfDeposit", "bad").Eq("&Id", "notice")); // Успешно проверено!!! 2016-02-18
                        //aqb.LeftJoin(bqb.Def)

                        /*var source = new QuerySourceDef
                        {
                            DocDefId = bankAccountDepositDefId,
                            Alias = "bad"
                        };
                        var joinDef = new QueryJoinDef
                        {
                            Operation = SqlSourceJoinType.LeftOuter,
                            Source = source
                        };
                        aqb.Def.Sources.Add(source);
                        aqb.Def.Joins.Add(joinDef);
                        var joinCond = new QueryConditionDef()
                        {
                            Operation = ExpressionOperation.And,
                            Condition = ConditionOperation.Equal,
                            Left = new QueryConditionPartDef
                            {
                                Attribute = new QuerySingleAttributeDef
                                {
                                    Attribute = new QueryAttributeRef
                                    {
                                        Source = source,
                                        AttributeName = "NoticeOfDeposit"
                                    }
                                }
                            },
                            Right = new QueryConditionPartDef
                            {
                                Attribute = new QuerySingleAttributeDef
                                {
                                    Attribute = new QueryAttributeRef
                                    {
                                        Source = aqb.Def.Source,
                                        AttributeName = "&Id"
                                    }
                                }
                            }
                        };
                        joinDef.Conditions.Add(joinCond);*/
                        aqb.Where("NoticeOfDeposit", "bad").IsNull().Or("&Id").IsNull();

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        using (var query = sqlQueryBuilder.Build(aqb.Def))
                        {
                            var turnoverSrc = query.JoinSource(query.Source, accountTernoverDefId,
                                SqlSourceJoinType.Inner, "BankAccountTernover");
                            var accountSrc = query.JoinSource(turnoverSrc, bankAccountDefId, SqlSourceJoinType.Inner,
                                "BankAccount");
                            var appSrc = query.JoinSource(accountSrc, appDefId, SqlSourceJoinType.Inner, "Application");
                            var appStateSrc = query.JoinSource(appSrc, appStateDefId, SqlSourceJoinType.Inner,
                                "Application_State");
                            query.AddAttribute("&Id");
                            query.AddAttribute(turnoverSrc, "BankAccount");
                            query.AddAttribute(turnoverSrc, "BalanceEnd");
                            query.AddAttribute(appStateSrc, "DistrictId");

                            Console.WriteLine(query.BuildSql().ToString());
                        }
                    }
                }
            }
        }

        private static IAppServiceProvider GetProvider(IDataContext dataContext)
        {
            CreateBaseServiceFactories();
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            var provider = factory.Create(dataContext);
            return provider;
        }

        public static void CreateBaseServiceFactories()
        {
            AppServiceProvider.SetServiceFactoryFunc(typeof (IUserRepository),
                (arg) =>
                {
                    var prov = arg as IAppServiceProvider;
                    return new UserRepository(prov as IAppServiceProvider, prov.Get<IDataContext>());
                });
            AppServiceProvider.SetServiceFactoryFunc(typeof (IOrgRepository),
                prov =>
                    new OrgRepository((prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IAttributeRepository),
                (prov) => new AttributeRepository(prov as IAppServiceProvider));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IDocDefRepository),
                prov =>
                    new DocDefRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IDocRepository),
                prov =>
                    new DocRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IDocStateRepository),
                prov =>
                    new DocStateRepository((prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IDocumentTableMapRepository),
                (prov) =>
                    new DocumentTableMapRepository((prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof (IEnumRepository),
                (prov) =>
                    new EnumRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IFormRepository),
                (prov) =>
                    new FormRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof (ILanguageRepository),
                prov =>
                    new LanguageRepository(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IPermissionRepository),
                prov =>
                    new PermissionRepository(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

            //AppServiceProvider.SetServiceFactoryFunc(typeof(IWorkflowRepository), CreateWorkflowRepository);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IWorkflowEngine), CreateWorkflowEngine);
            AppServiceProvider.SetServiceFactoryFunc(typeof (IAttributeStorage),
                (prov, dc) =>
                    new ServiceDefInfo(new AttributeStorage(prov as IAppServiceProvider, dc as IDataContext), true));

            AppServiceProvider.SetServiceFactoryFunc(typeof (IDocumentStorage),
                (prov, dc) =>
                    new ServiceDefInfo(new DocumentStorage(prov as IAppServiceProvider, dc as IDataContext), true));

            AppServiceProvider.SetServiceFactoryFunc(typeof (ITemplateReportGeneratorProvider),
                (prov, dc) =>
                    new ServiceDefInfo(
                        new TemplateReportGeneratorProvider(prov as IAppServiceProvider, dc as IDataContext), true));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IControlFactory),
                (prov, dc) =>
                    new ServiceDefInfo(new ControlFactory(prov as IAppServiceProvider,
                        dc as IDataContext), false));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IComboBoxEnumProvider),
                prov =>
                    new ComboBoxEnumProvider(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof (ISqlQueryBuilderFactory),
                prov =>
                    new SqlQueryBuilderFactory(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilderFactory), CreateSqlQueryBuilderFactory2);
            AppServiceProvider.SetServiceFactoryFunc(typeof (ISqlQueryReaderFactory),
                prov => new SqlQueryReaderFactory(prov as IAppServiceProvider,
                    (prov as IAppServiceProvider).Get<IDataContext>()));

            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory), CreateSqlQueryReaderFactory2);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContextConfigSectionNameProvider), CreateDataContextConfigSectionNameProvider);

            //AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContext), CreateDataContext);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IMultiDataContext), CreateDataContext);
            AppServiceProvider.SetServiceFactoryFunc(typeof (ISqlQueryBuilder),
                prov =>
                    new SqlQueryBuilderTool(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));
            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder), CreateSqlQueryBuilder2);

            AppServiceProvider.SetServiceFactoryFunc(typeof (IXlsFormDefBuilderFactory),
                (prov) => new XlsFormDefBuilderFactory(prov as IAppServiceProvider));

            AppServiceProvider.SetServiceFactoryFunc(typeof(IQueryRepository),
                prov =>
                    new QueryRepository(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

        }
    }
}
