using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Storage;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.Cissa.Report.Xls;
using System.Data.SqlClient;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;

namespace DocQueryTest
{
    [TestClass]
    public class SqlQueryTest1
    {
        public static readonly string ConnectionString =
            "Data Source=195.38.189.100;Initial Catalog=cissa-with-children;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        // Document Defs Id
        public static readonly Guid OrderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        public static readonly Guid ReportDefId = new Guid("{0E05462C-4A4C-4729-972E-5074DB1DED4E}");
        public static readonly Guid ReportItemDefId = new Guid("{3C1A7B35-8300-4E4D-9BD9-F9AC0D4C81D6}");
        public static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        public static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");

        public static readonly Guid OrderNoDefId = new Guid("{7CD9DD2A-139C-4984-8C02-7A029D2D76DD}");
        public static readonly Guid OrderApplicationDefId = new Guid("{D70E4319-8264-4044-B791-3C1F5F1B2DB5}");
        public static readonly Guid AppRegDateDefId = new Guid("{E36E02FA-BDD3-4E3B-978D-B5BB50B7BCB7}");
        // Payment Type Enum Id
        protected static readonly Guid PoorBenefitPaymentEnumId = new Guid("{D24151CF-C8B0-4851-B0EC-6D6EB382DC61}");
        protected static readonly Guid TwinsBenefitPaymentEnumId = new Guid("{7F1B9709-8F99-473F-9AE0-2DDCD74BDE6E}");
        protected static readonly Guid Till3BenefitPaymentEnumId = new Guid("{9BC8A898-31F8-4F55-8C14-28F641142370}");
        protected static readonly Guid TripletsBenefitPaymentEnumId = new Guid("{64ACC17D-78B8-492E-AC81-7B1E4750F53A}");
        protected static readonly Guid UnderWardBenefitPaymentEnumId = new Guid("{BCE5B287-7495-4AD1-96A8-F52040A4CABF}");
        protected static readonly Guid OnBirthBenefitPaymentEnumId = new Guid("{43F0ED4A-EFF2-425D-8564-683551BA8F82}");
        // Report Item Type Id                                          
        protected static readonly Guid ReportItemTypeId1 = new Guid("{535E984F-4365-4D10-8D93-1D5DE0071083}");
        protected static readonly Guid ReportItemTypeId2 = new Guid("{0FD2B01F-9741-487D-9944-568C5A9E7E5D}");
        protected static readonly Guid ReportItemTypeId3 = new Guid("{FD72C53E-60EE-4439-A1AA-94FA829F25EA}");
        protected static readonly Guid ReportItemTypeId4 = new Guid("{946E0876-18BD-445C-A4EC-DC302D170E8A}");
        // EmploymentStatuses
        protected static readonly Guid СhildStatusEnumId = new Guid("{7001020C-4188-492D-90B2-63E8F2DB0A2C}"); // Член семьи до 16 лет

        protected static readonly Guid DefaultUserId = new Guid("{180B1E71-6CDA-4887-9F83-941A12D7C979}"); // R
        protected static readonly Guid DefaultOrgId = new Guid("{34DDCAF2-EB08-48E7-894A-29C929D62C83}"); // Первомайский район

        protected static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден

        [TestMethod]
        public void TestMethod1()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var sql = new SqlQuery(OrderDefId, provider) {WithNoLock = true};

                        sql.AddAttribute(OrderNoDefId);
                        sql.AddAttribute(sql.Source, "DateFrom1");
                        sql.AddAttribute(sql.Source, "DateFrom");
                        sql.JoinSource(sql.Source, AppDefId, SqlSourceJoinType.Inner, OrderApplicationDefId);
                        sql.AddAttribute(AppRegDateDefId);
                        sql.AddCondition(ExpressionOperation.And, OrderNoDefId, ConditionOperation.Like, "101%");
                        sql.AddCondition(ExpressionOperation.Or, OrderNoDefId, ConditionOperation.Like, "102%");

                        var s = sql.BuildCountSqlScript();
                        Console.WriteLine(s.ToString());
                    }
                }
            }
        }

        [TestMethod]
        public void Report1()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var qb = new QueryBuilder(OrderDefId, DefaultUserId);

                        /* qb.Where("&OrgId").Eq(DefaultOrgId).And("&State").Eq(ApprovedStateId)
                            .And("Application").Include("PaymentType").Eq(PoorBenefitPaymentEnumId)
                                .Or("PaymentType").Eq(TwinsBenefitPaymentEnumId)
                                .Or("PaymentType").Eq(Till3BenefitPaymentEnumId)
                                .Or("PaymentType").Eq(TripletsBenefitPaymentEnumId)
                                .Or("PaymentType").Eq(UnderWardBenefitPaymentEnumId)
                              /* .Or("PaymentType").Eq(onBirthBenefitPaymentId)♥1♥.End()
                            .And("OrderPayments").Include("Year").Eq(2012).And("Month").Eq(7).End();
                        */
                        qb.Where( /*"&OrgId").Eq(DefaultOrgId).And(*/"&State").Eq(ApprovedStateId)
                            .And("Application").Include("PaymentType").In(new object[]
                            {
                                PoorBenefitPaymentEnumId, TwinsBenefitPaymentEnumId, Till3BenefitPaymentEnumId,
                                TripletsBenefitPaymentEnumId, UnderWardBenefitPaymentEnumId
                            }).End() /*
                            .And("OrderPayments").Include("Year").Eq(2012).And("Month").Eq(7).End()*/;

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var sql = sqlQueryBuilder.Build(qb.Def);

                        sql.AddAttributes(new[] {"&OrgName" /*, "PaymentType", "Month", "Year", "Amount"*/});
                        sql.AddAttribute("&Created", "cast({0} as date)");
                        sql.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
                        sql.AddGroupAttribute("&OrgName");
                        sql.AddGroupAttribute("&Created");
                        // sql.AddGroupAttribute("&PaymentType");

                        var s = sql.BuildSql();
                        Console.WriteLine(s.ToString());
                    }
                }
            }
        }

        [TestMethod]
        public void ShowNullApplicantInApps()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var qb = new QueryBuilder(AppDefId, DefaultUserId);

                        qb.Where("Applicant").IsNull();

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var sql = sqlQueryBuilder.Build(qb.Def);

                        sql.AddAttributes(new[] {"&OrgName", "RegNo", "RegDate", "&Created", "Applicant"});
                        sql.AddOrderAttribute("RegNo");
                        // sql.TopNo = 10;
                        sql.SkipNo = 10;
                        var s = sql.BuildSql();
                        Console.WriteLine(s.ToString());
                    }
                }
            }
        }

        [TestMethod]
        public void SqlQueryExecutorTest()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var qb = new QueryBuilder(OrderDefId, DefaultUserId);

                        qb.Where("&OrgId").Eq(DefaultOrgId).And("&State").Eq(ApprovedStateId)
                            .And("Application").Include("PaymentType").In(new object[]
                            {
                                PoorBenefitPaymentEnumId,
                                TwinsBenefitPaymentEnumId,
                                Till3BenefitPaymentEnumId,
                                TripletsBenefitPaymentEnumId,
                                UnderWardBenefitPaymentEnumId
                            }).End()
                            .And("OrderPayments").Include("Year").Eq(2012).And("Month").Eq(7).End();
                        
                        var sql = new SqlQueryDefExecutor(qb, dataContext);

                        Console.WriteLine(@"Кол-во записей: " + sql.Count());

                        var apps = sql.All<Guid>("Application");
                        foreach (var appId in apps)
                        {
                            Console.WriteLine(appId);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SqlQueryGroupingTest()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        using (var query = new SqlQuery(provider, OrderPaymentDefId, Guid.Empty))
                        {
                            var orders = query.JoinSource(query.Source, OrderDefId, SqlSourceJoinType.Inner,
                                "OrderPayments");
                            var apps = query.JoinSource(orders, AppDefId, SqlSourceJoinType.Inner, "Application");
                            query.AddAttribute(orders, "Application");
                            query.AddCondition(ExpressionOperation.And, OrderDefId, "&State", ConditionOperation.Equal,
                                ApprovedStateId);
                            query.AddCondition(ExpressionOperation.And, AppDefId, "&OrgId", ConditionOperation.Equal,
                                DefaultOrgId);
                            query.AddCondition(ExpressionOperation.And, AppDefId, "PaymentType", ConditionOperation.In,
                                new object[]
                                {
                                    PoorBenefitPaymentEnumId,
                                    TwinsBenefitPaymentEnumId,
                                    Till3BenefitPaymentEnumId,
                                    TripletsBenefitPaymentEnumId,
                                    UnderWardBenefitPaymentEnumId
                                });
                            query.AddGroupAttribute("Application");
                            query.AddAttribute(new[]
                            {
                                new SqlQuerySourceAttributeRef(query.Source, "Year"),
                                new SqlQuerySourceAttributeRef(query.Source, "Month")
                            }, "MAX(cast({0} as Int) * 12 + cast({1} as Int))");
                            query.AddHavingCondition(OrderPaymentDefId, new[] {"Year", "Month"},
                                "MIN(cast({0} as Int) * 12 + cast({1} as Int))", ConditionOperation.Equal,
                                2012*12 + 9);

                            // var sql = new SqlQueryDefExecutor(query);
                            // Console.WriteLine(@"Кол-во записей: " + sql.Count());

                            Console.WriteLine(query.BuildSql());
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SqlQueryReaderTest1()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var qb = new QueryBuilder(OrderDefId, DefaultUserId);

                        qb.Where("&OrgId").Eq(DefaultOrgId).And("&State").Eq(ApprovedStateId)
                            .And("Application").Include("PaymentType").In(new object[]
                            {
                                PoorBenefitPaymentEnumId,
                                TwinsBenefitPaymentEnumId,
                                Till3BenefitPaymentEnumId,
                                TripletsBenefitPaymentEnumId,
                                UnderWardBenefitPaymentEnumId
                            }).End()
                            .And("OrderPayments").Include("Year").Eq(2012).And("Month").Eq(7).End();

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var sql = sqlQueryBuilder.Build(qb.Def);

                        sql.AddAttributes(new[] {"&Id", "&OrgId", "&State", "PaymentType", "Month", "Year", "Amount"});
                        sql.AddAttribute("&Created", "cast({0} as date)");
                        // sql.AddAttribute("Assignments");

                        var s = sql.BuildSql();
                        Console.WriteLine(s.ToString());

                        // using (var reader = new SqlQueryReader(ConnectionString, sql))
                        // {
                        //      reader.Open();
                        //      var i = 1;
                        //      while (reader.Read())
                        //      {
                        //          var id = reader.GetValue(0);
                        //          Console.WriteLine(String.Format("{0}. {1}", i++, id != null ? id.ToString() : "NULL"));
                        //      }
                        // }
                    }
                }
            }
        }

        [TestMethod]
        public void SqlQueryReaderTest2()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {   
                        var qb1 = new QueryBuilder(OrderDefId, DefaultUserId);

                        qb1.Where("&OrgId").Eq(DefaultOrgId).And("&State").Eq(ApprovedStateId)
                            .And("Application").Include("PaymentType").In(new object[]
                            {
                                PoorBenefitPaymentEnumId,
                                TwinsBenefitPaymentEnumId,
                                Till3BenefitPaymentEnumId,
                                TripletsBenefitPaymentEnumId,
                                UnderWardBenefitPaymentEnumId
                            }).End()
                            .And("OrderPayments").Include("Year").Eq(2012).And("Month").Eq(7).End();

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        using (var sql = sqlQueryBuilder.Build(qb1.Def))
                        {
                            sql.AddAttributes(new[] {"&Id", "PaymentType"});
                            sql.AddAttribute("Amount", SqlQuerySummaryFunction.Sum);
                            sql.AddGroupAttributes(new[] {"&Id", "PaymentType"});

                            using (var reader = new SqlQueryReader(sql))
                            {
                                reader.Open();
                                var i = 1;
                                while (reader.Read())
                                {
                                    var id = reader.GetValue(0);
                                    Console.WriteLine(@"{0}. {1}", i++, id != null ? id.ToString() : "NULL");
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void SqlQueryReaderTest3()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var qb1 = new QueryBuilder(OrderDefId, DefaultUserId);
                        var qb2 = new QueryBuilder(AppDefId, DefaultUserId);

                        qb1.Where("&OrgId").Eq(DefaultOrgId).And("&State").Eq(ApprovedStateId)
                            .And("OrderPayments").Include("Year").Eq(2012).And("Month").Eq(7).End();

                        qb2.Where("PaymentType").In(new object[]
                        {
                            PoorBenefitPaymentEnumId, TwinsBenefitPaymentEnumId,
                            Till3BenefitPaymentEnumId,
                            TripletsBenefitPaymentEnumId, UnderWardBenefitPaymentEnumId
                        }).And("&Id").In(qb1.Def, "Application");

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        using (var sql = sqlQueryBuilder.Build(qb2.Def))
                        {
                            sql.AddAttributes("&Id", "&OrgId", "&State", "PaymentType");
                            sql.AddAttribute("&Created", "cast({0} as date)");

                            using (var reader = new SqlQueryReader(dataContext, sql))
                            {
                                reader.Open();
                                var i = 1;
                                while (reader.Read())
                                {
                                    var id = reader.GetValue(0);
                                    Console.Write(@"{0}. {1}", i++, id != null ? id.ToString() : "NULL");
                                    Console.Write(reader.GetValue(1));
                                    Console.Write(reader.GetValue(2));
                                    Console.WriteLine(reader.GetValue(3));
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestPrivilegeReport()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        dynamic report = BuildPrivilegeReport(provider, 2012, 7, DefaultUserId);
                        Assert.IsNotNull(report);
                        Console.WriteLine(report.AppCount);
                    }
                }
            }
        }

        public static DynaDoc BuildPrivilegeReport(IAppServiceProvider provider, int year, int month, Guid userId)
        {
            var billDefId = Guid.Parse("{4447EA34-67AB-46F2-BE03-A406CAC4EABC}");
            var userRepo = provider.Get<IUserRepository>(); // new UserRepository();
            var userInfo = userRepo.GetUserInfo(userId);

            if (year < 2011 || year > 3000)
                throw new ApplicationException("Ошибка в значении года!");
            if (month < 1 || month > 12)
                throw new ApplicationException("Ошибка в значении месяца!");

            if (userInfo.OrganizationId == null)
                throw new ApplicationException("Не могу создать заявку! Организация не указана!");

            var qb = new QueryBuilder(billDefId, userId);

            qb.Where("Year").Eq(year).And("Month").Eq(month)
                .And("Organization").Eq(userInfo.OrganizationId);

            var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
            var query = sqlQueryBuilder.Build(qb.Def); //new DocQuery(qb.Def);

            var docs = new List<Guid>(GetQueryFirst(provider.Get<IDataContext>(), query, 1).Cast<Guid>());

            dynamic bill = docs.Count == 0 ? DynaDoc.CreateNew(billDefId, userId, provider) : new DynaDoc(docs[0], userId, provider);

            bill.Year = year;
            bill.Month = month;
            bill.Organization = userInfo.OrganizationId;

            BuildPrivilegeReportCalcItems(provider, bill, userId, (Guid)userInfo.OrganizationId,
                      year, month);

            bill.Save();
            return bill;
        }

        private static IEnumerable<object> GetQueryFirst(IDataContext dataContext, SqlQuery query, int i)
        {
            using (var reader = new SqlQueryReader(dataContext, query))
            {
                reader.Open();
                while (i > 0 && reader.Read())
                {
                    yield return reader.GetValue(0);
                }
            }
        }

        public static void BuildPrivilegeReportCalcItems(IAppServiceProvider provider, dynamic bill, Guid userId, Guid orgId,
            int year, int month)
        {
            /*using (*/
            var docRepo = provider.Get<IDocRepository>(); // new DocRepository(userId))

            var paymentTypeDefId = Guid.Parse("{A9C9A563-6BE1-48CB-8C04-462D02B565F8}");
            var paymentOrderDefId = Guid.Parse("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
            var orderPaymentDefId = Guid.Parse("{AD83752B-C412-4FEC-A345-BB0495C34150}");
            var billItemDefId = Guid.Parse("{66605D33-A39E-4709-8534-C1505C041182}");
            var tariffDefId = new Guid("{0F29B75F-DE90-4910-9524-B74CB0418A57}");
            var privilege46PaymentId = new Guid("{7BEFD6DA-042C-4A77-90F3-A4424033E4DD}");

            int totalCount = 0;
            double totalSum = 0;

            var qTariff = new QueryBuilder(tariffDefId);
            qTariff.Where("PaymentType").Eq(privilege46PaymentId);

            var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
            var tariffs = sqlQueryBuilder.Build(qTariff.Def);
            tariffs.AddAttributes(new[] {"&Id", "Category"});

            var tariffCategories = new List<Guid>();
            var dataContext = provider.Get<IDataContext>();
            using (var reader = new SqlQueryReader(dataContext, tariffs))
            {
                reader.Open();

                while (reader.Read())
                {
                    var category = reader.GetValue(1);
                    if (category == null) continue;
                    var categoryId = (Guid) category;
                    tariffCategories.Add(categoryId);
                }
                reader.Close();
            }

            foreach (var categoryId in tariffCategories)
            {
                var total = 0m;

                var qbApps = new QueryBuilder(AppDefId, userId);
                qbApps.Where("PaymentType").Eq(privilege46PaymentId).And("PaymentCategory").Eq(categoryId);

                var qb = new QueryBuilder(paymentOrderDefId, userId);

                qb.Where("&OrgId").Eq(orgId).And("&State").Eq( /*"Утвержден"*/ApprovedStateId)
                    .And("Application").In(qbApps.Def, "&Id")
                    .And("OrderPayments").Include("Year").Eq(year).And("Month").Eq(month).End();

                var query = sqlQueryBuilder.Build(qb.Def);
                query.AddAttribute("Amount", SqlQuerySummaryFunction.Sum);
                using (var reader = new SqlQueryReader(dataContext, query))
                {
                    reader.Open();
                    if (reader.Read())
                    {
                        var val = reader.GetValue(0);
                        total = (decimal) (val ?? 0m);
                    }
                }

                int count = 0;
                qb = new QueryBuilder(OrderDefId, userId);

                qb.Where(/*"&Id"*/"Application").In(qbApps.Def, /*"Application"*/"&Id");

                query = sqlQueryBuilder.Build(qb.Def);
                query.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
                using (var reader = new SqlQueryReader(dataContext, query))
                {
                    reader.Open();
                    if (reader.Read())
                        count = (int) (reader.GetValue(0) ?? 0);
                    reader.Close();
                }

                if (count != 0 || total != 0)
                {
                    using (dynamic item = new DynaDoc(docRepo.New(billItemDefId), userId, provider))
                    {
                        item.Category = categoryId;
                        item.AppCount = count;
                        item.NeedAmount = total;
                        item.Save();
                        bill.AddDocToList("Rows", item.Doc);
                    }
                }
                totalCount += count;
                totalSum += (double) total;
            }
            bill.NeedAmount = totalSum;
            bill.AppCount = totalCount;
        }

        private static void Execute(WorkflowContextData contextData)
        {
           /* dynamic period = new DynaDoc(context.CurrentDocument, context.UserId);

            var userRepo = new UserRepository();
            var userInfo = userRepo.GetUserInfo(context.UserId);
            var year = (int?)period.Year ?? 0;
            var month = (int?)period.Month ?? 0;

            if (year < 2011 || year > 3000)
                throw new ApplicationException("Ошибка в значении года!");
            if (month < 1 || month > 12)
                throw new ApplicationException("Ошибка в значении месяца!");

            if (userInfo.OrganizationId == null)
                throw new ApplicationException("Не могу создать заявку! Организация не указана!");

            var qb = new QueryBuilder(ReportDefId, context.UserId);

            qb.Where("Year").Eq(year).And("Month").Eq(month)
                .And("Organization").Eq(userInfo.OrganizationId);

            var query = new DocQuery(qb.Def);

            var docs = new List<Guid>(query.First(1));

            dynamic report;

            if (docs.Count == 0)
                // Создать заявку на финансирование ГП и ДК                                                                                                          
                report = DynaDoc.CreateNew(ReportDefId, context.UserId);
            else
            {
                report = new DynaDoc(docs[0], context.UserId);
                report.ClearDocAttrList("Rows");
            }

            report.Year = year;
            report.Month = month;
            report.Organization = userInfo.OrganizationId;

            var docRepo = new DocRepository(context.UserId);

            var items = new List<Doc>();
            var item = docRepo.New(ReportItemDefId);
            item["Type"] = ReportItemTypeId4;
            items.Add(item);
            report.AddDocToList("Rows", item);
            item = docRepo.New(ReportItemDefId);
            item["Type"] = ReportItemTypeId3;
            items.Add(item);
            report.AddDocToList("Rows", item);
            item = docRepo.New(ReportItemDefId);
            item["Type"] = ReportItemTypeId2;
            items.Add(item);
            report.AddDocToList("Rows", item);
            var item1 = CreateItem1((Guid)userInfo.OrganizationId, year, month, context.UserId);
            item = (item1 != null) ? item1.Doc : docRepo.New(ReportItemDefId);
            item["Type"] = ReportItemTypeId1;
            items.Add(item);
            report.AddDocToList("Rows", item);

            CalcItems(report, context, (Guid)userInfo.OrganizationId,
                      year, month, items[3], items[2], items[1], items[0]);

            report.Save();
            context.CurrentDocument = report.Doc;*/
        }
/*
        private static void CalcItems(dynamic report, WorkflowContext context, Guid orgId,
            int year, int month, Doc item1, Doc item2, Doc item3, Doc item4)
        {
            int childrenTill3 = 0;
            int twinsTill3 = 0;
            int tripletsTill3 = 0;
            int tripletsTill16 = 0;
            int countTill3 = 0;
            int children3to16 = 0;
            int students = 0;
            int underwards = 0;
            int count3to16 = 0;
            double NeedAmountTill3 = 0;
            double NeedAmount3to16 = 0;

            var qb = new QueryBuilder(OrderDefId, context.UserId);

            qb.Where("&OrgId").Eq(orgId).And("&State").Eq("Утвержден")
                .And("Application").Include("PaymentType").Eq(PoorBenefitPaymentEnumId)
                    .Or("PaymentType").Eq(TwinsBenefitPaymentEnumId)
                    .Or("PaymentType").Eq(Till3BenefitPaymentEnumId)
                    .Or("PaymentType").Eq(TripletsBenefitPaymentEnumId)
                    .Or("PaymentType").Eq(UnderWardBenefitPaymentEnumId)
                /*            .Or("PaymentType").Eq(onBirthBenefitPaymentId)♥1♥.End()
                .And("OrderPayments").Include("Year").Eq(year).And("Month").Eq(month).End();

            var query = new DocQuery(qb.Def);

            foreach (var orderId in query.All())
            {
                dynamic order = new DynaDoc(orderId, context.UserId);
                dynamic app = new DynaDoc(order.GetAttrDoc("Application"), context.UserId);

                var opqb = new QueryBuilder(order.Doc, "OrderPayments", context.UserId);
                opqb.Where("Year").Eq(year).And("Month").Eq(month);

                var opQuery = new DocQuery(opqb.Def);
                double paymentSum = opQuery.Sum("Amount") ?? 0;

                item4["AppCount"] = 1 + (int)(item4["AppCount"] ?? 0);

                if (app.PaymentType == Till3BenefitPaymentEnumId)
                {
                    var assignmentIds = app.GetAttrDocIdList("Assignments");
                    childrenTill3 += assignmentIds.Count; // + (int)(item4["ChildrenTill3"] ?? 0); 
                    countTill3 += assignmentIds.Count; // + (int)(item4["CountTill3"] ?? 0); 
                    NeedAmountTill3 += paymentSum; // + (double)(item4["NeedAmountTill3"] ?? 0); 
                }
                else if (app.PaymentType == TwinsBenefitPaymentEnumId)
                {
                    var assignmentIds = app.GetAttrDocIdList("Assignments");
                    twinsTill3 += assignmentIds.Count;// + (int)(item4["TwinsTill3"] ?? 0); 
                    countTill3 += assignmentIds.Count;// + (int)(item4["CountTill3"] ?? 0); 
                    NeedAmountTill3 += paymentSum; // + (double)(item4["NeedAmountTill3"] ?? 0); 
                }
                else if (app.PaymentType == TripletsBenefitPaymentEnumId)
                {
                    var assignmentIds = app.GetAttrDocIdList("Assignments");
                    tripletsTill3 += assignmentIds.Count;// + (int)(item4["TripletsTill3"] ?? 0); 
                    countTill3 += assignmentIds.Count;// + (int)(item4["CountTill3"] ?? 0); 
                    NeedAmountTill3 += paymentSum; // + (double)(item4["NeedAmountTill3"] ?? 0); 
                }
                else if (app.PaymentType == UnderWardBenefitPaymentEnumId)
                {
                    var assignmentIds = app.GetAttrDocIdList("Assignments");
                    underwards += assignmentIds.Count;
                    count3to16 += assignmentIds.Count;
                    NeedAmount3to16 += paymentSum;
                }
                else if (app.PaymentType == PoorBenefitPaymentEnumId)
                {
                    var assignments = app.GetAttrDocList("Assignments");
                    foreach (var assignmnt in assignments)
                    {
                        dynamic assignment = new DynaDoc(assignmnt, context.UserId);

                        if (assignment.EmploymentStatus != null &&
                            assignment.EmploymentStatus == СhildStatusEnumId)
                            children3to16++;
                        else
                            students++;
                        count3to16++;
                    }
                    NeedAmount3to16 += paymentSum; // + (double)(item4["NeedAmount3to16"] ?? 0); 
                }
            }
            item4["ChildrenTill3"] = childrenTill3;
            item4["TwinsTill3"] = twinsTill3;
            item4["TripletsTill3"] = tripletsTill3;
            item4["Underwards"] = underwards;
            item4["Children3to16"] = children3to16;
            item4["Students"] = students;
            item4["CountTill3"] = countTill3;
            item4["Count3to16"] = count3to16;
            item4["BeneficiarCount"] = countTill3 + count3to16;
            item4["NeedAmountTill3"] = NeedAmountTill3;
            item4["NeedAmount3to16"] = NeedAmount3to16;
            item4["NeedAmount"] = NeedAmountTill3 + NeedAmount3to16;
            if (countTill3 > 0)
                item4["AvgAmountTill3"] = NeedAmountTill3 / countTill3;
            else
                item4["AvgAmountTill3"] = 0;
            if (count3to16 > 0)
                item4["AvgAmount3to16"] = NeedAmount3to16 / count3to16;
            else
                item4["AvgAmount3to16"] = 0;
            if (countTill3 > 0 || count3to16 > 0)
                item4["AvgAmount"] = (NeedAmountTill3 + NeedAmount3to16) / (countTill3 + count3to16);
            else
                item4["AvgAmount"] = 0;
            if (month == 1)
            {
                item4["YearNeedAmountTill3"] = NeedAmountTill3;
                item4["YearNeedAmount3to16"] = NeedAmount3to16;
                item4["YearNeedAmount"] = NeedAmountTill3 + NeedAmount3to16;
            }
            else if (item1 != null)
            {
                double prevNeedAmountTill3 = (double)(item1["YearNeedAmountTill3"] ?? 0);
                double prevNeedAmount3to16 = (double)(item1["YearNeedAmount3to16"] ?? 0);
                item4["YearNeedAmountTill3"] = prevNeedAmountTill3 + NeedAmountTill3;
                item4["YearNeedAmount3to16"] = prevNeedAmount3to16 + NeedAmount3to16;
                item4["YearNeedAmount"] = NeedAmountTill3 + NeedAmount3to16 +
                  prevNeedAmountTill3 + prevNeedAmount3to16;
            }
            report.AppCount = item4["AppCount"];
            report.BeneficiarCount = countTill3 + count3to16;
            report.AvgAmount = item4["AvgAmount"];
            report.NeedAmount = item4["NeedAmount"];
            report.YearNeedAmount = item4["YearNeedAmount"];
        }

        private static DynaDoc CreateItem1(Guid orgId, int year, int month, Guid userId)
        {
            var m = (month > 1) ? month - 1 : 12;
            var y = (month > 1) ? year : year - 1;

            var qb = new QueryBuilder(ReportDefId, userId);

            qb.Where("&OrgId").Eq(orgId)/*.And("&State").Eq("Утвержден")♥1♥
                .And("Year").Eq(y).And("Month").Eq(m);

            var query = new DocQuery(qb.Def);
            var prevReportId = query.FirstOrDefault();
            if (prevReportId == null) return null;

            var prevReport = new DynaDoc((Guid)prevReportId, userId);
            qb = new QueryBuilder(prevReport.Doc, "Rows", userId);
            qb.Where("Type").Eq(ReportItemTypeId4);

            query = new DocQuery(qb.Def);
            var reportItem4Id = query.FirstOrDefault();
            if (reportItem4Id == null) return null;

            var reportItem4 = new DynaDoc((Guid)reportItem4Id, userId);
            return reportItem4.Clone();
        }*/

        public static IAppServiceProvider GetProvider(IDataContext dataContext)
        {
            CreateBaseServiceFactories();
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            var userDataProv = new UserDataProvider(DefaultUserId, "DefaultUser");
            var provider = factory.Create(dataContext, userDataProv);
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
            AppServiceProvider.SetServiceFactoryFunc(typeof(IAttributeRepository),
                (prov) => new AttributeRepository(prov as IAppServiceProvider));
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
                    new ServiceDefInfo(
                        new TemplateReportGeneratorProvider(prov as IAppServiceProvider, dc as IDataContext), true));
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
            AppServiceProvider.SetServiceFactoryFunc(typeof(IQueryRepository),
               prov => new QueryRepository(prov as IAppServiceProvider,
                   (prov as IAppServiceProvider).Get<IDataContext>()));
        }
    }
}
