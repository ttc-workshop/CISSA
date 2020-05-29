using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Tests
{
    // 1. Проверка наличия переоформленных дел
    // 1.1. При наличии переоформления - проверить наличие утвержденных поручений
    // 1.1.1. При наличии утвержденных поручений - вывести список утвержденных поручений для переоформления поручения
    // 1.1.1.1. При выборе пользователем поручения - создать аналогичное поручение с новыми выплатами
    // 1.1.1.2. При утверждении переоформленного поручения, старое поручение переместить в архив (Папка: переоформленные поручения)
    // 1.1.2. При выборе пользователя операции создать новое поручение, либо
    //        при отсутствии утвержденных поручений сделать следующее:
    // 1.1.2.1. Найти и расчитать период действия нового поручения (дата начала и конца)
    // 1.1.2.2. Вывести период на экран для пользователя
    // 1.1.2.3. Получить данные периода поручения и проверить на корректность (не выходит за рамки периода заявления, не превышает срок 2 года)
    // 1.1.2.4. Создать отстутствующие выплаты в заявлении за период поручения
    // 1.1.2.5. Создать выплату 1-го раздела (связать с выплатами заявления)
    // 1.1.2.6. Создать выплаты 2-го раздела

    // Добавить возможность добавления выплат из других поручений по данному заявлению в первый раздел ???

    public static class OrderBuilder2
    {
        // 1.1.1.1. Создание поручения на основе имеющегося (переоформление выплаты)
        public static void RebuildOrder(WorkflowContext context, Guid orgOrderId)
        {
            dynamic app = context.GetDynaDoc(context.Get<Doc>("Application"));
            dynamic order = context.GetDynaDoc(context.CurrentDocument);
            dynamic orgOrder = context.GetDynaDoc(orgOrderId);

            order.No = orgOrder.No;
            order.Date = orgOrder.Date;
            order.DateFrom = orgOrder.DateFrom;
            order.OriginalOrder = orgOrder.Id;
            order.Application = app.Id;
            order.DateFrom1 = orgOrder.DateFrom1;
            order.DateTo1 = orgOrder.DateTo1;
            order.DateFrom2 = orgOrder.DateFrom2;
            order.DateTo2 = orgOrder.DateTo2;

            CreateAppPayments(context, app, order.DateFrom, order.DateTo);
            context["DateFrom"] = order.DateFrom;
            context["DateTo"] = order.DateTo;
            CreateOrderPayments(context, order);
            order.Save();
        }

        private static readonly Guid AppPaymentDefId = new Guid("{FC353D96-2EB9-4742-BC3B-69CA98B67D17}");
        private static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        private static readonly Guid FirstSectionEnumId = new Guid("{2A273790-9091-4DBD-A712-12D46578196C}");
        private static readonly Guid SecondSectionEnumId = new Guid("{0B6C58B1-A6F1-4455-8092-9B8583ADA295}");
        private static readonly Guid OrderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        private static readonly Guid ApprovedStateTypeId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}");

        // Step 2: Создание поручения
        public static void Execute(WorkflowContext context)
        {
            dynamic app = context.GetDynaDoc(context.Get<Doc>("Application"));
            dynamic order = context.GetDynaDoc(context.CurrentDocument);

            if (order.Date == null)
                throw new ApplicationException("Дата формирования поручения не указана!");
            if (order.DateFrom == null)
                throw new ApplicationException("Дата начала периода выплат не указана!");
            if (order.DateTo == null)
                throw new ApplicationException("Дата окончания периода выплат не указана!");

            // Получение дат периода выплат поручения
            var appDateFrom = (DateTime) app.AssignFrom;
            var appIsUnlimited = (app.IsUnlimited != null) && (bool) app.IsUnlimited;
            var appDateTo = app.AssignTo != null ? (DateTime) app.AssignTo : DateTime.MaxValue;
            if (appIsUnlimited) appDateTo = DateTime.MaxValue;
            var orderDate = (DateTime) order.Date;

            var orderDateFrom = (DateTime) order.DateFrom;
            if (orderDateFrom > appDateTo)
                throw new ApplicationException("Выплаты для формирования поручения не найдены!");

            var startOrderDate = new DateTime(orderDateFrom.Year, orderDateFrom.Month, 1);
            if (orderDate > startOrderDate)
            {
                startOrderDate = orderDate;
                if (startOrderDate.Day >= 20) startOrderDate = startOrderDate.AddMonths(1);
            }
            var lastOrderDate = new DateTime(startOrderDate.Year + 1, 12, 31);
            var orderDateTo = (order.DateTo != null) ? (DateTime) order.DateTo : lastOrderDate;
            if (orderDateTo > lastOrderDate) orderDateTo = lastOrderDate;
            if (orderDateTo > appDateTo) orderDateTo = appDateTo;
            if (orderDateFrom >= orderDateTo)
                throw new ApplicationException("Ошибка в периоде выплат поручения!");

            // Получение порядкового номера поручения
            var userInfo = context.GetUserInfo();
            if (userInfo.OrganizationId == null)
                throw new ApplicationException("Организация не указана! Вы не можете создавать документы данного типа.");

            var regNo = GeneratorRepository.GetNewId(context.DataContext, (Guid) userInfo.OrganizationId, app.DocDef.Id).ToString();

            while (regNo.Length < 6) regNo = "0" + regNo;

            order.No = (userInfo.OrganizationCode ?? "???") + regNo;

            // Создать выплаты в заявлении
            CreateAppPayments(context, app, orderDateFrom, orderDateTo);
            context["DateFrom"] = orderDateFrom;
            context["DateTo"] = orderDateTo;
            CreateOrderPayments(context, order);

            order.Save();
        }

        private static void CreateAppPayments(WorkflowContext context, dynamic app,
            DateTime dateFrom, DateTime dateTo)
        {
            var amount = Math.Round((decimal) (app.PaymentSum ?? 0m), 2);

            var dFrom = (DateTime) app.AssignFrom;
            if (dFrom < dateFrom) dFrom = dateFrom;
            var mFrom = dFrom.Month;
            var yFrom = dFrom.Year;
            var m = dateTo.Month;
            var y = dateTo.Year;

            while (((y*12) + (m - 1)) >= ((yFrom*12) + (mFrom - 1)))
            {
                var p = FindAppPaymentFor(context, app.Doc, y, m);
                dynamic payment = p != null ? context.GetDynaDoc(p) : context.NewDynaDoc(AppPaymentDefId);
                // Инициализация выплат
                payment.Month = m;
                payment.Year = y;
                payment.Canceled = false;
                // Инициализация последней выплаты
                if (dateTo.Month == m && dateTo.Year == y)
                {
                    var lastDay = new DateTime(y, m, 1).AddMonths(1).AddDays(-1);
                    payment.Amount = dateTo < lastDay
                        ? Math.Round((decimal) (amount*dateTo.Day)/DateTime.DaysInMonth(y, m), 2)
                        : Math.Round((decimal) amount, 2);
                }
                // Инициализация первой выплаты
                else if (dFrom.Month == m && dFrom.Year == y)
                {
                    if (dFrom.Day > 1)
                    {
                        var lastDay = new DateTime(y, m, 1).AddMonths(1);
                        var k = (decimal) (lastDay - dFrom).TotalDays/DateTime.DaysInMonth(y, m);
                        payment.Amount = Math.Round((decimal) (amount*k), 2);
                    }
                    else
                        payment.Amount = Math.Round((decimal) amount, 2);
                }
                else payment.Amount = Math.Round((decimal) amount, 2);
                payment.Save();

                app.AddDocToList("Payments", payment.Doc);

                if (m <= 1)
                {
                    m = 12;
                    y--;
                }
                else m--;
            }
            //    app.Save(); 
        }

        private static Doc FindAppPaymentFor(WorkflowContext context, Doc app, int year, int month)
        {
            while (app != null)
            {
                var query = new SqlQuery(app, "Payments", context.UserId, "payments", context.Provider);
                query.AddAttribute("&Id");
                query.AndCondition("Year", ConditionOperation.Equal, year);
                query.AndCondition("Month", ConditionOperation.Equal, month);

                using (var reader = new SqlQueryReader(context.DataContext, query))
                {
                    if (reader.Read())
                        return context.Documents.LoadById(reader.GetGuid(0));
                }
                var appId = app["OriginalApplication"];
                if (appId != null)
                    app = context.Documents.LoadById((Guid)appId);
                else
                    return null;
            }
            return null;
        }

        public static void CreateOrderPayments(WorkflowContext context, dynamic order)
        {
            dynamic app = context.GetDynaDoc(order.Application);

            var orderDate = (DateTime)order.Date;
            var payDate = orderDate.Day >= 20 ? orderDate.AddMonths(1) : orderDate;

            var dateFrom1 = (DateTime)context["DateFrom"];
            var dateTo1 = new DateTime(payDate.Year + 1, 12, 31);
            var dateFrom2 = DateTime.MaxValue;
            var dateTo2 = (DateTime)context["DateTo"];
            if (dateTo1 < dateTo2) dateTo2 = dateTo1;
            dateTo1 = DateTime.MinValue;

            var amount1 = 0m;
            var amount2 = 0m; //app.PaymentSum;

            var hasFirstSection = false;
            var hasSecondSection = false;

            var mFrom = dateFrom1.Month;
            var yFrom = dateFrom1.Year;
            var m = dateTo2.Month;
            var y = dateTo2.Year;

            while (((y * 12) + (m - 1)) >= ((yFrom * 12) + (mFrom - 1)))
            {
                var d1 = new DateTime(y, m, 1);
                var d2 = new DateTime(y, m, DateTime.DaysInMonth(y, m));
                var d3 = new DateTime(y, m, 20);

                var p = FindAppPaymentFor(context, app.Doc, y, m);
                if (p == null)
                    throw new ApplicationException(String.Format("Выплата за {0:00}.{1} не найдена!", m, y));

                dynamic payment = context.NewDynaDoc(OrderPaymentDefId);
                payment.Amount = p["Amount"];
                payment.Payment = p.Id;
                if (orderDate > d3 || (mFrom == dateTo2.Month && yFrom == dateTo2.Year))  // I раздел
                {
                    amount1 += Math.Round(payment.Amount ?? 0m, 2);
                    payment.Section = FirstSectionEnumId;
                    payment.Year = payDate.Year;
                    payment.Month = payDate.Month;
                    hasFirstSection = true;
                    if (dateTo1 < d2) dateTo1 = d2;
                }
                else // II раздел
                {
                    amount2 = Math.Round(payment.Amount ?? 0m, 2);
                    payment.Section = SecondSectionEnumId;
                    payment.Year = y;
                    payment.Month = m;
                    hasSecondSection = true;
                    if (dateFrom2 > d1) dateFrom2 = d1;
                }
                order.AddDocToList("OrderPayments", payment.Doc);

                if (m <= 1) { m = 12; y--; } else m--;
            }
            order.Amount1 = amount1;
            order.Amount2 = amount2;
            if (hasFirstSection)
            {
                order.DateFrom1 = dateFrom1;
                order.DateTo1 = dateTo1;
                order.DateFrom = dateFrom1;
            }
            else
                order.DateFrom = dateFrom2;
            if (hasSecondSection)
            {
                order.DateFrom2 = dateFrom2;
                order.DateTo2 = dateTo2;
                order.DateTo = dateTo2;
            }
            else
                order.DateTo = dateTo2;
            order.ExpiryDate = dateTo2;
            context.SuccessFlag = true;
        }

        private static void Exec2(WorkflowContext context)
        {
            var approvedStateTypeId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}");

            dynamic app = context.GetDynaDoc(context.Get<Doc>("Application"));
            dynamic order = context.GetDynaDoc(context.CurrentDocument);

            var appDateFrom = (DateTime)app.AssignFrom;
            var appUnlimited = (app.IsUnlimited != null) && (bool)app.IsUnlimited;
            var appDateTo = app.AssignTo != null ? (DateTime)app.AssignTo : DateTime.MaxValue; // ?? DateTime.MinValue;

            var paymentDate = appDateFrom;

            var qb = new QueryBuilder(order.DocDef.Id);
            qb.Where("&InState").Eq(approvedStateTypeId).And("Application").Eq(app.Id);
            var sqlQueryBuilder = context.Get<ISqlQueryBuilder>();
            var query = sqlQueryBuilder.Build(qb.Def);

            var executor = new SqlQueryExecutor(context.DataContext, query);
            var result = executor.Max("ExpiryDate");
            if (result != null && result != DBNull.Value)
                paymentDate = (DateTime)result;

            var startOrderDate = new DateTime(paymentDate.Year, paymentDate.Month, 1);
            if (startOrderDate.Day > 1)
                startOrderDate = startOrderDate.AddMonths(1);
            var finishOrderDate = new DateTime(startOrderDate.Year + 1, 12, 31);
            if (finishOrderDate > appDateTo) finishOrderDate = appDateTo;

            order.DateFrom = startOrderDate;
            order.DateTo = finishOrderDate;
        }

        public static void SetOriginalOrderQuery(WorkflowContext context, Doc app, dynamic order)
        {
            var orgApps = new List<object>();

            while (app["OriginalApplication"] != null)
            {
                var appId = (Guid) app["OriginalApplication"];
                orgApps.Add(appId);
                app = context.Documents.LoadById(appId);
            }

            if (orgApps.Count > 0)
            {
                var query = new QueryBuilder(order.DocDef.Id);
                query.Where("&State").Eq(ApprovedStateTypeId)
                     .And("Application").In(orgApps.ToArray());

                context.CurrentQuery = query.Def;
            }
        }

        public static DateTime? GetAppUnboundPaymentDate(WorkflowContext context, Doc app)
        {
            var qb = new QueryBuilder(OrderDefId);
            qb.Where("&InState").Eq(ApprovedStateTypeId).And("Application").Eq(app.Id);
            var sqlQueryBuilder = context.Get<ISqlQueryBuilder>();
            var query = sqlQueryBuilder.Build(qb.Def);
            //query.AddAttribute("ExpiryDate", SqlQuerySummaryFunction.Max);
            var executor = new SqlQueryExecutor(context.DataContext, query);
            var result = executor.Max("ExpiryDate");
            if (result != null) return (DateTime) result;
            return null;
        }
    }
}
