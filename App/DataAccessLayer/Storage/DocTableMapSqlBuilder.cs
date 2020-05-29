using System;
using System.Data;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Maps;

namespace Intersoft.CISSA.DataAccessLayer.Storage
{
    public class DocTableMapSqlBuilder
    {
        public IDbCommand Command { get; private set; }
        public DocumentTableMap Map { get; private set; }
        public Doc Document { get; private set; }

        public DocTableMapSqlBuilder(IDbCommand command, DocumentTableMap map, Doc doc)
        {
            Command = command;
            Map = map;
            Document = doc;
        }

        public string InsertFields { get; private set; }
        public string UpdateFields { get; private set; }
        public string InsertValues { get; private set; }

        public int ParamIndex { get; private set; }

        private const string InsertOrUpdateSql = "update [{0}] with (serializable) set\n  {1} \n where [Id] = @id \n" +
                                                 "if @@rowcount = 0 \n" +
                                                 "begin \n" +
                                                 " insert [{0}] with(rowlock) ([Id], {2}) values (@id, {3}) \n" +
                                                 "end";

        internal bool Build()
        {
            UpdateFields = "";
            InsertFields = "";
            InsertValues = "";
            ParamIndex = 0;

            var idParam = Command.CreateParameter();
            idParam.ParameterName = "@id";
            idParam.Value = Document.Id;
            Command.Parameters.Add(idParam);
            var i = 0;

            foreach (var field in Map.Fields)
            {
                if (field.AttrDefId == Guid.Empty && field.Type.HasFlag(AttributeFieldType.Data))
                {
                    if (String.Equals(field.FieldName, "Id", StringComparison.OrdinalIgnoreCase))
                    {

                    }
                    else if (String.Equals(field.FieldName, "State", StringComparison.OrdinalIgnoreCase))
                    {
                        if (Document.State != null)
                            BuildUpdateValue(field, Document.State.Type.Id);
                        i++;
                    }
                    else if (String.Equals(field.FieldName, "Created", StringComparison.OrdinalIgnoreCase))
                    {
                        BuildInsertValue(field, Document.CreationTime);
                        i++;
                    }
                    else if (String.Equals(field.FieldName, "Last_Modified", StringComparison.OrdinalIgnoreCase))
                    {
                        BuildInsertValue(field, Document.ModifiedTime);
                        i++;
                    }
                    else if (String.Equals(field.FieldName, "Organization_Id", StringComparison.OrdinalIgnoreCase))
                    {
                        BuildInsertValue(field, Document.OrganizationId);
                        i++;
                    }
                    else if (String.Equals(field.FieldName, "User_Id", StringComparison.OrdinalIgnoreCase))
                    {
                        BuildInsertValue(field, Document.UserId);
                        i++;
                    }
                    else if (String.Equals(field.FieldName, "Def_Id", StringComparison.OrdinalIgnoreCase))
                    {
                        BuildInsertValue(field, Document.DocDef.Id);
                        i++;
                    }
                }
                else if (field.Type.HasFlag(AttributeFieldType.Data))
                {
                    var attrDefId = field.AttrDefId;
                    var attr = Document.Attributes.FirstOrDefault(a => a.AttrDef.Id == attrDefId);

                    if (attr != null)
                    {
                        BuildUpdateValue(field, attr.ObjectValue);
                        i++;
                    }
                }
            }
            Command.CommandText = string.Format(InsertOrUpdateSql, Map.TableName, UpdateFields, InsertFields, InsertValues);
            return i > 0;
        }

        private void BuildInsertValue(AttributeFieldMap field, object value)
        {
            if (InsertFields.Length > 0) InsertFields += ", ";
            InsertFields += String.Format("[{0}]", field.FieldName);
            if (InsertValues.Length > 0) InsertValues += ", ";
            InsertValues += "@" + ParamIndex;
            var param = Command.CreateParameter();
            param.ParameterName = "@" + ParamIndex;
            param.Value = value;
            Command.Parameters.Add(param);
            ParamIndex++;
        }

        private void BuildUpdateValue(AttributeFieldMap field, object value)
        {
            if (UpdateFields.Length > 0) UpdateFields += ", ";
            UpdateFields += String.Format("[{0}] = @{1}", field.FieldName, ParamIndex);

            if (InsertFields.Length > 0) InsertFields += ", ";
            InsertFields += String.Format("[{0}]", field.FieldName);
            if (InsertValues.Length > 0) InsertValues += ", ";
            InsertValues += "@" + ParamIndex;
            var param = Command.CreateParameter();
            param.ParameterName = "@" + ParamIndex;
            if (value == null)
                param.Value = DBNull.Value;
            else if (String.IsNullOrEmpty(value.ToString()))
                param.Value = DBNull.Value;
            else
                param.Value = value;
            Command.Parameters.Add(param);
            ParamIndex++;
        }
    }
}
