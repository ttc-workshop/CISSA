using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject1.ServiceReference;

namespace TestProject1
{
    /// <summary>
    /// Сводное описание для UnitTest1
    /// </summary>
    [TestClass]
    public class ServiceUserTests
    {
        [TestMethod]
        public void GetUserInfo()
        {
            var client = new UserManagerClient();
            client.ClientCredentials.UserName.UserName="D";
            client.ClientCredentials.UserName.Password="123";

            client.TryConnect();
            Console.WriteLine("TryConnect - Ok.");
            var userInfo = client.GetUserInfo();
            
            Assert.IsNotNull(userInfo);
            Console.WriteLine("username: {0}, firstName: {1}", userInfo.UserName, userInfo.FirstName);
        }

        [TestMethod]
        public void Test()
        {
            var client = new PresentationManagerClient();
            client.ClientCredentials.UserName.UserName = "R";
            client.ClientCredentials.UserName.Password = "123";

            //SomeClass bizControlData = client.Test();
            //var par1Type = bizControlData.Param1.GetType();
        }

        [TestMethod]
        public void GetForm()
        {
            var client = new PresentationManagerClient();
            client.ClientCredentials.UserName.UserName = "R";
            client.ClientCredentials.UserName.Password = "123";

            var frm = client.GetDetailForm(
                            Guid.Parse("B88FE6CA-D327-44C5-BC84-37F47D85FB4A"), 0);
            
            Assert.IsNotNull(frm);
        }

        [TestMethod]
        public void GetFormCacheTest()
        {
            var client = new PresentationManagerClient();
            client.ClientCredentials.UserName.UserName = "R";
            client.ClientCredentials.UserName.Password = "123";

            for (int i = 0; i < 1000; i++)
            {
                var frm = client.GetDetailForm(
                                Guid.Parse("B88FE6CA-D327-44C5-BC84-37F47D85FB4A"), 0);

                Assert.IsNotNull(frm);
            }
        }

        [TestMethod]
        public void TestClientOpenningPerformance()
        {
            for (int i = 0; i < 100; i++)
            {
                var watch = new Stopwatch();
                var watch1 = new Stopwatch();
                watch.Start();
                watch1.Start();
                var client = new PresentationManagerClient();
                client.ClientCredentials.UserName.UserName = "D";
                client.ClientCredentials.UserName.Password = "123";
                client.Open();
                watch1.Stop();
                var ms1 = watch1.ElapsedMilliseconds;
                var watch2 = new Stopwatch();
                watch2.Start();
                client.GetMenus(0);
                watch2.Stop();
                var ms2 = watch2.ElapsedMilliseconds;
                var watch3 = new Stopwatch();
                watch3.Start();
                client.Close();
                watch3.Stop();
                watch.Stop();

                Console.WriteLine(i + ". start: " + ms1 + " ms,\texecute: " + ms2 + " ms,\tstop: " + watch3.ElapsedMilliseconds + " ms,\ttotal: " + watch.ElapsedMilliseconds);
                /*var frm = client.GetDetailForm(
                                Guid.Parse("B88FE6CA-D327-44C5-BC84-37F47D85FB4A"), 0);*/
            }
        }

        [TestMethod]
        public void TestChannelOpenningPerformance()
        {
            for (int i = 0; i < 100; i++)
            {
                var watch = new Stopwatch();
                var watch1 = new Stopwatch();
                watch.Start();
                watch1.Start();
                var factory = new ChannelFactory<IPresentationManager>(new WSHttpBinding("WSHttpBinding_IPresentationManager"), "http://localhost/AsistService/BizService.svc");
                factory.Credentials.UserName.UserName = "D";
                factory.Credentials.UserName.Password = "123";
                var client = factory.CreateChannel();
                watch1.Stop();
                var ms1 = watch1.ElapsedMilliseconds;
                var watch2 = new Stopwatch();
                watch2.Start();
                client.GetMenus(0);
                watch2.Stop();
                var ms2 = watch2.ElapsedMilliseconds;
                var watch3 = new Stopwatch();
                watch3.Start();
                ((ICommunicationObject) client).Close();
                watch3.Stop();
                watch.Stop();
                
                Console.WriteLine(i + ". start: " + ms1 + " ms,\texecute: " + ms2 + " ms,\tstop: " + watch3.ElapsedMilliseconds + " ms,\ttotal: " + watch.ElapsedMilliseconds);
                /*var frm = client.GetDetailForm(
                                Guid.Parse("B88FE6CA-D327-44C5-BC84-37F47D85FB4A"), 0);*/
            }
        }
    }
}
