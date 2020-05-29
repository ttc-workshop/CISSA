using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Intersoft.CISSA.UserApp.Models;
using Intersoft.CISSA.UserApp.Models.Application;
using Intersoft.CISSA.UserApp.Models.Application.ContextStates;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Controllers
{
    public class HomeController : BaseController
    {
        [Authorize]
        public ActionResult Index()
        {
            var state = Get();
            if (state != null) return RedirectTo(state);
            return RedirectToAction("LogOn", "Account");

            try
            {
                ViewBag.Message = "Главная форма";
                var pm = GetPresentationProxy();
                {
                    var mainForm = pm.Proxy.GetMainForm();
 
                    return View(mainForm);
                }
            }
            catch (Exception e)
            {
                return RedirectTo(new ExceptionState(this, e.Message));
            }
        }

        [Authorize]
        public ActionResult Main()
        {
            var state = Get();
            if (state == null)
                return RedirectToAction("LogOn", "Account");

            try
            {
                ViewBag.Message = Resources.Home.MainForm /*"Главная форма"*/;
                var pm = GetPresentationProxy();
                {
                    var mainForm = pm.Proxy.GetMainForm();

                    if (Request.IsAjaxRequest()) return PartialView(mainForm);
                    return View(mainForm);
                }
            }
            catch (Exception e)
            {
                return RedirectTo(new ExceptionState(this, e.Message));
            }
        }

        public ActionResult Current()
        {
            var state = Get();
            if (state != null) return RedirectTo(state);
            return RedirectToAction("LogOn", "Account");
        }

        public ActionResult BackToMain()
        {
            var state = Find<RunProcess>();

            if (state != null)
            {
                return RedirectTo(
                            new AskForm(this, Resources.Home.ConfirmProcessTerminationToBackMain /*"Вы уверены, что хотите прервать процесс(ы) и вернуться на главную форму?"*/,
                                   new ContextAction("Home", "ForceToMain"), null));
            }
            return RedirectToAction("ForceToMain", "Home");
        }

        public ActionResult ForceToMain()
        {
            try
            {
                var state = FindCheck<MainForm>();

                Set(state);
                if (state != null) return RedirectTo(state);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult SetLanguage(int id)
        {
            try
            {
                var userInfo = GetUserInfo();

                if (userInfo.LanguageId != id)
                {
                    var um = GetUserProxy();
                    {
                        um.Proxy.SetUserLanguage(id);
                        userInfo.LanguageId = id;
                    }
                }
                var state = Get();
                while (state != null)
                {
                    if (state is MainForm)
                        ((MainForm)state).CheckMenuLanguage(this);
                    else if (state is BaseForm)
                        ((BaseForm)state).CheckFormLanguage(this);

                    state = state.Previous;
                }

                state = Get();
                if (state != null) return RedirectTo(state);
                return RedirectToAction("LogOn", "Account");
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ShowAbout()
        {
            return RedirectTo(new AboutForm(this));
        }

        public ActionResult About()
        {
            if (Request.IsAjaxRequest()) return PartialView();
            return View();
        }

        public ActionResult Back()
        {
            StartOperation();
            var state = Get();
            if (state != null) Set(state.Previous);
            return ContextStateResult();
        }

        public ActionResult MainMenu()
        {
            var state = Find<MainForm>();

            if (state != null)
            {
                var menus = state.GetMenus(this);

                ViewData["CurrentMenuId"] = Get().MenuId;
                ViewData["ExpandedNodes"] = new string[] {};

                var httpCookie = Request.Cookies["ExpandedNodes"];
                if (httpCookie != null)
                {
                    var urlDecode = HttpUtility.UrlDecode(httpCookie.Value);
                    if (urlDecode != null)
                        ViewData["ExpandedNodes"] = urlDecode.ToUpper().Split(';');
                }

                return PartialView(menus);
            }
            return PartialView((object)null);
        }

        public ActionResult RunProcess(Guid id, Guid menuId)
        {
            ContextState state = Find<RunProcess>();

            if (state != null)
            {
                return RedirectTo(
                            new AskForm(this, Resources.Home.ConfirmProcessTerminationToStartOther /*"Вы уверены, что хотите прервать процесс(ы) и запустить другой процесс?"*/,
                                   new ContextAction("Home", "ForceToRunProcess", new { id, menuId }), null));
            }
            try
            {
                state = FindCheck<MainForm>();

                Set(state);

                return RedirectToAction("Run", "Process", new {id, docId = (Guid?) null, menuId});
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ForceToRunProcess(Guid id, Guid menuId)
        {
            try
            {
                var state = FindCheck<MainForm>();

                Set(state);

                return RedirectToAction("Run", "Process", new {id, docId = (Guid?) null, menuId});
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ShowList(Guid id, Guid menuId)
        {
            try
            {
                ContextState state = Find<RunProcess>();

                if (state != null)
                {
                    return RedirectTo(
                        new AskForm(this, Resources.Home.ConfirmProcessTerminationToOpenForm /*"Вы уверены, что хотите прервать процесс(ы) и открыть форму?"*/,
                                    new ContextAction("Home", "ForceToShowList", new { id, menuId }), null));
                }
                state = FindCheck<MainForm>();

                Set(state);

                return RedirectToAction("ShowList", "Form", new {id, menuId, noLoad = true});
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ForceToShowList(Guid id, Guid menuId)
        {
            try
            {
                var state = FindCheck<MainForm>();

                Set(state);

                return RedirectToAction("ShowList", "Form", new {id, menuId});
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ShowFilterList(Guid id, Guid docStateId, Guid menuId)
        {
            try
            {
                ContextState state = Find<RunProcess>();

                if (state != null)
                {
                    return RedirectTo(
                        new AskForm(this, Resources.Home.ConfirmProcessTerminationToOpenForm /*"Вы уверены, что хотите прервать процесс(ы) и открыть форму?"*/,
                                    new ContextAction("Home", "ForceToShowFilterList", new { id, docStateId, menuId }), null));
                }
                state = FindCheck<MainForm>();

                Set(state);

                return RedirectToAction("ShowFilterList", "Form", new { id, docStateId, menuId, noLoad = true });
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ForceToShowFilterList(Guid id, Guid docStateId, Guid menuId)
        {
            try
            {
                var state = FindCheck<MainForm>();

                Set(state);

                return RedirectToAction("ShowFilterList", "Form", new {id, docStateId, menuId});
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult ToolBar()
        {
            try
            {
                var state = Get();

                ViewData["InProcess"] = Find<RunProcess>() != null;

                return PartialView(state);
            }
            catch
            {
                return PartialView();
            }
        }

        public ActionResult ClearCache()
        {
            try
            {
                var users = GetUserProxy();
                users.Proxy.ClearCache();

                return RedirectToAction("CacheInfo", "Home");
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult Clear()
        {
            try
            {
                var users = GetUserProxy();
                users.Proxy.ClearCache();

                var state = Get();
                if (state != null) return RedirectTo(state);
                return RedirectToAction("LogOn", "Account");
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult CacheInfo()
        {
            try
            {
                var users = GetUserProxy();
                ViewData["Info"] = users.Proxy.GetCacheInfo();

                if (Request.IsAjaxRequest()) return PartialView();
                return View();
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        public ActionResult MonitorInfo()
        {
            try
            {
                var users = GetUserProxy();
                var info = TreeToList(users.Proxy.GetMonitorNodes());

                if (Request.IsAjaxRequest()) return PartialView(info);
                return View(info);
            }
            catch (Exception e)
            {
                return ThrowException(e);
            }
        }

        private IEnumerable<MonitorNode> TreeToList(IEnumerable<MonitorNode> tree)
        {
            if (tree == null) yield break;

            foreach (var node in tree)
            {
                yield return node;

                if (node.Items != null)
                    foreach (var child in TreeToList(node.Items))
                    {
                        yield return child;
                    }
                ;
            }
        }
    }
}
