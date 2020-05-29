using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Model.Templates;
using Intersoft.CISSA.DataAccessLayer.Model.Workflow;
using Intersoft.CISSA.DataAccessLayer.Repository;
using Intersoft.CISSA.DataAccessLayer.Storage;

namespace OutputMetaCode
{
    class Program
    {
        public const string EnumDefs = "Справочники";
        public const string DocDefs = "Классы документов";
        public const string ProcessDefs = "Процессы";
        public const string FormDefs = "Формы";
        public const string TableDefs = "Табличные формы";
        public const string DocStateDefs = "Состояния документов";

        public const string HtmlHeadStyle = "<link href=\"style.css\" rel=\"stylesheet\" type=\"text/css\" />";

        private static void Main(string[] args)
        {
            var connectionString = ConfigurationManager.AppSettings.Get("connectionString");
            var currentUserName = ConfigurationManager.AppSettings.Get("userName");
            var currentUserPassword = ConfigurationManager.AppSettings.Get("userPassword");
            var fileName = ConfigurationManager.AppSettings.Get("fileName");
            var columnNo = ConfigurationManager.AppSettings.Get("columnNo");

            var time = DateTime.Now;
            //var doc = new Doc {Id = Guid.NewGuid()};
            dynamic t = new object(); //new DynaDoc(doc, Guid.NewGuid());
            Console.WriteLine(time.ToShortTimeString());
            using (var connection = new SqlConnection(connectionString))
                try
                {
                    using (var dataContext = new DataContext(connection))
                    {
                        CreateBaseServiceFactories();
                        var providerFactory = AppServiceProviderFactoryProvider.GetFactory();
                        using (var provider = providerFactory.Create(dataContext))
                        {
                            var serviceRegistrator = provider.Get<IAppServiceProviderRegistrator>();

                            serviceRegistrator.AddService(new UserDataProvider(currentUserName, currentUserPassword,
                                provider));

                            var sb = new ScriptStringBuilder();
                            sb.AppendLine("<html lang=\"ru\">");
                            sb.BeginBlock();
                            try
                            {
                                sb.AppendLine(
                                    "<head><meta charset=\"utf-8\"><title>Спецификация метасущностей АСИСТ</title>");
                                sb.AppendLine(HtmlHeadStyle + "</head>");

                                sb.AppendLine("<body>");
                                sb.BeginBlock();
                                try
                                {
                                    sb.AppendLine("<h1>" + EnumDefs + "</h1>");
                                    sb.AppendLine("<table border=\"1\" cellspacing=\"0\">");
                                    sb.BeginBlock();
                                    try
                                    {
                                        OutputEnumDefs(dataContext, EnumDefs + @".html", sb);
                                    }
                                    finally
                                    {
                                        sb.EndBlock();
                                        sb.AppendLine("</table>");
                                    }
                                    sb.AppendLine("<h1>" + DocDefs + "</h1>");
                                    sb.AppendLine("<table border=\"1\" cellspacing=\"0\">");
                                    sb.BeginBlock();
                                    try
                                    {
                                        OutputDocDefs(dataContext, DocDefs + @".html", sb);
                                    }
                                    finally
                                    {
                                        sb.EndBlock();
                                        sb.AppendLine("</table>");
                                    }
                                    sb.AppendLine("<h1>" + FormDefs + "</h1>");
                                    sb.AppendLine("<table border=\"1\" cellspacing=\"0\">");
                                    sb.BeginBlock();
                                    try
                                    {
                                        OutputFormDefs(dataContext, FormDefs + @".html", sb);
                                    }
                                    finally
                                    {
                                        sb.EndBlock();
                                        sb.AppendLine("</table>");
                                    }
                                }
                                finally
                                {
                                    sb.EndBlock();
                                    sb.AppendLine("</body>");
                                }
                            }
                            finally
                            {
                                sb.EndBlock();
                                sb.AppendLine("</html>");
                            }
                            CreateFileStream("index.html", sb);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(@"Ошибка: {0}", e.Message);
                    Console.ReadKey();
                }
            /*var dataContextFactory = DataContextFactoryProvider.GetFactory();

            using (var dataContext = dataContextFactory.CreateMultiDc(DataContextConfigSectionName))
            {
            }*/
            var finishTime = DateTime.Now;
            Console.WriteLine(@"Finish at " + finishTime.ToShortTimeString());
            var dTime = finishTime - time;
            Console.WriteLine(dTime.TotalSeconds + @" s");
            //            Console.ReadKey();
        }

        private const string SelectDocDefsSql = 
            @"SELECT e.Id, od.Full_Name, od.Name, aod.Full_Name, e.Is_Public, " +
                "CASE od5.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od5.Full_Name + ']->', isnull('{' + od5.Name + '}->', '')) + " +
		        "CASE od4.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od4.Full_Name + ']->', isnull('{' + od4.Name + '}->', '')) + " + 
		        "CASE od3.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od3.Full_Name + ']->', isnull('{' + od3.Name + '}->', '[-]')) + " + 
		        "CASE od2.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od2.Full_Name + ']->', isnull('{' + od2.Name + '}->', '[-]')) + " + 
		        "CASE od.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od.Full_Name + ']', isnull('{' + od.Name + '}', '[-]')) as Object_Path, e.Ancestor_Id " +
            "FROM Document_Defs e INNER JOIN Object_Defs od ON od.Id = e.Id " +
                "LEFT OUTER JOIN Object_Defs aod on aod.Id = e.Ancestor_Id " +
                "left outer join Object_Defs od2 on od.Parent_Id = od2.Id " +
	            "left outer join Object_Defs od3 on od2.Parent_Id = od3.Id " +
            	"left outer join Object_Defs od4 on od3.Parent_Id = od4.Id " +
	            "left outer join Object_Defs od5 on od4.Parent_Id = od5.Id " +
            "WHERE od.Full_Name NOT LIKE '*%' AND (od.Deleted IS NULL OR od.Deleted = 0) " +
            "ORDER BY od.Full_Name";

        private static void OutputDocDefs(IDataContext dataContext, string fileName, ScriptStringBuilder index)
        {
            var sb = new ScriptStringBuilder();
            sb.AppendLine("<html lang=\"ru\">");
            sb.BeginBlock();
            try
            {
                sb.AppendLine("<head><meta charset=\"utf-8\"><title>" + fileName + "</title>");
                sb.AppendLine(HtmlHeadStyle + "</head>");

                sb.AppendLine("<body>");
                sb.BeginBlock();
                try
                {
                    sb.AppendLine("<h1>Классы документов базы метаданных АСИСТ</h1>");
                    using (var command = dataContext.CreateCommand(SelectDocDefsSql))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var i = 1;
                            while (reader.Read())
                            {
                                if (reader.IsDBNull(1)) continue;
                                var defId = reader.GetGuid(0);
                                var defName = reader.GetString(1);

                                index.AppendFormat(
                                    "<tr><td>{0}.</td><td>[{{{1}}}]</td><td><a href=\"{2}.html#{1}\">{3}</a></td></tr>", i,
                                    defId, DocDefs, defName);
                                index.AppendLine();

                                sb.AppendLine("<a name=\"" + defId + "\"><h2>" + i + ". " + defName + "</h2></a>");
                                sb.AppendLine("<table border=\"1\" cellspacing=\"0\">");
                                sb.BeginBlock();
                                sb.AppendFormat("<tr><td><b>ID класса документа</b></td><td>[{{{0}}}]</td></tr>", defId);
                                sb.AppendFormat("<tr><td><b>Наименование</b></td><td>{0}</td></tr>", defName);
                                sb.AppendFormat("<tr><td><b>Программный идентификатор</b></td><td>{0}</td></tr>", reader.IsDBNull(2) ? "" : reader.GetString(2));

                                if (reader.IsDBNull(6))
                                    sb.AppendLine("<tr><td><b>Класс-предок</b></td><td>-</td></tr>");
                                else
                                    sb.AppendFormat(
                                        "<tr><td><b>Класс-предок</b></td><td><a href=\"#{1}\">{0}</a></td></tr>",
                                        reader.IsDBNull(3) ? "-" : reader.GetString(3), reader.GetGuid(6));
                                var visibility = (reader.IsDBNull(4) || !reader.GetBoolean(4))
                                    ? "Приватный"
                                    : "Публичный";
                                sb.AppendFormat("<tr><td><b>Видимость</b></td><td>{0}</td></tr>", visibility);
                                sb.AppendFormat("<tr><td><b>Расположение</b></td><td>{0}</td></tr>", reader.IsDBNull(5) ? "" : reader.GetString(5));
                                sb.EndBlock();
                                sb.AppendLine("</table>");
                                sb.AppendLine("<br/>");

                                sb.AppendLine("<h3>" + i + ".1. Атрибуты документа</h3>");

                                sb.AppendLine("<table border=\"1\" cellspacing=\"0\">");
                                sb.BeginBlock();
                                sb.AppendLine(
                                    "<thead><th>№</th><th>Id</th><th>Наименование</th><th>Программный идентификатор</th><th>Тип данных</th><th>Ссылка на метаобъект</th></thead>");
                                try
                                {
                                    sb.AppendLine("<tbody>");
                                    sb.BeginBlock();
                                    try
                                    {
                                        OutputDocDefAttributes(dataContext, sb, defId, "");
                                    }
                                    finally
                                    {
                                        sb.EndBlock();
                                        sb.AppendLine("</tbody>");
                                    }
                                }
                                finally
                                {
                                    sb.EndBlock();
                                    sb.AppendLine("</table>");
                                }
                                sb.AppendLine("<br/>");

                                sb.AppendLine("<h3>" + i + ".2. Ссылки на документ</h3>");
                                OutputObjectReferences(dataContext, sb, defId);
                                sb.AppendLine("<br/>");

                                i++;
                            }
                        }
                    }
                }
                finally
                {
                    sb.EndBlock();
                    sb.AppendLine("</body>");
                }
            }
            finally
            {
                sb.EndBlock();
                sb.AppendLine("</html>");
            }
            CreateFileStream(fileName, sb);
        }

        private const string SelectDocDefAttributesSql =
            @"SELECT e.Id, 1 AS Obj_Type, od.Full_Name AS Obj_Name, od.Name, dt.Name, dod.Full_Name, eod.Full_Name, dt.Id, e.Document_Id, e.Enum_Id, od.Order_Index " +
            "FROM Attribute_Defs e INNER JOIN Object_Defs od ON od.Id = e.Id " +
                "JOIN Data_Types dt ON e.Type_Id = dt.Id " +
                "LEFT OUTER JOIN Object_Defs dod ON dod.Id = e.Document_Id " +
                "LEFT OUTER JOIN Object_Defs eod ON eod.Id = e.Enum_Id " +
            "WHERE od.Parent_Id = @defId AND (od.Deleted IS NULL OR od.Deleted = 0) " +
            "UNION ALL SELECT f.Id, 2, od.Full_Name, od.Name, NULL, NULL, NULL, NULL, NULL, NULL, od.Order_Index " +
            "FROM Folder_Defs f JOIN Object_Defs od ON od.Id = f.Id " +
            "WHERE od.Parent_Id = @defId AND (od.Deleted IS NULL OR od.Deleted = 0) " +
            "ORDER BY Order_Index";

        private static void OutputDocDefAttributes(IDataContext dataContext, ScriptStringBuilder sb, Guid defId,
            string superNo)
        {
            var i = 1;
            using (var command = dataContext.CreateCommand(SelectDocDefAttributesSql))
            {
                command.Parameters.Add(new SqlParameter("@defId", SqlDbType.UniqueIdentifier) {Value = defId});

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);
                        var objType = (int) reader.GetValue(1);
                        var startA = "";
                        var endA = "";
                        if (objType == 1)
                        {
                            sb.AppendLine("<tr>");
                            sb.BeginBlock();
                            sb.AppendFormat("<td>{0}.</td>", superNo + i);
                            sb.AppendFormat("<td><a name=\"{0}\">[{{{0}}}]<a></td>", id);
                            sb.AppendFormat("<td>{0}</td>", reader.IsDBNull(2) ? "" : reader.GetString(2)); // Name
                            sb.AppendFormat("<td>{0}</td>", reader.IsDBNull(3) ? "" : reader.GetString(3)); // Identifier
                            sb.AppendFormat("<td>{0}</td>", reader.IsDBNull(4) ? "" : reader.GetString(4)); // Type Name
                            var dataType = reader.IsDBNull(7) ? 0 : reader.GetInt16(7);
                            if (dataType == 6)
                            {
                                var docId = reader.IsDBNull(8) ? Guid.Empty : reader.GetGuid(8);
                                if (docId != Guid.Empty)
                                {
                                    startA =  "<a href=\"#" + docId + "\">";
                                    endA = "</a>";
                                }
                                sb.AppendFormat("<td>{1}Документ: {0}{2}</td>", reader.IsDBNull(5) ? "" : reader.GetString(5), startA, endA);
                            }
                            else if (dataType == 5)
                            {
                                var enumId = reader.IsDBNull(9) ? Guid.Empty : reader.GetGuid(9);
                                if (enumId != Guid.Empty)
                                {
                                    startA = "<a href=\"" + EnumDefs + ".html#" + enumId + "\">";
                                    endA = "</a>";
                                }
                                sb.AppendFormat("<td>{1}Справочник: {0}{2}</td>",
                                    reader.IsDBNull(6) ? "" : reader.GetString(6), startA, endA);
                            }
                            else
                                sb.Append("<td></td>");
                            sb.EndBlock();
                            sb.AppendLine("</tr>");
                        }
                        else
                        {
                            sb.AppendLine("<tr>");
                            sb.BeginBlock();
                            sb.AppendFormat("<td>{0}.</td>", superNo + i);
                            sb.AppendFormat("<td>[{{{0}}}]</td>", id);
                            var category = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            if (string.IsNullOrEmpty(category))
                                category = reader.IsDBNull(3) ? "" : reader.GetString(3);
                            sb.AppendFormat("<td colspan=\"4\"><i>{0}</i></td>", category);
                            sb.EndBlock();
                            sb.AppendLine("</tr>");

                            OutputDocDefAttributes(dataContext, sb, id, superNo + i + ".");
                        }
                        i++;
                    }
                }
            }
        }

        private const string SelectFormDefsSql =
            @"SELECT e.Id, od.Full_Name, od.Name, aod.Full_Name, e.Document_Id, " +
                "CASE od5.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od5.Full_Name + ']->', isnull('{' + od5.Name + '}->', '')) + " +
                "CASE od4.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od4.Full_Name + ']->', isnull('{' + od4.Name + '}->', '')) + " +
                "CASE od3.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od3.Full_Name + ']->', isnull('{' + od3.Name + '}->', '[-]')) + " +
                "CASE od2.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od2.Full_Name + ']->', isnull('{' + od2.Name + '}->', '[-]')) + " +
                "CASE od.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od.Full_Name + ']', isnull('{' + od.Name + '}', '[-]')) as Object_Path, e.Form_Type " +
            "FROM (SELECT f.Id, 1 AS Form_Type, Document_Id FROM Forms f " +
                  "UNION ALL SELECT Id, 2, Document_Id FROM Table_Forms) e " +
                "INNER JOIN Object_Defs od ON od.Id = e.Id " +
                "LEFT OUTER JOIN Object_Defs aod on aod.Id = e.Document_Id " +
                "left outer join Object_Defs od2 on od.Parent_Id = od2.Id " +
                "left outer join Object_Defs od3 on od2.Parent_Id = od3.Id " +
                "left outer join Object_Defs od4 on od3.Parent_Id = od4.Id " +
                "left outer join Object_Defs od5 on od4.Parent_Id = od5.Id " +
            "WHERE od.Full_Name NOT LIKE '*%' AND (od.Deleted IS NULL OR od.Deleted = 0) " +
            "ORDER BY od.Full_Name";

        private static void OutputFormDefs(IDataContext dataContext, string fileName, ScriptStringBuilder index)
        {
            var sb = new ScriptStringBuilder();
            sb.AppendLine("<html lang=\"ru\">");
            sb.BeginBlock();
            try
            {
                sb.AppendLine("<head><meta charset=\"utf-8\"><title>" + fileName + "</title>");
                sb.AppendLine(HtmlHeadStyle + "</head>");

                sb.AppendLine("<body>");
                sb.BeginBlock();
                try
                {
                    sb.AppendLine("<h1>Описание форм базы метаданных АСИСТ</h1>");
                    using (var command = dataContext.CreateCommand(SelectFormDefsSql))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var i = 1;
                            while (reader.Read())
                            {
                                if (reader.IsDBNull(1)) continue;
                                var defId = reader.GetGuid(0);
                                var defName = reader.GetString(1);

                                index.AppendFormat(
                                    "<tr><td>{0}.</td><td>[{{{1}}}]</td><td><a href=\"{2}.html#{1}\">{3}</a></td></tr>", i,
                                    defId, FormDefs, defName);
                                index.AppendLine();

                                sb.AppendLine("<a name=\"" + defId + "\"><h2>" + i + ". " + defName + "</h2></a>");
                                sb.AppendLine("<table border=\"1\" cellspacing=\"0\">");
                                sb.BeginBlock();
                                sb.AppendFormat("<tr><td><b>ID формы</b></td><td>[{{{0}}}]</td></tr>", defId);
                                sb.AppendFormat("<tr><td><b>Наименование</b></td><td>{0}</td></tr>", defName);
                                sb.AppendFormat("<tr><td><b>Программный идентификатор</b></td><td>{0}</td></tr>", reader.IsDBNull(2) ? "" : reader.GetString(2));
                                sb.AppendFormat("<tr><td><b>Тип формы</b></td><td>{0}</td></tr>", reader.GetInt32(6) == 1 ? "Детальная форма" : "Табличная форма");

                                if (reader.IsDBNull(4))
                                    sb.AppendLine("<tr><td><b>Базовый документ</b></td><td>-</td></tr>");
                                else
                                    sb.AppendFormat(
                                        "<tr><td><b>Базовый документ</b></td><td><a href=\"{2}.html#{1}\">{0}</a></td></tr>",
                                        reader.IsDBNull(3) ? "-" : reader.GetString(3), reader.GetGuid(4), DocDefs);
                                sb.AppendFormat("<tr><td><b>Расположение</b></td><td>{0}</td></tr>", reader.IsDBNull(5) ? "" : reader.GetString(5));
                                sb.EndBlock();
                                sb.AppendLine("</table>");
                                sb.AppendLine("<br/>");

                                sb.AppendLine("<h3>" + i + ".1. Визуальные элементы</h3>");

                                sb.AppendLine("<table border=\"1\" cellspacing=\"0\">");
                                sb.BeginBlock();
                                sb.AppendLine(
                                    "<thead><th>№</th><th>Id</th><th>Наименование</th><th>Программный идентификатор</th><th>Тип</th><th>Атрибут</th><th>Форма</th><th>Процесс</th></thead>");
                                try
                                {
                                    sb.AppendLine("<tbody>");
                                    sb.BeginBlock();
                                    try
                                    {
                                        OutputFormControls(dataContext, sb, defId, "");
                                    }
                                    finally
                                    {
                                        sb.EndBlock();
                                        sb.AppendLine("</tbody>");
                                    }
                                }
                                finally
                                {
                                    sb.EndBlock();
                                    sb.AppendLine("</table>");
                                }
                                sb.AppendLine("<br/>");

                                sb.AppendLine("<h3>" + i + ".2. Ссылки на форму</h3>");
                                OutputObjectReferences(dataContext, sb, defId);
                                sb.AppendLine("<br/>");

                                i++;
                            }
                        }
                    }
                }
                finally
                {
                    sb.EndBlock();
                    sb.AppendLine("</body>");
                }
            }
            finally
            {
                sb.EndBlock();
                sb.AppendLine("</html>");
            }
            CreateFileStream(fileName, sb);
        }

        private const string SelectFormControlsSql =
            @"SELECT c.Id, Ctrl_Type, od.Full_Name, od.Name, c.Attr_Id, c.Attr_Name, c.Attr_Name2, c.Form_Id, c.Form_Name, c.Proc_Id, c.Proc_Name " +
            "FROM Object_Defs od JOIN (" +
                "SELECT e.Id as Id, 1 AS Ctrl_Type, ad.Id as Attr_Id, ado.Full_Name as Attr_Name, e.Attribute_Name as Attr_Name2, null as Form_Id, null as Form_Name, e.Process_Id as Proc_Id, po.Full_Name as Proc_Name " +
                "FROM Editors e " +
                    "LEFT OUTER JOIN Attribute_Defs ad ON e.Attribute_Id = ad.Id " +
                    "LEFT OUTER JOIN Object_Defs ado ON ado.Id = ad.Id " +
                    "LEFT OUTER JOIN Object_Defs po ON po.Id = e.Process_Id " +
                "UNION ALL SELECT c.Id, 2, ad.Id, ado.Full_Name, c.Attribute_Name, NULL, NULL, NULL, NULL " +
                "FROM Combo_Boxes c " +
                    "LEFT OUTER JOIN Attribute_Defs ad ON c.Attribute_Id = ad.Id " +
                    "LEFT OUTER JOIN Object_Defs ado ON ado.Id = ad.Id " +
                "UNION ALL SELECT c.Id, 3, NULL, NULL, NULL, NULL, NULL, c.Process_Id, po.Full_Name " +
                "FROM Buttons c " +
                    "LEFT OUTER JOIN Object_Defs po ON po.Id = c.Process_Id " +
                "UNION ALL SELECT c.Id, 4, NULL, NULL, NULL, c.Form_Id, fo.Full_Name, c.Process_Id, po.Full_Name " +
                "FROM Menus c " +
                    "LEFT OUTER JOIN Object_Defs po ON po.Id = c.Process_Id " +
                    "LEFT OUTER JOIN Object_Defs fo ON fo.Id = c.Form_Id " +
                "UNION ALL SELECT c.Id, 5, c.Attribute_Id, ao.Full_Name, c.Attribute_Name, c.Form_Id, fo.Full_Name, NULL, NULL " +
                "FROM DocumentControl c " +
                    "LEFT OUTER JOIN Object_Defs ao ON ao.Id = c.Attribute_Id " +
                    "LEFT OUTER JOIN Object_Defs fo ON fo.Id = c.Form_Id " +
                "UNION ALL SELECT c.Id, 6, c.Attribute_Id, ao.Full_Name, c.Attribute_Name, c.Form_Id, fo.Full_Name, NULL, NULL " +
                "FROM DocumentList_Forms c " +
                    "LEFT OUTER JOIN Object_Defs ao ON ao.Id = c.Attribute_Id " +
                    "LEFT OUTER JOIN Object_Defs fo ON fo.Id = c.Form_Id " +
                "UNION ALL SELECT c.Id, 7, c.Attribute_Id, ao.Full_Name, NULL, NULL, NULL, NULL, NULL " +
                "FROM Images c " +
                    "LEFT OUTER JOIN Object_Defs ao ON ao.Id = c.Attribute_Id " +
                "UNION ALL SELECT c.Id, 8, NULL, NULL, NULL, NULL, NULL, NULL, NULL " +
                "FROM Panels c " +
                "UNION ALL SELECT c.Id, 9, c.Attribute_Id, ado.Full_Name, c.Attribute_Name, NULL, NULL, NULL, NULL " +
                "FROM Table_Columns c " +
                    "LEFT OUTER JOIN Object_Defs ado ON ado.Id = c.Attribute_Id " +
                "UNION ALL SELECT c.Id, 10, NULL, NULL, NULL, NULL, NULL, NULL, NULL " +
                "FROM Texts c " +
                ") c ON c.Id = od.Id " +
            "WHERE od.Parent_Id = @defId AND (od.Deleted IS NULL OR od.Deleted = 0) " +
            "ORDER BY od.Order_Index";

        private static void OutputFormControls(IDataContext dataContext, ScriptStringBuilder sb, Guid defId,
            string superNo)
        {
            var i = 1;
            using (var command = dataContext.CreateCommand(SelectFormControlsSql))
            {
                command.Parameters.Add(new SqlParameter("@defId", SqlDbType.UniqueIdentifier) { Value = defId });

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetGuid(0);
                        var objType = (int) reader.GetValue(1);
                        var objTypeName = "";
                        switch (objType)
                        {
                            case 1:
                                objTypeName = "Поле ввода";
                                break;
                            case 2:
                                objTypeName = "Выпадающий список";
                                break;
                            case 3:
                                objTypeName = "Кнопка";
                                break;
                            case 4:
                                objTypeName = "Меню";
                                break;
                            case 5:
                                objTypeName = "Панель документа";
                                break;
                            case 6:
                                objTypeName = "Список документов";
                                break;
                            case 7:
                                objTypeName = "Изображение";
                                break;
                            case 8:
                                objTypeName = "Панель";
                                break;
                            case 9:
                                objTypeName = "Табличная колонка";
                                break;
                            case 10:
                                objTypeName = "Текст";
                                break;
                        }
                        sb.AppendLine("<tr>");
                        sb.BeginBlock();
                        sb.AppendFormat("<td>{0}.</td>", superNo + i); // #
                        sb.AppendFormat("<td>[{{{0}}}]</td>", id); // ID
                        sb.AppendFormat("<td>{0}</td>", reader.IsDBNull(2) ? "" : reader.GetString(2)); // Name
                        sb.AppendFormat("<td>{0}</td>", reader.IsDBNull(3) ? "" : reader.GetString(3)); // Identifier
                        sb.AppendFormat("<td>{0}</td>", objTypeName); // Control Type
                        var attrId = reader.IsDBNull(4) ? Guid.Empty : reader.GetGuid(4);
                        if (attrId != Guid.Empty)
                            sb.AppendFormat("<td><a href=\"" + DocDefs + ".html#{1}\">{0}</a></td>",
                                reader.IsDBNull(5) ? "" : reader.GetString(5), attrId); // Attribute
                        else sb.AppendFormat("<td>{0}</td>", reader.IsDBNull(6) ? "-" : reader.GetString(6));

                        var formId = reader.IsDBNull(7) ? Guid.Empty : reader.GetGuid(7);
                        if (formId != Guid.Empty)
                        {
                            sb.AppendFormat("<td><a href=\"#{1}\">{0}</a></td>",
                                reader.IsDBNull(8) ? "" : reader.GetString(8), formId); // Form
                        }
                        else sb.AppendLine("<td>-</td>");
                        var procId = reader.IsDBNull(9) ? Guid.Empty : reader.GetGuid(9);
                        if (procId != Guid.Empty)
                        {
                            sb.AppendFormat("<td><a href=\"" + ProcessDefs + ".html#{1}\">{0}</a></td>",
                                reader.IsDBNull(10) ? "" : reader.GetString(10), procId); // Process
                        }
                        else sb.AppendLine("<td>-</td>");
                        sb.EndBlock();
                        sb.AppendLine("</tr>");

                        if (objType == 1 || objType == 4 || objType == 5 || objType == 6 || objType == 8 || objType == 9)
                            OutputFormControls(dataContext, sb, id, superNo + i + ".");
                        i++;
                    }
                }
            }
        }
        
        private const string SelectEnumDefsSql = @"SELECT e.Id, od.Full_Name, od.Name, " +
                "CASE od5.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od5.Full_Name + ']->', isnull('{' + od5.Name + '}->', '')) + " +
                "CASE od4.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od4.Full_Name + ']->', isnull('{' + od4.Name + '}->', '')) + " +
                "CASE od3.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od3.Full_Name + ']->', isnull('{' + od3.Name + '}->', '[-]')) + " +
                "CASE od2.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od2.Full_Name + ']->', isnull('{' + od2.Name + '}->', '[-]')) + " +
                "CASE od.Deleted WHEN 1 THEN '!' ELSE '' END + isnull('[' + od.Full_Name + ']', isnull('{' + od.Name + '}', '[-]')) as Object_Path " +
            "FROM Enum_Defs e INNER JOIN Object_Defs od ON od.Id = e.Id " +
                "left outer join Object_Defs od2 on od.Parent_Id = od2.Id " +
                "left outer join Object_Defs od3 on od2.Parent_Id = od3.Id " +
                "left outer join Object_Defs od4 on od3.Parent_Id = od4.Id " +
                "left outer join Object_Defs od5 on od4.Parent_Id = od5.Id " +
            "WHERE od.Deleted IS NULL OR od.Deleted = 0 " +
            "ORDER BY od.Full_Name";

        private static void OutputEnumDefs(IDataContext dataContext, string fileName, ScriptStringBuilder index)
        {
            var sb = new ScriptStringBuilder();
            sb.AppendLine("<html lang=\"ru\">");
            sb.BeginBlock();
            try
            {
                sb.AppendLine("<head><meta charset=\"utf-8\"><title>" + EnumDefs + "</title>");
                sb.AppendLine(HtmlHeadStyle + "</head>");

                sb.AppendLine("<body>");
                sb.BeginBlock();
                try
                {
                    sb.AppendLine("<h1>Справочники АСИСТ</h1>");
                    using (var command = dataContext.CreateCommand(SelectEnumDefsSql))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var i = 1;
                            while (reader.Read())
                            {
                                var enumDefId = reader.GetGuid(0);
                                var enumDefName = reader.GetString(1);

                                index.AppendFormat(
                                    "<tr><td>{0}.</td><td>[{{{1}}}]</td><td><a href=\"{2}.html#{1}\">{3}</a></td></tr>", i,
                                    enumDefId, EnumDefs, enumDefName);
                                index.AppendLine();
                                
                                sb.AppendLine("<a name=\"" + enumDefId + "\"><h2>" + i + ". " + enumDefName + "</h2></a>");
                                sb.AppendLine("<table border=\"1\" cellspacing=\"0\">");
                                sb.BeginBlock();
                                sb.AppendFormat("<tr><td><b>ID справочника</b></td><td>[{{{0}}}]</td></tr>", enumDefId);
                                sb.AppendFormat("<tr><td><b>Наименование справочника</b></td><td>{0}</td></tr>", enumDefName);
                                sb.AppendFormat("<tr><td><b>Программный идентификатор</b></td><td>{0}</td></tr>", reader.IsDBNull(2) ? "" : reader.GetString(2));
                                sb.AppendFormat("<tr><td><b>Расположение</b></td><td>{0}</td></tr>", reader.IsDBNull(3) ? "" : reader.GetString(3));
                                sb.EndBlock();
                                sb.AppendLine("</table>");
                                sb.AppendLine("<br/>");

                                sb.AppendLine("<h3>" + i + ".1. Элементы справочника</h3>");
                                OutputEnumDefItems(dataContext, sb, enumDefId);
                                sb.AppendLine("<br/>");

                                sb.AppendLine("<h3>" + i + ".2. Ссылки на справочник</h3>");
                                OutputObjectReferences(dataContext, sb, enumDefId);
                                sb.AppendLine("<br/>");

                                i++;
                            }
                        }
                    }
                }
                finally
                {
                    sb.EndBlock();
                    sb.AppendLine("</body>");
                }
            }
            finally
            {
                sb.EndBlock();
                sb.AppendLine("</html>");
            }
            CreateFileStream(fileName, sb);
        }

        private const string SelectEnumDefItemsSql = @"SELECT e.Id, od.Full_Name, od.Name " +
            "FROM Enum_Items e INNER JOIN Object_Defs od ON od.Id = e.Id " +
            "WHERE od.Parent_Id = @defId AND (od.Deleted IS NULL OR od.Deleted = 0) " +
            "ORDER BY od.Order_Index";

        private static void OutputEnumDefItems(IDataContext dataContext, ScriptStringBuilder sb, Guid id)
        {
            sb.AppendLine("<table border=\"1\" cellspacing=\"0\">");
            sb.BeginBlock();
            try
            {
                sb.AppendLine("<thead><th>Id</th><th>Наименование</th></thead>");
                sb.AppendLine("<tbody>");
                sb.BeginBlock();
                try
                {
                    using (var command = dataContext.CreateCommand(SelectEnumDefItemsSql))
                    {
                        command.Parameters.Add(new SqlParameter("@defId", SqlDbType.UniqueIdentifier) { Value = id });

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sb.AppendLine("<tr>");
                                sb.BeginBlock();
                                sb.AppendFormat("<td>[{{{0}}}]</td>", reader.GetGuid(0));
                                sb.AppendFormat("<td>{0}</td>", reader.GetString(1));
                                sb.EndBlock();
                                sb.AppendLine("</tr>");
                            }
                        }
                    }
                }
                finally
                {
                    sb.EndBlock();
                    sb.AppendLine("</tbody>");
                }
            }
            finally
            {
                sb.EndBlock();
                sb.AppendLine("</table>");
            }
        }

        private static void OutputObjectReferences(IDataContext dataContext, ScriptStringBuilder sb, Guid id)
        {
            sb.AppendLine("<table border=\"1\" cellspacing=\"0\">");
            sb.BeginBlock();
            try
            {
                sb.AppendLine("<thead><th>Id</th><th>Наименование объекта</th><th>Тип связи</th><th>Расположение объекта</th></thead>");
                sb.AppendLine("<tbody>");
                sb.BeginBlock();
                try
                {
                    using (var command = dataContext.CreateCommand(SelectObjectReferencesSql))
                    {
                        command.Parameters.Add(new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Value = id });

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sb.AppendLine("<tr>");
                                sb.BeginBlock();
                                var objId = reader.GetGuid(0);
                                sb.AppendFormat("<td>[{{{0}}}]</td>", objId);
                                sb.AppendFormat("<td><a href=\"index.html#{1}\">{0}</a></td>", reader.GetString(1), objId);
                                sb.AppendFormat("<td>{0}</td>", reader.GetString(2));
                                sb.AppendFormat("<td>{0}</td>", reader.GetString(3));
                                sb.EndBlock();
                                sb.AppendLine("</tr>");
                            }
                        }
                    }
                }
                finally
                {
                    sb.EndBlock();
                    sb.AppendLine("</tbody>");
                }
            }
            finally
            {
                sb.EndBlock();
                sb.AppendLine("</table>");
            }
        }

        private const string SelectObjectReferencesSql = @"
select 
	o.Id, o.Object_Type, o.Ref_Type, 
	--od1.Deleted, --od1.Name as Object_Name,
	isnull('[' + od5.Full_Name + ']->', isnull('{' + od5.Name + '}->', '')) + 
		isnull('[' + od4.Full_Name + ']->', isnull('{' + od4.Name + '}->', '')) + 
		isnull('[' + od3.Full_Name + ']->', isnull('{' + od3.Name + '}->', '[-]')) + 
		isnull('[' + od2.Full_Name + ']->', isnull('{' + od2.Name + '}->', '[-]')) + 
		isnull('[' + od1.Full_Name + ']', isnull('{' + od1.Name + '}', '[-]')) as Object_Path
from Object_Defs od1
	left outer join Object_Defs od2 on od1.Parent_Id = od2.Id
	left outer join Object_Defs od3 on od2.Parent_Id = od3.Id
	left outer join Object_Defs od4 on od3.Parent_Id = od4.Id
	left outer join Object_Defs od5 on od4.Parent_Id = od5.Id
	inner join (
		select Id, 'Класс документа' as Object_Type, 'Наследование' as Ref_Type
		from Document_Defs
		where Ancestor_Id = @id
		union all
		select Id, 'Атрибут документа', 'Ссылка на документ'
		from Attribute_Defs
		where Document_Id = @id
		union all
		select Id, 'Атрибут документа', 'Ссылка на справочник'
		from Attribute_Defs
		where Enum_Id = @id
		union all
		select Id, 'Форма', 'Базовый документ'
		from Forms
		where Document_Id = @id
		union all
		select Id, 'Табличная форма', 'Базовый документ'
		from Table_Forms
		where Document_Id = @id
		union all
		select Id, 'Табличная форма', 'Детальная форма'
		from Table_Forms dd1
		where dd1.Form_Id = @id
		union all
		select Id, 'Табличная форма', 'Форма фильтра'
		from Table_Forms dd1
		where dd1.Filter_Form_Id = @id
		union all
		select Id, 'Панель документа', 'Атрибут'
		from DocumentControl dd1
		where dd1.Attribute_Id = @id
		union all
		select Id, 'Панель документа', 'Форма'
		from DocumentControl dd1
		where dd1.Form_Id = @id
		union all
		select Id, 'Табличная панель', 'Атрибут'
		from DocumentList_Forms dd1
		where dd1.Attribute_Id = @id
		union all
		select Id, 'Табличная панель', 'Форма'
		from DocumentList_Forms dd1
		where dd1.Form_Id = @id
		union all
		select Id, 'Панель', 'Атрибут'
		from Table_Columns dd1
		where dd1.Attribute_Id = @id
		union all
		select Id, 'Грид', 'Базовый документ'
		from Grids dd1
		where dd1.Document_Id = @id
		union all
		select Id, 'Поле формы', 'Атрибут'
		from Editors dd1
		where dd1.Attribute_Id = @id
		union all
		select Id, 'Комбо бокс', 'Атрибут'
		from Combo_Boxes dd1
		where dd1.Attribute_Id = @id
		union all
		select Id, 'Комбо бокс', 'Отображаемый атрибут'
		from Combo_Boxes dd1
		where dd1.Detail_Attribute_Id = @id
		union all
		select Id, 'Кнопка', 'Процесс'
		from Buttons dd1
		where dd1.Process_Id = @id
		union all
		select Id, 'Кнопка', 'Пользовательский выбор'
		from Buttons dd1
		where dd1.User_Action_Id = @id
		union all
		select Id, 'Пункт меню', 'Процесс'
		from Menus dd1
		where dd1.Process_Id = @id
		union all
		select Id, 'Пункт меню', 'Табличная форма'
		from Menus dd1
		where dd1.Form_Id = @id
		union all
		select Id, 'Пункт меню', 'Статус документа'
		from Menus dd1
		where dd1.State_Type_Id = @id
		union all
		select Id, 'Действие над документом', 'Класс документа'
		from Document_Activities dd1
		where dd1.Document_Id = @id
		union all
		select Id, 'Действие над статусом документа', 'Статус'
		from Document_State_Activities dd1
		where dd1.State_Type_Id = @id
		union all
		select Id, 'Финальное действие', 'Отображаемая форма'
		from Finish_Activities dd1
		where dd1.Form_Id = @id
		union all
		select Id, 'Пользовательское действие', 'Форма'
		from Presentation_Activities dd1
		where dd1.Form_Id = @id
		union all
		select Id, 'Вызов внешнего процесса', 'Шлюз доступа'
		from Gate_Call_Activities dd1
		where dd1.Gate_Id = @id
		union all
		select Id, 'Вызов процесса', 'Процесс'
		from Process_Call_Activities dd1
		where dd1.Process_Id = @id
		union all
		select Id, 'Скрипт-действие', 'Ссылка в программном коде'
		from Script_Activities dd1
		where dd1.Script Like '%' + cast(@id as varchar(40)) + '%'
		union all
		select Id, 'Скрипт-процесса', 'Ссылка в программном коде'
		from Workflow_Processes dd1
		where dd1.Script Like '%' + cast(@id as varchar(40)) + '%'
		union all
		select Id, 'Доступ к процессу для внешних сервисов', 'Процесс'
		from Workflow_Gates dd1
		where dd1.Process_Id = @id
		union all
		select Id, 'Поток управления', 'Действие-источник'
		from Activity_Links dd1
		where dd1.Source_Id = @id
		union all
		select Id, 'Поток управление', 'Действие-приемник'
		from Activity_Links dd1
		where dd1.Target_Id = @id
		union all
		select Id, 'Поток управления', 'Пользовательский выбор'
		from Activity_Links dd1
		where dd1.User_Action_Id = @id
		union all
		select Id, 'Условие выборки', 'Атрибут в левой части условия'
		from Conditions dd1
		where dd1.Left_Attribute_Id = @id
		union all
		select Id, 'Условие выборки', 'Атрибут в правой части условия'
		from Conditions dd1
		where dd1.Right_Attribute_Id = @id
		union all
		select Id, 'Запрос', 'Ссылка на класс документа - Источник данных запроса'
		from Queries dd1
		where dd1.Document_Id = @id
		union all
		select Id, 'Под запрос', 'Ссылка на класс документа - Источник данных запроса'
		from Query_Sources dd1
		where dd1.Document_Id = @id
		union all
		select Id, 'Подзапрос', 'Ссылка на другой запрос'
		from Query_Sources dd1
		where dd1.Query_Id = @id
		union all
		select Id, 'Организация', 'Тип организации'
		from Organizations dd1
		where dd1.Type_Id = @id
		union all
		select Id, 'Worker', 'OrgPosition'
		from Workers dd1
		where dd1.OrgPosition_Id = @id
		union all
		select Id, 'Привязка права доступа', 'Объект, к которому привязано право доступа'
		from Object_Defs dd1
			join Permission_Defs pd1 on pd1.Def_Id = dd1.Id
		where pd1.Permission_Id = @id
		union all
		select Id, 'Привязка права доступа', 'Право доступа'
		from Object_Defs od1
			join Permission_Defs pd1 on pd1.Permission_Id = od1.Id
		where pd1.Def_Id = @id
		union all
		select OrgUnit_Id, 'Привязка доступа видимости', 'Доступ к орг.структуре'
		from OrgUnits_ObjectDefs oo
		where oo.ObjDef_Id = @id
		union all
		select ObjDef_Id, 'Привязка доступа видимости', 'Объект, имеющий доступ'
		from OrgUnits_ObjectDefs
		where OrgUnit_Id = @id
		union all
		select Org_Unit_Id, 'Привязка ролей доступа', 'Орг.структура'
		from Org_Units_Roles
		where Role_Id = @id
		union all
		select Role_Id, 'Привзяка ролей доступа', 'Роль'
		from Org_Units_Roles
		where Org_Unit_Id = @id
		union all
		select Worker_Id, 'Привязка ролей доступа', 'Пользователь'
		from Worker_Roles
		where Role_Id = @id
		union all
		select Role_Id, 'Привязка ролей доступа', 'Назначенная роль'
		from Worker_Roles
		where Worker_Id = @id
		union all
		select Org_Position_Id, 'Привязка ролей доступа', 'Орг. позиция, связанная с ролью'
		from Org_Positions_Roles
		where Role_Id = @id
		union all
		select Role_Id, 'Привязка ролей доступа', 'Назначенная роль'
		from Org_Positions_Roles
		where Org_Position_Id = @id
		union all
		select Def_Id, 'Привязка ролей доступа', 'Объект, которому назначена роль'
		from Role_Refs
		where Role_Id = @id
		union all
		select Role_Id, 'Привязка ролей доступа', 'Назначенная роль'
		from Role_Refs
		where Def_Id = @id
		union all
		select Permission_Id, 'Привязка прав доступа', 'Назначенное право доступа'
		from Role_Permissions
		where Role_Id = @id
		union all
		select Role_Id, 'Привязка прав доступа', 'Роль, содержащая право доступа'
		from Role_Permissions
		where Permission_Id = @id
	) as o on o.Id = od1.Id
where
    isnull(od5.Deleted, 0) = 0 and isnull(od4.Deleted, 0) = 0 and isnull(od3.Deleted, 0) = 0 and isnull(od2.Deleted, 0) = 0 and
    isnull(od1.Deleted, 0) = 0
order by Object_Path";

        private static void CreateFileStream(string fileName, ScriptStringBuilder sb)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                var sw = new StreamWriter(fileStream) {AutoFlush = true};
                sb.WriteTo(sw);
            }
        }

        public static void CreateBaseServiceFactories()
        {
            AppServiceProvider.SetServiceFactoryFunc(typeof(IUserRepository),
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
            AppServiceProvider.SetServiceFactoryFunc(typeof(IDocumentTableMapRepository),
                (prov) =>
                    new DocumentTableMapRepository((prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(IEnumRepository),
                (prov) =>
                    new EnumRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IFormRepository),
                (prov) =>
                    new FormRepository(prov as IAppServiceProvider, (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(ILanguageRepository),
                prov =>
                    new LanguageRepository(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));
            AppServiceProvider.SetServiceFactoryFunc(typeof(IPermissionRepository),
                prov =>
                    new PermissionRepository(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(IAttributeStorage),
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

            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory),
                prov => new SqlQueryReaderFactory(prov as IAppServiceProvider,
                    (prov as IAppServiceProvider).Get<IDataContext>()));

            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryReaderFactory),
                (prov, dc) =>
                    new ServiceDefInfo(new SqlQueryReaderFactory(prov as IAppServiceProvider,
                        dc as IDataContext), false));
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContextConfigSectionNameProvider), CreateDataContextConfigSectionNameProvider);

            //AppServiceProvider.SetServiceFactoryFunc(typeof(IDataContext), CreateDataContext);
            //AppServiceProvider.SetServiceFactoryFunc(typeof(IMultiDataContext), CreateDataContext);
            AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder),
                prov =>
                    new SqlQueryBuilderTool(prov as IAppServiceProvider,
                        (prov as IAppServiceProvider).Get<IDataContext>()));
            //AppServiceProvider.SetServiceFactoryFunc(typeof(ISqlQueryBuilder), CreateSqlQueryBuilder2);
            AppServiceProvider.SetServiceFactoryFunc(typeof(IQueryRepository),
                prov => new QueryRepository(prov as IAppServiceProvider,
                    (prov as IAppServiceProvider).Get<IDataContext>()));
        }
    }
}
