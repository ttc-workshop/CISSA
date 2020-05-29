using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public class UploadFile : BaseContextState
    {
        public string Message { get; private set; }
        public string FileExt { get; private set; }

        public IList<UserAction> UserActions { get; private set; }

        public UploadFile(IContext context, string message, IList<UserAction> userActions = null)
            : base(context)
        {
            Message = message;
            UserActions = userActions;
        }

        public UploadFile(IContext context, ContextState previous, string message, IList<UserAction> userActions = null)
            : base(context, previous)
        {
            Message = message;
            UserActions = userActions;
        }

        private readonly List<StateBlobData> _uploadedFiles = new List<StateBlobData>();
        public List<StateBlobData> UploadedFiles { get { return _uploadedFiles; } }

        public void AddFile(string fileName, byte[] data)
        {
            var uploadedFile = new StateBlobData(Guid.Empty, Guid.Empty, data, fileName);
            UploadedFiles.Add(uploadedFile);
        }

        public void RemoveBlobData(string fileName)
        {
            var uploadedFile = UploadedFiles.FirstOrDefault(f => String.Equals(f.FileName, fileName, StringComparison.OrdinalIgnoreCase));
            if (uploadedFile != null)
                UploadedFiles.Remove(uploadedFile);
        }
        public override ContextAction GetAction(IContext context)
        {
            return new ContextAction("Form", "Upload");
        }
    }
}