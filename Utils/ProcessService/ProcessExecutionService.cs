using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel.Security;
using System.ServiceProcess;
using System.Threading;
using ProcessService.Models.Config;
using ProcessService.BizServiceReference;

namespace ProcessService
{
    public partial class ProcessExecutionService : ServiceBase
    {
        public ProcessExecutionService()
        {
            InitializeComponent();
        }

        protected string UserName;
        protected string UserPassword;

        private Guid _processId;
        private string _processName;

        protected override void OnStart(string[] args)
        {
            try
            {
                Initialize();
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var processes = config.GetSection("processes") as ProcessesConfigurationSection;
                if (processes != null)
                {
                    if (processes.Processes != null && processes.Processes.Count > 0)
                    {
                        using (var client = Connect())
                        {
                            foreach (var process in processes.Processes.OfType<ProcessElement>())
                            {
                                _processId = process.Id;
                                _processName = process.Name;
                                var inputParams = new Dictionary<string, object>();
                                var processParams = process.Params;
                                if (processParams != null)
                                    foreach (var param in processParams.OfType<ParamElement>())
                                    {
                                        inputParams.Add(param.Name, param.Value);
                                    }

                                var processContext = client.WorkflowExecute(_processId, inputParams);
                                //if (processContext != null && processContext.State )
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        protected override void OnStop()
        {
        }

        private WorkflowManagerClient Connect()
        {
            var client = new WorkflowManagerClient();
            if (client.ClientCredentials != null)
            {
                client.ClientCredentials.UserName.UserName = UserName;
                client.ClientCredentials.UserName.Password = UserPassword;
            }
            client.Open();
            return client;
        }

        private void Initialize()
        {
            var dataContextSettings =
                ConfigurationManager.GetSection("user") as
                    System.Collections.Specialized.NameValueCollection;

            if (dataContextSettings == null) return;

            UserName = dataContextSettings["username"];
            UserPassword = dataContextSettings["password"];
        }

        protected void LogException(Exception e)
        {
            try
            {
                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var controllerName = this.GetType().Name;
                using (var writer = new StreamWriter(String.Format("c:\\distr\\cissa\\{0}-Errors-{1}.log", controllerName, today), true))
                {
                    writer.WriteLine("{0}: \"{1}\"; message: \"{2}\"", DateTime.Now, GetType().Name, e.Message);
                    if (e.InnerException != null)
                        writer.WriteLine("  - inner exception: \"{0}\"", e.InnerException.Message);
                    writer.WriteLine("  -- Stack: {0}", e.StackTrace);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }


    }

    public class ProcessExecutionInfo
    {
        public Guid Id { get; private set; }

        private readonly List<ProcessParamInfo> _params = new List<ProcessParamInfo>();
        public List<ProcessParamInfo> Params { get { return _params;  } }

        public ProcessExecutionInfo(Guid id)
        {
            Id = id;
        }

        public ProcessParamInfo AddParam(string name, string value)
        {
            var paramInfo = new ProcessParamInfo {Name = name, Value = value};
            _params.Add(paramInfo);
            return paramInfo;
        }
    }

    public class ProcessParamInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
