using System;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using NPOI.HSSF.UserModel;
using NPOI.SS.Util;

namespace Intersoft.CISSA.DataAccessLayer.Model.Templates
{
    public class ExcelTemplateRepository : ITemplateReportGenerator
    {
        public IAppServiceProvider Provider { get; private set; }
        //public IDataContext DataContext { get; private set; }
        //private readonly bool _ownDataContext;
        public Guid UserId { get; private set; }
        public ExcelTemplateRepository(IAppServiceProvider provider, Guid userId)
        {
            Provider = provider;
            UserId = userId;
        }
/*

        public ExcelTemplateRepository(IDataContext dataContext, Guid userId)
        {
            if (dataContext == null)
            {
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else
                DataContext = dataContext;
            UserId = userId;
        }

        public ExcelTemplateRepository(Guid userId) : this(null, userId) {}
*/

        public Stream Generate(Doc document, string fileName)
        {
            using (var temp = new FileStream(fileName, FileMode.Open))
            {
                using (var wb = new HSSFWorkbook(temp))
                {
                    using (var dynaDoc = new DynaDoc(document, UserId, Provider))
                        FillDoc(wb, "", dynaDoc);

                    var stream = new MemoryStream();
                    wb.Write(stream);
                    return stream;
                }
            }
        }

        public Stream Generate(string fileName, IStringParams prms)
        {
            using (var temp = new FileStream(fileName, FileMode.Open))
            {
                using (var wb = new HSSFWorkbook(temp))
                {
                    FillDoc(wb, prms);

                    var stream = new MemoryStream();
                    wb.Write(stream);
                    return stream;
                }
            }
        }

        public Stream Generate(Doc doc, string fileName, IStringParams prms)
        {
            using (var temp = new FileStream(fileName, FileMode.Open))
            {
                using (var wb = new HSSFWorkbook(temp))
                {
                    using (var dynaDoc = new DynaDoc(doc, UserId, Provider))
                        FillDoc(wb, "", dynaDoc);
                    FillDoc(wb, prms);

                    var stream = new MemoryStream();
                    wb.Write(stream);
                    return stream;
                }
            }
        }

        private void FillDoc(HSSFWorkbook workbook, string prefix, DynaDoc doc)
        {
            var pref = String.IsNullOrEmpty(prefix) ? "" : prefix + ".";

            foreach (var attr in doc.Doc.Attributes)
            {
                var value = attr.ObjectValue;

                if (value == null) continue;

                if (attr is DocAttribute)
                {
                    var attrDoc = doc.GetAttrDoc((DocAttribute)attr);

                    if (attrDoc != null)
                        using (var dynaDoc = new DynaDoc(attrDoc, UserId, Provider))
                            FillDoc(workbook, pref + attr.AttrDef.Name, dynaDoc);
                }
                else if (attr is DocListAttribute)
                {
                    var index = 0;
                    foreach (var docItem in doc.GetAttrDocList((DocListAttribute)attr))
                    {
                        using (var dynaDoc = new DynaDoc(docItem, UserId, Provider))
                            FillDoc(workbook, string.Format("{0}{1}.{2}", pref, attr.AttrDef.Name, index), dynaDoc);
                        index++;
                    }
                }
                else if (attr is EnumAttribute)
                {
                    var enumValue = doc.GetAttrEnum((EnumAttribute)attr);

                    if (enumValue != null)
                        SetNamedRangeValue(workbook, pref + attr.AttrDef.Name, enumValue.Value);
                }
                else if (attr is DateTimeAttribute)
                    SetNamedRangeValue(workbook, pref + attr.AttrDef.Name, String.Format("{0:d}", value));
                else if (attr is CurrencyAttribute || attr is FloatAttribute)
                    SetNamedRangeValue(workbook, pref + attr.AttrDef.Name, String.Format("{0:F2}", value));
                else
                    SetNamedRangeValue(workbook, pref + attr.AttrDef.Name, value);
            }
        }

        public static bool IsNumeric(object value)
        {
            return value is Byte ||
                   value is Int16 ||
                   value is Int32 ||
                   value is Int64 ||
                   value is SByte ||
                   value is UInt16 ||
                   value is UInt32 ||
                   value is UInt64 ||
                   value is Decimal ||
                   value is Double ||
                   value is Single;
        }
        public static bool IsFloat(object value)
        {
            return (value is float || value is double || value is Decimal);
        }
        public static bool IsInteger(object value)
        {
            return (value is SByte ||
                    value is Int16 ||
                    value is Int32 ||
                    value is Int64 ||
                    value is Byte ||
                    value is UInt16 ||
                    value is UInt32 ||
                    value is UInt64);
        }

        private static void SetNamedRangeValue(HSSFWorkbook workbook, string name, object value)
        {
            if (value == null) return;

            var range = GetWorkbookName(workbook, name);

            if (range != null)
            {
                try
                {
                    var cellRef = new CellReference(range.RefersToFormula);
                    var sheet = workbook.GetSheet(cellRef.SheetName);
                    var row = sheet.GetRow(cellRef.Row);
                    var cell = row.GetCell(cellRef.Col);

                    double d;

                    if (IsInteger(value))
                    {
                        var i = Convert.ToInt32(value);
                        cell.SetCellValue(i);
                    }
                    else if (value is bool)
                    {
                        var b = Convert.ToBoolean(value);
                        cell.SetCellValue(b);
                    }
                    else if (IsFloat(value))
                    {
                        d = Convert.ToDouble(value);
                        cell.SetCellValue(d);
                    }
                    else if (value is decimal)
                    {
                        d = Convert.ToDouble(value);
                        cell.SetCellValue(d);
                    }
                    else if (value is DateTime)
                    {
                        var dt = Convert.ToDateTime(value);
                        cell.SetCellValue(dt);
                    }
                    else
                        cell.SetCellValue(value.ToString());
                }
                catch(Exception)
                {
                }
            }
        }

        private static NPOI.SS.UserModel.Name GetWorkbookName(HSSFWorkbook workbook, string name)
        {
            for (int i = 0; i < workbook.NumberOfNames; i++)
            {
                var namedRange = workbook.GetNameAt(i);

                if (String.Equals(name, namedRange.NameName, StringComparison.OrdinalIgnoreCase)) return namedRange;
            }
            return null;
        }

        private static void FillDoc(HSSFWorkbook workbook, IStringParams prms)
        {
            if (prms == null) return;

            for (int i = 0; i < workbook.NumberOfNames; i++)
            {
                var name = workbook.GetNameAt(i);
                var value = prms.Get(name.NameName);

                if (String.IsNullOrEmpty(value)) continue;

                try
                {
                    var cellRef = new CellReference(name.RefersToFormula);
                    var sheet = workbook.GetSheet(cellRef.SheetName);
                    var row = sheet.GetRow(cellRef.Row);
                    var cell = row.GetCell(cellRef.Col);

                    cell.SetCellValue(value);
                }
                catch(Exception)
                {
                }
            }
        }
/*
        public void Dispose()
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
                    Logger.OutputLog(e, "ExcelTemplateRepository.Dispose");
                    throw;
                }
            }
        }

        ~ExcelTemplateRepository()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "ExcelTemplateRepository.Finalize");
                    throw;
                }
        }*/
    }
}
