using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Intersoft.CISSA.UserApp.Models;
using Intersoft.CISSA.UserApp.Models.Application.ContextStates;
using Resources;

namespace Intersoft.CISSA.UserApp.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {
        //
        // GET: /Report/

        public ActionResult Template()
        {
            try
            {
                var state = Check<GenerateTemplate>();

                /*Set(state.Previous);

                var ext = Path.GetExtension(state.FileName);
                var fileName = Path.GetFileName(state.FileName);
                var rm = GetReportProxy();
                {
                    var buffer = rm.Proxy.GenerateFromTemplate(state.Document, state.ProcessContext, state.FileName);

                    return File(buffer, "application/" + ext, fileName);
                }*/
                return View("File", state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult File()
        {
            try
            {
                var state = Check<SendFile>();

                /*Set(state.Previous);

                var rm = GetReportProxy();
                var ext = Path.GetExtension(state.FileName);
                var fileName = Path.GetFileName(state.FileName);
                var buffer = rm.Proxy.GetFile(state.FileName);

                return File(buffer, "application/" + ext, fileName);*/
                return View(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult Download()
        {
            try
            {
                var state = Get();
                var sendFile = state as SendFile;
                if (sendFile != null)
                {
                    // Set(state.Previous);

                    var rm = GetReportProxy();
                    //var ext = Path.GetExtension(state.FileName);
                    var fileName = Path.GetFileName(sendFile.FileName);
                    var buffer = rm.Proxy.GetFile(sendFile.FileName);

                    return File(buffer, "application/octet-stream", fileName);
                }
                var template = state as GenerateTemplate;
                if (template != null)
                {
                    // var ext = Path.GetExtension(template.FileName);
                    var fileName = Path.GetFileName(template.FileName);
                    var rm = GetReportProxy();
                    {
                        var buffer = rm.Proxy.GenerateFromTemplate(template.Document, template.ProcessContext, template.FileName);

                        return File(buffer, "application/octet-stream", fileName);
                    }
                }
                return ThrowException(ContextState, Resources.Base.AppContextError);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult DownloadComplete()
        {
            try
            {
                var state = Get();

                if (state is SendFile || state is GenerateTemplate)
                {
                    var prevState = state.Previous;
                    if (prevState != null)
                    {
                        var process = prevState as RunProcess;
                        if (process != null)
                            process.DownloadComplete(this);

                        return SetAndRedirectTo(prevState);
                    }
                }

                return ThrowException(ContextState, Resources.Base.AppContextError);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult Create()
        {
            StartOperation();
            try
            {
                return RedirectTo(new CreateTableReport(this));
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult Select()
        {
            StartOperation();
            try
            {
                var state = Check<CreateTableReport>();

                return View(state.Sources);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult SelectSource(Guid id)
        {
            StartOperation();
            try
            {
                var state = Check<CreateTableReport>();

                if (state.Sources != null)
                {
                    var source = state.Sources.FirstOrDefault(s => s.Id == id);

                    if (source != null)
                    {
                        return RedirectTo(new ConfigTableReport(this, state.Previous, id));
                    }
                }
                return View(state.Sources);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult Config()
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();

                return View(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult AddSource(Guid id)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();

                var sourceDef = state.Context.Def.Sources.FirstOrDefault(s => s.Id == id);
                if (sourceDef == null)
                    return ThrowException(Resources.Report.SourceNotFound);

                state.SourceId = sourceDef.Id;

                return View(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult JoinSource(Guid id, Guid docDefId, Guid attrId)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();

                if (id == Guid.Empty)
                    return ThrowException(Resources.Report.SourceNotFound);

                state.JoinSource(this, id, docDefId, attrId);

                return RedirectTo(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult RemoveSource(Guid id)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();

                if (id == Guid.Empty)
                    return ThrowException(Resources.Report.SourceNotFound);

                state.RemoveSource(this, id);

                return RedirectTo(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult SelectColumn(Guid id)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.SourceId = id;
                return View(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        /*public ActionResult AddColumn(Guid sourceId, Guid attrId)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();

                state.AddColumn(this, sourceId, attrId);
                return RedirectTo(state);
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }*/
        public ActionResult AddColumns(FormCollection fields)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();

                var sourceId = Guid.Parse(fields.Get("SourceId"));
                var attrs = new List<Guid>();
                foreach (string fldName in fields)
                {
                    Guid attrId;
                    if (!Guid.TryParse(fldName, out attrId)) continue;

                    var value = fields[fldName];

                    if (String.IsNullOrEmpty(value)) continue;
                    bool result;
                    if (value == "true,false")
                    {
                        string temp = value;
                        value = temp.Substring(0, 4);
                    }

                    if (bool.TryParse(value, out result) && result)
                        attrs.Add(attrId);
                }
                if (attrs.Count > 0)
                    state.AddColumns(this, sourceId, attrs);

                return RedirectTo(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            } 
        }

        public ActionResult EditColumn(Guid id)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();

                return View(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult RemoveColumn(Guid id)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.RemoveColumn(this, id);
                return RedirectTo(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult MoveColumn(Guid id, bool up)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.MoveColumn(this, id, up);
                return RedirectTo(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult AddRootExpCondition()
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.AddRootExpCondition(this);
                return RedirectTo(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult SelectCondition(Guid id)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.SourceId = id;
                return View(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult AddCondition(Guid sourceId, Guid attrId)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.AddParamCondition(this, sourceId, attrId);
                return RedirectTo(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult AddAttrCondition(Guid sourceId, Guid attrId)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.AddAttrCondition(this, sourceId, attrId);
                return RedirectTo(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult RemoveCondition(Guid id)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.RemoveCondition(this, id);
                return RedirectTo(state);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult Execute()
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                var xlsReportFile = state.Execute(this);
                return File(xlsReportFile, "application/xls", "UserReport.xls");
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult Store()
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                var reportContentData = state.Serialize(this);
                //var bytes = Encoding.UTF8.GetBytes(reportContentData);  //new byte[reportContentData.Length * sizeof(char)];
                //Buffer.BlockCopy(reportContentData.ToCharArray(), 0, bytes, 0, bytes.Length);
                return File(reportContentData, "application/octet-stream", "UserReport.xml");
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        [HttpPost]
        public ActionResult Restore(IEnumerable<HttpPostedFileBase> attachments)
        {
            try
            {
                if (attachments != null)
                {
                    var file = attachments.FirstOrDefault();

                    if (file != null)
                    {
                        using (var buf = new MemoryStream())
                        {
                            file.InputStream.Position = 0;
                            file.InputStream.CopyTo(buf);

                            var state = new ConfigTableReport(this, ContextState, buf.GetBuffer(), file.FileName);
                        }
                        return ContextStateResult();
                    }
                }
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
            return ThrowException(Report.RestoreError);
        }

        public ActionResult UpdateColumnSortType(Guid id, string value)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.SetColumnSortType(id, value);
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult UpdateColumnGrouping(string id, string value)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                if (!String.IsNullOrEmpty(id))
                {
                    var sid = id.Length == 37 ? id.Substring(1) : id;
                    Guid guid;
                    if (Guid.TryParse(sid, out guid))
                        state.SetColumnGrouping(guid, value);
                }
                return Json(new {}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult UpdateConditionExpression(Guid id, string value)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.SetConditionExpression(id, value);
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult UpdateConditionOperation(string id, string value)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                if (!String.IsNullOrEmpty(id))
                {
                    var sid = id.Length == 37 ? id.Substring(1) : id;
                    Guid guid;
                    if (Guid.TryParse(sid, out guid))
                        state.SetConditionOperation(guid, value);
                }
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult UpdateConditionRightParamEnumValue(Guid id, string value)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.SetConditionRightParamValue(id, value);
                return Json(new { }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult SetReportCaption(string value)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                state.Context.Def.Caption = value;
                return Json(new { caption = value }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult SetReportConditionParam(Guid id, string caption, string value)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                return state.SetConditionParamCaptionValue(id, caption, value)
                    ? Json(new {conditions = new[] {new {id, caption, value}}}, JsonRequestBehavior.AllowGet)
                    : Json(new {is_alert = true});
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult SetReportColumnCaption(Guid id, string value)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                return state.SetColumnCaption(id, value)
                    ? Json(new {columns = new[] {new {id, caption = value}}}, JsonRequestBehavior.AllowGet)
                    : Json(new {columns = new[] {new {id}}}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }

        public ActionResult UpdateConditionRightSource(string id, string value)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                var b = false;
                var guid = Guid.Empty;
                if (!String.IsNullOrEmpty(id))
                {
                    var sid = id.Length > 36 ? id.Substring(id.Length - 36) : id;
                    b = Guid.TryParse(sid, out guid);
                }
                if (b)
                {
                    var data = state.SetConditionRightSource(guid, value);
                    return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                
                return Json(new {}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }
        public ActionResult UpdateConditionRightAttribute(string id, string value)
        {
            StartOperation();
            try
            {
                var state = Check<ConfigTableReport>();
                var b = false;
                var guid = Guid.Empty;
                if (!String.IsNullOrEmpty(id))
                {
                    var sid = id.Length > 36 ? id.Substring(id.Length - 36) : id;
                    b = Guid.TryParse(sid, out guid);
                }
                if (b)
                {
                    var data = state.SetConditionRightAttribute(guid, value);
                    return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }
                
                return Json(new {}, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return ThrowException(ContextState, e);
            }
        }
    }
}
