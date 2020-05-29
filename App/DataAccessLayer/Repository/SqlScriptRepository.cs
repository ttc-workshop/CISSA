using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class SqlScriptRepository: ISqlScriptRepository
    {
        public IList<Guid> Execute(Guid scriptId)
        {
            using (var dataContext = new DataContext())
            {
                var query = from x in dataContext.Entities.Object_Defs.OfType<Script>()
                            where x.Id == scriptId
                            select x.Script_Text;

                if (query.Any())
                {
                    string script = query.First();

                    using (var cnn = dataContext.StoreConnection)
                    {
                        var cmd = cnn.CreateCommand();

                        cmd.CommandText = script;
                        cmd.CommandType = CommandType.Text;
                        cmd.Connection = cnn;

                        var tbl = new DataTable();
                        cnn.Open();
                        
                        (new SqlDataAdapter((SqlCommand) cmd)).Fill(tbl);
                        cnn.Close();

                        if (tbl.Rows.Count > 0)
                        {
                            return (from DataRow row in tbl.Rows
                                    select (Guid) row[0]
                                   ).ToList();
                        }

                    }
                }
                else
                {
                    throw new ApplicationException(
                        string.Format("Скрипт с идентификатором {0} не существует.", scriptId));
                }
            }
            return null;
        }
    }
}
