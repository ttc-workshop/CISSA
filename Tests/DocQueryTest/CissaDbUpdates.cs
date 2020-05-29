using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DocQueryTest
{
    [TestClass]
    public class CissaDbUpdates
    {
        [TestMethod]
        public void TestMethod1()
        {
            var appDefId = new Guid("{04D25808-6DE9-42F5-8855-6F68A94A224C}");
            var orderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");

            var stateNewId = new Guid("{5CD9E88D-671E-4A44-AD92-9F74DA3B47F7}");
            var stateAprovedId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}");

            var qbOrders = new QueryBuilder(orderDefId);
            qbOrders.Where("&State").Eq(stateAprovedId).Or("&State").Eq(stateNewId);

            var qbApps = new QueryBuilder(appDefId);
            qbApps.Where("&State").Neq(stateAprovedId).And("&Id").In(qbOrders.Def, "Application");

            using (var apps = new DocQuery(qbApps))
            {
                using (var docRepo = new DocRepository())
                {
                    foreach (var appId in apps.All())
                    {
                        var app = docRepo.LoadById(appId);
                        //                docRepo.SetDocState();
                        Console.WriteLine(String.Format("Id: {0}, No: {1}, State: {2}", appId, app["No"] ?? "-",
                                                        app.State != null ? app.State.Type.Name : "??"));
                    }
                }
            }
        }
    }
}
