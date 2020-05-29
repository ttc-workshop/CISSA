using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlQueryTest
{
    [TestClass]
    public class AsistSqlQueryTest
    {
        public const string AsistConnectionString =
            "Data Source=192.168.0.11;Initial Catalog=asist-meta;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        public const string NrszConnectionString =
            "Data Source=localhost;Initial Catalog=nrsz_db;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        [TestMethod]
        public void BuildNoticeOfReminderTest()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = UnitTest1.GetProvider(dataContext))
                    {
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var docRepo = provider.Get<IDocRepository>();
                        // Получить последний банковский отчет
                        Doc report;
                        var rqb = new QueryBuilder(bankTurnoverReportDefId);
                        using (var query = sqlQueryBuilder.Build(rqb.Def))
                        {
                            query.AddAttribute("&Id");
                            query.AddOrderAttribute("PeriodTo", false);
                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                if (!reader.Read()) return;
                                report = docRepo.LoadById(reader.Reader.GetGuid(0));
                            }
                        }
                        var dateFrom = (DateTime) report["PeriodFrom"];
                        var dateTo = (DateTime) report["PeriodTo"];
                        var date = new DateTime(dateFrom.Year, dateFrom.Month, 1);
                        var noticeDate = date.AddMonths(-2);

                        var turnovers = new List<Doc>();
                        var nqb = new QueryBuilder(noteBalanceDefId);
                        /*var aqb = new QueryBuilder(assignmentDefId);
                        aqb.Where("Year")
                            .Eq(date.Year)
                            .And("Month")
                            .Eq(date.Month)
                            .And("&Id")
                            .NotIn(nqb.Def, "Assignment");*/

                        var qb = new QueryBuilder(bankAccountTurnoverDefId);
                        qb.Where("Report").Eq(report.Id).And("Status").Eq(SuccessAccountLoadEnumId)
                            .And("BalanceEnd").Gt(0m).And("&Id").NotIn(nqb.Def, "BankAccountTernover");
                        // .And("BankAccount").Include("Application").In(aqb.Def, "Application");

                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var accountSrc = query.JoinSource(query.Source, bankAccountDefId, SqlSourceJoinType.Inner,
                                "BankAccount");
                            var appSrc = query.JoinSource(accountSrc, appDefId, SqlSourceJoinType.Inner, "Application");
                            var assignmentSrc = query.JoinSource(appSrc, assignmentDefId, SqlSourceJoinType.Inner,
                                "Application");
                            query.AddAttribute(query.Source, "&Id");
                            query.AddAttribute(accountSrc, "Application");
                            query.AndCondition(assignmentSrc, "Year", ConditionOperation.Equal, noticeDate.Year);
                            query.AndCondition(assignmentSrc, "Month", ConditionOperation.Equal, noticeDate.Month);
                            var noticeQuery = sqlQueryBuilder.Build(nqb.Def);
                            query.AddCondition(ExpressionOperation.And, assignmentSrc, "&Id", ConditionOperation.NotIn, noticeQuery, "Assignment");

                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void BuildNoticeOfReminder_v2Test()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = UnitTest1.GetProvider(dataContext))
                    {
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var docRepo = provider.Get<IDocRepository>();
                        // Получить последний банковский отчет
                        /* Doc report;
                        var rqb = new QueryBuilder(bankTurnoverReportDefId);
                        using (var query = context.CreateSqlQuery(rqb.Def))
                        {
                            query.AddAttribute("&Id");
                            query.AddOrderAttribute("PeriodTo", false);
                            using (var reader = context.CreateSqlReader(query))
                            {
                                if (!reader.Read()) 
                                    throw new ApplicationException("Банковский отчет на найден!");
                                report = context.Documents.LoadById(reader.Reader.GetGuid(0));
                            }
                        }
                        var dateFrom = (DateTime)report["PeriodFrom"];
                        var dateTo = (DateTime)report["PeriodTo"];
                        var date = new DateTime(dateFrom.Year, dateFrom.Month, 1);
                        var noticeDate = date.AddMonths(-2);
                        context.Log(dateFrom.ToString());
                        context.Log(noticeDate.ToString()); */

                        var apps = new List<Guid>();
                        var reportPeriods = new List<DateTime>();
                        // var turnovers = new List<Doc>();                      
                        /* var aqb = new QueryBuilder(assignmentDefId, context.UserId);
                        aqb.Where("Year").Eq(date.Year).And("Month").Eq(date.Month).And("&Id").NotIn(nqb.Def, "Assignment"); */

                        var qb = new QueryBuilder(bankAccountTurnoverDefId);
                        qb.Where(/*"Report").Eq(report.Id).And(*/"Status").Eq(SuccessAccountLoadEnumId)
                            .And("BalanceEnd").Gt(0m); //.And("&Id").NotIn(nqb.Def, "BankAccountTernover");
                        // .And("BankAccount").Include("Application").In(aqb.Def, "Application");

                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var accountSrc = query.JoinSource(query.Source, bankAccountDefId, SqlSourceJoinType.Inner, "BankAccount");
                            var appSrc = query.JoinSource(accountSrc, appDefId, SqlSourceJoinType.Inner, "Application");
                            var assignmentSrc = query.JoinSource(appSrc, assignmentDefId, SqlSourceJoinType.Inner, "Application");
                            var reportSrc = query.JoinSource(query.Source, bankTurnoverReportDefId, SqlSourceJoinType.Inner,
                                "Report");
                            query.AndCondition(appSrc, "&State", ConditionOperation.Equal, OnPaidStateTypeId);
                            // query.AddAttribute(query.Source, "&Id");
                            query.AddAttribute(accountSrc, "Application");
                            query.AddAttribute(reportSrc, "PeriodTo", "MAX({0})");
                            query.AddAttribute(assignmentSrc, "&Id");
                            query.AddAttribute(assignmentSrc, "Year");
                            query.AddAttribute(assignmentSrc, "Month");
                            query.AddGroupAttribute("Application");
                            query.AddGroupAttribute(assignmentSrc, "&Id");
                            query.AddGroupAttribute(assignmentSrc, "Year");
                            query.AddGroupAttribute(assignmentSrc, "Month");
                            // query.AddAttribute(assignmentSrc, "&Id");
                            // query.AndCondition(assignmentSrc, "Year", ConditionOperation.Equal, noticeDate.Year);
                            // query.AndCondition(assignmentSrc, "Month", ConditionOperation.Equal, noticeDate.Month);

                            var nqb = new QueryBuilder(noteBalanceDefId);
                            var noticeQuery = sqlQueryBuilder.Build(nqb.Def);
                            query.AddCondition(ExpressionOperation.And, assignmentSrc, "&Id", ConditionOperation.NotIn, noticeQuery, "Assignment");

                            var i = 1;
                            using (var reader = new SqlQueryReader(dataContext, query))
                            {
                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());
                                while (reader.Read())
                                {
                                    // var turnoverId = reader.GetGuid(0);
                                    var appId = /* reader.IsDbNull(0) ? Guid.Empty :*/ reader.GetGuid(0);
                                    var reportPeriod = reader.IsDbNull(1) ? DateTime.MinValue : reader.GetDateTime(1);
                                    var assignmentId = reader.IsDbNull(2) ? Guid.Empty : reader.GetGuid(2);
                                    var assignmentYear = reader.IsDbNull(3) ? 0 : reader.GetInt32(3);
                                    var assignmentMonth = reader.IsDbNull(4) ? 0 : reader.GetInt32(4);

                                    if (reportPeriod != DateTime.MinValue && assignmentYear != 0 && assignmentMonth != 0)
                                    {
                                        var noticeDate = new DateTime(reportPeriod.Year, reportPeriod.Month, 1).AddMonths(-2);
                                        if (noticeDate == new DateTime(assignmentYear, assignmentMonth, 1))
                                        {
                                            apps.Add(appId);
                                            reportPeriods.Add(reportPeriod);
                                            Console.WriteLine(@"{0}. appId='{1}'; ym='{2}.{3}'; reportDate='{4}'", i, appId, assignmentYear, assignmentMonth, reportPeriod);
                                        }
                                    }
                                    i++;
                                }
                            }
                        }
                        /*if (apps.Count > 0)
                        {
                            var bqb = new QueryBuilder(bankAccountTurnoverDefId);
                            bqb.Where("Status").Eq(SuccessAccountLoadEnumId)
                                .And("BalanceEnd").Gt(0m)
                                .And("BankAccount").Include("Application").Eq(Guid.Empty, "AppId").End()
                                .And("Report").Include("PeriodTo").Eq(DateTime.MinValue, "PeriodTo");

                            using (var query = sqlQueryBuilder.Build(bqb.Def))
                            {
                                query.AddAttribute("&Id");
                                for (var i = 0; i < apps.Count; i++)
                                {
                                    var appId = apps[i];
                                    var reportPeriod = reportPeriods[i];
                                    query.SetParams("AppId", appId);
                                    query.SetParams("PeriodTo", reportPeriod);

                                    using (var reader = new SqlQueryReader(dataContext, query))
                                    {
                                        if (reader.Read())
                                        {
                                            /*Doc note = docRepo.New(noteBalanceDefId);
                                            note["BankAccountTernover"] = reader.Reader.GetGuid(0);
                                            // note["Assignment"] = assignmentId;
                                            docRepo.Save(note);
                                            docRepo.SetDocState(note.Id, newStateId);#1#
                                            Console.WriteLine();
                                        }
                                    }
                                }
                            }
                        } */
                    }
                }
            }
        }

        [TestMethod]
        public void BuildNewNoticeOfReminderTest()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = UnitTest1.GetProvider(dataContext))
                    {
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var docRepo = provider.Get<IDocRepository>();
                        /*// Получить последний банковский отчет
                        Doc report;
                        var rqb = new QueryBuilder(bankTurnoverReportDefId);
                        using (var query = sqlQueryBuilder.Build(rqb.Def))
                        {
                            query.AddAttribute("&Id");
                            query.AddOrderAttribute("PeriodTo", false);
                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                if (!reader.Read()) return;
                                report = docRepo.LoadById(reader.Reader.GetGuid(0));
                            }
                        }
                        var dateFrom = (DateTime)report["PeriodFrom"];
                        var dateTo = (DateTime)report["PeriodTo"];
                        var date = new DateTime(dateFrom.Year, dateFrom.Month, 1);
                        var noticeDate = date.AddMonths(-2);*/

                        var turnovers = new List<Doc>();
                        /*var aqb = new QueryBuilder(assignmentDefId);
                        aqb.Where("Year")
                            .Eq(date.Year)
                            .And("Month")
                            .Eq(date.Month)
                            .And("&Id")
                            .NotIn(nqb.Def, "Assignment");*/

                        var qb = new QueryBuilder(bankAccountTurnoverDefId);
                        qb.Where(/*"Report").Eq(report.Id).And(*/"Status").Eq(SuccessAccountLoadEnumId)
                            .And("BalanceEnd").Gt(0m); //.And("BankAccount").Include().NotIn(nqb.Def, "BankAccountTernover");
                        // .And("BankAccount").Include("Application").In(aqb.Def, "Application");

                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var accountSrc = query.JoinSource(query.Source, bankAccountDefId, SqlSourceJoinType.Inner,
                                "BankAccount");
                            var appSrc = query.JoinSource(accountSrc, appDefId, SqlSourceJoinType.Inner, "Application");
                            var assignmentSrc = query.JoinSource(appSrc, assignmentDefId, SqlSourceJoinType.Inner,
                                "Application");
                            var reportSrc = query.JoinSource(query.Source, bankTurnoverReportDefId, SqlSourceJoinType.Inner,
                                "Report");
                            query.AndCondition(appSrc, "&State", ConditionOperation.Equal, OnPaidStateTypeId);
                            query.AddAttribute(accountSrc, "Application");
                            query.AddAttribute(reportSrc, "PeriodTo", "MAX({0})");
                            query.AddAttribute(new[]
                            {
                                new SqlQuerySourceAttributeRef(assignmentSrc, "Year"),
                                new SqlQuerySourceAttributeRef(assignmentSrc, "Month")
                            }, "MAX(cast({0} as Int) * 16 + cast({1} as Int))");
                            query.AddGroupAttribute("Application");

                            var nqb = new QueryBuilder(noteBalanceDefId);
                            var noticeQuery = sqlQueryBuilder.Build(nqb.Def);
                            var bats = noticeQuery.JoinSource(noticeQuery.Source, bankAccountTurnoverDefId, SqlSourceJoinType.Inner, "BankAccountTernover");
                            var bas = noticeQuery.JoinSource(bats, bankAccountDefId, SqlSourceJoinType.Inner, "BankAccount");
                            noticeQuery.AddAttribute(bas, "Application");
                            query.AddCondition(ExpressionOperation.And, appSrc, "&Id", ConditionOperation.NotIn, noticeQuery, "Application");

                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void BuildDepositNotificationTest()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = UnitTest1.GetProvider(dataContext))
                    {
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var docRepo = provider.Get<IDocRepository>();

                        var nqb = new QueryBuilder(noteDepositDefId);
                        var tqb = new QueryBuilder(bankAccountTurnoverDefId);
                        tqb.Where("Status").Eq(SuccessAccountLoadEnumId)
                            .And("BalanceEnd").Gt(0m).And("&Id").NotIn(nqb.Def, "BankAccountTernover");

                        using (var query = sqlQueryBuilder.Build(tqb.Def))
                        {
                            var accountSrc = query.JoinSource(query.Source, bankAccountDefId, SqlSourceJoinType.Inner,
                                "BankAccount");
                            var appSrc = query.JoinSource(accountSrc, appDefId, SqlSourceJoinType.Inner, "Application");
                            var assignmentSrc = query.JoinSource(appSrc, assignmentDefId, SqlSourceJoinType.Inner,
                                "Application");
                            var reportSrc = query.JoinSource(query.Source, bankTurnoverReportDefId, SqlSourceJoinType.Inner,
                                "Report");
                            var paymentSrc = query.JoinSource(assignmentSrc, PaymentDefId, SqlSourceJoinType.LeftOuter,
                                "Assignment");
                            query.AndCondition(appSrc, "&State", ConditionOperation.Equal, OnPaidStateTypeId);
                            query.AndCondition(paymentSrc, "BankAccount", ConditionOperation.Equal, accountSrc, "&Id");
                            query.AddAttribute(accountSrc, "Application");
                            query.AddAttribute(reportSrc, "PeriodTo", "MAX({0})");
                            query.AddAttribute(new[]
                            {
                                new SqlQuerySourceAttributeRef(assignmentSrc, "Year"),
                                new SqlQuerySourceAttributeRef(assignmentSrc, "Month")
                            }, "MAX(cast({0} as Int) * 16 + cast({1} as Int))");
                            query.AddAttribute(assignmentSrc, "&Id", "count({0})");
                            query.AddAttribute(paymentSrc, "&Id", "count({0})");
                            query.AddAttribute(appSrc, "No");
                            //query.AddAttribute(accountSrc, "&Id");
                            query.AddAttribute(query.Source, "&Id", "count({0})");
                            query.AddGroupAttribute(accountSrc, "Application");
                            query.AddGroupAttribute(appSrc, "No");
                            //query.AddGroupAttribute(accountSrc, "&Id");
                            
                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());
                                while (reader.Read())
                                {
                                    var appId = reader.GetGuid(0);
                                    var reportPeriod = reader.IsDbNull(1) ? DateTime.MinValue : reader.GetDateTime(1);
                                    var lastAssignPeriod = reader.IsDbNull(2) ? 0 : reader.GetInt32(2);
                                    var assignmentCount = reader.IsDbNull(3) ? 0 : reader.GetInt32(3);
                                    var paymentCount = reader.IsDbNull(4) ? 0 : reader.GetInt32(4);

                                    if (reportPeriod != DateTime.MinValue && lastAssignPeriod != 0)
                                    {
                                        var lastAssignMonth = lastAssignPeriod%16;
                                        var lastAssignYear = Convert.ToInt32(lastAssignPeriod/16);
                                        Console.WriteLine(lastAssignYear + @"." + lastAssignMonth + @"; " + assignmentCount + @", " + paymentCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        [TestMethod]
        public void SqlQueryStateConditionTest()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = UnitTest1.GetProvider(dataContext))
                    {
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var docRepo = provider.Get<IDocRepository>();
                        var sqlReaderBuilder = provider.Get<ISqlQueryReaderFactory>();

                        var qb = new QueryBuilder(DistrictNoRegistryDefId);
                        qb.Where("&State").IsNull();

                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            Console.WriteLine(query.BuildSql());
                            using (var reader = sqlReaderBuilder.Create(query))
                            {
                                if (reader.Read())
                                    Console.WriteLine(@"Не могу утвердить реестр! В реестре имеются оплаты без статуса!");
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestQueryDefSql()
        {
            var registryDefId = new Guid("{B3BB3306-C3B4-4F67-98BF-B015DEEDEFFF}"); // Банк.реестр на оплату
            var districtNoRegistryDefId = new Guid("{DB434DEC-259F-4563-9213-301D9E38753D}");

            var deferredStateId = new Guid("{685EE7C0-ADD3-4C61-A893-E536959A09EA}");

            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = UnitTest1.GetProvider(dataContext))
                    {
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var docRepo = provider.Get<IDocRepository>();
                        var sqlReaderBuilder = provider.Get<ISqlQueryReaderFactory>();

                        var year = 2016;
                        var month = 5;

                        var rqb = new QueryBuilder(districtNoRegistryDefId);
                        var rgqb = new QueryBuilder(registryDefId);
                        rgqb.Where("DeferredRegistry").IsNotNull();
                        var qb = new QueryBuilder(registryDefId);

                        rqb.Where("&State").Eq(deferredStateId);
                        qb.Where("&Id").In(rqb.Def, "Registry")
                            .And("&Id").NotIn(rgqb.Def, "DeferredRegistry")
                            .AndExp("Year").Lt(year).OrExp("Year").Eq(year).And("Month").Le(month);

                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            Console.WriteLine(query.BuildSql());
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestQueryRecordDuplication()
        {
            var noticeDefId = new Guid("{DBFF063F-FB7F-4BDD-94AF-B7DBFD4E4505}"); // Уведомление о переназначении
            var noticeFormId = new Guid("{07C12D5A-50D1-4400-9EFC-94C43A9AFA07}");

            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = UnitTest1.GetProvider(dataContext))
                    {
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var formRepo = provider.Get<IFormRepository>();
                        var sqlReaderBuilder = provider.Get<ISqlQueryReaderFactory>();

                        var form = formRepo.GetTableForm(noticeFormId);

                        using (var query = sqlQueryBuilder.Build(form))
                        {
                            Console.WriteLine(query.BuildSql());
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestAppPersonDuplicationQuery()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = UnitTest1.GetProvider(dataContext))
                    {
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var sqlReaderBuilder = provider.Get<ISqlQueryReaderFactory>();

                        var qb = new QueryBuilder(appDefId);
                        qb.SetAlias("app");
                        qb.Where("&State").In(new object[] {RegisteredStateId, AssignedStateId});
                        var qb2 = new QueryBuilder(appDefId);
                        qb2.Where("&State").In(new object[] {PaidStateTypeId, TerminatedWithReturnStateId});
                        qb.Join(qb2.Def, "dup",
                            c =>
                                c.And("Person", "app")
                                    .Neq("Person", "dup")
                                    .And("Person", "app").Include("Last_Name")
                                    .Eq("Last_Name", "dup")
                                    .And("First_Name").Eq("First_Name").And("Middle_Name").Eq("Middle_Name"));
                        /*using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var personSrc = query.JoinSource(query.Source, PersonDefId, SqlSourceJoinType.Inner, "Person");
                            var dupSrc = query.InnerJoinSource()JoinSource(query.Source, appDefId, SqlSourceJoinType.Inner, )
                            query.AndCondition(personSrc, "Last_Name")
                            Console.WriteLine(query.BuildSql());
                        }*/
                    }
                }
            }
        }

        [TestMethod]
        public void TestJoinWithConditionQuery()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = UnitTest1.GetProvider(dataContext))
                    {
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var sqlReaderBuilder = provider.Get<ISqlQueryReaderFactory>();
                        var docDefRepo = provider.Get<IDocDefRepository>();

                        var qb = new QueryBuilder(TerminationNoteDefId);
                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var solutionSrc = query.JoinSource(query.Source, SolutionDefId, SqlSourceJoinType.Inner,
                                "NoticeOfTermination");
                            var appSrc = query.JoinSource(solutionSrc, appDefId, SqlSourceJoinType.Inner, "Application");
                            var personSrc = query.JoinSource(appSrc, PersonDefId, SqlSourceJoinType.Inner, "Person");
                            var appStateSrc = query.JoinSource(appSrc, AppStateDefId, SqlSourceJoinType.Inner,
                                "Application_State");
                            var districtSrc = query.JoinSource(appStateSrc, DistrictDefId, SqlSourceJoinType.LeftOuter,
                                "DistrictId");
                            var djamoatSrc = query.JoinSource(appStateSrc, DjamoatDefId, SqlSourceJoinType.LeftOuter,
                                "DjamoatId");
                            var villageSrc = query.JoinSource(appStateSrc, VillageDefId, SqlSourceJoinType.LeftOuter,
                                "VillageId");

                            var dicDef = docDefRepo.DocDefById(reasonDicDefId);
                            var alias = query.GetSourceAlias(dicDef.Name);
                            var dicSrc = new SqlQueryDocSource(provider, dicDef, alias) {WithNoLock = true};
                            query.Sources.Add(dicSrc);
                            var dicJoin = new SqlQueryJoin(solutionSrc, dicSrc, SqlSourceJoinType.LeftOuter, Guid.Empty);
                            query.SourceJoins.Add(dicJoin);
                            var cond = dicJoin.AddCondition(ExpressionOperation.And, solutionSrc, "Reason",
                                ConditionOperation.Equal, dicSrc, "Refusal_Reason");
                            /*cond.Left.Attributes[0] = new SqlQuerySourceAttributeRef(solutionSrc,
                                solutionSrc.GetAttribute("Reason"));
                            cond.Right.Attributes.Add(new SqlQuerySourceAttributeRef(dicSrc,
                                dicSrc.GetAttribute("Refusal_Reason")));
                            cond.Right.Params = null;
                            cond.Right.Expression = string.Empty;*/

                            query.AddAttribute(appSrc, "No");
                            query.AddAttribute(appSrc, "Date");
                            query.AddAttribute(personSrc, "Last_Name");
                            query.AddAttribute(personSrc, "First_Name");
                            query.AddAttribute(personSrc, "Middle_Name");
                            query.AddAttribute(districtSrc, "Name").Alias = "DistrictName";
                            query.AddAttribute(djamoatSrc, "Name").Alias = "DjamoatName";
                            query.AddAttribute(villageSrc, "Name").Alias = "VillageName";
                            query.AddAttribute(appStateSrc, "Street");
                            query.AddAttribute(appStateSrc, "House");
                            query.AddAttribute(appStateSrc, "Flat");
                            query.AddAttribute(dicSrc, "Desc").Alias = "Reason";
                            query.AddAttribute(query.Source, "&Id");

                            using (var reader = sqlReaderBuilder.Create(query))
                            {
                                Console.WriteLine(query.BuildSql());
                                reader.Read();
                            }
                        }
                    }
                }
            }
        }

        private static readonly Guid appDefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}");
        private static readonly Guid assignmentDefId = new Guid("{51935CC6-CC48-4DAC-8853-DA8F57C057E8}");
        private static readonly Guid noteBalanceDefId = new Guid("{1EEA4005-0813-4B46-AB56-5AA190E78394}");
        private static readonly Guid noteDepositDefId = new Guid("{05A93EA0-1E7F-489D-B3A7-7024B31C6D50}");
        private static readonly Guid bankAccountTurnoverDefId = new Guid("{982C14E1-3F9C-4559-8835-314C612AB021}");
        private static readonly Guid bankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}");
        private static readonly Guid newStateId = new Guid("{69BDBC88-A1B5-4686-B49A-0D9EB93D7207}");
        private static readonly Guid bankTurnoverReportDefId = new Guid("{BF01AAF9-4838-42C9-8F47-0171DCCD9C3D}");
        private static readonly Guid PaymentDefId = new Guid("{68667FBB-C149-4FB3-93AD-1BBCE3936B6E}");
        private static readonly Guid PersonDefId = new Guid("{6052978A-1ECB-4F96-A16B-93548936AFC0}");

        private static readonly Guid SuccessAccountLoadEnumId = new Guid("{C7A4D0D0-04FC-410D-BA41-073910529CFF}");
        private static readonly Guid OnPaidStateTypeId = new Guid("{78C294B5-B6EA-4075-9EEF-52073A6A2511}");

        private static readonly Guid DistrictNoRegistryDefId = new Guid("{DB434DEC-259F-4563-9213-301D9E38753D}");
        private static readonly Guid PaidStateTypeId = new Guid("{9BCE67C9-DD5D-42BC-9D07-E194CD3A804C}");
        private static readonly Guid PostponedStateTypeId = new Guid("{DB434DEC-259F-4563-9213-301D9E38753D}");
        private static readonly Guid RegisteredStateId = new Guid("0AE798A6-5471-4E2C-999E-7371799F6AD0");
        private static readonly Guid AssignedStateId = new Guid("ACB44CC8-BF44-44F4-8056-723CED22536C");
        private static readonly Guid TerminatedWithReturnStateId = new Guid("0B0C57CD-DFF0-4DBB-BCCA-7128A58D018B");

        private static readonly Guid TerminationNoteDefId = new Guid("{AC364927-7182-4238-B6C9-14BACA526237}");
        private static readonly Guid SolutionDefId = new Guid("{60CE6E64-14C4-4B76-8582-A2077C45400C}");

        private static readonly Guid AppStateDefId = new Guid("{547BBA55-2281-4388-A1FC-EE890168AC2D}");
        private static readonly Guid DistrictDefId = new Guid("{4D029337-C025-442E-8E93-AFD1852073AC}");
        private static readonly Guid DjamoatDefId = new Guid("{967D525D-9B76-44BE-93FA-BD4639EA515A}");
        private static readonly Guid VillageDefId = new Guid("{B70BAD68-7532-471F-A705-364FD5F1BA9E}");
        private static readonly Guid reasonDicDefId = new Guid("{77F7A90C-4695-4E83-8C24-3837AB1EB943}"); // Справочник

        private static readonly Guid printedStateId = new Guid("{CAB49419-651C-444B-8F4C-E58EAC4DC5DA}");//статус распечатан

    }

}
