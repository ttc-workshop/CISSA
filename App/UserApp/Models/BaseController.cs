using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Intersoft.CISSA.UserApp.Models.Application;
using Intersoft.CISSA.UserApp.Models.Application.ContextStates;
using Intersoft.CISSA.UserApp.Models.Resource;
using Intersoft.CISSA.UserApp.ServiceReference;
using Intersoft.CISSA.UserApp.Utils;
using Telerik.Web.Mvc.Extensions;

namespace Intersoft.CISSA.UserApp.Models
{
    class ContextStateException : ApplicationException
    {
        public ContextStateException() {}
        public ContextStateException(string message) : base(message) {}
    }

    public class BaseController : Controller, IContext
    {
        private ContextState _contextState;
        protected ContextState ContextState
        {
            get { return _contextState ?? (_contextState = (ContextState) Session["UserAppContextState"]); }
        }

        private BizConnection _connection;
        internal BizConnection Connection
        {
            get { return _connection ?? (_connection = (BizConnection) Session["UserAppBizConnection"]); }
            set 
            { 
                Session["UserAppBizConnection"] = value;
                _connection = value;
            }
        }

        public bool IsConnected()
        {
            return (Connection != null) && Connection.Connected;
        }

        private void CheckConnected()
        {
            if (!IsConnected())
                throw new BizConnectionException(Resources.Base.ConnectionNotEstablished /*"Соединение с сервисом бизнес-логики не установлено!"*/);
        }

        private IList<ResourceDesc> _resources;

        public IList<ResourceDesc> ResourceList
        {
            get
            {
                if (_resources != null) return _resources;
                _resources = new List<ResourceDesc>();
                var sessionResources = Session["Resources"];
                if (sessionResources != null)
                    _resources.AddRange((IList<ResourceDesc>)sessionResources);
                return _resources;
            }
        }

        public string GetUserName()
        {
            CheckConnected();
            return Connection.UserName;
        }

        public Guid GetUserId()
        {
            CheckConnected();
            return Connection.UserId;
        }

        public UserInfo GetUserInfo()
        {
            CheckConnected();
            return Connection.Info;
        }

        public DateTime StartOperation()
        {
            var s = Session["StartOperation"];

            if (s == null)
            {
                var time = DateTime.Now;
                Session["StartOperation"] = time;

                return time;
            }
            return (DateTime) s;
        }

        public TimeSpan FinishOperation()
        {
            var startTime = Session["StartOperation"];

            if (startTime != null)
            {
                Session.Remove("StartOperation");
                return DateTime.Now - (DateTime) startTime;
            }
            return TimeSpan.Zero;
        }

        public int GetLanguage()
        {
            var userInfo = GetUserInfo();

            return userInfo != null ? userInfo.LanguageId : 0;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            if (Connection != null && Connection.Connected)
            {
                var lang = GetLanguage();
                var cultureName = "ru";

                try
                {
                    var um = GetUserProxy();
                    cultureName = um.Proxy.GetLanguageCultureName(lang);
                }
                catch
                {
                    cultureName = null;
                }

                if (String.IsNullOrEmpty(cultureName))
                    switch (lang)
                    {
                        case 0:
                            cultureName = "ru";
                            break;
                        case 1:
                            cultureName = "ky";
                            break;
                        case 2:
                            cultureName = "en";
                            break;
                        case 4:
                            cultureName = "tg";
                            break;
                        default:
                            cultureName = ConfigurationManager.AppSettings.Get("DefaultCulture");
                            break;
                    }

                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureName);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(cultureName);
            }
            else
            {
                var cookie = ControllerContext.HttpContext.Request.Cookies["culture"];
                var cultureName = Session["culture"] ?? (cookie != null ? cookie.Value : "ru");

                if (cultureName != null)
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(cultureName.ToString());
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(cultureName.ToString());
                }

            }
        }

        /*public IUserManager UserManager
        {
            get
            {
                var um = new UserManagerClient();
                if (um.ClientCredentials != null)
                {
                    um.ClientCredentials.UserName.UserName = Connection.UserName;
                    um.ClientCredentials.UserName.Password = Connection.Password;
                }
                um.Open();

                return um;
            }
        }*/

        private readonly object _locker = new Object();
        private ConnectionClient<UserManagerClient, IUserManager> _userProxy;
        private ConnectionClient<PresentationManagerClient, IPresentationManager> _presentationProxy;
        private ConnectionClient<DocManagerClient, IDocManager> _documentProxy;
        private ConnectionClient<WorkflowManagerClient, IWorkflowManager> _workflowProxy;
        private ConnectionClient<ReportManagerClient, IReportManager> _reportProxy;
        private ConnectionClient<QueryManagerClient, IQueryManager> _queryProxy;

        public ConnectionClient<UserManagerClient, IUserManager> GetUserProxy()
        {
            lock (_locker)
            {
                if (_userProxy == null)
                    _userProxy = new ConnectionClient<UserManagerClient, IUserManager>(GetUserManager());
                return _userProxy;
            }
        }

        public ConnectionClient<DocManagerClient, IDocManager> GetDocumentProxy()
        {
            lock (_locker)
            {
                if (_documentProxy == null)
                    _documentProxy = new ConnectionClient<DocManagerClient, IDocManager>(GetDocumentManager());
                return _documentProxy;
            }
        }

        public ConnectionClient<PresentationManagerClient, IPresentationManager> GetPresentationProxy()
        {
            lock (_locker)
            {
                if (_presentationProxy == null)
                    _presentationProxy =
                        new ConnectionClient<PresentationManagerClient, IPresentationManager>(GetPresentationManager());
                return _presentationProxy;
            }
        }

        public ConnectionClient<QueryManagerClient, IQueryManager> GetQueryProxy()
        {
            lock (_locker)
            {
                if (_queryProxy == null)
                    _queryProxy = new ConnectionClient<QueryManagerClient, IQueryManager>(GetQueryManager());
                return _queryProxy;
            }
        }

        public ConnectionClient<ReportManagerClient, IReportManager> GetReportProxy()
        {
            lock (_locker)
            {
                if (_reportProxy == null)
                    _reportProxy = new ConnectionClient<ReportManagerClient, IReportManager>(GetReportManager());
                return _reportProxy;
            }
        }

        public ConnectionClient<WorkflowManagerClient, IWorkflowManager> GetWorkflowProxy()
        {
            lock (_locker)
            {
                if (_workflowProxy == null)
                    _workflowProxy = new ConnectionClient<WorkflowManagerClient, IWorkflowManager>(GetWorkflowManager());
                return _workflowProxy;
            }
        }

        public BizForm FindFormById(Guid formId)
        {
            var state = ContextState;

            while (state != null)
            {
                if (state is IFormContextState)
                {
                    var form = ((IFormContextState) state).GetForm();

                    if (form != null && form.Id == formId)
                        return form;
                }
                state = state.Previous;
            }
            return null;
        }

        public Doc FindDocumentById(Guid docId)
        {
            var state = ContextState;

            while (state != null)
            {
                if (state is IDocumentContextState)
                {
                    var document = ((IDocumentContextState) state).GetDocument(this);

                    if (document != null && document.Id == docId)
                        return document;
                }
                state = state.Previous;
            }
            return null;
        }

        public BizControl FindControlById(Guid controlId)
        {
            var state = ContextState;

            while (state != null)
            {
                if (state is IFormContextState)
                {
                    var form = ((IFormContextState)state).GetForm();

                    if (form != null)
                    {
                        var control = FindControlIn(form, controlId);
                        if (control != null) return control;
                    }
                    if (state is BaseTableForm)
                    {
                        var filter = ((BaseTableForm) state).FilterForm;

                        if (filter != null)
                        {
                            var control = FindControlIn(form, controlId);
                            if (control != null) return control;
                        }
                    }
                }
                state = state.Previous;
            }
            return null;
        }

        public BizControl FindControlIn(BizControl control, Guid controlId)
        {
            if (control == null) return null;
            if (control.Id == controlId) return control;

            if (control.Children != null)
                foreach(var child in control.Children)
                {
                    if (child.Id == controlId) return child;

                    var docControl = child as BizDocumentControl;
                    if (docControl != null && docControl.DocForm != null)
                    {
                        var docControlSub = FindControlIn(docControl.DocForm, controlId);
                        if (docControlSub != null) return docControlSub;
                    }
                    else
                    {
                        var docListControl = child as BizDocumentListForm;
                        if (docListControl != null && docListControl.TableForm != null)
                        {
                            var docListControlSub = FindControlIn(docListControl.TableForm, controlId);
                            if (docListControlSub != null) return docListControlSub;
                        }
                    }
                    var sub = FindControlIn(child, controlId);
                    if (sub != null) return sub;
                }

            return null;
        }

        protected UserManagerClient GetUserManager()
        {
            var um = new UserManagerClient();
            if (um.ClientCredentials != null)
            {
                um.ClientCredentials.UserName.UserName = Connection.UserName;
                um.ClientCredentials.UserName.Password = Connection.Password;
            }
            um.Open();

            return um;
        }

/*        public IDocManager DocumentManager
        {
            get
            {
                var dm = new DocManagerClient();
                if (dm.ClientCredentials != null)
                {
                    dm.ClientCredentials.UserName.UserName = Connection.UserName;
                    dm.ClientCredentials.UserName.Password = Connection.Password;
                }
                dm.Open();

                return dm;
            }
        }*/

        protected DocManagerClient GetDocumentManager()
        {
            var dm = new DocManagerClient();
            if (dm.ClientCredentials != null)
            {
                dm.ClientCredentials.UserName.UserName = Connection.UserName;
                dm.ClientCredentials.UserName.Password = Connection.Password;
            }
            dm.Open();

            return dm;
        }

        /*public IPresentationManager PresentationManager
        {
            get
            {
                var pm = new PresentationManagerClient();
                if (pm.ClientCredentials != null)
                {
                    pm.ClientCredentials.UserName.UserName = Connection.UserName;
                    pm.ClientCredentials.UserName.Password = Connection.Password;
                }
                pm.Open();

                return pm;
            }
        }*/

        protected PresentationManagerClient GetPresentationManager()
        {
            var pm = new PresentationManagerClient();
            if (pm.ClientCredentials != null)
            {
                pm.ClientCredentials.UserName.UserName = Connection.UserName;
                pm.ClientCredentials.UserName.Password = Connection.Password;
            }
            pm.Open();

            return pm;
        }

        /*public IWorkflowManager WorkflowManager
        {
            get
            {
                var wm = new WorkflowManagerClient();
                if (wm.ClientCredentials != null)
                {
                    wm.ClientCredentials.UserName.UserName = Connection.UserName;
                    wm.ClientCredentials.UserName.Password = Connection.Password;
                }
                wm.Open();

                return wm;
            }
        }*/

        protected WorkflowManagerClient GetWorkflowManager()
        {
            var wm = new WorkflowManagerClient();
            if (wm.ClientCredentials != null)
            {
                wm.ClientCredentials.UserName.UserName = Connection.UserName;
                wm.ClientCredentials.UserName.Password = Connection.Password;
            }
            wm.Open();

            return wm;
        }

        /*public IReportManager ReportManager
        {
            get
            {
                var rm = new ReportManagerClient();
                if (rm.ClientCredentials != null)
                {
                    rm.ClientCredentials.UserName.UserName = Connection.UserName;
                    rm.ClientCredentials.UserName.Password = Connection.Password;
                }
                rm.Open();

                return rm;
            }
        }*/

        protected ReportManagerClient GetReportManager()
        {
            var rm = new ReportManagerClient();
            if (rm.ClientCredentials != null)
            {
                rm.ClientCredentials.UserName.UserName = Connection.UserName;
                rm.ClientCredentials.UserName.Password = Connection.Password;
            }
            rm.Open();

            return rm;
        }

        /*public IQueryManager QueryManager
        {
            get
            {
                var qm = new QueryManagerClient();
                if (qm.ClientCredentials != null)
                {
                    qm.ClientCredentials.UserName.UserName = Connection.UserName;
                    qm.ClientCredentials.UserName.Password = Connection.Password;
                }
                qm.Open();

                return qm;
            }
        }*/

        protected QueryManagerClient GetQueryManager()
        {
            var qm = new QueryManagerClient();
            if (qm.ClientCredentials != null)
            {
                qm.ClientCredentials.UserName.UserName = Connection.UserName;
                qm.ClientCredentials.UserName.Password = Connection.Password;
            }
            qm.Open();

            return qm;
        }

        public ActionResult Return()
        {
            var state = Get();
            if (state != null)
            {
                if (state.Previous is RunProcess)
                    Set(state.Previous.Previous);
                else
                    Set(state.Previous);
            }

            return ContextStateResult();
        }

        public ResourceDesc FindResource(Guid id)
        {
            var resourceList = ResourceList;

            return resourceList != null ? resourceList.FirstOrDefault(desc => desc.Id == id) : null;
        }

        public ResourceDesc GetResource(Guid id)
        {
            var resource = FindResource(id);

            if (resource == null)
                throw new ApplicationException(String.Format("Ресурс \"{0}\" не найден", id));

            return resource;
        }

        protected Guid AddResource(ResourceDesc desc)
        {
            _resources = ResourceList ?? new List<ResourceDesc>();
            _resources.Add(desc);
            Session["Resources"] = _resources;
            return desc.Id;
        }

        protected Guid RemoveResource(ResourceDesc desc)
        {
            _resources = ResourceList ?? new List<ResourceDesc>();
            _resources.Remove(desc);
            Session["Resources"] = _resources;
            return desc.Id;
        }

        protected void RemoveResource(Guid id)
        {
            _resources = ResourceList ?? new List<ResourceDesc>();
            var desc = _resources.FirstOrDefault(d => d.Id == id);
            if (desc != null)
                _resources.Remove(desc);
            Session["Resources"] = _resources;
        }

        internal ActionResult RedirectTo(ContextAction action)
        {
            if (String.IsNullOrEmpty(action.ControllerName))
            {
                return action.RouteValues != null ? 
                    RedirectToAction(action.ActionName, action.RouteValues) : RedirectToAction(action.ActionName);
            }

            if (action.RouteValues != null)
                return RedirectToAction(action.ActionName, action.ControllerName, action.RouteValues);

            return RedirectToAction(action.ActionName, action.ControllerName);
        }

        internal ActionResult SetAndRedirectTo(ContextState state)
        {
            Set(state);
            return RedirectTo(state);
        }

        internal ActionResult RedirectTo(ContextState state)
        {
            if (state != null)
                return RedirectTo(state.GetAction(this));

            if (Request.IsAjaxRequest())
                return ThrowException("Контекст не найден!");

            return RedirectToAction("LogOn", "Account");
        }

        internal ActionResult ContextStateResult()
        {
            return RedirectTo(Get());
        }

        internal ActionResult ContextStateResult<T>() where T : ContextState
        {
            return RedirectTo(Check<T>().GetAction(this));
        }

        internal ActionResult ThrowException(Exception e)
        {
            LogException(e);
            return RedirectTo(new ExceptionState(this, e.Message));
        }

        internal ActionResult ThrowException(string message)
        {
            return RedirectTo(new ExceptionState(this, message));
        }

        internal ActionResult ThrowException(ContextState previous, string message)
        {
            return RedirectTo(new ExceptionState(this, previous, message));
        }

        internal ActionResult ThrowException(ContextState previous, Exception e)
        {
            LogException(e);
            return RedirectTo(new ExceptionState(this, previous, e.Message));
        }

        /*protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
           /* if (filterContext.HttpContext.Session != null)
            {
                if (filterContext.HttpContext.Session.IsNewSession)
                {
                    string cookie = filterContext.HttpContext.Request.Headers["Cookie"];
                    if ((cookie != null) && (cookie.IndexOf("_sessionId") >= 0))
                    {
                        filterContext.Result =
                            RedirectToAction("LogOn", "Account");
                        return;
                    }
                }
            }#1#
            base.OnActionExecuting(filterContext);
        }*/
        
        protected void LogException(Exception e)
        {
            try
            {
                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var controllerName = GetType().Name;
                using (var writer = new StreamWriter(String.Format("c:\\distr\\cissa\\{0}Errors-{1}.log", controllerName, today), true))
                {
                    var userInfo = Connection != null && Connection.Info != null
                                       ? String.Format("{0}, {1}", Connection.Info.UserName, Connection.Info.OrganizationName)
                                       : "Неизвестный пользователь";

                    writer.WriteLine("{0}: \"{1}\"; \"{2}\"; message: \"{3}\"", DateTime.Now, userInfo, GetType().Name, e.Message);
                    if (e.InnerException != null)
                        writer.WriteLine("  - inner exception: \"{0}\"", e.InnerException.Message);
                    writer.WriteLine("  -- Stack: {0}", e.StackTrace);
                }
            }
            catch
            {
                // ignored
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            LogException(filterContext.Exception);

            /*if (filterContext.Exception is BizConnectionException || ContextState == null ||
                filterContext.Exception is CommunicationException || 
                filterContext.Exception is TimeoutException ||
                filterContext.Exception is FaultException)
            {
                filterContext.Result = RedirectToAction("LogOn", "Account");
                filterContext.ExceptionHandled = true;
            }
            if (filterContext.Exception is ContextStateException)
            {
                filterContext.Result = ThrowException(filterContext.Exception);
                filterContext.ExceptionHandled = true;
            }*/
            base.OnException(filterContext);
        }

        protected override void OnResultExecuting(ResultExecutingContext context)
        {
            CheckAndHandleFileResult(context);

            base.OnResultExecuting(context);
        }

        private const string FileDownloadCookieName = "fileDownload";

        /// <summary>
        /// If the current response is a FileResult (an MVC base class for files) then write a
        /// cookie to inform jquery.fileDownload that a successful file download has occured
        /// </summary>
        /// <param name="context"></param>
        private void CheckAndHandleFileResult(ResultExecutingContext context)
        {
            if (context.Result is FileResult)
                //jquery.fileDownload uses this cookie to determine that a file download has completed successfully
                Response.SetCookie(new HttpCookie(FileDownloadCookieName, "true") { Path = "/" });
            else
                //ensure that the cookie is removed in case someone did a file download without using jquery.fileDownload
                if (Request.Cookies[FileDownloadCookieName] != null)
                    Response.Cookies[FileDownloadCookieName].Expires = DateTime.Now.AddYears(-1);
        }

        /// <summary>
        /// Возвращает состояние конекста определенного типа
        /// </summary>
        /// <typeparam name="T">Тип состояния</typeparam>
        /// <returns></returns>
        public T Get<T>() where T : ContextState
        {
            var state = ContextState;

            if (state is T) return (T)state;

            return null;
        }

        public ContextState Get()
        {
            return ContextState;
        }

        public T Check<T>() where T : ContextState
        {
            var state = ContextState;

            if (state is T) return (T)state;

            if (state == null)
                throw new ContextStateException(Resources.Base.AppContextNotFound /*"Статус контекста приложения не найден"*/);

            throw new ContextStateException(Resources.Base.AppContextError /*"Ошибка в статусе контекста приложения"*/);
        }

        public ContextState Check()
        {
            var state = ContextState;

            if (state == null)
                throw new ContextStateException(Resources.Base.AppContextNotFound /*"Статус контекста приложения не найден"*/);

            return state;
        }

        public T Find<T>() where T : ContextState
        {
            var state = ContextState;

            while (state != null)
            {
                if (state is T) return (T) state;
                state = state.Previous;
            }

            return null;
        }

        public T FindCheck<T>() where T : ContextState
        {
            var state = ContextState;

            while (state != null)
            {
                if (state is T) return (T)state;
                state = state.Previous;
            }

            throw new ContextStateException(Resources.Base.AppContextNotFound /*"Статус контекста приложения не найден"*/);
        }

        public void Set(ContextState state)
        {
            Session["UserAppContextState"] = state;
            _contextState = state;
        }

        internal new ActionResult File(byte[] buffer, string contentType, string fileName)
        {
            return base.File(buffer, contentType, fileName);
        }

        internal new ActionResult File(Stream stream, string contentType, string fileName)
        {
            return base.File(stream, contentType, fileName);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (_userProxy != null) _userProxy.Dispose();
                if (_documentProxy != null) _documentProxy.Dispose();
                if (_presentationProxy != null) _presentationProxy.Dispose();
                if (_workflowProxy != null) _workflowProxy.Dispose();
                if (_queryProxy != null) _queryProxy.Dispose();
                if (_reportProxy != null) _reportProxy.Dispose();
            }
        }
    }
}
