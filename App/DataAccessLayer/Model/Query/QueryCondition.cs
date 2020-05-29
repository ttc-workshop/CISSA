using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    public class QueryCondition // TODO: Зачем этот класс?
    {
        public QueryConditionDef Condition { get; private set; }
        public DocDef DocDef { get; private set; }

        public QueryCondition(QueryConditionDef condition, DocDef docDef)
        {
            Condition = condition;
            DocDef = docDef;
        }

        public AttrDef AttrDef 
        { 
            get
            {
                var helper = new QueryAttributeDefHelper(Condition.Left.Attribute);
                return DocDef.Attributes.FirstOrDefault(a => helper.IsSame(a.Name));

                //return DocDef.Attributes.FirstOrDefault(a => String.CompareOrdinal(a.Name, Condition.AttributeName) == 0);
            }
        }

        /*protected static IQueryable<Document> BuildSource(DocDef docDef, IDataContext context, Guid userId)
        {
            using (var defRepo = new DocDefRepository(context, userId))
            {
                var descIds = defRepo.GetDocDefDescendant(docDef.Id).ToList();

                return from doc in context.Documents
                       where descIds.Contains(doc.Def_Id ?? Guid.Empty) && (doc.Deleted == null || doc.Deleted == false)
                       select doc;
            }
        }*/
    }
}
