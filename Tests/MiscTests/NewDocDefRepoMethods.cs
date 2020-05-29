using System;
using System.Data.SqlClient;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiscTests
{
    [TestClass]
    public class NewDocDefRepoMethods
    {
        public const string AsistConnectionString =
            "Data Source=localhost;Initial Catalog=asist_db;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        [TestMethod]
        public void GetDocDefNameTest()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var docDefRepo = provider.Get<IDocDefRepository>();

                        var list = docDefRepo.GetDocDefNames();

                        foreach (var item in list)
                        {
                            Console.WriteLine(@"{0} - {1}",  item.Id, item.Name);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void GetDocDefRelationTest()
        {
            var appDefId = new Guid("4f9f2ae2-7180-4850-a3f4-5fb47313bcc0");

            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppProvider.CreateProvider(dataContext))
                    {
                        var docDefRepo = provider.Get<IDocDefRepository>();

                        var relations = docDefRepo.GetDocDefRelations(appDefId);

                        foreach (var relation in relations)
                        {
                            Console.WriteLine(@"{0} - {1}", relation.DocumentCaption, relation.AttributeCaption);
                        }
                    }
                }
            }
        }
    }
}
