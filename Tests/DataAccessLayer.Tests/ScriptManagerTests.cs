using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    [TestClass]
    public class ScriptManagerTests
    {
        [TestMethod]
        public void TestMethodSimple()
        {
            const string script = "";

            var scriptManager = new ScriptManager(script);

            Assert.AreEqual("", scriptManager.PreparedScript);
        }

        [TestMethod]
        public void TestNull()
        {
            var scriptManager = new ScriptManager(null);

            Assert.AreEqual("", scriptManager.PreparedScript);
        }

        //[TestMethod]
        //public void TestMethodMultLineCommentRemove()
        //{
        //    const string script = "  /* привет это однострочный комент для удаления */ ";

        //    var scriptManager = new ScriptManager(script);

        //    Assert.AreEqual("", scriptManager.PreparedScript);
        //}

        //[TestMethod]
        //public void TestMethodOneLineCommentRemove()
        //{
        //    const string script = "  // привет это однострочный комент для удаления ";

        //    var scriptManager = new ScriptManager(script);

        //    Assert.AreEqual("", scriptManager.PreparedScript);
        //}

        //[TestMethod]
        //public void TestMethodOneLineCommentRemove2()
        //{
        //    const string script = "  // привет это однострочный комент для удаления " +
        //        " var xxx = 123 -- some comment \n" +
        //        " // hi this is comment";

        //    var scriptManager = new ScriptManager(script);

        //    Assert.AreEqual("", scriptManager.PreparedScript);
        //}

        //[TestMethod]
        //public void TestMethodOneLineCommentRemove3()
        //{
        //    const string script = "  // привет это однострочный комент для удаления \n" +
        //        " var xxx = 123 // some comment \n" +
        //        " // hi this is comment";

        //    var scriptManager = new ScriptManager(script);

        //    Assert.AreEqual("var xxx = 123", scriptManager.PreparedScript);
        //}

        //[TestMethod]
        //public void TestMethodMultiLineCommentRemove2()
        //{
        //    const string script = "  123/* привет это однострочный \n еще строка \n комент для удаления ";

        //    var scriptManager = new ScriptManager(script);

        //    Assert.AreEqual("123", scriptManager.PreparedScript);
        //}

        //[TestMethod]
        //public void TestMethodMultiLineCommentRemove()
        //{
        //    const string script = "  123/* привет это однострочный \n еще строка \n комент для удаления */ ";

        //    var scriptManager = new ScriptManager(script);

        //    Assert.AreEqual("123", scriptManager.PreparedScript);
        //}

        //[TestMethod]
        //public void TestMethodMultiCommentRemove()
        //{
        //    const string script = "  123/* привет это однос*/ 33/* привет это однос*/ привет это /*однострочный \n еще строка \n комент для удаления */ ";

        //    var scriptManager = new ScriptManager(script);

        //    Assert.AreEqual("123 33 привет это", scriptManager.PreparedScript);
        //}


        [TestMethod]
        public void SimpleTestFunction()
        {
            // using (var dataContext = new DataContext()) // Устарело
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create())
            {
                const string script = "{ context[\"Test\"] = 555; }";

                var scriptManager = new ScriptManager(script);

                var context = new WorkflowContext(new WorkflowContextData(), provider /*dataContext*/);
                scriptManager.Execute(context);

                Assert.AreEqual(script, scriptManager.PreparedScript);
                Assert.AreEqual(555, context["Test"]);
            }
        }

        [TestMethod]
        public void SimpleTestFunctionWithComment()
        {
            // using (var dataContext = new DataContext()) // Устарело
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            using (var provider = factory.Create())
            {
                const string script = "{ /*som coment*/context[\"Test\"]/* another comment */ = 555; }";

                var scriptManager = new ScriptManager(script);

                var context = new WorkflowContext(new WorkflowContextData(), provider /*dataContext*/);
                scriptManager.Execute(context);

                //Assert.AreEqual("{ context[\"Test\"] = 555; }", scriptManager.PreparedScript);
                Assert.AreEqual(555, context["Test"]);
            }
        }

        [TestMethod]
        public void ContextTest()
        {
            using (var dataContext = new DataContext())
            {
                var context = new WorkflowContext(new WorkflowContextData(), dataContext);
                context["Test"] = 555;
                Assert.AreEqual(555, context["Test"]);
            }
        }

        [TestMethod]
        public void SimpleTestExpression()
        {
            using (var dataContext = new DataContext())
            {
                const string script = "( 2 + 2 )";

                var scriptManager = new ScriptManager(script);

                var context = new WorkflowContext(new WorkflowContextData(), dataContext);
                object result = scriptManager.ExecuteWithResult(context);

                Assert.AreEqual(script, scriptManager.PreparedScript);
                Assert.AreEqual(4, result);
            }
        }

        [TestMethod]
        public void SimpleTestExpressionWithComment()
        {
            using (var dataContext = new DataContext())
            {
                const string script = "( 5 /* som comment */+ 5 )";

                var scriptManager = new ScriptManager(script);

                var context = new WorkflowContext(new WorkflowContextData(), dataContext);
                object result = scriptManager.ExecuteWithResult(context);

                //Assert.AreEqual("( 5 + 5 )", scriptManager.PreparedScript);
                Assert.AreEqual(10, result);
            }
        }

        [TestMethod]
        public void SimpleTestClass()
        {
            const string script = " class Expression { \n " +
                                      "   public static void Execute(WorkflowContext context) { " +
                                      "     context[\"xxx\"] = 123 ; " +
                                      "   } " +
                                      " } ";

            var scriptManager = new ScriptManager(script);

            using (var dataContext = new DataContext())
            {
                var context = new WorkflowContext(new WorkflowContextData(), dataContext);
                scriptManager.Execute(context);

                Assert.AreEqual(123, context["xxx"]);
            }
        }

        [TestMethod]
        public void SimpleTestClassWithComments()
        {
            const string script = 
                " class Expression /*som comments*/ { \n " +
                "   public static void Execute(WorkflowContext context) { " +
                "     context[\"xxx\"] = 123 ; /*some comment*/ " +
                "   } " +
                " } ";

            var scriptManager = new ScriptManager(script);

            using (var dataContext = new DataContext())
            {
                var context = new WorkflowContext(new WorkflowContextData(), dataContext);
                scriptManager.Execute(context);

                Assert.AreEqual(123, context["xxx"]);
            }
        }

        [TestMethod]
        public void SimpleTestClassWithComments2()
        {
            const string script = 
@"// Тело процедуры
{
    context.FilterDocument = context.CurrentDocument;" + "\n}";

            var scriptManager = new ScriptManager(script);

            using (var dataContext = new DataContext())
            {
                var context = new WorkflowContext(new WorkflowContextData {CurrentDocument = new Doc()}, dataContext);

                scriptManager.Execute(context);

                Assert.IsNotNull(context["FilterDocument"]);
            }
        }    
    }
}
