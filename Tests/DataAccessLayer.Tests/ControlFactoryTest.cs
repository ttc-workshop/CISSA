using System;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    [TestClass]
    public class ControlFactoryTest
    {
        [TestMethod]
        public void CreateApplicationForm()
        {
            var formId = new Guid("{3D705AA9-3240-4CD7-9160-17892AF3A3F2}");

            using (var dataContext = new DataContext())
            {
                var control = dataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Form>().FirstOrDefault(c => c.Id == formId);
                var factory = new ControlFactory(dataContext /*, Guid.Empty*/);

                var form = factory.Create(control);

                Assert.IsNotNull(form);
                Assert.IsInstanceOfType(form, typeof (BizDetailForm));
                Assert.AreEqual(((BizDetailForm) form).Children.Count, 5);
            }
        }
    }
}
