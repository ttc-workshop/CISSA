using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Report;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DocQueryTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        private readonly static DateTime MaxDate = new DateTime(9999, 12, 31);

        public UnitTest1()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void QueryWithState()
        {
            using (var dataContext = new DataContext())
            {
                var builder = new QueryBuilder("SocialApplication");
                builder.Where("&State").Eq("{A9CD37C4-A718-4DE1-9E95-EC8EC280C8D4}");
                var query = new DocQuery(builder.Def, dataContext);
                foreach (var id in query.All())
                {
                    Console.WriteLine(id.ToString());
                }
            }
        }

        [TestMethod]
        public void QueryFromDoc1()
        {
            using (var dataContext = new DataContext())
            {
                var defRepo = new DocDefRepository(dataContext);
                var docRepo = new DocRepository(dataContext);

                var doc = docRepo.New(defRepo.DocDefByName("Person-Sheet").Id);

                var attr = doc.Get<TextAttribute>("LastName");
                attr.Value = "Иван";

                var engine = new QueryEngine(dataContext);
                var query = new DocQuery(engine.QueryFromDoc(doc), dataContext);

                foreach (var id in query.All())
                {
                    Console.WriteLine(id.ToString());
                }
            }
        }

        [TestMethod]
        public void QueryFromDoc2()
        {
            using (var dataContext = new DataContext())
            {
                var defRepo = new DocDefRepository(dataContext);
                var docRepo = new DocRepository(dataContext);

                var doc = docRepo.New(defRepo.DocDefByName("SocialApplication").Id);

                var attr = doc.Get<DocAttribute>("Applicant");
                attr.Document = docRepo.New(defRepo.DocDefByName("Person-Sheet").Id);
                var attr2 = attr.Document.Get<TextAttribute>("LastName");
                attr2.Value = "Иван";

                var engine = new QueryEngine(dataContext);
                var query = new DocQuery(engine.QueryFromDoc(doc), dataContext);

                foreach (var id in query.All())
                {
                    Console.WriteLine(id.ToString());
                }
            }
        }

        [TestMethod]
        public void TestQueryWithUser()
        {
            var builder = new QueryBuilder("SocialApplication");
            
            builder.Where("&UserName").Eq("R");

            using (var dataContext = new DataContext())
            {
                var query = new DocQuery(builder.Def, dataContext);

                foreach (var id in query.All())
                {
                    Console.WriteLine(id.ToString());
                }
            }
        }

        [TestMethod]
        public void DiffQuery5()
        {
            //
            var builder = new QueryBuilder("SocialApplication");

            builder.Where("&State").Eq("На регистрации");

            using (var dataContext = new DataContext())
            {
                var query = new DocQuery(builder.Def, dataContext);
                foreach (var id in query.All())
                {
                    Console.WriteLine(id.ToString());
                }
            }
        }

        [TestMethod]
        public void DiffQuery2()
        {
            var builder = new QueryBuilder("Person-Sheet");
            builder.Where("LastName").Contains("Иван");

            using (var dataContext = new DataContext())
            {
                var query = new DocQuery(builder.Def, dataContext);
                foreach (var id in query.All())
                {
                    Console.WriteLine(id.ToString());
                }
            }
        }

        [TestMethod]
        public void DiffQuery3()
        {
            var builder = new QueryBuilder("SocialApplication");
            builder.Where("Applicant").Include("LastName").Contains("Иван").End()/*.And("PaymentSum").Ge(300)*/;

            using (var dataContext = new DataContext((EntityConnection) null))
            {
                var query = new DocQuery(builder.Def, dataContext);
                foreach (var id in query.All())
                {
                    Console.WriteLine(id.ToString());
                }
            }
        }

        [TestMethod]
        public void DiffQuery4()
        {
            var builder = new QueryBuilder("SocialApplication");
            builder.Where("FamilyMembers").Include("Person").Include("LastName").Contains("Иван").End().End().And("PaymentSum").Ge(300);

            using (var dataContext = new DataContext())
            {
                var query = new DocQuery(builder.Def, dataContext);
                foreach (var id in query.All())
                {
                    Console.WriteLine(id.ToString());
                }
            }
        }

        [TestMethod]
        public void CalculateDocAttrListSum()
        {
            using (var dataContext = new DataContext())
            {
                var docRepo = new DocRepository(dataContext);
                var doc = docRepo.LoadById(Guid.Parse("aa2428cf-3ccd-4b3a-91d6-c5966650499a"));

                var sum = docRepo.CalcAttrDocListSum(doc, "Incomes", "Amount");

                Console.WriteLine(sum);
            }
        }

        [TestMethod]
        public void CalculateDocAttrListSum2()
        {
            using (var dataContext = new DataContext())
            {
                var docRepo = new DocRepository(dataContext);
                var doc = docRepo.LoadById(Guid.Parse("aa2428cf-3ccd-4b3a-91d6-c5966650499a"));

                var defRepo = new DocDefRepository(dataContext);
                var attr = doc.Attributes.First(a => String.Compare(a.AttrDef.Name, "Incomes") == 0);

                var sumDocDef = defRepo.DocDefById(attr.AttrDef.DocDefType.Id);

                var sumAttr = sumDocDef.Attributes.First(a => String.Compare(a.Name, "Amount", true) == 0);

                var maxDate = new DateTime(9990, 12, 31);
                var en = dataContext.Entities;

                var attrDocList = en.DocumentList_Attributes
                    .Where(a => a.Document_Id == doc.Id && a.Def_Id == attr.AttrDef.Id &&
                                a.Expired >= maxDate)
                    .Select(a => a.Value);
                foreach (var guid in attrDocList)
                {
                    Console.WriteLine(guid);
                }

                double? result = null;

                switch (sumAttr.Type.Id)
                {
                    case (short) CissaDataType.Int:
                        var ri = en.Int_Attributes.Include("Documents")
                            .Where(a => a.Def_Id == sumAttr.Id && a.Expired >= maxDate &&
                                        attrDocList.Contains(a.Document_Id))
                            .Select(a => a.Value);
                        if (ri.Any()) result = ri.Sum();
                        break;
                    case (short) CissaDataType.Float:
                        var rf = en.Float_Attributes.Include("Documents")
                            .Where(a => a.Def_Id == sumAttr.Id && a.Expired >= maxDate &&
                                        attrDocList.Contains(a.Document_Id))
                            .Select(a => a.Value);
                        if (rf.Any()) result = rf.Sum();
                        break;
                    case (short) CissaDataType.Currency:
                        var rc = en.Currency_Attributes.Include("Documents")
                            .Where(a => a.Def_Id == sumAttr.Id && a.Expired >= maxDate &&
                                        attrDocList.Contains(a.Document_Id))
                            .Select(a => a.Value);
                        if (rc.Any()) result = (double) rc.Sum();
                        break;
                }

                Console.WriteLine(result);
            }
        }

        [TestMethod]
        public void DiffQuery()
        {
            using (var dataContext = new DataContext())
            {
                var defRepo = new DocDefRepository(dataContext);

                var socAppDef = defRepo.DocDefByName("SocialApplication");
                var personSheetDef = defRepo.DocDefByName("Person-Sheet");

                var saDefIds = defRepo.GetDocDefDescendant(socAppDef.Id).ToList();
                var em = dataContext.Entities;

                var saq = from doc in em.Documents
                          where saDefIds.Contains(doc.Def_Id ?? Guid.Empty) &&
                                (doc.Deleted == null || doc.Deleted == false)
                          select doc;

                var psDefIds = defRepo.GetDocDefDescendant(personSheetDef.Id).ToList();

                var psq = from doc in em.Documents
                          where psDefIds.Contains(doc.Def_Id ?? Guid.Empty) &&
                                (doc.Deleted == null || doc.Deleted == false)
                          select doc;

                var pstaq = from a in em.Text_Attributes
                            where a.Value.Contains("Иван") && a.Expired >= MaxDate
                            select a.Document;

                psq = psq.Intersect(pstaq);

                foreach (var docId in psq)
                {
                    Console.WriteLine(docId.ToString());
                }

                var sapsq = from a in em.Document_Attributes.Include("Documents")
                            where psq.Select(d => d.Id).Contains(a.Value) && a.Expired >= MaxDate
                            select a.Document;

                saq = saq.Intersect(sapsq);

                Console.WriteLine(@"---------------");
                foreach (var docId in saq.Select(d => d.Id))
                {
                    Console.WriteLine(docId.ToString());
                }
            }
        }

        [TestMethod]
        public void TestDifQuery1()
        {
            using (var dataContext = new DataContext())
            {
                var defRepo = new DocDefRepository(dataContext);

                var socAppDef = defRepo.DocDefByName("SocialApplication");
                var personSheetDef = defRepo.DocDefByName("Person-Sheet");

                var saDefIds = defRepo.GetDocDefDescendant(socAppDef.Id).ToList();
                var psDefIds = defRepo.GetDocDefDescendant(personSheetDef.Id).ToList();


                /*           var query = em.Documents.Where(d => saDefIds.Contains(d.Id) && (d.Deleted == null || d.Deleted == false))
                               .Intersect(
                                   em.Document_Attributes.Include("Documents").Where(a => (
                                       em.Documents.Where(d1 => psDefIds.Contains(d1.Def_Id ?? Guid.Empty) &&
                                           (d1.Deleted == null || d1.Deleted == false).Select(d1 => d1.Id))).
                               )*/
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (var dataContext = new DataContext())
            {
                var excludeStatusId = new Guid("{04452CFA-6230-48C4-BB21-4CCBCF0845D3}");
                var childAccountDefId = new Guid("{DC974E79-A44C-42A2-AE77-02F953ABE936}");
                var em = dataContext.Entities;
                /*            var accounts =
                                em.Enum_Attributes.Where(a => a.Def_Id == excludeStatusId && a.Expired >= MaxDate &&
                                    a.Document.Def_Id == childAccountDefId).Select(ea => ea.Document_Id);*/
                var accounts = em.Documents.Where(d => d.Def_Id == childAccountDefId).Select(d => d.Id);

                var enumRepo = new EnumRepository(dataContext);

                var sexList = enumRepo.GetEnumItems("Sex");
                var nationList = enumRepo.GetEnumItems("Nationalities");
                var regions = new string[]
                                  {"Бишкек", "Чуйская", "Ошская", "Ош", "Баткен", "Ыссык", "Нарын", "Талас", "Джалал"};

                var sexCountList = new Dictionary<int, int>();
                var ageCountList = new Dictionary<int, Dictionary<int, int>>();
                var nationCountList = new Dictionary<Guid?, Dictionary<int, int>>();
                var regionCountList = new Dictionary<string, Dictionary<int, int>>();

                foreach (var accountId in accounts)
                {
                    var account = new DynaDoc(accountId, Guid.Empty, dataContext);

                    var status = account.Doc["ChildStatus"];
                    if (status == null || (Guid) status != excludeStatusId)
                    {
                        var personDoc = account.GetAttrDoc("Person");
                        if (personDoc == null) continue;
                        
                        var person = new DynaDoc(personDoc, Guid.Empty, dataContext);

                        var sexId = (Guid?) person.Doc["Sex"];

                        int sex = sexId == new Guid("{C3DCB977-2781-418A-BB96-12FE7F3F041B}")
                                      ? 2
                                      : sexId == new Guid("{BC064CB6-0EF7-4535-9208-4288EA6EFD21}") ? 1 : 0;

                        if (sexCountList.ContainsKey(sex))
                            sexCountList[sex]++;
                        else
                            sexCountList.Add(sex, 1);

                        var birthDate = person.Doc["BirthDate"];
                        int age = birthDate != null ? (DateTime.Today - (DateTime) birthDate).Days/365 : 0;

                        Dictionary<int, int> rec;
                        if (!ageCountList.ContainsKey(age))
                        {
                            rec = new Dictionary<int, int>();
                            rec.Add(0, 0);
                            rec.Add(1, 0);
                            rec.Add(2, 0);
                            ageCountList.Add(age, rec);
                        }
                        rec = ageCountList[age];
                        rec[sex]++;

                        var nation = (Guid) (person.Doc["Nationality"] ?? Guid.Empty);


                        if (!nationCountList.ContainsKey(nation))
                        {
                            rec = new Dictionary<int, int> {{0, 0}, {1, 0}, {2, 0}};
                            nationCountList.Add(nation, rec);
                        }
                        rec = nationCountList[nation];
                        rec[sex]++;

                        var address = account.Doc["Address"];

                        if (address != null)
                        {
                            var s = address.ToString().ToUpper();

                            foreach (var region in regions)
                            {
                                if (s.ToUpper().Contains(region.ToUpper()))
                                {
                                    if (!regionCountList.ContainsKey(region))
                                    {
                                        rec = new Dictionary<int, int>();
                                        rec.Add(0, 0);
                                        rec.Add(1, 0);
                                        rec.Add(2, 0);
                                        regionCountList.Add(region, rec);
                                    }

                                    rec = regionCountList[region];
                                    rec[sex]++;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (!regionCountList.ContainsKey(""))
                            {
                                rec = new Dictionary<int, int>();
                                rec.Add(0, 0);
                                rec.Add(1, 0);
                                rec.Add(2, 0);
                                regionCountList.Add("", rec);
                            }
                            rec = regionCountList[""];
                            rec[sex]++;
                        }
                    }
                }

                const string title =
                    "Отчетные данные по государственному банку данных о детях оставшихся без попечения родителей";
                var report = new ExcelReport(title, "report", "Отчет");

                report.AddCell(title, 0, 0, TextStyle.BoldText);
                int row = 1;
                report.AddCell("№", 0, row, TextStyle.BoldText);
                report.AddCell("Наименование", 1, row, TextStyle.BoldText);
                report.SetColumnWidth(1, 18);
                report.AddCell("Мальчики", 2, row, TextStyle.BoldText);
                report.SetColumnWidth(2, 10);
                report.AddCell("Девочки", 3, row, TextStyle.BoldText);
                report.SetColumnWidth(3, 10);
                report.AddCell("Не указано", 4, row, TextStyle.BoldText);
                report.SetColumnWidth(4, 10);
                report.AddCell("Итого", 5, row, TextStyle.BoldText);
                report.SetColumnWidth(5, 10);
                row++;
                report.AddCell("По возрасту:", 0, row, TextStyle.BoldText);
                row++;
                Dictionary<int, int> val;
                foreach (var age in ageCountList.OrderBy(p => p.Key))
                {
                    report.AddCell(age.Key.ToString(), 1, row, TextStyle.NormalText);
                    val = age.Value;
                    report.AddCell(val[2], 2, row, TextStyle.NormalText);
                    report.AddCell(val[1], 3, row, TextStyle.NormalText);
                    report.AddCell(val[0], 4, row, TextStyle.NormalText);
                    report.AddCell(val[2] + val[1] + val[0], 5, row, TextStyle.BoldText);
                    row++;
                }
                report.AddCell("По национальности:", 0, row, TextStyle.BoldText);
                row++;
                foreach (var nation in nationCountList)
                {
                    var enumValue = nationList.FirstOrDefault(i => i.Id == (Guid) nation.Key);
                    var name = enumValue != null ? enumValue.Value : "Не указано";

                    report.AddCell(name, 1, row, TextStyle.NormalText);
                    val = nation.Value;
                    report.AddCell(val[2], 2, row, TextStyle.NormalText);
                    report.AddCell(val[1], 3, row, TextStyle.NormalText);
                    report.AddCell(val[0], 4, row, TextStyle.NormalText);
                    report.AddCell(val[2] + val[1] + val[0], 5, row, TextStyle.BoldText);
                    row++;
                }
                report.AddCell("По месту нахождения:", 0, row, TextStyle.BoldText);
                row++;
                foreach (var region in regionCountList)
                {
                    report.AddCell(String.IsNullOrEmpty(region.Key) ? "Не указано" : region.Key, 1, row,
                                   TextStyle.NormalText);
                    val = region.Value;
                    report.AddCell(val[2], 2, row, TextStyle.NormalText);
                    report.AddCell(val[1], 3, row, TextStyle.NormalText);
                    report.AddCell(val[0], 4, row, TextStyle.NormalText);
                    report.AddCell(val[2] + val[1] + val[0], 5, row, TextStyle.BoldText);
                    row++;
                }
                report.AddCell("Всего:", 0, row, TextStyle.BoldText);
                row++;
                int totalSum = 0;
                if (sexCountList.ContainsKey(2))
                {
                    var total = sexCountList[2];
                    report.AddCell(total.ToString(), 2, row, TextStyle.NormalText);
                    totalSum += total;
                }
                if (sexCountList.ContainsKey(1))
                {
                    var total = sexCountList[1];
                    report.AddCell(total.ToString(), 3, row, TextStyle.NormalText);
                    totalSum += total;
                }
                if (sexCountList.ContainsKey(0))
                {
                    var total = sexCountList[0];
                    report.AddCell(total.ToString(), 4, row, TextStyle.NormalText);
                    totalSum += total;
                }
                report.AddCell(totalSum.ToString(), 5, row, TextStyle.BoldText);

                report.ExcelSaveToFile(@"c:\\distr\\cissa\\ChildrenReport.xls");
            }
        }
    }
}
