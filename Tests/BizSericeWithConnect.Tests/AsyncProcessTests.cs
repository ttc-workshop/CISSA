using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject1.ServiceReference;

namespace TestProject1
{
    [TestClass]
    public class AsyncProcessTests
    {
        [TestMethod]
        public void TestAsyncProcessCall()
        {
            var client = new AsyncWorkflowManagerClient();
            client.ClientCredentials.UserName.UserName = "D";
            client.ClientCredentials.UserName.Password = "123";

            var taskId = client.ExecuteProcess(new Guid("{00479F39-136C-4CBF-9357-F372A4949F5C}"), null); // Массовый расчет КОН
            Console.WriteLine("Task Id - [{0}]", taskId);
            var taskInfo = client.GetProcessTaskState(taskId);
            Console.WriteLine("Task state: {0}", taskInfo.State);
            Assert.IsNotNull(taskInfo);
            while (taskInfo.State == 0)
            {
                taskInfo = client.GetProcessTaskState(taskId);
                Console.WriteLine("Task state: {0}", taskInfo.State);
                Thread.Sleep(1000);
            }
            Console.WriteLine("End task state: {0}", taskInfo.State);
            var data = client.EndProcessTask(taskId);
            
            Console.WriteLine(data != null ? data.ToString() : "null");
        }
    }
}
