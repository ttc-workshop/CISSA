using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class CreateTableReport : BaseContextState
    {
        public List<DocDefName> Sources { get; set; }

        public CreateTableReport(IContext context) : base(context)
        {
            InitializeSourceList(context);
        }

        public CreateTableReport(IContext context, ContextState previous) : base(context, previous)
        {
            InitializeSourceList(context);
        }

        protected void InitializeSourceList(IContext context)
        {
            var dm = context.GetDocumentProxy();
            Sources = dm.Proxy.GetDocDefNames().Where(d => !String.IsNullOrEmpty(d.Caption)).ToList();
        }

        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Report", "Select");
        }
    }
}