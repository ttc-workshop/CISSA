using System;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Core;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    public enum PresentationActivityType
    {
        Default = 0,
        OpenForm = 1,
        OpenSelectForm = 2,
        OpenParamForm = 3,
        ShowMessage = 4,
        DefineFormId = 5, // ???
        GenerateReportFromTemplate = 6,
        DownloadFile = 7,
        UploadFile = 8
    }

    [DataContract]
    public class PresentationActivity : WorkflowActivity
    {
//        public Guid ExitUserAction = Guid.Parse("{8EF9BEAC-F47A-46F4-A259-800DA813FB63}");

        public PresentationActivity(Presentation_Activity activity/*, DataContext dataContext*/)
            : base(activity/*, dataContext*/)
        {
            FormId = activity.Form_Id;
            Message = activity.Message;
        }

        public Guid? FormId { get; private set; }
        public string Message { get; private set; }

        public override void Execute(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            if ((Operation >= (int) PresentationActivityType.Default &&
                 Operation <= (int) PresentationActivityType.OpenParamForm) && FormId == null)
                context.ThrowException("FormId is null", "Идентификатор формы не указан!");

            switch (Operation)
            {
                case (int)PresentationActivityType.Default:
                case (int)PresentationActivityType.OpenForm:
                    context.ShowForm(FormId);
                    context.FormCaption = Message;
                    break;
                case (int)PresentationActivityType.OpenSelectForm:
                    context.ShowSelectForm(FormId);
                    context.FormCaption = Message;
                    break;
                case (int)PresentationActivityType.OpenParamForm:
                    context.ShowParamForm(FormId);
                    context.FormCaption = Message;
                    break;
                case (int)PresentationActivityType.ShowMessage:
                    context.ShowMessage(String.IsNullOrEmpty(Message) ? context.Message : Message);
                    break;
                case (int)PresentationActivityType.DefineFormId:
                    context.CurrentFormId = FormId;
                    base.Execute(context, provider, dataContext);
                    break;
                case (int)PresentationActivityType.GenerateReportFromTemplate:
                    context.ShowTemplateReport(Message);
                    break;
                case (int)PresentationActivityType.DownloadFile:
                    context.SendFile(Message);
                    break;
                case (int)PresentationActivityType.UploadFile:
                    context.UploadFile(Message);
                    break;
                default:
                    context.ShowForm(FormId);
                    break;
            }
        }

        public override void AfterExecution(WorkflowContext context, IAppServiceProvider provider, IDataContext dataContext)
        {
            base.Execute(context, provider, dataContext);
        }
    }
}