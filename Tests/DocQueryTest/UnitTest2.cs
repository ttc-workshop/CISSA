using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace DocQueryTest
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void TestMethod1()
        {
            var enumRepo = new EnumRepository();
            var paymentTypeDefId = Guid.Parse("{A9C9A563-6BE1-48CB-8C04-462D02B565F8}");
            var paymentOrderDefId = Guid.Parse("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
            var userId = Guid.Parse("180b1e71-6cda-4887-9f83-941a12d7c979");

            foreach (var enumValue in enumRepo.GetEnumItems(paymentTypeDefId))
            {
                var qb = new QueryBuilder(paymentOrderDefId, userId);

                qb.Where("&State").Eq("Утвержден").And("Application").Include("PaymentType").Eq(enumValue.Id).End()
                    .And("Payments1").Include("Year").Eq(2012).And("Month").Eq(2).End();

                var query = new DocQuery(qb.Def);
                var count1 = query.Count();
                var sum1 = query.Sum("Amount1") ?? 0;

                qb = new QueryBuilder(paymentOrderDefId, userId);

                qb.Where("&State").Eq("Утвержден").And("Application").Include("PaymentType").Eq(enumValue.Id).End()
                    .And("Payments2").Include("Year").Eq(2012).And("Month").Eq(2).End();

                query = new DocQuery(qb.Def);
                var count2 = query.Count();
                var sum2 = query.Sum("Amount2") ?? 0; 
                
                Console.WriteLine(@"payment: {0}, count: {1}, sum: {2}", enumValue.Value, count1 + count2, sum1 + sum2);

//                foreach (var docId in query.All())
//                {
//                    Console.WriteLine(@"-- {0}", docId);    
//                }
                
            }
        }

        [TestMethod]
        public void TestMethod2()
        {
            var paymentTypeDefId = Guid.Parse("{A9C9A563-6BE1-48CB-8C04-462D02B565F8}");
            var paymentOrderDefId = Guid.Parse("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
            var userId = Guid.Parse("180b1e71-6cda-4887-9f83-941a12d7c979");

            var qb = new QueryBuilder(paymentOrderDefId, userId);

            qb.Where("&State").Eq(/*"Утвержден"*/Guid.NewGuid()).And("Application").Include("PaymentType").Eq(paymentTypeDefId).End()
                .And("Payments1").Include("Year").Eq(2012).Or("Month").Eq(2).End();

            var ser = new DataContractJsonSerializer(typeof(QueryDef));
            var ms = new MemoryStream();

            ser.WriteObject(ms, qb.Def);
            ms.Position = 0;
            Console.WriteLine(ms.Length);
            byte[] bytes = ms.ToArray();
            ms.Close();
            Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));

            using (var dataContext = new DataContext(new EntityConnection("name=cissaEntities")))
            {
                var query = SqlQueryBuilder.Build(dataContext, qb.Def);

                using (var reader = new SqlQueryReader(dataContext, query))
                {
                    var sql = reader.GetSql();
                    Console.Write(sql);
//                    reader.Read();
                }
            }
        }

        [TestMethod]
        public void ShowStudentGreatThanAge()
        {
            var appDefId = Guid.Parse("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
            var enumStudentGt21Id = Guid.Parse("{46AEAEA2-D42F-47C9-A571-9F3789413CC3}");
            var userId = Guid.Parse("180b1e71-6cda-4887-9f83-941a12d7c979");

            var qb = new QueryBuilder(appDefId, userId);

            qb.Where("EmploymentStatus").Eq(enumStudentGt21Id)
                .Or("FamilyMembers").Include("EmploymentStatus").Eq(enumStudentGt21Id).End();

            var query = new DocQuery(qb);
            foreach (var app in query.All())
            {
                Console.WriteLine(app);
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            var enumRepo = new EnumRepository();
            var paymentTypeDefId = Guid.Parse("{A9C9A563-6BE1-48CB-8C04-462D02B565F8}");
            var paymentOrderDefId = Guid.Parse("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
            var orderPaymentDefId = Guid.Parse("{AD83752B-C412-4FEC-A345-BB0495C34150}");
            var userId = Guid.Parse("180b1e71-6cda-4887-9f83-941a12d7c979");

            foreach (var enumValue in enumRepo.GetEnumItems(paymentTypeDefId))
            {
                var qb = new QueryBuilder(paymentOrderDefId, userId);

                qb.Where(/*"&State").Eq("Утвержден").And(*/"Application").Include("PaymentType").Eq(enumValue.Id).End();

                var qb2 = new QueryBuilder(orderPaymentDefId, userId);

                qb2.Where("Year").Eq(2012).And("Month").Eq(4).And("&Id").In(qb.Def, "OrderPayments");

                var query = new DocQuery(qb2.Def);
                var count1 = query.Count();
                var sum1 = query.Sum("Amount") ?? 0;

                Console.WriteLine(@"payment: {0}, count: {1}, sum: {2}", enumValue.Value, count1, sum1);

                //                foreach (var docId in query.All())
                //                {
                //                    Console.WriteLine(@"-- {0}", docId);    
                //                }

            }
        }

        [TestMethod]
        public void GetNotPaidOrderPayments()
        {
            {
                var accountId = Guid.Parse("{6e13c6fa-c6d7-4709-a0e4-0f75ea00bfb4}"); // context.Get<Guid>("InputDocumentId"); //("AccountId");

                var actualPaymentDefId = Guid.Parse("{A8B9DAB6-CDEA-44A5-BAF5-D19F1879B9A6}");
                var orderDefId = Guid.Parse("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
                var orderPaymentDefId = Guid.Parse("{AD83752B-C412-4FEC-A345-BB0495C34150}");
                var accountDefId = Guid.Parse("{81C532F6-F5B0-4EFC-8305-44E864E778D3}");

                var userId = Guid.Parse("180b1e71-6cda-4887-9f83-941a12d7c979");

                var curAccount = new QueryBuilder(accountDefId, userId);
                curAccount.Where("&Id").Eq(accountId);

                var actualPayments = new QueryBuilder(actualPaymentDefId, userId);
                actualPayments.Where("&Id").In(curAccount.Def, "ActualPayments");

                var orders = new QueryBuilder(orderDefId, userId);
                orders.Where("Account").In(curAccount.Def).And("&State").Eq("Утвержден");

                var orderPayments = new QueryBuilder(orderPaymentDefId, userId);
                orderPayments.Where("&Id").In(orders.Def, "OrderPayments")
                    .AndNot("&Id").In(actualPayments.Def, "OrderPayment");

                var query = new DocQuery(curAccount);
                var rows = query.All();

                Console.WriteLine(@"Лицевые счета: " + rows.Count().ToString());
                foreach (var id in rows)
                {
                    Console.WriteLine(id.ToString());
                }

                query = new DocQuery(actualPayments);
                rows = query.All();

                Console.WriteLine(@"Фактические выплаты: " + rows.Count().ToString());
                foreach (var id in rows)
                {
                    Console.WriteLine(id.ToString());
                }

                query = new DocQuery(orders);
                rows = query.All();

                Console.WriteLine(@"Поручения: " + rows.Count().ToString());
                foreach (var id in rows)
                {
                    Console.WriteLine(id.ToString());
                }

                query = new DocQuery(orderPayments);
                rows = query.All();

                Console.WriteLine(@"Выплаты поручений: " + rows.Count().ToString());
                foreach (var id in rows)
                {
                    Console.WriteLine(id.ToString());
                }
            }
        }

        [TestMethod]
        public void CheckDocumentVisibility()
        {
            using (var dataContext = new DataContext())
            {
                var userRepo = new UserRepository(dataContext);
                UserInfo userInfo = userRepo.GetUserInfo(Guid.Parse("1E35C341-5CC4-4CF9-823F-8B869B8DFDC1") /*Guid.Parse("{E6B4CC44-EBC9-43B7-B7D0-02DC68E1E7AC}")*/);
                var em = dataContext.Entities;

                if (userInfo.OrgUnitTypeId != null)
                {
                    var userOrgUnits =
                        em.Object_Defs.OfType<Org_Unit>().SelectMany(ou => ou.Org_Units_1).Select(ou => ou.Id);
                    var organizations =
                        em.Object_Defs.OfType<Organization>().Where(
                            o => o.Org_Units_1.Any(ou => ou.Id == userInfo.OrgUnitTypeId));
                    var orgUnits =
                        em.Object_Defs.OfType<Org_Unit>().Where(
                            o => o.Org_Units_1.Any(ou => ou.Id == userInfo.OrgUnitTypeId));

                    foreach (var org in organizations)
                        Console.WriteLine(org.Full_Name);
                    Console.WriteLine(@"---------------");
                    foreach (var orgUnit in orgUnits)
                        Console.WriteLine(orgUnit.Full_Name);
                }
            }
        }
    }
}
