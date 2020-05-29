using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    [TestClass]
    public class SqlScriptRepositoryTest
    {
        [TestMethod]
        public void TestScriptExecute()
        {
            var repo = new SqlScriptRepository();
            IList<Guid> docs = repo.Execute(Guid.Parse("0cc4cef1-7f00-41ea-b7d9-93b860635aac"));

            Assert.IsNotNull(docs);
            Assert.AreNotSame(0, docs.Count);
        }
    }
}
