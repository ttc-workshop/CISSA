using System;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Core
{
    public class UserDataProvider : IUserDataProvider
    {
        public Guid UserId { get; private set; }
        public string UserName { get; private set; }

        public UserDataProvider(Guid userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }

        public UserDataProvider(string userName, string password, IAppServiceProvider provider)
        {
            var userRepo = provider.Get<IUserRepository>();

            var userId = userRepo.FindUserId(userName, password);

            if (userId == null)
                throw new ApplicationException(String.Format("Username \"{0}\" not found", userName));

            UserId = (Guid) userId;
            UserName = userName;
        }
    }
}