using System;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Intersoft.CISSA.DataAccessLayerTests
{
    /// <summary>
    /// Сводное описание для WorkFlowRepositoryTests
    /// </summary>
    [TestClass]
    public class WorkFlowRepositoryTests
    {
        [TestMethod]
        public void LoadPresentationActivityById()
        {
            var repo = new WorkflowRepository();

            Guid activityId1 = Guid.Parse("09ae9d17-0b21-41c3-aeab-6b535146d421");
            var activity1 = (PresentationActivity)repo.LoadActivityById(activityId1);

            Assert.IsNotNull(activity1);
        }

        [TestMethod]
        public void LoadDocumentActivityById()
        {
            var repo = new WorkflowRepository();

            Guid activityId2 = Guid.Parse("09ae9d17-0b21-41c3-aeab-6b535146d422");
            var activity2 = (DocumentActivity)repo.LoadActivityById(activityId2);

            Assert.IsNotNull(activity2);
        }

        [TestMethod]
        public void LoadDocumentStateActivityById()
        {
            var repo = new WorkflowRepository();

            Guid activityId3 = Guid.Parse("09ae9d17-0b21-41c3-aeab-6b535146d423");
            var activity3 = (DocumentStateActivity)repo.LoadActivityById(activityId3);

            Assert.IsNotNull(activity3);
        }

        [TestMethod]
        public void LoadProcessById()
        {
            var repo = new WorkflowRepository();
            Guid processId = Guid.Parse("1fb55c09-e971-4b5f-95e3-1f621f192740");

            WorkflowProcess process = repo.LoadProcessById(processId);

            Assert.IsNotNull(process);
        }

        // [TestMethod]
        public void LoadProcessActivities()
        {
            var repo = new WorkflowRepository();
            Guid processId = Guid.Parse("1fb55c09-e971-4b5f-95e3-1f621f192740");

            //var activities = repo.LoadProcessActivities(processId); // Не используется

            /*Assert.IsNotNull(activities);
            Assert.AreNotSame(3, activities.Count);*/
        }
    }
}
