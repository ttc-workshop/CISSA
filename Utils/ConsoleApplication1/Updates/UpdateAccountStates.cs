using System;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Updates
{
    public static class UpdateAccountStates
    {
        private static readonly Guid AccountDefId = new Guid("{81C532F6-F5B0-4EFC-8305-44E864E778D3}");
        private static readonly Guid ActualStateTypeId = new Guid("{73ED410A-EB64-401B-B5F9-7C9ECE31CB87}");

        public static void Execute(IAppServiceProvider provider, IDataContext dataContext)
        {
            //using (var docRepo = new DocRepository())
            var docRepo = provider.Get<IDocRepository>();
            {
                var query = new SqlQuery(provider, AccountDefId, Guid.Empty);
                query.AddAttribute("&Id");

                using (var reader = new SqlQueryReader(dataContext, query))
                {
                    var count = reader.GetCount();
                    var i = 1;
                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);

                        docRepo.SetDocState(id, ActualStateTypeId);
                        Console.WriteLine(@"{0} of {1} Id: {2}", i, count, id);
                        i++;
                    }
                }
            }
        }
    }
}
