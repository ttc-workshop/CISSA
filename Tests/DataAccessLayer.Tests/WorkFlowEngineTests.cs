using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    /// <summary>
    /// Сводное описание для WorkFlowEngineTests
    /// </summary>
    [TestClass]
    public class WorkFlowEngineTests
    {
        private readonly Guid _currentUserId = Guid.Parse("180B1E71-6CDA-4887-9F83-941A12D7C979");

        private const string ConnectionString =
            "Data Source=localhost;Initial Catalog=cissa-4atkal;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";

        [TestMethod]
        public void RunProcess()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("1fb55c09-e971-4b5f-95e3-1f621f192740");
                        var contextData = engine.Run(processId /*, _currentUserId*/);

                        Assert.IsNotNull(contextData);
                        Assert.AreEqual(WorkflowRuntimeState.ShowForm, contextData.State);
                    }
                }
            }
        }

        [TestMethod]
        public void RunProcessWithParams()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("1fb55c09-e971-4b5f-95e3-1f621f192740");
                        var parameters = new Dictionary<String, object>
                        {
                            {"SomeParam1", "Some string value"},
                            {"SomeParam2", 344},
                            {"SomeParam3", 38.4}
                        };

                        var context = new WorkflowContext(engine.Run(processId, /*_currentUserId,*/ parameters),
                            provider);

                        Assert.AreEqual("Some string value", context["SomeParam1"]);
                        Assert.AreEqual(344, context["SomeParam2"]);
                        Assert.AreEqual(38.4, context["SomeParam3"]);


                        Assert.IsNotNull(context);
                        Assert.AreEqual(WorkflowRuntimeState.ShowForm, context.State);
                    }
                }
            }
        }

        [TestMethod]
        public void RunSocialAppAssignment()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("{57517907-3647-4D43-9934-6DE75299538D}");
                        var parameters = new Dictionary<String, object>
                        {
                            {"InputDocumentId", Guid.Parse("3e7c777b-855c-40cc-bf23-b25cdc65951f")}
                        };

                        var contextData = engine.Run(processId, /*_currentUserId,*/ parameters);

                        Assert.IsNotNull(contextData);
                    }
                }
            }
        }

        [TestMethod]
        public void Continue()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("1fb55c09-e971-4b5f-95e3-1f621f192740");
                        var context = new WorkflowContextData(processId, _currentUserId);

                        WorkflowContextData resultContextData = engine.Continue(context);

                        Assert.IsNotNull(resultContextData);
                    }
                }
            }
        }

        [TestMethod]
        public void ScriptProcessRun()
        {

            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("{79428E66-E984-4305-9313-3B4044EDC2F8}");
                        var context = new WorkflowContext(engine.Run(processId), provider);

                        Assert.IsNotNull(context.GetVariable("Param"));
                        Assert.AreEqual(context.GetVariable("Param"), 111);
                        Assert.IsNotNull(context.CurrentDocument);
                        /*
                            Assert.IsNotNull(context.CurrentDocument["Lastname"]);
                            Assert.AreEqual(context.CurrentDocument["Lastname"], "Hello");
                            Assert.AreEqual(WorkflowRuntimeState.ShowForm, context.State);
                */

                        context.ShowReturn(Guid.Empty);

                        engine.Continue(context.Data);

                        Assert.AreEqual(WorkflowRuntimeState.Finish, context.State);
                    }
                }
            }
        }

        [TestMethod]
        public void BenefitProcessRun()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("{E12FE8FC-ACE8-469A-AE1F-D05B40D7AB0B}");
                        var context = new WorkflowContext(engine.Run(processId), provider);

                        Assert.AreEqual(WorkflowRuntimeState.ShowForm, context.State);
                        Assert.IsNotNull(context.CurrentFormId);
                        Assert.IsNotNull(context.CurrentDocument);

                        foreach (var ea in context.CurrentDocument.AttrEnum)
                            ea.Value = Guid.Parse("{7A68C30F-4CBC-49C1-A4E6-AB03B169C542}");
                        foreach (var dta in context.CurrentDocument.AttrDateTime)
                            dta.Value = DateTime.Now;

                        context.ShowReturn(Guid.Parse("{1F921E62-5B79-4F19-8360-000000000001}"));

                        engine.Continue(context.Data);

                        Assert.AreEqual(WorkflowRuntimeState.Finish, context.State);
                    }
                }
            }
        }

        [TestMethod]
        public void Process2Run()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("{B8C364D3-54F4-4C41-B04A-00B13562F1E4}");
                        var context = new WorkflowContext(engine.Run(processId), provider);

                        Assert.AreEqual(WorkflowRuntimeState.ShowForm, context.State);

                        foreach (var ea in context.CurrentDocument.AttrEnum)
                            ea.Value = Guid.Parse("{7A68C30F-4CBC-49C1-A4E6-AB03B169C542}");
                        foreach (var dta in context.CurrentDocument.AttrDateTime)
                            dta.Value = DateTime.Now;

                        context.ShowReturn(Guid.Parse("{1F921E62-5B79-4F19-8360-000000000001}"));

                        engine.Continue(context.Data);

                        Assert.AreEqual(WorkflowRuntimeState.Finish, context.State);
                    }
                }
            }
        }


        [TestMethod]
        public void SetGetParamDynaContext()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("{B8C364D3-54F4-4C41-B04A-00B13562F1E4}");
                        var context = new WorkflowContext(engine.Run(processId), provider);

                        dynamic con = new DynaContext(context);

                        con.Test = 123;

                        Assert.AreEqual(123, con.Test);
                    }
                }
            }
        }
    }
}
