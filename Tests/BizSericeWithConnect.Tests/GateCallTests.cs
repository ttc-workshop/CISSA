using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject1.ServiceReference;

namespace TestProject1
{
    [TestClass]
    public class GateCallTests
    {
        private const string TjTestAsistServiceUtl = @"http://192.168.0.64/AsistService/BizService.svc";
        private const string TjTestNrszServiceUtl = @"http://192.168.0.64/NrszService/BizService.svc";
        private const string LocalAsistServiceUtl = @"http://localhost/AsistService/BizService.svc";

        [TestMethod]
        public void TestExternalProcLaunch()
        {
            var data = new WorkflowContextData();

            Launch(TjTestNrszServiceUtl, "d", "123", "FindPersons", data);
        }

        public ExternalProcessExecuteResult Launch(string serviceUrl, string userName, string password, string processName,
            WorkflowContextData contextData)
        {
            var binding = new WSHttpBinding(SecurityMode.Message);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            binding.SendTimeout = new TimeSpan(0, 10, 0);
            binding.OpenTimeout = new TimeSpan(0, 10, 0);
            binding.MaxReceivedMessageSize = 1024 * 1024 * 1024;

            var uri = new Uri(serviceUrl);
            var endpoint = //new EndpointAddress(serviceUrl);
                new EndpointAddress(uri, EndpointIdentity.CreateDnsIdentity("localhost"));
            var channelFactory = new ChannelFactory<IWorkflowManager>(binding, endpoint);

            if (channelFactory.Credentials != null)
            {
                channelFactory.Credentials.UserName.UserName = userName;
                channelFactory.Credentials.UserName.Password = password;

                channelFactory.Credentials.ServiceCertificate.SetDefaultCertificate(
                    StoreLocation.LocalMachine, StoreName.TrustedPeople, X509FindType.FindBySubjectName, "localhost");
            }

            IWorkflowManager client = null;
            try
            {
                client = channelFactory.CreateChannel();
                var result = client.WorkflowGateExecute(processName, contextData);

                ((ICommunicationObject)client).Close();

                return result;
            }
            catch (Exception e)
            {
                throw;
            }

            throw new ApplicationException(String.Format("Не могу установить соединение для {0}!", serviceUrl));
        }
    }
}
