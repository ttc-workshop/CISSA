using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    public enum SystemIdent { Id, State, Created, OrgId, OrgName, OrgCode, UserId, UserName }

    [DataContract]
    public class QuerySystemCondition : QueryCondition
    {
        [DataMember]
        public SystemIdent Ident { get; private set; }

        private readonly static DateTime MaxDate = new DateTime(9999, 12, 31);

        public QuerySystemCondition() {}

        public QuerySystemCondition(SystemIdent ident, ExpressionOperation operation) :
            base(operation)
        {
            Ident = ident;
        }

        public override IQueryable<Document> Build(DocDef docDef, ObjectContext context, Guid userId)
        {
            var em = (cissaEntities)context;

            var source = BuildSource(docDef, context, userId);

            var text = Value != null ? Value.ToString() : "";
            switch (Condition) 
            {
                case ConditionOperation.Equal:
                    switch(Ident)
                    {
                        case SystemIdent.Id:
                            var docId = Guid.Parse(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Id == docId && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.State:
                            Guid stateId;
                            if (!Guid.TryParse(text, out stateId))
                            {
                                var dsRepo = new DocStateRepository();
                                stateId = dsRepo.GetDocStateTypeId(text);
                            }
                            return source.Intersect(
                                    em.Document_States.Where(s => s.State_Type_Id == stateId).Select(s => s.Document));
                        case SystemIdent.Created:
                            var val = Convert.ToDateTime(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Created == val && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.OrgId:
                            var orgId = Guid.Parse(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Organization_Id == orgId && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.OrgName:
                            var orgRepo = new OrgRepository(userId);
                            var orgId2 = orgRepo.GetOrgIdByName(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Organization_Id == orgId2 && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.OrgCode:
                            var orgRepo2 = new OrgRepository(userId);
                            var orgId3 = orgRepo2.GetOrgIdByCode(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.Organization_Id == orgId3 && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.UserId:
                            var userRefId = Guid.Parse(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.UserId == userRefId && (d.Deleted == null || d.Deleted == false)));
                        case SystemIdent.UserName:
                            var userRepo = new UserRepository();
                            var userId2 = userRepo.GetUserId(text);
                            return source.Intersect(
                                em.Documents.Where(d => d.UserId == userId2 && (d.Deleted == null || d.Deleted == false)));
                    }
                    break;
            }
            return source;
        }
    }
}
