using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Intersoft.CISSA.UserApp.Models;
using Intersoft.CISSA.UserApp.Models.Application;
using Intersoft.CISSA.UserApp.Models.Application.ContextStates;
using Intersoft.CISSA.UserApp.ServiceReference;
using Resources;

namespace Intersoft.CISSA.UserApp.Controllers
{
    [Authorize]
    public class ProcessController : BaseController
    {
        //
        // GET: /Process/
//        private readonly IWorkflowManager _workflowManager;

        public ActionResult Run(Guid id, Guid? docId, Guid? menuId)
        {
            try
            {
                if (docId != null) 
                    new RunProcess(this, id, (Guid) docId) {MenuId = menuId};
                else 
                    new RunProcess(this, id) {MenuId = menuId};

                return ContextStateResult();
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult Click(Guid id)
        {
            try
            {
                var state = Get();

                var process = Find<RunProcess>();

                if (process != null)
                {
                    if (state is BaseDocForm && ((BaseDocForm) state).Document != null)
                    {
                        process.ProcessContext.CurrentDocument = ((BaseDocForm) state).Document;
                    }

                    Set(process);

                    var wm = GetWorkflowProxy();
                    {
                        process.ContinueWithUserAction(id, wm.Proxy);
                    }
                }
                else
                    new ExceptionState(this, state.Previous, Base.AppContextError
                        /*"Ошибка в статусе контекста!"*/);

                return ContextStateResult();
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ShowForm()
        {
            try
            {
                var process = Check<RunProcess>();

                if (process.ProcessContext.CurrentFormId != null)
                {
                    var formId = (Guid)process.ProcessContext.CurrentFormId;
                    var form = process.FindFormById(formId);
                    if (form == null)
                    {
                        var pm = GetPresentationProxy();
                        form = pm.Proxy.GetAnyForm(formId, GetLanguage());
                    }
                    if (form is BizDetailForm)
                    {
                        if (process.ProcessContext.CurrentDocument != null)
                        {
                            if (process.ProcessContext.CurrentDocument.IsNew)
                                return RedirectTo(
                                    new EditDocForm(this, formId, process.ProcessContext.CurrentDocument,
                                        process.ProcessContext.UserActions, process.ProcessContext.ErrorMessages)
                                    {
                                        FormCaption = process.ProcessContext.FormCaption,
                                        IsProcessState = true,
                                        ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                                    });

                            return
                                RedirectTo(
                                    new ShowDocForm(this, formId, process.ProcessContext.CurrentDocument,
                                        process.ProcessContext.UserActions, process.ProcessContext.ErrorMessages)
                                    {
                                        FormCaption = process.ProcessContext.FormCaption,
                                        IsProcessState = true,
                                        ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                                    });
                        }

                        if (process.ProcessContext.CurrentDocumentId != null)
                        {
                            return
                                RedirectTo(
                                    new ShowDocForm(this, formId, (Guid) process.ProcessContext.CurrentDocumentId,
                                        process.ProcessContext.UserActions, process.ProcessContext.ErrorMessages)
                                    {
                                        FormCaption = process.ProcessContext.FormCaption,
                                        IsProcessState = true,
                                        ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                                    });
                        }

                        return
                            RedirectTo(
                                new ExceptionState(this, process.Previous, Process.ParameterError
                                    /*"Не все параметры заданы!"*/));
                    }
                    if (form is BizTableForm)
                    {
                        var newTableState =
                            new ShowTableForm(this, formId, process.ProcessContext.FilterDocument,
                                process.ProcessContext.UserActions)
                            {
                                FormCaption = process.ProcessContext.FormCaption,
                                QueryDef = process.ProcessContext.CurrentQuery,
                                IsProcessState = true,
                                ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                            };

                        var filterForm = newTableState.GetFilterForm(this);
                        if (filterForm != null)
                            newTableState.FilterControlOptions = GetFormOptions(filterForm.Id,
                                process.ProcessContext.FormOptions);
                        return RedirectTo(newTableState);
                    }

                    return ThrowException(Form.FormTypeError /*"Ошибка в типе формы"*/);
                }
                return RedirectTo(
                    new ExceptionState(this, process.Previous, Process.ParameterError
                        /*"Не все параметры заданы!"*/));
            }
            catch (Exception e)
            {
                return RedirectTo(new ExceptionState(this, e.Message));
            }
        }

        public ActionResult ShowSelectForm()
        {
            try
            {
                var process = Check<RunProcess>();

                if (process.ProcessContext.CurrentFormId != null)
                {
                    var formId = (Guid) process.ProcessContext.CurrentFormId;

                    if (process.ProcessContext.CurrentQuery != null)
                    {
                        return InitFilterAndRedirectTo(
                            new ShowSelectForm(this, formId, process.ProcessContext.FilterDocument,
                                process.ProcessContext.UserActions)
                            {
                                QueryDef = process.ProcessContext.CurrentQuery,
                                FormCaption = process.ProcessContext.FormCaption,
                                IsProcessState = true,
                                ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                            }, process.ProcessContext.FormOptions);
                    }
                    if (process.ProcessContext.DocumentList != null)
                    {
                        return InitFilterAndRedirectTo(
                            new ShowSelectForm(this, formId, process.ProcessContext.DocumentList,
                                process.ProcessContext.UserActions)
                            {
                                FormCaption = process.ProcessContext.FormCaption,
                                IsProcessState = true,
                                ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                            }, process.ProcessContext.FormOptions);
                    }
                    if (process.ProcessContext.ControlDatas != null)
                    {
                        return InitFilterAndRedirectTo(
                            new ShowSelectForm(this, formId, process.ProcessContext.ControlDatas,
                                process.ProcessContext.UserActions)
                            {
                                FormCaption = process.ProcessContext.FormCaption,
                                IsProcessState = true,
                                ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                            }, process.ProcessContext.FormOptions);
                    }

                    return InitFilterAndRedirectTo(
                        new ShowSelectForm(this, formId, process.ProcessContext.FilterDocument,
                            process.ProcessContext.UserActions)
                        {
                            FormCaption = process.ProcessContext.FormCaption,
                            DocStateId = process.ProcessContext.FilterDocStateId,
                            IsProcessState = true,
                            ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                        }, process.ProcessContext.FormOptions);
                }

                return RedirectTo(
                    new ExceptionState(this, process.Previous, Process.ParameterError
                        /*"Не все параметры заданы!"*/));
            }
            catch (Exception e)
            {
                return RedirectTo(new ExceptionState(this, e.Message));
            }
        }

        internal ActionResult InitFilterAndRedirectTo(BaseTableForm state, IList<BizFormOptions> options)
        {
            if (state != null)
            {
                state.FilterForm = state.GetFilterForm(this);
                if (state.FilterForm != null)
                    state.FilterControlOptions = GetFormOptions(state.FilterForm.Id, options);
            }
            return RedirectTo(state);
        }

        public ActionResult ShowParamForm()
        {
            try
            {
                var process = Check<RunProcess>();

                if (process.ProcessContext.CurrentFormId != null)
                {
                    var formId = (Guid)process.ProcessContext.CurrentFormId;

                    return RedirectTo(
                        new ShowParamForm(this, formId, process.ProcessContext.CurrentDocument,
                            process.ProcessContext.UserActions, process.ProcessContext.ErrorMessages)
                        {
                            FormCaption = process.ProcessContext.FormCaption,
                            IsProcessState = true,
                            ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                        });
                }

                return RedirectTo(
                    new ExceptionState(this, process.Previous, Process.ParameterError
                        /*"Не все параметры заданы!"*/));
            }
            catch (Exception e)
            {
                return RedirectTo(new ExceptionState(this, e.Message));
            }
        }

        public ActionResult ShowTemplateReport()
        {
            try
            {
                var process = Check<RunProcess>();
                if (process.ProcessContext != null)
                {
                    ContextState prev = process;
                    var fileName = process.ProcessContext.TemplateFileName;
                    var document = process.ProcessContext != null ? process.ProcessContext.CurrentDocument : null;
                    /*var wm = GetWorkflowProxy();
                    process.Continue(wm.Proxy);
                    if (process.ProcessContext != null && process.ProcessContext.State == WorkflowRuntimeState.Finish)
                        prev = process.Previous;*/

                    return RedirectTo(
                        new GenerateTemplate(this, prev, fileName, process.ProcessContext)
                        {
                            Document = document,
                            IsProcessState = true
                        });
                }
                return ThrowException(Process.ProcessContextNotFound);
            }
            catch (Exception e)
            {
                return RedirectTo(new ExceptionState(this, e.Message));
            }
        }

        public ActionResult SendFile()
        {
            try
            {
                var process = Check<RunProcess>();
                if (process.ProcessContext != null)
                {
                    ContextState prev = process;
                    if (process.ProcessContext.DownloadFiles != null && process.ProcessContext.DownloadFiles.Count > 0)
                    {
                        var downloadFile = process.ProcessContext.DownloadFiles[0];
                        /*
                        // CANCELLED:TODO Добавить переход на следующее действие в текущем процессе
                        // Set(process.Previous); // ??

                        var ext = Path.GetExtension(downloadFile.FileName);
                        return File(downloadFile.Data, "application/" + ext, downloadFile.FileName);*/

                        return RedirectTo(new SendFile(this, prev, downloadFile.FileName, downloadFile.Data));
                    }

                    var fileName = process.ProcessContext.TemplateFileName ?? "UnnamedFile" + DateTime.Today.ToString("yyyy-mm-dd");
                    /*var wm = GetWorkflowProxy();
                    process.Continue(wm.Proxy);
                    if (process.ProcessContext != null && process.ProcessContext.State == WorkflowRuntimeState.Finish)
                        prev = process.Previous;*/

                    return RedirectTo(new SendFile(this, prev, fileName));
                }
                return ThrowException(Process.ProcessContextNotFound);
            }
            catch (Exception e)
            {
                return RedirectTo(new ExceptionState(this, e.Message));
            }
        }

        public ActionResult UploadFile()
        {
            try
            {
                var process = Check<RunProcess>();

                return RedirectTo(new UploadFile(this, process.ProcessContext.Message, process.ProcessContext.UserActions));
            }
            catch (Exception e)
            {
                return RedirectTo(new ExceptionState(this, e.Message));
            }
        }

        public ActionResult ShowMessage()
        {
            try
            {
                var process = Check<RunProcess>();

                return RedirectTo(new ShowMessage(this, process.ProcessContext.Message, process.ProcessContext.UserActions));
            }
            catch (Exception e)
            {
                return RedirectTo(new ExceptionState(this, e.Message));
            }
        }

        public ActionResult Finish()
        {
            try
            {
                var state = Find<RunProcess>();

                if (state != null)
                {
                    if (state.Previous is BaseContextState)
                        ((BaseContextState) state.Previous).Update(this);

                    Set(state.Previous);

                    if (state.ProcessContext.CurrentFormId != null)
                        return ShowFinishForm(state);
                }
                else
                    new ExceptionState(this, Process.ProcessContextNotFound /*"Контекст процесса не найден!"*/);

                return ContextStateResult();
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ShowFinishForm(RunProcess process)
        {
            try
            {
                if (process.ProcessContext.CurrentFormId != null)
                {
                    var formId = (Guid) process.ProcessContext.CurrentFormId;
                    var form = process.FindFormById(formId);
                    if (form == null)
                    {
                        var pm = GetPresentationProxy();
                        form = pm.Proxy.GetAnyForm(formId, GetLanguage());
                    }

                    if (form is BizDetailForm)
                    {
                        if (process.ProcessContext.CurrentDocument != null)
                        {
                            if (process.ProcessContext.CurrentDocument.IsNew)
                                return RedirectTo(
                                    new EditDocForm(this, formId, process.ProcessContext.CurrentDocument,
                                        process.ProcessContext.UserActions)
                                    {
                                        FormCaption = process.ProcessContext.FormCaption,
                                        ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                                    });

                            return
                                RedirectTo(
                                    new ShowDocForm(this, formId, process.ProcessContext.CurrentDocument,
                                        process.ProcessContext.UserActions)
                                    {
                                        FormCaption = process.ProcessContext.FormCaption,
                                        ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                                    });
                        }

                        if (process.ProcessContext.CurrentDocumentId != null)
                        {
                            return RedirectTo(
                                new ShowDocForm(this, formId, (Guid) process.ProcessContext.CurrentDocumentId,
                                    process.ProcessContext.UserActions)
                                {
                                    FormCaption = process.ProcessContext.FormCaption,
                                    ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                                });
                        }

                        return RedirectTo(
                            new ExceptionState(this, process.Previous, Process.ParameterError
                                /*"Не все параметры заданы!"*/));
                    }
                    if (form is BizTableForm)
                    {
                        return RedirectTo(new ShowTableForm(this, formId, process.ProcessContext.FilterDocument,
                                                            process.ProcessContext.UserActions)
                        {
                            FormCaption = process.ProcessContext.FormCaption,
                            QueryDef = process.ProcessContext.CurrentQuery,
                            ControlOptions = GetFormOptions(formId, process.ProcessContext.FormOptions)
                        });
                    }

                    return ThrowException(Form.FormTypeError /*"Ошибка в типе формы"*/);
                }
                return RedirectTo(
                    new ExceptionState(this, process.Previous, Process.ParameterError
                        /*"Не все параметры заданы!"*/));
            }
            catch (Exception e)
            {
                return RedirectTo(new ExceptionState(this, e.Message));
            }
        }

        public ActionResult ShowException()
        {
            var state = Find<RunProcess>();

            if (state != null && state.ProcessContext != null)
                return ThrowException(state.Previous, state.ProcessContext.Message);
            return ThrowException(Base.AppContextError /*"Ошибка в статусе контекста!"*/);
        }

        public ActionResult Continue()
        {
            try
            {
                var state = Get();

                var prevState = state != null ? state.Previous : null;

                var runProcess = prevState as RunProcess;
                if (runProcess != null)
                {
                    if (state is BaseDocForm && ((BaseDocForm) state).Document != null)
                        runProcess.ProcessContext.CurrentDocument = ((BaseDocForm) state).Document;

                    Set(runProcess);

                    var wm = GetWorkflowProxy();
                    runProcess.Continue(wm.Proxy);
                }
                else
                    return ThrowException(prevState, Base.AppContextError /*"Ошибка в статусе контекста!"*/);

                return ContextStateResult();
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult AskCancel()
        {
            try
            {
                var state = Find<RunProcess>();

                if (state != null)
                    return RedirectTo(
                        new AskForm(this, Process.TerminateProcess
                            /*"Вы уверены, что хотите завершить текущий процесс?"*/,
                            new ContextAction("Process", "Cancel"), null));

                return RedirectToAction("Process", "Return");
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult Cancel()
        {
            try
            {
                var state = Find<RunProcess>();

                if (state != null) return SetAndRedirectTo(state.Previous);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        private static IList<BizControlOption> GetFormOptions(Guid formId, IList<BizFormOptions> options)
        {
            List<BizControlOption> result = null;

            if (options != null)
            {
                var formOption = options.FirstOrDefault(fo => fo.Id == formId);
                if (formOption != null)
                    result = formOption.Options;
            }

            return result;
        }

    }
}
