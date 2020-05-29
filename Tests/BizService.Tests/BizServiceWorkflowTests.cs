using System;
using System.Collections.Generic;
using Intersoft.CISSA.BizServiceTests.FakeRepo;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.BizServiceTests
{
    /// <summary>
    /// Сводное описание для BizServiceWorkflowTests
    /// </summary>
    [TestClass]
    public class BizServiceWorkflowTests
    {
        public static Guid UserId = new Guid("{180B1E71-6CDA-4887-9F83-941A12D7C979}");

        private static BizService.BizService GetService()
        {
            var formRepo = new FakeWorkflowEngine(UserId);

            return new BizService.BizService(formRepo, "TestUserName").SetUserRepository(new FakeUserRepository());
        }

        [TestMethod]
        public void WorkflowExecute()
        {
            var service = GetService();
            Guid processId = Guid.NewGuid();
            service.WorkflowExecute(processId, null);
        }

        [TestMethod]
        public void WorkflowExecuteWitParams()
        {
            var service = GetService();
            Guid processId = Guid.NewGuid();
            var parms = new Dictionary<String, object>
                            {
                                {"SomeParam", "some string value"},
                                {"SomeParam2", 34},
                                {"SomeParam3", 6.234}
                            };

            service.WorkflowExecute(processId, parms);
        }


        [TestMethod]
        public void WorkflowContinue()
        {
            var service = GetService();
            
            var context = new WorkflowContextData(Guid.Empty, UserId);
            Guid userAction = Guid.NewGuid();
            
            service.WorkflowContinueWithUserAction(context, userAction);
        }
    }
}
