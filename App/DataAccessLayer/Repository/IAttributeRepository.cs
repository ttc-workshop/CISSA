using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IAttributeRepository
    {
//        AttributeBase GetAttributeById(Guid attributeDefId, Guid docId);
//        AttributeBase GetAttributeByName(String attributeName, Guid docDefId);

        AttributeBase GetAttributeById(Guid attributeDefId, Doc doucment);
//        List<Guid> GetAttributeDocList(out int pageCount, Guid docId, Guid attributeDefId, int pageNo, int pageSize = 0);
        AttributeBase CreateAttribute(AttrDef getByName);
    }
}
