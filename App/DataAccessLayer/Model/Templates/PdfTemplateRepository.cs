using System;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.PdfFiller;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Templates
{
    public class PdfTemplateRepository : ITemplateReportGenerator
    {
        private IAppServiceProvider Provider { get; set; }
        private IDataContext DataContext { get; set; }
        //private readonly bool _ownDataContext;

        private Guid UserId { get; set; }

        public PdfTemplateRepository(IAppServiceProvider provider, Guid userId)
        {
            Provider = provider;
            UserId = userId;
        }
        public PdfTemplateRepository(IDataContext dataContext, Guid userId)
        {
            /*if (dataContext == null)
            {
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else*/
                DataContext = dataContext;
            UserId = userId;
        }
/*

        public PdfTemplateRepository(Guid userId) : this(null, userId) {}
        public PdfTemplateRepository(IDataContext dataContext) : this(dataContext, Guid.Empty) {}
*/
        
        public Stream Generate(Doc document, string fileName)
        {
            var stream = new MemoryStream();
            try
            {
                using (var documentBuilder = new DocumentBuilder(fileName, stream, "\\windows\\fonts\\times.ttf"))
                {
                    if (document == null) return stream;
                    using (var dynaDoc = new DynaDoc(document, UserId, Provider))
                        FillDoc(documentBuilder, "", dynaDoc);
                }

                return stream;
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        public Stream Generate(string fileName, IStringParams prms)
        {
            var stream = new FileStream(fileName, FileMode.Open);
            try
            {
                using (var documentBuilder = new DocumentBuilder(fileName, stream, "\\windows\\fonts\\times.ttf"))
                {
                    FillDoc(documentBuilder, prms);
                }

                return stream;
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        public Stream Generate(Doc document, string fileName, IStringParams prms)
        {
            var stream = new MemoryStream();
            try
            {
                using (var documentBuilder = new DocumentBuilder(fileName, stream, "\\windows\\fonts\\times.ttf"))
                {
                    if (document != null)
                        using (var dynaDoc = new DynaDoc(document, UserId, Provider))
                            FillDoc(documentBuilder, "", dynaDoc);
                    FillDoc(documentBuilder, prms);
                }

                return stream;
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        public byte[] Generate(Guid docId, string fileName)
        {
            using (var stream = new MemoryStream())
            {
                using (var documentBuilder = new DocumentBuilder(fileName, stream, "\\windows\\fonts\\times.ttf"))
                {
                    using (var dynaDoc = new DynaDoc(docId, UserId, Provider))
                        FillDoc(documentBuilder, "", dynaDoc);
                }

                return stream.GetBuffer();
            }
        }

        public byte[] Generate(Guid docId, string fileName, IStringParams prms)
        {
            using (var stream = new MemoryStream())
            {
                using (var documentBuilder = new DocumentBuilder(fileName, stream, "\\windows\\fonts\\times.ttf"))
                {
                    using (var dynaDoc = new DynaDoc(docId, UserId, Provider))
                        FillDoc(documentBuilder, "", dynaDoc);
                    FillDoc(documentBuilder, prms);
                }

                return stream.GetBuffer();
            }
        }

        private void FillDoc(DocumentBuilder builder, string prefix, DynaDoc doc)
        {                            
            
            var pref = String.IsNullOrEmpty(prefix) ? "" : prefix + ".";

            //foreach (var attr in doc.Doc.Attributes)
            //{
            //    var fn = Logger.GetLogFileName("ReportManagerError");
            //    Logger.OutputLog(fn, "ERROR BLOB: " + attr.AttrDef.Name + " ----- " + attr.GetType() + " ----- " + attr.ObjectValue.ToString().Length + " ----- " + (attr is BlobAttribute));
            //}

            foreach (var attr in doc.Doc.Attributes)
            {
                var value = attr.ObjectValue;

                try
                {
                    if (attr is BlobAttribute)
                    {
                        //var fn = Logger.GetLogFileName("ReportManagerError");
                        //Logger.OutputLog(fn, "ERROR BLOB: " + attr.AttrDef.Name + " ----- " + attr.GetType());

                        IDocRepository _docRepo = Provider.Get<IDocRepository>();

                        BlobData img = _docRepo.GetBlobAttrData(doc.Doc.Id, attr.AttrDef);

                        //Logger.OutputLog(fn, "ERROR BLOB: " + img.FileName);

                        // We assume only Image types blobs are used in reports
                        builder.SetBlobField(pref + attr.AttrDef.Name, img.Data);

                        continue;
                    }
                }
                catch (Exception e)
                {
                    var fn = Logger.GetLogFileName("ReportManagerError");
                    Logger.OutputLog(fn, e, e.Message);
                }

                if (value == null) continue;

                if (attr is DocAttribute)
                {
                    var attrDoc = doc.GetAttrDoc((DocAttribute)attr);

                    if (attrDoc != null)
                        using (var dynaDoc = new DynaDoc(attrDoc, UserId, Provider))
                            FillDoc(builder, pref + attr.AttrDef.Name, dynaDoc);
                }
                else if (attr is DocListAttribute)
                {
                    var index = 0;
                    foreach (var docItem in doc.GetAttrDocList((DocListAttribute)attr))
                    {
                        using (var dynaDoc = new DynaDoc(docItem, UserId, Provider))
                            FillDoc(builder, string.Format("{0}{1}.{2}", pref, attr.AttrDef.Name, index), dynaDoc);
                        index++;
                    }
                }
                else if (attr is EnumAttribute)
                {
                    var enumValue = doc.GetAttrEnum((EnumAttribute)attr);

                    if (enumValue != null)
                        builder.SetField(pref + attr.AttrDef.Name, enumValue.Value);
                }
                else if (attr is DateTimeAttribute)
                {
                    builder.SetField(pref + attr.AttrDef.Name, String.Format("{0:d}", value));
                }
                else if (attr is CurrencyAttribute || attr is FloatAttribute)
                {
                    builder.SetField(pref + attr.AttrDef.Name, String.Format("{0:F2}", value));
                }               
                else
                    builder.SetField(pref + attr.AttrDef.Name, value);
            }
        }

        private static void FillDoc(DocumentBuilder builder, IStringParams prms)
        {
            if (prms == null) return;

            foreach (var fieldName in builder.GetFieldNames())
            {
                var value = prms.Get(fieldName);

                if (String.IsNullOrEmpty(value)) continue;

                builder.SetField(fieldName, value);
            }
        }

        /*public void Dispose()
        {
            if (_ownDataContext && DataContext != null)
            {
                try
                {
                    DataContext.Dispose();
                    DataContext = null;
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "PdfTemplateRepository.Dispose");
                    throw;
                }
            }
        }

        ~PdfTemplateRepository()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "PdfTemplateRepository.Dispose");
                    throw;
                }
        }*/
    }
}
