using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using ProcessExecutor.Models.Config;
using ProcessExecutor.ServiceReference;

namespace ProcessExecutor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new ProcessExecutionService();
            service.Execute();
        }
    }

    public class ProcessExecutionService {

        protected string UserName;
        protected string UserPassword;

        private Guid _processId;
        private string _processName;

        private string _logPath;

        public void Execute()
        {
            try
            {
                Initialize();
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var processes = config.GetSection("processes") as ProcessesConfigurationSection;
                if (processes != null && processes.Processes != null && processes.Processes.Count > 0)
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

                        using (var client = Connect())
                        {
                            var processContext = client.WorkflowExecute(_processId, inputParams);
                            //if (processContext != null && processContext.State )
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }
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
                ConfigurationManager.GetSection("users") as
                    NameValueCollection;

            if (dataContextSettings == null) return;

            UserName = dataContextSettings["username"];
            UserPassword = dataContextSettings["password"];

            _logPath = ConfigurationManager.AppSettings["logPath"];
            if (string.IsNullOrWhiteSpace(_logPath)) _logPath = @"c:\distr\cissa\";
            else if (!_logPath.EndsWith(@"\")) _logPath += @"\";
        }

        protected void LogException(Exception e)
        {
            try
            {
                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var controllerName = this.GetType().Name;
                using (var writer = new StreamWriter(string.Format(_logPath + "{0}-Errors-{1}.log", controllerName, today), true))
                {
                    writer.WriteLine("{0}: \"{1}\"; message: \"{2}\"", DateTime.Now, _processName, e.Message);
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
}
