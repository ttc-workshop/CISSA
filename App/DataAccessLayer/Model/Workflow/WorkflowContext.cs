using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Repository;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class WorkflowContext : IStringParams, IAppServiceProvider
    {
        public IAppServiceProvider Provider { get; private set; }

        private readonly IDataContext _dataContext;

        public IDataContext DataContext
        {
            get
            {
                /*var multiDc = _dataContext as IMultiDataContext;
                if (multiDc != null) return multiDc.GetDocumentContext;*/

                return _dataContext; 
            }
        }

        public WorkflowContextData Data { get; private set; }

        private readonly IDocRepository _docRepo;
        private readonly IUserRepository _userRepo;
        private readonly IDocDefRepository _defRepo;
        private readonly IOrgRepository _orgRepo;
        private readonly IEnumRepository _enumRepo;

        private readonly ISqlQueryBuilderFactory _sqlQueryBuilderFactory;
        private readonly ISqlQueryReaderFactory _sqlQueryReaderFactory;

        // TODO: Remove this constructor and change tests
        [Obsolete("Устаревший конструктор! Необходимо удалить! Используется в тестах")]
        public WorkflowContext(WorkflowContextData data, IDataContext dataContext)
        {
            if (data == null) throw new ArgumentNullException(@"data");
            if (dataContext == null) throw new ArgumentNullException(@"dataContext");

            _dataContext = dataContext;
            Data = data;
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = factory.Create();

            /*_docRepo = new DocRepository(DataContext, UserId);
            _userRepo = new UserRepository(DataContext);
            _defRepo = new DocDefRepository(DataContext, UserId);
            _orgRepo = new OrgRepository(DataContext/*, Data.UserId#1#);
            _enumRepo = new EnumRepository(DataContext);*/

            _docRepo = Provider.Get<IDocRepository>();
            _userRepo = Provider.Get<IUserRepository>();
            _defRepo = Provider.Get<IDocDefRepository>();
            _orgRepo = Provider.Get<IOrgRepository>();
            _enumRepo = Provider.Get<IEnumRepository>();

            _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
            _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
        }

        public WorkflowContext(WorkflowContextData data, IAppServiceProvider provider)
        {
            if (data == null) throw new ArgumentNullException(@"data");
            if (provider == null) throw new ArgumentNullException(@"provider");

            Provider = provider;

            _dataContext = Provider.Find<IMultiDataContext>() ?? Provider.Get<IDataContext>();
            Data = data;

            _docRepo = Provider.Get<IDocRepository>();
            _userRepo = Provider.Get<IUserRepository>();
            _defRepo = Provider.Get<IDocDefRepository>();
            _orgRepo = Provider.Get<IOrgRepository>();
            _enumRepo = Provider.Get<IEnumRepository>();

            _sqlQueryBuilderFactory = Provider.Get<ISqlQueryBuilderFactory>();
            _sqlQueryReaderFactory = Provider.Get<ISqlQueryReaderFactory>();
        }

        public List<WorkflowVariable> Variables
        {
            get { return Data.Variables; }
            set { Data.Variables = value; }
        }

        public object this[string attributeName]
        {
            get
            {
                return GetVariable(attributeName);
            }
            set
            {
                SetVariable(attributeName, value);
            }
        }

        public WorkflowContextData Parent { get { return Data.Parent; } }

        public WorkflowRuntimeState State { get { return Data.State; } }

        public Guid ParentProcessId { get { return Data.ParentProcessId; } }

        public Guid? ProcessId { get { return Data.ProcessId; } }

        public string ProcessName { get { return Data.ProcessName; } }
        
        public Guid? GateId { get { return Data.GateId; } }

        public Guid UserId { get { return Data.UserId; } }

        public Guid ActivityId { get { return Data.ActivityId; } }

        public string GateProcessName { get { return Data.GateProcessName; } }

        public List<UserAction> UserActions { get { return Data.UserActions; } set { Data.UserActions = value; } }

        public Guid? CurrentFormId { get { return Data.CurrentFormId; } set { Data.CurrentFormId = value; } }

        public string FormCaption { get { return Data.FormCaption; } set { Data.FormCaption = value; } }

        public Guid ReportId { get { return Data.ReportId; } }

        public string ExceptionName { get { return Data.ExceptionName; } }

        public WorkflowContextData ReturnedContextData { get { return Data.ReturnedContextData; } }

        public WorkflowContext ReturnedContext { get { return Data.ReturnedContextData != null ? new WorkflowContext(Data.ReturnedContextData, Provider) : null; } }

        public bool ReturnedSuccessFlag { get { return Data.ReturnedSuccessFlag; } }

        public bool SuccessFlag
        {
            get { return Data.SuccessFlag; } 
            set { Data.SuccessFlag = value; }
        }

        public string Message { get { return Data.Message; } set { Data.Message = value; } }

        public Doc CurrentDocument
        {
            get
            {
                var data = Data;
                while (data != null)
                {
                    var doc = data.CurrentDocument;
                    if (doc != null)
                    {
                        if (data != Data) CurrentDocument = doc;
                        return doc;
                    }
                    data = data.Parent;
                }
                return null;
            }
            set { Data.CurrentDocument = value; }
        }

        public Guid? CurrentDocumentId
        {
            get
            {
                var data = Data;
                while (data != null)
                {
                    var docId = data.CurrentDocumentId;
                    if (docId != null)
                    {
                        if (data != Data) CurrentDocumentId = docId;
                        return docId;
                    }
                    data = data.Parent;
                }
                return null;
            }
            set { Data.CurrentDocumentId = value; }
        }

        public Guid? CurrentDocumentDefId
        {
            get
            {
                var data = Data;
                while (data != null)
                {
                    var defId = data.CurrentDocumentDefId;
                    if (defId != null)
                    {
                        if (data != Data) CurrentDocumentDefId = defId;
                        return defId;
                    }
                    data = data.Parent;
                }
                return null;
            }
            set { Data.CurrentDocumentDefId = value; }
        }

        public Guid? UserActionId { get { return Data.UserActionId; } }

        public Doc SelectedDocument
        {
            get
            {
                var data = Data;
                while (data != null)
                {
                    var doc = data.SelectedDocument;
                    if (doc != null)
                    {
                        if (data != Data) SelectedDocument = doc;
                        return doc;
                    }
                    data = data.Parent;
                }
                return null;
            }
            set { Data.SelectedDocument = value; }
        }

        public Guid? SelectedDocumentId
        {
            get
            {
                var data = Data;
                while (data != null)
                {
                    var docId = data.SelectedDocumentId;
                    if (docId != null)
                    {
                        if (data != Data) SelectedDocumentId = docId;
                        return docId;
                    }
                    data = data.Parent;
                }
                return null;
            }
            set { Data.SelectedDocumentId = value; }
        }

        public Doc FilterDocument
        {
            get
            {
                var data = Data;
                while (data != null)
                {
                    var doc = data.FilterDocument;
                    if (doc != null)
                    {
                        if (data != Data) FilterDocument = doc;
                        return doc;
                    }
                    data = data.Parent;
                }
                return null;
            }
            set { Data.FilterDocument = value; }
        }

        public List<Guid> DocumentList { get { return Data.DocumentList; } set { Data.DocumentList = value; } }

        public string TemplateFileName { get { return Data.TemplateFileName; } set { Data.TemplateFileName = value; } }

        public QueryDef CurrentQuery { get { return Data.CurrentQuery; } set { Data.CurrentQuery = value; } }

        public List<BizControl> ControlDatas { get { return Data.ControlDatas; } set { Data.ControlDatas = value; } }

        public Guid? FilterDocStateId { get { return Data.FilterDocStateId; } set { Data.FilterDocStateId = value; } }

        public List<ModelMessage> ErrorMessages { get { return Data.ErrorMessages; } }

        public List<BizFormOptions> FormOptions { get { return Data.FormOptions; } }

        public byte[] UploadFileData { get { return Data.UploadFileData; } }
        public string UploadFileName { get { return Data.UploadFileName; } }

        public List<FileData> DownloadFiles { get { return Data.DownloadFiles; } }

        public FileData AddDownloadFile(string fileName, byte[] fileData)
        {
            var result = new FileData {FileName = fileName, Data = fileData};

            if (Data.DownloadFiles == null)
                Data.DownloadFiles = new List<FileData>();

            Data.DownloadFiles.Add(result);
            return result;
        }

        public ExternalProcessReturnData ReturnData { get { return Data.ReturnData; } }

        public bool PreviewForm { get { return Data.PreviewForm; } set { Data.PreviewForm = value; } }

        public Guid PreviewFormId { get { return Data.PreviewFormId; } set { Data.PreviewFormId = value; } }

        public void ShowForm(Guid? formId)
        {
            Data.State = WorkflowRuntimeState.ShowForm;

            CurrentFormId = formId;
        }

        public void ShowSelectForm(Guid? formId)
        {
            Data.State = WorkflowRuntimeState.ShowSelectForm;

            CurrentFormId = formId;
        }

        public void ShowParamForm(Guid? formId)
        {
            Data.State = WorkflowRuntimeState.ShowParamForm;

            CurrentFormId = formId;
        }

        public void RunActivity(Guid activityId)
        {
            Data.State = WorkflowRuntimeState.Run;

            Data.ActivityId = activityId;

            var user = GetUserInfo();

            // using (var repo = new WorkflowRepository(DataContext))
            var repo = Provider.Get<IWorkflowRepository>();
            UserActions = new List<UserAction>(repo.GetActivityUserActions(activityId, user.LanguageId));
        }

        public void ShowReport(Guid reportId)
        {
            Data.State = WorkflowRuntimeState.ShowReport;

            Data.ReportId = reportId;
        }

        public void ShowTemplateReport(string fileName)
        {
            Data.State = WorkflowRuntimeState.ShowTemplateReport;

            Data.TemplateFileName = fileName;
        }

        public void SendFile(string fileName)
        {
            Data.State = WorkflowRuntimeState.SendFile;

            Data.TemplateFileName = fileName;
        }

        public void UploadFile(string message)
        {
            Data.State = WorkflowRuntimeState.UploadFile;

            Data.Message = message;
            Data.UploadFileData = null;
            Data.UploadFileName = String.Empty;
        }

        public void ShowMessage(string message)
        {
            Data.State = WorkflowRuntimeState.ShowMessage;

            Data.Message = message;
        }

        public void ThrowException(string exceptionName, string message)
        {
            Data.State = WorkflowRuntimeState.Exception;

            Data.ExceptionName = String.IsNullOrEmpty(exceptionName) ? "WorkflowException" : exceptionName;
            Data.Message = message;
        }

        public void ThrowException(Exception e)
        {
            Data.State = WorkflowRuntimeState.Exception;

            if (e != null)
            {
                Data.ExceptionName = e.GetType().Name;
                Data.Message = e.Message;
                var ie = e.InnerException;
                while (ie != null)
                {
                    Data.Message += "\n" + ie.Message;
                    ie = ie.InnerException;
                }
            }
            else
            {
                Data.ExceptionName = "WorkflowException";
                Data.Message = "Неизвестное исключение";
            }
        }

        public void Finish()
        {
            Data.State = WorkflowRuntimeState.Finish;

            Data.CurrentFormId = null;
            Data.Message = String.Empty;
        }

        public void Finish(Guid formId)
        {
            Data.State = WorkflowRuntimeState.Finish;

            Data.CurrentFormId = formId;
            Data.Message = String.Empty;
        }

        public void Finish(string message)
        {
            Data.State = WorkflowRuntimeState.Finish;

            Data.CurrentFormId = null;
            Data.Message = message;
        }

        public void CallProcess(Guid processId)
        {
            Data.State = WorkflowRuntimeState.ProcessCall;

            Data.ProcessId = processId;
        }

        public void CallGateProcess(Guid gateId, string processName)
        {
            Data.State = WorkflowRuntimeState.GateProcessCall;

            Data.GateId = gateId;
            Data.GateProcessName = processName;
        }

        public void ProcessReturn(WorkflowContextData returnContextData)
        {
            Data.State = WorkflowRuntimeState.ProcessReturn;

            Data.ReturnedContextData = returnContextData;
            Data.ReturnedContextData.Parent = null;
            Data.ReturnedSuccessFlag = ReturnedContextData.SuccessFlag;
        }

        public void ShowReturn(Guid? userActionId)
        {
            Data.State = WorkflowRuntimeState.ShowReturn;

            Data.UserActionId = userActionId;
        }

        public void ShowSelectReturn(Guid docId)
        {
            Data.State = WorkflowRuntimeState.ShowSelectReturn;

            Data.SelectedDocumentId = docId;
        }

        public void ShowSelectReturn(Doc document)
        {
            Data.State = WorkflowRuntimeState.ShowSelectReturn;

            Data.SelectedDocument = document;
        }

        public void UploadFileReturn(byte[] uploadData, string fileName)
        {
            Data.State = WorkflowRuntimeState.UploadFileReturn;

            Data.UploadFileData = uploadData;
            Data.UploadFileName = fileName;
        }

        public UserInfo GetUserInfo()
        {
            return _userRepo.GetUserInfo(UserId);
        }

        private static object GetVariableFrom(WorkflowContextData data, string name)
        {
            if (data == null || data.Variables == null) return null;

            var variable = data.Variables.Find(v => String.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase));

            var objectVariable = variable as ObjectVariable;
            if (objectVariable != null) return objectVariable.Value;
            var documentVariable = variable as DocumentVariable;
            if (documentVariable != null) return documentVariable.Value;
            var attributeVariable = variable as AttributeVariable;
            if (attributeVariable != null) return attributeVariable.Value;
            var valueVariable = variable as EnumValueVariable;
            if (valueVariable != null) return valueVariable.Value;
            var listVariable = variable as ObjectListVariable;
            if (listVariable != null) return listVariable.Value;
            var docListVariable = variable as DocListVariable;
            if (docListVariable != null) return docListVariable.Value;

            return null;
        }

        public object GetVariable(string name)
        {
            var value = GetVariableFrom(Data, name);

            if (value != null) return value;

            var parent = Parent;
            while (parent != null)
            {
                value = GetVariableFrom(parent, name);
                if (value != null) return value;
                parent = parent.Parent;
            }

            return null;
        }

        protected List<WorkflowVariable> GetVariablesContain(string name)
        {
            if (Data.Variables.Exists(v => String.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase)))
                return Data.Variables;

            var parent = Parent;
            while (parent != null)
            {
                if (parent.Variables.Exists(v => String.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase)))
                    return parent.Variables;
                parent = parent.Parent;
            }
            return null;
        }

        public void SetLocalVariable(string name, object value)
        {
            var v =
                Data.Variables.FirstOrDefault(i => String.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));

            if (v != null) Data.Variables.Remove(v);

            var doc = value as Doc;
            if (doc != null)
                Data.Variables.Add(new DocumentVariable {Name = name, Value = doc});
            else if (value is AttributeBase)
                Data.Variables.Add(new AttributeVariable {Name = name, Value = (AttributeBase) value});
            else if (value is EnumValue)
                Data.Variables.Add(new EnumValueVariable {Name = name, Value = (EnumValue) value});
            else if (value is IEnumerable)
            {
                if (value.GetType().IsGenericType)
                {
                    var args = value.GetType().GetGenericArguments();
                    if (args.Length > 1)
                        throw new ApplicationException(
                            "Сложный дженерик список не поддерживается в качестве переменной в контексте бизнес-процесса!");

                    if (args[0].IsValueType || args[0] == typeof (String))
                    {
                        Data.Variables.Add(new ObjectListVariable
                        {
                            Name = name,
                            Value = ((IEnumerable) value).Cast<object>().ToList()
                        });
                    }
                    else if (args[0] == typeof (Doc))
                        Data.Variables.Add(new DocListVariable
                        {
                            Name = name,
                            Value = ((IEnumerable) value).Cast<Doc>().ToList()
                        });
                }
                else
                    Data.Variables.Add(new ObjectListVariable
                    {
                        Name = name,
                        Value = ((IEnumerable) value).Cast<object>().ToList()
                    });
            }
            else
                Data.Variables.Add(new ObjectVariable {Name = name, Value = value});
        }

        public void SetVariable(string name, object value)
        {
            var vars = GetVariablesContain(name);
            if (vars != null)
            {
                var v = vars.Find(i => String.Equals(i.Name, name, StringComparison.OrdinalIgnoreCase));
                vars.Remove(v);
            }
            else
            {
                vars = Data.Variables;
            }
            if (value is Doc)
                vars.Add(new DocumentVariable { Name = name, Value = (Doc)value });
            else if (value is AttributeBase)
                vars.Add(new AttributeVariable { Name = name, Value = (AttributeBase)value });
            else if (value is EnumValue)
                vars.Add(new EnumValueVariable { Name = name, Value = (EnumValue)value });
            //            else if (value is DocQuery)
            //                vars.Add(new QueryVariable { Name = name, Value = (DocQuery) value });
            else if (value is String)
                vars.Add(new ObjectVariable {Name = name, Value = value});
            else if (value is IEnumerable)
            {
                if (value.GetType().IsGenericType)
                {
                    var args = value.GetType().GetGenericArguments();
                    if (args.Length > 1)
                        throw new ApplicationException(
                            "Сложный дженерик список не поддерживается в качестве переменной в контексте бизнес-процесса!");

                    if (args[0].IsValueType || args[0] == typeof (String))
                    {
                        Data.Variables.Add(new ObjectListVariable
                        {
                            Name = name,
                            Value = ((IEnumerable) value).Cast<object>().ToList()
                        });
                    }
                    else if (args[0] == typeof (Doc))
                        Data.Variables.Add(new DocListVariable
                        {
                            Name = name,
                            Value = ((IEnumerable) value).Cast<Doc>().ToList()
                        });
                    else
                        Data.Variables.Add(new ObjectListVariable
                        {
                            Name = name,
                            Value = ((IEnumerable)value).Cast<object>().ToList()
                        });
                }
                else
                    Data.Variables.Add(new ObjectListVariable
                    {
                        Name = name,
                        Value = ((IEnumerable) value).Cast<object>().ToList()
                    });
            }
            else
                vars.Add(new ObjectVariable {Name = name, Value = value});
        }

        private readonly static object ExpressionExecLock = new object();

        public object ExecuteExpression(string expression)
        {
            try
            {
                var scriptManager = new ScriptManager(expression);
                // lock (ExpressionExecLock)  // 09/02/17
                {
                    return scriptManager.ExecuteWithResult(this);
                }
            }
            catch (Exception e)
            {
                try
                {
                    using (var writer = new StreamWriter(Logger.GetLogFileName("WorkflowLinkExpression"), true))
                    {
                        var userInfo = GetUserInfo();

                        writer.WriteLine("{0}: \"{1}\", \"{2}\"; {3}: '{4}'; \"{5}\"\n   message: \"{6}\"", DateTime.Now,
                            userInfo.UserName, userInfo.OrganizationName, GetType().Name, expression, ProcessName, e.Message);
                        if (e.InnerException != null)
                            writer.WriteLine("  - inner exception: \"{0}\"", e.InnerException.Message);
                        writer.WriteLine("  -- Stack: {0}", e.StackTrace);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
                throw;
            }
        }

        public void SetVariables(Dictionary<string, object> @params)
        {
            if (@params == null) return;

            foreach (KeyValuePair<string, object> valuePair in @params)
            {
                SetVariable(valuePair.Key, valuePair.Value);
            }
        }

        public T Get<T>(string name)
        {
            var value = GetVariable(name);

            if (value == null)
                throw new Exception(String.Format("Переменная контекста процесса \"{0}\" не найдена!", name));
            if (value is T)
                return (T)value;

            throw new Exception(String.Format("Ошибка в типе переменной контекста процесса \"{0}\"", name));
        }

        public T Find<T>(string name)
        {
            var value = GetVariable(name);

            if (value == null || value is T) return (T)value;

            throw new Exception(String.Format("Ошибка в типе переменной контекста процесса \"{0}\"", name));
        }

        public string Get(string name)
        {
            var data = GetVariable(name);

            return data != null ? data.ToString() : "";
        }

        public IDocRepository Documents { get { return _docRepo; } }
        public IOrgRepository Orgs { get { return _orgRepo; } }
        public IUserRepository Users { get { return _userRepo; } }
        public IDocDefRepository DocDefs { get { return _defRepo; } }
        public IEnumRepository Enums { get { return _enumRepo; } }

        public DynaDoc NewDynaDoc(Guid defId)
        {
            var doc = _docRepo.New(defId);
            return new DynaDoc(doc, Provider);
        }

        public DynaDoc NewDynaDoc(Guid defId, Guid userId)
        {
            var doc = _docRepo.New(defId);
            return new DynaDoc(doc, userId, Provider);
        }

        public DynaDoc GetDynaDoc(Doc doc)
        {
            return new DynaDoc(doc, Provider);
        }

        public DynaDoc GetDynaDoc(Doc doc, Guid userId)
        {
            return new DynaDoc(doc, userId, Provider /*DataContext*/);
        }

        public DynaDoc GetDynaDoc(Guid docId)
        {
            return new DynaDoc(docId, Provider);
        }

        public DynaDoc GetDynaDoc(Guid docId, Guid userId)
        {
            return new DynaDoc(docId, userId, Provider);
        }

        public Doc GetDoc(Guid docId)
        {
            return _docRepo.LoadById(docId);
        }

        public Doc NewDoc(Guid defId)
        {
            return _docRepo.New(defId);
        }

        public void SaveDoc(Doc doc)
        {
            _docRepo.Save(doc);
        }

        public void Save(DynaDoc doc)
        {
            _docRepo.Save(doc.Doc);
        }

        public void AddDocToList(Doc doc, Doc target, string attrDefName)
        {
            _docRepo.AddDocToList(doc.Id, target, attrDefName);
        }

        public EnumDef GetEnumDef(Guid enumDefId)
        {
            return _enumRepo.Get(enumDefId);
        }

        public EnumValue GetEnumItem(Guid enumId)
        {
            return _enumRepo.GetValue(enumId);
        }

        public List<EnumValue> GetEnumItems(Guid enumDefId)
        {
            return new List<EnumValue>(_enumRepo.GetEnumItems(enumDefId));
        }

        /// <summary>
        /// Создает SqlQuery из QueryDef 
        /// </summary>
        /// <param name="def">QueryDef</param>
        /// <returns>SqlQuery</returns>
        public SqlQuery CreateSqlQuery(QueryDef def)
        {
            var sqb = _sqlQueryBuilderFactory.Create();
            return sqb.Build(def);  //SqlQueryBuilder.Build(DataContext, def);
        }
        public SqlQuery CreateSqlQuery(QueryBuilder builder)
        {
            var sqb = _sqlQueryBuilderFactory.Create();
            return sqb.Build(builder.Def);  //SqlQueryBuilder.Build(DataContext, builder.Def);
        }
        public SqlQueryReader CreateSqlReader(SqlQuery query)
        {
            return _sqlQueryReaderFactory.Create(query);
            // return new SqlQueryReader(DataContext, query);
        }

        public void SetLogFileName(string fileName)
        {
            Data.LogFileName = fileName;
        }

        public void AddErrorMessage(Guid key, string message)
        {
            if (Data.ErrorMessages == null)
                Data.ErrorMessages = new List<ModelMessage>();

            var builder = new ModelMessageBuilder(Data.ErrorMessages);
            builder.AddMessage(key, message);
        }

        public void AddErrorMessage(string name, string message)
        {
            if (Data.ErrorMessages == null)
                Data.ErrorMessages = new List<ModelMessage>();

            var builder = new ModelMessageBuilder(Data.ErrorMessages);
            builder.AddMessage(name, message);
        }

        public void AddErrorMessages(IEnumerable<ModelMessage> messages)
        {
            if (messages == null) return;

            if (Data.ErrorMessages == null)
                Data.ErrorMessages = new List<ModelMessage>();

            Data.ErrorMessages.AddRange(messages);
        }

        public void ClearErrorMessages()
        {
            if (Data.ErrorMessages != null)
                Data.ErrorMessages.Clear();
        }

        public bool HasErrorMessages()
        {
            return Data.ErrorMessages != null && Data.ErrorMessages.Count > 0;
        }

        public void SetFormControlHidden(Guid formId, Guid controlId, bool enable)
        {
            SetFormControlOption(formId, controlId, BizControlOptionFlags.Hidden, enable);
        }
        public void SetFormControlEditable(Guid formId, Guid controlId, bool enable)
        {
            SetFormControlOption(formId, controlId, BizControlOptionFlags.ReadOnly, enable);
        }
        public void SetFormControlDisabled(Guid formId, Guid controlId, bool enable)
        {
            SetFormControlOption(formId, controlId, BizControlOptionFlags.Disabled, enable);
        }

        public void SetFormControlHidden(Guid formId, string attrName, bool enable)
        {
            SetFormControlOption(formId, attrName, BizControlOptionFlags.Hidden, enable);
        }
        public void SetFormControlEditable(Guid formId, string attrName, bool enable)
        {
            SetFormControlOption(formId, attrName, BizControlOptionFlags.ReadOnly, enable);
        }
        public void SetFormControlDisabled(Guid formId, string attrName, bool enable)
        {
            SetFormControlOption(formId, attrName, BizControlOptionFlags.Disabled, enable);
        }

        public void SetFormControlOption(Guid formId, Guid controlId, BizControlOptionFlags flag, bool enable)
        {
            List<BizControlOption> list;
            if (Data.FormOptions == null)
            {
                if (!enable) return;

                Data.FormOptions = new List<BizFormOptions>();
                list = new List<BizControlOption>
                {
                    new BizControlOption {Id = controlId, Flags = flag}
                };
                Data.FormOptions.Add(new BizFormOptions() {Id = formId, Options = list});
            }
            else
            {
                var formOptions = Data.FormOptions.FirstOrDefault(fo => fo.Id != formId);
                if (formOptions == null)
                {
                    if (!enable) return;

                    list = new List<BizControlOption>
                    {
                        new BizControlOption {Id = controlId, Flags = flag}
                    };
                    Data.FormOptions.Add(new BizFormOptions() {Id = formId, Options = list});
                }
                else
                {
                    var ctrlOption = formOptions.Options.FirstOrDefault(o => o.Id == controlId);
                    if (ctrlOption != null)
                    {
                        if (enable)
                            ctrlOption.Flags |= flag;
                        else
                            ctrlOption.Flags ^= flag;
                    }
                    else if (enable)
                        formOptions.Options.Add(new BizControlOption { Id = controlId, Flags = flag });
                }
            }
        }
        public void SetFormControlOption(Guid formId, string attrName, BizControlOptionFlags flag, bool enable)
        {
            List<BizControlOption> list;
            if (Data.FormOptions == null)
            {
                if (!enable) return;

                Data.FormOptions = new List<BizFormOptions>();
                list = new List<BizControlOption>
                {
                    new BizControlOption {AttributeName = attrName, Flags = flag}
                };
                Data.FormOptions.Add(new BizFormOptions() {Id = formId, Options = list});
            }
            else
            {
                var formOptions = Data.FormOptions.FirstOrDefault(fo => fo.Id == formId);
                if (formOptions == null)
                {
                    if (!enable) return;

                    list = new List<BizControlOption>
                    {
                        new BizControlOption {AttributeName = attrName, Flags = flag}
                    };
                    Data.FormOptions.Add(new BizFormOptions() { Id = formId, Options = list });
                }
                else
                {
                    var ctrlOption = formOptions.Options.FirstOrDefault(o => String.Equals(o.AttributeName, attrName, StringComparison.OrdinalIgnoreCase));
                    if (ctrlOption != null)
                    {
                        if (enable)
                            ctrlOption.Flags |= flag;
                        else
                            ctrlOption.Flags ^= flag;
                    }
                    else if (enable)
                        formOptions.Options.Add(new BizControlOption { AttributeName = attrName, Flags = flag });
                }
            }
        }

        public void AddDocsToControlDatas(List<Doc> docs, Guid formId)
        {
            if (docs != null && docs.Count > 0)
            {
                var formRepo = Provider.Get<IFormRepository>();
                var form = formRepo.GetForm(formId);
                if (ControlDatas == null)
                    ControlDatas = new List<BizControl>();
                ControlDatas.AddRange(formRepo.GetTableFormRows(form, docs, 0, 0));
            }
        }

        public bool HasDocumentBlobData(Guid docId, Guid attrId)
        {
            if (Data.BlobDatas == null) return false;

            return Data.BlobDatas.Any(d => d.DocumentId == docId && d.AttributeDefId == attrId && d.Data != null);
        }

        public bool HasDocumentBlobData(Doc document, string attrName)
        {
            if (Data.BlobDatas == null) return false;

            var attr =
                document.Attributes.FirstOrDefault(
                    a =>
                        a.AttrDef != null &&
                        a.AttrDef.Type.Id == (short) CissaDataType.Blob &&
                        String.Equals(a.AttrDef.Name, attrName, StringComparison.OrdinalIgnoreCase));

            return attr != null && HasDocumentBlobData(document.Id, attr.AttrDef.Id);
        }

        public void Log(string text)
        {
            try
            {
                var fileName = Data.LogFileName;

                if (String.IsNullOrEmpty(fileName))
                    fileName = "WorkflowContext";

                fileName = Logger.GetLogFileName(fileName);

                using (var writer = new StreamWriter(fileName, true))
                {
                    var userInfo = GetUserInfo();
                    writer.WriteLine("{0}-{1}: {2}", DateTime.Now, userInfo.UserName, text);
                }
            }
            catch
            {
                // ignored
            }
        }
        public void Log(string text, params object[] args)
        {
            Log(String.Format(text, args));
        }

        public T Find<T>() where T : class
        {
            return Provider.Find<T>();
        }

        public T Find<T>(object arg) where T : class
        {
            return Provider.Find<T>(arg);
        }

        public T Get<T>() where T : class
        {
            return Provider.Get<T>();
        }

        public T Get<T>(object arg) where T : class
        {
            return Provider.Get<T>(arg);
        }

        public int GetServiceCount()
        {
            return Provider.GetServiceCount();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}