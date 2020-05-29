using System;
using System.Collections.Generic;
using System.Diagnostics;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Reports
{
    public static class PoorBenefitReport
    {
        // Виды выплат                             
        private static Guid poorBenefitPaymentId = new Guid("{D24151CF-C8B0-4851-B0EC-6D6EB382DC61}");
        private static Guid twinsBenefitPaymentId = new Guid("{7F1B9709-8F99-473F-9AE0-2DDCD74BDE6E}");
        private static Guid till3BenefitPaymentId = new Guid("{9BC8A898-31F8-4F55-8C14-28F641142370}");
        private static Guid tripletsBenefitPaymentId = new Guid("{64ACC17D-78B8-492E-AC81-7B1E4750F53A}");
        private static Guid underWardBenefitPaymentId = new Guid("{BCE5B287-7495-4AD1-96A8-F52040A4CABF}");
        private static Guid onBirthBenefitPaymentId = new Guid("{43F0ED4A-EFF2-425D-8564-683551BA8F82}");
        // Document Defs Id
        private static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
        private static Guid orderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        private static Guid orderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        private static readonly Guid AssignmentDefId = new Guid("{5D599CE4-76C5-4894-91CC-4EB3560196CE}");
        private static Guid reportDefId = new Guid("{0E05462C-4A4C-4729-972E-5074DB1DED4E}");
        private static Guid reportItemDefId = new Guid("{3C1A7B35-8300-4E4D-9BD9-F9AC0D4C81D6}");
        // Report Item Type Id                                          
        private static Guid typeId1 = new Guid("{535E984F-4365-4D10-8D93-1D5DE0071083}");
        private static Guid typeId2 = new Guid("{0FD2B01F-9741-487D-9944-568C5A9E7E5D}");
        private static Guid typeId3 = new Guid("{FD72C53E-60EE-4439-A1AA-94FA829F25EA}");
        private static Guid typeId4 = new Guid("{946E0876-18BD-445C-A4EC-DC302D170E8A}");
        // EmploymentStatuses
        private static Guid childStatusId = new Guid("{7001020C-4188-492D-90B2-63E8F2DB0A2C}"); // Член семьи до 16 лет
        // States
        public static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден

        public static void Build(IAppServiceProvider provider, IDataContext dataContext, int year, int month, Guid userId)
        {
            var userRepo = provider.Get<IUserRepository>(); //new UserRepository();
            var userInfo = userRepo.GetUserInfo(userId);

            if (year < 2011 || year > 3000)
                throw new ApplicationException("Ошибка в значении года!");
            if (month < 1 || month > 12)
                throw new ApplicationException("Ошибка в значении месяца!");

            if (userInfo.OrganizationId == null)
                throw new ApplicationException("Не могу создать заявку! Организация не указана!");

            var qb = new QueryBuilder(reportDefId, userId);

            qb.Where("Year").Eq(year).And("Month").Eq(month).And("&State").Neq(ApprovedStateId)
                .And("&OrgId").Eq(userInfo.OrganizationId);

            var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
            var query = sqlQueryBuilder.Build(qb.Def);
            query.AddAttribute("&Id");
            Console.WriteLine(query.BuildSql());
            var docs = new List<Guid>();
            using (var reader = new SqlQueryReader(dataContext, query))
            {
                reader.Open();
                if (reader.Read())
                    docs.Add(reader.GetGuid(0));
                reader.Close();
            }

            dynamic report;

            if (docs.Count == 0)
                // Создать заявку на финансирование ГП и ДК                                                                                                          
                report = DynaDoc.CreateNew(reportDefId, userId, provider);
            else
            {
                report = new DynaDoc(docs[0], userId, provider);
                report.ClearDocAttrList("Rows");
            }

            report.Year = year;
            report.Month = month;
            report.Organization = userInfo.OrganizationId;

            //using (var docRepo = new DocRepository(userId))
            var docRepo = provider.Get<IDocRepository>();
            {
                var items = new List<Doc>();
                var item = docRepo.New(reportItemDefId);
                item["Type"] = typeId4;
                items.Add(item);
                report.AddDocToList("Rows", item);
                item = docRepo.New(reportItemDefId);
                item["Type"] = typeId3;
                items.Add(item);
                report.AddDocToList("Rows", item);
                item = docRepo.New(reportItemDefId);
                item["Type"] = typeId2;
                items.Add(item);
                report.AddDocToList("Rows", item);
                var item1 = CreateItem1(provider, dataContext, (Guid) userInfo.OrganizationId, year, month, userId);
                item = (item1 != null) ? item1.Doc : docRepo.New(reportItemDefId);
                item["Type"] = typeId1;
                items.Add(item);
                report.AddDocToList("Rows", item);

                CalcItems(provider, dataContext, report, userId, (Guid) userInfo.OrganizationId,
                          year, month, items[3], items[2], items[1], items[0]);

                report.Save();
            }
        }

        public static void CalcItems(IAppServiceProvider provider, IDataContext dataContext, dynamic report, Guid userId, Guid orgId,
            int year, int month, Doc item1, Doc item2, Doc item3, Doc item4)
        {
//            int childrenTill3 = 0;
//            int twinsTill3 = 0;
//            int tripletsTill3 = 0;
//            int tripletsTill16 = 0;
            int countTill3 = 0;
//            int children3to16 = 0;
//            int students = 0;
//            int underwards = 0;
            int count3to16 = 0;
            double NeedAmountTill3 = 0;
            double NeedAmount3to16 = 0;

            var qb = new QueryBuilder(orderDefId, userId);

            qb.Where("&OrgId").Eq(orgId).And("&State").Eq(ApprovedStateId)
                .And("Application").Include("PaymentType").In(new object[]
                {
                    poorBenefitPaymentId,
                    twinsBenefitPaymentId, till3BenefitPaymentId,
                    tripletsBenefitPaymentId,
                    underWardBenefitPaymentId
                })
                /*            .Or("PaymentType").Eq(onBirthBenefitPaymentId)*/.End()
                .And("OrderPayments").Include("Year").Eq(year).And("Month").Eq(month).End();

            var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
            var query = sqlQueryBuilder.Build(qb.Def);
            query.AddAttributes(new[] {"Application", "PaymentType"});
            query.AddGroupAttributes(new[] { "Application", "PaymentType" });
            query.AddAttribute("Amount", SqlQuerySummaryFunction.Sum);
            //Console.WriteLine(query.BuildSql().ToString());
            using (var reader = new SqlQueryReader(dataContext, query))
            {
                CalcItem(provider, dataContext, userId, orgId, year, month, item4, reader);
/*                reader.Open();
                while (reader.Read())
                {
                    var appId = reader.GetGuid(0);
                    var paymentType = reader.GetGuid(1);

                    double paymentSum = reader.IsDbNull(2) ? 0 : (double) reader.GetDecimal(2);

                    item4["AppCount"] = 1 + (int) (item4["AppCount"] ?? 0);

                    if (paymentType == till3BenefitPaymentId)
                    {
                        var count = GetAssignmentCount(appId, userId);
                        childrenTill3 += count; // + (int)(item4["ChildrenTill3"] ?? 0); 
                        countTill3 += count; // + (int)(item4["CountTill3"] ?? 0); 
                        NeedAmountTill3 += paymentSum; // + (double)(item4["NeedAmountTill3"] ?? 0); 
                    }
                    else if (paymentType == twinsBenefitPaymentId)
                    {
                        var count = GetAssignmentCount(appId, userId);
                        twinsTill3 += count; // + (int)(item4["TwinsTill3"] ?? 0); 
                        countTill3 += count; // + (int)(item4["CountTill3"] ?? 0); 
                        NeedAmountTill3 += paymentSum; // + (double)(item4["NeedAmountTill3"] ?? 0); 
                    }
                    else if (paymentType == tripletsBenefitPaymentId)
                    {
                        var count = GetAssignmentCount(appId, userId);
                        tripletsTill3 += count; // + (int)(item4["TripletsTill3"] ?? 0); 
                        countTill3 += count; // + (int)(item4["CountTill3"] ?? 0); 
                        NeedAmountTill3 += paymentSum; // + (double)(item4["NeedAmountTill3"] ?? 0); 
                    }
                    else if (paymentType == underWardBenefitPaymentId)
                    {
                        var count = GetAssignmentCount(appId, userId);
                        underwards += count;
                        count3to16 += count;
                        NeedAmount3to16 += paymentSum;
                    }
                    else if (paymentType == poorBenefitPaymentId)
                    {
                        var assignments = GetAssignmentEmploymentStatuses(appId, userId);
                        foreach (var employmentStatus in assignments)
                        {
                            if (employmentStatus != null &&
                                employmentStatus == childStatusId)
                                children3to16++;
                            else
                                students++;
                            count3to16++;
                        }
                        NeedAmount3to16 += paymentSum; // + (double)(item4["NeedAmount3to16"] ?? 0); 
                    }
                }
                reader.Close();*/
            }
/*            item4["ChildrenTill3"] = childrenTill3;
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
            else*/ 
            if (item1 != null)
            {
                var val = item1["YearNeedAmountTill3"];
                var prevNeedAmountTill3 = val != null ? Convert.ToDouble(val) : 0.0;
                val = item1["YearNeedAmount3to16"];
                var prevNeedAmount3to16 = val != null ? Convert.ToDouble(val) : 0.0;
                NeedAmountTill3 = (double) (item4["YearNeedAmountTill3"] ?? 0.0);
                NeedAmount3to16 = (double) (item4["YearNeedAmount3to16"] ?? 0.0);
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
            CalcItem2(provider, dataContext, userId, orgId, year, month, item2);
            CalcItem3(provider, dataContext, userId, orgId, year, month, item3);
        }

        public static void CalcItem(IAppServiceProvider provider, IDataContext dataContext, Guid userId, Guid orgId,
            int year, int month, Doc item, SqlQueryReader reader)
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

            reader.Open();
            while (reader.Read())
            {
                var appId = reader.GetGuid(0);
                var paymentType = reader.GetGuid(1);

                double paymentSum = reader.IsDbNull(2) ? 0 : (double)reader.GetDecimal(2);

                item["AppCount"] = 1 + (int) (item["AppCount"] ?? 0);

                if (paymentType == till3BenefitPaymentId)
                {
                    var count = GetAssignmentCount(provider, dataContext, appId, userId);
                    childrenTill3 += count;
                    countTill3 += count; 
                    NeedAmountTill3 += paymentSum;
                }
                else if (paymentType == twinsBenefitPaymentId)
                {
                    var count = GetAssignmentCount(provider, dataContext, appId, userId);
                    twinsTill3 += count;
                    countTill3 += count;
                    NeedAmountTill3 += paymentSum;
                }
                else if (paymentType == tripletsBenefitPaymentId)
                {
                    var count = GetAssignmentCount(provider, dataContext, appId, userId);
                    tripletsTill3 += count;
                    countTill3 += count;
                    NeedAmountTill3 += paymentSum;
                }
                else if (paymentType == underWardBenefitPaymentId)
                {
                    var count = GetAssignmentCount(provider, dataContext, appId, userId);
                    underwards += count;
                    count3to16 += count;
                    NeedAmount3to16 += paymentSum;
                }
                else if (paymentType == poorBenefitPaymentId)
                {
                    var assignments = GetAssignmentEmploymentStatuses(provider, dataContext, appId, userId);
                    foreach (var employmentStatus in assignments)
                    {
                        if (employmentStatus != null &&
                            employmentStatus == childStatusId)
                            children3to16++;
                        else
                            students++;
                        count3to16++;
                    }
                    NeedAmount3to16 += paymentSum; 
                }
            }
            reader.Close();

            item["ChildrenTill3"] = childrenTill3;
            item["TwinsTill3"] = twinsTill3;
            item["TripletsTill3"] = tripletsTill3;
            item["Underwards"] = underwards;
            item["Children3to16"] = children3to16;
            item["Students"] = students;
            item["CountTill3"] = countTill3;
            item["Count3to16"] = count3to16;
            item["BeneficiarCount"] = countTill3 + count3to16;
            item["NeedAmountTill3"] = NeedAmountTill3;
            item["NeedAmount3to16"] = NeedAmount3to16;
            item["NeedAmount"] = NeedAmountTill3 + NeedAmount3to16;
            if (countTill3 > 0)
                item["AvgAmountTill3"] = NeedAmountTill3 / countTill3;
            else
                item["AvgAmountTill3"] = 0;
            if (count3to16 > 0)
                item["AvgAmount3to16"] = NeedAmount3to16 / count3to16;
            else
                item["AvgAmount3to16"] = 0;
            if (countTill3 > 0 || count3to16 > 0)
                item["AvgAmount"] = (NeedAmountTill3 + NeedAmount3to16) / (countTill3 + count3to16);
            else
                item["AvgAmount"] = 0;
        }

        // Вновь назначенные
        private static void CalcItem2(IAppServiceProvider provider, IDataContext dataContext, Guid userId, Guid orgId,
            int year, int month, Doc item2)
        {
            var query = new SqlQuery(provider, orderPaymentDefId, userId);

            var orders = query.JoinSource(query.Source, orderDefId, SqlSourceJoinType.Inner, "OrderPayments");
            var apps = query.JoinSource(orders, AppDefId, SqlSourceJoinType.Inner, "Application");
            query.AddAttribute(orders, "Application");
            query.AddAttribute(apps, "PaymentType");
            query.AddAttribute("Amount", SqlQuerySummaryFunction.Sum);
//          query.AddAttribute(orders, "Application");
            query.AddCondition(ExpressionOperation.And, orderDefId, "&State", ConditionOperation.Equal, ApprovedStateId);
            query.AddCondition(ExpressionOperation.And, AppDefId, "&OrgId", ConditionOperation.Equal, orgId);
            query.AddCondition(ExpressionOperation.And, AppDefId, "PaymentType", ConditionOperation.In,
                               new object[]
                                   {
                                       poorBenefitPaymentId,
                                       twinsBenefitPaymentId, 
                                       till3BenefitPaymentId,
                                       tripletsBenefitPaymentId,
                                       underWardBenefitPaymentId
                                   });
            query.AddGroupAttributes(new[] { "Application", "PaymentType" });
            query.AddHavingCondition(orderPaymentDefId, new[] { "Year", "Month" },
                                     "MIN(cast({0} as Int) * 100 + cast({1} as Int))", ConditionOperation.Equal,
                                     year * 100 + month);

            Debug.Print(query.BuildSql().ToString());
            using (var reader = new SqlQueryReader(dataContext, query))
            {
                CalcItem(provider, dataContext, userId, orgId, year, month, item2, reader);
            }
        }

        // Снятые по сроку
        private static void CalcItem3(IAppServiceProvider provider, IDataContext dataContext, Guid userId, Guid orgId,
            int year, int month, Doc item3)
        {
            var query = new SqlQuery(provider, orderPaymentDefId, userId);

            var orders = query.JoinSource(query.Source, orderDefId, SqlSourceJoinType.Inner, "OrderPayments");
            var apps = query.JoinSource(orders, AppDefId, SqlSourceJoinType.Inner, "Application");
            query.AddAttribute(orders, "Application");
            query.AddAttribute(apps, "PaymentType");
            query.AddAttribute("Amount", SqlQuerySummaryFunction.Sum);
//            query.AddAttribute(orders, "Application");
            query.AddCondition(ExpressionOperation.And, orderDefId, "&State", ConditionOperation.Equal, ApprovedStateId);
            query.AddCondition(ExpressionOperation.And, AppDefId, "&OrgId", ConditionOperation.Equal, orgId);
            query.AddCondition(ExpressionOperation.And, AppDefId, "PaymentType", ConditionOperation.In,
                               new object[]
                                   {
                                       poorBenefitPaymentId,
                                       twinsBenefitPaymentId, 
                                       till3BenefitPaymentId,
                                       tripletsBenefitPaymentId,
                                       underWardBenefitPaymentId
                                   });
            query.AddGroupAttributes(new[] { "Application", "PaymentType" });
            query.AddHavingCondition(orderPaymentDefId, new[] { "Year", "Month" },
                                     "MAX(cast({0} as Int) * 100 + cast({1} as Int))", ConditionOperation.Equal,
                                     year * 100 + (month - 1));

            using (var reader = new SqlQueryReader(dataContext, query))
            {
                CalcItem(provider, dataContext, userId, orgId, year, month, item3, reader);
            }
        }

        private static int GetAssignmentCount(IAppServiceProvider provider, IDataContext dataContext, Guid appId, Guid userId)
        {
            var qb = new QueryBuilder(AppDefId, userId);
            qb.Where("&Id").Eq(appId);
            var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
            var query = sqlQueryBuilder.Build(qb.Def);
            query.AddAttribute("Assignments", SqlQuerySummaryFunction.Count);

            var count = 0;
            using (var reader = new SqlQueryReader(dataContext, query))
            {
                reader.Open();
                if (reader.Read())
                    count = reader.GetInt32(0);
                reader.Close();
            }
            return count;
        }

        private static IEnumerable<Guid?> GetAssignmentEmploymentStatuses(IAppServiceProvider provider, IDataContext dataContext, Guid appId, Guid userId)
        {
            var qb = new QueryBuilder(AppDefId, userId);
            qb.Where("&Id").Eq(appId);
            var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
            var query = sqlQueryBuilder.Build(qb.Def);
            var source = query.JoinSource(query.Source, AssignmentDefId, SqlSourceJoinType.Inner, "Assignments");
            query.AddAttribute(source, "EmploymentStatus");

            var list = new List<Guid?>();
            using (var reader = new SqlQueryReader(dataContext, query))
            {
                reader.Open();
                while (reader.Read())
                    if (!reader.IsDbNull(0))
                        list.Add(reader.GetGuid(0));
                    else 
                        list.Add(null);
                reader.Close();
            }
            return list;
        }

        public static DynaDoc CreateItem1(IAppServiceProvider provider, IDataContext dataContext, Guid orgId, int year, int month, Guid userId)
        {
            var m = (month > 1) ? month - 1 : 12;
            var y = (month > 1) ? year : year - 1;

            var qb = new QueryBuilder(reportDefId, userId);

            qb.Where("&OrgId").Eq(orgId)/*.And("&State").Eq("Утвержден")*/
                .And("Year").Eq(y).And("Month").Eq(m);

            var query = new DocQuery(qb.Def, dataContext);
            var prevReportId = query.FirstOrDefault();
            if (prevReportId == null) return null;

            var prevReport = new DynaDoc((Guid) prevReportId, userId, provider);
            qb = new QueryBuilder(prevReport.Doc, "Rows", userId);
            qb.Where("Type").Eq(typeId4);

            query = new DocQuery(qb.Def, dataContext);
            var reportItem4Id = query.FirstOrDefault();
            if (reportItem4Id == null) return null;

            var reportItem4 = new DynaDoc((Guid)reportItem4Id, userId, provider);
            return reportItem4.Clone();
        }
    }
}
