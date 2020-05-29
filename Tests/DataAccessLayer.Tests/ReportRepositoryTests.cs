using System;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Reports;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    [TestClass]
    public class ReportRepositoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ApplicationException), "Отчета с идентификатором 00000000-0000-0000-0000-000000000000 не существует")]
        public void TextExists()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new ReportRepository(dataContext);
                var reportId = Guid.Empty;

                BizTableReport tableReport = repo.GetReport(reportId);

                Assert.IsNotNull(tableReport);
            }
        }

        [TestMethod]
        public void TestNotNull()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new ReportRepository(dataContext);
                var reportId = Guid.Parse("295690be-7d94-43f4-bfc8-37b2fcc936c5");

                BizTableReport tableReport = repo.GetReport(reportId);

                Assert.IsNotNull(tableReport);
            }
        }

        [TestMethod]
        public void TestReportHeader()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new ReportRepository(dataContext);
                var reportId = Guid.Parse("295690be-7d94-43f4-bfc8-37b2fcc936c5");

                BizTableReport tableReport = repo.GetReport(reportId);

                Assert.IsNotNull(tableReport);
                Assert.IsNotNull(tableReport.ReportHeaders);
                Assert.AreNotEqual(0, tableReport.ReportHeaders.Count);
            }
        }

        [TestMethod]
        public void TestReportFotter()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new ReportRepository(dataContext);
                var reportId = Guid.Parse("295690be-7d94-43f4-bfc8-37b2fcc936c5");

                BizTableReport tableReport = repo.GetReport(reportId);

                Assert.IsNotNull(tableReport);
                Assert.IsNotNull(tableReport.ReportFooters);
                Assert.AreNotEqual(0, tableReport.ReportFooters.Count);
            }
        }

        [TestMethod]
        public void TestPageHeader()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new ReportRepository(dataContext);
                var reportId = Guid.Parse("295690be-7d94-43f4-bfc8-37b2fcc936c5");

                BizTableReport tableReport = repo.GetReport(reportId);

                Assert.IsNotNull(tableReport);
                Assert.IsNotNull(tableReport.PageHeaders);
                Assert.AreNotEqual(0, tableReport.PageHeaders.Count);
            }
        }

        [TestMethod]
        public void TestPageFooter()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new ReportRepository(dataContext);
                var reportId = Guid.Parse("295690be-7d94-43f4-bfc8-37b2fcc936c5");

                BizTableReport tableReport = repo.GetReport(reportId);

                Assert.IsNotNull(tableReport);
                Assert.IsNotNull(tableReport.PageFooters);
                Assert.AreNotEqual(0, tableReport.PageFooters.Count);
            }
        }

        [TestMethod]
        public void TestDetail()
        {
            using (var dataContext = new DataContext())
            {
                var repo = new ReportRepository(dataContext);
                var reportId = Guid.Parse("295690be-7d94-43f4-bfc8-37b2fcc936c5");

                BizTableReport tableReport = repo.GetReport(reportId);

                Assert.IsNotNull(tableReport);
                Assert.IsNotNull(tableReport.ReportDetails);
                Assert.AreNotEqual(0, tableReport.ReportDetails.Count);
            }
        }
    }
}
