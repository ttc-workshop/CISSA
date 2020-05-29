using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Storage
{
    public class AttributeData
    {
        public DateTime Created { get; set; }
        public object Value { get; set; }
        public DateTime Expired { get; set; }
        public Guid DefId { get; set; }
        public int DataType { get; set; }

        public string Value2 { get; set; }
    }

    public interface IAttributeStorage
    {
        AttributeData Load(Guid docId, AttrDef attrDef);
        IEnumerable<AttributeData> LoadAll(Guid docId);
        void Save(Guid docId, AttributeBase attr, Guid userId);
        void Save(Guid docId, AttributeBase attr, Guid userId, DateTime date);
        void DirectSave(Guid docId, AttrDef attrDef, object value, Guid userId, DateTime date, string description);
        void RemoveDocListItem(Guid docId, Guid attrDefId, Guid itemDocId, DateTime date);
        void AddDocListItem(Guid docId, Guid attrDefId, Guid itemDocId, Guid userId, DateTime date);
        bool ExistsDocInList(Guid docId, Guid attrDefId, Guid itemDocId);
        void ClearDocList(Guid docId, Guid attrDefId, DateTime time = new DateTime());
        bool HasRefToDoc(Guid docId);
        List<DocRef> GetDocRefs(Guid docId, DocRefSourceType sourceType = DocRefSourceType.All);
        void DeleteAttributes(Guid docId);
        bool CheckUniqueness(Guid docId, AttrDef attrDef, object value);
    }
}
