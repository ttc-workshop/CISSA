using System.Collections.Generic;
using Intersoft.Cissa.Report;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;

namespace Cissa.Tests.ReportTest
{
    [TestClass]
    public class ExcelReportSimpeTests
    {
        [TestMethod]
        public void TestManyData()
        {
            var excelReport = new ExcelReport("Test many data", "Unknown", "sheet 1");
            
            excelReport.AddEmptyRows(5);
            excelReport.AddLogo(0, 0);

            for(int rowIndex=1; rowIndex<30000; rowIndex++)
            {
                for(int columnIndex=1; columnIndex<20; columnIndex++)
                {
                    excelReport.AddCell(rowIndex * columnIndex, columnIndex, rowIndex, TextStyle.NormalText);
                }
            }

            excelReport.SaveToExcelFile(@"c:\\TestManyData.xls");
        }

        [TestMethod]
        public void TestManyDataWithBorder()
        {
            var excelReport = new ExcelReport("Test many data with border", "Unknown", "sheet 1");

            excelReport.AddEmptyRows(5);
            excelReport.AddLogo(0, 0);

            for (int rowIndex = 1; rowIndex < 30000; rowIndex++)
            {
                var row = excelReport.GetNextRow();
                for (int columnIndex = 1; columnIndex < 20; columnIndex++)
                {
                    excelReport.AddCell(rowIndex * columnIndex, columnIndex, row,  TextStyle.NormalText, HSSFColor.WHITE.index, CellBorderType.THIN, HSSFColor.BLACK.index );
                }
            }

            excelReport.SaveToExcelFile(@"c:\\TestManyDataWithBorder.xls");
        }

        [TestMethod]
        public void TestAddList()
        {
            var excelReport = new ExcelReport("TestAddList", "Unknown", "sheet 1");

            excelReport.AddEmptyRows(5);
            

            var pesonList = new List<TestPerson>(); 
            for (int rowIndex = 1; rowIndex < 30000; rowIndex++)
            {
                var person = new TestPerson
                                 {
                                     Age = rowIndex,
                                     FirstName = "Ivan",
                                     LastName = "Ivanov"
                                 };

                pesonList.Add(person);
            }

            excelReport.AddList(pesonList);
            
            excelReport.AddLogo(0, 0);

            excelReport.SaveToExcelFile(@"c:\\PesonList.xls");
        }

        [TestMethod]
        public void TestAddListWitCustomColums()
        {
            var excelReport = new ExcelReport("TestAddListWitCustomColums", "Unknown", "sheet 1");

            excelReport.AddEmptyRows(5);

            var pesonList = new List<TestPerson>();
            for (int rowIndex = 1; rowIndex < 30000; rowIndex++)
            {
                var person = new TestPerson
                                 {
                                     Age = rowIndex % 100,
                                     FirstName = "Ivan",
                                     LastName = "Ivanov"
                                 };

                pesonList.Add(person);
            }

            var columns = new List<DataColumn> { 
                                                   DataColumn.GetRowNumberColumn("№"),
                                                   new DataColumn{ FieldAlias = "Фамилия", FieldName = "LastName", ExcelColumWidth = 20},
                                                   new DataColumn{ FieldAlias = "Имя", FieldName = "FirstName", ExcelColumWidth = 20},
                                                   new DataColumn{ FieldAlias = "Возраст", FieldName = "Age", ExcelColumWidth = 20}
                                               };

            excelReport.AddList(pesonList, columns);

            excelReport.AddLogo(0, 0);

            excelReport.SaveToExcelFile(@"c:\\PesonListWitCustomColums.xls");
        }


        [TestMethod]
        public void TestCellAdd2()
        {
            var excelReport = new ExcelReport("Test many data", "Unknown", "sheet 1");

            excelReport.AddEmptyRows(5);
            excelReport.AddLogo(0, 0);

            for (int rowIndex = 1; rowIndex < 30000; rowIndex++)
            {
                //var row = excelReport.GetNextRow();
                for (int columnIndex = 1; columnIndex < 20; columnIndex++)
                {
                    //excelReport.AddCell(rowIndex * columnIndex, columnIndex, row, TextStyle.NormalText);
                    excelReport.AddCell("test " + columnIndex + rowIndex, columnIndex, rowIndex, TextStyle.NormalText, HSSFColor.WHITE.index, CellBorderType.NONE, HSSFColor.WHITE.index);
                } 
            }

            excelReport.SaveToExcelFile(@"c:\\TestCellAdd2.xls");
        }
    }
}