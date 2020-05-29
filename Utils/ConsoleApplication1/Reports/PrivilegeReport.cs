using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Reports
{
    public static class PrivilegeReport
    {
        // Document Defs Id
        public static readonly Guid ReportDefId = new Guid("{4447EA34-67AB-46F2-BE03-A406CAC4EABC}");
        public static readonly Guid ReportItemDefId = new Guid("{66605D33-A39E-4709-8534-C1505C041182}");

        public static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
        public static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        public static readonly Guid OrderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        public static readonly Guid TariffDefId = new Guid("{0F29B75F-DE90-4910-9524-B74CB0418A57}");
        public static readonly Guid Privilege46PaymentId = new Guid("{7BEFD6DA-042C-4A77-90F3-A4424033E4DD}");
        // States
        public static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден

        public static DynaDoc Build(int year, int month, Guid userId)
        {
            var userRepo = new UserRepository();
            var userInfo = userRepo.GetUserInfo(userId);

            if (year < 2011 || year > 3000)
                throw new ApplicationException("Ошибка в значении года!");
            if (month < 1 || month > 12)
                throw new ApplicationException("Ошибка в значении месяца!");

            if (userInfo.OrganizationId == null)
                throw new ApplicationException("Не могу создать заявку! Организация не указана!");

            var qb = new QueryBuilder(ReportDefId, userId);

            qb.Where("Year").Eq(year).And("Month").Eq(month)
                .And("&State").Neq(ApprovedStateId)
                .And("Organization").Eq(userInfo.OrganizationId);

            var query = new DocQuery(qb.Def);

            var docs = new List<Guid>(query.First(1));

            dynamic bill = docs.Count == 0 ? DynaDoc.CreateNew(ReportDefId, userId) : new DynaDoc(docs[0], userId);

            bill.Year = year;
            bill.Month = month;
            bill.Organization = userInfo.OrganizationId;

            CalcItems(bill, userId, (Guid)userInfo.OrganizationId,
                      year, month);

            bill.Save();
            return bill;
        }

        public static void CalcItems(dynamic bill, Guid userId, Guid orgId,
            int year, int month)
        {
            var docRepo = new DocRepository(userId);

            int totalCount = 0;
            double totalSum = 0;

            var qTariff = new QueryBuilder(TariffDefId);
            qTariff.Where("PaymentType").Eq(Privilege46PaymentId);
            var tariffs = SqlQueryBuilder.Build(qTariff.Def);
            tariffs.AddAttributes(new[] { "&Id", "Category" });

            var tariffCategories = new List<Guid>();

            using (var reader = new SqlQueryReader(tariffs))
            {
                reader.Open();

                while (reader.Read())
                {
                    var category = reader.GetValue(1);
                    if (category == null) continue;
                    var categoryId = (Guid)category;
                    tariffCategories.Add(categoryId);
                }
                reader.Close();
            }

            int i = 1;
            foreach (var categoryId in tariffCategories)
            {
                Console.WriteLine(string.Format("{0}.{1}", i++, categoryId));

                double total = 0;

                var qbApps = new QueryBuilder(AppDefId, userId);
                qbApps.Where("PaymentType").Eq(Privilege46PaymentId).And("PaymentCategory").Eq(categoryId);

                var qb = new QueryBuilder(OrderDefId, userId);

                qb.Where("&OrgId").Eq(orgId).And("&State").Eq(ApprovedStateId)
                    .And("Application").In(qbApps.Def, "&Id")
                    .And("OrderPayments").Include("Year").Eq(year).And("Month").Eq(month).End();

                var query = SqlQueryBuilder.Build(qb.Def);
                query.AddAttribute("Amount", SqlQuerySummaryFunction.Sum);
                using (var reader = new SqlQueryReader(query))
                {
                    reader.Open();
                    if (reader.Read())
                    {
                        if (!reader.Reader.IsDBNull(0))
                        {
                            total = (double) reader.Reader.GetDecimal(0);
                        }
                        Console.WriteLine(@"  Total: " + total.ToString());
                    }
                    reader.Close();
                }

                int count = 0;
                var qbAppCount = new QueryBuilder(AppDefId, userId);

                qbAppCount.Where("&Id").In(qb.Def, "Application");

                query = SqlQueryBuilder.Build(qbAppCount.Def);
                query.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
                using (var reader = new SqlQueryReader(query))
                {
                    reader.Open();
                    if (reader.Read())
                    {
                        count = (int)(reader.GetValue(0) ?? 0);
                        Console.WriteLine(@"  Count: " + count.ToString());
                    }
                    reader.Close();
                }

                if (count != 0 || total != 0)
                {
                    dynamic item = new DynaDoc(docRepo.New(ReportItemDefId), userId);

                    item.Category = categoryId;
                    item.AppCount = count;
                    item.NeedAmount = total;
                    item.Save();
                    bill.AddDocToList("Rows", item.Doc);
                }
                totalCount += count;
                totalSum += (double)total;
            }
            bill.NeedAmount = totalSum;
            bill.AppCount = totalCount;
        }
    }
}
