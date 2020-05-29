using System;
using System.Data.SqlClient;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Utils;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Storage;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;


namespace Cissa.Tests.ReportTest
{
    [TestClass]
    public class CsvText
    {
        public const string AsistConnectionString =
            "Data Source=195.38.189.100;Initial Catalog=asist_db;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        public static readonly Guid PaymentDefId = new Guid("{68667FBB-C149-4FB3-93AD-1BBCE3936B6E}");
        public static readonly Guid BankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}");
        public static readonly Guid AssignmentDefId = new Guid("{51935CC6-CC48-4DAC-8853-DA8F57C057E8}");
        public static readonly Guid AppDefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}");
        public static readonly Guid PersonDefId = new Guid("{6052978A-1ECB-4F96-A16B-93548936AFC0}");
        public static readonly Guid AppStateDefId = new Guid("{547BBA55-2281-4388-A1FC-EE890168AC2D}");
        public static readonly Guid DistrictDefId = new Guid("{4D029337-C025-442E-8E93-AFD1852073AC}");
        public static readonly Guid DjamoatDefId = new Guid("{967D525D-9B76-44BE-93FA-BD4639EA515A}");
        public static readonly Guid VillageDefId = new Guid("{B70BAD68-7532-471F-A705-364FD5F1BA9E}");

        [TestMethod]
        public void XlsTestMethod()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        using (var def = new XlsDef())
                        {
                            var qb = new QueryBuilder(PaymentDefId);
                            //qb.Where("Registry").Eq(sheet.Id);
                            var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                            var query = sqlQueryBuilder.Build(qb.Def);
                            var bankAccountSrc = query.JoinSource(query.Source, BankAccountDefId,
                                SqlSourceJoinType.Inner,
                                "BankAccount");
                            var assignSrc = query.JoinSource(query.Source, AssignmentDefId, SqlSourceJoinType.Inner,
                                "Assignment");
                            var appSrc = query.JoinSource(assignSrc, AppDefId, SqlSourceJoinType.Inner, "Application");
                            var personSrc = query.JoinSource(appSrc, PersonDefId, SqlSourceJoinType.Inner, "Person");
                            var appStateSrc = query.JoinSource(appSrc, AppStateDefId, SqlSourceJoinType.Inner,
                                "Application_State");
                            var districtSrc = query.JoinSource(appStateSrc, DistrictDefId, SqlSourceJoinType.LeftOuter,
                                "DistrictId");
                            query.AddAttribute(personSrc, "IIN");
                            query.AddAttribute(bankAccountSrc, "Account_No");
                            query.AddAttribute(personSrc, "Last_Name");
                            query.AddAttribute(personSrc, "First_Name");
                            query.AddAttribute(personSrc, "Middle_Name");
                            query.AddAttribute(personSrc, "PassportSeries");
                            query.AddAttribute(personSrc, "PassportNo");
                            query.AddAttribute("&Id");
                            query.AddAttribute(assignSrc, "Amount");


                            var h = def.AddArea().AddRow();
                            h.AddNode("Id");
                            h.AddNode("ПИН");
                            h.AddNode("Номер банковского счета");
                            h.AddNode("Фамилия, Имя, Отчество");
                            h.AddNode("Номер паспорта");
                            h.AddNode("Сумма пособия");

                            using (var reader = new SqlQueryReader(dataContext, query))
                            {
                                while (reader.Read())
                                {
                                    var inn = !reader.IsDbNull(0) ? reader.GetString(0) : "";
                                    var accountNo = !reader.IsDbNull(1) ? reader.GetString(1) : "";
                                    var ln = !reader.IsDbNull(2) ? reader.GetString(2) : "";
                                    var fn = !reader.IsDbNull(3) ? reader.GetString(3) : "";
                                    var mn = !reader.IsDbNull(4) ? reader.GetString(4) : "";
                                    var ps = !reader.IsDbNull(5) ? reader.GetString(5) : "";
                                    var pNo = !reader.IsDbNull(6) ? reader.GetString(6) : "";
                                    var id = !reader.IsDbNull(7) ? reader.GetGuid(7) : Guid.Empty;
                                    decimal amount = !reader.IsDbNull(8) ? reader.GetDecimal(8) : 0;

                                    var r = def.AddArea().AddRow();
                                    r.AddColumn().AddText(id.ToString());
                                    r.AddColumn().AddText(inn);
                                    r.AddColumn().AddText(accountNo);
                                    r.AddColumn().AddText(string.Format("{0} {1} {2}", ln, fn, mn));
                                    r.AddColumn().AddText(string.Format("{0} {1}", ps, pNo));
                                    r.AddColumn().AddFloat((double) amount);

                                }
                            }

                            var builder = new XlsBuilder(def);
                            var workbook = builder.Build();
                            using (
                                var stream = new FileStream(
                                    "c:\\distr\\cissa\\UnloadingBankPaymentRegistry.xls", FileMode.Create))
                            {
                                workbook.Write(stream);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void CsvTestMethod()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        using (
                            var output = new StreamWriter("c:\\distr\\cissa\\UnloadingBankPaymentRegistry.csv", false,
                                System.Text.Encoding.UTF8))
                        {
                            var qb = new QueryBuilder(PaymentDefId);
                            // qb.Where("Registry").Eq(sheet.Id);
                            var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                            var query = sqlQueryBuilder.Build(qb.Def);
                            var bankAccountSrc = query.JoinSource(query.Source, BankAccountDefId,
                                SqlSourceJoinType.Inner,
                                "BankAccount");
                            var assignSrc = query.JoinSource(query.Source, AssignmentDefId, SqlSourceJoinType.Inner,
                                "Assignment");
                            var appSrc = query.JoinSource(assignSrc, AppDefId, SqlSourceJoinType.Inner, "Application");
                            var personSrc = query.JoinSource(appSrc, PersonDefId, SqlSourceJoinType.Inner, "Person");
                            var appStateSrc = query.JoinSource(appSrc, AppStateDefId, SqlSourceJoinType.Inner,
                                "Application_State");
                            var djamoatSrc = query.JoinSource(appStateSrc, DjamoatDefId, SqlSourceJoinType.LeftOuter,
                                "DjamoatId");
                            var villageSrc = query.JoinSource(appStateSrc, VillageDefId, SqlSourceJoinType.LeftOuter,
                                "VillageId");
                            query.AddAttribute(personSrc, "IIN");
                            query.AddAttribute(bankAccountSrc, "Account_No");
                            query.AddAttribute(personSrc, "Last_Name");
                            query.AddAttribute(personSrc, "First_Name");
                            query.AddAttribute(personSrc, "Middle_Name");
                            query.AddAttribute(personSrc, "Date_of_Birth");
                            query.AddAttribute(personSrc, "PassportSeries");
                            query.AddAttribute(personSrc, "PassportNo");
                            query.AddAttribute(personSrc, "Date_of_Issue");
                            query.AddAttribute(personSrc, "Issuing_Authority");
                            query.AddAttribute("&Id");
                            query.AddAttribute(djamoatSrc, "Name");
                            query.AddAttribute(villageSrc, "Name");
                            query.AddAttribute(appStateSrc, "Street");
                            query.AddAttribute(appStateSrc, "House");
                            query.AddAttribute(appStateSrc, "Flat");
                            // query.AddAttribute(assignSrc, "Amount");

                            var w = new QueryCsvWriter(provider, dataContext, query, output)
                            {
                                Header = true,
                                ValuePrepareFunc = ConvertTjToRusChars
                            };
                            w.AddField("&Id");
                            w.AddField("IIN");
                            w.AddField("Account_No");
                            w.AddField("Last_Name");
                            w.AddField("First_Name");
                            w.AddField("Middle_Name");
                            w.AddField("Date_of_Birth");
                            w.AddField("PassportSeries");
                            w.AddField("PassportNo");
                            w.AddField("Date_of_Issue");
                            w.AddField("Issuing_Authority");
                            w.AddField("Name", "Djamoat");
                            w.AddField("Name1", "Kishlak");
                            w.AddField("Street");
                            w.AddField("House");
                            w.AddField("Flat");

                            w.Write();
                        }
                    }
                }
            }
        }

        private static readonly IDictionary<char, char> TjChars = new Dictionary<char, char> { { 'ї', 'и' }, { 'ў', 'у' }, { 'њ', 'х' }, { 'љ', 'ч' }, { 'ќ', 'к' }, { 'ѓ', 'г' } };

        private static string ConvertTjToRusChars(int i, string s)
        {
            return TjChars.Aggregate(s, (current, pair) => current.Replace(pair.Key, pair.Value));
        }

        private IAppServiceProvider GetProvider(IDataContext dataContext)
        {
            CreateBaseServiceFactories();
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            var provider = factory.Create(dataContext);
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

            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory), CreateSqlQueryReaderFactory2);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContextConfigSectionNameProvider), CreateDataContextConfigSectionNameProvider);

            //AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContext), CreateDataContext);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IMultiDataContext), CreateDataContext);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder),
                prov =>
                    new SqlQueryBuilderTool(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));
            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder), CreateSqlQueryBuilder2);

            AppServiceProvider.SetServiceFactoryFunc(typeof(IXlsFormDefBuilderFactory),
                (prov) => new XlsFormDefBuilderFactory(prov as IAppServiceProvider));
        }
    }
}
