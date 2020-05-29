using System;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Core;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public class DocumentActivity : WorkflowActivity
    {
//        public WorkflowDocumentOperation Operation { get; private set; }

        public DocumentActivity(Document_Activity activity)
            : base(activity)
        {
            DocumentDefId = activity.Document_Id ?? Guid.Empty;
        }

        public Guid DocumentDefId { get; private set; }

        public override void Execute(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            //using (var docRepo = new DocRepository(dataContext, context.UserId))
            var docRepo = provider.Get<IDocRepository>();
            {
                try
                {
                    switch ((WorkflowDocumentOperation) Operation)
                    {
                        case WorkflowDocumentOperation.CreateNew:
                            context.CurrentDocument = docRepo.New(DocumentDefId);
                            break;
                        case WorkflowDocumentOperation.SaveCurrent:
                            if (context.CurrentDocument != null)
                            {
                                //lock(DocRepository.DocSaveLock)
                                context.CurrentDocument = docRepo.Save(context.CurrentDocument);
                                context.CurrentDocumentId = context.CurrentDocument.Id;

                                // Сохранение BLOB данных из контекста
                                if (context.Data.BlobDatas != null)
                                {
                                    foreach (var blobData in
                                        context.Data.BlobDatas.Where(
                                            bd => bd.DocumentId == context.CurrentDocument.Id))
                                    {
                                        docRepo.SaveBlobAttrData(context.CurrentDocument.Id, blobData.AttributeDefId,
                                            blobData.Data, blobData.FileName);
                                    }
                                }
                            }
                            else
                                throw new ApplicationException("Нет документа для сохранения");
                            break;
                        case WorkflowDocumentOperation.DeleteCurrent:
                            if (context.CurrentDocument != null) docRepo.DeleteById(context.CurrentDocument.Id);
                            else
                                throw new ApplicationException("Нет документа для удаления");
                            break;
                        case WorkflowDocumentOperation.CreateNewById:
                            if (context.CurrentDocumentDefId != null)
                                context.CurrentDocument = docRepo.New((Guid) context.CurrentDocumentDefId);
                            else
                                throw new ApplicationException("Не могу создать документ. Класс документа не указан!");
                            break;
                        case WorkflowDocumentOperation.LoadById:
                            if (context.CurrentDocumentId != null)
                                context.CurrentDocument = docRepo.LoadById((Guid) context.CurrentDocumentId);
                            else
                                throw new ApplicationException(
                                    "Не могу загрузить документ. Идентификатор документа не указан!");
                            break;
                        case WorkflowDocumentOperation.DeleteById:
                            if (context.CurrentDocumentId != null)
                                docRepo.DeleteById((Guid) context.CurrentDocumentId);
                            else
                                throw new ApplicationException(
                                    "Не могу удалить документ. Идентификатор документа не указан!");
                            break;
                        case WorkflowDocumentOperation.DefineDocDefId:
                            context.CurrentDocumentDefId = DocumentDefId;
                            break;
                            /*
                                            case WorkflowDocumentOperation.FindDoc:
                        //                        List<Guid> docs = new List<Guid>();
                                                var en = new Entities();

                                                var docs = en.Object_Defs.OfType<Document_Def>()
                                                break;
                        */
                    }

                    base.Execute(context, provider, dataContext);
                }
                catch (Exception e)
                {
                    OnException(context, e);
                    context.ThrowException(e);
                }
            }
        }
    }
}