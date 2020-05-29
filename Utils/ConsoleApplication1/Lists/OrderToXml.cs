using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Lists
{
    public class OrderToXml
    {
        public const string ConnectionString = "Data Source=195.38.189.100;Initial Catalog=cissa-with-children;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True;Connect Timeout=120";
        public static readonly Guid OrderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        public static readonly Guid ReportDefId = new Guid("{0E05462C-4A4C-4729-972E-5074DB1DED4E}");
        public static readonly Guid ReportItemDefId = new Guid("{3C1A7B35-8300-4E4D-9BD9-F9AC0D4C81D6}");
        public static readonly Guid PostOrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        public static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
        public static readonly Guid AccountDefId = new Guid("{81C532F6-F5B0-4EFC-8305-44E864E778D3}");
        public static readonly Guid PersonDefId = new Guid("{6F5B8A06-361E-4559-8A53-9CB480A9B16C}");
        public static readonly Guid NotPaymentF20DefId = new Guid("{A0370B35-11A8-41D2-96AF-AB6C956DE5F1}");
        // States
        public static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден
        // Enums
        public static readonly Guid PaymentTypeEnumDefId = new Guid("{A9C9A563-6BE1-48CB-8C04-462D02B565F8}");
        public static readonly Guid CategoryTypeEnumDefId = new Guid("{9FF88649-11F9-4842-BD05-E0568F552724}");

        public static void Build(Guid orgId, IAppServiceProvider provider, IDataContext dataContext)
        {
            /*using (var connection = new SqlConnection(ConnectionString))
            {
                var dataContext = new DataContext(connection);

                using (var mdc = new MultiDataContext(new[] {dataContext}))
                {
                    var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                    using (var provider = providerFactory.Create(mdc))
                    {*/
            var orgRepo = provider.Get<IOrgRepository>();
            var orgInfo = orgRepo.Get(orgId);
                        var paymentQb = new QueryBuilder(OrderPaymentDefId);
                        paymentQb.Where("Year").Eq(2016).And("Month").Eq(1);
                        var subOrders = new QueryBuilder(PostOrderDefId);
            subOrders.Where("OrderPayments").In(paymentQb.Def, "&Id");
                     
                        var qb = new QueryBuilder(PostOrderDefId);
            qb.Where("&State").Eq(ApprovedStateId).And("&OrgId").Eq(orgId).And("&Id").In(subOrders.Def, "&Id"); //.And("OrderPayments").In(paymentQb.Def, "&Id");

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();

                        var query = sqlQueryBuilder.Build(qb.Def);

                        var app = query.JoinSource(query.Source, AppDefId, SqlSourceJoinType.Inner, "Application");
                        var account = query.JoinSource(query.Source, AccountDefId, SqlSourceJoinType.LeftOuter,
                            "Account");

                        query.AddAttribute("&Id");
                        query.AddAttribute("Application");
                        query.AddAttribute(app, "ZipCode");
                        query.AddAttribute(app, "PostCode");
                        query.AddAttribute(account, "AccountNo");
                        query.AddAttribute("Date");
                        query.AddAttribute(app, "RegNo");
                        var person = query.JoinSource(app, PersonDefId, SqlSourceJoinType.LeftOuter, "Applicant");
                        query.AddAttribute(person, "PIN");
                        query.AddAttribute(person, "LastName");
                        query.AddAttribute(person, "FirstName");
                        query.AddAttribute(person, "MiddleName");
                        query.AddAttribute(person, "PassportNo");
                        query.AddAttribute(person, "PassportDate");
                        query.AddAttribute(person, "PassportOrg");
                        query.AddAttribute(app, "PaymentType");
                        query.AddAttribute(app, "PaymentCategory");
                        query.AddAttributes("DateFrom", "DateTo", "DateFrom1", "DateTo1", "DateFrom2", "DateTo2",
                            "Amount1",
                            "Amount2");

                        query.AddOrderAttribute("PostCode");
                        query.AddOrderAttribute("AccountNo");
                        query.AddOrderAttribute("LastName");
                        query.AddOrderAttribute("FirstName");

                        /* var enumRepo = new EnumRepository(dataContext);
                var payments = enumRepo.GetEnumItems(PaymentTypeEnumDefId);
                var categories = enumRepo.GetEnumItems(CategoryTypeEnumDefId);*/

            var sql = query.BuildSql();
            var t = DateTime.Today;
            var fn = String.Format("c:\\distr\\cissa\\BiPost\\Sql-{0}-{1}-{2}.log", t.Year, t.Month, t.Day);
                        using (var writer = new StreamWriter(fn, true))
                        {
                            writer.WriteLine("{0}: {1}", DateTime.Now, sql);
                        }
           
                        var builder = new SqlQueryXmlBuilder(provider, dataContext, query) {AfterEachRecord = AddPayments};
                        var xml = builder.BuildAll(i =>
                        {
                            if (i%50 == 0)
                            {
                                Console.WriteLine(); Console.Write(@"  records: {0} ", i);
                            }
                            else Console.Write(@".");
                        });
                        var settings = new XmlWriterSettings()
                        {
                            Indent = true,
                            NewLineOnAttributes = true,
                            IndentChars = "\t",
                            Encoding = Encoding.UTF8
                        };
                        using (var xmlWriter = XmlWriter.Create(@"c:\distr\cissa\BiPost\orders-" + orgInfo.Name + "." + orgId + ".xml", settings))
                        {
                            xml.WriteTo(xmlWriter);
                        }
                        /*return;
                        using (var reader = new SqlQueryReader(dataContext, query))
                        {
                            reader.Open();
                            while (reader.Read())
                            {
                                var postCode = !reader.IsDbNull(0) ? reader.GetString(0) : String.Empty;
                                var accountNo = !reader.IsDbNull(1) ? reader.GetString(1) : String.Empty;
                                var regDate = !reader.IsDbNull(2)
                                    ? reader.GetDateTime(2).ToShortDateString()
                                    : String.Empty;
                                var regNo = !reader.IsDbNull(3) ? reader.GetString(3) : String.Empty;
                                var pin = !reader.IsDbNull(4) ? reader.GetString(4) : String.Empty;
                                var lastName = !reader.IsDbNull(5) ? reader.GetString(5) : String.Empty;
                                var firstName = !reader.IsDbNull(6) ? reader.GetString(6) : String.Empty;
                                var paymentType = !reader.IsDbNull(7) ? reader.GetGuid(7) : Guid.Empty;
                                var category = !reader.IsDbNull(8) ? reader.GetGuid(8) : Guid.Empty;
                                var dateFrom = !reader.IsDbNull(9)
                                    ? reader.GetDateTime(9).ToShortDateString()
                                    : String.Empty;
                                var dateTo = !reader.IsDbNull(10)
                                    ? reader.GetDateTime(10).ToShortDateString()
                                    : String.Empty;
                                var dateFrom1 = !reader.IsDbNull(11)
                                    ? reader.GetDateTime(11).ToShortDateString()
                                    : String.Empty;
                                var dateTo1 = !reader.IsDbNull(12)
                                    ? reader.GetDateTime(12).ToShortDateString()
                                    : String.Empty;
                                var dateFrom2 = !reader.IsDbNull(13)
                                    ? reader.GetDateTime(13).ToShortDateString()
                                    : String.Empty;
                                var dateTo2 = !reader.IsDbNull(14)
                                    ? reader.GetDateTime(14).ToShortDateString()
                                    : String.Empty;
                                var amount1 = !reader.IsDbNull(15) ? reader.GetDecimal(15) : 0m;
                                var amount2 = !reader.IsDbNull(16) ? reader.GetDecimal(16) : 0m;

                                /*var enumValue = payments.FirstOrDefault(v => v.Id == paymentType);
                        if (enumValue != null) ;
                        enumValue = categories.FirstOrDefault(v => v.Id == category);
                        if (enumValue != null) ;#1#
                            }
                            reader.Close();
                        }*/
                   /* }
                }
            }*/
        }

        private static void AddPayments(SqlQueryXmlBuilder parentBuilder, SqlQueryReader orderReader, IList<XElement> list)
        {
            var orderId = orderReader.GetGuid(0);
            var provider = parentBuilder.Provider;
//            using (var docRepo = new DocRepository(orderReader.DataContext))
            {
//                var order = docRepo.LoadById(orderId);
                var qb = new QueryBuilder(OrderPaymentDefId);

                var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                var query = sqlQueryBuilder.Build(qb.Def);
                var payment = query.Source;
                var order = query.JoinSource(query.Source, PostOrderDefId, SqlSourceJoinType.Inner, "OrderPayments");
                query.AndCondition(order, "&Id", ConditionOperation.Equal, orderId);

                query.AddAttribute(payment, "&Id");
                query.AddAttribute(payment, "Section");
                query.AddAttribute(payment, "Year");
                query.AddAttribute(payment, "Month");
                query.AddAttribute(payment, "Amount");

//                var account = query.JoinSource(order, AccountDefId, SqlSourceJoinType.Inner, "Account");
//                var notPayment = query.JoinSource(account, NotPaymentF20DefId, SqlSourceJoinType.LeftOuter, "NotPayment");

//                var exp = query.AddExpCondition(ExpressionOperation.And);
//                var exp1 = query.AddExpCondition(ExpressionOperation.And, exp);
//                query.AndCondition(query.Source, "Year", ConditionOperation.Equal, notPayment, "Year", exp1);
//                query.AndCondition(query.Source, "Month", ConditionOperation.Equal, notPayment, "Month", exp1);
//                var exp2 = query.AddExpCondition(ExpressionOperation.Or, exp);
                query.AndCondition(query.Source, "Year", ConditionOperation.Equal, 2016/*, exp2*/);
                query.AndCondition(query.Source, "Month", ConditionOperation.Equal, 1/*, exp2*/);

                query.AddOrderAttribute(payment, "Year");
                query.AddOrderAttribute(payment, "Month");

                var builder = new SqlQueryXmlBuilder(provider, parentBuilder.DataContext, query);
                list.Add(builder.BuildAll(null));
            }
        }
    }

    public class AllRaionOrderToXml
    {
        public static readonly Guid USR = new Guid("85EB79A9-7B5B-4ECB-A8E5-801489D729BB");
        public static readonly Guid HeadOfPaymentUnit = new Guid("{90C559A0-3624-4027-8C99-7413A1D44651}");
        public static readonly Guid SeniorSpecialistOfPaymentUnit = new Guid("{A8C66B60-1420-4CE5-87AE-D16CD080A6A2}");
        public static readonly Guid SpecialistOfPaymentUnit = new Guid("{1FED7A7D-5387-4BBD-AE95-62E5BD3F052B}");

        public static readonly Guid[] LoadedRaions = 
        {
            new Guid("915D0BEB-53C1-440F-9CBA-00612369FEFC"), new Guid("B631F3B0-1656-49C5-9152-0252F304D29B"),
            new Guid("1E5868C3-B522-4DBF-BC8F-02C75A899C0F"), new Guid("B0B7490F-E48D-4DC7-9548-0505D73C858F"),
            new Guid("3A305A9A-D30E-4C38-9E41-09DF993A658E"), new Guid("416366EE-580B-4E57-8641-0D90A2F5AB73"),
            new Guid("9375DF49-1B41-4EFD-BB79-0FC2A86ABED6"), new Guid("0A409F28-4A73-4CED-B368-10C6BE53419F"),
            new Guid("D319DC07-E7F7-4997-AFCD-17CEAD707B7F"), new Guid("17A6CA38-8B7B-4F5C-973B-18F31646CC03"),
            new Guid("C139D4DE-9F64-46D2-A908-1C477B78ECCD"), new Guid("2FEA2DF5-24F6-4E53-BF6C-22604AC014C1"),
            new Guid("F6CECF6A-2D5D-44C1-BC2F-29A836965531"), new Guid("34DDCAF2-EB08-48E7-894A-29C929D62C83"),
            new Guid("5026031D-2BC9-4A24-8DB5-30C7DD992352"), new Guid("73121A2F-34DF-4749-848B-319A12EACEE6"),
            new Guid("376FF578-372D-43A4-818E-31BC64327BEF"), new Guid("78D008DF-F7E8-4F99-92A1-42E7AE6E34C3"), 
            new Guid("E0AF1DF7-AA64-45DD-873E-510CD413AD35"), new Guid("17C0AC69-2247-41E8-B086-54599FE11CED"), 
            new Guid("43842F64-7BB7-45EC-930B-54AD19186382"), new Guid("20745158-EAE3-434C-BDF5-5F893C8963ED"), 
        };

        public static void Start(IDataContext dataContext)
        {
            var en = dataContext.GetEntityDataContext();
            foreach (var org in en.Entities.Object_Defs.OfType<Organization>().Where(o => o.Type_Id == USR && (o.Deleted == null || o.Deleted == false) && !LoadedRaions.Contains(o.Id)))
            {
                Console.WriteLine(@"
[{0}] Start " + org.Full_Name + @" raion [{1}]", DateTime.Now, org.Id);
                var worker =
                    en.Entities.Object_Defs.OfType<Worker>()
                        .First(
                            w =>
                                w.Parent_Id == org.Id && (w.Deleted == null || w.Deleted == false) &&
                                (w.OrgPosition_Id == HeadOfPaymentUnit || w.OrgPosition_Id == SeniorSpecialistOfPaymentUnit || w.OrgPosition_Id == SpecialistOfPaymentUnit));


                Console.WriteLine(@"  with username: " + worker.User_Name);
                var provider = GetProvider(dataContext, worker.Id, worker.User_Name);

                OrderToXml.Build(org.Id, provider, dataContext);
            }
        }

        public static void Start(Guid[] orgIds, IDataContext dataContext)
        {
            var en = dataContext.GetEntityDataContext();
            foreach (var orgId in orgIds)
            {
                var org = en.Entities.Object_Defs.OfType<Organization>().First(o => o.Id == orgId);
                Console.WriteLine(@"
[{0}] Start " + org.Full_Name + @" raion [{1}]", DateTime.Now, org.Id);
                var worker =
                    en.Entities.Object_Defs.OfType<Worker>()
                        .First(
                            w =>
                                w.Parent_Id == org.Id && (w.Deleted == null || w.Deleted == false) &&
                                (w.OrgPosition_Id == HeadOfPaymentUnit ||
                                 w.OrgPosition_Id == SeniorSpecialistOfPaymentUnit ||
                                 w.OrgPosition_Id == SpecialistOfPaymentUnit));


                Console.WriteLine(@"  with username: " + worker.User_Name);
                var provider = GetProvider(dataContext, worker.Id, worker.User_Name);

                OrderToXml.Build(org.Id, provider, dataContext);
            }
        }

        private static IAppServiceProvider GetProvider(IDataContext dataContext, Guid userId, string userName)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            var currentUser = new UserDataProvider(userId, userName);
            var provider = factory.Create(dataContext);
            var regs = provider as IAppServiceProviderRegistrator;
            if (regs != null) regs.AddService(currentUser);
            return provider;
        }
    }
}