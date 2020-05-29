using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Reports
{
    public static class ChildBirthBenefitReport
    {
        // Виды выплат                             
        private static readonly Guid ChildBirthBenefitPaymentId = new Guid("{43F0ED4A-EFF2-425D-8564-683551BA8F82}");
        // Document Defs Id
        private static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        private static readonly Guid AssignmentDefId = new Guid("{5D599CE4-76C5-4894-91CC-4EB3560196CE}");
        private static readonly Guid ReportDefId = new Guid("{8B480233-09BE-4849-A501-0DB1355BB71F}");
        // MembershipTypes                                                           
        private static readonly Guid TwinsTypeId = new Guid("{45D55628-5E72-42B8-8B8D-667346E79046}");
        private static readonly Guid TripletsTypeId = new Guid("{8CCAE21E-128A-4728-9479-9C094271C614}");
        // States
        public static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден

        public static Doc Build(int year, int month, Guid userId)
        {
            UserInfo userInfo;

            /*using (*/
            var userRepo = new UserRepository();/*)*/
                userInfo = userRepo.GetUserInfo(userId);

            if (year < 2011 || year > 3000)
                throw new ApplicationException("Ошибка в значении года!");
            if (month < 1 || month > 12)
                throw new ApplicationException("Ошибка в значении месяца!");

            if (userInfo.OrganizationId == null)
                throw new ApplicationException("Не могу создать заявку! Организация не указана!");

            var qb = new QueryBuilder(ReportDefId, userId);

            qb.Where("Year").Eq(year).And("Month").Eq(month)//.And("&State").Neq(ApprovedStateId)
                .And("Organization").Eq(userInfo.OrganizationId);

            var query = new DocQuery(qb.Def);

            var docs = new List<Guid>(query.First(1));

            dynamic report;

            if (docs.Count == 0)
                // Создать заявку на финансирование ГП и ДК                                                                                                          
                report = DynaDoc.CreateNew(ReportDefId, userId);
            else
            {
                report = new DynaDoc(docs[0], userId);
//                report.ClearDocAttrList("Rows");
            }

            report.Year = year;
            report.Month = month;
            report.Organization = userInfo.OrganizationId;

            CalcReport(report, userId, (Guid)userInfo.OrganizationId, year, month);

            report.Save();
            return report.Doc;
        }

        public static void CalcReport(dynamic report, Guid userId, Guid orgId,
            int year, int month)
        {
            int appCount = 0;
            int childrenCount = 0;
            int children1 = 0;
            int twins = 0;
            int triplets = 0;
            double needAmount = 0;

            var qb = new QueryBuilder(OrderDefId, userId);

            qb.Where("&OrgId").Eq(orgId).And("&State").Eq(ApprovedStateId) //"Утвержден")
                .And("Application").Include("PaymentType").Eq(ChildBirthBenefitPaymentId).End()
                .And("OrderPayments").Include("Year").Eq(year).And("Month").Eq(month).End();

            var query = SqlQueryBuilder.Build(qb.Def);
            query.AddAttribute("Application");
            query.AddAttribute("Amount", SqlQuerySummaryFunction.Sum);
            query.AddGroupAttribute("Application");

            using(var reader = new SqlQueryReader(query))
            {
                reader.Open();
                while (reader.Read())
                {
                    double paymentSum = !reader.Reader.IsDBNull(1) ? (double) reader.Reader.GetDecimal(1) : 0;

                    appCount++;

                    var assignmentMembershipTypes = GetAssignmentMembershipTypes(reader.Reader.GetGuid(0), userId);
                    foreach (var membershipType in assignmentMembershipTypes)
                    {
                        if (membershipType != null)
                        {
                            if (membershipType == TwinsTypeId)
                                twins++;
                            else if (membershipType == TripletsTypeId)
                                triplets++;
                            else
                                children1++;
                        }
                        else children1++;
                        childrenCount++;
                    }
                    needAmount += paymentSum;
                }
                reader.Close();
            }
            report.MonthAppCount = appCount;
            report.MonthChildrenCount = childrenCount;
            report.MonthTwins = twins;
            report.MonthTriplets = triplets;
            report.Month1Children = children1;
            report.MonthNeedAmount = needAmount;
            if (month == 1)
            {
                report.YearNeedAmount = 0;
                report.TotalAmount = needAmount;
            }
            else
            {
                dynamic prevReport = GetPrevReport(orgId, year, month, userId);

                if (prevReport == null) return;

                appCount = (prevReport.YearAppCount ?? 0) + (prevReport.MonthAppCount ?? 0);
                childrenCount = (prevReport.YearChildrenCount ?? 0) + (prevReport.MonthChildrenCount ?? 0);
                children1 = (prevReport.Year1Children ?? 0) + (prevReport.Month1Children ?? 0);
                twins = (prevReport.YearTwins ?? 0) + (prevReport.MonthTwins ?? 0);
                triplets = (prevReport.YearTriplets ?? 0) + (prevReport.MonthTriplets ?? 0);
                needAmount = (prevReport.TotalAmount ?? 0);

                report.YearAppCount = appCount;
                report.YearChildrenCount = childrenCount;
                report.YearTwins = twins;
                report.YearTriplets = triplets;
                report.Year1Children = children1;
                report.YearNeedAmount = needAmount;

                report.TotalAmount = report.MonthNeedAmount + needAmount;
            }
        }

        private static IEnumerable<Guid?> GetAssignmentMembershipTypes(Guid appId, Guid userId)
        {
            var list = new List<Guid?>();
            var qb = new QueryBuilder(AssignmentDefId, userId);
            qb.Where("&Id").Eq(appId);

            var query = SqlQueryBuilder.Build(qb);
            query.AddAttribute("MembershipType");
            using(var reader = new SqlQueryReader(query))
            {
                reader.Open();
                while(reader.Read())
                {
                    list.Add(reader.Reader.IsDBNull(0) ? (Guid?) null : reader.Reader.GetGuid(0));
                }
                reader.Close();
            }
            return list;
        }

        public static DynaDoc GetPrevReport(Guid orgId, int year, int month, Guid userId)
        {
            var m = (month > 1) ? month - 1 : 12;
            var y = (month > 1) ? year : year - 1;

            var qb = new QueryBuilder(ReportDefId, userId);

            qb.Where("&OrgId").Eq(orgId)/*.And("&State").Eq("Утвержден")*/
                .And("Year").Eq(y).And("Month").Eq(m);

            var query = new DocQuery(qb.Def);
            var prevReportId = query.FirstOrDefault();
            if (prevReportId == null) return null;

            return new DynaDoc((Guid)prevReportId, userId);
        }
    }
}
