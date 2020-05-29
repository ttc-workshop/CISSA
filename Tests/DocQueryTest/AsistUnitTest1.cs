using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSourceJoinType = Intersoft.CISSA.DataAccessLayer.Model.Query.Sql.SqlSourceJoinType;

namespace DocQueryTest
{
    [TestClass]
    public class AsistUnitTest1
    {
        public const string DefaultConnectionString =
            "Data Source=195.38.189.100;Initial Catalog=asist_db;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        public static readonly Guid ScheduleDefId = new Guid("{31890BE3-B9C9-49B4-ADA3-4BAAB42D58F0}");
        public static readonly Guid DistrictScheduleDefId = new Guid("{B214C045-95EC-48C2-AD61-72F9ABAB19C8}");
        public static readonly Guid DistrictScheduleItemDefId = new Guid("{B43C8F14-78D5-481E-B613-652E1D96B221}");

        public static readonly Guid AppDefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}");
        public static readonly Guid AppStateDefId = new Guid("{547BBA55-2281-4388-A1FC-EE890168AC2D}");
        public static readonly Guid BankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}");
        public static readonly Guid AssignmentDefId = new Guid("{51935CC6-CC48-4DAC-8853-DA8F57C057E8}");

        public static readonly Guid AssignedStateId = new Guid("{ACB44CC8-BF44-44F4-8056-723CED22536C}");
        public static readonly Guid OnPaymentStateId = new Guid("{78C294B5-B6EA-4075-9EEF-52073A6A2511}");

        [TestMethod]
        public void PaymentScheduleTest()
        {
            using (var connection = new SqlConnection(DefaultConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    var docRepo = new DocRepository(dataContext);
                    var items = new List<SItem>();
                    var year = 2015;

                    var qb = new QueryBuilder(AssignmentDefId);
                    qb.Where("Year").Eq(year); //.And("Application").Include("&State").Eq(OnPaymentStateId);

                    using (var query = SqlQueryBuilder.Build(dataContext, qb.Def))
                    {
                        var appSource = query.JoinSource(query.Source, AppDefId, SqlSourceJoinType.Inner, "Application");
                        var appStateSource =
                            query.JoinSource(appSource, AppStateDefId, SqlSourceJoinType.Inner, "Application_State");
                        // query.AddAttribute(appSource, "&Created", "cast(year({0}) as varchar) + '.' + cast(month({0}) as varchar)");
                        query.AddAttribute(appStateSource, "DistrictId");
                        query.AddAttribute(appSource, "Date", "year({0})");
                        query.AddAttribute(appSource, "Date", "month({0})");
                        query.AddAttribute(query.Source, "Month");
                        query.AddAttribute(query.Source, "No");
                        query.AddAttribute(query.Source, "&Id", "count({0})");

                        query.AndCondition(appSource, "&State", ConditionOperation.Equal, OnPaymentStateId);

                        query.AddGroupAttribute(appStateSource, "DistrictId");
                        query.AddGroupAttribute(appSource, "Date", "year({0})");
                        query.AddGroupAttribute(appSource, "Date", "month({0})");
                        query.AddGroupAttribute(query.Source, "Month");
                        query.AddGroupAttribute(query.Source, "No");
                        // query.AddGroupAttribute(appSource, "&Created", "cast(year({0}) as varchar) + '.' + cast(month({0}) as varchar)");
                        // query.AddGroupAttribute(appSource, "&Created", "month({0})");
                        query.AddOrderAttribute(appStateSource, "DistrictId");
                        query.AddOrderAttribute(appSource, "Date", "year({0})");
                        query.AddOrderAttribute(appSource, "Date", "month({0})");
                        // query.AddOrderAttribute(appSource, "&Created", "cast(year({0}) as varchar) + '.' + cast(month({0}) as varchar)");
                        query.AddOrderAttribute(query.Source, "Month");
                        query.AddOrderAttribute(query.Source, "No");

                        using (var reader = new SqlQueryReader(dataContext, query))
                        {
                            while (reader.Read())
                            {
                                foreach (var fld in reader.Fields)
                                {
                                    Console.Write(@"{0}: {1} ", fld.AttributeName, reader.GetValue(fld.Index));
                                }
                                Console.WriteLine();

                                var districtId = reader.IsDbNull(0) ? Guid.Empty : reader.GetGuid(0);
                                var appYear = reader.GetInt32(1);
                                var appMonth = reader.GetInt32(2);
                                var month = reader.GetInt32(3);
                                var no = reader.IsDbNull(0) ? 0 : reader.GetInt32(4);
                                var count = reader.GetInt32(5);

                                items.Add(new SItem
                                {
                                    DistrictId = districtId,
                                    AppYear = appYear,
                                    AppMonth = appMonth,
                                    Month = month,
                                    No = no,
                                    Count = count
                                });
                            }
                        }
                    }
                    foreach (var dId in items.GroupBy(i => i.DistrictId).Select(i => i.Key))
                    {
                        Console.WriteLine(dId);
                        for (var m = 1; m < 13; m++)
                        {
                            var cs = items.Where(i => i.DistrictId == dId && i.Month == m).Sum(i => i.Count);
                            Console.WriteLine(@"Count_{0}: {1}; ", m, cs);

                        }
                    }
                }
            }
        }

        internal class SItem
        {
            public Guid DistrictId { get; set; }
            public int No { get; set; }
            public int Month { get; set; }
            public int AppYear { get; set; }
            public int AppMonth { get; set; }
            public int Count { get; set; }
        }

        public static readonly Guid DevelopUserId = new Guid("2D6819C9-DB76-43FC-8D9F-EC940539B014");

        [TestMethod]
        public void BuildPaymentScheduleReport()
        {
            using (var connection = new SqlConnection(DefaultConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    var docRepo = new DocRepository(dataContext, DevelopUserId);
                    var items = new List<SItem>();
                    const int year = 2015;
                    var schedule = docRepo.New(ScheduleDefId);
                    schedule["Year"] = year;
                    schedule["Date"] = DateTime.Today;
                    docRepo.Save(schedule);

                    var qb = new QueryBuilder(AssignmentDefId);
                    qb.Where("Year").Eq(year); //.And("Application").Include("&State").Eq(OnPaymentStateId);

                    using (var query = SqlQueryBuilder.Build(dataContext, qb.Def))
                    {
                        var appSource = query.JoinSource(query.Source, AppDefId, SqlSourceJoinType.Inner, "Application");
                        var appStateSource =
                            query.JoinSource(appSource, AppStateDefId, SqlSourceJoinType.Inner, "Application_State");

                        query.AddAttribute(appStateSource, "DistrictId");
                        query.AddAttribute(appSource, "Date", "year({0})");
                        query.AddAttribute(appSource, "Date", "month({0})");
                        query.AddAttribute(query.Source, "Month");
                        query.AddAttribute(query.Source, "No");
                        query.AddAttribute(query.Source, "&Id", "count({0})");

//                        query.AndCondition(appSource, "&State", ConditionOperation.Equal, OnPaymentStateId);

                        query.AddGroupAttribute(appStateSource, "DistrictId");
                        query.AddGroupAttribute(appSource, "Date", "year({0})");
                        query.AddGroupAttribute(appSource, "Date", "month({0})");
                        query.AddGroupAttribute(query.Source, "Month");
                        query.AddGroupAttribute(query.Source, "No");

                        query.AddOrderAttribute(appStateSource, "DistrictId");
                        query.AddOrderAttribute(appSource, "Date", "year({0})");
                        query.AddOrderAttribute(appSource, "Date", "month({0})");
                        query.AddOrderAttribute(query.Source, "Month");
                        query.AddOrderAttribute(query.Source, "No");

                        using (var reader = new SqlQueryReader(dataContext, query))
                        {
                            while (reader.Read())
                            {
                                var districtId = reader.IsDbNull(0) ? Guid.Empty : reader.GetGuid(0);
                                var appYear = reader.GetInt32(1);
                                var appMonth = reader.GetInt32(2);
                                var month = reader.GetInt32(3);
                                var no = reader.IsDbNull(4) ? 0 : reader.GetInt32(4);
                                var count = reader.GetInt32(5);

                                items.Add(new SItem
                                {
                                    DistrictId = districtId,
                                    AppYear = appYear,
                                    AppMonth = appMonth,
                                    Month = month,
                                    No = no,
                                    Count = count
                                });
                            }
                        }
                    }
                    foreach (var dId in items.GroupBy(i => i.DistrictId).Select(i => i.Key))
                    {
                        var ds = docRepo.New(DistrictScheduleDefId);
                        ds["Schedule"] = schedule.Id;
                        ds["District"] = dId;

                        for (var m = 1; m < 13; m++)
                        {
                            var cs = items.Where(i => i.DistrictId == dId && i.Month == m).Sum(i => i.Count);
                            ds["Count_" + m.ToString()] = cs;
                            ds["Amount_" + m.ToString()] = cs*100;
                        }
                        docRepo.Save(ds);

                        var ri = 1;
                        var dItems = items.Where(i => i.DistrictId == dId).ToList();
                        foreach (var appYearMonth in dItems.GroupBy(i => new {i.AppYear, i.AppMonth}).Select(g => g.Key)
                            )
                        {
                            var si = docRepo.New(DistrictScheduleItemDefId);
                            si["Schedule"] = ds.Id;
                            si["No"] = ri;
                            si["Year"] = appYearMonth.AppYear;
                            si["Month"] = appYearMonth.AppMonth;
                            for (var m = 1; m < 13; m++)
                            {
                                var item = dItems.FirstOrDefault(i => i.AppYear == appYearMonth.AppYear &&
                                                                      i.AppMonth == appYearMonth.AppMonth &&
                                                                      i.Month == m);
                                if (item != null)
                                {
                                    si["Count_" + m.ToString()] = item.Count;
                                    si["Pay_No_" + m.ToString()] = item.No;
                                }
                            }
                            ri++;
                            docRepo.Save(si);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlQueryRightAttributeCondition1()
        {
            var assignedStateTypeId = new Guid("{ACB44CC8-BF44-44F4-8056-723CED22536C}");
            var refusedStateTypeId = new Guid("{5D8FF804-E287-41D5-8594-35A333F3CB49}");

            using (var connection = new SqlConnection(DefaultConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    var docRepo = new DocRepository(dataContext, DevelopUserId);

                    var qb = new QueryBuilder(AppDefId);
                    qb.Where("&State").In(new object[] {assignedStateTypeId, refusedStateTypeId});

                    using (var query = SqlQueryBuilder.Build(dataContext, qb.Def))
                    {
                        var personSrc = query.JoinSource(query.Source, PersonDefId, SqlSourceJoinType.Inner, "Person");
//                        var appStateSrc = query.JoinSource(query.Source, AppStateDefId, SqlSourceJoinType.Inner,
//                            "Application_State");

                        query.AddAttribute(personSrc, "IIN");
                        query.AddAttribute(personSrc, "Last_Name");
                        query.AddAttribute(personSrc, "First_Name");
                        query.AddAttribute(personSrc, "Middle_Name");
                        query.AddAttribute(personSrc, "PassportSeries");
                        query.AddAttribute(personSrc, "PassportNo");
                        query.AddAttribute(personSrc, "Date_of_Birth");

                        query.AndCondition(personSrc, "Last_Name", ConditionOperation.Contains, personSrc, "First_Name");

                        using (var r = new SqlQueryReader(dataContext, query))
                        {
                            while (r.Read())
                            {
                                foreach (var fld in r.Fields)
                                {
                                    Console.WriteLine(@"{0}: {1} ", fld.AttributeName, r.GetValue(fld.Index));
                                }
                                Console.WriteLine(@"---------------------");
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestSqlQueryRightAttributeCondition2()
        {
            var assignedStateTypeId = new Guid("{ACB44CC8-BF44-44F4-8056-723CED22536C}");
            var refusedStateTypeId = new Guid("{5D8FF804-E287-41D5-8594-35A333F3CB49}");

            using (var connection = new SqlConnection(DefaultConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    var docRepo = new DocRepository(dataContext, DevelopUserId);

                    var qb = new QueryBuilder(AppDefId);
                    qb.Where("&State").In(new object[] {assignedStateTypeId, refusedStateTypeId});

                    using (var query = SqlQueryBuilder.Build(dataContext, qb.Def))
                    {
                        var personSrc = query.JoinSource(query.Source, PersonDefId, SqlSourceJoinType.Inner, "Person");
                        //                        var appStateSrc = query.JoinSource(query.Source, AppStateDefId, SqlSourceJoinType.Inner,
                        //                            "Application_State");

                        query.AddAttribute(personSrc, "IIN");
                        query.AddAttribute(personSrc, "Last_Name");
                        query.AddAttribute(personSrc, "First_Name");
                        query.AddAttribute(personSrc, "Middle_Name");
                        query.AddAttribute(personSrc, "PassportSeries");
                        query.AddAttribute(personSrc, "PassportNo");
                        query.AddAttribute(personSrc, "Date_of_Birth");

                        query.AndCondition(personSrc, "Date_of_Birth", ConditionOperation.Levenstein, query.Source,
                            "Date");

                        using (var r = new SqlQueryReader(dataContext, query))
                        {
                            Console.WriteLine(r.GetSql());
                            Console.WriteLine(@"-----------------------");

                            while (r.Read())
                            {
                                foreach (var fld in r.Fields)
                                {
                                    Console.WriteLine(@"{0}: {1} ", fld.AttributeName, r.GetValue(fld.Index));
                                }
                                Console.WriteLine(@"---------------------");
                            }
                        }
                    }
                }
            }
        }

        public static readonly Guid PersonDefId = new Guid("{6052978A-1ECB-4F96-A16B-93548936AFC0}");
        public static readonly Guid DistrictDefId = new Guid("{4D029337-C025-442E-8E93-AFD1852073AC}");

        [TestMethod]
        public void TestPaymentRegistryBuilding()
        {
            var appDefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}");
            var bankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}");
            var assignmentDefId = new Guid("{51935CC6-CC48-4DAC-8853-DA8F57C057E8}");
            var paymentDefId = new Guid("{68667FBB-C149-4FB3-93AD-1BBCE3936B6E}");

            var assignedStateId = new Guid("{ACB44CC8-BF44-44F4-8056-723CED22536C}");
            var onPaymentStateId = new Guid("{78C294B5-B6EA-4075-9EEF-52073A6A2511}");

            var year = 2015;
            if (year < 2000) throw new ApplicationException("Ошибка в значении года!");
            var month = 4;
            if (month < 1 || month > 12) throw new ApplicationException("Ошибка в значении месяца!");

            var bqb = new QueryBuilder(bankAccountDefId, DevelopUserId);
            var aqb = new QueryBuilder(assignmentDefId, DevelopUserId);
            var pqb = new QueryBuilder(paymentDefId, DevelopUserId);

            bqb.Where("Account_No").IsNotNull()
                .And("Application").Include("&State").In(new object[] {assignedStateId, onPaymentStateId});
            // Bank accounts
            aqb.AndExp("Year").Lt(year).Or("Year").Eq(year).And("Month").Le(month).End()
                .And("Application").In(bqb.Def, "Application")
                .And("&Id").NotIn(pqb.Def, "Assignment");

            using (var connection = new SqlConnection(DefaultConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var query = SqlQueryBuilder.Build(dataContext, aqb.Def))
                    {
                        query.AddAttribute(query.Source, "&Id");
                        var appSource =
                            query.JoinSource(query.Source, appDefId, SqlSourceJoinType.Inner, "Application");
                        var bankAccountSource =
                            query.JoinSource(appSource, bankAccountDefId, SqlSourceJoinType.Inner, "Application");
                        query.AddAttribute(bankAccountSource, "&Id");

                        using (var reader = new SqlQueryReader(dataContext, query))
                        {
                            reader.Open();
                            Console.WriteLine(reader.GetSql());

                            var count = 0;

                            while (reader.Read())
                            {
                                count++;
                                Console.WriteLine(@"№: {0}", count);
                                Console.WriteLine(@"Assignment: {0}", reader.Reader.GetGuid(0));
                                Console.WriteLine(@"BankAccount: {0}", reader.Reader.GetGuid(1));
                                Console.WriteLine(@"-------------------------");
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void FindAppDuplicationTest()
        {
            var assignedStateTypeId = new Guid("{ACB44CC8-BF44-44F4-8056-723CED22536C}");
            var refusedStateTypeId = new Guid("{5D8FF804-E287-41D5-8594-35A333F3CB49}");
            var registeredStateTypeId = new Guid("{0AE798A6-5471-4E2C-999E-7371799F6AD0}");

            var appDuplicateRegistryDefId = new Guid("{8700295C-5E8A-40D8-B71D-F1CC26FCE64C}");
            var appDuplicateDefId = new Guid("{A59AFED9-5E86-46CB-8FCF-56F9B6DF97EC}");
            var appDuplicateLinkDefId = new Guid("{EB9E1B38-6FF4-4145-8D96-05BC54CD67E2}");
            
            using (var connection = new SqlConnection(DefaultConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    var docRepo = new DocRepository(dataContext, DevelopUserId);

    Doc registry = docRepo.New(appDuplicateRegistryDefId);
    bool registrySaved = false;

    var sqb = new QueryBuilder(AppDefId, DevelopUserId);
    sqb.Where("&State").In(new object[] { registeredStateTypeId, assignedStateTypeId, refusedStateTypeId });

    using (var sQuery = SqlQueryBuilder.Build(dataContext, sqb.Def))
    {
        var personSrc = sQuery.JoinSource(sQuery.Source, PersonDefId, SqlSourceJoinType.Inner, "Person");

        sQuery.AddAttribute(sQuery.Source, "&Id");
        sQuery.AddAttribute(personSrc, "Last_Name");
        sQuery.AddAttribute(personSrc, "First_Name");
        sQuery.AddAttribute(personSrc, "PassportNo");
        sQuery.AddAttribute(personSrc, "Date_of_Birth");

        var qb = new QueryBuilder(AppDefId, DevelopUserId);
        using (var r = new SqlQueryReader(dataContext, sQuery))
        {
            while (r.Read())
            {
                var appId = r.GetGuid(0);

                using (var dQuery = SqlQueryBuilder.Build(dataContext, qb.Def))
                {
                    var dPersonSrc = dQuery.JoinSource(dQuery.Source, PersonDefId, SqlSourceJoinType.Inner, "Person");

                    dQuery.AndCondition(dPersonSrc, "Last_Name", ConditionOperation.Levenstein, r.GetString(1));
                    dQuery.AndCondition(dPersonSrc, "First_Name", ConditionOperation.Levenstein, r.GetString(2));
                    if (!r.IsDbNull(3))
                        dQuery.AndCondition(dPersonSrc, "PassportNo", ConditionOperation.Levenstein, r.GetString(3));
                    if (!r.IsDbNull(4))
                        dQuery.AndCondition(dPersonSrc, "Date_of_Birth", ConditionOperation.Levenstein, r.GetDateTime(4));

                    using (var tr = new SqlQueryReader(dataContext, dQuery))
                    {
                        while (tr.Read())
                        {
                            if (!registrySaved)
                            {
                                docRepo.Save(registry);
                                registrySaved = true;
                            }
                            var appDup = docRepo.New(appDuplicateDefId);
                            appDup["Registry"] = registry.Id;
                            appDup["Application"] = appId;
                            docRepo.Save(appDup);

                            var appDupLink = docRepo.New(appDuplicateLinkDefId);
                            appDupLink["Duplicate"] = appDup.Id;
                            appDupLink["Application"] = tr.GetGuid(0);
                            docRepo.Save(appDupLink);
                        }
                    }
                }
            }
        }
    }
                }
            }
        }
    }
}
