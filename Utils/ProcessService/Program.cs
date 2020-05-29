using System.ServiceProcess;

namespace ProcessService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] 
            { 
                new ProcessExecutionService() 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
