using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IUserRepository
    {
        UserInfo FindUserInfo(string userName);

        /// <summary>
        /// Получает данные пользователя по имени пользователя
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <returns>Информация о пользователя</returns>
        UserInfo GetUserInfo(string userName);

        UserInfo FindUserInfo(Guid userId);
        /// <summary>
        /// Получает данные пользователя по Id пользователя
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Информация о пользователя</returns>
        UserInfo GetUserInfo(Guid userId);

        /// <summary>
        /// Поозволяет сменить пароль пользователя
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <param name="oldPassword">Старый пароль пользователя</param>
        /// <param name="newPassword">Новый пароль пользователя</param>
        /// <returns>Бизнес результат</returns>
        BizResult ChangeUserPassword(string userName, string oldPassword, string newPassword);

        /// <summary>
        /// Проверяет существование пользователя с переданным паролем
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        /// <returns>true - если имя пользователя и пароль верные</returns>
        bool Validate(string userName, string password);

        /// <summary>
        /// Установить язык пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="languageId">Код языка</param>
        void SetUserLanguage(Guid userId, int languageId);

        IEnumerable<Guid> GetUserAccessUsers(Guid? userId);
        IEnumerable<Guid> GetUserAccessOrgs(Guid userId);

        Guid? FindUserId(string userName, string password);
    }
}
