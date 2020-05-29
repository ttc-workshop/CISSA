using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    [DataContract]
    public class WorkflowGateRef
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string ServiceUrl { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public CertificateData CertificateData { get; set; }
    }

    public enum CertificateStoreLocation
    {
        CurrentUser = 1,
        LocalMachine = 2
    }

    public enum CertificateStoreName
    {
        My = 1,
        TrustedPeople = 2,
        AuthRoot = 3,
        Root = 4,
        TrustedPublisher = 5,
        Disallowed = 6,
        CertificateAuthority = 7,
        AddressBook = 8
    }

    public enum CertificateFindType
    {
        BySubjectName = 1,
        BySubjectKeyIdentifier = 2,
        BySubjectDistinguishedName = 3,
        BySerialNumber = 4,
        ByIssuerName = 5,
        ByIssuerDistinguishedName = 6,
        ByThumbprint = 7,
        ByTimeValid = 8,
        ByTimeExpired = 9,
        ByTimeNotYetValid = 10,
        ByTemplateName = 11,
        ByExtension = 12,
        ByKeyUsage = 13,
        ByCertificatePolicy = 14,
        ByApplicationPolicy = 15
    }

    [DataContract]
    public class CertificateData
    {
        [DataMember]
        public CertificateStoreLocation StoreLocation { get; set; }
        [DataMember]
        public CertificateStoreName StoreName { get; set; }
        [DataMember]
        public CertificateFindType FindType { get; set; }
        [DataMember]
        public object FindValue { get; set; }
    }

    public static class CertificateDataHelper
    {
        public static CertificateStoreLocation GetStoreLocation(int? value)
        {
            if (value == null || value == 0 || value == 1)
                return CertificateStoreLocation.CurrentUser;
            return CertificateStoreLocation.LocalMachine;
        }

        public static CertificateStoreName GetStoreName(int? value)
        {
            if (value == null || value == 0 || value == 1)
                return CertificateStoreName.My;
            if (value > (int) CertificateStoreName.AddressBook)
                return CertificateStoreName.AddressBook;
            return (CertificateStoreName) value;
        }

        public static CertificateFindType GetFindType(int? value)
        {
            if (value == null || value == 0 || value == 1)
                return CertificateFindType.BySubjectName;;
            if (value > (int) CertificateFindType.ByApplicationPolicy)
                return CertificateFindType.ByApplicationPolicy;
            return (CertificateFindType) value;
        }
    }
}