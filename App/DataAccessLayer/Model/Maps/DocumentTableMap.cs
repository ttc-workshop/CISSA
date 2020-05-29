using System;
using System.Collections.Generic;
using System.Linq;

namespace Intersoft.CISSA.DataAccessLayer.Model.Maps
{
    public class DocumentTableMap
    {
        public Guid DocDefId { get; private set; }
        public string TableName { get; private set; }
        public bool IsView { get; set; }

        public bool IsVirtual { get; set; }

        public DocumentTableMap Ancestor { get; set; }

        public DocumentTableMap(Guid id, string name)
        {
            DocDefId = id;
            TableName = name;
            IsView = false;
        }

        public DocumentTableMap(Guid id, string name, DocumentTableMap ancestor)
        {
            DocDefId = id;
            TableName = name;
            Ancestor = ancestor;
            IsView = false;
        }

        public DocumentTableMap(Guid id, string name, bool isView)
        {
            DocDefId = id;
            TableName = name;
            IsView = isView;
        }

        public DocumentTableMap(Guid id, string name, bool isView, DocumentTableMap ancestor)
        {
            DocDefId = id;
            TableName = name;
            IsView = isView;
            Ancestor = ancestor;
        }

        private readonly List<AttributeFieldMap> _fields = new List<AttributeFieldMap>();
        public List<AttributeFieldMap> Fields { get { return _fields; } }

        public AttributeFieldMap FindIdentField(string name)
        {
            return
                Fields.FirstOrDefault(
                    f =>
                        f.AttrDefId == Guid.Empty &&
                        String.Equals(f.FieldName, name, StringComparison.OrdinalIgnoreCase)) ??
                (Ancestor != null ? Ancestor.FindIdentField(name) : null);
        }

        public AttributeFieldMap FindField(Guid id, string name)
        {
            return
                Fields.FirstOrDefault(
                    f => f.AttrDefId == id && String.Equals(f.FieldName, name, StringComparison.OrdinalIgnoreCase)) ??
                (Ancestor != null ? Ancestor.FindField(id, name) : null);
        }

        public AttributeFieldMap FindField(Guid id)
        {
            return Fields.FirstOrDefault(f => f.AttrDefId == id) ?? (Ancestor != null ? Ancestor.FindField(id) : null);
        }
    }
}
