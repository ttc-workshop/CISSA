using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Maps;

namespace Intersoft.CISSA.DataAccessLayer.Storage
{
    public class DocumentStorage : IDocumentStorage
    {
        public IDataContext DataContext { get; private set; }

//        public SqlConnection Connection { get; private set; }

        public DocumentStorage(IDataContext dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            DataContext = dataContext;
        }
        public DocumentStorage(IAppServiceProvider provider, IDataContext dataContext) : this(dataContext)
        {
            //DataContext = dataContext; // provider.Get<IDataContext>();
        }

        /*public DocumentStorage(SqlConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("Connection");

            Connection = connection;
        }*/

        public static readonly DateTime MaxDate = new DateTime(9999, 12, 31);

        private const string SelectLastDocumentSql =
            "SELECT [Id], [Created], [Def_Id], [UserId], [Organization_Id], [Org_Position_Id], [Last_Modified]\n" +
            "FROM [Documents] WITH(NOLOCK)\n" +
            "WHERE [Id] = @Id AND ([Deleted] IS NULL OR [Deleted] = 0)";

        public DocData Load(Guid id)
        {
            using (var command = CreateCommand(SelectLastDocumentSql))
            {
                AddParamWithValue(command, "@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new DocData
                        {
                            Id = reader.GetGuid(0),
                            Created = reader.IsDBNull(2) ? (DateTime?) null : reader.GetDateTime(1),
                            DefId = reader.IsDBNull(2) ? (Guid?) null : reader.GetGuid(2),
                            UserId = reader.GetGuid(3),
                            OrganizationId = reader.IsDBNull(4) ? (Guid?) null : reader.GetGuid(4),
                            PositionId = reader.IsDBNull(5) ? (Guid?) null : reader.GetGuid(5),
                            LastModified = reader.IsDBNull(6) ? (DateTime?) null : reader.GetDateTime(6)
                        };
                    }
                    return null;
                }
            }
        }

        private const string UpdateDocumentSql =
            "UPDATE [Documents] WITH(rowlock) SET [Last_Modified] = @Modified WHERE [Id] = @Id";

        private const string InsertDocumentSql =
            "INSERT INTO [Documents] WITH(rowlock)\n" +
            " ([Id], [Def_Id], [Created], [UserId], [Organization_Id], [Org_Position_Id], [Last_Modified])\n" +
            "VALUES (@Id, @DefId, @Created, @UserId, @OrgId, @PositionId, @Modified);";

        private const string SaveDocumentSql =
            "UPDATE [Documents] WITH(rowlock) SET [Last_Modified] = @Modified WHERE [Id] = @Id;\n" +
            "IF @@rowcount = 0 BEGIN\n" +
            "INSERT INTO [Documents] WITH(rowlock)\n" +
            " ([Id], [Def_Id], [Created], [UserId], [Organization_Id], [Org_Position_Id], [Last_Modified])\n" +
            "VALUES (@Id, @DefId, @Created, @UserId, @OrgId, @PositionId, @Modified);\n" +
            "END";

        public void Save(Doc doc, UserInfo userInfo, DateTime date)
        {
            DataContext.BeginTransaction();
            try
            {
                using (var command = CreateCommand(SaveDocumentSql))
                {
                    AddParamWithValue(command, "@Id", doc.Id);
                    AddParamWithValue(command, "@DefId", doc.DocDef.Id);
                    AddParamWithValue(command, "@Created", doc.CreationTime);
                    AddParamWithValue(command, "@UserId", userInfo.Id);
                    AddParamWithValue(command, "@OrgId", userInfo.OrganizationId);
                    AddParamWithValue(command, "@PositionId", userInfo.PositionId);
                    AddParamWithValue(command, "@Modified", date);

                    command.ExecuteNonQuery();
                }
                /*if (IsExists(doc.Id))
                {
                    using (var command = CreateCommand(UpdateDocumentSql))
                    {
                        AddParamWithValue(command, "@Modified", date);
                        AddParamWithValue(command, "@Id", doc.Id);

                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var command = CreateCommand(InsertDocumentSql))
                    {
                        AddParamWithValue(command, "@Id", doc.Id);
                        AddParamWithValue(command, "@DefId", doc.DocDef.Id);
                        AddParamWithValue(command, "@Created", doc.CreationTime);
                        AddParamWithValue(command, "@UserId", userInfo.Id);
                        AddParamWithValue(command, "@OrgId", userInfo.OrganizationId);
                        AddParamWithValue(command, "@PositionId", userInfo.PositionId);
                        AddParamWithValue(command, "@Modified", date);

                        command.ExecuteNonQuery();
                    }
                }*/
                DataContext.Commit();
            }
            catch
            {
                DataContext.Rollback();
                throw;
            }
        }

        private const string SelectExistsDocumentSql =
            "SELECT CASE WHEN EXISTS(SELECT [Id] FROM [Documents] WITH(NOLOCK) WHERE [Id] = @Id) THEN 1 ELSE 0 END AS Expr1;";

        public bool IsExists(Guid docId)
        {
            using (var command = CreateCommand(SelectExistsDocumentSql))
            {
                AddParamWithValue(command, "@Id", docId);

                var result = (int) command.ExecuteScalar();

                return result == 1;
            }
        }

        private const string SelectLastDocumentStateSql =
            "SELECT [Id], [Created], [State_Type_Id], [Worker_Id]\n" +
            "FROM [Document_States] WITH(NOLOCK)\n" +
            "WHERE [Document_Id] = @Id AND [Expired] = '99991231'";
        private const string SelectDateDocumentStateSql =
            "SELECT [Id], [Created], [State_Type_Id], [Worker_Id]\n" +
            "FROM [Document_States] WITH(NOLOCK)\n" +
            "WHERE [Document_Id] = @Id AND [Created] <= @Date AND [Expired] > @Date";

        public DocStateData LoadDocState(Guid docId)
        {
            return LoadDocState(docId, MaxDate);
        }

        public DocStateData LoadDocState(Guid docId, DateTime forDate)
        {
            var sql = forDate == MaxDate ? SelectLastDocumentStateSql : SelectDateDocumentStateSql;

            using (var command = CreateCommand(sql))
            {
                AddParamWithValue(command, "@Id", docId);
                if (forDate < MaxDate)
                    AddParamWithValue(command, "@Date", forDate);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new DocStateData
                        {
                            Id = reader.GetGuid(0),
                            Created = reader.GetDateTime(1),
                            StateTypeId = reader.GetGuid(2),
                            UserId = reader.GetGuid(3)
                        };
                    }
                    return null;
                }
            }
        }

        private const string ExpiryDocumentStateSql =
            "UPDATE [Document_States] WITH(rowlock) SET\n" +
            "[Expired] = @Date\n" +
            "WHERE [Document_Id] = @Id AND [Expired] = '99991231'";
        private const string InsertDocumentStateSql =
            "INSERT [Document_States] WITH(rowlock) ([Id], [Document_Id], [Created], [State_Type_Id], [Expired], [Worker_Id])\n" +
            "VALUES (@Id, @DocId, @Created, @StateTypeId, @Expired, @UserId)";

        private const string SaveDocumentStateSql =
            "IF NOT EXISTS(SELECT [Id] FROM [Document_States] WITH(NOLOCK) WHERE " +
            "[Document_Id] = @DocId AND [State_Type_Id] = @StateTypeId AND [Expired] = '99991231')\n" +
            "BEGIN\n" +
            "UPDATE [Document_States] WITH(rowlock) SET [Expired] = @Date\n" +
            "WHERE [Document_Id] = @DocId AND [Expired] = '99991231';\n" +
            "INSERT [Document_States] WITH(rowlock) ([Id], [Document_Id], [Created], [State_Type_Id], [Expired], [Worker_Id])\n" +
            "VALUES (@Id, @DocId, @Date, @StateTypeId, @Expired, @UserId);\n" +
            "END";
        
        public void SaveDocState(Guid id, Guid docId, Guid stateTypeId, Guid userId, DateTime forDate)
        {
            DataContext.BeginTransaction();
            try
            {
                using (var command = CreateCommand(SaveDocumentStateSql))
                {
                    AddParamWithValue(command, "@Id", id);
                    AddParamWithValue(command, "@DocId", docId);
                    AddParamWithValue(command, "@Date", forDate);
                    AddParamWithValue(command, "@StateTypeId", stateTypeId);
                    AddParamWithValue(command, "@Expired", MaxDate);
                    AddParamWithValue(command, "@UserId", userId);

                    command.ExecuteNonQuery();
                } 
                /*using (var command = CreateCommand(ExpiryDocumentStateSql))
                {
                    AddParamWithValue(command, "@Id", docId);
                    AddParamWithValue(command, "@Date", forDate);

                    command.ExecuteNonQuery();
                }
                using (var command = CreateCommand(InsertDocumentStateSql))
                {
                    AddParamWithValue(command, "@Id", id);
                    AddParamWithValue(command, "@DocId", docId);
                    AddParamWithValue(command, "@Created", forDate);
                    AddParamWithValue(command, "@StateTypeId", stateTypeId);
                    AddParamWithValue(command, "@Expired", MaxDate);
                    AddParamWithValue(command, "@UserId", userId);

                    command.ExecuteNonQuery();
                }*/
                DataContext.Commit();
            }
            catch
            {
                DataContext.Rollback();
                throw;
            }
        }

        private const string SelectDocumentImageSql =
            "SELECT [Data] FROM [Document_Contents] WITH(NOLOCK) WHERE [Id] = @Id";

        public string LoadDocImage(Guid docId)
        {
            using (var command = CreateCommand(SelectDocumentImageSql))
            {
                AddParamWithValue(command, "@Id", docId);

                var val = command.ExecuteScalar();

                return val != null ? val.ToString() : String.Empty;
            }
        }

        private const string SelectExistsDocumentImageSql =
            "SELECT CASE WHEN EXISTS(SELECT [Id] FROM [Document_Contents] WITH(NOLOCK) WHERE [Id] = @Id) THEN 1 ELSE 0 END AS Expr1;";

        public bool ExistsDocImage(Guid docId)
        {
            using (var command = CreateCommand(SelectExistsDocumentImageSql))
            {
                AddParamWithValue(command, "@Id", docId);

                var result = (int)command.ExecuteScalar();

                return result == 1;
            }
        }

        private const string UpdateDocumentImageSql =
            "UPDATE [Document_Contents] WITH(rowlock) SET [Data] = @Data WHERE [Id] = @Id";
        private const string InsertDocumentImageSql =
            "INSERT INTO [Document_Contents] WITH(rowlock) ([Id], [Data])\n" +
            "VALUES (@Id, @Data);";

        private const string SaveDocumentImageSql =
            "UPDATE [Document_Contents] WITH(rowlock) SET [Data] = @Data WHERE [Id] = @Id;\n" +
            "IF @@rowcount = 0 BEGIN\n" +
            "INSERT INTO [Document_Contents] WITH(rowlock) ([Id], [Data])\n" +
            "VALUES (@Id, @Data);\n" +
            "END";

        public void SaveDocImage(Guid docId, string image)
        {
            DataContext.BeginTransaction();
            try
            {
                using (var command = CreateCommand(SaveDocumentImageSql))
                {
                    AddParamWithValue(command, "@Id", docId);
                    AddParamWithValue(command, "@Data", image);

                    command.ExecuteNonQuery();
                }
                /*if (ExistsDocImage(docId))
                {
                    using (var command = CreateCommand(UpdateDocumentImageSql))
                    {
                        AddParamWithValue(command, "@Id", docId);
                        AddParamWithValue(command, "@Data", image);

                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var command = CreateCommand(InsertDocumentImageSql))
                    {
                        AddParamWithValue(command, "@Id", docId);
                        AddParamWithValue(command, "@Data", image);

                        command.ExecuteNonQuery();
                    }
                }*/
                DataContext.Commit();
            }
            catch
            {
                DataContext.Rollback();
                throw;
            }
        }

        private const string HideDocumentSql =
            "UPDATE Documents WITH(rowlock) SET Deleted = 1, Last_Modified = @now WHERE Id = @Id";

        public void Hide(Guid docId)
        {
            DataContext.BeginTransaction();
            try
            {
                using (var command = CreateCommand(HideDocumentSql))
                {
                    AddParamWithValue(command, "@Id", docId);
                    AddParamWithValue(command, "@now", DateTime.Now);

                    command.ExecuteNonQuery();
                }
                DataContext.Commit();
            }
            catch
            {
                DataContext.Rollback();
                throw;
            }
        }

        private const string DeleteDocumentSql =
            "DELETE FROM Documents WHERE Id = @Id";

        public void Delete(Guid docId)
        {
            DataContext.BeginTransaction();
            try
            {
                using (var command = CreateCommand(DeleteDocumentSql))
                {
                    AddParamWithValue(command, "@Id", docId);

                    command.ExecuteNonQuery();
                }
                DataContext.Commit();
            }
            catch
            {
                DataContext.Rollback();
                throw;
            }
        }

        public void WriteDocToTable(DocumentTableMap map, Doc doc)
        {
            using (var command = CreateCommand(""))
            {
                var sqlBuilder = new DocTableMapSqlBuilder(command, map, doc);

                if (sqlBuilder.Build())
                    try
                    {
                        DataContext.BeginTransaction();
                        try
                        {
                            command.ExecuteNonQuery();
                            DataContext.Commit();
                        }
                        catch
                        {
                            DataContext.Rollback();
                            throw;
                        }
                    }
                    catch(Exception)
                    {
                        OutputSqlLog(command.CommandText);
                        throw;
                    }
            }
        }

        private const string SelectDocumentStateListSql = 
            "SELECT [Id], [Created], [State_Type_Id], [Worker_Id]\n" +
            "FROM [Document_States] WITH(NOLOCK)\n" +
            "WHERE [Document_Id] = @Id\n" +
            "ORDER BY [Created]";
        public void FillDocStates(Guid docId, List<DocStateData> docStates)
        {
            using (var command = CreateCommand(SelectDocumentStateListSql))
            {
                AddParamWithValue(command, "@Id", docId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        docStates.Add(
                            new DocStateData
                            {
                                Id = reader.GetGuid(0),
                                Created = reader.GetDateTime(1),
                                StateTypeId = reader.GetGuid(2),
                                UserId = reader.GetGuid(3)
                            });
                    }
                }
            }
        }

        private const string SelectDocumentDefStateTypeSql =
            "SELECT distinct [State_Type_Id]\n" +
            "FROM [Document_States] ds\n" +
            "INNER JOIN [Documents] d on d.[Id] = ds.[Document_Id]\n" +
            "WHERE d.[Def_Id] = @defId";
        public List<Guid> GetDocDefStateTypes(Guid docDefId)
        {
            var list = new List<Guid>();

            using (var command = CreateCommand(SelectDocumentDefStateTypeSql))
            {
                AddParamWithValue(command, "@defId", docDefId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(reader.GetGuid(0));
                    }
                }
            }

            return list;
        }

        private const string DeleteDocumentStatesSql =
            "DELETE FROM Document_States WHERE Document_Id = @Id";
        public void DeleteDocStates(Guid docId)
        {
            DataContext.BeginTransaction();
            try
            {
                using (var command = CreateCommand(DeleteDocumentStatesSql))
                {
                    AddParamWithValue(command, "@Id", docId);

                    command.ExecuteNonQuery();
                }
                DataContext.Commit();
            }
            catch
            {
                DataContext.Rollback();
                throw;
            }
        }

        private IDbCommand CreateCommand(string sql)
        {
            return DataContext.CreateCommand(sql);
        }

        private static void AddParamWithValue(IDbCommand command, string paramName, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = paramName;
            param.Value = value;

            command.Parameters.Add(param);
        }

        public static void OutputSqlLog(string sql)
        {
            try
            {
                using (var writer = new StreamWriter(Logger.GetLogFileName("DocToTableSql"), true))
                {
                    writer.WriteLine("{0}: {1}", DateTime.Now, sql);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}