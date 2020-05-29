using System;
using System.Collections.Generic;
using System.ServiceModel;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.BizService.Interfaces
{
    /// <summary>
    /// Интерфейс работы с запросами выборки данных
    /// </summary>
    [ServiceContract]
    public interface IQueryManager
    {
        /// <summary>
        /// Возвращает список документов попадающих в запрос
        /// </summary>
        /// <param name="queryDef">Запрос на выборку данных</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Количество строк на странице</param>
        /// <returns>Список идентификаторв документов</returns>
        [OperationContract]
        List<Guid> GetDocList(QueryDef queryDef, int pageNo, int pageSize);

        /// <summary>
        /// Возвращает количество и список документов попадающих в запрос
        /// </summary>
        /// <param name="count">Количество документов попадающих под запрос в БД</param>
        /// <param name="queryDef">Запрос на выборку данных</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Количество строк на странице</param>
        /// <returns>Список идентификаторв документов</returns>
        [OperationContract]
        List<Guid> GetDocListWithCount(out int count, QueryDef queryDef, int pageNo, int pageSize);

        /// <summary>
        /// Подсчитывает количество строк в запросе
        /// </summary>
        /// <param name="query">Запрос на выборку данных</param>
        /// <returns>Количество строк</returns>
        [OperationContract]
        int GetQueryCount(QueryDef query);

        /// <summary>
        /// Формирует запрос из текущего документа
        /// </summary>
        /// <param name="document">Документ на основе которого будет формироваться структура запроса</param>
        /// <returns>Запрос на выборку данных из БД</returns>
        [OperationContract]
        QueryDef QueryFromDoc(Doc document);

        /// <summary>
        /// Формирует запрос из формы
        /// </summary>
        /// <param name="form">Форма на основе которого будет формироваться структура запроса</param>
        /// <returns>Запрос на выборку данных из БД</returns>
        [OperationContract]
        QueryDef QueryFromForm(BizForm form);

        /// <summary>
        /// Создает запрос на основе структуры и статуса документа
        /// </summary>
        /// <param name="docDefId">Идентификатор класса документа</param>
        /// <param name="docStateId">Статус документа, по которому будет производится выборка</param>
        /// <returns>Запрос на выборку данных из БД</returns>
        [OperationContract]
        QueryDef CreateQuery(Guid docDefId, Guid? docStateId);
    }
}
