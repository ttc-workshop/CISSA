using System;
using System.Data.Entity.Core.EntityClient;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Updates
{
    public class SetOrderTerminationDate
    {
        private static readonly Guid OrderDefId = new Guid("{19EA8D75-2EE7-42CA-BE3B-D7E41F343DDD}");
        private static readonly Guid TerminationNotificationDefId = new Guid("{B04237B3-4352-4B55-8399-6AD8AC2BD722}");

        public static readonly Guid ApprovedStateTypeId = new Guid("{66D7FA1C-77EF-470D-A70B-0D6E5E16D942}"); // Утвержден

        public static void Start()
        {
            using (var connection = new EntityConnection("name=cissaEntities"))
            {
                using (var dataContext = new DataContext(connection))
                {
                    SetApprovedTerminationDate(dataContext);
                    SetNullExpiryDate(dataContext);
                }
            }
        }

        protected static void SetApprovedTerminationDate(IDataContext dataContext)
        {
            var query = new SqlQuery(TerminationNotificationDefId, dataContext);

            query.AndCondition("&State", ConditionOperation.Equal, ApprovedStateTypeId);
            query.AndCondition("Order", ConditionOperation.IsNotNull, null);
            query.AndCondition("TerminationDate", ConditionOperation.IsNotNull, null);
            query.AddAttributes("Order", "TerminationDate");

            using (var docRepo = new DocRepository(dataContext))
            {
                using (var reader = new SqlQueryReader(query))
                {
                    var i = 0;
                    while (reader.Read())
                    {
                        var orderId = reader.GetGuid(0);
                        var terminationDate = reader.GetDateTime(1);

                        var order = docRepo.LoadById(orderId);
                        order["ExpiryDate"] = terminationDate;
                        docRepo.Save(order);
                        WriteLog("SetApprovedTerminationDate", String.Format("{0}. {1} set termination date {2}", i, order["No"] ?? "---", terminationDate));
                        i++;
                    }
                }
            }
        }

        protected static void SetNullExpiryDate(IDataContext dataContext)
        {
            var query = new SqlQuery(OrderDefId, dataContext);

//            query.AndCondition("&State", ConditionOperation.Equal, ApprovedStateTypeId);
            query.AndCondition("DateTo", ConditionOperation.IsNotNull, null);
            query.AndCondition("ExpiryDate", ConditionOperation.IsNull, null);
            query.AddAttributes("&Id", "DateTo");

            using (var docRepo = new DocRepository(dataContext))
            {
                using (var reader = new SqlQueryReader(query))
                {
                    var i = 0;
                    while (reader.Read())
                    {
                        var orderId = reader.GetGuid(0);
                        var finishDate = reader.GetDateTime(1);

                        var order = docRepo.LoadById(orderId);
                        order["ExpiryDate"] = finishDate;
                        docRepo.Save(order);
                        WriteLog("SetNullExpiryDate", String.Format("{0}. {1} set finish date {2}", i, order["No"] ?? "---", finishDate));
                        i++;
                    }
                }
            }
        }

        private static void WriteLog(string filePrefix, string message)
        {
            try
            {
                Console.WriteLine(message);
                using (var writer = new StreamWriter(filePrefix + DateTime.Today.ToShortDateString()  + ".log", true))
                {
                    writer.WriteLine("{0}: {1}", DateTime.Now, message);
                }
            }
            catch (Exception)
            {
            }            
        }
    }
}
