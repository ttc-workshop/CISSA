using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cissa.Tests.ReportTest
{
    [TestClass]
    public class XlsTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var def = new XlsDef())
            {
                // Header
                def.AddArea().AddRow().AddEmptyCell();
                def.AddArea().AddRow().AddText("Еженедельные сведения по компенсационным выплатам");
                def.AddArea().AddRow().AddEmptyCell();

                // Grid Header
                var h1 = def.AddArea().AddRow();
                h1.AddNode("виды компенсаций");
                h1.AddNode("Численность");
                h1.AddNode("Задолженность");
                var node1 = h1.AddNode(new XlsText("С начала года"));
                node1.AddColumn().AddText("Потребная сумма");
                node1.AddColumn().AddText("Профинансированно");

                var node2 = h1.AddNode(new XlsText("в текущем месяце"));
                node2.AddColumn().AddText("Потребная сумма");
                node2.AddColumn().AddText("Профинансированно");

                var node3 = h1.AddNode(new XlsText("Задолженность"));
                node3.AddColumn().AddText("от потребной суммы");
                node3.AddColumn().AddText("от месячной профинансированной суммы");

                var builder = new XlsBuilder(def);
                var workbook = builder.Build();
                using (var stream = new FileStream(@"c:\distr\cissa\testXlsDef1.xls", FileMode.Create))
                {
                    workbook.Write(stream);
                }
            }
        }

        [TestMethod]
        public void TestMethod11()
        {
            using (var def = new XlsDef())
            {
                // Header
                def.AddArea().AddRow() /*.AddEmptyCell()*/;
                def.AddArea().AddRow().AddText("Еженедельные сведения по компенсационным выплатам", 9);
                def.AddArea().AddRow().AddEmptyCell();

                // Grid Header
                var h1 = def.AddArea().AddRow();
                h1.AddNode("виды компенсаций", 0, 2);
                h1.AddNode("Численность", 0, 2);
                h1.AddNode("Задолженность", 0, 2);
                var node1 = h1.AddNode("С начала года", 2);
                node1.AddNode("Потребная сумма");
                node1.AddNode("Профинансированно");

                var node2 = h1.AddNode("в текущем месяце", 2);
                node2.AddNode("Потребная сумма");
                node2.AddNode("Профинансированно");

                var node3 = h1.AddNode("Задолженность", 2);
                node3.AddNode("от потребной суммы");
                node3.AddNode("от месячной профинансированной суммы");

                var builder = new XlsBuilder(def);
                var workbook = builder.Build();
                using (var stream = new FileStream(@"c:\distr\cissa\testXlsDef2.xls", FileMode.Create))
                {
                    workbook.Write(stream);
                }
            }
        }

        public const string ChatkalConnectionString =
            "Data Source=localhost;Initial Catalog=cissa-4atkal;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        public const string AsistConnectionString =
            "Data Source=localhost;Initial Catalog=asist_db2;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        public const string CissaConnectionString =
            "Data Source=195.38.189.100;Initial Catalog=cissa;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        [TestMethod]
        public void TestMethod2()
        {
            using (var connection = new SqlConnection(ChatkalConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var mdc = new MultiDataContext(new[] {dataContext}))
                    {
                        var factory = AppServiceProviderFactoryProvider.GetFactory();
                        using (var provider = factory.Create())
                        {
                            var defRepo = provider.Get<IDocDefRepository>(); // new DocDefRepository(dataContext);
                            var docDef = defRepo.DocDefByName("Person-Sheet");

                            using (var def = new XlsDef())
                            {
                                // Header
                                def.Style.FontName = "Arial Narrow";
                                def.AddArea().AddRow().AddEmptyCell();
                                def.AddArea().AddRow().AddText("Список граждан");
                                def.AddArea().AddRow().AddEmptyCell();

                                // Grid Header
                                var h1 = def.AddArea().AddRow();
                                foreach (var attr in docDef.Attributes)
                                {
                                    h1.AddNode(attr.Name).ShowAllBorders(true);
                                }

                                var qb = new QueryBuilder(docDef.Id);
                                qb.Where("LastName").Contains("ИВАН");

                                var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                                var query = sqlQueryBuilder.Build(qb.Def);
                                query.AddAttribute("&Id");
                                var list = new List<Guid>();
                                using (var r = new SqlQueryReader(dataContext, query))
                                    while (r.Read())
                                        list.Add(r.GetGuid(0));
                                using (var docDataSet = new DocDataSet(provider, dataContext, list, Guid.Empty))
                                {
                                    var gridRow = def.AddGrid(docDataSet).AddRow();
                                    foreach (var attr in docDef.Attributes)
                                    {
                                        gridRow.ShowAllBorders(true);
                                        gridRow.Style.AutoWidth = true;
                                        gridRow.Style.AutoHeight = true;
                                        gridRow.AddDataField(new DocDataSetField(docDataSet, attr));
                                    }

                                    var builder = new XlsBuilder(def);
                                    var workbook = builder.Build();
                                    using (
                                        var stream = new FileStream(@"c:\distr\cissa\testXlsDefPersons.xls", FileMode.Create))
                                    {
                                        workbook.Write(stream);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            using (var connection = new SqlConnection(CissaConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var formRepo = provider.Get<IFormRepository>(); //new FormRepository(dataContext);
                        var form = formRepo.GetTableForm(Guid.Parse("{B46A77AB-3F36-42CD-998A-018BE911AD16}"));

                        var defRepo = provider.Get<IDocDefRepository>(); // new DocDefRepository(dataContext);
                        var docDef = defRepo.DocDefById(form.DocumentDefId ?? Guid.Empty);

                        var qb = new QueryBuilder(docDef.Id);
                        qb.Where("PostCode").Eq("10");
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        //var query = new DocQuery(qb.Def, dataContext);
                        using (var query = sqlQueryBuilder.Build(qb.Def, form, null))
                        {
                            query.TopNo = 100;
                            using (var reader = new SqlQueryReader(dataContext, query))
                            {
                                var defBuilder = new XlsGridDefBuilder(provider, form, reader /*.All()*/);
                                var def = defBuilder.BuildFromBizForm();
                                def.Style.FontName = "Arial Narrow";
                                def.Style.VAlign = VAlignment.Middle;

                                var builder = new XlsBuilder(def);
                                var workbook = builder.Build();

                                using (
                                    var stream = new FileStream(@"c:\distr\cissa\testXlsDefApps.xls", FileMode.Create))
                                {
                                    workbook.Write(stream);
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void XlsFormDefBuilderTest()
        {
            using (var dataContext = new DataContext())
            {
                var formRepo = new FormRepository(dataContext);
                var form = formRepo.GetDetailForm(Guid.Parse("{90958557-E6B0-40A8-88D8-75B71130D5FC}"));

                var docRepo = new DocRepository(dataContext);
                var doc = docRepo.LoadById(Guid.Parse("626b662c-17a9-4f70-9a06-13d2337a681a"));

                var defBuilder = new XlsFormDefBuilder(dataContext, form, Guid.Empty);
                var def = defBuilder.Build(doc);
                def.Style.Borders = TableCellBorder.All;

                var builder = new XlsBuilder(def);
                var workbook = builder.Build();
                using (var stream = new FileStream(@"c:\distr\cissa\testXlsDefAppForm.xls", FileMode.Create))
                {
                    workbook.Write(stream);
                }
            }
        }

        [TestMethod]
        public void XlsGridReportDefBuilderTest()
        {
            using (var connection = new SqlConnection(CissaConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var formRepo = provider.Get<IFormRepository>(); //new FormRepository(dataContext);
                        var form = formRepo.GetTableForm(Guid.Parse("{B46A77AB-3F36-42CD-998A-018BE911AD16}"));

                        var defRepo = provider.Get<IDocDefRepository>(); //new DocDefRepository(dataContext);
                        var docDef = defRepo.DocDefById(form.DocumentDefId ?? Guid.Empty);

                        var qb = new QueryBuilder(docDef.Id);
                        qb.Where("PostCode").Eq("10");
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        //var query = new DocQuery(qb.Def, dataContext);
                        var query = sqlQueryBuilder.Build(qb.Def, form, null/*, Guid.Empty, dataContext*/);
                        query.TopNo = 100;
                        using (var reader = new SqlQueryReader(dataContext, query))
                        {
                            var defBuilder = new XlsGridReportDefBuilder(dataContext, form, reader /*.All()*/,
                                Guid.Empty);
                            defBuilder.AddHeaderText("Тестовый отчет").Bold().Center().FontDSize(4);
                            defBuilder.AddHeaderText("за 04.2015").Bold().Center();

                            defBuilder.AddFooterText("Подпись: ").Right().Margins(10, 10);
                            var table = defBuilder.AddFooterTable();
                            table.Bold().Center().Margins(30, 30).Style.Borders = TableCellBorder.All;
                            table.Add(0, 0, "Руководитель");
                            table.Add(1, 0, "Финансовый менеджер");
                            table.Add(2, 0, "Начальник отдела АСП");
                            table.Add(0, 1, "Подпись");
                            table.Add(1, 1, "Подпись");
                            table.Add(2, 1, "Подпись");
                            table.Add(0, 2, "Дата");
                            table.Add(1, 2, "Дата");
                            table.Add(2, 2, "Дата").Italic().Right();

                            var def = defBuilder.BuildFromBizForm();
                            def.Style.FontName = "Arial Narrow";
                            def.Style.VAlign = VAlignment.Middle;

                            var builder = new XlsBuilder(def);
                            var workbook = builder.Build();

                            using (
                                var stream = new FileStream(@"c:\distr\cissa\testXlsGridReportDefBuilder.xls",
                                    FileMode.Create))
                            {
                                workbook.Write(stream);
                            }
                        }
                    }
                }
            }
        }
    }
}
