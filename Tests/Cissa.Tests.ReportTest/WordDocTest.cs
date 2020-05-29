using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using Intersoft.Cissa.Report.Builders;
using Intersoft.Cissa.Report.Common;
using Intersoft.Cissa.Report.Styles;
using Intersoft.Cissa.Report.WordDoc;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Builders;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Word = Microsoft.Office.Interop.Word;
using Intersoft.CISSA.DataAccessLayer.Storage;

namespace Cissa.Tests.ReportTest
{
    [TestClass]
    public class WordDocTest
    {
        [TestMethod]
        public void BuildWordWithInteropTest()
        {
            object oMissing = Missing.Value;
            object oEndOfDoc = "\\endofdoc"; /* \endofdoc is a predefined bookmark */

            //Start Word and create a new document.
            Word._Application oWord = new Word.Application {Visible = false};
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

        public const string ChatkalConnectionString =
            "Data Source=localhost;Initial Catalog=cissa-4atkal;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        public const string AsistConnectionString =
            "Data Source=localhost;Initial Catalog=asist_db2;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";
        public const string NrszConnectionString =
            "Data Source=localhost;Initial Catalog=nrsz_db;Password=QQQwww123;Persist Security Info=True;User ID=sa;MultipleActiveResultSets=True";


        [TestMethod]
        public void BuildDocFromXlsDef()
        {
            using (var connection = new SqlConnection(ChatkalConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var formRepo = provider.Get<IFormRepository>(); // new FormRepository(dataContext);
                        var form = formRepo.GetDetailForm(Guid.Parse("{90958557-E6B0-40A8-88D8-75B71130D5FC}"));

                        var docRepo = provider.Get<IDocRepository>(); // new DocRepository(dataContext);
                        var doc = docRepo.LoadById(Guid.Parse("12113F50-1DAC-400E-941B-012D0A1CCC29"));

                        var builderFactory = provider.Get<IXlsFormDefBuilderFactory>();
                        var defBuilder = builderFactory.Create(form);
                            // new XlsFormDefBuilder(dataContext, form, Guid.Empty);
                        var def = defBuilder.Build(doc);
                        def.Style.Borders = TableCellBorder.All;

                        var builder = new MsWordDocBuilder(dataContext);
                        using (var stream = new FileStream(@"c:\distr\cissa\testXlsDefAppForm.doc", FileMode.Create))
                        {
                            builder.BuildFromXlsDef(def, stream);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void BuildDocFromXlsDef2()
        {
            using (var connection = new SqlConnection(ChatkalConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var formRepo = provider.Get<IFormRepository>(); // new FormRepository(dataContext);
                        var form = formRepo.GetDetailForm(Guid.Parse("{90958557-E6B0-40A8-88D8-75B71130D5FC}"));

                        var docRepo = provider.Get<IDocRepository>(); // new DocRepository(dataContext);
                        var doc = docRepo.LoadById(Guid.Parse("12113F50-1DAC-400E-941B-012D0A1CCC29"));

                        var builderFactory = provider.Get<IXlsFormDefBuilderFactory>();
                        var defBuilder = builderFactory.Create(form);
                            // new XlsFormDefBuilder(dataContext, form, Guid.Empty);
                        var def = defBuilder.Build(doc);
                        def.Style.Borders = TableCellBorder.All;

                        var wordDef = new WordDocDef();
                        var builder = new XlsDefToWordDefConverter(def, wordDef);
                        using (var stream = new FileStream(@"c:\distr\cissa\testXlsDefAppForm2.doc", FileMode.Create))
                        {
                            var wordBuilder = new WordDocBuilder(wordDef);
                            wordBuilder.Build(stream);
                        }
                    }
                }
            }
        }

        public static readonly Guid PaymentDefId = new Guid("{68667FBB-C149-4FB3-93AD-1BBCE3936B6E}");
        public static readonly Guid BankAccountDefId = new Guid("{BE6D5C1F-48A6-483B-980A-14CEFF781FD4}");
        public static readonly Guid AssignmentDefId = new Guid("{51935CC6-CC48-4DAC-8853-DA8F57C057E8}");
        public static readonly Guid AppDefId = new Guid("{4F9F2AE2-7180-4850-A3F4-5FB47313BCC0}");
        public static readonly Guid PersonDefId = new Guid("{6052978A-1ECB-4F96-A16B-93548936AFC0}");
        public static readonly Guid AppStateDefId = new Guid("{547BBA55-2281-4388-A1FC-EE890168AC2D}");
        public static readonly Guid DistrictDefId = new Guid("{4D029337-C025-442E-8E93-AFD1852073AC}");
        public static readonly Guid DjamoatDefId = new Guid("{967D525D-9B76-44BE-93FA-BD4639EA515A}");
        public static readonly Guid VillageDefId = new Guid("{B70BAD68-7532-471F-A705-364FD5F1BA9E}");
        private static readonly Guid AssignmentNoteDefId = new Guid("{C9DAC468-625A-4609-A92A-F0006F270928}");


        [TestMethod]
        public void TestXmlWordDocDefBuilder()
        {
            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var qb = new QueryBuilder(AssignmentNoteDefId);
                        //qb.Where("Registry").Eq(sheet.Id);
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var query = sqlQueryBuilder.Build(qb.Def);

                        var bankAccountSrc = query.JoinSource(query.Source, BankAccountDefId, SqlSourceJoinType.Inner,
                            "Bank_Account");
                        var appSrc = query.JoinSource(bankAccountSrc, AppDefId, SqlSourceJoinType.Inner, "Application");
                        var personSrc = query.JoinSource(appSrc, PersonDefId, SqlSourceJoinType.Inner, "Person");
                        var appStateSrc = query.JoinSource(appSrc, AppStateDefId, SqlSourceJoinType.Inner,
                            "Application_State");
                        var districtSrc = query.JoinSource(appStateSrc, DistrictDefId, SqlSourceJoinType.LeftOuter,
                            "DistrictId");
                        var villageSrc = query.JoinSource(appStateSrc, VillageDefId, SqlSourceJoinType.LeftOuter,
                            "VillageId");
                        query.AddAttribute(personSrc, "IIN");
                        query.AddAttribute(bankAccountSrc, "Account_No");
                        query.AddAttribute(bankAccountSrc, "Application");
                        query.AddAttribute(personSrc, "Last_Name");
                        query.AddAttribute(personSrc, "First_Name");
                        query.AddAttribute(personSrc, "Middle_Name");
                        query.AddAttribute(personSrc, "PassportSeries");
                        query.AddAttribute(personSrc, "PassportNo");
                        query.AddAttribute("&Id");
                        query.AddAttribute(appSrc, "ApplicationDate");
                        query.AddAttribute(personSrc, "PassportSeries");
                        query.AddAttribute(personSrc, "PassportNo");
                        query.AddAttribute(appStateSrc, "Street");
                        query.AddAttribute(appStateSrc, "House");
                        query.AddAttribute(appStateSrc, "Flat");
                        query.AddAttribute(query.Source, "&Id");
                        query.AddAttribute(villageSrc, "Name").Alias = "VillageName";
                        query.AddAttribute(districtSrc, "Name").Alias = "DistrictName";


                        qb = new QueryBuilder(AssignmentDefId);
                        qb.Where("Application").Eq(Guid.Empty, "appId");

                        var assignQuery = sqlQueryBuilder.Build(qb.Def);
                        assignQuery.AddAttribute("Month");
                        assignQuery.AddAttribute("Year");

                        using (var r = new SqlQueryReader(dataContext, query))
                        {
                            using (var assignments = new SqlQueryReader(dataContext, assignQuery))
                            {
                                assignments.AddMasterLink(r, "Application", "appId");

                                using (var stream =
                                    new FileStream(@"C:\CissaFiles\xml\BuildBenefitFailureNotification.xml"/*"c:\distr\cissa\BuildWordDocReportTest2.xml"*/, FileMode.Open))
                                {
                                    var builder = new XmlWordDocDefBuilder(stream);
                                    builder.AddDataSet("notifications", new SqlQueryDataSet(provider, r));
                                    builder.AddDataSet("assignments", new SqlQueryDataSet(provider, assignments));

                                    var wdDef = builder.Build();

                                    var b = new WordDocBuilder(wdDef);
                                    using (
                                        var wordDocStream = new FileStream(@"c:\distr\cissa\wordDocFromXmlTest2.doc",
                                            FileMode.Create))
                                        b.Build(wordDocStream);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static readonly Guid BankDefId = new Guid("{B722BED0-562E-4872-8DD7-ACC31A0C1E12}");
        private static readonly Guid BankFilialDefId = new Guid("{92A20311-450A-4CC7-B176-9B0103D4DC18}");

        [TestMethod]
        public void PrintAsistAssignNotifications()
        {
            var fd = new DateTime(2015, 1, 1); //DateTime);period["DateFrom"];
            var ld = new DateTime(2015, 12, 31); //DateTime);period["DateTo"];
            var districtId = new Guid("592d7513-edb6-4f7d-8f05-d71fdb16c041");

            using (var connection = new SqlConnection(AsistConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        // Формирование запроса
                        var qb = new QueryBuilder(AssignmentNoteDefId);
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var bankAccountSrc = query.JoinSource(query.Source, BankAccountDefId,
                                SqlSourceJoinType.Inner, "Bank_Account");
                            var appSrc = query.JoinSource(bankAccountSrc, AppDefId, SqlSourceJoinType.Inner,
                                "Application");
                            var personSrc = query.JoinSource(appSrc, PersonDefId, SqlSourceJoinType.Inner, "Person");
                            var appStateSrc = query.JoinSource(appSrc, AppStateDefId, SqlSourceJoinType.Inner,
                                "Application_State");
                            var districtSrc = query.JoinSource(appStateSrc, DistrictDefId, SqlSourceJoinType.LeftOuter,
                                "DistrictId");
                            var djamoatSrc = query.JoinSource(appStateSrc, DjamoatDefId, SqlSourceJoinType.LeftOuter,
                                "DjamoatId");
                            var villageSrc = query.JoinSource(appStateSrc, VillageDefId, SqlSourceJoinType.LeftOuter,
                                "VillageId");
                            var bankInfoSrc = query.JoinSource(districtSrc, BankFilialDefId, SqlSourceJoinType.LeftOuter,
                                "District");
                            var bankNameSrc = query.JoinSource(bankInfoSrc, BankDefId, SqlSourceJoinType.LeftOuter,
                                "Bank");

                            //query.AndCondition(query.Source, "&State", ConditionOperation.Equal, newStateId);
                            query.AndCondition(appSrc, "ApplicationDate", ConditionOperation.GreatEqual, fd);
                            query.AndCondition(appSrc, "ApplicationDate", ConditionOperation.LessEqual, ld);
                            /*if (districtId != Guid.Empty)
                                query.AndCondition(appStateSrc, "DistrictId", ConditionOperation.Equal, districtId);*/
                            query.AddAttribute(appSrc, "ApplicationDate");
                            query.AddAttribute(appSrc, "No");
                            query.AddAttribute(personSrc, "IIN");
                            query.AddAttribute(bankAccountSrc, "Account_No");
                            query.AddAttribute(bankAccountSrc, "Application");
                            query.AddAttribute(personSrc, "Last_Name");
                            query.AddAttribute(personSrc, "First_Name");
                            query.AddAttribute(personSrc, "Middle_Name");
                            query.AddAttribute(personSrc, "PassportSeries");
                            query.AddAttribute(personSrc, "PassportNo");
                            query.AddAttribute(appStateSrc, "Street");
                            query.AddAttribute(appStateSrc, "House");
                            query.AddAttribute(appStateSrc, "Flat");
                            query.AddAttribute(query.Source, "&Id");
                            query.AddAttribute(bankNameSrc, "Name");
                            query.AddAttribute(bankInfoSrc, "No.Branch");
                            query.AddAttribute(bankInfoSrc, "Address");
                            query.AddAttribute(villageSrc, "Name").Alias = "VillageName";
                            query.AddAttribute(districtSrc, "Name").Alias = "DistrictName";
                            query.AddAttribute(djamoatSrc, "Name").Alias = "DjamoatName";

                            var assignqb = new QueryBuilder(AssignmentDefId);
                            assignqb.Where("Application").Eq(Guid.Empty, "appId");

                            var assignQuery = sqlQueryBuilder.Build(assignqb.Def);
                            assignQuery.AddAttribute("Month");
                            assignQuery.AddAttribute("Year");

                            using (var notifications = new SqlQueryReader(provider, query))
                            {
                                using (var assignments = new SqlQueryReader(provider, assignQuery))
                                {
                                    assignments.AddMasterLink(notifications, "Application", "appId");
                                    // Создание потока из XML файла из которого формируется Word документ
                                    using (
                                        var xmlStream =
                                            new FileStream(@"c:\cissaFiles\xml\BuildBenefitAssignNotifications.xml",
                                                FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        //@"c:\distr\cissa\BuildWordDocReportTest.xml"
                                        // Создатель образа Word документа из XML
                                        var xmlBuilder = new XmlWordDocDefBuilder(xmlStream);
                                        // Подключение запроса
                                        xmlBuilder.AddDataSet("notifications", new SqlQueryDataSet(provider, notifications));
                                        xmlBuilder.AddDataSet("assignments", new SqlQueryDataSet(provider, assignments));
                                        xmlBuilder.AddFunction("MMMM", GetTjMonthNames);
                                        // Формирование образа Word документа
                                        var wordDef = xmlBuilder.Build();
                                        // Создатель Word документа
                                        var wordBuilder = new WordDocBuilder(wordDef);
                                        // Создание потока куда будет записан Word документ
                                        using (
                                            var wordDocStream =
                                                new FileStream(
                                                    @"c:\cissaFiles\asistPrintFiles\BenefitAssignNotification.doc",
                                                    FileMode.Create))
                                            // создание документа и сохранение в файл
                                            wordBuilder.Build(wordDocStream);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void NrszReport3Test()
        {
            var docId = new Guid("ab5e0193-97ff-4644-82cd-bb9ba8236dee");

            using (var connection = new SqlConnection(NrszConnectionString))
            {
                using (var dataContext = new DataContext(connection))
                {
                    using (var provider = GetProvider(dataContext))
                    {
                        var docRepo = provider.Get<IDocRepository>();
                        var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
                        var letter = docRepo.LoadById(docId);
                        //Формирование запроса*
                        var qb = new QueryBuilder(AssignedServiceDefId);
                        qb.Where("&Id").Eq(letter.Id);

                        using (var query = sqlQueryBuilder.Build(qb.Def))
                        {
                            var personSrc = query.JoinSource(query.Source, PersonDefId, SqlSourceJoinType.Inner, "Person");

                            query.AddAttribute(personSrc, "Last_Name");
                            query.AddAttribute(personSrc, "First_Name");
                            query.AddAttribute(personSrc, "Middle_Name");
                            query.AddAttribute(personSrc, "IIN");
                            query.AddAttribute(personSrc, "Date_of_Birth");
                            query.AddAttribute(personSrc, "PassportSeries");
                            query.AddAttribute(personSrc, "PassportNo");
                            query.AddAttribute(query.Source, "ServiceType");
                            query.AddAttribute(query.Source, "DateFrom");
                            query.AddAttribute(query.Source, "DateTo");
                            query.AddAttribute(query.Source, "Amount");
                            /*query.AddAttribute(appStateSrc, "Street");
                            query.AddAttribute(appStateSrc, "House");
                            query.AddAttribute(appStateSrc, "Flat");*/
                            //query.AddAttribute(query.Source, "&Id");

                            using (var reader = new SqlQueryReader(dataContext, query))
                            {
                                //Создание потока из XML файла из которого формируется Word документ
                                using (var xmlStream = new FileStream(@"c:\cissaFiles\xml\BuildBenefitPersonalInformation.xml", FileMode.Open))
                                {
                                    //Создатель образа Word документа из XML
                                    var xmlBuilder = new XmlWordDocDefBuilder(xmlStream);
                                    //Подключение запроса
                                    xmlBuilder.AddDataSet("query", new SqlQueryDataSet(provider, reader));
                                    //Формирование образа Word документа
                                    var wordDef = xmlBuilder.Build();
                                    //Создатель Word документа
                                    var wordBuilder = new WordDocBuilder(wordDef);
                                    //Создание потока куда будет записан Word документ
                                    using (var wordDocStream = new FileStream(@"c:\cissaFiles\asistPrintFiles\BenefitPersonalInformation.doc", FileMode.Create))
                                        //создание документа и сохранение в файл
                                        wordBuilder.Build(wordDocStream);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static readonly Guid AssignedServiceDefId = new Guid("{A16EE2A1-CFDF-4B7A-8A32-28CC094C3486}");
        
        private static readonly string[] TjMonths = { "январи", "феврали", "марти", "апрели", "майи", "июни", "июли", "августи", "сентябри", "октябри", "ноябри", "декабри" };

        private static object GetTjMonthNames(object s)
        {
            var i = Convert.ToInt16(s);
            if (i > 0 && i <= 12) return TjMonths[i - 1];
            return s;
        }

        public static IAppServiceProvider GetProvider(IDataContext dataContext)
        {
            CreateBaseServiceFactories();
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            var provider = factory.Create(dataContext);
            return provider;
        }

        private static void CreateBaseServiceFactories()
        {
            AppServiceProvider.SetServiceFactoryFunc(typeof (IUserRepository),
                (arg) =>
                {
                    var prov = arg as IAppServiceProvider;
                    return new UserRepository(prov as IAppServiceProvider, prov.Get<IDataContext>());
                });
            AppServiceProvider.SetServiceFactoryFunc(typeof(IOrgRepository),
                prov =>
                    new OrgRepository((prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IAttributeRepository), (prov) => new AttributeRepository(prov as IAppServiceProvider));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocDefRepository),
                prov =>
                    new DocDefRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocRepository),
                prov =>
                    new DocRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocStateRepository),
                prov =>
                    new DocStateRepository((prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IDocumentTableMapRepository),
                (prov) =>
                    new DocumentTableMapRepository((prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(IEnumRepository),
                (prov) =>
                    new EnumRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IFormRepository),
                (prov) =>
                    new FormRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof (ILanguageRepository),
                prov =>
                    new LanguageRepository(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof (IPermissionRepository),
                prov =>
                    new PermissionRepository(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

            //AppServiceProvider.SetServiceFactoryFunc(typeof(IWorkflowRepository), CreateWorkflowRepository);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IWorkflowEngine), CreateWorkflowEngine);
            AppServiceProvider.SetServiceFactoryFunc(typeof (IAttributeStorage),
                (prov, dc) =>
                    new ServiceDefInfo(new AttributeStorage(prov as IAppServiceProvider, dc as IDataContext), true));

            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocumentStorage), 
                (prov, dc) =>
                    new ServiceDefInfo(new DocumentStorage(prov as IAppServiceProvider, dc as IDataContext), true));

            AppServiceProvider.SetServiceFactoryFunc(typeof(ITemplateReportGeneratorProvider),
                (prov, dc) =>
                    new ServiceDefInfo(new TemplateReportGeneratorProvider(prov as IAppServiceProvider, dc as IDataContext), true));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IControlFactory),
                (prov, dc) =>
                    new ServiceDefInfo(new ControlFactory(prov as IAppServiceProvider,
                        dc as IDataContext), false));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IComboBoxEnumProvider),
                prov =>
                    new ComboBoxEnumProvider(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilderFactory), 
                prov =>
                    new SqlQueryBuilderFactory(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilderFactory), CreateSqlQueryBuilderFactory2);
            AppServiceProvider.SetServiceFactoryFunc(typeof (ISqlQueryReaderFactory),
                prov => new SqlQueryReaderFactory(prov as IAppServiceProvider,
                    (prov as IAppServiceProvider).Get<IDataContext>()));

            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory), CreateSqlQueryReaderFactory2);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContextConfigSectionNameProvider), CreateDataContextConfigSectionNameProvider);

            //AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContext), CreateDataContext);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IMultiDataContext), CreateDataContext);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder),
                prov =>
                    new SqlQueryBuilderTool(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));
            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder), CreateSqlQueryBuilder2);

            AppServiceProvider.SetServiceFactoryFunc(typeof(IXlsFormDefBuilderFactory),
                (prov) => new XlsFormDefBuilderFactory(prov as IAppServiceProvider));

            AppServiceProvider.SetServiceFactoryFunc(typeof(IQueryRepository), (prov) => new QueryRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
        }
    }

}
