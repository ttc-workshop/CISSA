using System;
using System.IO;
using System.Text;
using Microsoft.VisualBasic.FileIO;

namespace Intersoft.CISSA.DataAccessLayer.Utils
{
    public class CsvReader: IDisposable
    {
        public string FileName { get; private set; }
        public Stream Stream { get; private set; }
        public Encoding DefaultEncoding { get; private set; }

        private string[] _delimiters = {","};
        public string[] Delimiters 
        { 
            get { return _delimiters; } 
            set { _delimiters = value; }
        }

        public CsvReader(string fileName)
        {
            FileName = fileName;
            EndOfFile = false;
        }

        public CsvReader(Stream stream, Encoding encoding)
        {
            Stream = stream;
            DefaultEncoding = encoding;
            EndOfFile = false;
        }

        public CsvReader(Stream stream) : this(stream, Encoding.UTF8) { }

        public string[] Fields { get; internal set; }
        public bool EndOfFile { get; private set; }

        private TextFieldParser _parser;
        public bool Read()
        {
            if (!EndOfFile)
            {
                if (_parser == null)
                {
                    _parser = Stream == null
                        ? new TextFieldParser(FileName) {TextFieldType = FieldType.Delimited}
                        : new TextFieldParser(Stream, DefaultEncoding) {TextFieldType = FieldType.Delimited};

                    _parser.SetDelimiters(Delimiters);
                }
                if (!_parser.EndOfData)
                {
                    Fields = _parser.ReadFields();
                    return true;
                }
                _parser.Close();
                _parser.Dispose();
                _parser = null;
                EndOfFile = true;
            }
            return false;
        }

        public void Reset()
        {
            EndOfFile = false;
            if (Stream != null) Stream.Position = 0;
        }

        public void Dispose()
        {
            if (_parser != null)
            {
                _parser.Dispose();
                _parser = null;
            }
        }
    }
}
