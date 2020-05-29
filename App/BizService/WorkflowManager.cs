using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Intersoft.CISSA.BizService.Utils;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.BizService
{
    public partial class BizService
    {
        private IWorkflowEngine WorkflowEngine { get; set; }
        private IWorkflowRepository WorkflowRepo { get; set; }

        public BizService(IWorkflowEngine workflowEngine, string currentUserName)
        {
            WorkflowEngine = workflowEngine;
            CurrentUserName = currentUserName;
        }

        /// <summary>
        /// Запуск бизнес-процесса
        /// </summary>
        /// <param name="processId">Идентификатор бизнес-процесса</param>
        /// <param name="processParameters">Входные параметры для бизнес-процесса</param>
        /// <returns>Контекст исполнения бизнес-процесса</returns>
        public WorkflowContextData WorkflowExecute(Guid processId, Dictionary<String, object> processParameters)
        {
            //var user = UserRepo.GetUserInfo(CurrentUserName);
            return WorkflowEngine.Run(processId, processParameters);
        }

        /// <summary>
        /// Продолжение выполнения бизнес-процесса с учетом пользовательского действия
        /// </summary>
        /// <param name="contextData">Контекст выполнения бизнес-процесса</param>
        /// <param name="userAction">Идентификатор пользовательского действия</param>
        /// <returns>Контекст выполнения бизнес-процесса</returns>
        public WorkflowContextData WorkflowContinueWithUserAction(WorkflowContextData contextData, Guid userAction /*, Guid? forDocId */)
        {
            var context = new WorkflowContext(contextData, Provider);

            if (contextData != null) context.ShowReturn(userAction);

            return WorkflowEngine.Continue(contextData);
        }

        /// <summary>
        /// Продолжение выполнения бизнес-процесса с учетом документа
        /// </summary>
        /// <param name="contextData">Контекст выполнения бизнес-процесса</param>
        /// <param name="docId">Идентификатор документа</param>
        /// <returns>Контекст выполнения бизнес-процесса</returns>
        public WorkflowContextData WorkflowContinueWithDocumentId(WorkflowContextData contextData, Guid docId)
        {
            var context = new WorkflowContext(contextData, Provider);
            if (contextData != null) context.ShowSelectReturn(docId);

            return WorkflowEngine.Continue(contextData);
        }

        /// <summary>
        /// Продолжение выполнения бизнес-процесса с учетом документа
        /// </summary>
        /// <param name="contextData">Контекст выполнения бизнес-процесса</param>
        /// <param name="document">Документ</param>
        /// <returns>Контекст выполнения бизнес-процесса</returns>
        public WorkflowContextData WorkflowContinueWithDocument(WorkflowContextData contextData, Doc document)
        {
            if (contextData != null)
            {
                var context = new WorkflowContext(contextData, Provider);
                contextData.SelectedDocument = document;
                context.ShowSelectReturn(document != null ? document.Id : Guid.Empty);
            }

            return WorkflowEngine.Continue(contextData);
        }

        /// <summary>
        /// Продолжение выполнения бизнес-процесса
        /// </summary>
        /// <param name="contextData">Контекст выполнения бизнес-процесса</param>
        /// <returns>Контекст выполнения бизнес-процесса</returns>
        public WorkflowContextData WorkflowContinue(WorkflowContextData contextData)
        {
            var context = new WorkflowContext(contextData, Provider);
            if (contextData != null) context.ShowReturn(null);

            return WorkflowEngine.Continue(contextData);
        }

        public WorkflowContextData WorkflowContinueWithUploadFile(WorkflowContextData contextData, byte[] uploadData, string fileName)
        {
            var context = new WorkflowContext(contextData, Provider);
            if (contextData != null) context.UploadFileReturn(uploadData, fileName);

            return WorkflowEngine.Continue(contextData);
        }

        public ExternalProcessExecuteResult WorkflowGateExecute(string gateName, WorkflowContextData contextData)
        {
            return WorkflowEngine.RunGate(gateName, contextData);
        }
    }

    public partial class BizService
    {
        internal static IList<WorkflowProcessExecutionTask> ProcessTasks = new List<WorkflowProcessExecutionTask>();
        // internal static object WorkflowProcessTaskLock = new object();
        internal static readonly ReaderWriterLock ProcessTaskLock = new ReaderWriterLock();
        private const int LockTimeout = 500000;

        public Guid ExecuteProcess(Guid processId, Dictionary<string, object> processParameters)
        {
            var contextData = new WorkflowContextData(processId, CurrentUserId);
            var context = new WorkflowContext(contextData, Provider);
            context.SetVariables(processParameters);

            var userName = CurrentUserName;
            var userId = CurrentUserId;
            var taskId = Guid.NewGuid();
            var task = new WorkflowProcessExecutionTask(taskId, contextData, () =>
            {
                var taskMonitor = new DataAccessLayer.Model.Misc.Monitor("Process Task "/* + processId.ToString()*/);
                try
                {
                    var dataContextFactory = DataContextFactoryProvider.GetFactory();
                    using (
                        var dataContext =
                            dataContextFactory.CreateMultiDc(BaseServiceFactory.DataContextConfigSectionName))
                    {
                        var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                        using (var provider = providerFactory.Create(dataContext))
                        {
                            var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();
                            serviceRegistrator.AddService(new UserDataProvider(userId, userName));

                            var workflowEngine = provider.Get<IWorkflowEngine>();

                            CompleteTask(taskId, workflowEngine.Run(processId, contextData));
                        }
                    }
                    taskMonitor.Complete();
                }
                catch (Exception e)
                {
                    CompleteTaskException(taskId, e);
                    taskMonitor.CompleteWithException(e);
                }
                /*
                for (var i = 0; i < 200; i++)
                {
                    Thread.Sleep(100);
                }
                CompleteTask(taskId, contextData);*/
            });

            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireWriterLock(LockTimeout);
            try
            {
                ProcessTasks.Add(task);
            }
            finally
            {
                ProcessTaskLock.ReleaseWriterLock();
            }
            task.Start();
            return task.Id;
        }

        public Guid ContinueProcess(WorkflowContextData contextData)
        {
            var context = new WorkflowContext(contextData, Provider);
            if (contextData == null) return Guid.Empty;

            context.ShowReturn(null);

            var userName = CurrentUserName;
            var userId = CurrentUserId;
            var taskId = Guid.NewGuid();
            var task = new WorkflowProcessExecutionTask(taskId, contextData, () =>
            {
                var taskMonitor = new DataAccessLayer.Model.Misc.Monitor("Continue Process Task"/* + contextData.ParentProcessId.ToString()*/);
                try
                {
                    var dataContextFactory = DataContextFactoryProvider.GetFactory();
                    using (
                        var dataContext =
                            dataContextFactory.CreateMultiDc(BaseServiceFactory.DataContextConfigSectionName))
                    {
                        var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                        using (var provider = providerFactory.Create(dataContext))
                        {
                            var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();
                            serviceRegistrator.AddService(new UserDataProvider(userId, userName));

                            var workflowEngine = provider.Get<IWorkflowEngine>();

                            CompleteTask(taskId, workflowEngine.Continue(contextData));
                        }
                    }
                    taskMonitor.Complete();
                }
                catch (Exception e)
                {
                    CompleteTaskException(taskId, e);
                    taskMonitor.CompleteWithException(e);
                }
            });

            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireWriterLock(LockTimeout);
            try
            {
                ProcessTasks.Add(task);
            }
            finally
            {
                ProcessTaskLock.ReleaseWriterLock();
            }
            task.Start();
            return task.Id;
        }

        public Guid ContinueProcessWithUserAction(WorkflowContextData contextData, Guid userAction)
        {
            var context = new WorkflowContext(contextData, Provider);
            if (contextData == null) return Guid.Empty;

            context.ShowReturn(userAction);

            var userName = CurrentUserName;
            var userId = CurrentUserId;
            var taskId = Guid.NewGuid();
            var task = new WorkflowProcessExecutionTask(taskId, contextData, () =>
            {
                var taskMonitor = new DataAccessLayer.Model.Misc.Monitor("Continue Process Task with User Action"/* + contextData.ParentProcessId.ToString()*/);
                try
                {
                    var dataContextFactory = DataContextFactoryProvider.GetFactory();
                    using (
                        var dataContext =
                            dataContextFactory.CreateMultiDc(BaseServiceFactory.DataContextConfigSectionName))
                    {
                        var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                        using (var provider = providerFactory.Create(dataContext))
                        {
                            var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();
                            serviceRegistrator.AddService(new UserDataProvider(userId, userName));

                            var workflowEngine = provider.Get<IWorkflowEngine>();

                            CompleteTask(taskId, workflowEngine.Continue(contextData));
                        }
                    }
                    taskMonitor.Complete();
                }
                catch (Exception e)
                {
                    CompleteTaskException(taskId, e);
                    taskMonitor.CompleteWithException(e);
                }
            });

            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireWriterLock(LockTimeout);
            try
            {
                ProcessTasks.Add(task);
            }
            finally
            {
                ProcessTaskLock.ReleaseWriterLock();
            }
            task.Start();
            return task.Id;
        }

        public Guid ContinueProcessWithDocumentId(WorkflowContextData contextData, Guid docId)
        {
            var context = new WorkflowContext(contextData, Provider);
            if (contextData == null) return Guid.Empty;

            context.ShowSelectReturn(docId);

            var userName = CurrentUserName;
            var userId = CurrentUserId;
            var taskId = Guid.NewGuid();
            var task = new WorkflowProcessExecutionTask(taskId, contextData, () =>
            {
                var taskMonitor = new DataAccessLayer.Model.Misc.Monitor("Continue Process Task with DocId"/* + contextData.ParentProcessId.ToString()*/);
                try
                {
                    var dataContextFactory = DataContextFactoryProvider.GetFactory();
                    using (
                        var dataContext =
                            dataContextFactory.CreateMultiDc(BaseServiceFactory.DataContextConfigSectionName))
                    {
                        var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                        using (var provider = providerFactory.Create(dataContext))
                        {
                            var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();
                            serviceRegistrator.AddService(new UserDataProvider(userId, userName));

                            var workflowEngine = provider.Get<IWorkflowEngine>();

                            CompleteTask(taskId, workflowEngine.Continue(contextData));
                        }
                    }
                    taskMonitor.Complete();
                }
                catch (Exception e)
                {
                    CompleteTaskException(taskId, e);
                    taskMonitor.CompleteWithException(e);
                }
            });

            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireWriterLock(LockTimeout);
            try
            {
                ProcessTasks.Add(task);
            }
            finally
            {
                ProcessTaskLock.ReleaseWriterLock();
            }
            task.Start();
            return task.Id;
        }

        public Guid ContinueProcessWithDocument(WorkflowContextData contextData, Doc document)
        {
            var context = new WorkflowContext(contextData, Provider);
            if (contextData == null) return Guid.Empty;

            context.ShowSelectReturn(document);

            var userName = CurrentUserName;
            var userId = CurrentUserId;
            var taskId = Guid.NewGuid();
            var task = new WorkflowProcessExecutionTask(taskId, contextData, () =>
            {
                var taskMonitor = new DataAccessLayer.Model.Misc.Monitor("Continue Process Task with Doc"/* + contextData.ParentProcessId.ToString()*/);
                try
                {
                    var dataContextFactory = DataContextFactoryProvider.GetFactory();
                    using (
                        var dataContext =
                            dataContextFactory.CreateMultiDc(BaseServiceFactory.DataContextConfigSectionName))
                    {
                        var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                        using (var provider = providerFactory.Create(dataContext))
                        {
                            var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();
                            serviceRegistrator.AddService(new UserDataProvider(userId, userName));

                            var workflowEngine = provider.Get<IWorkflowEngine>();

                            CompleteTask(taskId, workflowEngine.Continue(contextData));
                        }
                    }
                    taskMonitor.Complete();
                }
                catch (Exception e)
                {
                    CompleteTaskException(taskId, e);
                    taskMonitor.CompleteWithException(e);
                }
            });

            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireWriterLock(LockTimeout);
            try
            {
                ProcessTasks.Add(task);
            }
            finally
            {
                ProcessTaskLock.ReleaseWriterLock();
            }
            task.Start();
            return task.Id;
        }

        public Guid ContinueProcessWithUploadFile(WorkflowContextData contextData, byte[] uploadData, string fileName)
        {
            var context = new WorkflowContext(contextData, Provider);
            if (contextData == null) return Guid.Empty;

            context.UploadFileReturn(uploadData, fileName);

            var userName = CurrentUserName;
            var userId = CurrentUserId;
            var taskId = Guid.NewGuid();
            var task = new WorkflowProcessExecutionTask(taskId, contextData, () =>
            {
                var taskMonitor = new DataAccessLayer.Model.Misc.Monitor("Continue Process Task with Upload file"/* + contextData.ParentProcessId.ToString()*/);
                try
                {
                    var dataContextFactory = DataContextFactoryProvider.GetFactory();
                    using (
                        var dataContext =
                            dataContextFactory.CreateMultiDc(BaseServiceFactory.DataContextConfigSectionName))
                    {
                        var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                        using (var provider = providerFactory.Create(dataContext))
                        {
                            var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();
                            serviceRegistrator.AddService(new UserDataProvider(userId, userName));

                            var workflowEngine = provider.Get<IWorkflowEngine>();

                            CompleteTask(taskId, workflowEngine.Continue(contextData));
                        }
                    }
                    taskMonitor.Complete();
                }
                catch (Exception e)
                {
                    CompleteTaskException(taskId, e);
                    taskMonitor.CompleteWithException(e);
                }
            });

            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireWriterLock(LockTimeout);
            try
            {
                ProcessTasks.Add(task);
            }
            finally
            {
                ProcessTaskLock.ReleaseWriterLock();
            }
            task.Start();
            return task.Id;
        }

        public Guid ExecuteGate(string gateName, WorkflowContextData contextData)
        {
            throw new NotImplementedException();
        }

        public WorkflowProcessExecutionTaskState GetProcessTaskState(Guid taskId)
        {
            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireReaderLock(LockTimeout);
            try
            {
                var task = ProcessTasks.FirstOrDefault(t => t.Id == taskId);

                if (task == null) return null;

                return new WorkflowProcessExecutionTaskState
                {
                    State = task.State,
                    ExceptionInfo = task.ExceptionInfo,
                    Progress = new ProcessProgressInfo
                    {
                        Message = task.ContextData.Message
                    },
                    Created = task.Created,
                    ProcessId = task.ProcessId,
                    ProcessName = task.ProcessName
                };
            }
            finally
            {
                ProcessTaskLock.ReleaseReaderLock();
            }
        }

        public List<WorkflowProcessExecutionTaskState> GetProcessTaskStates()
        {
            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireReaderLock(LockTimeout);
            try
            {
                return ProcessTasks.Select(task =>
                    new WorkflowProcessExecutionTaskState
                    {
                        State = task.State,
                        ExceptionInfo = task.ExceptionInfo,
                        Progress = new ProcessProgressInfo
                        {
                            Message = task.ContextData.Message
                        },
                        Created = task.Created,
                        ProcessId = task.ProcessId,
                        ProcessName = task.ProcessName
                    }).ToList();
            }
            finally
            {
                ProcessTaskLock.ReleaseReaderLock();
            }
        }

        public WorkflowContextData EndProcessTask(Guid taskId)
        {
            try
            {
                WorkflowProcessExecutionTask task;
                WorkflowContextData contextData;

                // lock (WorkflowProcessTaskLock)
                ProcessTaskLock.AcquireReaderLock(LockTimeout);
                try
                {
                    task = ProcessTasks.FirstOrDefault(t => t.Id == taskId);

                    if (task == null) return null;
                    var lc = ProcessTaskLock.UpgradeToWriterLock(LockTimeout);
                    try
                    {
                        ProcessTasks.Remove(task);
                        contextData = task.ContextData;

                        if (task.Task != null) task.Task.Dispose();
                    }
                    finally
                    {
                        ProcessTaskLock.DowngradeFromWriterLock(ref lc);
                    }
                    return contextData;
                }
                finally
                {
                    ProcessTaskLock.ReleaseReaderLock();
                }

            }
            catch (Exception e)
            {
                Logger.OutputLog("WorkflowManager", e, "EndProcessTask");
            }
            return null;
        }

        public void TerminateProcessTask(Guid taskId)
        {
            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireReaderLock(LockTimeout);
            try
            {
                var processTask = ProcessTasks.FirstOrDefault(t => t.Id == taskId);

                if (processTask == null) return;
            }
            finally
            {
                ProcessTaskLock.ReleaseReaderLock();
            }
        }

        private static void CompleteTask(Guid taskId, WorkflowContextData contextData)
        {
            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireReaderLock(LockTimeout);
            try
            {
                var task = ProcessTasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null) return;
                var lc = ProcessTaskLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    task.State = WorkflowProcessExecutionTask.SUCCESS;
                    task.ContextData = contextData;
                }
                finally
                {
                    ProcessTaskLock.DowngradeFromWriterLock(ref lc);
                }
            }
            finally
            {
                ProcessTaskLock.ReleaseReaderLock();
            }
        }

        private void CompleteTaskException(Guid taskId, Exception e)
        {
            // lock (WorkflowProcessTaskLock)
            ProcessTaskLock.AcquireReaderLock(LockTimeout);
            try
            {
                var task = ProcessTasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null) return;
                var lc = ProcessTaskLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    task.State = WorkflowProcessExecutionTask.FAIL;
                    task.ExceptionInfo = GetExceptionInfo(e);
                }
                finally
                {
                    ProcessTaskLock.DowngradeFromWriterLock(ref lc);
                }
            }
            finally
            {
                ProcessTaskLock.ReleaseReaderLock();
            }
            Logger.OutputLog("WorkflowManager", e, "Execute/Continue Process");
        }

        private ExceptionInfo GetExceptionInfo(Exception e)
        {
            if (e != null)
                return new ExceptionInfo
                {
                    ExceptionName = e.GetType().Name,
                    Message = e.Message,
                    StackTrace = e.StackTrace,
                    InnerException = GetExceptionInfo(e.InnerException)
                };

            return null;
        }
    }
}