using System;
using System.IO;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.Cissa.Report.Xls;

namespace ConsoleApplication1.Lists
{
    public static class OrderList
    {
        public static readonly Guid BalykchiOrgId = new Guid("2FEA2DF5-24F6-4E53-BF6C-22604AC014C1");
        public static readonly Guid KarakuldjaOrgId = new Guid("2A0481C0-FB12-4048-9153-CB7AE997A26C");

        public static readonly Guid OrderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        public static readonly Guid ReportDefId = new Guid("{0E05462C-4A4C-4729-972E-5074DB1DED4E}");
        public static readonly Guid ReportItemDefId = new Guid("{3C1A7B35-8300-4E4D-9BD9-F9AC0D4C81D6}");
        public static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        public static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
        public static readonly Guid AccountDefId = new Guid("{81C532F6-F5B0-4EFC-8305-44E864E778D3}");
        public static readonly Guid PersonDefId = new Guid("{6F5B8A06-361E-4559-8A53-9CB480A9B16C}");
        // States
        public static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден

        public static void Build(Guid orgId)
        {
            var qb = new QueryBuilder(OrderDefId);
            qb.Where("&State").Eq(ApprovedStateId).And("&OrgId").Eq(orgId)
                /*.And("OrderPayments").Include("Year").Eq(2012).And("Month").Eq(9)*/;

            var qb2 = new QueryBuilder(OrderDefId);
            qb2.Where("&Id").In(qb.Def, "&Id");

            var query = SqlQueryBuilder.Build(qb/*qb2*/);

            var app = query.JoinSource(query.Source, AppDefId, SqlSourceJoinType.Inner, "Application");
            var account = query.JoinSource(query.Source, AccountDefId, SqlSourceJoinType.LeftOuter, "Account");

            query.AddAttribute(app, "PostCode");
            query.AddAttribute(account, "AccountNo");
            query.AddAttribute("Date");
            query.AddAttribute(app, "RegNo");
            var person = query.JoinSource(app, PersonDefId, SqlSourceJoinType.LeftOuter, "Applicant");
            query.AddAttribute(person, "PIN");
            query.AddAttribute(person, "LastName");
            query.AddAttribute(person, "FirstName");
            query.AddAttribute(app, "PaymentType");
            query.AddAttribute(app, "PaymentCategory");
            query.AddAttributes(new[] { "DateFrom", "DateTo", "DateFrom1", "DateTo1", "DateFrom2", "DateTo2", "Amount1", "Amount2" });
            query.AddOrderAttribute("PostCode");
            query.AddOrderAttribute("AccountNo");

            var enumRepo = new EnumRepository();
            var payments = enumRepo.GetEnumItems(new Guid("{A9C9A563-6BE1-48CB-8C04-462D02B565F8}"));
            var categories = enumRepo.GetEnumItems(new Guid("{9FF88649-11F9-4842-BD05-E0568F552724}"));

            using (var reader = new SqlQueryReader(query))
            {
                using (var def = new XlsDef())
                {
                    // Header
                    def.AddArea().AddRow().AddEmptyCell();
                    def.AddArea().AddRow().AddText("Поручения");
                    def.AddArea().AddRow().AddEmptyCell();

                    // Grid Header
                    var h1 = def.AddArea().AddRow();
                    h1.AddNode("ПО");
                    h1.AddNode("Л/с");
                    h1.AddNode("Дата");
                    h1.AddNode("Базовый №");
                    //                var node1 = h1.AddNode(new XlsText("Заявитель"));
                    h1.AddColumn().AddText("ПИН");
                    h1.AddColumn().AddText("Фамилия");
                    h1.AddColumn().AddText("Имя");
                    h1.AddNode("Выплата");
                    h1.AddNode("Категория");

                    //                var node2 = h1.AddNode(new XlsText("Период поручения"));
                    h1.AddColumn().AddText("С");
                    h1.AddColumn().AddText("По");

                    //                var node3 = h1.AddNode(new XlsText("Первый раздел"));
                    h1.AddColumn().AddText("1-раздел С");
                    h1.AddColumn().AddText("1-раздел По");
                    h1.AddColumn().AddText("Сумма 1-го раздела");

                    //                var node4 = h1.AddNode(new XlsText("Первый раздел"));
                    h1.AddColumn().AddText("2-раздел С");
                    h1.AddColumn().AddText("2-раздел По");
                    h1.AddColumn().AddText("Сумма 2-го раздела");

                    reader.Open();
                    while (reader.Read())
                    {
                        var postCode = !reader.IsDbNull(0) ? reader.GetString(0) : String.Empty;
                        var accountNo = !reader.IsDbNull(1) ? reader.GetString(1) : String.Empty;
                        var regDate = !reader.IsDbNull(2) ? reader.GetDateTime(2).ToShortDateString() : String.Empty;
                        var regNo = !reader.IsDbNull(3) ? reader.GetString(3) : String.Empty;
                        var pin = !reader.IsDbNull(4) ? reader.GetString(4) : String.Empty;
                        var lastName = !reader.IsDbNull(5) ? reader.GetString(5) : String.Empty;
                        var firstName = !reader.IsDbNull(6) ? reader.GetString(6) : String.Empty;
                        var paymentType = !reader.IsDbNull(7) ? reader.GetGuid(7) : Guid.Empty;
                        var category = !reader.IsDbNull(8) ? reader.GetGuid(8) : Guid.Empty;
                        var dateFrom = !reader.IsDbNull(9) ? reader.GetDateTime(9).ToShortDateString() : String.Empty;
                        var dateTo = !reader.IsDbNull(10) ? reader.GetDateTime(10).ToShortDateString() : String.Empty;
                        var dateFrom1 = !reader.IsDbNull(11) ? reader.GetDateTime(11).ToShortDateString() : String.Empty;
                        var dateTo1 = !reader.IsDbNull(12) ? reader.GetDateTime(12).ToShortDateString() : String.Empty;
                        var dateFrom2 = !reader.IsDbNull(13) ? reader.GetDateTime(13).ToShortDateString() : String.Empty;
                        var dateTo2 = !reader.IsDbNull(14) ? reader.GetDateTime(14).ToShortDateString() : String.Empty;
                        var amount1 = !reader.IsDbNull(15) ? reader.GetDecimal(15) : 0m;
                        var amount2 = !reader.IsDbNull(16) ? reader.GetDecimal(16) : 0m;

                        var r = def.AddArea().AddRow();
                        r.AddColumn().AddText(postCode);
                        r.AddColumn().AddText(accountNo);
                        r.AddColumn().AddText(regDate);
                        r.AddColumn().AddText(regNo);
                        r.AddColumn().AddText(pin);
                        r.AddColumn().AddText(lastName);
                        r.AddColumn().AddText(firstName);
                        var enumValue = payments.FirstOrDefault(v => v.Id == paymentType);
                        if (enumValue != null)
                            r.AddColumn().AddText(enumValue.Value);
                        else
                            r.AddEmptyCell();
                        enumValue = categories.FirstOrDefault(v => v.Id == category);
                        if (enumValue != null)
                            r.AddColumn().AddText(enumValue.Value);
                        else
                            r.AddEmptyCell();
                        r.AddColumn().AddText(dateFrom);
                        r.AddColumn().AddText(dateTo);
                        r.AddColumn().AddText(dateFrom1);
                        r.AddColumn().AddText(dateTo1);
                        if (amount1 > 0)
                            r.AddColumn().AddFloat((double) amount1);
                        else
                            r.AddEmptyCell();
                        r.AddColumn().AddText(dateFrom2);
                        r.AddColumn().AddText(dateTo2);
                        r.AddColumn().AddFloat((double) amount2);
                    }
                    reader.Close();
                    var builder = new XlsBuilder(def);
                    var workbook = builder.Build();
                    using (var stream = new FileStream(@"c:\\distr\\cissa\\1MayOrders.xls", FileMode.Create))
                    {
                        workbook.Write(stream);
                    }
                }
            }
        }
    }
}
