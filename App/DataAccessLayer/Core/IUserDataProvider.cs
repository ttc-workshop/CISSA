using System;

namespace Intersoft.CISSA.DataAccessLayer.Core
{
    public interface IUserDataProvider
    {
        Guid UserId { get; }
        string UserName { get; }
    }
}