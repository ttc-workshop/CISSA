using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace ConsoleApplication1.Lists
{
    public class FormControls2Excel
    {
        public const string ConnectionString = "Data Source=195.38.189.100;Initial Catalog=asist_db;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True;Connect Timeout=120";
        public const string FileName = @"c:\distr\cissa\FormControls4Hints.xml";

        public static void Build(IAppServiceProvider provider, IDataContext dataContext)
        {
            var en = dataContext.GetEntityDataContext();
            var formRepo = provider.Get<IFormRepository>();

            var root = new XElement("Forms");
            foreach (
                var id in
                    en.Entities.Object_Defs.OfType<Form>()
                        .Where(o => o.Deleted == null || o.Deleted == false)
                        .Select(o => o.Id))
            {
                var form = formRepo.FindDetailForm(id);

                if (form != null)
                {
                    AddFormControls(form, root);
                }
            }
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                NewLineOnAttributes = true,
                IndentChars = "\t",
                Encoding = Encoding.UTF8
            };
            using (var xmlWriter = XmlWriter.Create(FileName, settings))
            {
                root.WriteTo(xmlWriter);
            }
        }

        private static void AddFormControls(BizControl control, XElement parent)
        {
            var xControl = new XElement(GetControlTypeName(control), new XAttribute("id", control.Id), new XAttribute("name", control.Caption ?? control.Name ?? ""), new XAttribute("hint", ""));
            
            parent.Add(xControl);

            if (control.Children != null)
                foreach (var child in control.Children)
                {
                    AddFormControls(child, xControl);
                }
        }

        private static string GetControlTypeName(BizControl control)
        {
            if (control is BizDetailForm) return "form";
            if (control is BizPanel) return "panel";
            if (control is BizDocumentControl) return "documentPanel";
            if (control is BizDocumentListForm) return "documentList";
            if (control is BizEdit) return "edit";
            if (control is BizButton) return "button";
            if (control is BizComboBox) return "comboBox";
            if (control is BizTableColumn) return "panel";
            return "control";
        }
    }
}
