using System;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Core;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class ProcessCallActivity : WorkflowActivity
    {
//        public string ReturnVars { get; private set; }

        public ProcessCallActivity(Process_Call_Activity activity)
            : base(activity)
        {
            CallProcessId = activity.Process_Id;
        }

        public Guid? CallProcessId { get; set; }

        public override void Execute(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            if (CallProcessId != null)
                context.CallProcess((Guid) CallProcessId);
            else
                context.ThrowException("No Call Process", "Вызываемый процесс не указан");
        }

        public override void AfterExecution(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            try
            {
                /*if (context.ReturnVariables != null)
                {
                    var parser = new CsParser(ReturnVars);

                    while (parser.NextToken() != TokenType.Eof)
                    {
                        if (parser.Token == TokenType.Ident)
                        {
                            CopyVar(parser, context);
                        }
                        else if (parser.Token == TokenType.Eof) 
                            break;
                        else
                        {
                            context.ThrowException("ProcessCallReturn", "Ошибка в выражении");
                            return;
                        }
                    }
                }*/
                base.Execute(context, provider, dataContext);
            }
            catch (Exception e)
            {
                context.ThrowException(e);
            }
        }

        /*private static void CopyVar(CsParser parser, WorkflowContext context)
        {
            var s = parser.TokenSymbol;

            var v = context.ReturnVariables.FirstOrDefault(i => String.Compare(i.Name, parser.TokenSymbol, true) == 0);

            if (v != null)
            {
                if (parser.NextToken() == TokenType.Operation && parser.TokenSymbol == "=>")
                {
                    v.Name = parser.GetIdent();
                }

                var oldVar = context.Variables.FirstOrDefault(i => String.Compare(i.Name, parser.TokenSymbol, true) == 0);
                if (oldVar != null) context.Variables.Remove(oldVar);

                context.Variables.Add(v);
            }
            else
            {
                parser.NextToken();
            }
        }*/
    }
}