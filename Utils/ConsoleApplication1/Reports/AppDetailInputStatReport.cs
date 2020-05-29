using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;

namespace ConsoleApplication1.Reports
{
    public static class AppDetailInputStatReport
    {
        public static readonly Guid OrderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        public static readonly Guid ReportDefId = new Guid("{0E05462C-4A4C-4729-972E-5074DB1DED4E}");
        public static readonly Guid ReportItemDefId = new Guid("{3C1A7B35-8300-4E4D-9BD9-F9AC0D4C81D6}");
        public static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        public static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");

        public static readonly Guid Compensation46PaymentId = new Guid("{7BEFD6DA-042C-4A77-90F3-A4424033E4DD}");
// Виды ЕСП выплат                             
        private static readonly Guid RetirementBenefitPaymentId = new Guid("{AFAA7F86-74AE-4260-9933-A56F7845E55A}");
            // ЕСП престарелым гражданам

        private static readonly Guid ChildBenefitPaymentId = new Guid("{AB3F8C41-897A-4574-BAA0-B7CD4AAA1C80}");
            // ЕСП на инвалида - члена семьи

        private static readonly Guid InvalidBenefitPaymentId = new Guid("{70C28E62-2387-4A59-917D-A366ADE119A8}");
            // ЕСП по инвалидности

        private static readonly Guid SurvivorBenefitPaymentId = new Guid("{839D5712-E75B-4E71-83F7-168CE4F089C0}");
            // ЕСП детям при утере кормильца 

        private static readonly Guid AidsFromBenefitPaymentId = new Guid("{3BEBE4F9-0B15-41CB-9B96-54E83819AB0F}");
            // ЕСП детям, инфецированным ВИЧ/СПИД

        private static readonly Guid AidsBenefitPaymentId = new Guid("{47EEBFBC-A4E9-495D-A6A1-F87B5C3057C9}");
            // ЕСП детям от матерей с ВИЧ/СПИД до 18 месяцев

        private static readonly Guid OrphanBenefitPaymentId = new Guid("{4F12C366-7E2F-4208-9CB8-4EAB6E6C0EF1}");
            // ЕСП круглым сиротам

// Виды ЕПМС выплат                             
        private static readonly Guid PoorBenefitPaymentId = new Guid("{D24151CF-C8B0-4851-B0EC-6D6EB382DC61}");
        private static readonly Guid TwinsBenefitPaymentId = new Guid("{7F1B9709-8F99-473F-9AE0-2DDCD74BDE6E}");
        private static readonly Guid Till3BenefitPaymentId = new Guid("{9BC8A898-31F8-4F55-8C14-28F641142370}");
        private static readonly Guid TripletsBenefitPaymentId = new Guid("{64ACC17D-78B8-492E-AC81-7B1E4750F53A}");
        private static readonly Guid UnderWardBenefitPaymentId = new Guid("{BCE5B287-7495-4AD1-96A8-F52040A4CABF}");
        private static readonly Guid BirthBenefitPaymentId = new Guid("{43F0ED4A-EFF2-425D-8564-683551BA8F82}");

        private static readonly Guid[] SocialBenefitPayments = {
            RetirementBenefitPaymentId,
            ChildBenefitPaymentId,
            InvalidBenefitPaymentId,
            SurvivorBenefitPaymentId,
            AidsFromBenefitPaymentId,
            AidsBenefitPaymentId,
            OrphanBenefitPaymentId
        };

        private static readonly Guid[] PoorBenefitPayments = {
            PoorBenefitPaymentId,
            TwinsBenefitPaymentId,
            Till3BenefitPaymentId,
            TripletsBenefitPaymentId,
            UnderWardBenefitPaymentId,
            BirthBenefitPaymentId
        };

        public static void Build(IAppServiceProvider provider, IDataContext dataContext, int year, int month, Guid userId)
        {
            var query = new SqlQuery(provider, AppDefId, userId);

/*
    query.Joins(b =>
                    {
                        b.LeftOuterJoin("OldApplication");
                        b.Join("Applicant");
                    });
*/

            query.AddAttribute("&OrgId");
            query.AddAttribute("&OrgName");
            query.AddAttribute("PaymentType");
            query.AddAttribute("&Created", SqlQuerySummaryFunction.Max);
            query.AddAttribute("&Id", SqlQuerySummaryFunction.Count);
            query.AddGroupAttributes(new[] {"&OrgId", "&OrgName", "PaymentType"});
            query.AddOrderAttribute("&OrgName");

            var firstDate = new DateTime(year, month, 1);
            var lastDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));

            query.AddCondition(ExpressionOperation.And, AppDefId, "&Created", ConditionOperation.GreatEqual, firstDate);
            query.AddCondition(ExpressionOperation.And, AppDefId, "&Created", ConditionOperation.LessEqual, lastDate);

            var items = new List<ReportItem>();

            using (var reader = new SqlQueryReader(dataContext, query))
            {
                reader.Open();
                while (reader.Read())
                {
                    var orgId = !reader.IsDbNull(0) ? reader.GetGuid(0) : Guid.Empty;
                    var orgName = !reader.IsDbNull(1) ? reader.GetString(1) : String.Empty;
                    var paymentType = !reader.IsDbNull(2) ? reader.GetGuid(2) : Guid.Empty;
                    var created = !reader.IsDbNull(3) ? reader.GetDateTime(3) : DateTime.MinValue;
                    var count = !reader.IsDbNull(4) ? reader.GetInt32(4) : 0;
                    Console.WriteLine(orgName + @"; " + paymentType + @"; " + count);
                    if (orgId != Guid.Empty)
                    {
                        var item = GetItem(items, orgId, orgName);

                        if (paymentType == Compensation46PaymentId)
                            item.CompensationBenefits = item.CompensationBenefits + count;
                        else if (SocialBenefitPayments.Contains(paymentType))
                            item.SocialBenefits = item.SocialBenefits + count;
                        else if (PoorBenefitPayments.Contains(paymentType))
                            item.PoorBenefits = item.PoorBenefits + count;
                        else
                            item.OtherBenefits = item.OtherBenefits + count;
                    }
                }
                reader.Close();
            }
            using (var def = new XlsDef())
            {
                // Header
                def.AddArea().AddRow().AddText(String.Format("Количество введенных дел за {0}.{1}", month, year));
                def.AddArea().AddRow().AddEmptyCell();

                // Grid Header
                var h1 = def.AddArea().AddRow();
                h1.AddNode("№");
                h1.AddNode("Наименование УСР");
                h1.AddNode("Кол-во ЕПМС");
                h1.AddNode("Кол-во ЕСП");
                h1.AddNode("Кол-во ДК");
                h1.AddNode("Кол-во других");
                h1.ShowAllBorders(true);

                var i = 1;
                foreach (var item in items)
                {
                    var r = def.AddArea().AddRow();
                    r.ShowAllBorders(true);
                    r.AddColumn().AddInt(i);
                    r.AddColumn().AddText(item.OrgName);
                    r.AddColumn().AddInt(item.PoorBenefits);
                    r.AddColumn().AddInt(item.SocialBenefits);
                    r.AddColumn().AddInt(item.CompensationBenefits);
                    r.AddColumn().AddInt(item.OtherBenefits);
                    i++;
                }
                var builder = new XlsBuilder(def);
                var workbook = builder.Build();
                using (var stream = new FileStream(@"c:\DetailAppInputStatReport.xls", FileMode.Create))
                {
                    workbook.Write(stream);
                }
            }
        }

        private static ReportItem GetItem(List<ReportItem> items, Guid orgId, string orgName)
        {
            foreach (var item in items)
            {
                if (item.OrgId == orgId) return item;
            }
            var newItem = new ReportItem {OrgId = orgId, OrgName = orgName};
            items.Add(newItem);
            return newItem;
        }

        public class ReportItem
        {
            public Guid OrgId;
            public string OrgName;
            public int PoorBenefits;
            public int SocialBenefits;
            public int CompensationBenefits;
            public int OtherBenefits;
        }

    }
}
