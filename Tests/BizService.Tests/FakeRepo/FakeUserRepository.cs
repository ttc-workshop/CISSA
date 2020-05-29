using System;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.BizServiceTests.FakeRepo
{
    class FakeUserRepository: IUserRepository
    {
        public UserInfo FindUserInfo(string userName)
        {
            return new UserInfo
                       {
                           FirstName = "Some",
                           Id = Guid.NewGuid(),
                           LastName = "Name",
                           MiddleName = "xxx",
                           OrgUnitTypeName = "SomeOrgUnitName",
                           PositionName = "Title",
                           UserName = userName
                       };
        }

        /// <summary>
        /// Получает данные пользователя по имени пользователя
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <returns>Информация о пользователя</returns>
        public UserInfo GetUserInfo(string userName)
        {
            return FindUserInfo(userName);
        }

        public UserInfo FindUserInfo(Guid userId)
        {
            return new UserInfo
            {
                FirstName = "Some",
                Id = userId,
                LastName = "Name",
                MiddleName = "xxx",
                OrgUnitTypeName = "SomeOrgUnitName",
                PositionName = "Title",
                UserName = "SomeUserName"
            };
        }

        public UserInfo GetUserInfo(Guid userId)
        {
            return FindUserInfo(userId);
        }

        /// <summary>
        /// Поозволяет сменить пароль пользователя
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <param name="oldPassword">Старый пароль пользователя</param>
        /// <param name="newPassword">Новый пароль пользователя</param>
        /// <returns>Бизнес результат</returns>
        public BizResult ChangeUserPassword(string userName, string oldPassword, string newPassword)
        {
            return new BizResult { Type = BizResultType.Message, Message = "Смена пароля прошла успешно." };
        }

        /// <summary>
        /// Проверяет существование пользователя с переданным паролем
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        /// <returns>true - если имя пользователя и пароль верные</returns>
        public bool Validate(string userName, string password)
        {
            return userName == "SomeUser" && password == "123";
        }

        public void SetUserLanguage(Guid userId, int languageId)
        {
            throw new NotImplementedException();
        }


        public System.Collections.Generic.IEnumerable<Guid> GetUserAccessUsers(Guid? userId)
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<Guid> GetUserAccessOrgs(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
