using System;
using System.IO;

namespace Intersoft.Cissa.Report
{
    public interface IExcelable
    {
        void SaveToExcelFile(String path);
        MemoryStream GetExcelMemoryStream();
        byte[] GetExcelBytes();
    }
}