using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Intersoft.CISSA.UserApp.ServiceReference;
using Intersoft.CISSA.UserApp.Utils;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class MainForm : ContextState, IMainForm
    {
        public Guid UserId { get; private set; }
        public UserInfo UserInfo { get; private set; }

        public MainForm(IContext context, ContextState previous, UserInfo userInfo) : base(context, previous)
        {
            UserInfo = userInfo;
            UserId = userInfo.Id;
        }

        public MainForm(IContext context, UserInfo userInfo)
            : base(context)
        {
            UserInfo = userInfo;
            UserId = userInfo.Id;
        }

        private List<BizMenu> _menus = null;

        public List<BizMenu> GetMenus(IPresentationManager pm, int languageId = 0)
        {
            return _menus ?? (_menus = pm.GetMenus(languageId)); 
        }

        public List<BizMenu> GetMenus(IContext context)
        {
            if (_menus != null) return _menus;

            var pm = context.GetPresentationProxy();
            return _menus = pm.Proxy.GetMenus(context.GetLanguage());
        }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Home", "Main");
        }

        public void CheckMenuLanguage(IContext context)
        {
            if (_menus != null)
            {
                var langId = context.GetLanguage();

                foreach (var menu in _menus)
                {
                    if (menu.LanguageId != langId)
                    {
                        var pm = context.GetPresentationProxy();
                        _menus = pm.Proxy.TranslateMenus(_menus, langId);

                        return;
                    }
                }
            }
        }
    }
}