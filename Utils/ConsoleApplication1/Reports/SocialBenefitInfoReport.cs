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
    public static class SocialBenefitInfoReport
    {
        // Виды выплат                             
        private static readonly Guid retirementBenefitPaymentId = new Guid("{AFAA7F86-74AE-4260-9933-A56F7845E55A}");  // ЕСП престарелым гражданам
        private static readonly Guid childBenefitPaymentId = new Guid("{AB3F8C41-897A-4574-BAA0-B7CD4AAA1C80}");  // ЕСП на инвалида - члена семьи
        private static readonly Guid invalidBenefitPaymentId = new Guid("{70C28E62-2387-4A59-917D-A366ADE119A8}");  // ЕСП по инвалидности
        private static readonly Guid survivorBenefitPaymentId = new Guid("{839D5712-E75B-4E71-83F7-168CE4F089C0}");  // ЕСП детям при утере кормильца 
        private static readonly Guid aidsFromBenefitPaymentId = new Guid("{3BEBE4F9-0B15-41CB-9B96-54E83819AB0F}");  // ЕСП детям, инфецированным ВИЧ/СПИД
        private static readonly Guid aidsBenefitPaymentId = new Guid("{47EEBFBC-A4E9-495D-A6A1-F87B5C3057C9}");  // ЕСП детям от матерей с ВИЧ/СПИД до 18 месяцев
        private static readonly Guid orphanBenefitPaymentId = new Guid("{4F12C366-7E2F-4208-9CB8-4EAB6E6C0EF1}");   // ЕСП круглым сиротам
        // Виды категорий                             
        private static readonly Guid alpineRetirementCategoryId = new Guid("{587D9992-DBB7-4BAD-A358-0FA571EBDB37}");  // Престарелые жители высокогорья
        private static readonly Guid retirementCategoryId = new Guid("{56DD1E0D-F693-470D-8756-6969DFA71A02}");  // Престарелые, при отсутствии права на пенсионное обеспечение
        private static readonly Guid heroMotherCategoryId = new Guid("{F21D59E6-BBAA-40D8-89FC-C7B3A707E8E6}");  // Награжден. медалью "батыр-эне" - мать-героиня
        private static readonly Guid childDisableCategoryId = new Guid("{FDBDD774-EB88-46EA-9559-005D655BC196}"); // ЛОВЗ до 18
        private static readonly Guid childISPCategoryId = new Guid("{1E750C67-2DDF-488E-A4C4-D94547433067}"); // Дети ДЦП
        private static readonly Guid childhood1CategoryId = new Guid("{D18791C9-DE0A-4E15-92A8-20EF140C51ED}"); // ЛОВЗ детства 1 гр.
        private static readonly Guid childhood2CategoryId = new Guid("{305621EC-9ECC-4AF9-810D-5B639C339D50}"); // ЛОВЗ детства 2 гр.
        private static readonly Guid childhood3CategoryId = new Guid("{FD3B12FB-55D3-4229-975E-342AC126E942}"); // ЛОВЗ детства 3 гр.
        private static readonly Guid childenAidsCategoryId = new Guid("{2222FD98-B885-4DC0-A0D4-271600AF281A}"); // Дети ВИЧ-инфиц. или больные СПИДом
        private static readonly Guid commonDeseas1CategoryId = new Guid("{409BCDA9-6770-4D3F-B515-7DE0E341C63D}"); // ЛОВЗ 1 гр. общего заболевания
        private static readonly Guid commonDeseas2CategoryId = new Guid("{0955ED04-8A32-476B-AE6E-E51DE0F2C66D}"); // ЛОВЗ 2 гр. общего заболевания
        private static readonly Guid commonDeseas3CategoryId = new Guid("{7B622DDA-D6C0-48CA-AFA9-7C74149D8BD5}"); // ЛОВЗ 3 гр. общего заболевания
        private static readonly Guid hearDisabled1CategoryId = new Guid("{C7B397DF-8325-4F0D-A94C-F8BD5E8152E4}"); // ЛОВЗ 1 гр. по слуху
        private static readonly Guid hearDisabled2CategoryId = new Guid("{18A54BA7-0330-40B3-AE9E-67D049004AB0}"); // ЛОВЗ 2 гр. по слуху
        private static readonly Guid hearDisabled3CategoryId = new Guid("{DB4E8C78-A17F-44FD-890D-A67F1EA2B457}"); // ЛОВЗ 3 гр. по слуху
        private static readonly Guid eyesDisabled1CategoryId = new Guid("{5EE97C99-B2A1-4759-9A28-876B08FE7BA8}"); // ЛОВЗ 1 гр. по зрению
        private static readonly Guid eyesDisabled2CategoryId = new Guid("{947E64F0-18EC-4A6B-80C0-FCECFE7C67C2}"); // ЛОВЗ 2 гр. по зрению
        private static readonly Guid eyesDisabled3CategoryId = new Guid("{8B787FCA-62EE-4261-9094-1FFAC1BF4C02}"); // ЛОВЗ 3 гр. по зрению
        // Document Defs Id                       
        private static readonly Guid orderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        private static readonly Guid reportDefId = new Guid("{BC58F3B2-8804-4A26-8018-77CF49321673}");
        private static readonly Guid reportItemDefId = new Guid("{464DA26B-5CE2-4713-A3B9-41831015EB11}");
        // States
        public static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден
        // Report Item Type Id                                          
        private static readonly Guid typeId1 = new Guid("{535E984F-4365-4D10-8D93-1D5DE0071083}");
        private static readonly Guid typeId2 = new Guid("{0FD2B01F-9741-487D-9944-568C5A9E7E5D}");
        private static readonly Guid typeId3 = new Guid("{FD72C53E-60EE-4439-A1AA-94FA829F25EA}");
        private static readonly Guid typeId4 = new Guid("{946E0876-18BD-445C-A4EC-DC302D170E8A}");

        public static Doc Build(int year, int month, Guid userId)
        {
            var userRepo = new UserRepository();
            var userInfo = userRepo.GetUserInfo(userId);

            if (year < 2011 || year > 3000)
                throw new ApplicationException("Ошибка в значении года!");
            if (month < 1 || month > 12)
                throw new ApplicationException("Ошибка в значении месяца!");

            if (userInfo.OrganizationId == null)
                throw new ApplicationException("Не могу создать заявку! Организация не указана!");

            var qb = new QueryBuilder(reportDefId, userId);

            qb.Where("Year").Eq(year).And("Month").Eq(month)
                .And("Organization").Eq(userInfo.OrganizationId);

            var query = new DocQuery(qb.Def);

            var docs = new List<Guid>(query.First(1));

            dynamic report;

            if (docs.Count == 0)
                // Создать отчет                                                                                                         
                report = DynaDoc.CreateNew(reportDefId, userId);
            else
            {
                report = new DynaDoc(docs[0], userId);
                report.ClearDocAttrList("Rows");
            }

            report.Year = year;
            report.Month = month;
            report.Organization = userInfo.OrganizationId;

            var docRepo = new DocRepository(userId);

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
            var item1 = CreateItem1((Guid)userInfo.OrganizationId, year, month, userId);
            item = (item1 != null) ? item1.Doc : docRepo.New(reportItemDefId);
            item["Type"] = typeId1;
            items.Add(item);
            report.AddDocToList("Rows", item);

            CalcItems2(report, userId, (Guid)userInfo.OrganizationId,
                        year, month, items[3], items[2], items[1], items[0]);

            report.Save();
            return report.Doc;
        }

        private static SqlQueryReader CreateReader(Guid orgId, int year, int month, Guid paymentType, Guid category, Guid userId)
        {
            var orders = new QueryBuilder(orderDefId, userId);

            orders.Where("&OrgId").Eq(orgId).And("&State").Eq(ApprovedStateId /*"Утвержден"*/)
                .And("Application").Include("PaymentType").Eq(paymentType)
                    .And("Assignments").Include("Category").Eq(category).End()
                .End()
                .And("OrderPayments").Include("Year").Eq(year).And("Month").Eq(month).End();
            var qOrders = SqlQueryBuilder.Build(orders);
            qOrders.AddAttribute("Assignments", "count(distinct({0}))");

            return new SqlQueryReader(qOrders);
        }

        private static SqlQueryReader CreateReader(Guid orgId, int year, int month, Guid paymentType, Guid userId)
        {
            var orders = new QueryBuilder(orderDefId, userId);

            orders.Where("&OrgId").Eq(orgId).And("&State").Eq(ApprovedStateId /*"Утвержден"*/)
                .And("Application").Include("PaymentType").Eq(paymentType).End()
                .And("OrderPayments").Include("Year").Eq(year).And("Month").Eq(month).End();
            var qOrders = SqlQueryBuilder.Build(orders);
            qOrders.AddAttribute("Assignments", "count(distinct({0}))");

            return new SqlQueryReader(qOrders);
        }

        public static void CalcItems2(dynamic report, Guid userId, Guid orgId,
            int year, int month, Doc item1, Doc item2, Doc item3, Doc item4)
        {
            int appCount = 0;
            int childhood1 = 0;
            int childhood2 = 0;
            int childhood3 = 0;
            int disabledTill18 = 0;
            int ispTill18 = 0;
            int childrenFromAids = 0;
            int childrenAids = 0;
            int commonDeseas1 = 0;
            int commonDeseas2 = 0;
            int commonDeseas3 = 0;
            int survivors = 0;
            int orphans = 0;
            int retirements = 0;
            int alpineRetirements = 0;
            int heroMothers = 0;
            double needAmount = 0;

            var qb = new QueryBuilder(orderDefId, userId);

            qb.Where("&OrgId").Eq(orgId).And("&State").Eq(ApprovedStateId /*"Утвержден"*/)
                .And("Application").Include("PaymentType").In(new object[]
                                                                    {
                                                                        retirementBenefitPaymentId,
                                                                        childBenefitPaymentId, invalidBenefitPaymentId,
                                                                        survivorBenefitPaymentId, orphanBenefitPaymentId,
                                                                        aidsFromBenefitPaymentId, aidsBenefitPaymentId
                                                                    }).End()
                .And("OrderPayments").Include("Year").Eq(year).And("Month").Eq(month).End();

            var query = SqlQueryBuilder.Build(qb);
            query.AddAttribute("Amount", SqlQuerySummaryFunction.Sum);
            using (var reader = new SqlQueryReader(query))
            {
                reader.Open();
                if (reader.Read())
                    needAmount = (double) reader.Reader.GetDecimal(0);
                reader.Close();
            }

            query = SqlQueryBuilder.Build(qb);
            query.AddAttribute("Application", "count(distinct {0})");
            using (var reader = new SqlQueryReader(query))
            {
                reader.Open();
                if (reader.Read())
                    appCount = reader.Reader.GetInt32(0);
                reader.Close();
            }

            using (var reader = CreateReader(orgId, year, month, orphanBenefitPaymentId, userId))
            {
                orphans += ReadReader(reader);
            }
            using (var reader = CreateReader(orgId, year, month, survivorBenefitPaymentId, userId))
            {
                survivors += ReadReader(reader);
            }
            using (var reader = CreateReader(orgId, year, month, aidsFromBenefitPaymentId, userId))
            {
                childrenFromAids += ReadReader(reader);
            }
            using (var reader = CreateReader(orgId, year, month, aidsBenefitPaymentId, userId))
            {
                childrenAids += ReadReader(reader);
            }
            using (var reader = CreateReader(orgId, year, month, retirementBenefitPaymentId, heroMotherCategoryId, userId))
            {
                heroMothers += ReadReader(reader);
            }
            using (var reader = CreateReader(orgId, year, month, retirementBenefitPaymentId, alpineRetirementCategoryId, userId))
                heroMothers += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, retirementBenefitPaymentId, retirementCategoryId, userId))
                retirements += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, retirementBenefitPaymentId, retirementCategoryId, userId))
                retirements += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, childDisableCategoryId, userId))
                disabledTill18 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, childISPCategoryId, userId))
                ispTill18 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, childhood1CategoryId, userId))
                childhood1 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, childhood2CategoryId, userId))
                childhood2 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, childhood3CategoryId, userId))
                childhood3 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, childenAidsCategoryId, userId))
                childrenAids += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, childenAidsCategoryId, userId))
                childrenAids += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, commonDeseas1CategoryId, userId))
                commonDeseas1 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, hearDisabled1CategoryId, userId))
                commonDeseas1 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, eyesDisabled1CategoryId, userId))
                commonDeseas1 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, commonDeseas2CategoryId, userId))
                commonDeseas2 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, hearDisabled2CategoryId, userId))
                commonDeseas2 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, eyesDisabled2CategoryId, userId))
                commonDeseas2 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, commonDeseas3CategoryId, userId))
                commonDeseas3 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, hearDisabled3CategoryId, userId))
                commonDeseas3 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, childBenefitPaymentId, eyesDisabled3CategoryId, userId))
                commonDeseas3 += ReadReader(reader);
            // ----- Invalid Benefit
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, childDisableCategoryId, userId))
                disabledTill18 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, childISPCategoryId, userId))
                ispTill18 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, childhood1CategoryId, userId))
                childhood1 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, childhood2CategoryId, userId))
                childhood2 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, childhood3CategoryId, userId))
                childhood3 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, childenAidsCategoryId, userId))
                childrenAids += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, childenAidsCategoryId, userId))
                childrenAids += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, commonDeseas1CategoryId, userId))
                commonDeseas1 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, hearDisabled1CategoryId, userId))
                commonDeseas1 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, eyesDisabled1CategoryId, userId))
                commonDeseas1 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, commonDeseas2CategoryId, userId))
                commonDeseas2 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, hearDisabled2CategoryId, userId))
                commonDeseas2 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, eyesDisabled2CategoryId, userId))
                commonDeseas2 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, commonDeseas3CategoryId, userId))
                commonDeseas3 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, hearDisabled3CategoryId, userId))
                commonDeseas3 += ReadReader(reader);
            using (var reader = CreateReader(orgId, year, month, invalidBenefitPaymentId, eyesDisabled3CategoryId, userId))
                commonDeseas3 += ReadReader(reader);

            item4["ChildhoodDisabled1"] = childhood1;
            item4["ChildhoodDisabled2"] = childhood2;
            item4["ChildhoodDisabled3"] = childhood3;
            item4["Total1"] = childhood1 + childhood2 + childhood3;

            item4["DisabledTill18"] = disabledTill18;
            item4["ISPTill18"] = ispTill18;
            item4["ChildrenFromAids"] = childrenFromAids;
            item4["ChildrenAids"] = childrenAids;
            item4["Total2"] = disabledTill18 + ispTill18 + childrenFromAids + childrenAids;

            item4["CommonDeseas1"] = commonDeseas1;
            item4["CommonDeseas2"] = commonDeseas2;
            item4["CommonDeseas3"] = commonDeseas3;
            item4["Total3"] = commonDeseas1 + commonDeseas2 + commonDeseas3;

            item4["Survivors"] = survivors;
            item4["Orphans"] = orphans;
            item4["Retirements"] = retirements;
            item4["AlpineRetirements"] = alpineRetirements;
            item4["HeroMother"] = heroMothers;
            item4["AppCount"] = appCount;
            var total = childhood1 + childhood2 + childhood3 +
                    disabledTill18 + ispTill18 + childrenFromAids + childrenAids +
                    commonDeseas1 + commonDeseas2 + commonDeseas3 +
                    survivors + orphans + retirements + alpineRetirements + heroMothers;
            item4["BeneficiarCount"] = total;
            item4["NeedAmount"] = needAmount;
            if (total != 0)
                item4["AvgAmount"] = needAmount / total;

            report.AppCount = appCount;
            report.BeneficiarCount = item4["BeneficiarCount"];
            report.AvgAmount = item4["AvgAmount"];
            report.NeedAmount = item4["NeedAmount"];
            //    report.YearNeedAmount = item4["YearNeedAmount"];
        }

        private static int ReadReader(SqlQueryReader reader)
        {
            var result = 0;
            reader.Open();
            if (reader.Read())
            {
                if (!reader.Reader.IsDBNull(0))
                    result = reader.Reader.GetInt32(0);
            }
            reader.Close();
            return result;
        }

        public static DynaDoc CreateItem1(Guid orgId, int year, int month, Guid userId)
        {
            var m = (month > 1) ? month - 1 : 12;
            var y = (month > 1) ? year : year - 1;

            var qb = new QueryBuilder(reportDefId, userId);

            qb.Where("&OrgId").Eq(orgId)/*.And("&State").Eq("Утвержден")*/
                .And("Year").Eq(y).And("Month").Eq(m);

            var query = SqlQueryBuilder.Build(qb.Def);
            query.AddAttribute("&Id");
            Guid? prevReportId = null;
            using(var reader = new SqlQueryReader(query))
            {
                reader.Open();
                if (reader.Read() && !reader.Reader.IsDBNull(0))
                    prevReportId = reader.Reader.GetGuid(0);
            }
            if (prevReportId == null) return null;

            var prevReport = new DynaDoc((Guid)prevReportId, userId);
            qb = new QueryBuilder(prevReport.Doc, "Rows", userId);
            qb.Where("Type").Eq(typeId4);

            var repItem = new DocQuery(qb.Def);
            var reportItem4Id = repItem.FirstOrDefault();
            if (reportItem4Id == null) return null;

            var reportItem4 = new DynaDoc((Guid)reportItem4Id, userId);
            return reportItem4.Clone();
        }
    }
}
