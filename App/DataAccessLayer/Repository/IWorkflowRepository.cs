using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IWorkflowRepository
    {
        /// <summary>
        /// Загружает процесс по идентификатору
        /// </summary>
        /// <param name="processId">Идентификатор процесса</param>
        /// <returns>Загруженный процесс</returns>
        WorkflowProcess LoadProcessById(Guid processId);

        /// <summary>
        /// Позволяет загрузить действие по идентификатору
        /// </summary>
        /// <param name="activityId">Идентификатор действия</param>
        /// <returns>Загруженное действие</returns>
        WorkflowActivity LoadActivityById(Guid activityId);
/*
        /// <summary>
        /// Загружает действия процесса
        /// </summary>
        /// <param name="processId">Идентификатор процесса</param>
        /// <returns>Список идентификаторов действий процесса</returns>
        IList<Guid> LoadProcessActivities(Guid processId);

        /// <summary>
        /// Загружает стартовое действие процесса
        /// </summary>
        /// <remarks>
        /// Лучше стартовое действие строго не прописывать в процессе. А брать его из таблицы Start_Activities
        /// </remarks>
        /// <param name="processId">Идентификатор процесса</param>
        /// <returns>Идентификатор стартового действия</returns>
        Guid GetProcessStartActivity(Guid processId);

        /// <summary>
        /// Загружает последующее действие процесса через выбранное действие пользователя
        /// </summary>
        /// <param name="processId">Идентификатор процесса</param>
        /// <param name="lastActivity">Последнее (передыдущее) действие</param>
        /// <param name="userActionId">Действие пользователя</param>
        WorkflowActivity GetActivityByUserActionId(Guid processId, Guid lastActivity, Guid userActionId);*/

        /// <summary>
        /// Загружает список возможных действий пользователя
        /// </summary>
        /// <param name="activityId">Идентификатор действия</param>
        /// <param name="languageId">Язык</param>
        IList<UserAction> GetActivityUserActions(Guid activityId, int languageId);

        /// <summary>
        /// Переводит список пользовательских действий на заданный язык
        /// </summary>
        /// <param name="userActions">Список пользовательских действий</param>
        /// <param name="languageId">Язык</param>
        /// <returns>Список пользовательских действий</returns>
        void TranslateUserActions(List<UserAction> userActions, int languageId);

        /// <summary>
        /// Загрузка шлюза по имени
        /// </summary>
        /// <param name="gateName">Имя шлюза</param>
        /// <returns>Шлюз</returns>
        WorkflowGate LoadGateByName(string gateName);

        WorkflowGateRef LoadGateRefById(Guid gaterefId);
    }
}
