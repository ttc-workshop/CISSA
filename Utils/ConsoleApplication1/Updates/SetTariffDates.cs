using System;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Updates
{
    public class SetTariffDates
    {
        public static readonly Guid TariffDefId = new Guid("{0F29B75F-DE90-4910-9524-B74CB0418A57}");

        public static void SetDates(IAppServiceProvider provider, IDataContext dataContext)
        {
            var query = new SqlQuery(provider, TariffDefId, Guid.Empty);
            query.AddAttributes("&Id", "EffectiveDate", "ExpiryDate");
            query.AddCondition(ExpressionOperation.And, TariffDefId, "EffectiveDate",
                               ConditionOperation.IsNull, null);
            query.AddCondition(ExpressionOperation.Or, TariffDefId, "ExpiryDate",
                               ConditionOperation.IsNull, null);

            int i = 0;
            /*using (*/
            var docRepo = provider.Get<IDocRepository>(); //new DocRepository();/*)*/
            {
                using (var reader = new SqlQueryReader(dataContext, query))
                {
                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);

                        var doc = docRepo.LoadById(id);

                        if (doc["EffectiveDate"] == null)
                            doc["EffectiveDate"] = new DateTime(2011, 1, 1);
                        if (doc["ExpiryDate"] == null)
                            doc["ExpiryDate"] = new DateTime(2100, 1, 1);

                        docRepo.Save(doc);
                        Console.WriteLine(@"  {0}. '{1}'", i, id);
                        i++;
                    }
                }
            }
        }
    }
}
