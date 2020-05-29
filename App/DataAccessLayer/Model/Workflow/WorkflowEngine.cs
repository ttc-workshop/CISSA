using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class WorkflowEngine : IWorkflowEngine, IEnumerable
    {
        private IAppServiceProvider Provider { get; set; }
        private IDataContext DataContext { get; set; }
        //private readonly bool _ownDataContext;

        private Guid UserId { get; set; }

        public WorkflowEngine(IDataContext dataContext, IWorkflowRepository workflowRepo, Guid userId)
        {
            /*if (dataContext == null)
            {
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else*/
                DataContext = dataContext;

            UserId = userId;
            Repository = workflowRepo /*?? new WorkflowRepository(DataContext)*/;
        }

        public WorkflowEngine(IDataContext dataContext, Guid userId) : this(dataContext, null, userId) { }

        //public WorkflowEngine() : this(null, Guid.Empty) { }

        public WorkflowEngine(Guid userId) : this(null, userId) { }

        public WorkflowEngine(IAppServiceProvider provider, IDataContext dataContext)
        {
            Provider = provider;
            DataContext = dataContext; //provider.Get<IDataContext>();

            var userData = provider.Get<IUserDataProvider>();
            UserId = userData.UserId;

            Repository = provider.Get<IWorkflowRepository>();
        }

        private IWorkflowRepository Repository { get; set; }

        #region IWorkflowEngine Members

        /// <summary>
        /// Запускает процесс на выполенение
        /// </summary>
        /// <param name="processId">Идентификатор процесса</param>
//        /// <param name="userId">Идентификатор пользователя запустившего процесс</param>
        /// <param name="processParameters">Параметры переданные в процесс</param>
        /// <returns>Результат выполения процесса (промежуточный, или конечный)</returns>
        public WorkflowContextData Run(Guid processId, Dictionary<String, object> processParameters = null)
        {
            var process = Repository.LoadProcessById(processId);

            DataContext.BeginTransaction();
            try
            {
                var context = Run(process, /*userId,*/ processParameters);
                DataContext.Commit();
                if (context != null)
                    return context.Data;
            }
            catch
            {
                DataContext.Rollback();
                throw;
            }

            return null;
        }

        public WorkflowContextData Run(Guid processId, WorkflowContextData contextData)
        {
            var process = Repository.LoadProcessById(processId);

            DataContext.BeginTransaction();
            try
            {

                var context = Run(process, new WorkflowContext(contextData, Provider));
                DataContext.Commit();
                if (context != null)
                    return context.Data;
            }
            catch
            {
                DataContext.Rollback();
                throw;
            }

            return null;
        }

        public WorkflowContextData Continue(WorkflowContextData contextData)
        {
            var context = new WorkflowContext(contextData, Provider);

            DataContext.BeginTransaction();
            try
            {
                var returnContext = RunActivities(context);
                DataContext.Commit();
                if (returnContext != null)
                    return returnContext.Data;
            }
            catch
            {
                DataContext.Rollback();
                throw;
            }
            return null;
        }

        public ExternalProcessExecuteResult RunGate(string gateName, WorkflowContextData contextData)
        {
            //var requestContext = new WorkflowContext(contextData, Provider);
            var result = new ExternalProcessExecuteResult {Data = contextData, Type = ExternalProcessExecuteResultType.Failed};
            try
            {
                var processGate = Repository.LoadGateByName(gateName);
                if (processGate != null)
                {
                    var process = Repository.LoadProcessById(processGate.ProcessId);

                    if (process != null)
                    {
                        var userProvider = Provider.Get<IUserDataProvider>();
                        var processContextData = new WorkflowContextData(process, contextData, userProvider.UserId);

                        var processContext = Run(process, new WorkflowContext(processContextData, Provider));
                        result.ReturnData = processContext.Data;
                        result.Type = ExternalProcessExecuteResultType.Success;

                        /*var responseContext = new WorkflowContext(processContext.Parent, Provider);
                    responseContext.ProcessReturn(processContext.Data);*/

                        return result;
                    }
                    result.Type = ExternalProcessExecuteResultType.ProcessNotFound;
                    return result;
                }

                result.Type = ExternalProcessExecuteResultType.GateNotFound;
                return result;
            }
            catch (Exception e)
            {
                result.Type = ExternalProcessExecuteResultType.Failed;
                result.Message = e.Message;

                try
                {
                    using (var writer = new StreamWriter(Logger.GetLogFileName("WorkflowGate"), true))
                    {
                        writer.WriteLine("{0}: \"{1}\", \"{2}\": '{3}' message: \"{4}\"", DateTime.Now,
                            contextData.ProcessName, contextData.UserId, GetType().Name, e.Message);
                        if (e.InnerException != null)
                            writer.WriteLine("  - inner exception: \"{0}\"", e.InnerException.Message);
                        writer.WriteLine("  -- Stack: {0}", e.StackTrace);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                return result;
            }
        }

        public WorkflowContextData RunExternalProcess(WorkflowGateRef gateRef, string processName, WorkflowContextData contextData)
        {
            var context = new WorkflowContext(contextData, Provider);

            if (gateRef != null)
            {
                try
                {
                    var launcher = Provider.Get<IExternalProcessLauncher>();
                    context.Data.ReturnData = null;
                    var result = launcher.Launch(gateRef, processName, Clone(contextData));
                    context.Data.ReturnData = new ExternalProcessReturnData
                    {
                        ResultType = result.Type,
                        Correct = false,
                        WithException = false
                    };

                    switch (result.Type)
                    {
                        case ExternalProcessExecuteResultType.Success:
                            context.ProcessReturn(result.ReturnData);

                            if (result.ReturnData.State == WorkflowRuntimeState.Finish)
                            {
                                context.ReturnData.Correct = true;
                                return context.Data;
                            }
                            if (result.ReturnData.State != WorkflowRuntimeState.Exception)
                            {
                                /*context.ThrowException("Process invalid state",
                                    String.Format("Процесс \"{0}\", на внешнем сервисе, вернул неверный статус завершения", processName));*/
                                context.ReturnData.ExceptionMessage =
                                    String.Format(
                                        "Процесс \"{0}\", на внешнем сервисе, вернул неверный статус завершения",
                                        processName);
                                return context.Data;
                            }
                            /*context.ThrowException("Process failed",
                                String.Format("Ошибка при выполнении процесса \"{0}\" на внешнем сервисе: {1}", processName, result.ReturnData.ExceptionName));*/
                            context.ReturnData.WithException = true;
                            context.ReturnData.ExceptionMessage =
                                String.Format("Ошибка при выполнении процесса \"{0}\" на внешнем сервисе: {1}",
                                    processName, result.ReturnData.ExceptionName);
                            return context.Data;

                        case ExternalProcessExecuteResultType.GateNotFound:
                            context.ThrowException("Gate not Found",
                                result.Message ?? "Интерфейс доступа на внешнем сервисе не найден");
                            break;

                        case ExternalProcessExecuteResultType.ProcessNotFound:
                            context.ThrowException("Process not Found",
                                result.Message ?? "Процесс на внешнем сервисе не найден");
                            break;

                        case ExternalProcessExecuteResultType.Failed:
                            context.ThrowException("Process failed",
                                result.Message ?? "Ошибка при выполнении процесса на внешнем сервисе");
                            break;
                    }
                }
                catch (Exception e)
                {
                    var msg = String.Format("Произошла ошибка при вызове внешнего процесса \"{0}\"", processName);
                    context.ThrowException(e.Message, msg);
                    var fileName = Logger.GetLogFileName("ExProccessLaunch");
                    Logger.OutputLog(fileName, e, msg);
                    return context.Data;
                }
            }
            else 
                context.ThrowException("No Process Gate", "Не могу запустить внешний процесс, индерфейс доступа к внешнему сервису не найден!");

            return context.Data;
        }

        private static WorkflowContextData Clone(WorkflowContextData contextData)
        {
            var cloner = new WorkflowContextDataCloner();
            return cloner.Clone(contextData);
        }

        #endregion

        private WorkflowContext Run(WorkflowProcess process, Dictionary<String, object> processParameters)
        {
            var contextData = new WorkflowContextData(process != null ? process.Id : Guid.Empty, UserId);
            var context = new WorkflowContext(contextData, Provider);

            context.SetVariables(processParameters);

            if (process != null)
            {
                if (process.StartActivityId == Guid.Empty)
                    throw new ApplicationException("Для процесса не задан StartActivity");

                ScriptManager.LoadScript(process.Script);

                context.RunActivity(process.StartActivityId);

                return RunActivities(context);
            }

            context.ThrowException("No Process", "Процесс не существует!");

            return context;
        }

        private WorkflowContext Run(WorkflowProcess process, WorkflowContext context)
        {
            if (process != null)
            {
                ScriptManager.LoadScript(process.Script);

                context.RunActivity(process.StartActivityId);

                return RunActivities(context);
            }

            context.ThrowException("No Process", "Процесс не существует!");

            return RunProcessActivities(context);
        }

        private WorkflowContext RunSubProcess(WorkflowProcess process, WorkflowContext context)
        {
            if (process != null)
            {
                ScriptManager.LoadScript(process.Script);

                context.RunActivity(process.StartActivityId);

                return RunProcessActivities(context);
            }

            context.ThrowException("No Process", "Процесс не существует!");

            return RunProcessActivities(context);
        }

        private WorkflowContext Continue(WorkflowContext context, Guid activityId)
        {
            var activity = Repository.LoadActivityById(activityId);

            if (activity == null)
                context.ThrowException("No Activity", "Операции не существует!");
            else
            {
                new WorkflowActivityEngine(activity, Provider, DataContext).Execute(context);
            }

            return RunProcessActivities(context);
        }

        [Obsolete("Устаревший метод. Метод больше не используется!")]
        private WorkflowContext RunNext(WorkflowContext context)
        {
            if (context.State == WorkflowRuntimeState.Finish)
            {
                if (context.Parent != null && context.Parent.State == WorkflowRuntimeState.ProcessCall)
                {
                    var callContext = new WorkflowContext(context.Parent, Provider);
                    callContext.ProcessReturn(context.Data);

                    return RunNext(callContext);
                }

                return context;
            }

            if (context.State == WorkflowRuntimeState.Run)
            {
                return Continue(context, context.ActivityId /*CurrentActivity.Id*/);
            }

            if (context.State == WorkflowRuntimeState.ProcessCall)
            {
                var process = Repository.LoadProcessById(context.ProcessId ?? Guid.Empty);

                if (process != null)
                {
                    var processContext = new WorkflowContextData(process, context.Data);

                    return Run(process, new WorkflowContext(processContext, Provider));
                }
                context.ThrowException("No Process", "Процесс не найден");
            }

            if (context.State == WorkflowRuntimeState.GateProcessCall)
            {
                var gateRef = Repository.LoadGateRefById(context.GateId ?? Guid.Empty);

                if (gateRef != null)
                {
                    context = new WorkflowContext(RunExternalProcess(gateRef, context.GateProcessName, context.Data), Provider);
                    return RunNext(context);
                }
                context.ThrowException("No GateRef", "Ссылка на шлюз внешнего процесса не найдена");
            }

            if (context.State == WorkflowRuntimeState.ProcessReturn)
            {
                var activity = Repository.LoadActivityById(context.ActivityId /*CurrentActivity.Id*/);

                if (activity != null)
                {
                    new WorkflowActivityEngine(activity, Provider, DataContext).AfterExecution(context);

                    return RunNext(context);
                }
                context.ThrowException("No Activity", "Действие не найдено");
            }

            if (context.State == WorkflowRuntimeState.ShowReturn || 
                context.State == WorkflowRuntimeState.ShowSelectReturn ||
                context.State == WorkflowRuntimeState.UploadFileReturn)
            {
                var activity = Repository.LoadActivityById(context.ActivityId);

                if (activity != null)
                {
                    new WorkflowActivityEngine(activity, Provider, DataContext).AfterExecution(context);

                    return RunNext(context);
                }
                context.ThrowException("No Activity", "Действие не найдено");
            }

            if (context.State == WorkflowRuntimeState.Exception)
            {
                return HandleException(context);
            }
            /*
                        if (context.State == WorkflowRuntimeState.ShowForm)
                            return context;
                        if (context.State == WorkflowRuntimeState.ShowMessage)
                            return context;
                        if (context.State == WorkflowRuntimeState.ShowReport)
                            return context;
                        if (context.State == WorkflowRuntimeState.SendFile)
                            return context;
            */
            return context;
        }

        private WorkflowContext RunActivities(WorkflowContext context)
        {
            var returnContext = RunProcessActivities(context);

            while (returnContext.State == WorkflowRuntimeState.Run ||
                   returnContext.State == WorkflowRuntimeState.ProcessCall ||
                   returnContext.State == WorkflowRuntimeState.GateProcessCall ||
                   returnContext.State == WorkflowRuntimeState.ProcessReturn ||
                   returnContext.State == WorkflowRuntimeState.ShowReturn ||
                   returnContext.State == WorkflowRuntimeState.ShowSelectReturn ||
                   returnContext.State == WorkflowRuntimeState.UploadFileReturn)
                returnContext = RunProcessActivities(returnContext);

            return returnContext;
        }

        private WorkflowContext RunProcessActivities(WorkflowContext context)
        {
            while (true)
            {
                switch (context.State)
                {
                    case WorkflowRuntimeState.Finish:
                        if (context.Parent != null && context.Parent.State == WorkflowRuntimeState.ProcessCall)
                        {
                            var callContext = new WorkflowContext(context.Parent, Provider);
                            callContext.ProcessReturn(context.Data);

                            context = callContext;
                            break;
                        }
                        return context;

                    case WorkflowRuntimeState.Run:
                        var runActivity = Repository.LoadActivityById(context.ActivityId);
                        if (runActivity == null)
                            context.ThrowException("No Activity", "Операции не существует!");
                        else
                            new WorkflowActivityEngine(runActivity, Provider, DataContext).Execute(context);
                        break;

                    case WorkflowRuntimeState.ProcessCall:
                        var process = Repository.LoadProcessById(context.ProcessId ?? Guid.Empty);
                        if (process != null)
                        {
                            var processContext = new WorkflowContextData(process, context.Data);

                            context = RunSubProcess(process, new WorkflowContext(processContext, Provider));

                            /*if (context.State == WorkflowRuntimeState.Finish)
                            {
                                context.ProcessReturn(returnedContext.Data);
                                context.Data.ReturnedExceptionFlag = false;
                            }
                            else if (returnedContext.State == WorkflowRuntimeState.Exception)
                            {
                                context.ProcessReturn(returnedContext.Data);
                                context.Data.ReturnedExceptionFlag = true;
                            }*/
                        }
                        else
                            context.ThrowException("No Process", "Процесс не найден");
                        break;

                    case WorkflowRuntimeState.GateProcessCall:
                        var gateRef = Repository.LoadGateRefById(context.GateId ?? Guid.Empty);
                        if (gateRef != null)
                            context = new WorkflowContext(
                                RunExternalProcess(gateRef, context.GateProcessName, context.Data), Provider);
                        else
                            context.ThrowException("No GateRef", "Ссылка на шлюз внешнего процесса не найдена");
                        break;

                    case WorkflowRuntimeState.ProcessReturn:
                        var processReturnActivity = Repository.LoadActivityById(context.ActivityId);
                        if (processReturnActivity != null)
                            new WorkflowActivityEngine(processReturnActivity, Provider, DataContext)
                                .AfterExecution(context);
                        else
                            context.ThrowException("No Activity", "Действие не найдено");
                        break;

                    case WorkflowRuntimeState.ShowReturn:
                    case WorkflowRuntimeState.ShowSelectReturn:
                    case WorkflowRuntimeState.UploadFileReturn:
                        var returnActivity = Repository.LoadActivityById(context.ActivityId);
                        if (returnActivity != null)
                            new WorkflowActivityEngine(returnActivity, Provider, DataContext)
                                .AfterExecution(context);
                        else
                            context.ThrowException("No Activity", "Действие не найдено");
                        break;

                    case WorkflowRuntimeState.Exception:
                        return HandleException(context);

                    default:
                        return context;
                }
            }
        }

        private WorkflowContext HandleException(WorkflowContext context)
        {
            /*if (context.Parent != null) // Закомментировано 2015-10-19 из-за неправильной реакции при вызовах внешних процессов
            {
                var activity = Repository.LoadActivityById(context.Parent.ActivityId /*CurrentActivity.Id#1#);

                var parentContext = new WorkflowContext(context.Parent, Provider);

                if (activity != null) activity.HandleException(parentContext, context.ExceptionName, context.Message);
                else
                {
                    parentContext.ThrowException(context.ExceptionName, context.Message);

                    return HandleException(parentContext);
                }

                return parentContext;  //RunNext(parentContext);
            }*/

            return context;
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class WorkflowContextDataCloner : IObjectCloner<WorkflowContextData>
    {
        public WorkflowContextData Clone(WorkflowContextData contextData)
        {
            return Deserialize(Serialize(contextData));
        }

        private static WorkflowContextData Deserialize(string data)
        {
            using (var read = new StringReader(data))
            {
                var serializer = new XmlSerializer(typeof(WorkflowContextData));
                using (var reader = new XmlTextReader(read))
                {
                    return (WorkflowContextData)serializer.Deserialize(reader);
                }
            }
        }

        private static string Serialize(WorkflowContextData data)
        {
            var serializer = new XmlSerializer(typeof(WorkflowContextData));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, data);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
    }
}