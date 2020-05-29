using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Storage
{
    public enum DocRefSourceType
    {
        All = 0,
        DocAttribute = 1,
        DocListAttribute = 2
    }

    public class AttributeStorage : IAttributeStorage
    {
        public IDataContext DataContext { get; private set; }
//        public SqlConnection Connection { get; private set; }

        public AttributeStorage(IDataContext dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            DataContext = dataContext;
        }
        public AttributeStorage(IAppServiceProvider provider, IDataContext dataContext) : this(dataContext)
        {
            //DataContext = provider.Get<IDataContext>();
        }

        public static readonly DateTime MaxDate = new DateTime(9999, 12, 31);

        public AttributeData Load(Guid docId, AttrDef attrDef)
        {
            return Load(docId, attrDef, MaxDate);
        }

        private const string SelectLastAttrSql =
            "SELECT [Value], [Created] FROM [{0}] WITH(NOLOCK)\n" +
            "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231'";
        private const string SelectLastBlobAttrSql =
            "SELECT [Value], [Created], [File_Name] FROM [{0}] WITH(NOLOCK)\n" +
            "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231'";
        private const string SelectDateAttrSql =
            "SELECT [Value], [Created], [Expired] FROM [{0}] WITH(NOLOCK)\n" +
            "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Created] <= @ForDate AND [Expired] > @ForDate";
        private const string SelectDateBlobAttrSql =
            "SELECT [Value], [Created], [Expired], [File_Name] FROM [{0}] WITH(NOLOCK)\n" +
            "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Created] <= @ForDate AND [Expired] > @ForDate";

        public AttributeData Load(Guid docId, AttrDef attrDef, DateTime forDate)
        {
            if (attrDef == null)
                return null;

            var tableName = GetAttributeTableName((CissaDataType) attrDef.Type.Id);

            IDbCommand command;

            if (forDate == MaxDate)
            {
                var sqlTemplate = (attrDef.Type.Id == (short) CissaDataType.Blob)
                    ? SelectLastBlobAttrSql
                    : SelectLastAttrSql;

                using (command = CreateCommand(String.Format(sqlTemplate, tableName)))
                {
                    AddParamWithValue(command, "@DocId", docId);
                    AddParamWithValue(command, "@DefId", attrDef.Id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new AttributeData
                            {
                                Created = reader.GetDateTime(1),
                                Value = reader.GetValue(0),
                                Expired = MaxDate,
                                Value2 = attrDef.Type.Id == (short) CissaDataType.Blob ? reader.GetString(2) : String.Empty
                            };
                        }
                        return null;
                    }
                }
            }

            var sqlDateTemplate = (attrDef.Type.Id == (short)CissaDataType.Blob)
                ? SelectDateBlobAttrSql
                : SelectDateAttrSql;
            using (command = CreateCommand(String.Format(sqlDateTemplate, tableName)))
            {
                AddParamWithValue(command, "@DocId", docId);
                AddParamWithValue(command, "@DefId", attrDef.Id);
                AddParamWithValue(command, "@ForDate", forDate);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new AttributeData
                        {
                            Created = reader.GetDateTime(1),
                            Value = reader.GetValue(0),
                            Expired = reader.GetDateTime(2),
                            Value2 = attrDef.Type.Id == (short)CissaDataType.Blob ? reader.GetString(3) : String.Empty
                        };
                    }
                    return null;
                }
            }
        }

        public List<Guid> LoadDocList(Guid docId, AttrDef attrDef, int limit = 0)
        {
            return LoadDocList(docId, attrDef, MaxDate, limit);
        }

        private const string SelectLastListAttrSql =
            "SELECT {0} [Value] FROM [{1}] WITH(NOLOCK)\n" +
            "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231'";
        private const string SelectDateListAttrSql =
            "SELECT {0} [Value] FROM [{1}]  WITH(NOLOCK)\n" +
            "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Created] <= @ForDate AND [Expired] > @ForDate";

        public List<Guid> LoadDocList(Guid docId, AttrDef attrDef, DateTime forDate, int limit = 0)
        {
            var tableName = GetAttributeTableName(CissaDataType.DocList);

            IDbCommand command;
            var limitParam = limit > 0 ? "TOP " + limit : String.Empty;

            if (forDate == MaxDate)
            {
                using (command = CreateCommand(String.Format(SelectLastListAttrSql, limitParam, tableName)))
                {
                    AddParamWithValue(command, "@DocId", docId);
                    AddParamWithValue(command, "@DefId", attrDef.Id);

                    return ReadDocListValues(command);
                }
            }

            using (command = CreateCommand(String.Format(SelectDateListAttrSql, limitParam, tableName)))
            {
                AddParamWithValue(command, "@DocId", docId);
                AddParamWithValue(command, "@DefId", attrDef.Id);
                AddParamWithValue(command, "@ForDate", forDate);

                return ReadDocListValues(command);
            }
        }

        private static List<Guid> ReadDocListValues(IDbCommand command)
        {
            var list = new List<Guid>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetGuid(0);
                    list.Add(id);
                }
            }

            return list;
        }

        protected const string SelectDocumentAttributeSql =
            "SELECT 1 AS AttrType, " +
            "Value AS Int_Value, " + // 1
            "null AS Currency_Type, " + // 2
            "null AS Text_Type, " + // 3
            "null AS Float_Type, " + // 4
            "null AS Guid_Type, " + // 5
            "null AS Bool_Type, " + // 6
            "null AS Date_Type, Created, Def_Id " + // 7
            "FROM Int_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 2, null, Value, null, null, null, null, null, Created, Def_Id " +
            "FROM Currency_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 3, null, null, Value, null, null, null, null, Created, Def_Id " +
            "FROM Text_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 4, null, null, null, Value, null, null, null, Created, Def_Id " +
            "FROM Float_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 5, null, null, null, null, Value, null, null, Created, Def_Id " +
            "FROM Enum_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 6, null, null, null, null, Value, null, null, Created, Def_Id " +
            "FROM Document_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 8, null, null, null, null, null, Value, null, Created, Def_Id " +
            "FROM Boolean_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 9, null, null, null, null, null, null, Value, Created, Def_Id " +
            "FROM Date_Time_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 12, null, null, null, null, Value, null, null, Created, Def_Id " +
            "FROM Org_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 13, null, null, null, null, Value, null, null, Created, Def_Id " +
            "FROM Doc_State_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 14, null, null, null, null, Value, null, null, Created, Def_Id " +
            "FROM Object_Def_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n" +
            "UNION ALL SELECT 15, null, null, File_Name, null, null, null, null, Created, Def_Id " +
            "FROM Image_Attributes WITH(NOLOCK) WHERE Document_Id = @Id AND Expired = '99991231'\n";

        public IEnumerable<AttributeData> LoadAll(Guid docId)
        {
            using (var command = CreateCommand(SelectDocumentAttributeSql))
            {
                AddParamWithValue(command, "@Id", docId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var attrData = new AttributeData
                        {
                            Created = reader.GetDateTime(8),
                            DataType = (int) reader.GetValue(0),
                            Expired = MaxDate,
                            DefId = reader.GetGuid(9)
                        };
                        switch (attrData.DataType)
                        {
                            case 1:
                                if (!reader.IsDBNull(1)) attrData.Value = reader.GetInt32(1);
                                break;
                            case 2:
                                if (!reader.IsDBNull(2)) attrData.Value = reader.GetDecimal(2);
                                break;
                            case 3:
                            case 15:
                                if (!reader.IsDBNull(3)) attrData.Value = reader.GetString(3);
                                break;
                            case 4:
                                if (!reader.IsDBNull(4)) attrData.Value = reader.GetDouble(4);
                                break;
                            case 5:
                            case 6:
                            case 12:
                            case 13:
                            case 14:
                                if (!reader.IsDBNull(5)) attrData.Value = reader.GetGuid(5);
                                break;
                            case 8:
                                if (!reader.IsDBNull(6)) attrData.Value = reader.GetBoolean(6);
                                break;
                            case 9:
                                if (!reader.IsDBNull(7)) attrData.Value = reader.GetDateTime(7);
                                break;
                        }
                        yield return attrData;
                    }
                }
            }
        }

        private const string ExpiryAttrSql =
            "UPDATE [{0}] WITH(rowlock) SET [Expired] = @Expired\n" +
            "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231'";

        private const string RemoveDocListAttrSql =
            "UPDATE [{0}] WITH(rowlock) SET [Expired] = @Expired\n" +
            "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231' AND [Value] = @Value";

        private const string CheckItemExistsSql =
            "SELECT CASE WHEN EXISTS(" +
            "SELECT * FROM [{0}] WITH(NOLOCK) WHERE " +
            "[Document_Id] = @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231' AND [Value] = @Value" +
            ") THEN 1 ELSE 0 END AS Expr1;";

        private const string InsertIfNotExistsAttrSql =
            "IF NOT EXISTS(SELECT * FROM [DocumentList_Attributes] WITH(NOLOCK) WHERE " +
            "[Document_Id] = @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231' AND [Value] = @Value)\n" +
            "INSERT INTO [DocumentList_Attributes] WITH(rowlock) ([Document_Id], [Def_Id], [Created], [Expired], [Value], [UserId])\n" +
            "VALUES(@DocId, @DefId, @Created, '99991231', @Value, @UserId)";

        public void Save(Guid docId, AttributeBase attr, Guid userId)
        {
            Save(docId, attr, userId, DateTime.Now);
        }

        public void Save(Guid docId, AttributeBase attr, Guid userId, DateTime date)
        {
            if (attr.AttrDef.Type.Id == (short) CissaDataType.DocList)
            {
                var docListAttr = (DocListAttribute) attr;

                if (docListAttr.AddedDocIds != null)
                    foreach (var itemId in docListAttr.AddedDocIds)
                    {
                        AddDocListItem(docId, attr, itemId, userId, date);
                    }
                docListAttr.AddedDocIds = null;

                if (docListAttr.AddedDocs != null)
                    foreach (var doc in docListAttr.AddedDocs)
                    {
                        AddDocListItem(docId, attr, doc.Id, userId, date);
                    }
                docListAttr.AddedDocs = null;
            }
            else if (attr.AttrDef.Type.Id == (short) CissaDataType.Blob && attr is BlobAttribute)
            {
                var blobAttr = attr as BlobAttribute;
                SaveBlob(docId, attr.AttrDef.Id, blobAttr.Value, blobAttr.FileName, userId, date);
            }
            else
                SimpleSave(docId, attr, userId, date);
        }

        public void DirectSave(Guid docId, AttrDef attrDef, object value, Guid userId, DateTime date, string description)
        {
            if (attrDef.Type.Id == (short)CissaDataType.DocList)
            {
                var doc = value as Doc;
                if (doc != null)
                    AddDocListItem(docId, attrDef.Id, doc.Id, userId, date);
                else if (value is Guid)
                {
                    AddDocListItem(docId, attrDef.Id, (Guid) value, userId, date);
                }
            }
            else if (attrDef.Type.Id == (short) CissaDataType.Blob)
            {
                SaveBlob(docId, attrDef.Id, (byte[]) value, description, userId, date);
            }
            else
                SimpleSave(docId, attrDef, value, userId, date);
        }

        private const string SaveBlobAttrSql =
            "UPDATE [{0}] WITH(rowlock) SET [Expired] = @Created\n" +
            "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231';\n" +
            "INSERT INTO [{0}] WITH(rowlock) ([Document_Id], [Def_Id], [Created], [Expired], [Value], [UserId], [File_Name])\n" +
            "VALUES(@DocId, @DefId, @Created, '99991231', @Value, @UserId, @FileName);";

        private void SaveBlob(Guid docId, Guid attrDefId, byte[] value, string fileName, Guid userId, DateTime date)
        {
            var tableName = CissaDataTypeHelper.GetTableName(CissaDataType.Blob);

            DataContext.BeginTransaction();
            try
            {
                if (value != null)
                    using (var command = CreateCommand(String.Format(SaveBlobAttrSql, tableName)))
                    {
                        AddParamWithValue(command, "@DocId", docId);
                        AddParamWithValue(command, "@DefId", attrDefId);
                        AddParamWithValue(command, "@Created", date);
                        AddParamWithValue(command, "@Value", value);
                        AddParamWithValue(command, "@UserId", userId);
                        AddParamWithValue(command, "@FileName", fileName);
                        command.ExecuteNonQuery();
                    }
                else
                    using (var command = CreateCommand(String.Format(ExpiryAttrSql, tableName)))
                    {
                        AddParamWithValue(command, "@DocId", docId);
                        AddParamWithValue(command, "@DefId", attrDefId);
                        AddParamWithValue(command, "@Expired", date);
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

        private const string SaveAttrSql =
            "UPDATE [{0}] WITH(rowlock) SET [Expired] = @Created\n" +
            "WHERE [Document_Id] = @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231';\n" +
            "INSERT INTO [{0}] WITH(rowlock) ([Document_Id], [Def_Id], [Created], [Expired], [Value], [UserId])\n" +
            "VALUES(@DocId, @DefId, @Created, '99991231', @Value, @UserId);";

        protected void SimpleSave(Guid docId, AttributeBase attr, Guid userId, DateTime date)
        {
            var tableName = GetAttributeTableName(attr);

            DataContext.BeginTransaction();
            try
            {

                var attribute = attr as TextAttribute;
                var emptyTextAttr = attribute != null && String.IsNullOrEmpty(attribute.Value);

                if (attr.ObjectValue != null && !emptyTextAttr)
                    using (var command = CreateCommand(String.Format(SaveAttrSql, tableName)))
                    {
                        AddParamWithValue(command, "@DocId", docId);
                        AddParamWithValue(command, "@DefId", attr.AttrDef.Id);
                        //AddParamWithValue(command, "@Expired", date);
                        AddParamWithValue(command, "@Created", date);
                        if (attribute != null)
                            AddParamWithValue(command, "@Value", attr.ObjectValue, SqlDbType.NVarChar);
                        else
                            AddParamWithValue(command, "@Value", attr.ObjectValue);
                        AddParamWithValue(command, "@UserId", userId);
                        command.ExecuteNonQuery();
                    }
                else
                    using (var command = CreateCommand(String.Format(ExpiryAttrSql, tableName)))
                    {
                        AddParamWithValue(command, "@DocId", docId);
                        AddParamWithValue(command, "@DefId", attr.AttrDef.Id);
                        AddParamWithValue(command, "@Expired", date);
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

        protected void SimpleSave(Guid docId, AttrDef attrDef, object value, Guid userId, DateTime date)
        {
            var tableName = CissaDataTypeHelper.GetTableName(attrDef.Type.Id);

            DataContext.BeginTransaction();
            try
            {
                var emptyText = value != null && attrDef.Type.Id == (short) CissaDataType.Text &&
                                String.IsNullOrEmpty(value.ToString());

                if (value != null && !emptyText)
                {
                    using (var command = CreateCommand(String.Format(SaveAttrSql, tableName)))
                    {
                        AddParamWithValue(command, "@DocId", docId);
                        AddParamWithValue(command, "@DefId", attrDef.Id);
                        AddParamWithValue(command, "@Created", date);
                        if (attrDef.Type.Id == (short) CissaDataType.Text)
                            AddParamWithValue(command, "@Value", value, SqlDbType.NVarChar);
                        else
                            AddParamWithValue(command, "@Value", value);
                        AddParamWithValue(command, "@UserId", userId);
                        command.ExecuteNonQuery();
                    }
                }
                else
                    using (var command = CreateCommand(String.Format(ExpiryAttrSql, tableName)))
                    {
                        AddParamWithValue(command, "@DocId", docId);
                        AddParamWithValue(command, "@DefId", attrDef.Id);
                        AddParamWithValue(command, "@Expired", date);
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
        public void RemoveDocListItem(Guid docId, AttributeBase attr, Guid itemDocId, DateTime date)
        {
            var tableName = GetAttributeTableName(attr);

            DataContext.BeginTransaction();
            try
            {
                using (var command = CreateCommand(String.Format(RemoveDocListAttrSql, tableName)))
                {
                    AddParamWithValue(command, "@DocId", docId);
                    AddParamWithValue(command, "@DefId", attr.AttrDef.Id);
                    AddParamWithValue(command, "@Expired", date);
                    AddParamWithValue(command, "@Value", itemDocId);
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

        public void RemoveDocListItem(Guid docId, Guid attrDefId, Guid itemDocId, DateTime date)
        {
            var tableName = GetAttributeTableName(CissaDataType.DocList);

            DataContext.BeginTransaction();
            try
            {
                using (var command = CreateCommand(String.Format(RemoveDocListAttrSql, tableName)))
                {
                    AddParamWithValue(command, "@DocId", docId);
                    AddParamWithValue(command, "@DefId", attrDefId);
                    AddParamWithValue(command, "@Expired", date);
                    AddParamWithValue(command, "@Value", itemDocId);
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

        public void AddDocListItem(Guid docId, AttributeBase attr, Guid itemDocId, Guid userId, DateTime date)
        {
            {

                DataContext.BeginTransaction();
                try
                {
                    using (var command = CreateCommand(InsertIfNotExistsAttrSql))
                    {
                        AddParamWithValue(command, "@DocId", docId);
                        AddParamWithValue(command, "@DefId", attr.AttrDef.Id);
                        AddParamWithValue(command, "@Created", date);
                        AddParamWithValue(command, "@Value", itemDocId);
                        AddParamWithValue(command, "@UserId", userId);
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
        }

        public void AddDocListItem(Guid docId, Guid attrDefId, Guid itemDocId, Guid userId, DateTime date)
        {
//            if (!ExistsDocInList(docId, attrDefId, itemDocId))
            {
//                var tableName = GetAttributeTableName(CissaDataType.DocList);

                DataContext.BeginTransaction();
                try
                {
                    using (var command = CreateCommand(InsertIfNotExistsAttrSql))
                    {
                        AddParamWithValue(command, "@DocId", docId);
                        AddParamWithValue(command, "@DefId", attrDefId);
                        AddParamWithValue(command, "@Created", date);
                        AddParamWithValue(command, "@Value", itemDocId);
                        AddParamWithValue(command, "@UserId", userId);
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
        }

        public bool ExistsDocInList(Guid docId, AttributeBase attr, Guid itemDocId)
        {
            return ExistsDocInList(docId, attr.AttrDef.Id, itemDocId);
        }

        public bool ExistsDocInList(Guid docId, Guid attrDefId, Guid itemDocId)
        {
            var tableName = GetAttributeTableName(CissaDataType.DocList);

            using (var command = CreateCommand(String.Format(CheckItemExistsSql, tableName)))
            {
                AddParamWithValue(command, "@DocId", docId);
                AddParamWithValue(command, "@DefId", attrDefId);
                AddParamWithValue(command, "@Value", itemDocId);

                var result = (int) command.ExecuteScalar();
                return result == 1;
            }
        }

        private const string ClearDocListSql =
            "UPDATE [DocumentList_Attributes] WITH(rowlock) SET " +
            "[Expired] = @now " +
            "WHERE [Document_Id] = @docId AND [Def_Id] = @defId AND [Expired] = '99991231'";

        public void ClearDocList(Guid docId, Guid attrDefId, DateTime time = new DateTime())
        {
            var clearTime = time == DateTime.MinValue ? DateTime.Now : time;

            DataContext.BeginTransaction();
            try
            {
                using (var command = CreateCommand(ClearDocListSql))
                {
                    AddParamWithValue(command, "@now", clearTime);
                    AddParamWithValue(command, "@docId", docId);
                    AddParamWithValue(command, "@defId", attrDefId);

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

        private const string ExistsDocAttrRefSql =
            "SELECT CASE WHEN EXISTS(" +
            "SELECT da.* " +
            "FROM Document_Attributes da WITH(NOLOCK) INNER JOIN Documents d WITH(NOLOCK) ON d.Id = da.Document_Id " +
            "WHERE da.Expired = '99991231' AND da.Value = @DocId AND (d.Deleted IS NULL OR d.Deleted = 0) " +
            ") THEN 1 ELSE 0 END AS Expr1;";
        private const string ExistsDocListAttrRefSql =
            "SELECT CASE WHEN EXISTS(" +
            "SELECT da.* " +
            "FROM DocumentList_Attributes da WITH(NOLOCK) INNER JOIN Documents d WITH(NOLOCK) ON d.Id = da.Document_Id " +
            "WHERE da.Expired = '99991231' AND da.Value = @DocId AND (d.Deleted IS NULL OR d.Deleted = 0) " +
            ") THEN 1 ELSE 0 END AS Expr1;";

        public bool HasRefToDoc(Guid docId)
        {
            using (var command = CreateCommand(ExistsDocAttrRefSql))
            {
                AddParamWithValue(command, "@DocId", docId);

                var result = (int)command.ExecuteScalar();
                if (result == 1) return true;
            }
            using (var command = CreateCommand(ExistsDocListAttrRefSql))
            {
                AddParamWithValue(command, "@DocId", docId);

                var result = (int)command.ExecuteScalar();
                return result == 1;
            }
        }

        private const string SelectDocumentRefSql =
            @"SELECT da.Document_Id, d.Def_Id, da.Def_Id, da.Created, da.UserId " +
            "FROM Document_Attributes da WITH(NOLOCK) " +
            "JOIN Documents d WITH(NOLOCK) ON d.Id = da.Document_Id " +
            "WHERE da.Expired = '99991231' AND da.Value = @docId";

        private const string SelectDocumentListRefSql =
            "SELECT dla.Document_Id, d2.Def_Id, dla.Def_Id, dla.Created, dla.UserId " +
            "FROM DocumentList_Attributes dla WITH(NOLOCK) " +
            "JOIN Documents d2 WITH(NOLOCK) ON d2.Id = dla.Document_Id " +
            "WHERE dla.Expired = '99991231' AND dla.Value = @docId";

        public List<DocRef> GetDocRefs(Guid docId, DocRefSourceType sourceType = DocRefSourceType.All)
        {
            var list = new List<DocRef>();

            using (var command = CreateCommand(SelectDocumentRefSql))
            {
                AddParamWithValue(command, "@docId", docId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(
                            new DocRef
                            {
                                DocumentId = reader.GetGuid(0),
                                DocumentDefId = reader.GetGuid(1),
                                AttributeDefId = reader.GetGuid(2),
                                Created = reader.GetDateTime(3),
                                UserId = reader.GetGuid(4),
                                IsList = false
                            });
                    }
                }
            }
            using (var command = CreateCommand(SelectDocumentListRefSql))
            {
                AddParamWithValue(command, "@docId", docId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(
                            new DocRef
                            {
                                DocumentId = reader.GetGuid(0),
                                DocumentDefId = reader.GetGuid(1),
                                AttributeDefId = reader.GetGuid(2),
                                Created = reader.GetDateTime(3),
                                UserId = reader.GetGuid(4),
                                IsList = true
                            });
                    }
                }
            }
            return list;
        }

        private const string DeleteDocAttributesSql =
            "delete from DocumentList_Attributes where Document_Id = @docId \n" +
            "delete from Text_Attributes where Document_Id = @docId \n" +
            "delete from Enum_Attributes where Document_Id = @docId \n" +
            "delete from Int_Attributes where Document_Id = @docId \n" +
            "delete from Document_Attributes where Document_Id = @docId \n" +
            "delete from Currency_Attributes where Document_Id = @docId \n" +
            "delete from Date_Time_Attributes where Document_Id = @docId \n" +
            "delete from Boolean_Attributes where Document_Id = @docId \n" +
            "delete from Float_Attributes where Document_Id = @docId \n" +
            "delete from Org_Attributes where Document_Id = @docId \n" +
            "delete from Doc_State_Attributes where Document_Id = @docId \n" +
            "delete from Blob_Attributes where Document_Id = @docId \n" +
            "delete from Image_Attributes where Document_Id = @docId \n" +
            "delete from Object_Def_Attributes where Document_Id = @docId \n";

        public void DeleteAttributes(Guid docId)
        {
            DataContext.BeginTransaction();
            try
            {
                using (var command = CreateCommand(DeleteDocAttributesSql))
                {
                    AddParamWithValue(command, "@docId", docId);

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

        private const string CheckUniquenessSql = 
            "SELECT CASE WHEN EXISTS(" +
            "SELECT * FROM [{0}] WITH(NOLOCK) WHERE " +
            "[Document_Id] <> @DocId AND [Def_Id] = @DefId AND [Expired] = '99991231' AND [Value] = @Value" +
            ") THEN 1 ELSE 0 END AS Expr1;";

        public bool CheckUniqueness(Guid docId, AttrDef attrDef, object value)
        {
            var tableName = GetAttributeTableName((CissaDataType) attrDef.Type.Id);

            using (var command = CreateCommand(String.Format(CheckUniquenessSql, tableName)))
            {
                AddParamWithValue(command, "@DocId", docId);
                AddParamWithValue(command, "@DefId", attrDef.Id);
                AddParamWithValue(command, "@Value", value);

                var result = (int)command.ExecuteScalar();
                if (result == 0) return true;
            }
            return false;
        }

        private static string GetAttributeTableName(AttributeBase attr)
        {
            return GetAttributeTableName((CissaDataType) attr.AttrDef.Type.Id);
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

        private static void AddParamWithValue(IDbCommand command, string paramName, object value, SqlDbType type)
        {
            var param = new SqlParameter(paramName, type) {Value = value};

            command.Parameters.Add(param);
        }

        private static string GetAttributeTableName(CissaDataType dataType)
        {
            var tableName = CissaDataTypeHelper.GetTableName(dataType);

            if (String.IsNullOrEmpty(tableName))
                throw new ApplicationException("Атрибут не является сохраняемым!");
            return tableName;
        }
    }
}