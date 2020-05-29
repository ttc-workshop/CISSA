using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Reports;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.Cissa.Report;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cissa.Tests.ReportTest
{
    [TestClass]
    public class DocumentListReportTest
    {
        private readonly Guid _userId = Guid.Parse("180B1E71-6CDA-4887-9F83-941A12D7C979");

        [TestMethod]
        public void DocReportTest()
        {
            var docDefId = Guid.Parse("{4455B9CB-2564-4A92-A295-E3C0BEDB7AC2}");

            using (var dataContext = new DataContext())
            {
                var docRepo = new DocRepository(dataContext, _userId);

                Doc templateDoc = docRepo.New(docDefId);
                int pageCount = 0;
                List<Guid> docIds = docRepo.List(out pageCount, docDefId, 1, 0);

                List<Doc> docs = docIds.Select(docRepo.LoadById).ToList();

                var report = new ExcelDocListReport("Тестовое название отчета", docs, templateDoc);

                report.SaveToExcelFile(@"C:\DocumentList.xls");
            }
        }


        [TestMethod]
        public void TableReportTest()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new ReportRepository(dataContext);
                var reportId = Guid.Parse("295690be-7d94-43f4-bfc8-37b2fcc936c5");
                BizTableReport tableReport = repo.GetReport(reportId);

                var docDefId = Guid.Parse("{4455B9CB-2564-4A92-A295-E3C0BEDB7AC2}");
                var docRepo = new DocRepository(dataContext, _userId);
                int pageCount = 0;
                List<Guid> docIds = docRepo.List(out pageCount, docDefId, 1, 0);

                List<Doc> docs = docIds.Select(id => docRepo.LoadById(id)).ToList();

                var report = new ExcelTableReport("Тестовое название отчета", docs, tableReport);

                report.SaveToExcelFile(@"c:\TableReportTest.xls");
            }
        }

        [TestMethod]
        public void TableReportTestWithAgregate()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new ReportRepository(dataContext);
                var reportId = Guid.Parse("{771E695E-2379-4E61-8F9E-F2E28E2D8933}");
                BizTableReport tableReport = repo.GetReport(reportId);

                var docDefId = Guid.Parse("{846B1B55-F110-452F-B08F-8CEB0A112BE0}");
                var docRepo = new DocRepository(dataContext, _userId);
                int pageCount = 0;
                List<Guid> docIds = docRepo.List(out pageCount, docDefId, 1, 0);

                List<Doc> docs = docIds.Select(id => docRepo.LoadById(id)).ToList();

                var report = new ExcelTableReport("Отчет с агрегатами", docs, tableReport);

                report.SaveToExcelFile(@"c:\TableReportAggregates.xls");
            }
        }
       
        [TestMethod]
        public void TableReportTest3()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new ReportRepository(dataContext);
                var reportId = Guid.Parse("{DFBE1017-A975-4A46-9FB3-970B4DC1C67E}");
                BizTableReport tableReport = repo.GetReport(reportId);

                var docDefId = Guid.Parse("{50E90782-110C-4AA0-B4CE-7CB233766F99}");
                var docRepo = new DocRepository(dataContext, _userId);
                int pageCount = 0;
                List<Guid> docIds = docRepo.List(out pageCount, docDefId, 1, 0);

                List<Doc> docs = docIds.Select(id => docRepo.LoadById(id)).ToList();

                var report = new ExcelTableReport("Тестовое название отчета", docs, tableReport);

                report.SaveToExcelFile(@"c:\TableReportTest3.xls");
            }
        }

    }
}
