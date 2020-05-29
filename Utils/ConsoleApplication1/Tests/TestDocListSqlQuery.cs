using System;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Tests
{
    public class TestDocListSqlQuery
    {
        public static void OutputQuery(IAppServiceProvider provider, IDataContext dataContext)
        {
            // using(var docRepo = new DocRepository())
            var docRepo = provider.Get<IDocRepository>();
            {
                var doc = docRepo.LoadById(new Guid("{a1df3eca-d3eb-4c84-98ec-be1433909197}"));

                Console.WriteLine(BuildQuery(provider, dataContext, doc, "Payments"));
            }
        }

        public static string BuildQuery(IAppServiceProvider provider, IDataContext dataContext, Doc doc, string attrName)
        {
            var attr = doc.Get<DocListAttribute>(attrName);

            return BuildQuery(provider, dataContext, doc, attr.AttrDef.Id);
        }

        public static string BuildQuery(IAppServiceProvider provider, IDataContext dataContext, Doc doc, Guid attrDefId)
        {
            var query = new SqlQuery(doc, attrDefId, provider.GetCurrentUserId(), "aaa", provider);

            return query.BuildSql().ToString();
        }
    }
}
