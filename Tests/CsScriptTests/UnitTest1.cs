using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using CSScriptLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsScriptTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class CsScriptTest
    {
        public CsScriptTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private const string script1 =
            @"public class First {
    public string Hello() { return ""Hello world!""; }
    public int Add(int a, int b) { return a + b; }
}";

        private const string script2 =
            @"public static class Second {
    public static string SayFirstHello() { return new First().Hello(); }
    public static int Calc(int a, int b) { return new First().Add(a, b); }
}";

        [TestMethod]
        public void TwoScriptCommunication()
        {
            var assembly1 = CSScript.LoadCode(script1);
            var assembly2 = CSScript.LoadCode(script2);

            MethodDelegate method = assembly2.GetStaticMethod("*.SayFirstHello");

            Assert.AreEqual(method(), "Hello world!");
        }

        [TestMethod]
        public void TwoScriptCommunication2()
        {
            var assembly1 = CSScript.LoadCode(script1);
            var assembly2 = CSScript.LoadCode(script2);

            MethodDelegate method = assembly2.GetStaticMethod("*.Calc", 10, 33);

            Assert.AreEqual(method(14, 33), 47);
        }

        [TestMethod]
        public void TwoScriptCommunication3()
        {
            CSScript.LoadCode(script1);
            var assembly2 = CSScript.LoadCode(script2);
            CSScript.LoadCode(script2);
            MethodDelegate method = assembly2.GetStaticMethod("*.Calc", 10, 33);
            CSScript.LoadCode(script1);

            Assert.AreEqual(method(14, 33), 47);
        }

        private const string script3 =
            @"public string Hello() { return ""Hello world!""; }
    public int Add(int a, int b) { return a + b; }";

        private const string script4 =
            @"public static string SayFirstHello() { return new First().Hello(); }
    public static int Calc(int a, int b) { return new First().Add(a, b); }";

        [TestMethod]
        public void TwoScriptCommunication4()
        {
            CSScript.LoadCode(script1);
            var assembly2 = CSScript.LoadMethod(script4);

            MethodDelegate method = assembly2.GetStaticMethod("*.Calc", 0, 0);

            Assert.AreEqual(method(14, 33), 47);
        }
    }
}
