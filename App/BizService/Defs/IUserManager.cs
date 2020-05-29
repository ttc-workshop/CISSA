using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Intersoft.CISSA.BizService.Defs
{
    [ServiceContract]
    public interface IUserManager
    {
        [OperationContract]
        BizResult ValidateUser(string username, string password);
        [OperationContract]
        UserInfo GetUserInfo(Guid userId);
        [OperationContract]
        BizResult ChangeUserPassword(Guid userId, string oldPassword, string newPassword);
    }

    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string MiddleName { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string OrgUnitName { get; set; }
    }
}
