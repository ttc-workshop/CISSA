using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class BaseForm : BaseContextState, IFormContextState
    {
        public Guid FormId { get; private set; }
        public string FormCaption { get; set; }

        public IList<BizControlOption> ControlOptions { get; set; }

        public BaseForm(IContext context, Guid formId)
            : base(context)
        {
            FormId = formId;
        }

        public BaseForm(IContext context, ContextState previous, Guid formId)
            : base(context, previous)
        {
            FormId = formId;
        }

        public BizForm Form { get; internal set; }

        private readonly List<StateBlobData> _blobDatas = new List<StateBlobData>();
        public List<StateBlobData> BlobDatas { get { return _blobDatas; } }
        public BizForm GetCurrentForm(IContext context)
        {
            if (Form != null) return Form;

            Form = FindFormById(FormId);

            if (Form != null) return Form;

            var pm = context.GetPresentationProxy();
            return Form = pm.Proxy.GetAnyForm(FormId, context.GetLanguage());
        }

        public BizForm GetCurrentForm(IPresentationManager pm, int languageId = 0)
        {
            if (Form != null) return Form;

            return Form = FindFormById(FormId) ?? pm.GetAnyForm(FormId, languageId);
        }

        public Guid GetFormId()
        {
            return FormId;
        }

        public BizForm GetForm()
        {
            return Form;
        }

        public BizControl FindControl(Guid id)
        {
            var form = GetForm();

            return FindControlIn(form, id);
        }

        public BizControl FindControlIn(BizControl control, Guid controlId)
        {
            if (control == null) return null;
            if (control.Id == controlId) return control;

            if (control.Children != null)
                foreach (var child in control.Children)
                {
                    if (child.Id == controlId) return child;

                    var docControl = child as BizDocumentControl;
                    if (docControl != null && docControl.DocForm != null)
                    {
                        var docControlSub = FindControlIn(docControl.DocForm, controlId);
                        if (docControlSub != null) return docControlSub;
                    }
                    else
                    {
                        var docListControl = child as BizDocumentListForm;
                        if (docListControl != null && docListControl.TableForm != null)
                        {
                            var docListControlSub = FindControlIn(docListControl.TableForm, controlId);
                            if (docListControlSub != null) return docListControlSub;
                        }
                    }
                    var sub = FindControlIn(child, controlId);
                    if (sub != null) return sub;
                }

            return null;
        }

        public virtual void CheckFormLanguage(IContext context)
        {
            if (Form != null && Form.LanguageId != context.GetLanguage())
            {
                var pm = context.GetPresentationProxy();
                Form = pm.Proxy.TranslateForm(Form, context.GetLanguage());
            }
        }

        public BizForm SetFormDoc(IContext context, Doc doc)
        {
            var pm = context.GetPresentationProxy();
            Form = (BizDetailForm)pm.Proxy.SetFormDoc(Form, doc);
            var language = context.GetLanguage();
            if (Form.LanguageId != language)
                Form = (BizDetailForm)pm.Proxy.TranslateForm(Form, language);

            return Form;
        }

        public StateBlobData GetBlobData(IContext context, Guid docId, Guid attrDefId)
        {
            var data = BlobDatas.FirstOrDefault(d => d.DocumentId == docId && d.AttributeDefId == attrDefId);

            if (data != null) return data;

            var parent = Previous;
            while (parent != null)
            {
                var formState = parent as BaseForm;
                if (formState != null)
                {
                    data =
                        formState.BlobDatas.FirstOrDefault(d => d.DocumentId == docId && d.AttributeDefId == attrDefId);
                    if (data != null) return data;
                }
                else if (parent is RunProcess && ((RunProcess)parent).ProcessContext.BlobDatas != null)
                {
                    var blobData = ((RunProcess) parent).ProcessContext.BlobDatas.FirstOrDefault(bd => bd.DocumentId == docId && bd.AttributeDefId == attrDefId);
                    if (blobData != null)
                    {
                        data = new StateBlobData(docId, attrDefId, blobData.Data, blobData.FileName);
                        return data;
                    }
                }
                parent = parent.Previous;
            }

            if (context != null)
            {
                var dm = context.GetDocumentProxy();
                var docBlob = dm.Proxy.GetDocBlob(docId, attrDefId);
                if (docBlob != null)
                {
                    data = new StateBlobData(docId, attrDefId, docBlob.Data, docBlob.FileName);
                    BlobDatas.Add(data);
                    return data;
                }
            }
            return null;
        }

        public StateBlobData GetImageData(IContext context, Guid docId, Guid attrDefId, int height, int width)
        {
            var data = BlobDatas.FirstOrDefault(d => d.DocumentId == docId && d.AttributeDefId == attrDefId);

            // TODO: Сделать ResizeImage!
            if (data != null) return data;

            var parent = Previous;
            while (parent != null)
            {
                var formState = parent as BaseForm;
                if (formState != null)
                {
                    data =
                        formState.BlobDatas.FirstOrDefault(d => d.DocumentId == docId && d.AttributeDefId == attrDefId);
                    if (data != null) return data;
                }
                else if (parent is RunProcess && ((RunProcess)parent).ProcessContext.BlobDatas != null)
                {
                    var blobData = ((RunProcess)parent).ProcessContext.BlobDatas.FirstOrDefault(bd => bd.DocumentId == docId && bd.AttributeDefId == attrDefId);
                    if (blobData != null)
                    {
                        data = new StateBlobData(docId, attrDefId, blobData.Data, blobData.FileName);
                        return data;
                    }
                }
                parent = parent.Previous;
            }

            if (context != null)
            {
                var dm = context.GetDocumentProxy();
                var docBlob = dm.Proxy.GetDocBlob(docId, attrDefId);
                if (docBlob != null)
                {
                    data = new StateBlobData(docId, attrDefId, docBlob.Data, docBlob.FileName);
                    BlobDatas.Add(data);
                    return data;
                }
            }
            return null;
        }
        
        public void SetBlobData(Guid docId, Guid attrDefId, byte[] data, string fileName)
        {
            var blobData = GetBlobData(null, docId, attrDefId);
            if (blobData != null)
            {
                blobData.NewData = data;
                blobData.NewFileName = fileName;

                if (!BlobDatas.Contains(blobData)) BlobDatas.Add(blobData);
            }
            else
            {
                blobData = new StateBlobData(docId, attrDefId, null, String.Empty)
                {
                    NewData = data,
                    NewFileName = fileName
                };
                BlobDatas.Add(blobData);
            }
            var process = Previous as RunProcess;
            if (process != null && process.ProcessContext != null)
            {
                var processBlobData =
                    process.ProcessContext.BlobDatas != null
                        ? process.ProcessContext.BlobDatas.FirstOrDefault(
                            bd => bd.DocumentId == docId && bd.AttributeDefId == attrDefId)
                        : null;
                if (processBlobData != null)
                {
                    processBlobData.Data = data;
                    processBlobData.FileName = fileName;
                }
                else
                {
                    processBlobData = new BlobData
                    {
                        DocumentId = docId,
                        AttributeDefId = attrDefId,
                        Data = data,
                        FileName = fileName
                    };
                    if (process.ProcessContext.BlobDatas == null)
                        process.ProcessContext.BlobDatas = new List<BlobData>();
                    process.ProcessContext.BlobDatas.Add(processBlobData);
                }
            }
        }

        public void RemoveBlobData(Guid docId, Guid attrDefId)
        {
            var blobData = GetBlobData(null, docId, attrDefId);
            if (blobData != null)
            {
                blobData.NewData = null;
                blobData.NewFileName = String.Empty;
            }
        }
    }
}