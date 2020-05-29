using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    [DataContract]
    public class QuerySource
    {
//        public DocDef DocDef { get; private set; }

        [DataMember]
        public Guid DocDefId { get; private set; }
        [DataMember]
        public string DocDefName { get; private set; }
        [DataMember]
        public Guid UserId { get; private set; }

        public QuerySource(Guid docDefId, Guid userId)
        {
//            var defRepo = new DocDefRepository();
//            DocDef = defRepo.DocDefById(docDefId);
            DocDefId = docDefId;
            UserId = userId;
        }

        public QuerySource(Guid docDefId) : this(docDefId, Guid.Empty) { }

        public QuerySource(string docDefName, Guid userId)
        {
//            var defRepo = new DocDefRepository();
//            DocDef = defRepo.DocDefByName(docDefName); 
            DocDefName = docDefName;
            DocDefId = Guid.Empty;
            UserId = userId;
        }

        public QuerySource(string docDefName) : this(docDefName, Guid.Empty) { }

        public IQueryable<Document> Build(ObjectContext context)
        {
            var defRepo = new DocDefRepository(UserId);

            if (DocDefId == Guid.Empty)
                DocDefId = defRepo.DocDefByName(DocDefName).Id;

            var descIds = defRepo.GetDocDefDescendant(DocDefId).ToList();

            return from doc in ((cissaEntities)context).Documents
                   where descIds.Contains(doc.Def_Id ?? Guid.Empty) && (doc.Deleted == null || doc.Deleted == false)
                   select doc;
        }

        public DocDef GetDocDef()
        {
            var defRepo = new DocDefRepository(UserId);

            DocDef docDef;
            if (DocDefId == Guid.Empty)
            {
                docDef = defRepo.DocDefByName(DocDefName);
                DocDefId = docDef.Id;
            }
            else
                docDef = defRepo.DocDefById(DocDefId);

            return docDef;
        }

        internal AttrDef GetAttrDef(string attributeName)
        {
            return
                GetDocDef()
                    .Attributes.FirstOrDefault(
                        a => String.Compare(a.Name, attributeName, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
