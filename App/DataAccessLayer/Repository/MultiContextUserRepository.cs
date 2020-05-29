using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class MultiContextUserRepository : IUserRepository, IDisposable
    {
        public IAppServiceProvider Provider { get; private set; }
        public IMultiDataContext DataContext { get; private set; }

        private readonly IList<IUserRepository> _repositories = new List<IUserRepository>();

        public MultiContextUserRepository(IAppServiceProvider provider)
        {
            Provider = provider;
            DataContext = provider.Get<IMultiDataContext>();

            foreach (var context in DataContext.Contexts)
            {
                if (context.DataType.HasFlag(DataContextType.Account))
                    _repositories.Add(new UserRepository(provider, context));
            }
        }

        public UserInfo FindUserInfo(string userName)
        {
            return _repositories.Select(repo => repo.FindUserInfo(userName)).FirstOrDefault(userInfo => userInfo != null);
        }

        public UserInfo GetUserInfo(string userName)
        {
            var userInfo = FindUserInfo(userName);
            if (userInfo == null)
                throw new ApplicationException(String.Format("Пользователь \"{0}\" не найден!", userName));
            return userInfo;
        }

        public UserInfo FindUserInfo(Guid userId)
        {
            return _repositories.Select(repo => repo.FindUserInfo(userId)).FirstOrDefault(userInfo => userInfo != null);
        }

        public UserInfo GetUserInfo(Guid userId)
        {
            var userInfo = FindUserInfo(userId);
            if (userInfo == null)
                throw new ApplicationException(String.Format("Пользователь с Id \"{0}\" не найден!", userId));
            return userInfo;
        }

        public BizResult ChangeUserPassword(string userName, string oldPassword, string newPassword)
        {
            var result = new BizResult { Type = BizResultType.Error, Message = "Неверное имя пользователя или пароль!" };

            foreach (var repo in _repositories)
            {
                result = repo.ChangeUserPassword(userName, oldPassword, newPassword);
                if (result.Type != BizResultType.Error) return result;
            }
            return result;
        }

        public bool Validate(string userName, string password)
        {
            return _repositories.Any(r => r.Validate(userName, password));
        }

        public void SetUserLanguage(Guid userId, int languageId)
        {
            var repo = _repositories.FirstOrDefault(r => r.FindUserInfo(userId) != null);

            if (repo != null)
                repo.SetUserLanguage(userId, languageId);
        }

        public IEnumerable<Guid> GetUserAccessUsers(Guid? userId)
        {
            return _repositories.SelectMany(repo => repo.GetUserAccessUsers(userId));
        }

        public IEnumerable<Guid> GetUserAccessOrgs(Guid userId)
        {
            return _repositories.SelectMany(repo => repo.GetUserAccessOrgs(userId));
        }

        public Guid? FindUserId(string userName, string password)
        {
            foreach (var result in _repositories.Select(repo => repo.FindUserId(userName, password)).Where(result => result != null))
            {
                return result;
            }
            return null;
        }

        public void Dispose()
        {
            foreach (var repo in _repositories.OfType<IDisposable>())
            {
                repo.Dispose();
            }
            _repositories.Clear();
        }
    }
}