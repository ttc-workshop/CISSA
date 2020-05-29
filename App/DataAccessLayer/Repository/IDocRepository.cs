using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IDocRepository: IDisposable
    {
        /// <summary>
        /// Сохраняет документ в БД
        /// </summary>
        /// <param name="docForSave">Сохраняемый документ</param>
        /// <returns>Сохраненный дкоумент</returns>
        Doc Save(Doc docForSave);

        /// <summary>
        /// Создает новый документ
        /// </summary>
        /// <param name="docDefId">Идентификатор описаниядокумента</param>
        /// <returns>Новый не инециализированный документ</returns>
        Doc New(Guid docDefId);

        /// <summary>
        /// Создает новый документ
        /// </summary>
        /// <param name="docDef">Описание документа</param>
        /// <returns>Новый не инициализированный документ</returns>
        Doc New(DocDef docDef);

        /// <summary>
        /// Инициализация документа из списка параметров
        /// </summary>
        /// <param name="doc">Документ</param>
        /// <param name="prms">Список параметров</param>
        /// <returns></returns>
        [Obsolete("Нигде не используется")]
        Doc InitDocFrom(Doc doc, IStringParams prms); // >> DocHelper

        /// <summary>
        /// Загружает документ из БД по идентификатору документа
        /// </summary>
        /// <param name="documentId">Идентификатор загружаемого документа</param>
        /// <returns>Загруженный документ</returns>
        Doc LoadById(Guid documentId);

        /// <summary>
        /// Загружает документ из БД по идентификатору документа
        /// </summary>
        /// <param name="documentId">Идентификатор загружаемого документа</param>
        /// <param name="forDate">Дата на которую необходимо загрузить документ</param>
        /// <returns>Загруженный документ</returns>
        Doc LoadById(Guid documentId, DateTime forDate);

        /// <summary>
        /// Загружает список документов из БД по идентификатору формы
        /// </summary>
        //// <param name="formId">Идентификатор формы</param>
        /// <returns>Список загруженных документов</returns>
        //IList<Doc> LoadByFormId(Guid formId);

        /// <summary>
        /// Удаляет документ из БД по идентификатору документа
        /// </summary>
        /// <param name="documentId">Идентификатор удаляемого документа</param>
        void DeleteById(Guid documentId);

        /// <summary>
        /// Скрывает документ
        /// </summary>
        /// <param name="documentId">Идентификатор скрываемого документа</param>
        void HideById(Guid documentId);

        /// <summary>
        /// Возвращает историю состояний документа
        /// </summary>
        /// <param name="docId">Идентификатор документа</param>
        /// <returns>Список состояний документа</returns>
        List<DocState> GetDocumentStates(Guid docId); // Добавить overload метод с параметрами fromDate, toDate

        /// <summary>
        /// Выполняет проверку переданного документа 
        /// </summary>
        /// <param name="document">Проверяемый документ</param>
        /// <returns>Проверенный и по возможности исправленный документ</returns>
        [Obsolete("Не используется")]
        Doc Check(Doc document); // >> DocHelper

        /// <summary>
        /// Подсчитывает автовычисляемые атрибуты
        /// </summary>
        /// <param name="doc">Документ в котором необходимо вычислить автовычисляемые атрибуты</param>
        /// <returns>Документ с вычисленными авто атрибутами</returns>
        [Obsolete("Нигде не используется")]
        Doc CalculateAutoAttributes(Doc doc); // Исключить из репозитария

/*
        /// <summary>
        /// Загружает список документов на основании фильтра
        /// </summary>
        /// <param name="parameters">Список параметров</param>
        /// <param name="logicOperation">Логическая операция объединения параметров</param>
        /// <param name="forDate">Дата на которую необходимо выполнить поиск документов</param>
        /// <returns>Список найденых документов</returns>
        List<Guid> Search(List<SearchParameter> parameters, LogicOperation logicOperation, DateTime forDate);

        /// <summary>
        /// Загружает список документов на основании фильтра
        /// </summary>
        /// <param name="parameters">Список параметров</param>
        /// <param name="logicOperation">Логическая операция объединения параметров</param>
        /// <returns>Список найденых документов</returns>
        List<Guid> Search(List<SearchParameter> parameters, LogicOperation logicOperation);
*/

        /// <summary>
        /// Получает текущее состояние документа для пользователя
        /// </summary>
        /// <param name="documentId">Идентификатор документа</param>
        /// <param name="forDate">Дата на которую необходимо получить состояние документа</param>
        /// <returns>Состояние документа</returns>
        DocState GetDocState(Guid documentId, DateTime forDate = new DateTime());

        /// <summary>
        /// Устанавливает состояние документа для пользователя
        /// </summary>
        //// <param name="workerId">Идентификатор пользователя</param>
        /// <param name="documentId">Идентификатор документа</param>
        /// <param name="stateTypeId">Идентификатор состояния</param>
        /// <returns></returns>
        void SetDocState(Guid documentId, Guid stateTypeId);

        /// <summary>
        /// Устанавливает состояние документа для пользователя
        /// </summary>
        //// <param name="workerId">Идентификатор пользователя</param>
        /// <param name="document">Документ</param>
        /// <param name="stateTypeId">Идентификатор состояния</param>
        /// <returns></returns>
        void SetDocState(Doc document, Guid stateTypeId);

        // TODO: Определить чем отличается от New?
        Doc CreateDoc(Guid docDefId);

        Doc CreateDoc(DocDef docDef);
            
        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageCount">Кол-во страниц</param>
        /// <returns>Список документов</returns>
        List<Guid> List(out int pageCount, Guid docDefId, int pageNo, int pageSize = 0); // >> DocListProvider?

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        List<Guid> List(out int count, Guid docDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttrId = null); // >> DocListProvider?

        /// <summary>
        /// Загружает список документов определенного класса
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docDefId">Класс документа</param>
        /// <param name="docStateId">Статус документа</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        List<Guid> List(out int count, Guid docDefId, Guid docStateId, int pageNo, int pageSize, Doc filter = null, Guid? sortAttrId = null); // >> DocListProvider?

        /// <summary>
        /// Загружает список документов определенного DocumentList_Attribute атрибута
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="document">Документ</param>
        /// <param name="attrDefId">Класс атрибута</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        List<Guid> DocAttrList(out int count, Doc document, Guid attrDefId, int pageNo, int pageSize, Doc filter = null, Guid? sortAttrId = null); // >> DocListProvider?

        /// <summary>
        /// Загружает список документов определенного DocumentList_Attribute атрибута
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="document">Документ</param>
        /// <param name="attrDefName">Класс атрибута</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        List<Guid> DocAttrList(out int count, Doc document, string attrDefName, int pageNo, int pageSize, Doc filter = null, Guid? sortAttrId = null); // >> DocQueryBuilder

        /// <summary>
        /// Загружает список документов определенного DocumentList_Attribute атрибута
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="document">Документ</param>
        /// <param name="attr">Атрибут</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        List<Guid> DocAttrList(out int count, Doc document, DocListAttribute attr, int pageNo, int pageSize, Doc filter = null, Guid? sortAttrId = null); // >> DocQueryBuilder

        /// <summary>
        /// Загружает список документов определенного DocumentList_Attribute атрибута
        /// </summary>
        /// <param name="count">Кол-во записей</param>
        /// <param name="docId">Идентификатор документа</param>
        /// <param name="attrDefId">Класс атрибута</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="sortAttrId">Атрибут сортировки</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Список документов</returns>
        List<Guid> DocAttrListById(out int count, Guid docId, Guid attrDefId, int pageNo, int pageSize, Doc filter = null, Guid? sortAttrId = null); // >> DocQueryBuilder

        /// <summary>
        /// Возвращает вложенный документ
        /// </summary>
        /// <param name="document">Текущий документ</param>
        /// <param name="docAttr">Атрибут ссылающийся на вложенный документ</param>
        /// <returns></returns>
        [Obsolete("Не используется")]
        Doc GetNestingDocument(Doc document, DocAttribute docAttr); // >> DocHelper

        /// <summary>
        /// Проверяет сохранен ли документ в БД
        /// </summary>
        /// <param name="document">Документ</param>
        /// <returns>Истина - сохранен</returns>
        bool DocIsStored(Doc document);

        /// <summary>
        /// Проверяет существует ли документ в БД
        /// </summary>
        /// <param name="docId">Id Документа</param>
        /// <returns>Истина - существует</returns>
        bool DocExists(Guid docId);

        /// <summary>
        /// Проверяет содержится ли документ в списке
        /// </summary>
        /// <param name="docId">Идентификатор документа, который нужно проверить на вхождение в список</param>
        /// <param name="attrDocId">Идентификатор документа которому принадлежит список</param>
        /// <param name="attrDefId">Идентификатор атрибута-списка</param>
        /// <returns>Истина - имеется в списке</returns>
        bool ExistsInDocList(Guid docId, Guid attrDocId, Guid attrDefId);

        /// <summary>
        /// Добавить документ в список
        /// </summary>
        /// <param name="docId">Идентификатор документа, который необходимо добавить в список</param>
        /// <param name="document">Документ содержащий список</param>
        /// <param name="attrDefId">Идентификатор списка</param>
        /// <returns>Текущий документ</returns>
        Doc AddDocToList(Guid docId, Doc document, Guid attrDefId);

        /// <summary>
        /// Добавить документ в список
        /// </summary>
        /// <param name="docId">Документ, который необходимо добавить в список</param>
        /// <param name="document">Документ содержащий список</param>
        /// <param name="attrName">Атрибут</param>
        /// <returns>Текущий документ</returns>
        Doc AddDocToList(Guid docId, Doc document, string attrName);

        /// <summary>
        /// Добавить документ в список
        /// </summary>
        /// <param name="doc">Документ, который необходимо добавить в список</param>
        /// <param name="document">Документ содержащий список</param>
        /// <param name="attr">Атрибут</param>
        /// <returns>Текущий документ</returns>
        Doc AddDocToList(Doc doc, Doc document, DocListAttribute attr);

        /// <summary>
        /// Удалить документ из списока
        /// </summary>
        /// <param name="docId">Документ, который необходимо добавить в список</param>
        /// <param name="document">Документ содержащий список</param>
        /// <param name="attr">Атрибут</param>
        /// <returns>Текущий документ</returns>
        void RemoveDocFromList(Guid docId, Doc document, DocListAttribute attr);

        /// <summary>
        /// Удалить документ из списока
        /// </summary>
        /// <param name="docId">Документ, который необходимо добавить в список</param>
        /// <param name="document">Документ содержащий список</param>
        /// <param name="attrName">Имя атрибута</param>
        /// <returns>Текущий документ</returns>
        void RemoveDocFromList(Guid docId, Doc document, string attrName);

        /// <summary>
        /// Очистить список
        /// </summary>
        /// <param name="docId">Идентификатор документа, у которого необходимо очистить список</param>
        /// <param name="attrDefId">Идентификатор списка</param>
        /// <returns>Текущий документ</returns>
        void ClearAttrDocList(Guid docId, Guid attrDefId);

        /// <summary>
        /// Подсчитать количество элементов в списке документа
        /// </summary>
        /// <param name="doc">Документ, у которого необходимо подсчитать список</param>
        /// <param name="attr">Атрибут</param>
        /// <returns>Текущий документ</returns>
        int CalcAttrDocListCount(Doc doc, DocListAttribute attr);

        double? CalcAttrDocListSum(Doc doc, DocListAttribute attr, string sumAttrName);

        /// <summary>
        /// Получает свойства документа по системному идентификатору
        /// </summary>
        /// <param name="document">Документ</param>
        /// <param name="ident">Системный идентификатор</param>
        /// <returns>Значение свойства документа</returns>
        object GetDocumentValue(Doc document, SystemIdent ident); // >> DocValueProvider

        BlobData GetBlobAttrData(Guid docId, AttrDef attrDef);
        BlobData GetBlobAttrData(Guid docId, Guid attrDefId);

        void SaveBlobAttrData(Guid docId, Guid attrDefId, byte[] data, string fileName);
        void SaveBlobAttrData(Doc doc, Guid attrDefId, byte[] data, string fileName);

        /// <summary>
        /// Копирует значения полей
        /// </summary>
        /// <param name="source">Документ-источник</param>
        /// <param name="target">Документ-приемник</param>
        void AssignDocTo(Doc source, Doc target); // >> DocHelper

        /// <summary>
        /// Заменяет атрибуты ссылки на документ с одного значения на другое
        /// </summary>
        /// <param name="docId1">Заменяемое значение атрибутов</param>
        /// <param name="docId2">Конечное значение атрибутов</param>
        void ReplaceRefToDoc(Guid docId1, Guid docId2);
    }
}
