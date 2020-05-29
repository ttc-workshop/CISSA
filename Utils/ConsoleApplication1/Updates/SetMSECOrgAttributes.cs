using System;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Updates
{
    public static class SetMsecOrgAttributes
    {
        public static readonly Guid GrownDocId = new Guid("{4714AC09-365F-49BA-9D74-333014E4E051}");
        public static readonly Guid ChildDocId = new Guid("{4AC6B066-9906-4E5E-A127-C74399F25EC4}");

        public static void Start(IAppServiceProvider provider, IDataContext dataContext)
        {
            Console.WriteLine(@"Взрослый");
            SetOrgAttribute(provider, dataContext, GrownDocId);
            Console.WriteLine(@"Детский");
            SetOrgAttribute(provider, dataContext, ChildDocId);
        }

        public static void SetOrgAttribute(IAppServiceProvider provider, IDataContext dataContext, Guid docDefId)
        {
            var query = new SqlQuery(provider, docDefId, provider.GetCurrentUserId());
            query.AddAttributes("&Id", "&OrgId", "OrganizationMSEC");
            query.AddCondition(ExpressionOperation.And, new Guid("{72BAC0BA-A245-4A95-A72B-1B4FEBC9467C}"),
                               ConditionOperation.IsNull, null);
            query.AddCondition(ExpressionOperation.And, docDefId, "&OrgId",
                               ConditionOperation.IsNotNull, null);

            int i = 0;
            // using (var docRepo = new DocRepository())
            var docRepo = provider.Get<IDocRepository>();
            {
                using (var reader = new SqlQueryReader(dataContext, query))
                {
                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);
                        var orgId = reader.GetGuid(1);

                        var doc = docRepo.LoadById(id);

                        doc["OrganizationMSEC"] = orgId;

                        docRepo.Save(doc);
                        Console.WriteLine(@"  {0}. '{1}'", i, id);
                        i++;
                    }
                }
            }
        }
    }
}
