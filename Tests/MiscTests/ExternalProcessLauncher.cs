using System;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using Intersoft.CISSA.BizService.Interfaces;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace MiscTests
{
    public class ExternalProcessLauncher : IExternalProcessLauncher
    {
        public ExternalProcessExecuteResult Launch(string serviceUrl, string userName, string password, string processName,
            WorkflowContextData contextData)
        {
            var binding = new WSHttpBinding(SecurityMode.Message);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            var endpoint = new EndpointAddress(serviceUrl);
            var channelFactory = new ChannelFactory<IWorkflowManager>(binding, endpoint);

            if (channelFactory.Credentials != null)
            {
                channelFactory.Credentials.UserName.UserName = userName;
                channelFactory.Credentials.UserName.Password = password;

                channelFactory.Credentials.ServiceCertificate.SetDefaultCertificate(
                    StoreLocation.CurrentUser, StoreName.My, X509FindType.FindBySubjectName, "localhost");
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
                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
                var fileName = Logger.GetLogFileName("ProcessLaunher");
                Logger.OutputLog(fileName, e, "ProcessLauncher.Launch");
            }

            throw new ApplicationException(String.Format("Не могу установить соединение для {0}!", serviceUrl));
        }

        public ExternalProcessExecuteResult Launch(WorkflowGateRef gateRef, string processName, WorkflowContextData contextData)
        {
            var binding = new WSHttpBinding(SecurityMode.Message);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            binding.SendTimeout = new TimeSpan(0, 10, 0);
            binding.OpenTimeout = new TimeSpan(0, 10, 0);
            binding.MaxReceivedMessageSize = 1024*1024*1024;

            binding.MaxBufferPoolSize = 1024 * 1024 * 1024;
            binding.ReaderQuotas.MaxArrayLength = 1024 * 1024 * 1024;
            binding.ReaderQuotas.MaxBytesPerRead = 1024 * 1024 * 1024;
            binding.ReaderQuotas.MaxDepth = 1024 * 1024;
            binding.ReaderQuotas.MaxNameTableCharCount = 1024 * 1024 * 1024;
            binding.ReaderQuotas.MaxStringContentLength = 1024 * 1024 * 1024;

//            var uri = new Uri(gateRef.ServiceUrl);
            var endpoint = new EndpointAddress(gateRef.ServiceUrl);
            var channelFactory = new ChannelFactory<IWorkflowManager>(binding, endpoint);

            if (channelFactory.Credentials != null)
            {
                channelFactory.Credentials.UserName.UserName = gateRef.UserName;
                channelFactory.Credentials.UserName.Password = gateRef.Password;

                var storeLocation = GetStoreLocation(gateRef);
                var storeName = GetStoreName(gateRef);
                var findType = GetFindType(gateRef);
                channelFactory.Credentials.ServiceCertificate.SetDefaultCertificate(
                    storeLocation, storeName, findType, gateRef.CertificateData.FindValue);
            }

            IWorkflowManager client = null;
            try
            {
                client = channelFactory.CreateChannel();
                var result = client.WorkflowGateExecute(processName, contextData);
                result.Data = contextData;

                ((ICommunicationObject)client).Close();

                return result;
            }
            catch (Exception e)
            {
                if (client != null)
                {
                    ((ICommunicationObject)client).Abort();
                }
                var fileName = Logger.GetLogFileName("ProcessLaunher");
                Logger.OutputLog(fileName, e, "ProcessLauncher.Launch");
            }

            throw new ApplicationException(String.Format("Не могу установить соединение для {0}!", gateRef.ServiceUrl));
        }

        private static StoreLocation GetStoreLocation(WorkflowGateRef gate)
        {
            switch (gate.CertificateData.StoreLocation)
            {
                case CertificateStoreLocation.CurrentUser: 
                    return StoreLocation.CurrentUser;
                default:
                    return StoreLocation.LocalMachine;
            }
        }

        private static StoreName GetStoreName(WorkflowGateRef gate)
        {
            switch (gate.CertificateData.StoreName)
            {
                case CertificateStoreName.My: return StoreName.My;
                case CertificateStoreName.TrustedPeople: return StoreName.TrustedPeople;
                case CertificateStoreName.Root: return StoreName.Root;
                case CertificateStoreName.TrustedPublisher: return StoreName.TrustedPublisher;
                case CertificateStoreName.AuthRoot: return StoreName.AuthRoot;
                case CertificateStoreName.CertificateAuthority: return StoreName.CertificateAuthority;
                case CertificateStoreName.Disallowed: return StoreName.Disallowed;
                default:
                    return StoreName.AddressBook;
            }
        }

        private static X509FindType GetFindType(WorkflowGateRef gate)
        {
            switch (gate.CertificateData.FindType)
            {
                case CertificateFindType.BySubjectName:
                    return X509FindType.FindBySubjectName;
                case CertificateFindType.BySubjectKeyIdentifier:
                    return X509FindType.FindBySubjectKeyIdentifier;
                case CertificateFindType.BySubjectDistinguishedName:
                    return X509FindType.FindBySubjectDistinguishedName;
                case CertificateFindType.BySerialNumber:
                    return X509FindType.FindBySerialNumber;
                case CertificateFindType.ByIssuerName:
                    return X509FindType.FindByIssuerName;
                case CertificateFindType.ByIssuerDistinguishedName:
                    return X509FindType.FindByIssuerDistinguishedName;
                case CertificateFindType.ByKeyUsage:
                    return X509FindType.FindByKeyUsage;
                case CertificateFindType.ByTemplateName:
                    return X509FindType.FindByTemplateName;
                case CertificateFindType.ByThumbprint:
                    return X509FindType.FindByThumbprint;
                case CertificateFindType.ByCertificatePolicy:
                    return X509FindType.FindByCertificatePolicy;
                case CertificateFindType.ByTimeExpired:
                    return X509FindType.FindByTimeExpired;
                case CertificateFindType.ByExtension:
                    return X509FindType.FindByExtension;
                case CertificateFindType.ByTimeNotYetValid:
                    return X509FindType.FindByTimeNotYetValid;
                case CertificateFindType.ByTimeValid:
                    return X509FindType.FindByTimeValid;
                default:
                    return X509FindType.FindByApplicationPolicy;
            }
        }
    }
}