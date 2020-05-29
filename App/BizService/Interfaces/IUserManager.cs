using System.Collections.Generic;
using System.ServiceModel;
using Intersoft.CISSA.BizService.Utils;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;

namespace Intersoft.CISSA.BizService.Interfaces
{
    /// <summary>
    /// Интерфейс работы с пользователями
    /// </summary>
    [ServiceContract]
    public interface IUserManager
    {
        /// <summary>
        /// Получает данные учетной записи пользователя
        /// </summary>
        /// <returns>Сведения о пользователе</returns>
        [OperationContract]
        UserInfo GetUserInfo();

        /// <summary>
        /// Метод для изменения пароля пользователя
        /// </summary>
        /// <param name="oldPassword">Старый пароль</param>
        /// <param name="newPassword">Новый пароль</param>
        /// <returns>Возвращает результат изменения пароля пользователя</returns>
        [OperationContract]
        BizResult ChangeUserPassword(string oldPassword, string newPassword);

        /// <summary>
        /// Метод для проверки соединения с сервисом бизнес-логики
        /// </summary>
        /// <returns>Возвращает истина - при успешном подключении к сервису</returns>
        [OperationContract]
        bool TryConnect();

        /// <summary>
        /// Метод установки языка пользовательского интерфейса для текущего пользователя
        /// </summary>
        /// <param name="languageId">Код устанавливаемого языка</param>
        [OperationContract]
        void SetUserLanguage(int languageId);

        /// <summary>
        /// Метод для очищения кэша
        /// </summary>
        [OperationContract]
        void ClearCache();

        /// <summary>
        /// Получает сведения о состоянии кэша
        /// </summary>
        /// <returns>Строка описания состояния кэша</returns>
        [OperationContract]
        string GetCacheInfo();

        [OperationContract]
        List<MonitorNode> GetMonitorNodes();

        [OperationContract]
        int? GetLanguageId(string cultureName);

        [OperationContract]
        string GetLanguageCultureName(int languageId);

        [OperationContract]
        List<ProcessInfo> GetProcessesInfo();
    }
}
