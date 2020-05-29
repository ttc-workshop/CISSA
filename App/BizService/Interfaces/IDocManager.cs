using System;
using System.Collections.Generic;
using System.ServiceModel;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.BizService.Interfaces
{
    /// <summary>
    /// Интерфейс работы с документами
    /// </summary>
    [ServiceContract]
    public interface IDocManager
    {
        /// <summary>
        /// Загружает документ по идентификатору
        /// </summary>
        /// <param name="documentId">Идентификатор загружаемого документа</param>
        /// <returns>Загруженный документ</returns>
        [OperationContract]
        Doc DocumentLoad(Guid documentId);

        /// <summary>
        /// Созадает новый документ по идентификатору типа документа
        /// </summary>
        /// <param name="docDefId">Идентификатор типа документа</param>
        /// <returns>Новый не инициализированный документ</returns>
        [OperationContract]
        Doc DocumentNew(Guid docDefId);

        /// <summary>
        /// Инициализация полей документа
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [OperationContract]
        Doc DocumentInit(Doc doc, WorkflowContextData data);
/*
        /// <summary>
        /// Загружает список документов на основании фильтра
        /// </summary>
        /// <param name="searchParameters">Список параметров</param>
        /// <param name="logicOperation">Логическая операция объединения параметров</param>
        /// <returns>Список найденых документов</returns>
        [OperationContract]
        List<Guid> DocumentSearch(List<SearchParameter> searchParameters, LogicOperation logicOperation);
*/

        /// <summary>
        /// Сохраняет документ
        /// </summary>
        /// <param name="doc">Сохраняемый дкоумент</param>
        /// <returns>Сохраненный документ</returns>
        [OperationContract]
        Doc DocumentSave(Doc doc);

        /// <summary>
        /// Удаляет документ
        /// </summary>
        /// <param name="documentId">Идентификатор удаляемого документа</param>
        [OperationContract]
        void DocumentDelete(Guid documentId);

        /// <summary>
        /// Возвращает историю состояний документа
        /// </summary>
        /// <param name="docId">Идентификатор документа</param>
        /// <returns>Список состояний документа</returns>
        [OperationContract]
        List<DocState> DocumentStateList(Guid docId);

        /// <summary>
        /// Загружает справочник
        /// </summary>
        /// <param name="enumId">Идентификатор справочника</param>
        /// <returns>Список значений справочника</returns>
        [OperationContract]
        List<EnumValue> GetEnumList(Guid enumId);

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageCount">Кол-во страниц</param>
        /// <returns>Список документов</returns>
        [OperationContract]
        List<Guid> DocumentList(out int pageCount, Guid docDefId, int pageNo, int pageSize = 0);

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttr">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        [OperationContract]
        List<Guid> DocumentFilterList(out int count, Guid docDefId, int pageNo, int pageSize = 0, Doc filter = null,
                                Guid? sortAttr = null);

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="docStateId">Статус документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttr">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        [OperationContract]
        List<Guid> DocumentStateFilterList(out int count, Guid docDefId, Guid docStateId, int pageNo, int pageSize, Doc filter = null,
                        Guid? sortAttr = null);

        /// <summary>
        /// Загружает список документов из атрибута
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="document">ID Документа</param>
        /// <param name="attrDefId">Класс атрибута</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="filter">Фильтр</param>
        /// <param name="sortAttr">Атрибут сортировки</param>
        /// <returns>Список документов</returns>
        [OperationContract]
        List<Guid> DocAttrList(out int count, Doc document, Guid attrDefId, int pageNo, int pageSize, Doc filter = null,
                               Guid? sortAttr = null);

        /// <summary>
        /// Загружает список документов из атрибута
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docId">Id Документа</param>
        /// <param name="attrDefId">Класс атрибута</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="filter">Фильтр</param>
        /// <param name="sortAttr">Атрибут сортировки</param>
        /// <returns>Список документов</returns>
        [OperationContract]
        List<Guid> DocAttrListById(out int count, Guid docId, Guid attrDefId, int pageNo, int pageSize, Doc filter = null,
                               Guid? sortAttr = null);

        /// <summary>
        /// Возвращает вложенный документ
        /// </summary>
        /// <param name="document">Текущий документ</param>
        /// <param name="docAttr">Атрибут ссылающийся на вложенный документ</param>
        /// <returns></returns>
        [OperationContract]
        Doc GetNestingDocument(Doc document, DocAttribute docAttr);

        /// <summary>
        /// Проверяет сохранен ли документ в БД
        /// </summary>
        /// <param name="document">Документ</param>
        /// <returns>Истина - сохранен</returns>
        [OperationContract]
        bool DocIsStored(Doc document);

        /// <summary>
        /// Проверяет содержится ли документ в списке
        /// </summary>
        /// <param name="docId">Идентификатор документа, который нужно проверить на вхождение в список</param>
        /// <param name="attrDocId">Идентификатор документа которому принадлежит список</param>
        /// <param name="attrDefId">Идентификатор атрибута-списка</param>
        /// <returns>Истина - имеется в списке</returns>
        [OperationContract]
        bool ExistsInDocList(Guid docId, Guid attrDocId, Guid attrDefId);

        /// <summary>
        /// Добавить документ в список
        /// </summary>
        /// <param name="docId">Идентификатор документа, который необходимо добавить в список</param>
        /// <param name="document">Документ содержащий список</param>
        /// <param name="attrDefId">Идентификатор списка</param>
        /// <returns>Текущий документ</returns>
        [OperationContract]
        Doc AddDocToList(Guid docId, Doc document, Guid attrDefId);

        [OperationContract]
        BlobData GetDocBlob(Guid docId, Guid attrDefId);

        [OperationContract]
        BlobData GetDocImage(Guid docId, Guid attrDefId, int height = 0, int width = 0);

        [OperationContract]
        void SaveDocImage(Guid docId, Guid attrDefId, byte[] data, string fileName);

        [OperationContract]
        List<DocDefName> GetDocDefNames();
    }
}