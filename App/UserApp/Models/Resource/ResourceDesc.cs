using System;
using System.IO;
using System.Web.Mvc;

namespace Intersoft.CISSA.UserApp.Models.Resource
{
    public abstract class ResourceDesc
    {
        public Guid Id { get; private set; }

        protected ResourceDesc()
        {
            Id = Guid.NewGuid();
        }

        public abstract string GetContentType();
        public abstract string GetDownloadFileName();

        public abstract ActionResult GetResourceFile(BaseController controller);
    }

    public class FileResource : ResourceDesc
    {
        public string FileName { get; private set; }

        public FileResource(string fileName)
        {
            FileName = fileName;
        }

        public override string GetContentType()
        {
            return "application/" + Path.GetExtension(FileName);
        }

        public override string GetDownloadFileName()
        {
            return FileName;
        }

        public override ActionResult GetResourceFile(BaseController controller)
        {
            var rm = controller.GetReportProxy();
            var buffer = rm.Proxy.GetFile(FileName);

            return controller.File(buffer, GetContentType(), FileName);
        }
    }
}