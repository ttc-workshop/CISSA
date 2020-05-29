using System;

namespace Intersoft.CISSA.DataAccessLayer.Model.Maps
{
    [Flags]
    public enum AttributeFieldType
    {
        View = 1,
        Data = 2,
        Search = 4,
        Order = 8
    }

    public class AttributeFieldMap
    {
        public Guid AttrDefId { get; private set; }
        public string FieldName { get; private set; }
        public AttributeFieldType Type { get; private set; }

        public AttributeFieldMap(Guid id, string name, AttributeFieldType type) 
        {
            AttrDefId = id;
            FieldName = name;
            Type = type;
        }

        public AttributeFieldMap(Guid id, string name) :
            this(id, name, AttributeFieldType.View | AttributeFieldType.Data | AttributeFieldType.Search | AttributeFieldType.Order)
        {
        }
    }
}