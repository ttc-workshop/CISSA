using System;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Core;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    [DataContract]
    public class DocumentStateActivity : WorkflowActivity
    {
        public DocumentStateActivity(Document_State_Activity activity)
            : base(activity)
        {
            DocStateTypeId = activity.State_Type_Id ?? Guid.Empty;
        }

        public Guid DocStateTypeId { get; private set; }

        public override void Execute(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            //using (var docRepo = new DocRepository(dataContext, context.UserId))
            var docRepo = provider.Get<IDocRepository>();
            {
                try
                {
                    var doc = context.CurrentDocument;
                    if (doc != null)
                        docRepo.SetDocState(doc, DocStateTypeId);
                    else if (context.CurrentDocumentId != null)
                        docRepo.SetDocState((Guid) context.CurrentDocumentId, DocStateTypeId);
                    else
                        context.ThrowException("Document Id not found", "Идентификатор документа не найден!");

                    base.Execute(context, provider, dataContext);
                }
                catch(Exception e)
                {
                    OnException(context, e);
                    context.ThrowException(e);
                }
            }
        }
    }
}