using System;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.Cissa.Report.Xls;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;

namespace ConsoleApplication1.Tests
{
    public static class GetDocList
    {
        public static void OutputDocListToFile(IAppServiceProvider provider, IDataContext dataContext, string fileName, Guid docDefId, Guid orgId)
        {
            var sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
            var query = sqlQueryBuilder.Build(docDefId, null, null);
            query.AddAttribute("&State");
            query.AddAttribute("&OrgName");
            var docDef = query.Source.GetDocDef();

            using (var def = new XlsDef())
            {
                // Header
                def.AddArea().AddRow().AddEmptyCell();
                def.AddArea().AddRow().AddText(docDef.Caption);
                def.AddArea().AddRow().AddEmptyCell();
                // Grid Header
                var h1 = def.AddArea().AddRow();
                foreach (var attr in query.Attributes)
                {
                    h1.AddNode(attr.AliasName);
                }

                using (var reader = new SqlQueryReader(dataContext, query))
                {
                    reader.Open();
                    while (reader.Read())
                    {
                        int i = 0;
                        var r = def.AddArea().AddRow();
                        foreach (var attr in query.Attributes)
                        {
                            var value = !reader.IsDbNull(i) ? reader.GetValue(i) : null;
                            if (value != null)
                                r.AddColumn().AddText(value.ToString());
                            else
                                r.AddColumn().AddEmptyCell();
                            i++;
                        }
                    }
                    reader.Close();
                }
                var builder = new XlsBuilder(def);
                var workbook = builder.Build();
                using (var stream = new FileStream(fileName, FileMode.Create))
                {
                    workbook.Write(stream);
                }
            }
        }
    }
}
