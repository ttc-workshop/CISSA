using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IEnumRepository
    {
        /// <summary>
        /// Позволяет проверить действительно ли значение принадлежит данному справочнику
        /// </summary>
        /// <param name="enumId">Идентификатор справочника</param>
        /// <param name="enumValueId">Идентификатор проверяемого значения</param>
        /// <returns>True если значение имеется в справочнике</returns>
        bool CheckEnumValue(Guid enumId, Guid enumValueId);

        /// <summary>
        /// Позволяет получить идентификатор (значение) элемента перечисления по имени
        /// </summary>
        /// <param name="enumId">Идентификатор перечисления</param>
        /// <param name="enumValue">Имя значения перечисления</param>
        /// <returns>Идентификатор значения справочника</returns>
        Guid GetEnumValueId(Guid enumId, string enumValue);

        /*/// <summary>
        /// Позволяет получить идентификатор (значение) элемента перечисления по имени
        /// </summary>
        /// <param name="enumDefName">Наименование перечисления</param>
        /// <param name="enumValue">Имя значения перечисления</param>
        /// <returns>Идентификатор значения справочника</returns>
        Guid GetEnumValueId(string enumDefName, string enumValue);
*/
        /// <summary>
        /// Позволяет получить значение элемента перечисления по Id
        /// </summary>
        /// <param name="enumId">Идентификатор перечисления</param>
        /// <param name="valueId">Идентификатор значения перечисления</param>
        /// <returns>Значение элемента справочника</returns>
        string GetEnumValue(Guid enumId, Guid valueId);

        /// <summary>
        /// Позволяет получить значение элемента перечисления по Id
        /// </summary>
        /// <param name="valueId">Идентификатор значения перечисления</param>
        /// <returns>Значение элемента справочника</returns>
        EnumValue GetValue(Guid valueId);

        /// <summary>
        /// Загружает справочник
        /// </summary>
        /// <param name="enumId">Идентификатор справочника</param>
        /// <returns>Список значений справочника</returns>
        IList<EnumValue> GetEnumItems(Guid enumId);

        /// <summary>
        /// Загружает справочник
        /// </summary>
        /// <param name="enumDefName">Наименование справочника</param>
        /// <returns>Список значений справочника</returns>
        IList<EnumValue> GetEnumItems(string enumDefName);

        /// <summary>
        /// Получает идентификатор перечисления
        /// </summary>
        /// <param name="enumDefName">Имя перечисления</param>
        /// <returns>Идентификатор перечисления</returns>
        Guid GetEnumDefId(string enumDefName);

        /// <summary>
        /// Получает класс перечисления
        /// </summary>
        /// <param name="id">Id перечисления</param>
        /// <returns>Идентификатор перечисления</returns>
        EnumDef Find(Guid id);

        /// <summary>
        /// Получает класс перечисления
        /// </summary>
        /// <param name="id">Id перечисления</param>
        /// <returns>Идентификатор перечисления</returns>
        EnumDef Get(Guid id);

        EnumDef Find(string enumDefName);
        EnumDef Get(string enumDefName);

        /// <summary>
        /// Переводит элементы списка на заданный язык
        /// </summary>
        /// <param name="items">Список элементов</param>
        /// <param name="languageId">Язык</param>
        void TranslateEnumItems(List<EnumValue> items, int languageId);
    }
}
