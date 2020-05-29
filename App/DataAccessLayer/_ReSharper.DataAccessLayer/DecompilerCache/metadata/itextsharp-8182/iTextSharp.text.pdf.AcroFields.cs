// Type: iTextSharp.text.pdf.AcroFields
// Assembly: itextsharp, Version=4.1.6.0, Culture=neutral, PublicKeyToken=8354ae6d2174ddca
// Assembly location: C:\Program Files\Perpetuum Software\Net ModelKit Suite\Bin\itextsharp.dll

using System.Collections;
using System.IO;
using System.Xml;

namespace iTextSharp.text.pdf
{
    public class AcroFields
    {
        public const int DA_FONT = 0;
        public const int DA_SIZE = 1;
        public const int DA_COLOR = 2;
        public const int FIELD_TYPE_NONE = 0;
        public const int FIELD_TYPE_PUSHBUTTON = 1;
        public const int FIELD_TYPE_CHECKBOX = 2;
        public const int FIELD_TYPE_RADIOBUTTON = 3;
        public const int FIELD_TYPE_TEXT = 4;
        public const int FIELD_TYPE_LIST = 5;
        public const int FIELD_TYPE_COMBO = 6;
        public const int FIELD_TYPE_SIGNATURE = 7;
        public Hashtable Fields { get; }
        public bool GenerateAppearances { get; set; }
        public int TotalRevisions { get; }
        public Hashtable FieldCache { get; set; }
        public ArrayList SubstitutionFonts { get; set; }
        public XfaForm Xfa { get; }
        public string[] GetAppearanceStates(string fieldName);
        public string[] GetListOptionExport(string fieldName);
        public string[] GetListOptionDisplay(string fieldName);
        public bool SetListOption(string fieldName, string[] exportValues, string[] displayValues);
        public int GetFieldType(string fieldName);
        public void ExportAsFdf(FdfWriter writer);
        public bool RenameField(string oldName, string newName);
        public static object[] SplitDAelements(string da);
        public void DecodeGenericDictionary(PdfDictionary merged, BaseField tx);
        public string GetField(string name);
        public string[] GetListSelection(string name);
        public bool SetFieldProperty(string field, string name, object value, int[] inst);
        public bool SetFieldProperty(string field, string name, int value, int[] inst);
        public void MergeXfaData(XmlNode n);
        public void SetFields(FdfReader fdf);
        public void SetFields(XfdfReader xfdf);
        public bool RegenerateField(string name);
        public bool SetField(string name, string value);
        public bool SetField(string name, string value, string display);
        public bool SetListSelection(string name, string[] value);
        public AcroFields.Item GetFieldItem(string name);
        public string GetTranslatedFieldName(string name);
        public float[] GetFieldPositions(string name);
        public bool RemoveFieldsFromPage(int page);
        public bool RemoveField(string name, int page);
        public bool RemoveField(string name);
        public ArrayList GetSignatureNames();
        public ArrayList GetBlankSignatureNames();
        public PdfDictionary GetSignatureDictionary(string name);
        public bool SignatureCoversWholeDocument(string name);
        public PdfPKCS7 VerifySignature(string name);
        public int GetRevision(string field);
        public Stream ExtractRevision(string field);
        public void SetExtraMargin(float extraMarginLeft, float extraMarginTop);
        public void AddSubstitutionFont(BaseFont font);
        public PushbuttonField GetNewPushbuttonFromField(string field);
        public PushbuttonField GetNewPushbuttonFromField(string field, int order);
        public bool ReplacePushbuttonField(string field, PdfFormField button);
        public bool ReplacePushbuttonField(string field, PdfFormField button, int order);

        #region Nested type: Item

        public class Item
        {
            public const int WRITE_MERGED = 1;
            public const int WRITE_WIDGET = 2;
            public const int WRITE_VALUE = 4;
            public ArrayList merged;
            public ArrayList page;
            public ArrayList tabOrder;
            public ArrayList values;
            public ArrayList widget_refs;
            public ArrayList widgets;
            public int Size { get; }
            public void WriteToAll(PdfName key, PdfObject value, int writeFlags);
            public void MarkUsed(AcroFields parentFields, int writeFlags);
            public PdfDictionary GetValue(int idx);
            public PdfDictionary GetWidget(int idx);
            public PdfIndirectReference GetWidgetRef(int idx);
            public PdfDictionary GetMerged(int idx);
            public int GetPage(int idx);
            public int GetTabOrder(int idx);
        }

        #endregion

        #region Nested type: RevisionStream

        public class RevisionStream : Stream
        {
            public override bool CanRead { get; }
            public override bool CanSeek { get; }
            public override bool CanWrite { get; }
            public override long Length { get; }
            public override long Position { get; set; }
            public override int ReadByte();
            public override int Read(byte[] b, int off, int len);
            public override void Close();
            public override void Flush();
            public override long Seek(long offset, SeekOrigin origin);
            public override void SetLength(long value);
            public override void Write(byte[] buffer, int offset, int count);
        }

        #endregion
    }
}
