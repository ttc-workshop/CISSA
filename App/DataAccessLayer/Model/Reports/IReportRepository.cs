using System;

namespace Intersoft.CISSA.DataAccessLayer.Model.Reports
{
    internal interface IReportRepository
    {
        /// <summary>
        /// Получаем общий отчет по идентификатору 
        /// </summary>
        /// <param name="reportId">Идентификатор отчета</param>
        /// <returns>Отчет</returns>
        BizTableReport GetReport(Guid reportId);
    }
}