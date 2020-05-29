using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Maps;

namespace Intersoft.CISSA.DataAccessLayer.Storage
{
    public class DocData
    {
        public Guid Id { get; set; }
        public DateTime? Created { get; set; }
        public Guid? DefId { get; set; }
        public Guid UserId { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? PositionId { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public interface IDocumentStorage
    {
        DocData Load(Guid id);
        void Save(Doc doc, UserInfo userInfo, DateTime date);
        bool IsExists(Guid docId);
        DocStateData LoadDocState(Guid docId, DateTime forDate);
        void SaveDocState(Guid id, Guid docId, Guid stateTypeId, Guid userId, DateTime forDate);
        void Hide(Guid docId);
        void Delete(Guid docId);
        void WriteDocToTable(DocumentTableMap map, Doc doc);
        void FillDocStates(Guid docId, List<DocStateData> docStates);

        List<Guid> GetDocDefStateTypes(Guid docDefId);

        void DeleteDocStates(Guid docId);
    }

    public class DocStateData
    {
        public Guid Id { get; set; }
        public Guid StateTypeId { get; set; }
        public DateTime Created { get; set; }

        public Guid UserId { get; set; }
    }
}
