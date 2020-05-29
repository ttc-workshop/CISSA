using System;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;

namespace ConsoleApplication1.Tests
{
    public static class SqlQueryExecutorTest
    {
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
        private static readonly Guid PoorBenefitPaymentEnumId = new Guid("{D24151CF-C8B0-4851-B0EC-6D6EB382DC61}");
        private static readonly Guid TwinsBenefitPaymentEnumId = new Guid("{7F1B9709-8F99-473F-9AE0-2DDCD74BDE6E}");
        private static readonly Guid Till3BenefitPaymentEnumId = new Guid("{9BC8A898-31F8-4F55-8C14-28F641142370}");
        private static readonly Guid TripletsBenefitPaymentEnumId = new Guid("{64ACC17D-78B8-492E-AC81-7B1E4750F53A}");
        private static readonly Guid UnderWardBenefitPaymentEnumId = new Guid("{BCE5B287-7495-4AD1-96A8-F52040A4CABF}");
        private static readonly Guid OnBirthBenefitPaymentEnumId = new Guid("{43F0ED4A-EFF2-425D-8564-683551BA8F82}");
        // Report Item Type Id                                          
        private static readonly Guid ReportItemTypeId1 = new Guid("{535E984F-4365-4D10-8D93-1D5DE0071083}");
        private static readonly Guid ReportItemTypeId2 = new Guid("{0FD2B01F-9741-487D-9944-568C5A9E7E5D}");
        private static readonly Guid ReportItemTypeId3 = new Guid("{FD72C53E-60EE-4439-A1AA-94FA829F25EA}");
        private static readonly Guid ReportItemTypeId4 = new Guid("{946E0876-18BD-445C-A4EC-DC302D170E8A}");
        // EmploymentStatuses
        private static readonly Guid СhildStatusEnumId = new Guid("{7001020C-4188-492D-90B2-63E8F2DB0A2C}"); // Член семьи до 16 лет

        private static readonly Guid DefaultUserId = new Guid("{180B1E71-6CDA-4887-9F83-941A12D7C979}"); // R
        private static readonly Guid DefaultOrgId = new Guid("{34DDCAF2-EB08-48E7-894A-29C929D62C83}"); // Первомайский район

        private static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден

        public static void Test1()
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

            using (var dataContext = new DataContext())
            {
                var sql = new SqlQueryDefExecutor(qb, dataContext);
                Console.WriteLine(@"Кол-во записей: " + sql.Count());

                var apps = sql.All<Guid>("Application");
                foreach (var appId in apps)
                {
                    Console.WriteLine(appId);
                }
            }
        }

        public static void QueryStateTest2()
        {
            var qb = new QueryBuilder(OrderDefId, DefaultUserId);

            qb.Where("&OrgId").Eq(DefaultOrgId).And("&InState").Eq(ApprovedStateId)
                .And("ExpiryDate").Ge(new DateTime(2012, 8, 1))
                .And("Application").Include("PaymentType").In(new object[]
                                                                  {
                                                                      PoorBenefitPaymentEnumId,
                                                                      TwinsBenefitPaymentEnumId,
                                                                      Till3BenefitPaymentEnumId,
                                                                      TripletsBenefitPaymentEnumId,
                                                                      UnderWardBenefitPaymentEnumId
                                                                  }).End()
                .And("OrderPayments").Include("Year").Eq(2012).And("Month").Eq(7).End();

            using (var query = SqlQueryBuilder.Build(qb))
            {
                var sql = query.BuildSql();
                Console.WriteLine(sql);
            }
            /*var sql = new SqlQueryDefExecutor(qb);
            Console.WriteLine(@"Кол-во записей: " + sql.Count());

            var apps = sql.All<Guid>("Application");
            foreach (var appId in apps)
            {
                Console.WriteLine(appId);
            }*/
        }

        private static readonly Guid grownMSECId = new Guid("{4714AC09-365F-49BA-9D74-333014E4E051}");     //МСЭК//Взрослый МСЭК (документ)
        public static readonly Guid SubscribeId = new Guid("{4664B4D5-1C08-4EED-8956-626065F6F6D3}"); //Подписан 
        private static readonly Guid personDefId = new Guid("{6F5B8A06-361E-4559-8A53-9CB480A9B16C}"); // Person-sheet 
        public static readonly Guid dynamicDefId = new Guid("{6ECA06F7-6DFE-45D6-B14F-578FD7CA42C4}");

        public static void MsecReportTest3()
        {
            var orgId = new Guid("{FF49248C-50F7-47DC-A8A8-95B0B7881578}");
//            var userId = new Guid("");

            var qb = new QueryBuilder(grownMSECId /*, userId*/);
            qb.Where("&OrgId").Eq(orgId).And("&State").Eq(SubscribeId);
                //.In(new object [] {ApprovedStateId, SubscribeId});
            using (var query = SqlQueryBuilder.Build(qb.Def))
            {
                query.JoinSource(query.Source, personDefId, SqlSourceJoinType.Inner, "Applicant");
                var dynamical = query.JoinSource(query.Source, dynamicDefId, SqlSourceJoinType.LeftOuter,
                    "DinamikaInvalidnosty");
                query.AddAttributes(new[]
                {
                    "DateOfExamenation", "DisabilityGroup", "Examination", "Sex", "Vozrast",
                    "CauseOfDisability", "Objective18-,3", "ValidityOfTheDirection",
                    "GoalOfExaminationGrown", "Examination4", "Objective-18,2", "Objective10", "01",
                    "Objective12-18,8", "IndividualRehabilitationProgram", "limitation", "03", "04"
                });
                query.AddAttribute(dynamical, "DisabilityGroup");
                using (var reader = new SqlQueryReader(query))
                {
                    Console.WriteLine(reader.GetCount());
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetValue(0));
                    }
                }
            }
        }
    }
}
