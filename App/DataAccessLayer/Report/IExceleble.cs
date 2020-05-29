using System;
using System.IO;

namespace Intersoft.CISSA.DataAccessLayer.Report
{
    public interface IExceleble
    {
        void ExcelSaveToFile(String path);
        MemoryStream ExcelGetMemoryStream();
        byte[] ExcelGetBytes();
    }
}