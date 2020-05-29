using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.pdf;

namespace Intersoft.CISSA.PdfFiller
{
    public class DocumentBuilder : IDisposable
    {
        private readonly PdfStamper _pdfStamper;
        private readonly Stream _stream;
        private bool _disposed;
        private readonly BaseFont _bf;

        public DocumentBuilder(string templatePath, Stream stream)
        {
            var pdfReader = new PdfReader(templatePath);
            _stream = stream;
            _pdfStamper = new PdfStamper(pdfReader, _stream);
            _pdfStamper.Writer.CloseStream = false;
        }

        public DocumentBuilder(string templatePath, Stream stream, string baseFontPath)
        {
            _bf = BaseFont.CreateFont(baseFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            var pdfReader = new PdfReader(templatePath);
            _stream = stream;
            _pdfStamper = new PdfStamper(pdfReader, _stream);
            _pdfStamper.Writer.CloseStream = false;
            //            _pdfStamper.Writer.SetPdfVersion(PdfWriter.PDF_VERSION_1_5);
            //            _pdfStamper.Writer.CompressionLevel = PdfStream.BEST_COMPRESSION;
            //            _pdfStamper.SetFullCompression();
            //            _pdfStamper.Writer.SetFullCompression();
        }

        //public void test()
        //{
        //    var fields = _pdfStamper.AcroFields;

        //    PushbuttonField ad = fields.GetNewPushbuttonFromField("08");
        //    ad.Layout = PushbuttonField.LAYOUT_ICON_ONLY;
        //    ad.ProportionalIcon = true;
        //    //ad.Image = iTextSharp.text.Image.GetInstance(ValueType(;
        //    fields.ReplacePushbuttonField("08", ad.Field);

        //    //fields.ReplacePushbuttonField("", new PdfFormField());
        //}
        public void SetBlobField(string fieldName, object value)
        {
            var fields = _pdfStamper.AcroFields;

            MemoryStream ms = new MemoryStream((byte[])value);
            System.Drawing.Image img = new System.Drawing.Bitmap(ms);
            
            PushbuttonField ad = fields.GetNewPushbuttonFromField(fieldName);
            ad.Layout = PushbuttonField.LAYOUT_ICON_ONLY;
            ad.ProportionalIcon = true;
            ad.Image = iTextSharp.text.Image.GetInstance(img, System.Drawing.Imaging.ImageFormat.Jpeg);
            fields.ReplacePushbuttonField(fieldName, ad.Field);

        }

        public void SetField(string fieldName, object value)
        {
            var fields = _pdfStamper.AcroFields;

            if (_bf != null)
                fields.SetFieldProperty(fieldName, "textfont", _bf, null);
            fields.SetField(fieldName, value.ToString());
        }

        public bool ContainsField(string fieldName)
        {
            var fields = _pdfStamper.AcroFields;

            return fields != null && fields.Fields != null && fields.Fields.ContainsKey(fieldName);
        }

        public ICollection<string> GetFieldNames()
        {
            var fields = _pdfStamper.AcroFields;

            return fields.Fields.Keys;
        }

        public Stream Stream { get { return _stream; } }

        protected virtual void Dispose(bool disposing)
        {
            if(_disposed) return;

            try
            {
                if(!disposing) return;
                if(_pdfStamper != null) _pdfStamper.Dispose();
            }
            finally
            {
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
