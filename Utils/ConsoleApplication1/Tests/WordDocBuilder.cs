using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Novacode;
using Word = Microsoft.Office.Interop.Word;

namespace ConsoleApplication1.Tests
{
    public class WordDocBuilder
    {
        public static void TestBuild()
        {
            object oMissing = Missing.Value;
            object oEndOfDoc = "\\endofdoc"; /* \endofdoc is a predefined bookmark */

            //Start Word and create a new document.
            Word._Application oWord = new Word.Application { Visible = false };
            Word._Document oDoc = oWord.Documents.Add(ref oMissing, ref oMissing,
                ref oMissing, ref oMissing);

            //Insert a paragraph at the beginning of the document.
            var oPara1 = oDoc.Content.Paragraphs.Add(ref oMissing);
            oPara1.Range.Text = "Heading 1";
            oPara1.Range.Font.Bold = 1;
            oPara1.Format.SpaceAfter = 24;    //24 pt spacing after paragraph.
            oPara1.Range.InsertParagraphAfter();

            //Insert a paragraph at the end of the document.
            object oRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            var oPara2 = oDoc.Content.Paragraphs.Add(ref oRng);
            oPara2.Range.Text = "Heading 2";
            oPara2.Format.SpaceAfter = 6;
            oPara2.Range.InsertParagraphAfter();

            //Insert another paragraph.
            oRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            var oPara3 = oDoc.Content.Paragraphs.Add(ref oRng);
            oPara3.Range.Text = "This is a sentence of normal text. Now here is a table:";
            oPara3.Range.Font.Bold = 0;
            oPara3.Format.SpaceAfter = 24;
            oPara3.Range.InsertParagraphAfter();

            //Insert a 3 x 5 table, fill it with data, and make the first row
            //bold and italic.
            Word.Range wrdRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            var oTable = oDoc.Tables.Add(wrdRng, 3, 5, ref oMissing, ref oMissing);
            oTable.Range.ParagraphFormat.SpaceAfter = 6;
            int r, c;
            string strText;
            for (r = 1; r <= 3; r++)
                for (c = 1; c <= 5; c++)
                {
                    strText = "r" + r + "c" + c;
                    oTable.Cell(r, c).Range.Text = strText;
                }
            oTable.Rows[1].Range.Font.Bold = 1;
            oTable.Rows[1].Range.Font.Italic = 1;

            //Add some text after the table.
            oRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            var oPara4 = oDoc.Content.Paragraphs.Add(ref oRng);
            oPara4.Range.InsertParagraphBefore();
            oPara4.Range.Text = "And here's another table:";
            oPara4.Format.SpaceAfter = 24;
            oPara4.Range.InsertParagraphAfter();

            //Insert a 5 x 2 table, fill it with data, and change the column widths.
            wrdRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            oTable = oDoc.Tables.Add(wrdRng, 5, 2, ref oMissing, ref oMissing);
            oTable.Range.ParagraphFormat.SpaceAfter = 6;
            for (r = 1; r <= 5; r++)
                for (c = 1; c <= 2; c++)
                {
                    strText = "r" + r + "c" + c;
                    oTable.Cell(r, c).Range.Text = strText;
                }
            oTable.Columns[1].Width = oWord.InchesToPoints(2); //Change width of columns 1 & 2
            oTable.Columns[2].Width = oWord.InchesToPoints(3);

            //Keep inserting text. When you get to 7 inches from top of the
            //document, insert a hard page break.
            object oPos;
            double dPos = oWord.InchesToPoints(7);
            oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range.InsertParagraphAfter();
            do
            {
                wrdRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
                wrdRng.ParagraphFormat.SpaceAfter = 6;
                wrdRng.InsertAfter("A line of text");
                wrdRng.InsertParagraphAfter();
                oPos = wrdRng.Information[Word.WdInformation.wdVerticalPositionRelativeToPage];
            }
            while (dPos >= Convert.ToDouble(oPos));
            object oCollapseEnd = Word.WdCollapseDirection.wdCollapseEnd;
            object oPageBreak = Word.WdBreakType.wdPageBreak;
            wrdRng.Collapse(ref oCollapseEnd);
            wrdRng.InsertBreak(ref oPageBreak);
            wrdRng.Collapse(ref oCollapseEnd);
            wrdRng.InsertAfter("We're now on page 2. Here's my chart:");
            wrdRng.InsertParagraphAfter();

            //Insert a chart.
            object oClassType = "MSGraph.Chart.8";
            wrdRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            var oShape = wrdRng.InlineShapes.AddOLEObject(ref oClassType, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing);

            //Demonstrate use of late bound oChart and oChartApp objects to
            //manipulate the chart object with MSGraph.
            object oChart = oShape.OLEFormat.Object;
            var oChartApp = oChart.GetType().InvokeMember("Application",
                BindingFlags.GetProperty, null, oChart, null);

            //Change the chart type to Line.
            var parameters = new Object[1];
            parameters[0] = 4; //xlLine = 4
            oChart.GetType().InvokeMember("ChartType", BindingFlags.SetProperty,
                null, oChart, parameters);

            //Update the chart image and quit MSGraph.
            oChartApp.GetType().InvokeMember("Update",
                BindingFlags.InvokeMethod, null, oChartApp, null);
            oChartApp.GetType().InvokeMember("Quit",
                BindingFlags.InvokeMethod, null, oChartApp, null);
            //... If desired, you can proceed from here using the Microsoft Graph 
            //Object model on the oChart and oChartApp objects to make additional
            //changes to the chart.

            //Set the width of the chart.
            oShape.Width = oWord.InchesToPoints(6.25f);
            oShape.Height = oWord.InchesToPoints(3.57f);

            //Add text after the chart.
            wrdRng = oDoc.Bookmarks.get_Item(ref oEndOfDoc).Range;
            wrdRng.InsertParagraphAfter();
            wrdRng.InsertAfter("THE END.");

            object fileName = @"C:\Distr\cissa\first_Word_Doc.doc";
            oDoc.SaveAs(ref fileName);
        }

        public static void TestDocXBuild()
        {
//            Assembly gAssembly = Assembly.GetExecutingAssembly();
            using (var stream = new MemoryStream())
            {
                DocX document = DocX.Create(stream); //(@"InvoiceTemplate.docx");
                Console.WriteLine(@"Width: {0}; Height: {1}", document.PageWidth, document.PageHeight);
                document.PageLayout.Orientation = Orientation.Landscape;
                Console.WriteLine(@"Width: {0}; Height: {1}", document.PageWidth, document.PageHeight);
                document.PageWidth = document.PageWidth / 2;
                document.PageHeight = document.PageHeight / 2;

                // Create a table for layout purposes (This table will be invisible).
                Table layoutTable = document.InsertTable(2, 2);
                layoutTable.Design = TableDesign.TableNormal;
                layoutTable.AutoFit = AutoFit.Window;

                // Dark formatting
                Formatting darkFormatting = new Formatting
                {
                    Bold = true,
                    Size = 12,
                    FontColor = Color.FromArgb(31, 73, 125)
                };

                // Light formatting
                Formatting lightFormatting = new Formatting
                {
                    Italic = true,
                    Size = 11,
                    FontColor = Color.FromArgb(79, 129, 189)
                };

                #region Company Name

                // Get the upper left Paragraph in the layout_table.
                Paragraph upperLeftParagraph = layoutTable.Rows[0].Cells[0].Paragraphs.First();

                // Create a custom property called company_name
                CustomProperty companyName = new CustomProperty("company_name", "Company Name");

                // Insert a field of type doc property (This will display the custom property 'company_name')
                layoutTable.Rows[0].Cells[0].Paragraphs.Last().InsertDocProperty(companyName, false, darkFormatting);

                // Force the next text insert to be on a new line.
                upperLeftParagraph.InsertText("\n", false);

                #endregion

                #region Company Slogan

                // Create a custom property called company_slogan
                CustomProperty companySlogan = new CustomProperty("company_slogan", "Company slogan goes here.");

                // Insert a field of type doc property (This will display the custom property 'company_slogan')
                upperLeftParagraph.InsertDocProperty(companySlogan, false, lightFormatting);

                #endregion

                #region Company Logo

                // Get the upper right Paragraph in the layout_table.
                Paragraph upperRightParagraph = layoutTable.Rows[0].Cells[1].Paragraphs.First();

                // Add a template logo image to this document.
                if (File.Exists(@"C:\Users\Администратор\Pictures\excel.png"))
                {
                    Novacode.Image logo = document.AddImage(@"C:\Users\Администратор\Pictures\excel.png");

                    // Insert this template logo into the upper right Paragraph.
                    Picture pictureLogo = logo.CreatePicture();
                    upperRightParagraph.InsertPicture(pictureLogo /*.Id, "", ""*/);
                }

                upperRightParagraph.Alignment = Alignment.right;

                #endregion

                // Custom properties cannot contain newlines, so the company address must be split into 3 custom properties.

                #region Hired Company Address

                // Create a custom property called company_address_line_one
                CustomProperty hiredCompanyAddressLineOne = new CustomProperty("hired_company_address_line_one",
                    "Street Address,");

                // Get the lower left Paragraph in the layout_table. 
                Paragraph lowerLeftParagraph = layoutTable.Rows[1].Cells[0].Paragraphs.Last();
                lowerLeftParagraph.InsertText("TO:\n", false, darkFormatting);

                // Insert a field of type doc property (This will display the custom property 'hired_company_address_line_one')
                lowerLeftParagraph.InsertDocProperty(hiredCompanyAddressLineOne, false, lightFormatting);

                // Force the next text insert to be on a new line.
                lowerLeftParagraph.InsertText("\n", false);

                // Create a custom property called company_address_line_two
                CustomProperty hiredCompanyAddressLineTwo = new CustomProperty("hired_company_address_line_two", "City,");

                // Insert a field of type doc property (This will display the custom property 'hired_company_address_line_two')
                lowerLeftParagraph.InsertDocProperty(hiredCompanyAddressLineTwo, false, lightFormatting);

                // Force the next text insert to be on a new line.
                lowerLeftParagraph.InsertText("\n", false);

                // Create a custom property called company_address_line_two
                CustomProperty hiredCompanyAddressLineThree = new CustomProperty("hired_company_address_line_three",
                    "Zip Code");

                // Insert a field of type doc property (This will display the custom property 'hired_company_address_line_three')
                lowerLeftParagraph.InsertDocProperty(hiredCompanyAddressLineThree, false, lightFormatting);

                #endregion

                #region Date & Invoice number

                // Get the lower right Paragraph from the layout table.
                Paragraph lowerRightParagraph = layoutTable.Rows[1].Cells[1].Paragraphs.First();

                CustomProperty invoiceDate = new CustomProperty("invoice_date", DateTime.Today.Date.ToString("d"));
                lowerRightParagraph.InsertText("Date: ", false, darkFormatting);
                lowerRightParagraph.InsertDocProperty(invoiceDate, false, lightFormatting);

                CustomProperty invoiceNumber = new CustomProperty("invoice_number", 1);
                lowerRightParagraph.InsertText("\nInvoice: ", false, darkFormatting);
                lowerRightParagraph.InsertText("#", false, lightFormatting);
                lowerRightParagraph.InsertDocProperty(invoiceNumber, false, lightFormatting);

                lowerRightParagraph.Alignment = Alignment.right;

                #endregion

                // Insert an empty Paragraph between two Tables, so that they do not touch.
                document.InsertParagraph(string.Empty, false);

                // This table will hold all of the invoice data.
                Table invoiceTable = document.InsertTable(4, 4);
                invoiceTable.Design = TableDesign.LightShadingAccent1;
                invoiceTable.Alignment = Alignment.center;

                // A nice thank you Paragraph.
                Paragraph thankyou =
                    document.InsertParagraph("\nThank you for your business, we hope to work with you again soon.",
                        false, darkFormatting);
                thankyou.Alignment = Alignment.center;

                #region Hired company details

                CustomProperty hiredCompanyDetailsLineOne = new CustomProperty("hired_company_details_line_one",
                    "Street Address, City, ZIP Code");
                CustomProperty hiredCompanyDetailsLineTwo = new CustomProperty("hired_company_details_line_two",
                    "Phone: 000-000-0000, Fax: 000-000-0000, e-mail: support@companyname.com");

                Paragraph companyDetails = document.InsertParagraph(string.Empty, false);
                companyDetails.InsertDocProperty(hiredCompanyDetailsLineOne, false, lightFormatting);
                companyDetails.InsertText("\n", false);
                companyDetails.InsertDocProperty(hiredCompanyDetailsLineTwo, false, lightFormatting);
                companyDetails.Alignment = Alignment.center;

                #endregion

                //document.SaveAs(@"c:\Distr\cissa\DocXTestExapmle.doc");
                document.Save();
                stream.Position = 0;
                using (var fileStream = new FileStream(@"c:\Distr\cissa\DocXTestExapmle1.doc", FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
    }
}
