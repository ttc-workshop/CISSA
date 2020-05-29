using System;
using System.Collections.Generic;
using System.ServiceModel;
using Intersoft.Cissa.Report.Context;
using Intersoft.Cissa.Report.Defs;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.BizService.Interfaces
{
    /// <summary>
    /// Интерфейс работы с файлами и формирования выходных документов в форматах PDF и Excel
    /// </summary>
    [ServiceContract]
    interface IReportManager
    {
        /// <summary>
        /// Формирует выходной документ из шаблона на основе документа и данных рабочего контекста
        /// </summary>
        /// <param name="document">Документ, из которого берутся данные для подстановки в шаблон</param>
        /// <param name="contextData">Контекст бизнес процесса, из которого берутся данные для подстановки в шаблон</param>
        /// <param name="fileName">Наименование файла - шаблона выходного документа в форматах (Excel, PDF)</param>
        /// <returns>Массив двоичных данных представляющих собой выходной файл</returns>
        [OperationContract]
        byte[] GenerateFromTemplate(Doc document, WorkflowContextData contextData, string fileName);

        /// <summary>
        /// Получает файл по имени
        /// </summary>
        /// <param name="fileName">Имя файла находящегося на сервере</param>
        /// <returns>Массив двоичных данных тела файла</returns>
        [OperationContract]
        byte[] GetFile(string fileName);

        /// <summary>
        /// Формирует Excel документ из формы и запоса выборки данных 
        /// </summary>
        /// <param name="form">Форма, задающая оформление данных в выходном документе</param>
        /// <param name="queryDef">Запрос для выборки данных из БД</param>
        /// <returns>Массив двоичных данных тела Excel файла</returns>
        [OperationContract]
        byte[] ExcelFromQuery(BizForm form, QueryDef queryDef);

        /// <summary>
        /// Формирует Excel документ из формы и списка документов
        /// </summary>
        /// <param name="form">Форма, задающая оформление данных в выходном документе</param>
        /// <param name="docIdList">Список идентификаторов документов, которые необходимо вывести в файл</param>
        /// <returns>Массив двоичных данных тела Excel файла</returns>
        [OperationContract]
        byte[] ExcelFromDocIdList(BizForm form, List<Guid> docIdList);

        /// <summary>
        /// Формирует Excel документ из формы и критериев выборки
        /// </summary>
        /// <param name="form">Форма, задающая оформление данных в выходном документе</param>
        /// <param name="docStateId">Идентификатор статуса документа в качестве критерия выборки документов из БД</param>
        /// <param name="filter">Форма фильтра с данными для ограничения выборки документов</param>
        /// <returns>Массив двоичных данных тела Excel файла</returns>
        [OperationContract]
        byte[] ExcelFromFilter(BizForm form, Guid? docStateId, Doc filter);

        /// <summary>
        /// Формирует Excel документ из формы и критериев выборки
        /// </summary>
        /// <param name="form">Форма, задающая оформление данных в выходном документе</param>
        /// <param name="docStateId">Идентификатор статуса документа в качестве критерия выборки документов из БД</param>
        /// <param name="filter">Форма фильтра с данными для ограничения выборки документов</param>
        /// <returns>Массив двоичных данных тела Excel файла</returns>
        [OperationContract]
        byte[] ExcelFromFormFilter(BizForm form, Guid? docStateId, BizForm filter);

        /// <summary>
        /// Формирует Excel документ для вложенной формы списка документов
        /// </summary>
        /// <param name="documentId">Идентификатор документа</param>
        /// <param name="docListForm">Форма вложенного списка</param>
        /// <returns></returns>
        [OperationContract]
        byte[] ExcelFromDocumentListForm(Guid documentId, BizDocumentListForm docListForm);

        [OperationContract]
        byte[] ExcelFromDetailForm(BizForm form, Doc doc);

        /// <summary>
        /// Создает контекст отчета по Id базового документа
        /// </summary>
        /// <param name="docDefId"></param>
        /// <returns></returns>
        [OperationContract]
        TableReportContext CreateTableReport(Guid docDefId);

        [OperationContract]
        TableReportContext JoinTableReportSource(TableReportContext context, Guid masterSourceId, Guid docDefId, Guid attrDefId);

        [OperationContract]
        TableReportContext RemoveTableReportSource(TableReportContext context, Guid sourceId);

        [OperationContract]
        TableReportContext AddTableReportColumn(TableReportContext context, Guid sourceId, Guid attrDefId);
        [OperationContract]
        TableReportContext AddTableReportColumns(TableReportContext context, Guid sourceId, Guid[] attrDefIds);

        [OperationContract]
        TableReportContext RemoveTableReportColumn(TableReportContext context, Guid columnId);

        [OperationContract]
        TableReportContext AddTableReportCondition(TableReportContext context, Guid leftSourceId, Guid leftAttrId, ReportConditionDefType type = ReportConditionDefType.Param);
        /*[OperationContract]
        TableReportContext AddTableReportCondition(TableReportContext context, Guid sourceId, Guid attrDefId);
        [OperationContract]
        TableReportContext AddTableReportCondition(TableReportContext context, Guid sourceId, Guid attrDefId);*/

        [OperationContract]
        TableReportContext RemoveTableReportCondition(TableReportContext context, Guid conditionId);

        [OperationContract]
        TableReportContext AddTableReportExpCondition(TableReportContext context);

        [OperationContract]
        byte[] ExecuteTableReport(TableReportContext context);

        [OperationContract]
        byte[] SerializeTableReport(TableReportContext context);

        [OperationContract]
        TableReportContext DeserializeTableReport(byte[] data);
    }
}