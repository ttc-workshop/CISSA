using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Intersoft.CISSA.UserApp.Models;
using Intersoft.CISSA.UserApp.Models.Application.ContextStates;
using Intersoft.CISSA.UserApp.Models.Repository;
using Intersoft.CISSA.UserApp.ServiceReference;
using Resources;
using Telerik.Web.Mvc.UI;

namespace Intersoft.CISSA.UserApp.Controllers
{
    [Authorize]
    public class FormController : BaseController
    {
        private IFormRepository _formRepository;
        protected IFormRepository Repository
        {
            get { return _formRepository ?? (_formRepository = new FormRepository(this)); }
        }

//        public FormController()
//        {
//            _formRepository = new FormRepository(PresentationManager);
//        }

        public ActionResult Show()
        {
            StartOperation();
            try
            {
                var state = Check<BaseForm>();

                if (state is BaseDocForm || state is NewDocForm)
                {
                    var form = state.GetCurrentForm(this) as BizDetailForm;

                    if (form != null)
                    {
                        var doc = state is BaseDocForm ? 
                            ((BaseDocForm)state).GetCurrentDocument(this) :
                            ((NewDocForm)state).GetCurrentDocument(this);

                        if (doc != null)
                        {
                            if (state.ControlOptions != null)
                            {
                                var pm = GetPresentationProxy();
                                form = (BizDetailForm) pm.Proxy.SetFormControlOptions(form, new List<BizControlOption>(state.ControlOptions));
                            }
//                                var curLanguage = GetLanguage();
/*
                                form = (BizDetailForm) pm.Proxy.SetFormDoc(form, doc);
                                if (form.LanguageId != curLanguage)
                                    form = (BizDetailForm)pm.Proxy.TranslateForm(form, curLanguage); */
                            form = state.SetFormDoc(this, doc) as BizDetailForm;

                            InitViewData(false);
                            if (state is BaseDocForm)
                                ViewData["UserActions"] = ((BaseDocForm) state).UserActions;
                            ViewData["FormCaption"] = String.IsNullOrEmpty(state.FormCaption)
                                ? form.Caption
                                : state.FormCaption;

                            if (!Request.IsAjaxRequest()) return View(form);
                            return PartialView(form);
                        }
                        return ThrowException(state.Previous, Form.DocumentNotFound /*"Документ не найден для отображения"*/);
                    }
                    return ThrowException(state.Previous, Form.FormTypeError /*"Ошибка в типе формы!"*/);
                }
                return ThrowException(state.Previous, Base.AppContextError /*"Ошибка в статусе контекста"*/);
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ShowDoc(Guid formId, Guid docId)
        {
            StartOperation();
            return RedirectTo(new ShowDocForm(this, formId, docId));
        }

        public ActionResult EditDoc(Guid formId, Guid docId)
        {
            StartOperation();
            return RedirectTo(new EditDocForm(this, formId, docId));
        }

        public ActionResult Edit()
        {
            StartOperation();
            try
            {
                var state = Check<EditDocForm>();
                var form = state.GetCurrentForm(this) as BizDetailForm;
                if (form != null)
                {
                    var doc = state.GetCurrentDocument(this);

                    form = (BizDetailForm) Repository.SetDoc(form, doc);
                    if (state.ControlOptions != null)
                    {
                        var pm = GetPresentationProxy();
                        form = (BizDetailForm)pm.Proxy.SetFormControlOptions(form, new List<BizControlOption>(state.ControlOptions));
                    }
                    if (state.ErrorMessages != null && state.ErrorMessages.Count > 0)
                    {
                        var pm = GetPresentationManager();
                        var errors = pm.GetFormErrors(form, new List<ModelMessage>(state.ErrorMessages));
                        if (errors != null && errors.Count > 0)
                            foreach (var error in errors)
                            {
                                ModelState.AddModelError(error.Key != Guid.Empty ? error.Key.ToString() : error.Name,
                                    error.Message);
                                ModelState.SetModelValue(error.Key != Guid.Empty ? error.Key.ToString() : error.Name,
                                    new ValueProviderResult(null, "", CultureInfo.CurrentCulture));
                            }
                    }

                    InitViewData(true);
                    ViewData["FormCaption"] = String.IsNullOrEmpty(state.FormCaption)
                                                  ? form.Caption
                                                  : state.FormCaption;

                    if (!Request.IsAjaxRequest()) return View(form);
                    return PartialView(form);
                }
                return ThrowException(state.Previous, Form.FormTypeError /*"Ошибка в типе формы"*/);
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult New()
        {
            StartOperation();
            try
            {
                var state = Check<NewDocForm>();

                var form = state.GetCurrentForm(this) as BizDetailForm;
                if (form != null)
                {
                    if (state.GetCurrentDocument(this) == null)
                        return
                            RedirectTo(new ExceptionState(this, state.Previous, Form.DocumentClassNotDefined
                                /*"Класс документа не определен!"*/));

                    form = (BizDetailForm) Repository.SetDoc(form, state.Document);

                    if (state.ControlOptions != null)
                    {
                        var pm = GetPresentationProxy();
                        form = (BizDetailForm)pm.Proxy.SetFormControlOptions(form, new List<BizControlOption>(state.ControlOptions));
                    }

                    InitViewData(true);
                    ViewData["FormCaption"] = String.IsNullOrEmpty(state.FormCaption)
                                                  ? form.Caption
                                                  : state.FormCaption;

                    if (!Request.IsAjaxRequest()) return View(form);
                    return PartialView(form);
                }

                return ThrowException(state.Previous, Form.FormTypeError /*"Ошибка в типе формы"*/);
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult NewDoc(Guid id)
        {
            StartOperation();
            return RedirectTo(new NewDocForm(this, id));
        }

        public ActionResult DeleteDoc(Guid formId, Guid docId)
        {
            StartOperation();
            try
            {
                var state = Get<BaseDocForm>();

                if (state == null)
                    return ThrowException(Base.AppContextStateError);
                if (state.FormId == formId && docId == state.DocumentId)
                {
                    var form = state.GetCurrentForm(this) as BizDetailForm;
                    if (form != null && form.AllowDelete)
                    {
                        var dm = GetDocumentManager();

                        dm.DocumentDelete(docId);
                        return SetAndRedirectTo(state.Previous);
                    }
                }

                return ThrowException(Form.FormOrDocumentNotFound);
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        private static BizDataControl FindSortControl(ICollection<BizControl> controls)
        {
            if (controls != null && controls.Count > 0)
                foreach (var control in controls)
                {
                    if (control is BizDataControl && control.SortType != SortType.None &&
                        ((BizDataControl)control).AttributeDefId != null)
                        return (BizDataControl) control;

                    var subControl = FindSortControl(control.Children);
                    if (subControl != null)
                        return subControl;
                }
            return null;
        }

        private static IEnumerable<BizDataControl> FindSortControls(ICollection<BizControl> controls)
        {
            var list = new List<BizDataControl>();

            if (controls != null && controls.Count > 0)
                foreach (var control in controls)
                {
                    if (control is BizDataControl && control.SortType != SortType.None &&
                        ((BizDataControl)control).AttributeDefId != null)
                        list.Add((BizDataControl)control);

                    list.AddRange(FindSortControls(control.Children));
                }
            return list;
        }

        public ActionResult List()
        {
            StartOperation();
            try
            {
                var state = Check<BaseTableForm>();

                var pm = GetPresentationProxy();

                var form = state.GetCurrentForm(this);

                if (state.ControlOptions != null)
                {
                    form = pm.Proxy.SetFormControlOptions(form, new List<BizControlOption>(state.ControlOptions));
                }
                if (form is BizTableForm && form.DocumentDefId != null)
                {
                    var mform = new ManagedTableForm((BizTableForm) form /*, dm.Proxy, pm.Proxy*/)
                                    {
                                        DocStateId = state.DocStateId
                                    };

                    if (((BizTableForm) form).FilterFormId != null)
                    {
                        if (state.FilterForm == null)
                            state.FilterForm =
                                pm.Proxy.GetDetailForm((Guid) ((BizTableForm) form).FilterFormId, GetLanguage());

                        mform.FilterForm = state.FilterForm;
                        if (state.FilterControlOptions != null)
                        {
                            mform.FilterForm =
                                (BizDetailForm)
                                    pm.Proxy.SetFormControlOptions(mform.FilterForm,
                                        new List<BizControlOption>(state.FilterControlOptions));
                        }

                        /*if (state.FilterDocument != null)
                                mform.FilterForm =
                                    (BizDetailForm) pm.Proxy.SetFormDoc(mform.FilterForm, state.FilterDocument);*/
                    }

                    int rowCount;

                    var sortControls = FindSortControls(form.Children);
                    var sortAttrs = new List<AttributeSort>();
                    foreach (var ctrl in sortControls)
                        if (ctrl.AttributeDefId != null && ctrl.AttributeDefId != Guid.Empty)
                            sortAttrs.Add(
                                new AttributeSort
                                    {
                                        AttributeId = (Guid) ctrl.AttributeDefId,
                                        Asc = ctrl.SortType == SortType.Ascending
                                    });

                    if (state.QueryDef != null)
                    {
                        /*var qm = GetQueryProxy();
                        {
                            mform.RowDocs = qm.Proxy.GetDocListWithCount(out rowCount, state.QueryDef,
                                                                         mform.Form.PageNo,
                                                                         mform.Form.PageSize);
                        }*/

                        mform.RowForms = pm.Proxy.GetTableFormRowsFromFilterQuery(form, state.QueryDef, mform.FilterForm,
                                                                            sortAttrs,
                                                                            mform.Form.PageNo,
                                                                            mform.Form.PageSize, out rowCount);
                        mform.RowCount = rowCount;
                        mform.Form.PageCount = rowCount/mform.PageSize;
                        if (mform.PageSize > 0)
                        {
                            mform.Form.PageCount = rowCount/mform.PageSize;
                            if (rowCount%mform.PageSize > 0) mform.Form.PageCount++;
                        }
                        else
                            mform.Form.PageCount = 1;
                    }
                    else if (state.DocumentIdList != null && state.DocumentIdList.Count > 0)
                    {
//                        mform.RowDocs = new List<Guid>(state.DocumentIdList);
                        mform.RowForms = pm.Proxy.GetTableFormRowsFromList(form,
                                                                           new List<Guid>(state.DocumentIdList),
                                                                           sortAttrs,
                                                                           mform.Form.PageNo,
                                                                           mform.Form.PageSize);

                        mform.RowCount = state.DocumentIdList.Count;
                        if (mform.PageSize > 0)
                        {
                            mform.Form.PageCount = mform.RowCount / mform.PageSize;
                            if (mform.RowCount % mform.PageSize > 0) mform.Form.PageCount++;
                        }
                        else
                            mform.Form.PageCount = 1;
                    }
                        // DONE: Добавить отображение данных по DocumentList: List<Doc>?? // 2015-08-17
                    else if (state.Controls != null)
                    {
                        mform.RowForms = new List<BizControl>(state.Controls);
                        mform.RowCount = state.Controls.Count;
                        /*if (mform.PageSize > 0)
                        {
                            mform.Form.PageCount = mform.RowCount / mform.PageSize;
                            if (mform.RowCount % mform.PageSize > 0) mform.Form.PageCount++;
                        }
                        else*/
                            mform.Form.PageCount = 1;
                    }
                    else
                    {
/*
                        var dm = GetDocumentProxy();
                        {
                            if (mform.DocStateId != null)
                                mform.RowDocs = dm.Proxy.DocumentStateFilterList(out rowCount,
                                                                                 (Guid) form.DocumentDefId,
                                                                                 (Guid) mform.DocStateId,
                                                                                 mform.Form.PageNo,
                                                                                 mform.Form.PageSize,
                                                                                 state.FilterDocument,
                                                                                 sortAttrDefId);
                            else
                                mform.RowDocs = dm.Proxy.DocumentFilterList(out rowCount,
                                                                            (Guid) form.DocumentDefId,
                                                                            mform.Form.PageNo,
                                                                            mform.Form.PageSize,
                                                                            state.FilterDocument,
                                                                            sortAttrDefId);
                        }
*/
                        var noLoad = state is ShowTableForm ? ((ShowTableForm) state).NoLoad : false;

                        if (!noLoad || true)
                        {
                            mform.RowForms = pm.Proxy.GetTableFormRows(form, mform.DocStateId,
                                                                       mform.FilterForm,
                                                                       sortAttrs,
                                                                       mform.Form.PageNo,
                                                                       mform.Form.PageSize, out rowCount);
                            mform.RowCount = rowCount;
                            if (mform.PageSize > 0)
                            {
                                mform.Form.PageCount = rowCount/mform.PageSize;
                                if (rowCount%mform.PageSize > 0) mform.Form.PageCount++;
                            }
                            else
                                mform.Form.PageCount = 1;
                        }
                        else
                        {
                            mform.RowForms = null;
                            mform.RowCount = -1;
                            mform.Form.PageCount = 1;
                        }
                    }
                    InitViewData(false);
                    ViewData["Selection"] = state is ShowSelectForm;
                    ViewData["UserActions"] = state.UserActions;
                    ViewData["FormCaption"] = String.IsNullOrEmpty(state.FormCaption)
                                                  ? form.Caption
                                                  : state.FormCaption;

                    if (!Request.IsAjaxRequest()) return View(mform);
                    return PartialView(mform);
                }
                return ThrowException(state.Previous, Form.TableFormError
                    /*"Форма не явлется табличной или заданы не все параметры"*/);

            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ShowList(Guid id, Guid? menuId, bool noLoad = true)
        {
            StartOperation();
            return RedirectTo(new ShowTableForm(this, id) {MenuId = menuId, NoLoad = noLoad});
        }

        public ActionResult ShowFilterList(Guid id, Guid? docStateId, Guid? menuId, bool noLoad = true)
        {
            StartOperation();
            return RedirectTo(new ShowTableForm(this, id, docStateId) { MenuId = menuId, NoLoad = noLoad});
        }

        public ActionResult Select()
        {
            StartOperation();
            try
            {
                var state = Check<BaseTableForm>();

                var pm = GetPresentationProxy();
                {
                    var form = state.GetCurrentForm(pm.Proxy);

                    if (state.ControlOptions != null)
                    {
                        form = pm.Proxy.SetFormControlOptions(form, new List<BizControlOption>(state.ControlOptions));
                    }
                    var tableForm = form as BizTableForm;
                    if (tableForm != null && tableForm.DocumentDefId != null)
                    {
                        var mform = new ManagedTableForm(tableForm);

                        if (tableForm.FilterFormId != null)
                        {
                            if (state.FilterForm == null)
                                state.FilterForm = pm.Proxy.GetDetailForm((Guid) tableForm.FilterFormId,
                                    GetLanguage());

                            mform.FilterForm = state.FilterForm;
                            if (state.FilterControlOptions != null)
                            {
                                mform.FilterForm =
                                    (BizDetailForm)
                                        pm.Proxy.SetFormControlOptions(mform.FilterForm,
                                            new List<BizControlOption>(state.FilterControlOptions));
                            }

                            if (state.FilterDocument != null)
                                mform.FilterForm =
                                    (BizDetailForm) pm.Proxy.SetFormDoc(mform.FilterForm, state.FilterDocument);
                        }

                        int rowCount;

                        var sortControls = FindSortControls(tableForm.Children);
                        var sortAttrs = new List<AttributeSort>();
                        foreach (var ctrl in sortControls)
                            if (ctrl.AttributeDefId != null && ctrl.AttributeDefId != Guid.Empty)
                                sortAttrs.Add(
                                    new AttributeSort
                                    {
                                        AttributeId = (Guid) ctrl.AttributeDefId,
                                        Asc = ctrl.SortType == SortType.Ascending
                                    });

                        if (state.QueryDef != null)
                        {
                            mform.RowForms = pm.Proxy.GetTableFormRowsFromQuery(tableForm, state.QueryDef,
                                                                                sortAttrs,
                                                                                mform.Form.PageNo,
                                                                                mform.Form.PageSize, out rowCount);

                            mform.RowCount = rowCount;
                            mform.Form.PageCount = rowCount/mform.PageSize;
                            if (mform.PageSize > 0)
                            {
                                mform.Form.PageCount = rowCount/mform.PageSize;
                                if (rowCount%mform.PageSize > 0) mform.Form.PageCount++;
                            }
                            else
                                mform.Form.PageCount = 1;
                        }
                        else if (state.DocumentIdList != null)
                        {
                            mform.RowForms = pm.Proxy.GetTableFormRowsFromList(tableForm,
                                                                               new List<Guid>(state.DocumentIdList),
                                                                               sortAttrs,
                                                                               mform.Form.PageNo,
                                                                               mform.Form.PageSize);

                            mform.RowCount = state.DocumentIdList.Count;
                            mform.Form.PageCount = mform.RowCount / mform.PageSize;
                            if (mform.PageSize > 0)
                            {
                                mform.Form.PageCount = mform.RowCount / mform.PageSize;
                                if (mform.RowCount%mform.PageSize > 0) mform.Form.PageCount++;
                            }
                            else
                                mform.Form.PageCount = 1;
                        }
                        else if (state.Controls != null)
                        {
                            mform.RowForms = new List<BizControl>(state.Controls);
                            mform.RowCount = state.Controls.Count;
                            mform.Form.PageCount = 1; //mform.RowCount / mform.PageSize;
                            /*if (mform.PageSize > 0)
                            {
                                mform.Form.PageCount = mform.RowCount / mform.PageSize;
                                if (mform.RowCount % mform.PageSize > 0) mform.Form.PageCount++;
                            }
                            else
                                mform.Form.PageCount = 1;*/
                        }
                        else
                        {
                            mform.DocStateId = state.DocStateId;
                            mform.RowForms = pm.Proxy.GetTableFormRows(tableForm, mform.DocStateId,
                                                                       mform.FilterForm,
                                                                       sortAttrs,
                                                                       mform.Form.PageNo,
                                                                       mform.Form.PageSize, out rowCount);
                            mform.RowCount = rowCount;
                            if (mform.PageSize > 0)
                            {
                                mform.Form.PageCount = rowCount/mform.PageSize;
                                if (rowCount%mform.PageSize > 0) mform.Form.PageCount++;
                            }
                            else
                                mform.Form.PageCount = 1;
                        }


                        InitViewData(false);
                        ViewData["Selection"] = state is ShowSelectForm;
                        ViewData["UserActions"] = state.UserActions;
                        ViewData["FormCaption"] = String.IsNullOrEmpty(state.FormCaption)
                                                      ? tableForm.Caption
                                                      : state.FormCaption;

                        if (!Request.IsAjaxRequest()) return View(mform);
                        return PartialView(mform);
                    }

                    return ThrowException(state.Previous, Form.TableFormError
                        /*"Форма не явлется табличной или заданы не все параметры"*/);
                }
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ShowSelectList(Guid id)
        {
            StartOperation();
            return RedirectTo(new ShowSelectForm(this, id));
        }

        public ActionResult SelectDoc(Guid id)
        {
            StartOperation();
            try
            {
                var state = Check<ShowSelectForm>();

                if (state.Previous is RunProcess)
                {
                    var wm = GetWorkflowProxy();
                    {
                        ((RunProcess) state.Previous).ContinueWithDocument(id, wm.Proxy);
                    }

                    return SetAndRedirectTo(state.Previous);
                }

                if (state.Previous is BaseDocForm)
                {
                    var docListId = ((BaseDocForm) state.Previous).DocListId;
                    if (docListId == null)
                        return ThrowException(state.Previous, Form.AppContextDataError
                            /*"Ошибка в данных контекста!"*/);

                    var form = ((BaseDocForm) state.Previous).GetCurrentForm(this);

                    if (form == null)
                        return ThrowException(state.Previous, Form.FormNotFound /*"Форма не найдена!"*/);

                    var docList =
                        FormRepository.FindControlById((Guid) docListId, form.Children) as BizDocumentListForm;
                    if (docList == null)
                        return ThrowException(state.Previous, Form.ControlNotFound /*"Визуальный элемент не найден!"*/);

                    var doc = ((BaseDocForm) state.Previous).GetCurrentDocument(this);
                    if (doc == null)
                        return ThrowException(state.Previous, Form.DocumentNotFound /*"Документ не найден!"*/);

                    var dm = GetDocumentProxy();
                    {
                        if (docList.AttributeDefId != null)
                            ((BaseDocForm) state.Previous).Document = dm.Proxy.AddDocToList(id, doc,
                                                                                            (Guid)
                                                                                            docList.AttributeDefId);
                    }

                    return SetAndRedirectTo(state.Previous);
                }

                return ThrowException(state.Previous, Base.AppContextError /*"Ошибка в контексте приложения"*/);
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult InsertDoc(Guid id)
        {
            StartOperation();
            try
            {
                var state = Check<BaseDocForm>();

                var form = state.GetCurrentForm(this);
                if (form == null)
                    return ThrowException(state.Previous, Form.FormNotFound /*"Форма не найдена!"*/);

                var docList = FormRepository.FindControlById(id, form.Children) as BizDocumentListForm;

                if (docList == null)
                    return ThrowException(state.Previous, Form.ControlNotFound /*"Визуальный элемент не найден!"*/);

                state.DocListId = id;

                return RedirectTo(new ShowSelectForm(this, docList.FormId ?? Guid.Empty));
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        /// <summary>
        /// Открывает форму параметра - вызывается только из процессов
        /// </summary>
        /// <returns></returns>
        public ActionResult Param()
        {
            StartOperation();
            try
            {
                var state = Check<ShowParamForm>();

                var form = state.GetCurrentForm(this) as BizDetailForm;
                if (form != null)
                {
                    var doc = state.GetCurrentDocument(this);

                    form = (BizDetailForm) Repository.SetDoc(form, doc);
                    if (state.ControlOptions != null)
                    {
                        var pm = GetPresentationProxy();
                        form = (BizDetailForm)pm.Proxy.SetFormControlOptions(form, new List<BizControlOption>(state.ControlOptions));
                    }
                    if (state.ErrorMessages != null && state.ErrorMessages.Count > 0)
                    {
                        var pm = GetPresentationProxy();
                        var errors = pm.Proxy.GetFormErrors(form, new List<ModelMessage>(state.ErrorMessages));
                        if (errors != null && errors.Count > 0)
                            foreach (var error in errors)
                            {
                                var key = error.Key != Guid.Empty ? error.Key.ToString() : error.Name;
                                if (String.IsNullOrEmpty(key)) key = "noname";
                                ModelState.AddModelError(key, error.Message);
                                ModelState.SetModelValue(key, new ValueProviderResult(null, "", CultureInfo.CurrentCulture));
                            }
                    }

                    InitViewData(true);
                    ViewData["UserActions"] = state.UserActions;
                    ViewData["FormCaption"] = String.IsNullOrEmpty(state.FormCaption)
                                                  ? form.Caption
                                                  : state.FormCaption;


                    if (Request.IsAjaxRequest()) return PartialView(form);
                    return View(form);
                }
                return
                    RedirectTo(new ExceptionState(this, state.Previous, Form.FormTypeError
                        /*"Ошибка в типе формы"*/));
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult /*Apply*/Filter(FormCollection fields)
        {
            StartOperation();
            try
            {
                var state = Check<BaseTableForm>();

                var form = state.FilterForm;
                foreach (string fldName in fields)
                {
                    Guid controlId;
                    var s = fldName.Length > 36 ? fldName.Remove(37) : fldName;

                    if (Guid.TryParse(s, out controlId))
                    {
                        object value = fields[s];
                        try
                        {
                            string message;
                            if (!Repository.AddFieldValue(out message, form, controlId, value, false))
                            {
                                ModelState.AddModelError(s, message);
                                ModelState.SetModelValue(s,
                                    new ValueProviderResult(null, value != null ? value.ToString() : "",
                                        CultureInfo.CurrentCulture));
                            }
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError(s, e.Message);
                        }
                    }
                }

                if (ModelState.IsValid)
                {
/*
                    if (state.FilterDocument == null)
                        var dm = GetDocumentProxy();
                        {
                            state.FilterDocument = dm.Proxy.DocumentNew(form.DocumentDefId ?? Guid.Empty);
                        }

                    if (state.FilterDocument != null)
                        state.FilterDocument = Repository.GetDoc(form, state.FilterDocument);
*/

                    var table = state.GetCurrentForm(this);
                    if (table is BizTableForm) ((BizTableForm) table).PageNo = 0;
                    if (state is ShowTableForm) ((ShowTableForm) state).NoLoad = false;

                    return RedirectToAction("List");
                }

                return List();
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ApplyParam(FormCollection fields)
        {
            StartOperation();
            try
            {
                var state = Check<ShowParamForm>();

                var form = state.GetCurrentForm(this);

                form = Repository.SetDoc(form, state.GetCurrentDocument(this));

                Guid userActionId;

                var hasUserActionId = Guid.TryParse(fields["UserAction"], out userActionId);

                foreach (string fldName in fields)
                {
                    Guid controlId;
                    var s = fldName.Length > 36 ? fldName.Remove(37) : fldName;

                    if (Guid.TryParse(s, out controlId))
                    {
                        object value = fields[s];
                        try
                        {
                            var message = "";
                            if (!Repository.AddFieldValue(out message, form, controlId, value, false))
                            {
                                ModelState.AddModelError(s, message);
                                ModelState.SetModelValue(s,
                                    new ValueProviderResult(null, value != null ? value.ToString() : "",
                                        CultureInfo.CurrentCulture));
                            }
                        }
                        catch (Exception e)
                        {
                            ModelState.AddModelError(s, e.Message);
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    if (state.Document == null)
                    {
                        var dm = GetDocumentProxy();
                        state.Document = dm.Proxy.DocumentNew(form.DocumentDefId ?? Guid.Empty);
                    }

                    var process = Find<RunProcess>();

                    if (process != null)
                    {
                        process.ProcessContext.CurrentDocument = Repository.GetDoc(form, state.Document);

                        Set(process);

                        var wm = GetWorkflowProxy();
                        {
                            process.ContinueWithUserAction(userActionId, wm.Proxy);
                        }
                    }
                    else
                        new ExceptionState(this, state.Previous, Base.AppContextError
                            /*"Ошибка в статусе контекста!"*/);

                    return ContextStateResult();
                }

                InitViewData(true);
                ViewData["UserActions"] = state.UserActions;
                ViewData["FormCaption"] = String.IsNullOrEmpty(state.FormCaption)
                                              ? form.Caption
                                              : state.FormCaption;

                if (Request.IsAjaxRequest()) return PartialView("Param", form);
                return View("Param", form);
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult Save(FormCollection fields)
        {
            StartOperation();
            try
            {
                var state = Check<BaseForm>();

                var form = state.GetCurrentForm(this);
                Doc document = null;

                if (state is BaseDocForm)
                    document = ((BaseDocForm)state).GetCurrentDocument(this);
                else if (state is NewDocForm)
                    document = ((NewDocForm) state).GetCurrentDocument(this);
                else
                    //throw new ApplicationException("Ошибка в статусе контекста");
                    return ThrowException(Get(), Base.AppContextError /*"Ошибка в статусе контекста"*/);

                form = Repository.SetDoc(form, document);

                foreach (string fldName in fields)
                {
                    Guid controlId;
                    var s = fldName.Length > 36 ? fldName.Remove(37) : fldName;

                    if (!Guid.TryParse(s, out controlId)) continue;

                    object value = fields[s];
                    try
                    {
                        string message;
                        if (!Repository.AddFieldValue(out message, form, controlId, value, false))
                        {
                            ModelState.AddModelError(s, message);
                            ModelState.SetModelValue(s,
                                new ValueProviderResult(null, value != null ? value.ToString() : "",
                                    CultureInfo.CurrentCulture));
                        }
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError(s, e.Message);
                    }
                }

                if (ModelState.IsValid)
                {
                    // TODO: Переделать процесс присвоения изменений в документ -> исключить полную запись формы в документ (FormRepository.GetFormDoc(form, doc)).
                    // 1. document = Repository.UpdateDoc(form, document, fields); -- Обновление документа (document) на основе формы (form) и значений полей формы (fields: Dictionary<Guid, object>)
                    // 2. form = Repository.SetDoc(form, document);
                    document = Repository.GetDoc(form, document);

//                    if (state is BaseDocForm) ((BaseDocForm) state).Document = document;
//                    if (state is NewDocForm) ((NewDocForm)state).Document = document;

                    if (state.IsProcessState)
                    {
                        IList<UserAction> userActions = null;
                        if (state is BaseDocForm) userActions = ((BaseDocForm) state).UserActions;

                        return
                            RedirectTo(new ShowDocForm(this, state.Previous, state.FormId, document, userActions)
                            	{Form = form});
                    }
                    var dm = GetDocumentProxy();
                    dm.Proxy.DocumentSave(document);
                    foreach (var blobData in state.BlobDatas)
                    {
                        if (blobData.NewData != null)
                            dm.Proxy.SaveDocImage(blobData.DocumentId, blobData.AttributeDefId, blobData.NewData, blobData.NewFileName);
                    }

                    if (state.Previous is BaseContextState)
                        ((BaseContextState) state.Previous).UpdateDocument(document);

                    Set(state.Previous);

                    return RedirectTo(state.Previous);
                }

                InitViewData(true);
                ViewData["FormCaption"] = String.IsNullOrEmpty(state.FormCaption)
                                              ? form.Caption
                                              : state.FormCaption;

                if (!Request.IsAjaxRequest()) return View("Edit", (BizDetailForm) form);
                return PartialView("Edit", (BizDetailForm) form);
            }
            catch (Exception e)
            {
                return ThrowException(Get(), e.Message);
            }
        }

        public ActionResult UserAction(Guid id)
        {
            var process = Find<RunProcess>();

            if (process != null)
            {
                var state = Get();

                if (state is BaseDocForm && ((BaseDocForm)state).Document != null)
                {
                    process.ProcessContext.CurrentDocument = ((BaseDocForm)state).Document;
                }

                Set(process);

                var wm = GetWorkflowProxy();
                process.ContinueWithUserAction(id, wm.Proxy);
            }
            else
                new ExceptionState(this, Base.AppContextError /*"Ошибка в статусе контекста!"*/);

            return ContextStateResult();
        }

        public ActionResult Step(int page)
        {
            StartOperation();
            var cstate = Get();

            if (cstate is BaseForm)
            {
                var form = ((BaseForm) cstate).GetCurrentForm(this);

                if (form is BizTableForm)
                {
                    ((BizTableForm) form).PageNo = page;

                    return RedirectTo(cstate);
                }
            }
            return ThrowException(Base.AppContextStateError
                /*"Ошибка в состоянии текущего контекста"*/);
        }

        public ActionResult ToExcel()
        {
            try
            {
                var state = Check<BaseTableForm>();

                var form = state.GetCurrentForm(this);
                
                if (form is BizTableForm && form.DocumentDefId != null)
                {
                    var rm = GetReportProxy();

                    if (state.QueryDef != null)
                    {
                        var buffer = rm.Proxy.ExcelFromQuery(form, state.QueryDef);

                        return File(buffer, "application/xls", form.Name + DateTime.Today.ToShortDateString() + ".xls");
                    }

                    if (state.DocumentIdList != null && state.DocumentIdList.Count > 0)
                    {
                        var buffer = rm.Proxy.ExcelFromDocIdList(form, new List<Guid>(state.DocumentIdList));

                        return File(buffer, "application/xls", form.Name + DateTime.Today.ToShortDateString() + ".xls");
                    }

                    {
                        var buffer = rm.Proxy.ExcelFromFormFilter(form, state.DocStateId, state.FilterForm);

                        return File(buffer, "application/xls", form.Name + DateTime.Today.ToShortDateString() + ".xls");
                    }
                }
                return ThrowException(state.Previous, Form.TableFormError
                    /*"Форма не явлется табличной или заданы не все параметры"*/);
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ListToExcel(Guid id, Guid docId)
        {
            var control = FindControlById(id) as BizDocumentListForm;

            if (control == null) return ThrowException(Resources.Form.ControlNotFound);

            var rm = GetReportProxy();

            var buffer = rm.Proxy.ExcelFromDocumentListForm(docId, control);
            return File(buffer, "application/xls", control.Name + DateTime.Today.ToShortDateString() + ".xls");
        }

        public ActionResult FormToExcel(Guid id, Guid docId)
        {
            var control = FindControlById(id) as BizForm;
            var doc = FindDocumentById(docId);

            if (control == null) return ThrowException(Resources.Form.ControlNotFound);

            var rm = GetReportProxy();

            var buffer = rm.Proxy.ExcelFromDetailForm(control, doc);
            return File(buffer, "application/xls", control.Name + DateTime.Today.ToShortDateString() + ".xls");
        }

        public ActionResult Message()
        {
            try
            {
                var state = Check<ShowMessage>();

                ViewData["Message"] = state.Message;
                ViewData["UserActions"] = state.UserActions;

                if (!Request.IsAjaxRequest()) return View();
                return PartialView();
            } 
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult Upload()
        {
            try
            {
                var state = Check<UploadFile>();

                ViewData["Message"] = state.Message;
                ViewData["UserActions"] = state.UserActions;

                if (!Request.IsAjaxRequest()) return View();
                return PartialView();
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        [HttpPost]
        public ActionResult Upload(IEnumerable<HttpPostedFileBase> attachments)
        {
            var process = Find<RunProcess>();
            var state = Get() as UploadFile;
            if (state != null)
            {
                // The Name of the Upload component is "attachments"                            
                if (attachments != null)
                {
                    var file = attachments.FirstOrDefault();

                    if (file != null)
                    {
                        Set(process);

                        var wm = GetWorkflowProxy();
                        using (var buf = new MemoryStream())
                        {
                            file.InputStream.Position = 0;
                            file.InputStream.CopyTo(buf);

                            process.ContinueWithUploadFile(buf.GetBuffer(), file.FileName, wm.Proxy);
                        }

                        ViewData["Message"] = state.Message;
                        ViewData["UserActions"] = state.UserActions;
                        return ContextStateResult();
                    }
                }
                return View();
            }
            return ThrowException(Base.AppContextError /*"Ошибка в статусе контекста!"*/);
        }

        public ActionResult SaveFile(IEnumerable<HttpPostedFileBase> files, Guid id, Guid attrId)
        {
            var state = Find<BaseForm>();
            if (state != null)
            {
                var file = files.FirstOrDefault();
                if (file != null)
                    using (var buf = new MemoryStream())
                    {
                        file.InputStream.Position = 0;
                        file.InputStream.CopyTo(buf);

                        state.SetBlobData(id, attrId, buf.ToArray(), file.FileName);

                        return Content("");
                    }
            }
            return Content("Cannot save files");
        }

        public ActionResult RemoveFile(string[] fileNames, Guid id, Guid attrId)
        {
            var state = Find<BaseForm>();
            if (state != null)
                state.RemoveBlobData(id, attrId);

            return Content("");
        }

        public ActionResult SaveImage(IEnumerable<HttpPostedFileBase> files, Guid id, Guid attrId)
        {
            return SaveFile(files, id, attrId);
        }

        public ActionResult RemoveImage(string[] fileNames, Guid id, Guid attrId)
        {
            return RemoveFile(fileNames, id, attrId);
        }

        public ActionResult Image(Guid id, int height = 0, int width = 0)
        {
            var image = FindControlById(id) as BizImage;

            if (image != null)
            {
                return File(GetResizedImage(image.ImageBytes, image.Height, image.Width), "image/jpeg");
            }
            return GetEmptyImage();
        }

        private ActionResult GetEmptyImage()
        {
            var dir = Server.MapPath("~/Content/img");
            var path = Path.Combine(dir, "spacer.gif");
            return File(path, "image/gif");
        }

        public ActionResult DocImage(Guid docId, Guid attrId, int height = 0, int width = 0)
        {
            var state = Find<BaseForm>();

            if (state != null)
            {
                var image = state.GetBlobData(this, docId, attrId);

                if (image != null)
                {
                    string ext;
                    if (image.NewData != null)
                    {
                        ext = Path.GetExtension(image.NewFileName);
                        if (!String.IsNullOrEmpty(ext)) ext = ext.Replace(".", "");
                        return File(GetResizedImage(image.NewData, height, width), "image/" + ext);
                    }
                    ext = Path.GetExtension(image.FileName);
                    if (!String.IsNullOrEmpty(ext)) ext = ext.Replace(".", "");
                    return File(GetResizedImage(image.Data, height, width), "image/" + ext);
                }
            }
            return GetEmptyImage();
        }

        private byte[] GetResizedImage(byte[] imageData, int height, int width)
        {
            if (width <= 0 && height <= 0) return imageData;

            using (var ms = new MemoryStream(imageData))
            {
                using (var ms2 = new MemoryStream())
                {
                    ResizeImage(height, width, ms, ms2);
                    ms2.Position = 0;
                    return ms2.ToArray();
                }
            }
            
        }

        public ActionResult DownloadFile(Guid docId, Guid attrId)
        {
            var state = Find<BaseForm>();

            if (state != null)
            {
                var blobData = state.GetBlobData(this, docId, attrId);

                if (blobData != null)
                {
                    string ext;
                    if (blobData.NewData != null)
                    {
                        ext = Path.GetExtension(blobData.NewFileName);
                        if (!String.IsNullOrEmpty(ext)) ext = ext.Replace(".", "");
                        return File(blobData.NewData, "application/" + ext, blobData.NewFileName);
                    }
                    ext = Path.GetExtension(blobData.FileName);
                    if (!String.IsNullOrEmpty(ext)) ext = ext.Replace(".", "");
                    return File(blobData.Data, "application/" + ext, blobData.FileName);
                }
            }
            return RedirectToAction("Current", "Home");
        }

        public ActionResult Error()
        {
            try
            {
                var state = Check<ExceptionState>();
                if (state.Previous == null) ViewData["NoContext"] = 1;
                ViewData["Message"] = state.Message;

                if (!Request.IsAjaxRequest()) return View();
                return PartialView();
            }
            catch(Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult Ask()
        {
            try
            {
                var state = Check<AskForm>();

                ViewData["Message"] = state.Message;

                if (!Request.IsAjaxRequest()) return View();
                return PartialView();
            }
            catch(Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult AskResult(int id)
        {
            try
            {
                var state = Check<AskForm>();

                var url = id != 0 ? state.YesAction : state.NoAction;

                Set(state.Previous);

                if (url == null) return ContextStateResult();

                return RedirectTo(url);
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult DocStates(Guid id)
        {
            var dm = GetDocumentProxy();
            {
                var states = dm.Proxy.DocumentStateList(id);

                if (!Request.IsAjaxRequest()) return View(states);
                return PartialView(states);
            }
        }

        public ActionResult DrawGrid(Guid gridId, Guid? docId, int pageNo, int pageSize)
        {
            var control = FindControlById(gridId);

            if (control == null) return View("NoGridControl");

//            if (control is BizTableForm)
//            {
//                var grid = new ManagedTableForm((BizTableForm) control);
//            }
            if (control is BizDocumentListForm)
            {
                var listControl = (BizDocumentListForm) control;
                if (listControl.TableForm == null && listControl.FormId != null)
                {
                    var pm = GetPresentationProxy();
                    listControl.TableForm = pm.Proxy.GetGridForm((Guid) listControl.FormId, GetLanguage());
                }
                if (listControl.TableForm != null)
                {
                    var tableForm = listControl.TableForm;

                    var grid = new ManagedTableForm(tableForm, listControl, docId, pageNo, pageSize);

                    if (docId != null && listControl.AttributeDefId != null)
                    {
                        int rowCount;

                        /*var dm = GetDocumentProxy();
                        {
                            grid.RowDocs = dm.Proxy.DocAttrListById(out rowCount, (Guid) docId,
                                                                    (Guid) listControl.AttributeDefId,
                                                                    pageNo, pageSize, null, null);
                        }*/
                        var pm = GetPresentationProxy();

                        grid.RowForms = pm.Proxy.GetDocListTableFormRows(tableForm, (Guid) docId,
                                                                         (Guid)listControl.AttributeDefId, pageNo, pageSize, out rowCount);
                        if (pageSize > 0)
                        {
                            tableForm.PageCount = rowCount/pageSize;
                            if (rowCount%pageSize > 0) tableForm.PageCount++;
                            tableForm.PageNo = pageNo;
                        }
                        grid.RowCount = rowCount;
                        grid.PageNo = pageNo;
                    }
                    else if (docId != null && listControl.FormAttributeDefId != null)
                    {
                        int rowCount;

                        var pm = GetPresentationProxy();

                        grid.RowForms = pm.Proxy.GetRefListTableFormRows(tableForm, (Guid)docId,
                                                                         (Guid)listControl.FormAttributeDefId, pageNo, pageSize, out rowCount);
                        if (pageSize > 0)
                        {
                            tableForm.PageCount = rowCount / pageSize;
                            if (rowCount % pageSize > 0) tableForm.PageCount++;
                            tableForm.PageNo = pageNo;
                        }
                        grid.RowCount = rowCount;
                        grid.PageNo = pageNo;
                    }
                    else
                        grid.RowDocs = new List<Guid>();

                    return PartialView("GridControl", grid);
                }
            }
//            if (control is BizDynamicDocumentListForm)
//            {
//                var listControl = (BizDynamicDocumentListForm) control;
//                if (listControl.TableForm != null)
//                {
//                    var tableForm = listControl.TableForm;
//                    var grid = new ManagedTableForm(tableForm);
//
//                    return View("DynamicGridControl", grid);
//                }
//            }
            return PartialView("GridControlError");
        }

        public ActionResult DrawGridRow(Guid formId, Guid docId)
        {
            var control = FindControlById(formId) /*?? pm.Proxy.GetAnyForm(formId)*/;

            try
            {
                if (control is BizTableForm)
                {
                    var dm = GetDocumentProxy();
                    {
                        var doc = dm.Proxy.DocumentLoad(docId);

                        var pm = GetPresentationProxy();
                        {
                            var tableForm = pm.Proxy.SetFormDoc((BizTableForm) control, doc);

                            return PartialView("GridRow", new ManagedTableFormRow(tableForm));
                        }
                    }
                }
                if (control is BizDocumentListForm)
                {
                    var listControl = (BizDocumentListForm) control;
                    if (listControl.TableForm != null)
                    {
                        BizForm tableForm = listControl.TableForm;

                        var dm = GetDocumentProxy();
                        {
                            var doc = dm.Proxy.DocumentLoad(docId);

                            var pm = GetPresentationProxy();
                            {
                                tableForm = pm.Proxy.SetFormDoc(tableForm, doc);
                            }
                        }
                        return PartialView("GridRow", new ManagedTableFormRow(tableForm));
                    }
                }
                return PartialView("GridRowError");
            }
            catch (Exception e)
            {
                ViewData["Error"] = e.Message;
                return PartialView("GridRowError");
            }
        }

        public ActionResult UpdateMasterControl(Guid id, string value)
        {
            try
            {
                var state = Get<BaseForm>();

                BizForm form = null;
                BizControl control = null;
                if (state is BaseDocForm)
                {
                    form = state.GetCurrentForm(this);
                    control = FindControlIn(form, id) as BizDataControl;
                }
                else if (state is BaseTableForm)
                {
                    form = ((BaseTableForm) state).GetFilterForm(this);
                    control = FindControlIn(form, id) as BizDataControl;
                }
                if (control == null) return Json(new {}, JsonRequestBehavior.AllowGet);

                if (control is BizComboBox)
                {
                    if (String.IsNullOrEmpty(value))
                        ((BizComboBox) control).Value = null;

                    Guid valueId;
                    if (Guid.TryParse(value, out valueId))
                    {
                        ((BizComboBox) control).Value = valueId;
                    }
                }

                var dependents = new List<ComboBoxUpdateData>();
                UpdateDependents(form, (BizComboBox)control, dependents);

                return new JsonResult {Data = dependents, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
            }
            catch
            {
                return Json(new {}, JsonRequestBehavior.AllowGet);
            }
        }

        private void UpdateDependents(BizForm form, BizComboBox control, List<ComboBoxUpdateData> updateList)
        {
            if (control.Dependents != null)
                foreach (var dependentId in control.Dependents)
                {
                    var dependent = FindControlIn(form, dependentId) as BizComboBox;
                    if (dependent != null /*&& dependent.Value != null*/)
                    {
                        var pm = GetPresentationProxy();
                        var items = pm.Proxy.GetFormComboBoxValues(form, dependent);
                        dependent.Items = items;
                        if (items == null && dependent.Value == null) continue;
                        string newValue = items != null && items.Any(v => v.Id == dependent.Value)
                            ? dependent.Value != null ? dependent.Value.ToString() : null
                            : null;

                        var data = new ComboBoxUpdateData(dependentId.ToString(), newValue);
                        if (items != null)
                            data.items = items.Select(i => new ComboBoxItem(i.Id.ToString(), i.Value)).ToArray();

                        updateList.Add(data);

                        UpdateDependents(form, dependent, updateList);
                    }
                }
        }

        public JsonResult ComboBoxBinding(Guid formId, Guid id)
        {
            try
            {
                var state = Check<BaseForm>();

                var form = state.FindFormById(formId);

                if (form != null)
                {
                    var comboBox = Repository.Find<BizComboBox>(form, id);

                    var pm = GetPresentationProxy();
                    var list = pm.Proxy.GetFormComboBoxValues(form, comboBox);

                    if (list != null)
                        return new JsonResult { Data = new SelectList(list, "Id", "Value"), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new {}, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult DropDownBinding(Guid formId, Guid id)
        {
            try
            {
                var state = Check<BaseForm>();

                var form = state.FindFormById(formId);

                if (form != null)
                {
                    var comboBox = Repository.Find<BizComboBox>(form, id);

                    var pm = GetPresentationProxy();
                    var list = pm.Proxy.GetFormComboBoxValues(form, comboBox);
                    if (list != null)
                    {
                        var dropDownList = new List<DropDownItem> {new DropDownItem {Value = null, Text = "-"}};
                        dropDownList.AddRange(list.Select(i => new DropDownItem { Value = i.Id.ToString(), Text = i.Value }));
                        var selectList = new SelectList(dropDownList, "Value", "Text");
                        return new JsonResult { Data = selectList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    }
                }
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveScanImage(Guid id, Guid attrId)
        {
            try
            {
                var state = Find<BaseForm>();
                if (state != null)
                {
                    var files = System.Web.HttpContext.Current.Request.Files;
                    var file = files["RemoteFile"];  //files.FirstOrDefault();
                    if (file != null)
                    {
                        var imageName = file.FileName;

                        file.SaveAs(/*Server.MapPath("/") + */"c:\\distr\\cissa\\" + imageName);
                        
                        using (var buf = new MemoryStream())
                        {
                            file.InputStream.Position = 0;
                            file.InputStream.CopyTo(buf);

                            state.SetBlobData(id, attrId, buf.ToArray(), file.FileName);

                            return Content("");
                        }
                    }
                        
                }
                return Content("Cannot save files");
                /*var uploadfile = files["RemoteFile"];
                var imageName = uploadfile.FileName;

                uploadfile.SaveAs(Server.MapPath("/") + "\\UploadedImages\\" + imageName);*/
            }
            catch (Exception ex)
            {
                return ThrowException(ex);
            }
        }

        public JsonResult CheckField(string fieldId, string value)
        {
            try
            {
                var state = Check<BaseForm>();

                var form = state.GetCurrentForm(this);

                var controlId = Guid.Parse(fieldId);
                try
                {
                    string message;
                    if (!Repository.AddFieldValue(out message, form, controlId, value, true))
                        return Json(new { Error = message }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new {Error = e.Message}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {Error = ""}, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Error = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        private static void ResizeImage(int height, int width, Stream fromStream, Stream toStream)
        {
            using (var image = System.Drawing.Image.FromStream(fromStream))
            {
                var fH = (double) height;
                var fW = (double) width;
                fH = ((fH > 0 && image.Height > fH) ? fH / image.Height : 1);
                fW = ((fW > 0 && image.Width > fW) ? fW / image.Width : 1);

                ResizeImage(Math.Min(fH, fW), image, toStream);
            }
        }
        private static void ResizeImage(double scaleFactor, Image image, Stream toStream)
        {
            var newWidth = (int)(image.Width * scaleFactor);
            var newHeight = (int)(image.Height * scaleFactor);
            using (var thumbnailBitmap = new Bitmap(newWidth, newHeight))
            {
                using (var thumbnailGraph = Graphics.FromImage(thumbnailBitmap))
                {
                    thumbnailGraph.CompositingQuality = CompositingQuality.HighQuality;
                    thumbnailGraph.SmoothingMode = SmoothingMode.HighQuality;
                    thumbnailGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                    thumbnailGraph.DrawImage(image, imageRectangle);

                    thumbnailBitmap.Save(toStream, image.RawFormat);
                }
            }
        }
        private void InitViewData(bool editMode)
        {
            ViewData["EditMode"] = editMode;
            ViewData["Editing"] = editMode;
            ViewData["Nesting"] = 0;
            
            var state = Get();

            ViewData["InProcess"] = state != null && (state is RunProcess || state.Previous is RunProcess);
            ViewData["ContextState"] = state;
            ViewData["ParentIsGrid"] = false;
            ViewData["DetailGrid"] = false;
//            ViewData["PresentationManager"] = (IPresentationManager) PresentationManager;
//            ViewData["DocumentManager"] = (IDocManager) DocumentManager;

            var duration = FinishOperation();
            if (duration != TimeSpan.Zero)
            {
                ViewData["Duration"] = duration;
            }
        }
    }
}
