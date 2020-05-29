using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.BizServiceTests.FakeRepo
{
    class FakeFormRepository : IFormRepository
    {
        /// <summary>
        /// Получает форму
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Код языка</param>
        /// <returns>Форма</returns>
        public BizForm GetForm(Guid formId, int languageId = 0)
        {
            return new BizForm();
        }

        public BizDetailForm GetDetailForm(Guid formId, int languageId = 0)
        {
            return new BizDetailForm();
        }

        public BizDetailForm GetDetailFormWithData(Guid formId, Guid docId, int languageId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получает форму для редактирования или создания документа
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="documentId">Идентификатор документа или Guid.Empty (для создания нового документа)</param>
        /// <param name="withoutChildrenDocumentsList">Без атрибутов "Список документов" Не то рекурсия!!!</param>
        /// <returns>Форма</returns>
        public BizForm GetForm(Guid formId, Guid documentId, bool withoutChildrenDocumentsList)
        {
            return new BizForm
            {
                Id = formId,
                DocumentId = documentId
            };
        }

        /// <summary>
        /// Получает форму для редактирования переданного документа
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="document">Открываемый документ в форме</param>
        /// <returns>Форма</returns>
        public BizForm GetForm(Guid formId, Doc document)
        {
            return new BizForm();
        }

        /// <summary>
        /// Получает табличную форму
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="languageId">Код языка</param>
        /// <returns>Табличная форма</returns>
        public BizTableForm GetTableForm(Guid formId, int languageId = 0)
        {
            return new BizTableForm();
        }

        public BizControl SetFormDoc(BizControl form, Doc document)
        {
            throw new NotImplementedException();
        }

        public Doc GetFormDoc(BizControl form, Doc document)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получает табличную форму
        /// </summary>
        /// <param name="formId">Идентификатор формы</param>
        /// <param name="documentsIds">Список идентификаторов документов</param>
        /// <returns>Табличная форма</returns>
        public BizTableForm GetTableForm(Guid formId, IEnumerable<Guid> documentsIds)
        {
            return new BizTableForm();
        }

        public BizForm SetFormDoc(BizForm form, Doc document)
        {
            return form;
        }

        public Doc GetFormDoc(BizForm form, Doc document)
        {
            return document;
        }

        /// <summary>
        /// Сохраняет данные на форме в БД
        /// </summary>
        /// <param name="form">Сохраняемая форма</param>
        /// <returns>Сохраненная форма</returns>
        public BizForm SaveForm(BizForm form)
        {
            return form;
        }

        /// <summary>
        /// Сохраняет данные на форме в БД
        /// </summary>
        /// <param name="form">Сохраняемая форма</param>
        /// <returns>Сохраненная форма</returns>
        public Doc GetDoc(BizForm form)
        {
            return new Doc();
        }

        public List<BizControl> GetTableFormRows(BizTableForm form, List<Guid> docIds)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRows(BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public int GetTableFormRowCount(BizForm form, Guid? docStateId, BizForm filter)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, QueryDef def, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, QueryDef def, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo,
            int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRows(BizForm form, QueryDef def, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRows(BizForm form, QueryDef def, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public int GetTableFormRowCount(BizForm form, QueryDef def)
        {
            throw new NotImplementedException();
        }

        public int GetTableFormRowCount(BizForm form, QueryDef def, BizForm filter)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRows(BizForm form, IEnumerable<Guid> docIds, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetDocListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetDocListTableFormRows(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public int GetDocListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetRefListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetRefListTableFormRows(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public int GetRefListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId)
        {
            throw new NotImplementedException();
        }

        public List<BizMenu> GetMenus(int languageId = 0)
        {
            throw new NotImplementedException();
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, Guid? docStateId, BizForm filter, List<Guid> sortAttrs, int pageNo, int pageSize)
        {
            throw new NotImplementedException();
        }

        public List<BizMenu> GetMenus(Guid userId, int languageId = 0)
        {
            return new List<BizMenu>();
        }

        public void TranslateForm(BizForm form, int languageId)
        {
            throw new NotImplementedException();
        }

        public void TranslateMenus(List<BizMenu> menus, int languageId)
        {
            throw new NotImplementedException();
        }

        public IList<ModelMessage> GetFormErrors(BizForm form, IList<ModelMessage> errors)
        {
            throw new NotImplementedException();
        }

        public BizForm SetFormOptions(BizForm form, IList<BizControlOption> options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Записывает значение атрибута документа в контрол
        /// </summary>
        /// <param name="control">Контрол</param>
        /// <param name="document">Документ</param>
        /// <returns></returns>
        public void SetControlData(BizControl control, Doc document)
        {
            return;
        }

        public BizDetailForm FindDetailForm(Guid formId)
        {
            throw new NotImplementedException();
        }

        public BizTableForm FindTableForm(Guid formId)
        {
            throw new NotImplementedException();
        }

        public BizForm FindForm(Guid formId)
        {
            throw new NotImplementedException();
        }
    }
}
