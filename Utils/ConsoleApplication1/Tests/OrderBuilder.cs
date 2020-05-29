using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
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

    public class OrderBuilder
    {
        private static readonly Guid ApprovedStateId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден

        // Step 1: Save Applicant Account
        public void SaveAccountDoc(WorkflowContext context)
        {
            context["AccountId"] = context.ReturnedContext.SelectedDocumentId;

            dynamic app = context.GetDynaDoc(context.Get<Doc>("Application"));

            dynamic order = context.GetDynaDoc(context.CurrentDocument);

            order.Application = app.Id;
            order.Date = DateTime.Now;
            // ++ Получение ссылки на переоформленное заявление
            var originalOrders = new List<Doc>();
            if (app.OriginalApplication != null)
            {
                var originalApp = app.Get<Guid>("OriginalApplication");
                // ++ Получение поручений связанных с переоформленным заявлением
                var query = new SqlQuery(order.DocDef);
                query.AddAttribute("&Id");
                query.AddCondition(ExpressionOperation.And, query.Source.GetDocDef(), "Application", ConditionOperation.Equal,
                                   originalApp);
                query.AddCondition(ExpressionOperation.And, query.Source.GetDocDef(), "&State", ConditionOperation.Equal,
                                   ApprovedStateId);
                query.AddOrderAttribute("&DateFrom", false);
                
                using(var reader = new SqlQueryReader(query))
                {
                    while (reader.Read())
                    {
                        var orderId = reader.GetGuid(0);
                        originalOrders.Add(context.Documents.LoadById(orderId));
                    }
                }
            }

            order.Account = context.ReturnedContext.SelectedDocumentId;
            // Получение периода назначения
            var dateFrom = (DateTime)app.AssignFrom;
            var unlimited = (app.IsUnlimited != null) ? (bool)app.IsUnlimited : false;
            var dateTo = app.AssignTo != null ? (DateTime)app.AssignTo : DateTime.MaxValue; // ?? DateTime.MinValue;

            // Если конец периода поручения больше даты начала выплат заявления, то взять период старого поручения!
            // Иначе оставить период по заявлению
            if (originalOrders.Count > 0)
            {
                dynamic orgOrder = context.GetDynaDoc(originalOrders[0]);

                var orgDateFrom = (DateTime?) orgOrder.DateFrom ?? DateTime.MinValue;
                var orgDateTo = (DateTime?) orgOrder.DateTo ?? DateTime.MinValue;

                if (orgDateTo > dateFrom)
                {
                    dateFrom = orgDateFrom;
                    dateTo = orgDateTo;
                }
            }

            var paymentDate = dateFrom;
            // Получение выплат
            var payments = Enumerable.ToList(app.GetAttrDocList("Payments"));

            // Получение последнего свободного периода выплат
            foreach (var paym in payments)
            {
                dynamic payment = context.GetDynaDoc(paym);
                var canceled = payment.Canceled != null ? (bool)payment.Canceled : false;
                if (!canceled)
                {
                    var d2 = new DateTime(payment.Year, payment.Month, 1).AddMonths(1);
                    if (d2 > paymentDate) paymentDate = d2;
                }
            }

            // Получение даты начала и конца выплат
            var startOrderDate = new DateTime(paymentDate.Year, paymentDate.Month, 1);
            if (startOrderDate.Day > 1)
                startOrderDate = startOrderDate.AddMonths(1);
            var paymentDateTo = new DateTime(startOrderDate.Year + 1, 12, 31);
            if (paymentDateTo > dateTo) paymentDateTo = dateTo;

            order.DateFrom = paymentDate;
            order.DateTo = paymentDateTo;
        }

        // Step 2: Order initialization
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
            var appDateFrom = (DateTime)app.AssignFrom;
            var appIsUnlimited = (app.IsUnlimited != null) ? (bool)app.IsUnlimited : false;
            var appDateTo = app.AssignTo != null ? (DateTime)app.AssignTo : DateTime.MaxValue;
            var orderDate = (DateTime)order.Date;

            var orderDateFrom = (DateTime)order.DateFrom;
            if (orderDateFrom > appDateTo)
                throw new ApplicationException("Выплаты для формирования поручения не найдены!");
            var startOrderDate = new DateTime(orderDateFrom.Year, orderDateFrom.Month, 1);
            if (orderDate > startOrderDate)
            {
                startOrderDate = orderDate;
                if (startOrderDate.Day >= 20) startOrderDate.AddMonths(1);
            }
            var lastOrderDate = new DateTime(startOrderDate.Year + 1, 12, 31);
            var orderDateTo = (order.DateTo != null) ? (DateTime)order.DateTo : lastOrderDate;
            if (orderDateTo > lastOrderDate) orderDateTo = lastOrderDate;
            if (orderDateTo > appDateTo) orderDateTo = appDateTo;
            if (orderDateFrom >= orderDateTo)
                throw new ApplicationException("Ошибка в периоде выплат поручения!");
            // Получение порядкового номера поручения
            var userInfo = context.GetUserInfo();
            if (userInfo.OrganizationId == null)
                throw new ApplicationException("Организация не указана! Вы не можете создавать документы данного типа.");

            var regNo = GeneratorRepository.GetNewId((Guid)userInfo.OrganizationId, app.DocDef.Id).ToString();

            while (regNo.Length < 6) regNo = "0" + regNo;

            order.No = (userInfo.OrganizationCode ?? "???") + regNo;

            // Выбрать выплаты с предыдущего заявления при переоформлении/
            var payments = Enumerable.ToList(app.GetAttrDocList("Payments"));
            CreateAppPayments(context, app, payments, orderDateFrom, orderDateTo);
            context["DateFrom"] = orderDateFrom;
            context["DateTo"] = orderDateTo;
        }

        private static readonly Guid AppPaymentDefId = new Guid("{FC353D96-2EB9-4742-BC3B-69CA98B67D17}");
        /// <summary>
        ///  Создание выплат в заявлении по периоду поручения
        /// </summary>
        /// <param name="context"></param>
        /// <param name="app"></param>
        /// <param name="payments"></param>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        private static void CreateAppPayments(WorkflowContext context, dynamic app,
            List<Doc> payments, DateTime dateFrom, DateTime dateTo)
        {
            var amount = (decimal)(app.PaymentSum ?? 0m);
            
            var dFrom = (DateTime) app.AssignFrom;
            if (dFrom < dateFrom) dFrom = dateFrom;
            var mFrom = dFrom.Month;
            var yFrom = dFrom.Year;
            var m = dateTo.Month;
            var y = dateTo.Year;

            while (((y * 12) + (m - 1)) >= ((yFrom * 12) + (mFrom - 1)))
            {
                dynamic payment;
                var p = FindPaymentFor(payments, y, m);
                if (p != null)
                    payment = context.GetDynaDoc(p);
                else
                    payment = context.NewDynaDoc(AppPaymentDefId);
                // Инициализация выплат
                payment.Month = m;
                payment.Year = y;
                payment.Canceled = false;
                // Инициализация последней выплаты
                if (dateTo.Month == m && dateTo.Year == y)
                {
                    var lastDay = new DateTime(y, m, 1).AddMonths(1).AddDays(-1);
                    if (dateTo < lastDay)
                    {
                        payment.Amount = (decimal)(amount * dateTo.Day) / DateTime.DaysInMonth(y, m);
                    }
                    else
                        payment.Amount = (decimal)amount;
                }
                // Инициализация первой выплаты
                else if (dFrom.Month == m && dFrom.Year == y)
                {
                    if (dFrom.Day > 1)
                    {
                        var lastDay = new DateTime(y, m, 1).AddMonths(1);
                        var k = (decimal)(lastDay - dFrom).TotalDays / DateTime.DaysInMonth(y, m);
                        payment.Amount = (decimal)(amount * k);
                    }
                    else
                        payment.Amount = (decimal)amount;
                }
                else payment.Amount = (decimal)amount;
                payment.Save();

                app.AddDocToList("Payments", payment.Doc);

                if (m <= 1) { m = 12; y--; } else m--;
            }
            //    app.Save(); 
        }

        private static void CreateAppPayments(WorkflowContext context, dynamic app,
            DateTime dateFrom, DateTime dateTo)
        {
            var amount = (decimal)(app.PaymentSum ?? 0m);

            var dFrom = (DateTime)app.AssignFrom;
            if (dFrom < dateFrom) dFrom = dateFrom;
            var mFrom = dFrom.Month;
            var yFrom = dFrom.Year;
            var m = dateTo.Month;
            var y = dateTo.Year;

            while (((y * 12) + (m - 1)) >= ((yFrom * 12) + (mFrom - 1)))
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
                        payment.Amount = Math.Round((decimal)amount, 2);
                }
                else payment.Amount = Math.Round((decimal)amount, 2);
                payment.Save();

                app.AddDocToList("Payments", payment.Doc);

                if (m <= 1) { m = 12; y--; } else m--;
            }
            //    app.Save(); 
        }

        private static Doc FindPaymentFor(IEnumerable<Doc> payments, int year, int month)
        {
            foreach (var p in payments)
            {
                if (p["Year"] != null && p["Month"] != null)
                {
                    if (year == (int)p["Year"] && month == (int)p["Month"])
                        return p;
                }
            }
            return null;
        }

        private static Doc FindAppPaymentFor(WorkflowContext context, Doc app, int year, int month)
        {
            while (app != null)
            {
                var query = new SqlQuery(app, "Payments", context.UserId, "payments", context.Provider);
                query.AddAttribute("&Id");
                query.AndCondition("Year", ConditionOperation.Equal, year);
                query.AndCondition("Month", ConditionOperation.Equal, month);
                using (var reader = new SqlQueryReader(query))
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

        protected static readonly Guid FirstSectionEnumId = new Guid("{2A273790-9091-4DBD-A712-12D46578196C}");
        protected static readonly Guid SecondSectionEnumId = new Guid("{0B6C58B1-A6F1-4455-8092-9B8583ADA295}");

        private static Doc FindOrderPaymentFor(WorkflowContext context, Doc order, int year, int month, bool firstSection)
        {
            if (order != null)
            {
                var query = new SqlQuery(order, "OrderPayments", context.UserId, "payments", context.Provider);
                query.AddAttribute("&Id");
                query.AndCondition("Year", ConditionOperation.Equal, year);
                query.AndCondition("Month", ConditionOperation.Equal, month);
                query.AndCondition("Section", ConditionOperation.Equal, firstSection ? FirstSectionEnumId : SecondSectionEnumId);
                using (var reader = new SqlQueryReader(context.DataContext, query))
                {
                    if (reader.Read())
                        return context.Documents.LoadById(reader.GetGuid(0));
                }
            }
            return null;
        }

        // Step 3: Payment creation
        public void CreateOrderPayment(WorkflowContext context)
        {
            dynamic order = context.GetDynaDoc(context.CurrentDocument);
            dynamic app = context.GetDynaDoc(order.Application);

            var d = (DateTime)order.Date;

            var payDate = d.Day >= 20 ? d.AddMonths(1) : d;
            var dateFrom1 = (DateTime)context["DateFrom"];
            var dateTo1 = new DateTime(payDate.Year + 1, 12, 31);
            var dateFrom2 = DateTime.MaxValue;
            var dateTo2 = (DateTime)context["DateTo"];
            if (dateTo1 < dateTo2) dateTo2 = dateTo1;
            dateTo1 = DateTime.MinValue;
            decimal amount1 = 0m;
            decimal amount2 = 0m; //app.PaymentSum;

            bool b = false;
            dynamic firstPayment = null;
            var orderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
            var firstSection = new Guid("{2A273790-9091-4DBD-A712-12D46578196C}"); // I раздел
            var secondSection = new Guid("{0B6C58B1-A6F1-4455-8092-9B8583ADA295}");// II раздел

            var payments = Enumerable.ToList(app.GetAttrDocList("Payments"));
            payments.Reverse();
            var onePayment = payments.Count == 1;
            foreach (var paym in payments)
            {
                dynamic payment = context.GetDynaDoc(paym);

                var d1 = new DateTime(payment.Year, payment.Month, 1);
                var d2 = new DateTime(payment.Year, payment.Month,
                                DateTime.DaysInMonth(payment.Year, payment.Month));
                var d3 = new DateTime(payment.Year, payment.Month, 20);

                if ((d1.Year * 12 + d1.Month) < (dateFrom1.Year * 12 + dateFrom1.Month) ||
                    (d1.Year * 12 + d1.Month) > (dateTo2.Year * 12 + dateTo2.Month)) continue;

                if (d > d3 || onePayment)  // Первый раздел
                {
                    amount1 += payment.Amount ?? 0;
                    if (dateTo1 < d2) dateTo1 = d2;
                    order.AddDocToList("Payments1", payment.Doc);

                    if (firstPayment == null)
                    {
                        firstPayment = context.NewDynaDoc(orderPaymentDefId); //DynaDoc.CreateNew(orderPaymentDefId, context.UserId);
                        firstPayment.Section = firstSection;
                        firstPayment.Month = payDate.Month;
                        firstPayment.Year = payDate.Year;
                    }
                }
                else   // Второй раздел
                {
                    amount2 = payment.Amount ?? 0;
                    if (dateFrom2 > d1) dateFrom2 = d1;
                    order.AddDocToList("Payments2", payment.Doc);

                    dynamic orderPayment = context.NewDynaDoc(orderPaymentDefId); //DynaDoc.CreateNew(orderPaymentDefId, context.UserId);
                    orderPayment.Section = secondSection;
                    orderPayment.Month = payment.Month;
                    orderPayment.Year = payment.Year;
                    orderPayment.Amount = payment.Amount;
                    orderPayment.Save();
                    order.AddDocToList("OrderPayments", orderPayment.Doc);
                }
                b = true;
            }
            if (b)
            {
                order.Amount1 = amount1;
                if (dateFrom1 < DateTime.MaxValue && dateTo1 > DateTime.MinValue)
                {
                    order.DateFrom1 = dateFrom1;
                    order.DateTo1 = dateTo1;
                    order.DateFrom = dateTo1;
                }
                else
                    order.DateFrom = dateFrom2;
                order.Amount2 = amount2;
                if (dateFrom2 < DateTime.MaxValue && dateTo2 > DateTime.MinValue)
                {
                    order.DateFrom2 = dateFrom2;
                    order.DateTo2 = dateTo2;
                    order.DateTo = dateTo2;
                }
                if (firstPayment != null)
                {
                    firstPayment.Amount = amount1;
                    firstPayment.Save();
                    order.AddDocToList("OrderPayments", firstPayment.Doc);
                }
            }
            else
                throw new ApplicationException("Поручение не сформировано!");

            context.SuccessFlag = true;
        }

        public void CreateOrderPayments(WorkflowContext context, dynamic order)
        {
            dynamic app = context.GetDynaDoc(order.Application);

            var orderDate = (DateTime) order.Date;
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

            var orderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");

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

                dynamic payment = context.NewDynaDoc(orderPaymentDefId);
                payment.Amount = p["Amount"];
                payment.Payment = p.Id;
                if (orderDate > d3 || (mFrom == m && yFrom == y))  // I раздел
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
            context.SuccessFlag = true;
        }
    }
}
