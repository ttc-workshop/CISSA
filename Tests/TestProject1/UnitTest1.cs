using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject1.ServiceReference;

namespace TestProject1
{
    /// <summary>
    /// Сводное описание для UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var client = new DocManagerClient();
            client.ClientCredentials.UserName.UserName="R";
            client.ClientCredentials.UserName.Password="123";
            var doc = client.DocumentLoad(Guid.NewGuid());
        }
    }
}
