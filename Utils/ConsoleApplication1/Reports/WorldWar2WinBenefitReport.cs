using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Reports
{
    public static class WorldWar2WinBenefitReport
    {
        // Виды выплат                             
        private static readonly Guid BenefitPaymentId = new Guid("{D450DE1B-ECD9-4286-A46A-59371E437C39}"); // [36] Ежегодная выплата к 9 мая
        // Document Defs Id
        private static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        private static readonly Guid ReportDefId = new Guid("{9B1BD1B7-76B6-4735-9362-3C2C2C89671F}");
        // CategoryTypes                                                           
        private static readonly Guid WarDisabled1CategoryId = new Guid("{32B76FDF-145B-492C-A850-BE903B7AB6CA}");
        private static readonly Guid WarDisabled2CategoryId = new Guid("{626357F4-5133-4426-A712-2E785E556F6E}");
        private static readonly Guid WarDisabled3CategoryId = new Guid("{62960950-7E8F-420F-BCE6-D7F92CA4EA93}");
        private static readonly Guid WarParticipantCategoryId = new Guid("{677A9F5A-79F3-43F1-948E-AE10F6007677}");
        private static readonly Guid WarPartAwardCategoryId = new Guid("{63DFD63A-2868-411F-A52F-039E37B1D39F}");
        private static readonly Guid LeningradCategoryId = new Guid("{145C4F38-9BCF-44B3-95D0-EA1B8D1B869B}");
        private static readonly Guid GettoCategoryId = new Guid("{C8B1465C-F377-4D9B-B23C-04FFC6836597}");
        private static readonly Guid SpvCategoryId = new Guid("{93933D4F-DE39-4C43-839F-6BE09DF1C9C5}");
        private static readonly Guid WorkArmyAwardCategoryId = new Guid("{891911A4-C7C9-4E8D-919B-6AE2B7DE1AB7}");
        private static readonly Guid WorkArmyCategoryId = new Guid("{B0004A56-4831-4CB8-B12B-C39280493A0B}");
        // States
        public static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден

        public static Doc Build(int year, Guid userId)
        {
            var userRepo = new UserRepository();
            var userInfo = userRepo.GetUserInfo(userId);

            if (year < 2011 || year > 3000)
                throw new ApplicationException("Ошибка в значении года!");

            if (userInfo.OrganizationId == null)
                throw new ApplicationException("Не могу создать заявку! Организация не указана!");

            var qb = new QueryBuilder(ReportDefId, userId);

            qb.Where("Year").Eq(year) //.And("Month").Eq(month)
                .And("Organization").Eq(userInfo.OrganizationId);

            var query = new DocQuery(qb.Def);

            var docs = new List<Guid>(query.First(1));

            dynamic report = docs.Count == 0
                                 ? DynaDoc.CreateNew(ReportDefId, userId)
                                 : new DynaDoc(docs[0], userId);

            report.Year = year;
            report.Organization = userInfo.OrganizationId;

            CalcReport(report, userId, (Guid)userInfo.OrganizationId, year);

            report.Save();
            return report.Doc;
        }

        public static void CalcReport(dynamic report, Guid userId, Guid orgId,
            int year)
        {
            int count1 = 0;
            int count2 = 0;
            int count3 = 0;
            int count4 = 0;
            int count5 = 0;
            int count6 = 0;
            int count7 = 0;
            double amount1 = 0;
            double amount2 = 0;
            double amount3 = 0;
            double amount4 = 0;
            double amount5 = 0;
            double amount6 = 0;
            double amount7 = 0;

            var qb = new QueryBuilder(OrderDefId, userId);

            qb.Where("&OrgId").Eq(orgId).And("&State").Eq(ApprovedStateId) //"Утвержден")
                .And("Application").Include("PaymentType").Eq(BenefitPaymentId).End()
                .And("OrderPayments").Include("Year").Eq(year)/*.And("Month").Eq(month)*/.End();

            var query = new DocQuery(qb.Def);

            foreach (var orderId in query.All())
            {
                dynamic order = new DynaDoc(orderId, userId);
                dynamic app = new DynaDoc(order.GetAttrDoc("Application"), userId);

                var assignments = app.GetAttrDocList("Assignments");
                foreach (var assignmnt in assignments)
                {
                    dynamic assignment = new DynaDoc(assignmnt, userId);

                    if (assignment.Category == WarDisabled1CategoryId ||
                        assignment.Category == WarDisabled2CategoryId ||
                        assignment.Category == WarDisabled3CategoryId)
                    {
                        count1++;
                        amount1 += assignment.Amount;
                    }
                    else if (assignment.Category == WarParticipantCategoryId ||
                             assignment.Category == WarPartAwardCategoryId)
                    {
                        count2++;
                        amount2 += assignment.Amount;
                    }
                    else if (assignment.Category == LeningradCategoryId)
                    {
                        count3++;
                        amount3 += assignment.Amount;
                    }
                    else if (assignment.Category == GettoCategoryId)
                    {
                        count4++;
                        amount4 += assignment.Amount;
                    }
                    else if (assignment.Category == SpvCategoryId)
                    {
                        count5++;
                        amount5 += assignment.Amount;
                    }
                    else if (assignment.Category == WorkArmyCategoryId ||
                             assignment.Category == WorkArmyAwardCategoryId)
                    {
                        count6++;
                        amount6 += assignment.Amount;
                    }
                }
            }
            report.Count1 = count1;
            report.Count2 = count2;
            report.Count3 = count3;
            report.Count4 = count4;
            report.Count5 = count5;
            report.Count6 = count6;
            report.Amount1 = amount1;
            report.Amount2 = amount2;
            report.Amount3 = amount3;
            report.Amount4 = amount4;
            report.Amount5 = amount5;
            report.Amount6 = amount6;

            report.TotalCount = count1 + count2 + count3 + count4 + count5 +
                count6 + count7;
            report.TotalAmount = amount1 + amount2 + amount3 + amount4 + amount5 +
                amount6 + amount7;
        }
    }
}
