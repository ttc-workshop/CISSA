using System;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    [TestClass]
    public class PermissionRepositoryTests
    {
        //[TestMethod]
        public void ListOfAccessibleDocuments()
        {
            using (var dataContext = new DataContext())
            {
                var perRepo = new PermissionRepository(dataContext);

                Guid userId = Guid.Parse("180B1E71-6CDA-4887-9F83-941A12D7C979");

                /*var result = perRepo.ListOfAccessibleObjects(userId); // Method removed!!
                for (int i = 0; i < 100; i++)
                {
                    result = perRepo.ListOfAccessibleObjects(userId);
                }

                Assert.IsNotNull(result);
                Assert.AreNotEqual(0, result.Count);*/
            }
        }

        //[TestMethod]
        public void TestEmptyUserId()
        {
            using (var dataContext = new DataContext())
            {
                var perRepo = new PermissionRepository(dataContext);

                var userId = new Guid();

                /* var result = perRepo.ListOfAccessibleObjects(userId); // Not supported

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count);*/
            }
        }
    }
}
