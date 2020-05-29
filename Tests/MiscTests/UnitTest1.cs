using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Excel;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiscTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ExcelDataReadTest()
        {
            System.Text.RegularExpressions.Regex numbers = new System.Text.RegularExpressions.Regex("^[0-9]");

            using (
                var stream = File.Open(@"C:\Users\Администратор\Downloads\Хисор_банк_отчёт об остатках.xls", FileMode.Open,
                    FileAccess.Read))
            {
                //2. Reading from a OpenXml Excel file (2007 format; *.xlsx)
                using (var excelCreator = new ExcelReaderCreator(stream, ExcelDataFormat.Binary))
                {
                    var started = false;
                    var excelReader = excelCreator.Reader;
                    //5. Data Reader methods
                    while (excelReader.Read())
                    {
                        if (!started)
                        {
                            var v = excelReader.GetString(0);
                            //if (v != "1") continue;
                            var isMatch = numbers.IsMatch(v);
                            if (v.Length < 20 || !isMatch) continue;
                        }
                        started = true;

                        var i = excelReader.GetString(0);
                        var fio = excelReader.GetString(1);
                        var accountNo = excelReader.GetString(2);
                        var clientId = excelReader.GetString(5);

                        Console.WriteLine(@"{0}. ФИО: ""{1}"" № счета: ""{2}"" клиент Id: ""{3}""", i, fio, accountNo,
                            clientId);
                    }
                    //6. Free resources (IExcelDataReader is IDisposable)
                    excelReader.Close();
                }
            }
        }

        public const string AsistConnectionString =
            "Data Source=localhost;Initial Catalog=asist_db;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        public const string NrszConnectionString =
            "Data Source=localhost;Initial Catalog=nrsz_db;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        [TestMethod]
        public void WorkflowContextDataCloneTest()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var engine = provider.Get<IObjectCloner<WorkflowContextData>>();
                        var data = new WorkflowContextData();
                        var context = new WorkflowContext(data, provider);
                        context["Message"] = "Hello World!!!";
                        var cloneData = engine.Clone(data);
                        context = new WorkflowContext(cloneData, provider);
                        Assert.AreEqual(context["Message"], "Hello World!!!", "Fail");
                    }
                }
            }
        }

        [TestMethod]
        public void DocAttrIsSameTest()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var docRepo = provider.Get<IDocRepository>();
                        var doc1 = docRepo.LoadById(new Guid("D7289560-1E37-41C3-98A3-00001EA41209"));
                        var doc2 = docRepo.LoadById(new Guid("EE213850-FC43-43DE-960B-000887D7EB17"));
                        var equal = doc1.AttrIsSame(doc2,
                            new[]
                            {
                                "Last_Name", "First_Name", "Middle_Name",
                                "PassportSeries", "PassportNo", "Issuing_Authority"
                            },
                            (v1, v2) =>
                            {
                                var s1 = v1 != null ? v1.ToString().Trim().ToUpper() : String.Empty;
                                var s2 = v2 != null ? v2.ToString().Trim().ToUpper() : String.Empty;
                                return String.Equals(s1, s2);
                            }, null);
                        Assert.AreEqual(doc1.AttrIsSame(doc2, "Last_Name"), "Fail");
                    }
                }
            }
        }

        [TestMethod]
        public void NrszCallFindPersonsProcessTest()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var userDataProvider = new UserDataProvider("d", "123", provider);
                        var reg = provider.Get<IAppServiceProviderRegistrator>();
                        reg.AddService(userDataProvider);

                        var contextData = new WorkflowContextData(Guid.NewGuid(), userDataProvider.UserId);
                        var workflowRepo = provider.Get<IWorkflowRepository>();
                        var gateRef = workflowRepo.LoadGateRefById(new Guid("{947DD458-210E-4350-BE0E-03F4CED7D78C}"));

                        var launcher = provider.Get<IExternalProcessLauncher>();
                        var result = launcher.Launch(gateRef, "FindPersons", contextData);
                        Console.WriteLine(result.Type + " " + result.Message);
                    }
                }
            }
        }

        public static readonly Guid RaionDefId = new Guid("{BA5D4276-6BFB-4180-9D4F-828E38E95601}");
        public static readonly Guid PersonDefId = new Guid("{6052978A-1ECB-4F96-A16B-93548936AFC0}");

        [TestMethod]
        public void NrszFindSamePersonTest()
        {
            using (var connection = new SqlConnection(NrszConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var userDataProvider = new UserDataProvider("d", "123", provider);
                        var reg = provider.Get<IAppServiceProviderRegistrator>();
                        reg.AddService(userDataProvider);

                        var qb = new QueryBuilder(PersonDefId);
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var exp1 = query.AddExpCondition(ExpressionOperation.And);

                            var v = "Иванов";
                            var name = v != null ? v.ToString() : String.Empty;
                            if (!String.IsNullOrEmpty(name))
                                query.AndCondition(query.Source, "Last_Name", ConditionOperation.Levenstein,
                                    name, exp1);
                            v = "Иван";
                            name = v != null ? v.ToString() : String.Empty;
                            if (!String.IsNullOrEmpty(name))
                                query.AndCondition(query.Source, "First_Name", ConditionOperation.Levenstein,
                                    name, exp1);
                            v = "Иванович";
                            name = (v != null ? v.ToString() : String.Empty).Trim().ToLower();
                            if (!String.IsNullOrEmpty(name))
                                query.AndCondition(query.Source, "Middle_Name", ConditionOperation.Levenstein,
                                    name, exp1);
                            var dt = new DateTime(1981,1,1);
                            if (dt != null)
                                query.AndCondition(query.Source, "Date_of_Birth", ConditionOperation.Levenstein,
                                    dt, exp1);

                            var passportSeries = "А";
                            var passportNo = "234465";

                            if (!String.IsNullOrEmpty(passportSeries) || !String.IsNullOrEmpty(passportNo))
                            {
                                var exp2 = query.AddExpCondition(ExpressionOperation.Or);
                                if (!String.IsNullOrEmpty(passportSeries))
                                    query.AndCondition(query.Source, "PassportSeries", ConditionOperation.Levenstein,
                                        passportSeries, exp2);
                                if (!String.IsNullOrEmpty(passportNo))
                                    query.AndCondition(query.Source, "PassportNo", ConditionOperation.Levenstein,
                                        passportNo, exp2);
                            }
                            var docRepo = provider.Get<IDocRepository>();
                            using (var reader = new SqlQueryReader(query))
                            {
                                var dupPersonList = new List<Guid>();
                                var personList = new List<Doc>();
                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());
                                while (reader.Read())
                                {
                                    var personId = reader.GetGuid(0);
                                    dupPersonList.Add(personId);
                                    var p = docRepo.LoadById(personId);
                                    personList.Add(p);
                                }

                                Console.WriteLine(dupPersonList.Count.ToString());
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void NrszPinCodeGenerationTest()
        {
            using (var connection = new SqlConnection(NrszConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var userDataProvider = new UserDataProvider("d", "123", provider);
                        var reg = provider.Get<IAppServiceProviderRegistrator>();
                        reg.AddService(userDataProvider);

                        int? raionNo = 68;
                        int? oblastNo = 5;

                        var qb = new QueryBuilder(RaionDefId);
                        qb.Where("Number").Eq((int) raionNo).And("Area").Include("Number").Eq((int) oblastNo);

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                if (!reader.Read())
                                    throw new ApplicationException(
                                        "Не могу сформировать ПИН. Ошибка в коде области или района!");
                            }
                        }

                        var s = raionNo.ToString();
                        while (s.Length < 3) s = '0' + s;
                        var pin = oblastNo + s;

                        qb = new QueryBuilder(PersonDefId);
                        qb.Where("IIN").Like(pin + "_______");
                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());
                                {
                                    var maxPin = reader.GetMax("IIN");
                                    Console.WriteLine(maxPin ?? "null");
                                    var no = 0;
                                    if (maxPin != null)
                                    {
                                        s = maxPin.ToString();
                                        if (!String.IsNullOrWhiteSpace(s) && s.Length > 4)
                                        {
                                            no = int.Parse(s.Substring(4, 6));
                                            Console.WriteLine("No: " + no);
                                        }
                                    }
                                    s = (no + 1).ToString();
                                    while (s.Length < 6) s = '0' + s;
                                }
                            }
                        }
                    }
                }
            }
        }


        [TestMethod]
        public void NrszFindPersonsTest()
        {
            using (var connection = new SqlConnection(NrszConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var userDataProvider = new UserDataProvider("d", "123", provider);
                        var reg = provider.Get<IAppServiceProviderRegistrator>();
                        reg.AddService(userDataProvider);

                        var lastName = "Иванов";
                        var firstName = "Иван";
                        var middleName = "Иванович";
                        var dt = new DateTime(1981, 1, 1);
                        var passportSeries = ""; //"А";
                        var passportNo = ""; //"234466";

                        var qb = new QueryBuilder(PersonDefId);
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var exp1 = query.AddExpCondition(ExpressionOperation.And);

                            var name = lastName != null ? lastName.ToString() : String.Empty;
                            if (!String.IsNullOrEmpty(name))
                                query.AndCondition(query.Source, "Last_Name", ConditionOperation.Levenstein,
                                    lastName, exp1);
                            name = firstName != null ? firstName.ToString() : String.Empty;
                            if (!String.IsNullOrEmpty(name))
                                query.AndCondition(query.Source, "First_Name", ConditionOperation.Levenstein,
                                    name, exp1);
                            name = (middleName != null ? middleName.ToString() : String.Empty).Trim().ToLower();
                            if (!String.IsNullOrEmpty(name))
                                query.AndCondition(query.Source, "Middle_Name", ConditionOperation.Levenstein,
                                    name, exp1);
                            if (dt != null)
                                query.AndCondition(query.Source, "Date_of_Birth", ConditionOperation.Levenstein,
                                    dt, exp1);

                            if (!String.IsNullOrEmpty(passportSeries) || !String.IsNullOrEmpty(passportNo))
                            {
                                var exp2 = query.AddExpCondition(ExpressionOperation.Or);
                                if (!String.IsNullOrEmpty(passportSeries))
                                    query.AndCondition(query.Source, "PassportSeries", ConditionOperation.Levenstein,
                                        passportSeries, exp2);
                                if (!String.IsNullOrEmpty(passportNo))
                                    query.AndCondition(query.Source, "PassportNo", ConditionOperation.Levenstein,
                                        passportNo, exp2);
                            }
                            var docRepo = provider.Get<IDocRepository>();
                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                var dupPersonList = new List<Guid>();

                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());
                                while (reader.Read())
                                {
                                    var personId = reader.GetGuid(0);
                                    dupPersonList.Add(personId);
                                    Console.WriteLine(personId);
                                }
                            }
                        }

                        var hasPassportSeries = !String.IsNullOrEmpty(passportSeries);
                        var hasPassportNo = !String.IsNullOrEmpty(passportNo);

                        if (hasPassportNo && hasPassportSeries)
                        {
                            qb.And("PassportSeries").Eq(passportSeries)
                                .And("PassportNo").Eq(passportNo);
                        }
                        qb.And("Last_Name").Eq(lastName).And("First_Name").Eq(firstName)
                            .And("Date_of_Birth").Eq(dt);
                        if (!String.IsNullOrEmpty(middleName))
                            qb.And("Middle_Name").Eq(middleName);

                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            query.AddAttribute("&Id");
                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                var dupPersonList = new List<Guid>();
                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());
                                while (reader.Read())
                                {
                                    dupPersonList.Add(reader.GetGuid(0));
                                    if (dupPersonList.Count > 1) break;
                                }

                                if (dupPersonList.Count == 1)
                                {
                                    Console.WriteLine("Same Person Found!!!");
                                }
                                else
                                {
                                    Console.WriteLine("Same Person Not Found!!!");
                                }
                                Console.WriteLine(dupPersonList.Count.ToString());
                            }
                        }
/*
                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            query.OrCondition(query.Source, "Last_Name", ConditionOperation.Levenstein,
                                lastName);
                            query.OrCondition(query.Source, "First_Name", ConditionOperation.Levenstein,
                                firstName);
                            var s = middleName;
                            var name = (s != null ? s.ToString() : String.Empty).Trim().ToLower();
                            if (!String.IsNullOrEmpty(name))
                                query.OrCondition(query.Source, "Middle_Name", ConditionOperation.Levenstein,
                                    name);
                            query.OrCondition(query.Source, "Date_of_Birth", ConditionOperation.Levenstein,
                                dt);

                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                var dupPersonList = new List<Guid>();
                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());
                                while (reader.Read())
                                {
                                    dupPersonList.Add(reader.GetGuid(0));
                                    Console.WriteLine(reader.GetGuid(0));
                                }
                            }
                        }
                        */
                    }
                }
            }
        }

        [TestMethod]
        public void LinaMethodTest()
        {
            var pin = "5068000001";

            var s = pin != null ? pin.ToString() : "";
            if (s.Length != 10)
                throw new ApplicationException("Неверная длина ПИН");

            var sum = 0;
            for (var i = 0; i <= 9; i++)
            {
                var ch = s[i];
                var dn = int.Parse(ch.ToString());
                if (i % 2 != 0)
                {
                    var n = dn * 2;
                    if (n > 9) n = n - 9;
                    sum += n;
                }
                else
                    sum += dn;
            }
            sum = 10 - (sum % 10);
            if (sum == 10) sum = 0;
            Console.WriteLine(sum.ToString() + ", " + s + sum.ToString());
        }

        [TestMethod]
        public void AsistBuildPaymentBankRegistryTest()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var userDataProvider = new UserDataProvider("d", "123", provider);
                        var reg = provider.Get<IAppServiceProviderRegistrator>();
                        reg.AddService(userDataProvider);

                        var appDefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}");
                        var bankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}");
                        var assignmentDefId = new Guid("{51935CC6-CC48-4DAC-8853-DA8F57C057E8}");
                        var paymentDefId = new Guid("{68667FBB-C149-4FB3-93AD-1BBCE3936B6E}");

                        var assignedStateId = new Guid("{ACB44CC8-BF44-44F4-8056-723CED22536C}");
                        var onPaymentStateId = new Guid("{78C294B5-B6EA-4075-9EEF-52073A6A2511}");
                        var DistrictBankOpenDefId = new Guid("{ADF1D21A-5FCE-4F42-8889-D0714DDF7967}");

                        var appStateDefId = new Guid("{547BBA55-2281-4388-A1FC-EE890168AC2D}");
                        var onPreparingStateTypeId = new Guid("{A0408744-2100-42DE-B03B-1B572102FBF5}");
    

                        var year = 2015;
                        if (year < 2000) throw new ApplicationException("Ошибка в значении года!");
                        var month = 10;
                        if (month < 1 || month > 12) throw new ApplicationException("Ошибка в значении месяца!");

                        var bqb = new QueryBuilder(bankAccountDefId);
                        var aqb = new QueryBuilder(assignmentDefId);
                        var pqb = new QueryBuilder(paymentDefId);

                        bqb.Where("Account_No").IsNotNull()
                            .And("Application").Include("&State").In(new object[] { assignedStateId, onPaymentStateId }); // Bank accounts
                        aqb.AndExp("Year").Lt(year).Or("Year").Eq(year).And("Month").Le(month).End()
                            .And("Application").In(bqb.Def, "Application")
                            .And("&Id").NotIn(pqb.Def, "Assignment");

                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();

                        using (var query = sqlQueryBuilder.Build(aqb.Def))
                        {
                            query.AddAttribute(query.Source, "&Id");
                            var appSource = query.JoinSource(query.Source, appDefId, SqlSourceJoinType.Inner, "Application");
                            var bankAccountSource = query.JoinSource(appSource, bankAccountDefId, SqlSourceJoinType.Inner, "Application");
                            var appStateSrc = query.JoinSource(appSource, appStateDefId, SqlSourceJoinType.Inner, "Application_State");

                            query.AddAttribute(bankAccountSource, "&Id");
                            query.AddAttribute(query.Source, "Amount");
                            query.AddAttribute(appStateSrc, "DistrictId");

                            using (var reader = new SqlQueryReader(provider, query))
                            {
                                reader.Open();
                                Console.WriteLine(reader.Query.BuildSql());

                                var count = 0;
                                var totalAmount = 0m;

                                var districtItems = new List<Doc>();

                                while (reader.Read())
                                {
                                    var districtId = reader.Reader.GetGuid(3);

                                    var assignmentId = reader.Reader.GetGuid(0);
                                    var bankAccountId = reader.Reader.GetGuid(1);
                                    var paymentAmount = reader.IsDbNull(2) ? 0 : reader.GetDecimal(2);
                                    totalAmount += paymentAmount;

                                    count++;
                                }
                            }
                        } 
                    }
                }
            }
        }

        [TestMethod]
        public void DateTimeConvertTest()
        {
            var dates = new string[] {"21.01.2016", "21/01/2016"/*, "20160121"*/};
            foreach (var s in dates)
            {
                object o = s;
                var d = Convert.ToDateTime(o);
                Console.WriteLine(d);
                Assert.AreEqual(new DateTime(2016,01,21), d);
            }
        }
    }
}
