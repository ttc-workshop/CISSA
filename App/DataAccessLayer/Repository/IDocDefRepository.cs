using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IDocDefRepository: IDisposable
    {
    /*    /// <summary>
        /// Получает список атрибутов по идентификатору типа документа
        /// </summary>
        /// <param name="docDefId">Идентификатор типа документа</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Список атрибутов</returns>
        IEnumerable<AttrDef> GetDocumentAttributes(Guid docDefId, Guid userId);*/

        /// <summary>
        /// Возвращает потомков для типа документа
        /// </summary>
        /// <param name="docDefId">Идентификатор типа документа</param>
        /// <returns>Список идентификаторов потомков</returns>
        IEnumerable<Guid> GetDocDefDescendant(Guid docDefId);

        // DONE: Реализовать 2 метода поиска для использования в мультиконтексте
        DocDef Find(Guid docDefId);
        DocDef Find(string docDefName);
        
        /// <summary>
        /// Загружает тип документа по идентификатору
        /// </summary>
        /// <param name="docDefId">Идентификатор типа документа</param>
        /// <returns>Тип документа</returns>
        DocDef DocDefById(Guid docDefId);

        /// <summary>
        /// Загружает тип документа по имени
        /// </summary>
        /// <param name="docDefName">Имя типа документа</param>
        /// <returns>Тип документа</returns>
        DocDef DocDefByName(String docDefName);

        IList<DocDefName> GetDocDefNames();

        IList<DocDefRelation> GetDocDefRelations(Guid docDefId);

        IList<DocDefRelation> GetDocDefRelations(DocDef docDef);
    }
}