using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Reports
{
    public class ReportRepository : IReportRepository
    {
        #region IReportRepository Members

        public IDataContext DataContext { get; private set; }

        public ReportRepository(IDataContext dataContext)
        {
            DataContext = dataContext;
        }

        /// <summary>
        /// Получаем общий отчет по идентификатору 
        /// </summary>
        /// <param name="reportId">Идентификатор отчета</param>
        /// <returns>Отчет</returns>
        public BizTableReport GetReport(Guid reportId)
        {
            var em = DataContext.GetEntityDataContext().Entities;
            IQueryable<Table_Report> queryReport = from r in em.Object_Defs.OfType<Table_Report>()
                                                   where r.Id == reportId
                                                   select r;

            if (!queryReport.Any())
            {
                throw new ApplicationException(
                    string.Format("Отчета с идентификатором {0} не существует", reportId));
            }

            var dbReport = queryReport.First();

            var reportReturn = new BizTableReport
                                   {
                                       Id = dbReport.Id,
                                       DocDef = new DocDef
                                                    {
                                                        Id = dbReport.Document_Defs.Id,
                                                        Name = dbReport.Document_Defs.Name
                                                    }
                                   };

            /* 1-Report Detail; 2-Report Header; 3-Report Footer; 4-Page Header; 5-Page Footer; */

            foreach (
                Report_Section detailSection in
                    dbReport.Children.OfType<Report_Section>().Where(s => s.Type_Id == 1))
            {
                var reportDetail = new BizReportDetail
                                       {
                                           Id = detailSection.Id,
                                           Children = GetBandItems(detailSection)
                                       };

                reportReturn.ReportDetails.Add(reportDetail);
            }

            foreach (Report_Section section in dbReport.Children.OfType<Report_Section>().Where(s => s.Type_Id == 2))
            {
                reportReturn.ReportHeaders.Add(section.Description);
            }

            foreach (Report_Section section in dbReport.Children.OfType<Report_Section>().Where(s => s.Type_Id == 3))
            {
                reportReturn.ReportFooters.Add(section.Description);
            }

            foreach (Report_Section section in dbReport.Children.OfType<Report_Section>().Where(s => s.Type_Id == 4))
            {
                reportReturn.PageHeaders.Add(section.Description);
            }

            foreach (Report_Section section in dbReport.Children.OfType<Report_Section>().Where(s => s.Type_Id == 5))
            {
                reportReturn.PageFooters.Add(section.Description);
            }


            return reportReturn;
        }

        #endregion

        private static IList<IDetailItem> GetBandItems(Control control)
        {
            if (!control.Children.IsLoaded) control.Children.Load();

            if (!control.Children.Any())
            {
                return null;
            }

            var children = new List<IDetailItem>();

            foreach (var objectDef in control.Children)
            {
                var childControl = (Control) objectDef;
                if (childControl is Report_Editor)
                {
                    var dbColumn = childControl as Report_Editor;

                    var column = new ReportColumn
                                     {
                                         AttributeId = dbColumn.Attribute_Id ?? Guid.Empty,
                                         Width = (float) (dbColumn.Width ?? 0),
                                         Text = dbColumn.Name,
                                         AggregateOperation =
                                             dbColumn.Summary_Id == null
                                                 ? AggregateOperation.None
                                                 : (AggregateOperation) dbColumn.Summary_Id
                                     };

                    children.Add(column);
                }
                else if (childControl is Report_Band)
                {
                    var dbBand = childControl as Report_Band;
                    var band = new ReportBand
                                   {
                                       Id = dbBand.Id,
                                       Children = GetBandItems(childControl),
                                       Text = dbBand.Name
                                   };

                    children.Add(band);
                }
            }

            return children;
        }
    }
}