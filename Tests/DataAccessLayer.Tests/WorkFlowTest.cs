using System;
using System.Collections;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    /// <summary>
    /// Сводное описание для WorkFlowTest1
    /// </summary>
    [TestClass]
    public class WorkFlowTest
    {
        private readonly Guid _currentUserId = Guid.Parse("180B1E71-6CDA-4887-9F83-941A12D7C979");

        [TestMethod]
        public void TestMethod1()
        {
            using (var connection = AppServiceProviderBase.GetConnection())
            {
                using (var dataContext = AppServiceProviderBase.GetDataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("B8C364D3-54F4-4C41-B04A-00B13562F1E4");
                        WorkflowContextData contextData = engine.Run(processId);

                        Assert.IsNotNull(contextData);
                        Assert.AreEqual(WorkflowRuntimeState.ShowForm, contextData.State);
                    }
                }
            }
        }

        [TestMethod]
        public void TestNaznacheniePosobiiCancel()
        {
            using (var connection = AppServiceProviderBase.GetConnection())
            {
                using (var dataContext = AppServiceProviderBase.GetDataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("{E12FE8FC-ACE8-469A-AE1F-D05B40D7AB0B}");
                        WorkflowContextData contextData = engine.Run(processId);
                        var context = new WorkflowContext(contextData, provider);

                        Assert.IsNotNull(contextData);
                        Assert.AreEqual(WorkflowRuntimeState.ShowForm, contextData.State);

                        // выбрано действие "Cancel"
                        Guid seletedActionId = Guid.Parse("1F921E62-5B79-4F19-8360-000000000002");

                        context.ShowReturn(seletedActionId);
                        engine.Continue(contextData);

                        Assert.IsNotNull(contextData);
                        Assert.AreEqual(WorkflowRuntimeState.Finish, contextData.State);
                    }
                }
            }
        }

        [TestMethod]
        public void TestNaznacheniePosobiiOk()
        {
            using (var connection = AppServiceProviderBase.GetConnection())
            {
                using (var dataContext = AppServiceProviderBase.GetDataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("{E12FE8FC-ACE8-469A-AE1F-D05B40D7AB0B}");
                        var contextData = engine.Run(processId);
                        var context = new WorkflowContext(contextData, provider);

                        Assert.IsNotNull(contextData);
                        Assert.AreEqual(WorkflowRuntimeState.ShowForm, contextData.State);
                        Assert.IsNotNull(contextData.CurrentFormId);

                        var formRepo = provider.Get<IFormRepository>();
                        var form = formRepo.GetForm((Guid) contextData.CurrentFormId);
                        Assert.IsNotNull(form.DocumentDefId);
                        Assert.IsNotNull(contextData.CurrentDocument);

                        formRepo.SetFormDoc(form, contextData.CurrentDocument);

                        foreach (var cntr in form.Children)
                        {
                            if (cntr is BizComboBox)
                            {
                                var combo = (BizComboBox) cntr;
                                combo.Value = Guid.Parse("7a68c30f-4cbc-49c1-a4e6-ab03b169c542");

                                // form.Children.Remove(cntr);
                                // form.Children.Add(combo);
                            }
                            if (cntr is BizEditText)
                                ((BizEditText) cntr).Value = "Hello world!!!";
                            if (cntr is BizEditDateTime)
                                ((BizEditDateTime) cntr).Value = DateTime.Now;
                        }
                        formRepo.GetFormDoc(form, contextData.CurrentDocument);

                        // выбрано действие "ОК"
                        var seletedActionId = Guid.Parse("1F921E62-5B79-4F19-8360-000000000001");

                        context.ShowReturn(seletedActionId);
                        engine.Continue(contextData);

                        Assert.IsNotNull(contextData);
                        Assert.AreEqual(WorkflowRuntimeState.Finish, contextData.State);
                    }
                }
            }
        }

        [TestMethod]
        public void DocStateActivityTest()
        {
            using (var connection = AppServiceProviderBase.GetConnection())
            {
                using (var dataContext = AppServiceProviderBase.GetDataContext(connection))
                {
                    using (var provider = AppServiceProviderBase.GetProvider(dataContext))
                    {
                        var engine = new WorkflowEngine(provider, dataContext);
                        var processId = Guid.Parse("E271DD2E-5D74-4731-A357-0C7ABCFA4B07");
                        WorkflowContextData contextData = engine.Run(processId);

                        Assert.IsNotNull(contextData);
                        Assert.AreEqual(WorkflowRuntimeState.Finish, contextData.State);
                    }
                }
            }
        }

        [TestMethod]
        public void TestTypeIsOperation()
        {
            var t = new List<Doc>();

            var ot = t.ConvertAll<object>(x => (object) x);

            Assert.IsTrue(t is IEnumerable);
            Assert.IsTrue(t.GetType().IsGenericType);
            Assert.IsTrue(t.GetType().GenericTypeArguments.Length == 1);
            var gt = t.GetType().GenericTypeArguments[0];
            Assert.IsTrue(gt.IsValueType || gt == typeof(String));
            //Assert.IsTrue(t is IEnumerable<Guid>);
            Assert.IsTrue(ot is IEnumerable<object>);
        }
    }
}
