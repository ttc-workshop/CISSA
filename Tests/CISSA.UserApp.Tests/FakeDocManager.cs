using System;
using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace CISSA.UserApp.Tests
{
    class FakeDocManager: IDocManager
    {
        public Doc DocumentLoad(Guid documentId)
        {
            return new Doc {Id = documentId};
        }

        public Doc DocumentNew(Guid docDefId)
        {
            return new Doc { DocDef = new DocDef { Id = docDefId } };
        }

        /*public List<Guid> DocumentSearch(List<SearchParameter> searchParameters, LogicOperation logicOperation)
        {
            return new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        }*/

        public Doc DocumentSave(Doc doc)
        {
            return doc;
        }

        public void DocumentDelete(Guid documentId)
        {
            return;
        }

        public List<DocState> DocumentStateList(Guid docId)
        {
            throw new NotImplementedException();
        }

        public List<EnumValue> GetEnumList(Guid enumId)
        {
            return new List<EnumValue>{ new EnumValue(), new EnumValue()};
        }

        public List<Guid> DocumentList(out int pageCount, Guid docDefId, int pageNo, int pageSize)
        {
            pageCount = 1;
            return new List<Guid>{ Guid.NewGuid(), Guid.NewGuid()};
        }
        
        public List<Guid> DocumentFilterList(out int count, Guid docDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttr)
        {
            count = 20;
            return new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        }

        public List<Guid> DocumentStateFilterList(out int count, Guid docDefId, Guid docStateId, int pageNo, int pageSize, Doc filter, Guid? sortAttr)
        {
            count = 20;
            return new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        }

        public List<Guid> DocAttrList(out int count, Doc document, Guid attrDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttr)
        {
            count = 20;
            return new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        }

        public List<Guid> DocAttrListById(out int count, Guid docId, Guid attrDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttr)
        {
            count = 20;
            return new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        }

        public Doc GetNestingDocument(Doc document, DocAttribute docAttr)
        {
            return document;
        }

        public bool DocIsStored(Doc document)
        {
            return true;
        }

        public bool ExistsInDocList(Guid docId, Guid attrDocId, Guid attrDefId)
        {
            return false;
        }

        public Doc AddDocToList(Guid docId, Doc document, Guid attrDefId/*, Guid userId*/)
        {
            return document;
        }

        public List<Guid> DocAttrList(out int count, Guid docId, Guid attrDefId, int pageNo, int pageSize, Doc filter, Guid? sortAttr)
        {
            count = 20;
            return new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        }
    }
}
