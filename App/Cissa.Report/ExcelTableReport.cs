using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Reports;
using Intersoft.CISSA.DataAccessLayer.Repository;
using NPOI.SS.UserModel;

namespace Intersoft.Cissa.Report
{
    public class ExcelTableReport : IExcelable
    {
        private readonly IExcelReport _report;
        private readonly IEnumRepository _enumRepository;

#if DEBUG
        public ExcelTableReport(IExcelReport report, IEnumRepository enumRepository)
        {
            _report = report;
            _enumRepository = enumRepository;
        }
#endif

        public ExcelTableReport(string reportName, IEnumerable<Doc> docs, BizTableReport reportData)
        {
            _report = new ExcelReport(reportName, "Empty", "Отчет");
            _enumRepository = new EnumRepository();

            CreateReport(reportName, docs, reportData);
        }

        private void CreateReport(string reportName, IEnumerable<Doc> docs, BizTableReport reportData)
        {
            // adding page header
            var pageHeaderString = reportData.PageHeaders.Aggregate("", (current, pageHeader) => current + pageHeader + "\n");
            _report.AddPageHeader(pageHeaderString, Position.Centre);

            // adding page footer
            var pageFooterString = reportData.PageFooters.Aggregate("", (current, pageFooter) => current + pageFooter + "\n");
            _report.AddPageFooter(pageFooterString, Position.Centre);

            // adding empty rows for logo 
            _report.AddEmptyRows(6);

            // report header
            foreach (string reportHeader in reportData.ReportHeaders)
            {
                var headerRow = _report.GetNextRow();
                _report.AddCell(reportHeader, 0, headerRow, TextStyle.Header2);
            }

            // Report table 
            Row row = _report.GetNextRow();
            _report.AddCell(reportName, 0, row, TextStyle.Header1);
            _report.SetRowHeight(row, 22);

            if (reportData.ReportDetails != null)
            {
                foreach (BizReportDetail repDetail in reportData.ReportDetails)
                {
                    int rowShift = _report.CurrentRowIndex + 2;
                    for (int rowIndex = 0; rowIndex < repDetail.ChildrenCountLevels; rowIndex++)
                    {
                        for (int columnIndex = 0; columnIndex < repDetail.LeafCount; columnIndex++)
                        {
                            _report.AddCell("", columnIndex, rowShift + rowIndex, TextStyle.TableHeaderGreyCenterdBorder);
                        }
                    }
                    WriteReportDetail(repDetail, rowShift - 1, repDetail.ChildrenCountLevels, 0);

                    foreach (Doc doc in docs)
                    {
                        Row tableRow = _report.GetNextRow();

                        int colIndex = 0;
                        foreach (ReportColumn col in GetColumns(repDetail))
                        {
                            Guid attrId = col.AttributeId;
                            IEnumerable<AttributeBase> findAttrQuery = from a in doc.Attributes
                                                                       where a.AttrDef.Id == attrId
                                                                       select a;
                            if (findAttrQuery.Any())
                            {
                                AttributeBase findedAttr = findAttrQuery.First();
                                if (findedAttr is EnumAttribute)
                                {
                                    var enumAttribute = findedAttr as EnumAttribute;
                                    if (enumAttribute.Value.HasValue)
                                    {
                                        string enumValue =
                                            _enumRepository.GetEnumValue(enumAttribute.AttrDef.EnumDefType.Id,
                                                                         enumAttribute.Value.Value);
                                        _report.AddCell(enumValue, colIndex, tableRow, TextStyle.TableRowNormal);
                                    }
                                }
                                else
                                {
                                    _report.AddCell(findedAttr.ObjectValue, colIndex, tableRow, TextStyle.TableRowNormal);
                                }
                            }
                            else
                            {
                                _report.AddCell("", colIndex, tableRow, TextStyle.TableRowNormal);
                            }
                            colIndex++;
                        }
                    }

                    Row aggregatesRow = _report.GetNextRow();
                    int aggrColIndex = 0;
                    foreach (ReportColumn col in GetColumns(repDetail))
                    {
                        /*  atr.AttrDef.Type.Id
                            1	Int
                            2	Currency
                            3	Text
                            4	Float
                            5	Enum
                            6	Doc
                            7	DocList
                            8	Bool
                            9	DateTime
                         */
                        Guid attrId = col.AttributeId;
                        IEnumerable<AttributeBase> attrQuery = from atr in docs.SelectMany(d => d.Attributes)
                                                               where atr.AttrDef.Id == attrId
                                                               select atr;

                        object aggregateValue;
                        switch (col.AggregateOperation)
                        {
                            case AggregateOperation.Count:
                                aggregateValue = attrQuery.Count();
                                break;

                            case AggregateOperation.Avg:
                                var q1 = attrQuery.Where(atr => (new[] { 1, 2, 4 }).Contains(atr.AttrDef.Type.Id));
                                aggregateValue = q1.Any() ? q1.Average(a => double.Parse((a.ObjectValue ?? 0).ToString())) : 0;
                                break;

                            case AggregateOperation.Sum:
                                var q2 = attrQuery.Where(atr => (new[] { 1, 2, 4 }).Contains(atr.AttrDef.Type.Id));
                                aggregateValue = q2.Any() ? q2.Sum(a => double.Parse((a.ObjectValue ?? 0).ToString())) : 0;
                                break;

                            case AggregateOperation.Max:
                                aggregateValue =
                                    attrQuery.Where(atr => (new[] { 1, 2, 4, 9 }).Contains(atr.AttrDef.Type.Id)).Max(
                                        a => a.ObjectValue);
                                break;

                            case AggregateOperation.Min:
                                aggregateValue =
                                    attrQuery.Where(atr => (new[] { 1, 2, 4, 9 }).Contains(atr.AttrDef.Type.Id)).Min(
                                        a => a.ObjectValue);
                                break;

                            default:
                                aggregateValue = "";
                                break;
                        }

                        _report.AddCell(aggrColIndex == 0 ? "Итог" : aggregateValue,
                            aggrColIndex, aggregatesRow, TextStyle.TableTotal);

                        aggrColIndex++;
                    }
                }
            }

            _report.AddEmptyRow();
            // report footer
            foreach (string reportFooter in reportData.ReportFooters)
            {
                var headerRow = _report.GetNextRow();
                _report.AddCell(reportFooter, 0, headerRow, TextStyle.Header2);
            }

            // report logo
            // adding logo after all other operations, because it can be resized.
            _report.AddLogo(0, 0);

        }

        private static IEnumerable<ReportColumn> GetColumns(IDetailItem repDetail)
        {
            var query = repDetail.Children.Where(col => col.IsColumn).Union(
                repDetail.Children.Where(col => col.IsColumn == false).SelectMany(GetColumns));

            return query.Select(det=>(ReportColumn)(det));
        }

        private void WriteReportDetail(IDetailItem repDetail, int level, int maxLevel, int columnNumber)
        {
            if (!repDetail.IsRoot)
            {
                _report.AddCell(repDetail.Text, columnNumber, level, TextStyle.TableHeaderGreyCenterdBorder);
            }

            if (repDetail.IsColumn)
            {
                _report.MergeRegion(level, level + maxLevel, columnNumber, columnNumber );
                _report.SetColumnWidth(columnNumber, ((ReportColumn)(repDetail)).Width);
            }
            else
            {
                if (!repDetail.IsRoot)
                {
                    _report.MergeRegion(level, level, columnNumber, repDetail.LeafCount + columnNumber - 1);
                }

                int prevLeavCount = 0;
                foreach (IDetailItem child in repDetail.Children)
                {
                    WriteReportDetail(child, level + 1, repDetail.ChildrenCountLevels -1 , columnNumber + prevLeavCount);
                    prevLeavCount += child.LeafCount;
                }
            }
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