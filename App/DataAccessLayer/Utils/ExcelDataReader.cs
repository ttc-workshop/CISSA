using System;
using System.Data;
using System.IO;
using Excel;

namespace Intersoft.CISSA.DataAccessLayer.Utils
{
    public enum ExcelDataFormat
    {
        Binary,
        OpenXml
    }

    public class ExcelReaderCreator: IDisposable
    {
        public ExcelReaderCreator(Stream stream, ExcelDataFormat format)
        {
            _reader = format == ExcelDataFormat.OpenXml
                ? ExcelReaderFactory.CreateOpenXmlReader(stream)
                : ExcelReaderFactory.CreateBinaryReader(stream);
        }

        private readonly IExcelDataReader _reader;
        public IDataReader Reader { get { return _reader; } }
        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
