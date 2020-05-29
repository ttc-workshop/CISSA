using System.Collections.Generic;
using System.IO;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Repository;
using NPOI.SS.UserModel;

namespace Intersoft.Cissa.Report
{
    public class ExcelDocListReport : IExcelable
    {
        private readonly IExcelReport _report;
        private readonly IEnumRepository _enumRepository;

#if DEBUG
        public ExcelDocListReport(IExcelReport report, IEnumRepository enumRepository)
        {
            _report = report;
            _enumRepository = enumRepository;
        }
#endif

        public ExcelDocListReport(string reportName, IEnumerable<Doc> docs, Doc docTemplate)
        {
            _report = new ExcelReport(reportName, "Empty", "Отчет");
            _enumRepository = new EnumRepository();

            CreateReport(reportName, docs, docTemplate);
        }

        private void CreateReport(string reportName, IEnumerable<Doc> docs, Doc docTemplate)
        {
            _report.AddEmptyRows(6);

            Row row = _report.GetNextRow();
            _report.AddCell(reportName, 0, row, TextStyle.Header1);
            _report.SetRowHeight(row, 22);

            if (docs == null) return;
            var enumerable = docs as Doc[] ?? docs.ToArray();
            if (!enumerable.Any()) return;

            Row rowHeader = _report.GetNextRow();
            int columnIndex = 0;
            foreach (AttributeBase attribute in docTemplate.Attributes)
            {
                AttributeBase attribute1 = attribute;

                _report.AddCell(attribute1.AttrDef.Name, columnIndex, rowHeader, TextStyle.TableHeaderGreyCenterdBorder);

                int colWidth = enumerable.SelectMany(d => d.Attributes)
                    .Where(a => a.AttrDef.Id == attribute1.AttrDef.Id && a.ObjectValue != null)
                    .Select(aa => aa.ObjectValue.ToString().Length).Max();

                _report.SetColumnWidth(columnIndex, colWidth);

                columnIndex++;
            }

            foreach (Doc doc in enumerable)
            {
                var tableRow = _report.GetNextRow();
                
                var colIndex = 0;
                foreach (var attr in docTemplate.Attributes)
                {
                    var attr1 = attr;
                    var findAttrQuery = (from a in doc.Attributes
                        where a.AttrDef.Id == attr1.AttrDef.Id
                        select a).ToList();

                    if (findAttrQuery.Any())
                    {
                        var findedAttr = findAttrQuery.First();
                        if (findedAttr is EnumAttribute)
                        {
                            var enumAttribute = findedAttr as EnumAttribute;
                            if (enumAttribute.Value.HasValue)
                            {
                                string enumValue = _enumRepository.GetEnumValue(enumAttribute.AttrDef.EnumDefType.Id,
                                                                                enumAttribute.Value.Value);
                                _report.AddCell(enumValue, colIndex, tableRow, TextStyle.NormalText);
                            }
                        }
                        else
                        {
                            _report.AddCell(findedAttr.ObjectValue, colIndex, tableRow, TextStyle.NormalText);
                        }
                    }
                    colIndex++;
                }
            }

            _report.AddLogo(0, 0);
        }

        public void SaveToExcelFile(string path)
        {
            _report.SaveToExcelFile(path);
        }

        public MemoryStream GetExcelMemoryStream()
        {
            return _report.GetExcelMemoryStream();
        }

        public byte[] GetExcelBytes()
        {
            return _report.GetExcelBytes();
        }
    }
}