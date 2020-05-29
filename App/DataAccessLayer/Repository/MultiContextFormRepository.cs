using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Raven.Abstractions.Extensions;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextFormRepository : IFormRepository
    {
        public IMultiDataContext DataContext { get; private set; }

        private readonly IList<IFormRepository> _repositories = new List<IFormRepository>();

        private readonly IDocRepository _docRepo;

        public MultiContextFormRepository(IAppServiceProvider provider)
        {
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Meta))
                    _repositories.Add(new FormRepository(provider, context));
            }
            _docRepo = provider.Get<IDocRepository>();
        }


        public BizDetailForm FindDetailForm(Guid formId)
        {
            return _repositories.Select(repo => repo.FindDetailForm(formId)).FirstOrDefault(f => f != null);
        }

        public BizTableForm FindTableForm(Guid formId)
        {
            return _repositories.Select(repo => repo.FindTableForm(formId)).FirstOrDefault(f => f != null);
        }

        public BizForm FindForm(Guid formId)
        {
            return _repositories.Select(repo => repo.FindForm(formId)).FirstOrDefault(f => f != null);
        }

        public BizForm GetForm(Guid formId, int languageId = 0)
        {
            var form = FindForm(formId);

            if (form == null)
                throw new ApplicationException(string.Format("Форма с идентификатором {0} не существует.", formId));

            if (languageId != 0) TranslateForm(form, languageId);

            return form;
        }

        public BizDetailForm GetDetailForm(Guid formId, int languageId = 0)
        {
            var form = FindDetailForm(formId);

            if (form == null)
                throw new ApplicationException(string.Format("Форма с идентификатором {0} не существует.", formId));

            if (languageId != 0) TranslateForm(form, languageId);

            return form;
        }

        public BizDetailForm GetDetailFormWithData(Guid formId, Guid docId, int languageId = 0)
        {
            var form = GetDetailForm(formId, languageId);

            var doc = _docRepo.LoadById(docId);

            if (doc != null)
                SetFormDoc(form, doc);

            return form;
        }

        public BizTableForm GetTableForm(Guid formId, int languageId = 0)
        {
            var form = FindTableForm(formId);

            if (form == null)
                throw new ApplicationException(string.Format("Форма с идентификатором {0} не существует.", formId));

            if (languageId != 0) TranslateForm(form, languageId);

            return form;
        }

        public BizControl SetFormDoc(BizControl form, Doc document)
        {
            // TODO: Вынести метод из репоизитария в Service/Helper класс
            return _repositories.First().SetFormDoc(form, document);
        }

        public Doc GetFormDoc(BizControl form, Doc document)
        {
            // TODO: Вынести метод из репоизитария в Service/Helper класс
            return _repositories.First().GetFormDoc(form, document);
        }

        /*public void SetControlData(BizControl control, Doc document)
        {
            throw new NotImplementedException();
        }*/

        // DONE: Вывести создание SqlQueryReader во внешний класс - SqlQueryReaderFactory
        public List<BizControl> GetTableFormRows(BizTableForm form, List<Guid> docIds)
        {
            return _repositories.First().GetTableFormRows(form, docIds);
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo,
            int pageSize)
        {
            return _repositories.First().GetTableFormRows(out count, form, docStateId, filter, sortAttrs, pageNo, pageSize);
        }

        public List<BizControl> GetTableFormRows(BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return _repositories.First().GetTableFormRows(form, docStateId, filter, sortAttrs, pageNo, pageSize);
        }

        public int GetTableFormRowCount(BizForm form, Guid? docStateId, BizForm filter)
        {
            return _repositories.First().GetTableFormRowCount(form, docStateId, filter);
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, QueryDef def, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return _repositories.First().GetTableFormRows(out count, form, def, sortAttrs, pageNo, pageSize);
        }

        public List<BizControl> GetTableFormRows(out int count, BizForm form, QueryDef def, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo,
            int pageSize)
        {
            return _repositories.First().GetTableFormRows(out count, form, def, filter, sortAttrs, pageNo, pageSize);
        }

        public List<BizControl> GetTableFormRows(BizForm form, QueryDef def, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return _repositories.First().GetTableFormRows(form, def, sortAttrs, pageNo, pageSize);
        }

        public List<BizControl> GetTableFormRows(BizForm form, QueryDef def, BizForm filter, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return _repositories.First().GetTableFormRows(form, def, filter, sortAttrs, pageNo, pageSize);
        }

        public int GetTableFormRowCount(BizForm form, QueryDef def)
        {
            return _repositories.First().GetTableFormRowCount(form, def);
        }

        public int GetTableFormRowCount(BizForm form, QueryDef def, BizForm filter)
        {
            return _repositories.First().GetTableFormRowCount(form, def, filter);
        }

        public List<BizControl> GetTableFormRows(BizForm form, IEnumerable<Guid> docIds, IEnumerable<AttributeSort> sortAttrs, int pageNo, int pageSize)
        {
            return _repositories.First().GetTableFormRows(form, docIds, sortAttrs, pageNo, pageSize);
        }

        public List<BizControl> GetTableFormRows(BizForm form, IEnumerable<Doc> docs, int pageNo, int pageSize)
        {
            return _repositories.First().GetTableFormRows(form, docs, pageNo, pageSize);
        }

        public List<BizControl> GetDocListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            return _repositories.First().GetDocListTableFormRows(out count, form, docId, attrDefId, pageNo, pageSize);
        }

        public List<BizControl> GetDocListTableFormRows(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            return _repositories.First().GetDocListTableFormRows(form, docId, attrDefId, pageNo, pageSize);
        }

        public int GetDocListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId)
        {
            return _repositories.First().GetDocListTableFormRowCount(form, docId, attrDefId);
        }

        public List<BizControl> GetRefListTableFormRows(out int count, BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            return _repositories.First().GetRefListTableFormRows(out count, form, docId, attrDefId, pageNo, pageSize);
        }

        public List<BizControl> GetRefListTableFormRows(BizForm form, Guid docId, Guid attrDefId, int pageNo, int pageSize)
        {
            return _repositories.First().GetRefListTableFormRows(form, docId, attrDefId, pageNo, pageSize);
        }

        public int GetRefListTableFormRowCount(BizForm form, Guid docId, Guid attrDefId)
        {
            // TODO: Вынести метод из репоизитария в Service/Helper класс
            return _repositories.First().GetRefListTableFormRowCount(form, docId, attrDefId);
        }

        public List<BizMenu> GetMenus(int languageId = 0)
        {
            return new List<BizMenu>(_repositories.SelectMany(repo => repo.GetMenus(languageId)));
        }

        public void TranslateForm(BizForm form, int languageId)
        {
            _repositories.ForEach(repo => repo.TranslateForm(form, languageId));
        }

        public void TranslateMenus(List<BizMenu> menus, int languageId)
        {
            _repositories.ForEach(repo => repo.TranslateMenus(menus, languageId));
        }

        public IList<ModelMessage> GetFormErrors(BizForm form, IList<ModelMessage> errors)
        {
            // TODO: Вынести метод из репоизитария в Service/Helper класс
            return _repositories.First().GetFormErrors(form, errors);
        }

        public BizForm SetFormOptions(BizForm form, IList<BizControlOption> options)
        {
            // TODO: Вынести метод из репоизитария в Service/Helper класс
            return _repositories.First().SetFormOptions(form, options);
        }

        /*public IList<EnumValue> GetFormComboBoxValueList(BizForm form, BizComboBox comboBox)
        {
            return _repositories.First().GetFormComboBoxValueList(form, comboBox);
        }*/
    }
}