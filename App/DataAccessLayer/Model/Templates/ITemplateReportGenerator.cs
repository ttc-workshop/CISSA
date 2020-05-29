using System;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;

namespace Intersoft.CISSA.DataAccessLayer.Model.Templates
{
    public interface ITemplateReportGenerator //: IDisposable
    {
        Stream Generate(Doc document, string fileName);
        Stream Generate(string fileName, IStringParams prms);
        Stream Generate(Doc doc, string fileName, IStringParams prms);
    }

    public interface ITemplateReportGeneratorProvider
    {
        ITemplateReportGenerator FindForTemplate(string fileName);
    }

    public class TemplateReportGeneratorProvider : ITemplateReportGeneratorProvider
    {
        public IAppServiceProvider Provider { get; private set; }
        public IDataContext DataContext { get; private set; }
        public Guid UserId { get; private set; }

       /* public TemplateReportGeneratorProvider(IDataContext dataContext, Guid userId)
        {
            DataContext = dataContext;
            UserId = userId;
        }*/

        public TemplateReportGeneratorProvider(IAppServiceProvider provider, IDataContext dataContext)
        {
            Provider = provider;
            DataContext = dataContext; //provider.Get<IDataContext>();

            // var userData = provider.Get<IUserDataProvider>();
            UserId = provider.GetCurrentUserId();
        }

        public ITemplateReportGenerator FindForTemplate(string fileName)
        {
            var ext = Path.GetExtension(fileName) ?? "";
            var fileHead = ext.Substring(0, 4).ToUpper();

            if (fileHead == ".PDF")
                return new PdfTemplateRepository(Provider, UserId);

            if (fileHead == ".XLS")
                return new ExcelTemplateRepository(Provider, UserId);

            return null;
        }
    }
}