using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IFormRepository
    {
        BizDetailForm FindDetailForm(Guid formId);
        BizTableForm FindTableForm(Guid formId);
        BizForm FindForm(Guid formId);

        /// <summary>
        /// Получает форму (любую)
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Форма</returns>
        BizForm GetForm(Guid formId, int languageId = 0);

        /// <summary>
        /// Получает детальную форму
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Форма</returns>
        BizDetailForm GetDetailForm(Guid formId, int languageId = 0);

        BizDetailForm GetDetailFormWithData(Guid formId, Guid docId, int languageId = 0);

        /// <summary>
        /// Получает табличную форму
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Табличная форма</returns>
        BizTableForm GetTableForm(Guid formId, int languageId = 0);
/*
        /// <summary>
        /// Получает форму для редактирования или создания документа
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="documentId">Идентификатор документа или Guid.Empty (для создания нового документа)</param>
        /// <param name="withoutChildrenDocumentsList">Без атрибутов "Список документов" Не то рекурсия!!!</param>
        /// <returns>Форма</returns>
        BizForm GetForm(Guid formId, Guid documentId, bool withoutChildrenDocumentsList);

        /// <summary>
        /// Получает форму для редактирования переданного документа
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="document">Открываемый документ в форме</param>
        /// <returns>Форма</returns>
        BizForm GetForm(Guid formId, Doc document);

        /// <summary>
        /// Получает табличную форму
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="documentsIds">Список идентификаторов документов</param>
        /// <returns>Табличная форма</returns>
        BizTableForm GetTableForm(Guid formId, IEnumerable<Guid> documentsIds);
*/
        /// <summary>
        /// Записывает значения атрибутов документа в форму
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="document">Документ</param>
        /// <returns>Форма с данными</returns>
        BizControl SetFormDoc(BizControl form, Doc document);

        /// <summary>
        /// Записывает значения из формы в документ
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="document">Документ</param>
        /// <returns>Измененный документ</returns>
        Doc GetFormDoc(BizControl form, Doc document);

        /// <summary>
        /// Записывает значения из документа в контрол
        /// </summary>
        /// <param name="control">Контрол</param>
        /// <param name="document">Документ</param>
        /// <returns></returns>
//        void SetControlData(BizControl control, Doc document);

        /// <summary>
        /// Возвращает список форм со строковыми данными для отображения табличных форм
        /// </summary>
        /// <param name="form">Табличная форма</param>
        /// <param name="docIds">Список Ид документов</param>
        /// <returns>Список форм со строковыми данными</returns>
        List<BizControl> GetTableFormRows(BizTableForm form, List<Guid> docIds);

        /// <summary>
        /// Возвращает список форм со строковыми данными для отображения табличных форм
        /// </summary>
        /// <param name="count">Кол-во строк</param>
        /// <param name="form">Табличная форма</param>
        /// <param name="docStateId">Статус отображаемых документов</param>
        /// <param name="filter">Данные фильтра</param>
        /// <param name="sortAttrs">Сортировка</param>
        /// <param name="pageNo">Номер страницы</param>
        /// <param name="pageSize">Размер страницы - кол-во строк</param>
        /// <returns>Список форм со строковыми данными</returns>
        List<BizControl> GetTableFormRows(out int count, BizForm form, Guid? docStateId, BizForm filter,
                                          IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);
        List<BizControl> GetTableFormRows(BizForm form, Guid? docStateId, BizForm filter,
                                          IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);
        int GetTableFormRowCount(BizForm form, Guid? docStateId, BizForm filter);

        List<BizControl> GetTableFormRows(out int count, BizForm form, QueryDef def,
                                          IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);
        List<BizControl> GetTableFormRows(out int count, BizForm form, QueryDef def, BizForm filter,
                                          IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);
        List<BizControl> GetTableFormRows(BizForm form, QueryDef def,
                                          IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);
        List<BizControl> GetTableFormRows(BizForm form, QueryDef def, BizForm filter,
                                          IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);
        int GetTableFormRowCount(BizForm form, QueryDef def);
        int GetTableFormRowCount(BizForm form, QueryDef def, BizForm filter);

        List<BizControl> GetTableFormRows(BizForm form, IEnumerable<Guid> docIds,
                                          IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize);
        List<BizControl> GetTableFormRows(BizForm form, IEnumerable<Doc> docs, int pageNo, int pageSize);
        List<BizControl> GetDocListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo,
            int pageSize);
        List<BizControl> GetDocListTableFormRows(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize);
        int GetDocListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId);

        List<BizControl> GetRefListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo,
                                                 int pageSize);
        List<BizControl> GetRefListTableFormRows(BizForm form, Guid docId, Guid attrDefId, int pageNo,
                                                 int pageSize);
        int GetRefListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId);
        /*
                            /// <summary>
                            /// Сохраняет данные на форме в БД
                            /// </summary>
                            /// <param name="form">Сохраняемая форма</param>
                            /// <returns>Сохраненная форма</returns>
                            BizForm SaveForm(BizForm form);

                            /// <summary>
                            /// Сохраняет данные на форме в БД
                            /// </summary>
                            /// <param name="form">Сохраняемая форма</param>
                            /// <returns>Сохраненная форма</returns>
                            Doc GetDoc(BizForm form);
                    */
        /// <summary>
        /// Возвращает дерево меню
        /// </summary>
        /// <param name="languageId">Язык</param>
        /// <returns>Список меню</returns>
        List<BizMenu> GetMenus(int languageId = 0);

        /// <summary>
        /// Переводит заголовки на заданный язык
        /// </summary>
        /// <param name="form">Форма</param>
        /// <param name="languageId">Язык</param>
        void TranslateForm(BizForm form, int languageId);

        /// <summary>
        /// Переводит список меню на заданный язык
        /// </summary>
        /// <param name="menus">Форма</param>
        /// <param name="languageId">Язык</param>
        void TranslateMenus(List<BizMenu> menus, int languageId);

        /// <summary>
        /// Возвращает сообщения об ошибках для заданной формы из общего списка ошибок
        /// </summary>
        /// <param name="form">Форма, для которой необходимо получить все ошибки</param>
        /// <param name="errors">Общий список ошибок</param>
        /// <returns>Ошибки</returns>
        IList<ModelMessage> GetFormErrors(BizForm form, IList<ModelMessage> errors);

        BizForm SetFormOptions(BizForm form, IList<BizControlOption> options);

        // New 2015-07-13
        // IList<EnumValue> GetFormComboBoxValueList(BizForm form, BizComboBox comboBox);
    }
}
