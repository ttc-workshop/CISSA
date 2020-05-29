using System;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    /// <summary>
    /// Summary description for EnumRepositoryTests
    /// </summary>
    [TestClass]
    public class EnumRepositoryTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var rep = new EnumRepository();
            var enumId = Guid.NewGuid();
            var enumValue = Guid.NewGuid();

            rep.CheckEnumValue(enumId, enumValue);
        }

        [TestMethod]
        public void GetEnumList()
        {
            var rep = new EnumRepository();
            var enumId = Guid.Parse("151f3ec2-c7ab-4037-a3ce-b25688e9be0b");
            
            var list = rep.GetEnumItems(enumId);

            Assert.IsNotNull(list);
            Assert.AreNotEqual(0, list.Count);
        }

        [TestMethod]
        public void GetEnumIdByName()
        {
            const string enumName = "FirstEnum";
            var rep = new EnumRepository();

            var enumId = rep.GetEnumDefId(enumName);

            Assert.AreEqual(Guid.Parse("151f3ec2-c7ab-4037-a3ce-b25688e9be0b"), enumId);
        }

        [TestMethod]
        public void GetEnumValue()
        {
            var rep = new EnumRepository();
            var enumId = Guid.Parse("edbb69ba-218b-49bd-99ac-fcddad525cdd");

            Guid enumValueId = rep.GetEnumValueId(enumId, "Женский");

            Assert.AreEqual(Guid.Parse("22811bde-2380-4fbe-8336-4f262a34fcbb"), enumValueId);
        }
    }
}
