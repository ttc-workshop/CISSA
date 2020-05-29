using System.Dynamic;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class DynaContext: DynamicObject
    {
        public WorkflowContext Context { get; set; }

        public DynaContext(WorkflowContext context)
        {
            Context = context;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            object val = value;
            if (value is DynaDoc) // конвертим динамические документы в обычные
            {
                val = ((DynaDoc) value).Doc;
            }

            Context.SetVariable(binder.Name, val);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var res = Context.GetVariable(binder.Name);
            if (res is Doc) res = new DynaDoc((Doc)res, Context.UserId, Context.DataContext); // конвертим обычные доки в динамические
            
            result = res;

            return true;
        }
    }
}
