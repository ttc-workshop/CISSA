using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class BaseTableForm : BaseForm
    {
        public Guid? DocStateId { get; set; }
//        [Obsolete("Устаревшее свойство")]
        public Doc FilterDocument { get; set; }
        public BizDetailForm FilterForm { get; set; }
        public IList<UserAction> UserActions { get; private set; }
        public IList<Guid> DocumentIdList { get; private set; }
        public IList<BizControl> Controls { get; private set; }
        public QueryDef QueryDef { get; set; }

        public IList<BizControlOption> FilterControlOptions { get; set; }

        public BaseTableForm(IContext context, Guid formId, IList<UserAction> userActions) : base(context, formId)
        {
            UserActions = userActions;
        }

        public BaseTableForm(IContext context, ContextState previous, Guid formId, IList<UserAction> userActions) 
            : base(context, previous, formId)
        {
            UserActions = userActions;
        }

        public BaseTableForm(IContext context, Guid formId, Guid? docStateId, IList<UserAction> userActions)
            : this(context, formId, userActions)
        {
            DocStateId = docStateId;
        }

        public BaseTableForm(IContext context, ContextState previous, Guid formId, Guid? docStateId, IList<UserAction> userActions)
            : this(context, previous, formId, userActions)
        {
            DocStateId = docStateId;
        }

        public BaseTableForm(IContext context, Guid formId, Doc filter, IList<UserAction> userActions)
            : this(context, formId, userActions)
        {
            FilterDocument = filter;
        }

        public BaseTableForm(IContext context, ContextState previous, Guid formId, Doc filter, IList<UserAction> userActions)
            : this(context, previous, formId, userActions)
        {
            FilterDocument = filter;
        }

        public BaseTableForm(IContext context, Guid formId, IList<Guid> docIdList, IList<UserAction> userActions)
            : this(context, formId, userActions)
        {
            DocumentIdList = docIdList;
        }

        public BaseTableForm(IContext context, ContextState previous, Guid formId, IList<Guid> docIdList, IList<UserAction> userActions)
            : this(context, previous, formId, userActions)
        {
            DocumentIdList = docIdList;
        }

        public BaseTableForm(IContext context, Guid formId, IList<BizControl> controls, IList<UserAction> userActions)
            : this(context, formId, userActions)
        {
            Controls = controls;
        }

        public BaseTableForm(IContext context, ContextState previous, Guid formId, IList<BizControl> controls, IList<UserAction> userActions)
            : this(context, previous, formId, userActions)
        {
            Controls = controls;
        }

        public override void CheckFormLanguage(IContext context)
        {
            base.CheckFormLanguage(context);

            if (FilterForm != null && FilterForm.LanguageId != context.GetLanguage())
            {
                var pm = context.GetPresentationProxy();
                FilterForm = (BizDetailForm) pm.Proxy.TranslateForm(FilterForm, context.GetLanguage());
            }
        }

        public BizDetailForm GetFilterForm(IContext context)
        {
            if (FilterForm != null) return FilterForm;

            var tableForm = GetCurrentForm(context) as BizTableForm;

            if (tableForm != null && tableForm.FilterFormId != null)
            {
                var pm = context.GetPresentationProxy();
                FilterForm = pm.Proxy.GetDetailForm((Guid) tableForm.FilterFormId, context.GetLanguage());
            }
            return FilterForm;
        }

        public override BizForm FindFormById(Guid formId)
        {
            if (FilterForm != null && FilterForm.Id == formId)
                return FilterForm;

            return base.FindFormById(formId);
        }
    }
}