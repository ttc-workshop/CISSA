using System;
using System.Data.Entity.Core.EntityClient;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DocQueryTest
{
    [TestClass]
    public class UnitTest3
    {
        // Document Defs Id
        public static readonly Guid OrderPaymentDefId = new Guid("{AD83752B-C412-4FEC-A345-BB0495C34150}");
        public static readonly Guid ReportDefId = new Guid("{0E05462C-4A4C-4729-972E-5074DB1DED4E}");
        public static readonly Guid ReportItemDefId = new Guid("{3C1A7B35-8300-4E4D-9BD9-F9AC0D4C81D6}");
        public static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        public static readonly Guid AppDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
        public static readonly Guid PersonDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");

        public static readonly Guid FirstMayUsrOrgId = new Guid("{34DDCAF2-EB08-48E7-894A-29C929D62C83}");

        [TestMethod]
        public void TestMethod1()
        {
            var qd = new QueryBuilder(AppDefId);
            qd.Where("Applicant").Include("PIN").IsNull();

            using (var query = SqlQueryBuilder.Build(qd))
            {
                query.AddAttribute(query.Source, "&Id");
                query.AddAttributes("LastName", "FirstName", "MiddleName", "BirthDate", "Sex", "PassportNo", "PassportDate", "Education");
                query.AddAttributes("ZipCode", "Town", "Street", "House", "Apartment", "HomePhone", "Category");

                query.AndCondition("&OrgId", ConditionOperation.Equal, FirstMayUsrOrgId);

                using (var reader = new SqlQueryReader(query))
                {
                    var sql = reader.GetSql();
                    Console.Write(sql);
                }
            }
        }

        [TestMethod]
        public void Test2()
        {
            var qb = new QueryBuilder(AppDefId);
            qb.Where("Applicant").IsNotNull().AndExp("RegNo").Contains("123%").Or("RegNo").Contains("321%").End().And("RegDate").IsNotNull();

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
                    reader.Read();
                }
            }
        }
        [TestMethod]
        public void Test3()
        {
//            var appDefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}");
            var bankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}");
            var assignmentDefId = new Guid("{51935CC6-CC48-4DAC-8853-DA8F57C057E8}");
            var paymentDefId = new Guid("{68667FBB-C149-4FB3-93AD-1BBCE3936B6E}");

            var assignedStateId = new Guid("{ACB44CC8-BF44-44F4-8056-723CED22536C}");

            var bqb = new QueryBuilder(bankAccountDefId);
            var aqb = new QueryBuilder(assignmentDefId);
            var pqb = new QueryBuilder(paymentDefId);

            const int month = 7;
            const int year = 2015;

            bqb.Where("Account_No").IsNotNull()
                .And("Application").Include("&State").Eq(assignedStateId); // Bank accounts
            aqb.AndExp("Year").Lt(year).Or("Year").Eq(year).And("Month").Le(month).End()
                .And("Application").In(bqb.Def, "Application")
                .And("&Id").NotIn(pqb.Def, "Assignment");

            var ser = new DataContractJsonSerializer(typeof(QueryDef));
            var ms = new MemoryStream();

            ser.WriteObject(ms, aqb.Def);
            ms.Position = 0;
            Console.WriteLine(ms.Length);
            byte[] bytes = ms.ToArray();
            ms.Close();
            Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));

            using (var dataContext = new DataContext(new EntityConnection("name=asistEntities")))
            {
                var query = SqlQueryBuilder.Build(dataContext, aqb.Def);
                query.AddAttribute(query.Source, "Year");
                query.AddAttribute(query.Source, "Month");

                using (var reader = new SqlQueryReader(dataContext, query))
                {
                    var sql = reader.GetSql();
                    Console.Write(sql);
                    reader.Read();
                }
            }
        }
    }
}
