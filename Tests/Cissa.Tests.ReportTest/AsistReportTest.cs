using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cissa.Tests.ReportTest
{
    [TestClass]
    public class AsistReportTest
    {
        public const string AsistConnectionString =
            "Data Source=localhost;Initial Catalog=asist_db2;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        [TestMethod]
        public void CommonFinancingData()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        double precent = 0;
                        var items = new List<SItem>();

                        var qb = new QueryBuilder(paymentDefId);
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var districtRegSrc = query.JoinSource(query.Source, DistrictBankPaymentDefId,
                                SqlSourceJoinType.Inner, "Registry");
                            var registrySrc = query.JoinSource(districtRegSrc, RegistryDefId, SqlSourceJoinType.Inner,
                                "BankPaymentRegistry");
                            var bankSrc = query.JoinSource(registrySrc, BankDefId, SqlSourceJoinType.Inner, "Bank");
                            var bankAccountSrc = query.JoinSource(query.Source, BankAccountDefId,
                                SqlSourceJoinType.Inner, "BankAccount");
                            var appSource = query.JoinSource(bankAccountSrc, AppDefId, SqlSourceJoinType.Inner,
                                "Application");
                            var appStateSource = query.JoinSource(appSource, AppStateDefId, SqlSourceJoinType.Inner,
                                "Application_State");
                            var assignMentSrc = query.JoinSource(query.Source, AssignmentDefId, SqlSourceJoinType.Inner,
                                "Assignment");

                            query.AndCondition(registrySrc, "Year", ConditionOperation.Equal, 2015);
                            query.AndCondition(registrySrc, "Month", ConditionOperation.Equal, 10);
                            // query.AndCondition(registrySrc, "Date", ConditionOperation.GreatEqual, fd);
                            // query.AndCondition(registrySrc, "Date", ConditionOperation.LessEqual, ld);
                            query.AndCondition(registrySrc, "&State", ConditionOperation.Equal, RegistryStateId);

                            query.AddAttribute(bankSrc, "PercentServices");
                            query.AddAttribute(appStateSource, "DistrictId");
                            query.AddAttribute(assignMentSrc, "No");
                            query.AddAttribute(assignMentSrc, "Month");
                            query.AddAttribute(assignMentSrc, "Amount");
                            query.AddAttribute(query.Source, "&Id", "count({0})");

                            query.AddGroupAttribute(bankSrc, "PercentServices");
                            query.AddGroupAttribute(appStateSource, "DistrictId");
                            query.AddGroupAttribute(assignMentSrc, "No");
                            query.AddGroupAttribute(assignMentSrc, "Month");
                            query.AddGroupAttribute(assignMentSrc, "Amount");

                            query.AddOrderAttribute(bankSrc, "PercentServices");
                            query.AddOrderAttribute(appStateSource, "DistrictId");
                            query.AddOrderAttribute(assignMentSrc, "No");
                            query.AddOrderAttribute(assignMentSrc, "Month");
                            query.AddOrderAttribute(assignMentSrc, "Amount");

                            var factory = provider.Get<ISqlQueryReaderFactory>();
                            using (var reader = factory.Create(query))
                            {
                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());
                                while (reader.Read())
                                {
                                    precent = reader.GetDouble(0);
                                    var districtId = reader.IsDbNull(1) ? Guid.Empty : reader.GetGuid(1);
                                    var no = reader.IsDbNull(2) ? 0 : reader.GetInt32(2);
                                    var assignMonth = reader.GetInt32(3);
                                    var amount = reader.GetDecimal(4);
                                    var count = reader.GetInt32(5);
                                    items.Add(new SItem
                                    {
                                        DistrictId = districtId,
                                        No = no,
                                        Month = assignMonth,
                                        Amount = amount,
                                        Precent = precent,
                                        Count = count
                                    });
                                }
                            }
                        }
                        decimal percentOfService = Convert.ToDecimal(precent);
                        foreach (var dId in items.GroupBy(i => i.DistrictId).Select(i => i.Key))
                        {
                            var districtNumber = 0;
                            var districtAmount = 0m;
                            decimal percentAmount = 0m;
                            var totalAmount = 0m;
                            for (var q = 1; q < 5; q++)
                            {
                                var quarterSum = items.Where(i => i.DistrictId == dId && i.No == q).Sum(i => i.Count);
                                Console.WriteLine(@"Number_{0} = {1}", q.ToString(), quarterSum);
                                Console.WriteLine(@"Amount_{0} = {1}", q.ToString(), quarterSum*100);
                                districtNumber += quarterSum;
                                districtAmount += quarterSum;
                            }
                            Console.WriteLine(@"Number_All = {0}", districtNumber);
                            Console.WriteLine(@"Amount_All = {0}", districtAmount*100);
                            Console.WriteLine(@"Percent_Services = {0}", districtAmount*100*percentOfService/100);
                            Console.WriteLine(@"Total_Amount = {0}",
                                districtAmount*100 + districtAmount*100*percentOfService/100);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void BuildNewPaymentScheduleTest()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var items = new List<SItem2>();

                        var qb = new QueryBuilder(AssignmentDefId);
                        //qb.Where("Year").Eq(year);

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var sqlQueryReaderFactory = provider.Get<ISqlQueryReaderFactory>();

                        using (var sub = sqlQueryBuilder.Build(qb.Def))
                        {
                            sub.AddAttribute(sub.Source, "Application");
                            //query.AddAttribute(appSource, "Date", "year({0})");
                            //query.AddAttribute(appSource, "Date", "month({0})");
                            sub.AddAttribute(new[]
                                {
                                    new SqlQuerySourceAttributeRef(sub.Source, "Year"),
                                    new SqlQuerySourceAttributeRef(sub.Source, "Month")
                                }, "MIN(cast({0} as Int) * 16 + cast({1} as Int))").Alias = "FirstPeriod";
                            //sub.AddAttribute(sub.Source, "Month");
                            //sub.AddAttribute(sub.Source, "No");

                            sub.AddGroupAttribute(sub.Source, "Application");
                            /*sub.AddGroupAttribute(new[]
                                {
                                    new SqlQuerySourceAttributeRef(sub.Source, "Year"),
                                    new SqlQuerySourceAttributeRef(sub.Source, "Month")
                                }, "MIN(cast({0} as Int) * 16 + cast({1} as Int))");
                            sub.AddGroupAttribute(sub.Source, "Month");
                            sub.AddGroupAttribute(sub.Source, "No");*/

                            Console.WriteLine(sub.BuildSql());

                            using (var query = sqlQueryBuilder.Build(qb.Def))
                            {
                                var appSource = query.JoinSource(query.Source, AppDefId, SqlSourceJoinType.Inner,
                                    "Application");
                                var appStateSource =
                                    query.JoinSource(appSource, AppStateDefId, SqlSourceJoinType.Inner,
                                        "Application_State");
                                var firstAssn = query.JoinSource(appSource, sub, SqlSourceJoinType.Inner, "Application");

                                query.AddAttribute(appStateSource, "DistrictId");
                                query.AddAttribute(firstAssn, "FirstPeriod").Alias = "Period";
                                //query.AddAttribute(appSource, "Date", "year({0})");
                                //query.AddAttribute(appSource, "Date", "month({0})");
                                //query.AddAttribute(query.Source, "Year");
                                query.AddAttribute(query.Source, "Month");
                                query.AddAttribute(query.Source, "No");
                                query.AddAttribute(query.Source, "&Id", "count({0})");
                                query.AddAttribute(query.Source, "Amount", "sum({0})");

                                //query.AndCondition(appSource, "&State", ConditionOperation.Equal, OnPaymentStateId);
                                query.AndCondition(query.Source, "Year", ConditionOperation.Equal, 2016);

                                query.AddGroupAttribute(appStateSource, "DistrictId");
                                query.AddGroupAttribute(firstAssn, "FirstPeriod");
                                //query.AddGroupAttribute(appSource, "Date", "year({0})");
                                //query.AddGroupAttribute(appSource, "Date", "month({0})");
                               // query.AddGroupAttribute(query.Source, "Year");
                                query.AddGroupAttribute(query.Source, "Month");
                                query.AddGroupAttribute(query.Source, "No");

                                query.AddOrderAttribute(appStateSource, "DistrictId");
                                query.AddOrderAttribute(firstAssn, "FirstPeriod");
                                //query.AddOrderAttribute(appSource, "Date", "year({0})");
                                //query.AddOrderAttribute(appSource, "Date", "month({0})");
                                //query.AddOrderAttribute(query.Source, "Year");
                                query.AddOrderAttribute(query.Source, "Month");
                                query.AddOrderAttribute(query.Source, "No");

                                Console.WriteLine(@"===================================================");
                                Console.WriteLine(query.BuildSql());
                                using (var reader = sqlQueryReaderFactory.Create(query))
                                {
                                    while (reader.Read())
                                    {
                                        var districtId = reader.IsDbNull(0) ? Guid.Empty : reader.GetGuid(0);
                                        //var appYear = reader.GetInt32(1);
                                        //var appMonth = reader.GetInt32(2);
                                        var aYear = reader.GetInt32(1);
                                        var month = reader.GetInt32(2);
                                        var no = reader.IsDbNull(3) ? 0 : reader.GetInt32(3);
                                        var count = reader.GetInt32(4);
                                        var amount = reader.IsDbNull(5) ? 0 : reader.GetDecimal(5);

                                        items.Add(new SItem2
                                        {
                                            DistrictId = districtId,
                                            AppYear = aYear,
                                            /*AppMonth = appMonth,*/
                                            Month = month,
                                            No = no,
                                            Count = count
                                        });
                                        Console.WriteLine(
                                            @"District: {0}; AppYear: {1}-{2}; Month: {3}; No: {4}; Count: {5}; Amount: {6}", districtId,
                                            aYear / 16, aYear % 16,
                                            month,
                                            no,
                                            count, amount);
                                    }
                                }
                            }
                            var decisions = new List<SItem2>();
                            qb = new QueryBuilder(TerminationDefId); // Decision Of Termination
                            qb.Where("&State").Eq(CompletedDecisionStateId);
                            using (var query = sqlQueryBuilder.Build(qb.Def))
                            {
                                var appSource = query.JoinSource(query.Source, AppDefId, SqlSourceJoinType.Inner,
                                        "Application");
                                var appStateSource =
                                    query.JoinSource(appSource, AppStateDefId, SqlSourceJoinType.Inner,
                                        "Application_State");
                                sub.AddGroupAttribute("Amount");
                                sub.AddAttribute(sub.Source, "Year", "MAX({0})").Alias = "Max_Year";
                                var firstAssn = query.JoinSource(appSource, sub, SqlSourceJoinType.Inner, "Application");

                                query.AddAttribute(appStateSource, "DistrictId");
                                query.AddAttribute(firstAssn, "FirstPeriod").Alias = "Period";
                                query.AddAttribute(query.Source, "Date", "month({0})");
                                query.AddAttribute(query.Source, "Date", "year({0})");
                                query.AddAttribute(firstAssn, "Amount", "sum({0})").Alias = "Amount";
                                query.AddAttribute(query.Source, "&Id", "count({0})");
                                query.AddAttribute(firstAssn, "Max_Year").Alias = "Max_Year";

                                //query.AndCondition(query.Source, "Date", ConditionOperation.GreatEqual, new DateTime(2016, 1, 1).AddMonths(-9));
                                //query.AndCondition(query.Source, "Date", ConditionOperation.LessThen, new DateTime(2016, 1, 1).AddMonths(12));
                                query.AndCondition(firstAssn, "Max_Year", ConditionOperation.Equal, 2016);

                                query.AddGroupAttribute(appStateSource, "DistrictId");
                                query.AddGroupAttribute(firstAssn, "FirstPeriod");
                                query.AddGroupAttribute(firstAssn, "Max_Year");
                                query.AddGroupAttribute(query.Source, "Date", "month({0})");
                                query.AddGroupAttribute(query.Source, "Date", "year({0})");
                                // query.AddGroupAttribute(firstAssn, "Amount");

                                query.AddOrderAttribute(appStateSource, "DistrictId");
                                query.AddOrderAttribute(firstAssn, "FirstPeriod");
                                query.AddOrderAttribute(firstAssn, "Max_Year");
                                query.AddOrderAttribute(query.Source, "Date", "month({0})");
                                query.AddOrderAttribute(query.Source, "Date", "year({0})");

                                Console.WriteLine(@"Decision of Terminations ****************************");
                                Console.WriteLine(query.BuildSql());
                                using (var reader = sqlQueryReaderFactory.Create(query))
                                {
                                    while (reader.Read())
                                    {
                                        var districtId = reader.IsDbNull(0) ? Guid.Empty : reader.GetGuid(0);
                                        var period = reader.GetInt32(1);
                                        var month = reader.IsDbNull(2) ? 0 : reader.GetInt32(2);
                                        var year = reader.IsDbNull(3) ? 0 : reader.GetInt32(3);
                                        var amount = reader.IsDbNull(4) ? 0 : reader.GetDecimal(4);
                                        var count = reader.GetInt32(5);
                                        var maxYear = reader.GetInt32(6);

                                        if (districtId == Guid.Empty)
                                        {
                                            continue;
                                        }
                                        decisions.Add(new SItem2
                                        {
                                            DistrictId = districtId,
                                            AppYear = period,
                                            Month = month,
                                            //                            No = no,
                                            Count = count
                                        });
                                        Console.WriteLine(
                                            @"District: {0}; AppYear: {1}-{2}; Year: {3}; Month: {4}; Count: {5}; Amount: {6}; Max-Year: {7}", districtId,
                                            period / 16, period % 16,
                                            year, month,
                                            count, amount, maxYear);
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        private static int? GetQuarter(int mon)
        {
            if (new[] {1, 2, 3}.Contains(mon))
                return 1;
            if (new[] {4, 5, 6}.Contains(mon))
                return 2;
            if (new[] {7, 8, 9}.Contains(mon))
                return 3;
            if (new[] {10, 11, 12}.Contains(mon))
                return 4;
            return null;
        }

        private static readonly Guid paymentDefId = new Guid("{68667FBB-C149-4FB3-93AD-1BBCE3936B6E}");
        private static readonly Guid DistrictBankPaymentDefId = new Guid("{ADF1D21A-5FCE-4F42-8889-D0714DDF7967}");
        private static readonly Guid BankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}");
        private static readonly Guid BankDefId = new Guid("{B722BED0-562E-4872-8DD7-ACC31A0C1E12}"); //Банк 
        private static readonly Guid RegistryDefId = new Guid("{B3BB3306-C3B4-4F67-98BF-B015DEEDEFFF}");
        private static readonly Guid AssignmentDefId = new Guid("{51935CC6-CC48-4DAC-8853-DA8F57C057E8}");
        private static readonly Guid AppDefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}");
        private static readonly Guid AppStateDefId = new Guid("{547BBA55-2281-4388-A1FC-EE890168AC2D}");
        private static readonly Guid TerminationDefId = new Guid("{60CE6E64-14C4-4B76-8582-A2077C45400C}");

        private static readonly Guid RegistryStateId = new Guid("{BA7384FE-895E-462F-8BAB-83CB593CDB08}");
        public static readonly Guid AssignedStateId = new Guid("{ACB44CC8-BF44-44F4-8056-723CED22536C}");
        public static readonly Guid OnPaymentStateId = new Guid("{78C294B5-B6EA-4075-9EEF-52073A6A2511}");
        public static readonly Guid TerminatedStateId = new Guid("{9D5EFFDB-7389-4E59-9490-BD57D7D94AB1}");
        public static readonly Guid TerminatedWithReturnStateId = new Guid("{0B0C57CD-DFF0-4DBB-BCCA-7128A58D018B}");
        public static readonly Guid CompletedStateId = new Guid("{62D08FE4-B847-4591-A7C9-E113E0E60BC3}");
        public static readonly Guid DepositedStateId = new Guid("{080E140C-943A-49A3-962A-6E892A58D7BE}");
        public static readonly Guid CompletedDecisionStateId = new Guid("{81D603D0-9A19-49B8-A1D6-16CC645450D2}");
    }

    internal class SItem
    { 
        public Guid DistrictId { get; set; }
        public int No { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
        public double Precent { get; set; }
    }

    internal class SItem2
    {
        public Guid DistrictId { get; set; }
        public int No { get; set; }
        public int Month { get; set; }
        public int AppYear { get; set; }
        public int AppMonth { get; set; }
        public int Count { get; set; }
    }
}
