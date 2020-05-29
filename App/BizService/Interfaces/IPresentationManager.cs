using System;
using System.Collections.Generic;
using System.ServiceModel;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Languages;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.BizService.Interfaces
{
    /// <summary>
    /// Интерфейс работы с визуальными формами
    /// </summary>
    [ServiceContract]
    public interface IPresentationManager
    {
        /// <summary>
        /// Загружает форму по идентификатору
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Код языка</param>
        /// <returns>Загруженная форма</returns>
        [OperationContract]
        BizForm GetAnyForm(Guid formId, int languageId = 0);

        /// <summary>
        /// Загружает форму по идентификатору
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Код языка</param>
        /// <returns>Загруженная форма</returns>
        [OperationContract]
        BizDetailForm GetDetailForm(Guid formId, int languageId = 0);

        /// <summary>
        /// Возвращает форму с данными по идентификатору
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="docId">Идентификатор документа</param>
        /// <param name="languageId">Код языка</param>
        /// <returns>Загруженная форма</returns>
        [OperationContract]
        BizDetailForm GetDetailFormWithData(Guid formId, Guid docId, int languageId = 0);

        /// <summary>
        /// Загружает табличную форму по идентификатору формы
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Код языка</param>
        /// <returns>Загруженная табличная форма</returns>
        [OperationContract]
        BizTableForm GetGridForm(Guid formId, int languageId);

        /// <summary>
        /// Записывает значения атрибутов документа в форму
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="document">Документ</param>
        /// <returns>Форма с данными</returns>
        [OperationContract]
        BizForm SetFormDoc(BizForm form, Doc document);

        /// <summary>
        /// Записывает значения из формы в документ
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="document">Документ</param>
        /// <returns>Измененный документ</returns>
        [OperationContract]
        Doc GetFormDoc(BizForm form, Doc document);

        /// <summary>
        /// Загружает главную форму
        /// </summary>
        /// <returns>Главная форма</returns>
        [OperationContract]
        BizForm GetMainForm();

        /// <summary>
        /// Возвращает дерево меню для пользователя
        /// </summary>
        /// <param name="languageId">Код языка</param>
        /// <returns>Список меню</returns>
        [OperationContract]
        IList<BizMenu> GetMenus(int languageId = 0);

        /// <summary>
        /// Возвращает список языков
        /// </summary>
        /// <returns>Список меню</returns>
        [OperationContract]
        IList<LanguageType> GetLanguages();

        /// <summary>
        /// Переводит форму на указанный язык
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Форма</returns>
        [OperationContract]
        BizForm TranslateForm(BizForm form, int languageId);

        /// <summary>
        /// Переводит список меню на заданный язык
        /// </summary>
        /// <param name="menus">Список меню</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Список меню</returns>
        [OperationContract]
        List<BizMenu> TranslateMenus(List<BizMenu> menus, int languageId);

        /// <summary>
        /// Переводит список пользовательских действий на заданный язык
        /// </summary>
        /// <param name="userActions">Список пользовательских действий</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Список пользовательских действий</returns>
        [OperationContract]
        List<UserAction> TranslateUserActions(List<UserAction> userActions, int languageId);
            
        /// <summary>
        /// Возвращает список строк с данными табличной формы с учетом критерием фильтра
        /// </summary>
        /// <param name="count">Возвращает количество строк данных в БД</param>
        /// <param name="form">Табличная форма, для которой нужны строки данных</param>
        /// <param name="docStateId">Статус документов, отображаемые в табличной форме</param>
        /// <param name="filter">Форма фильтра с данными</param>
        /// <param name="sortAttrs">Список атрибутов с условиями сортировки</param>
        /// <param name="pageNo">Номер отображаемой страницы</param>
        /// <param name="pageSize">Количество отображаемых строк в таблице</param>
        /// <returns>Список строк - визуальных элментов с данными</returns>
        [OperationContract]
        List<BizControl> GetTableFormRows(out int count, BizForm form, Guid? docStateId, BizForm filter,
                                          IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);

        [OperationContract]
        List<BizControl> GetTableFormRowData(BizForm form, Guid? docStateId, BizForm filter,
            IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);

        [OperationContract]
        int GetTableFormRowCount(BizForm form, Guid? docStateId, BizForm filter);

        /// <summary>
        /// Возвращает список строк табличной формы с данными, получаемыми из запроса выборки
        /// </summary>
        /// <param name="count">Количество строк попадаемых в выборку</param>
        /// <param name="form">Табличная форма</param>
        /// <param name="def">Запрос выборки</param>
        /// <param name="sortAttrs">Список атрибутов сотрировки</param>
        /// <param name="pageNo">Номер отображаемой страницы в табличной форме</param>
        /// <param name="pageSize">Количество отображаемых строк в форме</param>
        /// <returns>Список строк - визуальных элментов с данными</returns>
        [OperationContract]
        List<BizControl> GetTableFormRowsFromQuery(out int count, BizForm form, QueryDef def,
            IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);

        [OperationContract]
        List<BizControl> GetTableFormRowDataFromQuery(BizForm form, QueryDef def, IEnumerable<AttributeSort> sortAttrs,
            int pageNo, int pageSize);

        [OperationContract]
        List<BizControl> GetTableFormRowsFromFilterQuery(out int count, BizForm form, QueryDef def, BizForm filter,
            IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);

        [OperationContract]
        List<BizControl> GetTableFormRowDataFromFilterQuery(BizForm form, QueryDef def, BizForm filter, IEnumerable<AttributeSort> sortAttrs,
            int pageNo, int pageSize);

        [OperationContract]
        int GetTableFormRowCountFromQuery(BizForm form, QueryDef def);
        [OperationContract]
        int GetTableFormRowCountFromFilterQuery(BizForm form, QueryDef def, BizForm filter);

        /// <summary>
        /// Возвращает список строк табличной формы с данными, формируемые из списка документов
        /// </summary>
        /// <param name="form">Табличная форма</param>
        /// <param name="docIds">Список идентификаторов документов, которые необходимо высветить в таблице</param>
        /// <param name="sortAttrs">Список атрибутов сортировки строк</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Количество строк в странице</param>
        /// <returns>Список строк - визуальных элментов с данными</returns>
        [OperationContract]
        List<BizControl> GetTableFormRowsFromList(BizForm form, IEnumerable<Guid> docIds,
                                                  IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);

        /// <summary>
        /// Возвращает список строк табличной формы с данными, формируемые из списочного атрибута документа
        /// </summary>
        /// <param name="count">Количество документов хранящихся в списочном атрибуте</param>
        /// <param name="form">Таличная форма</param>
        /// <param name="docId">Идентификатор документа, содержащего списочный атрибут</param>
        /// <param name="attrDefId">Идентификатор списочного атрибута</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Количество отображаемых строк в таблице</param>
        /// <returns>Список строк - визуальных элментов с данными</returns>
        [OperationContract]
        List<BizControl> GetDocListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize);

        [OperationContract]
        List<BizControl> GetDocListTableFormRowData(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize);

        [OperationContract]
        int GetDocListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId);
        /// <summary>
        /// Возвращает список строк табличной формы с данными, формируемые из списка документов ссылающихся на указанный документ
        /// </summary>
        /// <param name="count">Количество документов попадающих под запрос</param>
        /// <param name="form">Табличная форма</param>
        /// <param name="docId">Идентификатор документа</param>
        /// <param name="attrDefId">Идентификатор ссылочного атрибута</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Количество строк на странице</param>
        /// <returns>Список строк - визуальных элментов с данными</returns>
        [OperationContract]
        List<BizControl> GetRefListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize);

        [OperationContract]
        List<BizControl> GetRefListTableFormRowData(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize);

        [OperationContract]
        int GetRefListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId);

        [OperationContract]
        IList<ModelMessage> GetFormErrors(BizForm form, IList<ModelMessage> errors);

        [OperationContract]
        BizForm SetFormOptions(BizForm form, IDictionary<Guid, IList<BizControlOption>> formOptions);

        [OperationContract]
        BizForm SetFormControlOptions(BizForm form, IList<BizControlOption> formOptions);

        [OperationContract]
        byte[] GetImage(Guid formId, Guid imageId, int height = 0, int width = 0);

        [OperationContract]
        IList<EnumValue> GetFormComboBoxValues(BizForm form, BizComboBox comboBox);
    }
}