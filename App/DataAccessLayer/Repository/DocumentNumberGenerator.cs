using System;
using System.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Context;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class DocumentNumberGenerator : IDocumentNumberGenerator
    {
        private IDataContext DataContext { get; set; }
        public DocumentNumberGenerator(IDataContext dataContext)
        {
            DataContext = dataContext;
        }

        private const string CheckItemExistsSql =
            "SELECT CASE WHEN EXISTS(" +
            "SELECT * FROM [Generators] WHERE [Organization_Id] = @OrgId AND [Document_Def_Id] = @DefId" +
            ") THEN 1 ELSE 0 END AS Expr1;";
        private const string SelectSql =
            "SELECT [Value] FROM [Generators] WHERE [Organization_Id] = @OrgId AND [Document_Def_Id] = @DefId";
        private const string InsertSql =
            "INSERT INTO [Generators] ([Organization_Id], [Document_Def_Id], [Value]) VALUES (@OrgId, @DefId, @Value)";
        private const string UpdateSql =
            "UPDATE [Generators] SET [Value] = @Value WHERE [Organization_Id] = @OrgId AND [Document_Def_Id] = @DefId";


        public long GetNewId(Guid orgId, Guid docDefId)
        {
            lock (GeneratorRepository.Locker)
            {
                DataContext.BeginTransaction();
                try
                {
                    long id = 1;
                    using (var command = DataContext.CreateCommand(SelectSql))
                    {
                        AddParamWithValue(command, "@OrgId", orgId);
                        AddParamWithValue(command, "@DefId", docDefId);

                        var value = command.ExecuteScalar();

                        id += ((value == null ? 0 : (long) value));

                        if (value == null)
                        {
                            using (var newValue = DataContext.CreateCommand(InsertSql))
                            {
                                AddParamWithValue(newValue, "@OrgId", orgId);
                                AddParamWithValue(newValue, "@DefId", docDefId);
                                AddParamWithValue(newValue, "@Value", id);

                                newValue.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (var newValue = DataContext.CreateCommand(UpdateSql))
                            {
                                AddParamWithValue(newValue, "@Value", id);
                                AddParamWithValue(newValue, "@OrgId", orgId);
                                AddParamWithValue(newValue, "@DefId", docDefId);

                                newValue.ExecuteNonQuery();
                            }
                        }
                    }
                    DataContext.Commit();
                    return id;
                }
                catch (Exception e)
                {
                    DataContext.Rollback();
                    Logger.OutputLog(e, "DocumentNumberGenerator.GetNewId");
                    throw;
                }
            }
        }
        private static void AddParamWithValue(IDbCommand command, string paramName, object value)
        {
            var param = command.CreateParameter();
            param.ParameterName = paramName;
            param.Value = value;

            command.Parameters.Add(param);
        }
    }
}