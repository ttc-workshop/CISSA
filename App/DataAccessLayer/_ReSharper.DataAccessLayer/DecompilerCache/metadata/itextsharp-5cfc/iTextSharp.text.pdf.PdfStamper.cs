// Type: iTextSharp.text.pdf.PdfStamper
// Assembly: itextsharp, Version=5.1.1.0, Culture=neutral, PublicKeyToken=8354ae6d2174ddca
// Assembly location: C:\Users\Ruslan\Desktop\cissa-2\trunk\Lib\iText\itextsharp.dll

using iTextSharp.text;
using iTextSharp.text.pdf.collection;
using iTextSharp.text.pdf.interfaces;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.IO;

namespace iTextSharp.text.pdf
{
    public class PdfStamper : IPdfViewerPreferences, IPdfEncryptionSettings, IDisposable
    {
        protected PdfStamperImp stamper;
        public PdfStamper(PdfReader reader, Stream os);
        public PdfStamper(PdfReader reader, Stream os, char pdfVersion);
        public PdfStamper(PdfReader reader, Stream os, char pdfVersion, bool append);
        public IDictionary<string, string> MoreInfo { get; set; }
        public PdfSignatureAppearance SignatureAppearance { get; }
        public bool RotateContents { get; set; }
        public PdfWriter Writer { get; }
        public PdfReader Reader { get; }
        public AcroFields AcroFields { get; }
        public bool FormFlattening { set; }
        public bool FreeTextFlattening { set; }
        public IList<Dictionary<string, object>> Outlines { set; }
        public string JavaScript { set; }
        public byte[] XmpMetadata { set; }
        public bool FullCompression { get; }

        #region IDisposable Members

        public virtual void Dispose();

        #endregion

        #region IPdfEncryptionSettings Members

        public void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, int encryptionType);
        public void SetEncryption(X509Certificate[] certs, int[] permissions, int encryptionType);

        #endregion

        #region IPdfViewerPreferences Members

        public virtual void AddViewerPreference(PdfName key, PdfObject value);
        public virtual int ViewerPreferences { set; }

        #endregion

        public void ReplacePage(PdfReader r, int pageImported, int pageReplaced);
        public void InsertPage(int pageNumber, Rectangle mediabox);
        public void Close();
        public PdfContentByte GetUnderContent(int pageNum);
        public PdfContentByte GetOverContent(int pageNum);
        public void SetEncryption(byte[] userPassword, byte[] ownerPassword, int permissions, bool strength128Bits);
        public void SetEncryption(bool strength, string userPassword, string ownerPassword, int permissions);
        public void SetEncryption(int encryptionType, string userPassword, string ownerPassword, int permissions);
        public PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber);
        public void AddAnnotation(PdfAnnotation annot, int page);
        public PdfFormField AddSignature(string name, int page, float llx, float lly, float urx, float ury);
        public void AddComments(FdfReader fdf);
        public void SetThumbnail(Image image, int page);
        public bool PartialFormFlattening(string name);
        public void AddFileAttachment(string description, byte[] fileStore, string file, string fileDisplay);
        public void AddFileAttachment(string description, PdfFileSpecification fs);
        public void MakePackage(PdfName initialView);
        public void MakePackage(PdfCollection collection);
        public void SetFullCompression();
        public void SetPageAction(PdfName actionType, PdfAction action, int page);
        public void SetDuration(int seconds, int page);
        public void SetTransition(PdfTransition transition, int page);

        public static PdfStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion, string tempFile,
                                                 bool append);

        public static PdfStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion);
        public static PdfStamper CreateSignature(PdfReader reader, Stream os, char pdfVersion, string tempFile);
        public Dictionary<string, PdfLayer> GetPdfLayers();
    }
}
