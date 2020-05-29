using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryField
    {
        public int Index { get; internal set; }
        public Guid AttributeId { get; internal set; }
        public bool IsIdent { get; internal set; }
        public string AttributeName { get; internal set; }
        public AttrDef AttrDef { get; internal set; }
        public SystemIdent Ident { get; internal set; }
        public bool IsExp { get; internal set; }
        public SqlQuerySelectAttribute SelectAttribute { get; internal set; }
        public Guid DocDefId { get; internal set; }
        public DocDef DocDef { get; internal set; }
        public BaseDataType DataType { get; internal set; }
    }

    public class SqlQueryReader : IDisposable
    {
        // public string ConnectionString { get; private set; }
        public SqlQuery Query { get; private set; }

        private readonly IDataContext _dataContext;

        public IDataContext DataContext
        {
            get
            {
                var mdc = _dataContext as IMultiDataContext;
                return mdc != null ? mdc.GetDocumentContext : _dataContext;
            }
        }

        // private readonly bool _ownDataContext;
        private DbConnection Connection { get; set; }
        // private bool _ownConnection;

        /*public SqlQueryReader(string connectionString, SqlQuery query)
        {
            ConnectionString = connectionString;
            Connection = new SqlConnection(connectionString);
            _ownConnection = true;
            DataContext = new DataContext(Connection);
            _ownDataContext = true;
            Query = query;
        }

        public SqlQueryReader(DbConnection connection, SqlQuery query)
        {
            Connection = connection;
            DataContext = new DataContext(connection);
            _ownDataContext = true;
            Query = query;
        }*/

        public SqlQueryReader(IAppServiceProvider provider, SqlQuery query)
        {
            _dataContext = provider.Get<IDataContext>();
            Connection = DataContext.StoreConnection;
            Query = query;
        }

        public SqlQueryReader(IDataContext dataContext, SqlQuery query)
        {
            _dataContext = dataContext;
            Connection = DataContext.StoreConnection;
            Query = query;
        }

        public SqlQueryReader(SqlQuery query) : this(query.DataContext, query) {}

        // Устаревшие методы
        /*public SqlQueryReader(DbConnection connection, QueryBuilder builder, string[] attrs)
        {
            Connection = connection;
            DataContext = new DataContext(connection);
            _ownDataContext = true;
            var sqb = SqlQueryBuilderFactory
            Query = SqlQueryBuilder.Build(DataContext, builder.Def);
            Query.AddAttributes(attrs);
        }

        public SqlQueryReader(string connectionString, QueryBuilder builder, string[] attrs)
        {
            ConnectionString = connectionString;
            Connection = new SqlConnection(connectionString);
            _ownConnection = true;
            DataContext = new DataContext(Connection);
            _ownDataContext = true;
            Query = SqlQueryBuilder.Build(DataContext, builder.Def);
            Query.AddAttributes(attrs);
        }

        public SqlQueryReader(IDataContext dataContext, QueryBuilder builder, string[] attrs)
        {
            DataContext = dataContext;
            Connection = dataContext.StoreConnection;
            Query = SqlQueryBuilder.Build(DataContext, builder.Def);
            Query.AddAttributes(attrs);
        }

        public SqlQueryReader(QueryBuilder builder, string[] attrs) : this(GetDefaultConnectionString(), builder, attrs) {}

        public SqlQueryReader(DbConnection connection, QueryDef def, string[] attrs)
        {
            Connection = connection;
            DataContext = new DataContext(Connection);
            _ownDataContext = true;
            Query = SqlQueryBuilder.Build(DataContext, def);
            Query.AddAttributes(attrs);
        }

        public SqlQueryReader(string connectionString, QueryDef def, string[] attrs)
        {
            ConnectionString = connectionString;
            Connection = new SqlConnection(connectionString);
            _ownConnection = true;
            DataContext = new DataContext(Connection);
            _ownDataContext = true;
            Query = SqlQueryBuilder.Build(DataContext, def);
            Query.AddAttributes(attrs);
        }*/

        /*public SqlQueryReader(IDataContext dataContext, QueryDef def, string[] attrs)
        {
            Connection = dataContext.StoreConnection;
            DataContext = new DataContext(Connection);
            _ownDataContext = true;
            Query = SqlQueryBuilder.Build(DataContext, def);
            Query.AddAttributes(attrs);
        }

        public SqlQueryReader(QueryDef def, string[] attrs) : this(GetDefaultConnectionString(), def, attrs) {}*/

        /*public static string GetDefaultConnectionString() // Устарело
        {
            using (var context = new cissaEntities())
            {
                var entityConnection = context.Connection as EntityConnection;
                if (entityConnection != null)
                    return entityConnection.StoreConnection.ConnectionString;
            }
            throw new ApplicationException("Не могу получить строку подключения!");
        }*/

        public IDataReader Reader { get; private set; }

        private string _recordNoFieldName = "__recNo";
        private int _recortdNo = 0;
        public int RecordNo { get { return _recortdNo; } }

        public string RecordNoFieldName
        {
            get { return _recordNoFieldName; }
            set { _recordNoFieldName = value; }
        }

        private readonly List<SqlQueryField> _fields = new List<SqlQueryField>();
        public IReadOnlyList<SqlQueryField> Fields { get { return _fields; } }

        private readonly IDictionary<SqlQueryMasterSlaveLink, SqlQueryReader> _masterLinks = new Dictionary<SqlQueryMasterSlaveLink, SqlQueryReader>();

        public SqlQueryMasterSlaveLink AddMasterLink(SqlQueryReader master, Guid masterAttrId, string paramName)
        {
            var link = new SqlQueryMasterSlaveLink(Query, masterAttrId, paramName);
            _masterLinks.Add(link, master);
            return link;
        }

        public SqlQueryMasterSlaveLink AddMasterLink(SqlQueryReader master, string masterAttrName, string paramName)
        {
            var link = new SqlQueryMasterSlaveLink(Query, masterAttrName, paramName);
            _masterLinks.Add(link, master);
            return link;
        }

        private void PrepareConnection()
        {
            /*if (Connection == null)  // Устарело
            {
                Connection = new SqlConnection(ConnectionString);
                _ownConnection = true;
            }*/
        }

        public bool Active { get { return Reader != null; } }

        public void Open()
        {
            SqlBuilder sql = null;
            try
            {
                PrepareConnection();

                PrepareParams();
                sql = Query.BuildSql();
                //OutputSqlLog(sql.ToString());
                if (!Connection.State.HasFlag(ConnectionState.Open)) Connection.Open();
                PrepareFields();

                //var command = new SqlCommand(sql.ToString(), (SqlConnection) Connection) {CommandTimeout = 600};
                var command = DataContext.CreateCommand(sql.ToString());
                //command.CommandTimeout = 600;

                Reader = command.ExecuteReader();
                _recortdNo = 0;
            }
            catch(Exception e)
            {
                OutputLog(e, "On Open", sql != null ? sql.ToString() : String.Empty);
                throw;
            }
        }

        private void PrepareFields()
        {
            _fields.Clear();
            var i = 0;
            foreach (var attr in Query.Attributes)
            {
                _fields.Add(
                    new SqlQueryField
                    {
                        Index = i,
                        SelectAttribute = attr,
                        AttrDef = attr.Def,
                        AttributeId = attr.Def != null ? attr.Def.Id : Guid.Empty,
                        AttributeName = attr.Def != null ? attr.Def.Name : SystemIdentConverter.Convert(attr.Ident),
                        IsIdent = attr.IsSystemIdent,
                        Ident = attr.IsSystemIdent ? attr.Ident : SystemIdent.Id,
                        IsExp = attr.IsMultiExpression,
                        DocDef = attr.Attributes[0].Source != null ? attr.Attributes[0].Source.GetDocDef() : null,
                        DocDefId =
                            attr.Attributes[0].Source != null ? attr.Attributes[0].Source.GetDocDef().Id : Guid.Empty,
                        DataType = attr.IsSystemIdent
                            ? SystemIdentConverter.ToBaseType(attr.Ident)
                            : attr.Def != null
                                ? CissaDataTypeHelper.ConvertToBase(attr.Def.Type.Id)
                                : BaseDataType.Unknown
                    });
                i++;
            }
        }

        public void Close()
        {
            if (Reader != null)
            {
                Reader.Close();
                Reader.Dispose();
                Reader = null;
            }
        }

        public void Dispose()
        {
            Close();
            /*if (_ownConnection && Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
            if (_ownDataContext && DataContext != null)
            {
                DataContext.Dispose();
                DataContext = null;
            }*/
        }

        public int GetCount()
        {
            string sql = String.Empty;
            try
            {
                PrepareParams();
                sql = Query.BuildCountSqlScript();

                PrepareConnection();
                using (var command = DataContext.CreateCommand(sql)) //new SqlCommand(sql, (SqlConnection) Connection) {CommandTimeout = 600};
                {
                    if (Connection.State == ConnectionState.Closed) Connection.Open();

                    var count = command.ExecuteScalar();
                    if (count != null) return (int) count;
                }
                return 0;
            }
            catch (Exception e)
            {
                OutputLog(e, "GetCount", sql);
                throw;
            }
        }

        public object GetSum(string attrDefName)
        {
            string sql = String.Empty;
            try
            {
                PrepareParams();
                sql = Query.BuildSumSqlScript(attrDefName);

                PrepareConnection();
                using (var command = DataContext.CreateCommand(sql)) //new SqlCommand(sql, (SqlConnection)Connection) { CommandTimeout = 600 };
                {
                    if (Connection.State == ConnectionState.Closed) Connection.Open();

                    return command.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                OutputLog(e, "GetSum", sql);
                throw;
            }
        }

        public object GetSum(Guid attrDefId)
        {
            string sql = String.Empty;
            try
            {
                PrepareParams();
                sql = Query.BuildSumSqlScript(attrDefId);

                PrepareConnection();
                using (var command = DataContext.CreateCommand(sql)) //new SqlCommand(sql, (SqlConnection)Connection) { CommandTimeout = 600 };
                {
                    if (Connection.State == ConnectionState.Closed) Connection.Open();

                    return command.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                OutputLog(e, "GetSum", sql);
                throw;
            }
        }

        public object GetMax(string attrDefName)
        {
            string sql = String.Empty;
            try
            {
                PrepareParams();
                sql = Query.BuildMaxSqlScript(attrDefName);

                PrepareConnection();
                using (var command = DataContext.CreateCommand(sql)) //new SqlCommand(sql, (SqlConnection) Connection) {CommandTimeout = 600};
                {
                    if (Connection.State == ConnectionState.Closed) Connection.Open();

                    return command.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                OutputLog(e, "GetMax", sql);
                throw;
            }
        }

        public object GetMin(string attrDefName)
        {
            string sql = String.Empty;
            try
            {
                PrepareParams();
                sql = Query.BuildMinSqlScript(attrDefName);

                PrepareConnection();
                using (var command = DataContext.CreateCommand(sql)) //new SqlCommand(sql, (SqlConnection) Connection) {CommandTimeout = 600};
                {
                    if (Connection.State == ConnectionState.Closed) Connection.Open();

                    return command.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                OutputLog(e, "GetMin", sql);
                throw;
            }
        }

        public object GetMax(Guid attrDefId)
        {
            string sql = String.Empty;
            try
            {
                PrepareParams();
                sql = Query.BuildMaxSqlScript(attrDefId);

                PrepareConnection();
                using (var command = DataContext.CreateCommand(sql)) // new SqlCommand(sql, (SqlConnection) Connection) {CommandTimeout = 600};
                {
                    if (Connection.State == ConnectionState.Closed) Connection.Open();

                    return command.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                OutputLog(e, "GetMax", sql);
                throw;
            }
        }

        public object GetMin(Guid attrDefId)
        {
            string sql = String.Empty;
            try
            {
                PrepareParams();
                sql = Query.BuildMinSqlScript(attrDefId);

                PrepareConnection();
                using (var command = DataContext.CreateCommand(sql)) // new SqlCommand(sql, (SqlConnection) Connection) {CommandTimeout = 600};
                {
                    if (Connection.State == ConnectionState.Closed) Connection.Open();

                    return command.ExecuteScalar();
                }
            }
            catch (Exception e)
            {
                OutputLog(e, "GetMin", sql);
                throw;
            }
        }

        private IDataReader CheckReader()
        {
            if (Reader == null)
                throw new ApplicationException("Не могу прочитать данные запроса!");
            return Reader;
        }

        public bool Read()
        {
            try
            {
                if (Reader == null) Open();

                if (Reader != null)
                {
                    if (Reader.Read())
                    {
                        _recortdNo++;
                        return true;
                    }
                    return false;
                }

                throw new ApplicationException("Не могу прочитать данные запроса!");
            }
            catch (Exception e)
            {
                OutputLog(e, "Read", Query.BuildSql().ToString());
                throw;
            }
        }

        public int TryGetAttributeIndex(Guid attrDefId)
        {
            var attr = Query.Attributes.FirstOrDefault(a => a.Def != null && a.Def.Id == attrDefId);

            if (attr == null) return -1;

            return Query.Attributes.IndexOf(attr);
        }

        public SqlQueryJoin TryGetAttributeSource(Guid attrDefId)
        {
            return Query.SourceJoins.FirstOrDefault(j => j.JoinAttribute != null && j.JoinAttribute.SameAttrDefId(attrDefId));
        }

        public SqlQueryJoin TryGetAttributeJoin(SqlQuerySource source, Guid attrDefId)
        {
            return Query.SourceJoins.FirstOrDefault(j => j.Source == source && j.JoinAttribute != null && j.JoinAttribute.SameAttrDefId(attrDefId));
        }

        public SqlQuerySource TryGetAttributeSource(SqlQuerySource source, Guid attrDefId)
        {
            var join = Query.SourceJoins.FirstOrDefault(j => j.Source == source && j.JoinAttribute != null && j.JoinAttribute.SameAttrDefId(attrDefId));

            return join != null ? join.Source : null;
        }

        public int TryGetAttributeIndex(SqlQueryJoin join, Guid attrDefId)
        {
            var attr =
                Query.Attributes.FirstOrDefault(a => a.Source == join.Source && a.Def != null && a.Def.Id == attrDefId);

            if (attr == null) return -1;

            return Query.Attributes.IndexOf(attr);
        }

        public int TryGetAttributeIndex(SqlQuerySource source, Guid attrDefId)
        {
            var attr =
                Query.Attributes.FirstOrDefault(a => a.Source == source && a.Def != null && a.Def.Id == attrDefId);

            if (attr == null) return -1;

            return Query.Attributes.IndexOf(attr);
        }

        public int TryGetAttributeIndex(string attrDefName)
        {
            SqlQuerySelectAttribute attr;

            if (attrDefName.Length > 0 && attrDefName[0] == '&')
            {
                var ident = SystemIdentConverter.Convert(attrDefName);

                attr = Query.Attributes.FirstOrDefault(a => a.IsSystemIdent && a.Attribute.Ident == ident);
            }
            else
                attr =
                    Query.Attributes.FirstOrDefault(
                        a => String.Equals(a.Def.Name, attrDefName, StringComparison.OrdinalIgnoreCase));

            if (attr == null) return -1;

            return Query.Attributes.IndexOf(attr);
        }

        public int TryGetAttributeIndex(SqlQuerySource source, string attrDefName)
        {
            SqlQuerySelectAttribute attr;

            if (attrDefName.Length > 0 && attrDefName[0] == '&')
            {
                var ident = SystemIdentConverter.Convert(attrDefName);

                attr = Query.Attributes.FirstOrDefault(a => a.Source == source && a.IsSystemIdent && a.Attribute.Ident == ident);
            }
            else
                attr =
                    Query.Attributes.FirstOrDefault(
                        a => a.Source == source && String.Equals(a.Def.Name, attrDefName, StringComparison.OrdinalIgnoreCase));

            if (attr == null) return -1;

            return Query.Attributes.IndexOf(attr);
        }

        public SqlQueryField FindField(Guid attrDefId)
        {
            return Fields.FirstOrDefault(f => f.AttributeId == attrDefId);
        }
        public SqlQueryField FindField(string attrName)
        {
            return Fields.FirstOrDefault(f =>
                (f.SelectAttribute != null && String.Equals(f.SelectAttribute.Alias, attrName, StringComparison.OrdinalIgnoreCase)) ||
                String.Equals(f.AttributeName, attrName, StringComparison.OrdinalIgnoreCase));
        }

        public string GetSql()
        {
            var s = String.Empty;
            try
            {
                if (Query != null)
                {
                    var sql = Query.BuildSql();
                    if (sql != null)
                    {
                        s = sql.ToString();
                        return s;
                    }
                }
                return String.Empty;
            }
            catch(Exception e)
            {
                OutputLog(e, "GetSql", s);
                throw;
            }
        }

        public int GetAttributeIndex(Guid attrDefId)
        {
            try
            {
                var i = TryGetAttributeIndex(attrDefId);
                if (i < 0)
                    throw new ApplicationException(String.Format("Атрибут \"{0}\" не найден в запросе", attrDefId));

                return i;
            }
            catch (Exception e)
            {
                OutputLog(e, "GetAttributeIndex", GetSql());
                throw;
            }
        }

        public int GetAttributeIndex(string attrDefName)
        {
            try
            {
                var i = TryGetAttributeIndex(attrDefName);
                if (i < 0)
                    throw new ApplicationException(String.Format("Атрибут \"{0}\" не найден в запросе", attrDefName));

                return i;
            }
            catch (Exception e)
            {
                OutputLog(e, "GetAttributeIndex", GetSql());
                throw;
            }
        }

        public void Fill(DataTable table)
        {
            table.Load(CheckReader());
        }

        public object GetValue(int i)
        {
            return CheckReader().IsDBNull(i) ? null : Reader.GetValue(i);
        }

        public object GetValue(Guid attrDefId)
        {
            return CheckReader().GetValue(GetAttributeIndex(attrDefId));
        }

        public object GetValue(string attrDefName)
        {
            return CheckReader().GetValue(GetAttributeIndex(attrDefName));
        }

        public int GetInt32(int i)
        {
            return CheckReader().GetInt32(i);
        }

        public int GetInt32(Guid attrDefId)
        {
            return CheckReader().GetInt32(GetAttributeIndex(attrDefId));
        }

        public int GetInt32(string attrDefName)
        {
            return CheckReader().GetInt32(GetAttributeIndex(attrDefName));
        }

        public short GetInt16(int i)
        {
            return CheckReader().GetInt16(i);
        }

        public short GetInt16(Guid attrDefId)
        {
            return CheckReader().GetInt16(GetAttributeIndex(attrDefId));
        }

        public short GetInt16(string attrDefName)
        {
            return CheckReader().GetInt16(GetAttributeIndex(attrDefName));
        }

        public long GetInt64(int i)
        {
            return CheckReader().GetInt64(i);
        }

        public long GetInt64(Guid attrDefId)
        {
            return CheckReader().GetInt64(GetAttributeIndex(attrDefId));
        }

        public long GetInt64(string attrDefName)
        {
            return CheckReader().GetInt64(GetAttributeIndex(attrDefName));
        }

        public Guid GetGuid(int i)
        {
            return CheckReader().GetGuid(i);
        }

        public Guid GetGuid(Guid attrDefId)
        {
            return CheckReader().GetGuid(GetAttributeIndex(attrDefId));
        }

        public Guid GetGuid(string attrDefName)
        {
            return CheckReader().GetGuid(GetAttributeIndex(attrDefName));
        }

        public string GetString(int i)
        {
            return CheckReader().GetString(i);
        }

        public string GetString(Guid attrDefId)
        {
            return CheckReader().GetString(GetAttributeIndex(attrDefId));
        }

        public string GetString(string attrDefName)
        {
            return CheckReader().GetString(GetAttributeIndex(attrDefName));
        }

        public double GetDouble(int i)
        {
            return CheckReader().GetDouble(i);
        }

        public double GetDouble(Guid attrDefId)
        {
            return CheckReader().GetDouble(GetAttributeIndex(attrDefId));
        }

        public double GetDouble(string attrDefName)
        {
            return CheckReader().GetDouble(GetAttributeIndex(attrDefName));
        }

        public decimal GetDecimal(int i)
        {
            return CheckReader().GetDecimal(i);
        }

        public decimal GetDecimal(Guid attrDefId)
        {
            return CheckReader().GetDecimal(GetAttributeIndex(attrDefId));
        }

        public decimal GetDecimal(string attrDefName)
        {
            return CheckReader().GetDecimal(GetAttributeIndex(attrDefName));
        }

        public DateTime GetDateTime(int i)
        {
            return CheckReader().GetDateTime(i);
        }

        public DateTime GetDateTime(Guid attrDefId)
        {
            return CheckReader().GetDateTime(GetAttributeIndex(attrDefId));
        }

        public DateTime GetDateTime(string attrDefName)
        {
            return CheckReader().GetDateTime(GetAttributeIndex(attrDefName));
        }

        public bool GetBoolean(int i)
        {
            return CheckReader().GetBoolean(i);
        }

        public bool GetBoolean(Guid attrDefId)
        {
            return CheckReader().GetBoolean(GetAttributeIndex(attrDefId));
        }

        public bool GetBoolean(string attrDefName)
        {
            return CheckReader().GetBoolean(GetAttributeIndex(attrDefName));
        }

        public bool IsDbNull(int index)
        {
            return CheckReader().IsDBNull(index);
        }

        public bool IsDbNull(Guid attrDefId)
        {
            return CheckReader().IsDBNull(GetAttributeIndex(attrDefId));
        }

        public bool IsDbNull(string attrDefName)
        {
            return CheckReader().IsDBNull(GetAttributeIndex(attrDefName));
        }

        public Type GetFieldType(int index)
        {
            return CheckReader().GetFieldType(index);
        }

        public Type GetFieldType(Guid attrDefId)
        {
            return CheckReader().GetFieldType(GetAttributeIndex(attrDefId));
        }

        public Type GetFieldType(string attrDefName)
        {
            return CheckReader().GetFieldType(GetAttributeIndex(attrDefName));
        }

        public Doc GetDoc(IDocRepository docRepo)
        {
            //using (var docRepo = new DocRepository(Guid.Empty))

            var doc = docRepo.CreateDoc(Query.Source.GetDocDef());

            var i = 0;
            foreach (var attr in Query.Attributes)
            {
                if (attr.Source == Query.Source)
                {
                    if (attr.Def == null)
                    {
                        switch (attr.Attribute.Ident)
                        {
                            case SystemIdent.Id:
                                if (!Reader.IsDBNull(i))
                                    doc.Id = Reader.GetGuid(i);
                                break;
                            case SystemIdent.Created:
                                if (!Reader.IsDBNull(i))
                                    doc.CreationTime = Reader.GetDateTime(i);
                                break;
                            case SystemIdent.UserId:
                                if (!Reader.IsDBNull(i))
                                    doc.UserId = Reader.GetGuid(i);
                                break;
                            case SystemIdent.OrgId:
                                if (!Reader.IsDBNull(i))
                                    doc.OrganizationId = Reader.GetGuid(i);
                                break;
                            case SystemIdent.State:
                                if (!Reader.IsDBNull(i))
                                    if (doc.State == null)
                                        doc.State = new DocState {Type = new DocStateType {Id = Reader.GetGuid(i)}};
                                    else
                                        doc.State.Type = new DocStateType {Id = Reader.GetGuid(i)};
                                break;
                            case SystemIdent.StateDate:
                                if (!Reader.IsDBNull(i))
                                    if (doc.State == null)
                                        doc.State = new DocState {Type = new DocStateType(), Created = Reader.GetDateTime(i)};
                                    else
                                        doc.State.Created = Reader.GetDateTime(i);
                                break;
                        }
                    }
                    else if (!Reader.IsDBNull(i))
                    {
                        switch (attr.Def.Type.Id)
                        {
                            case (short) CissaDataType.Text:
                                AddAttributeToDoc(doc, new TextAttribute(attr.Def) {Value = Reader.GetString(i)},
                                    attr.Def);
                                break;
                            case (short) CissaDataType.Int:
                                AddAttributeToDoc(doc, new IntAttribute(attr.Def) {Value = Reader.GetInt32(i)}, attr.Def);
                                break;
                            case (short) CissaDataType.Float:
                                AddAttributeToDoc(doc, new FloatAttribute(attr.Def) {Value = Reader.GetDouble(i)},
                                    attr.Def);
                                break;
                            case (short) CissaDataType.Currency:
                                AddAttributeToDoc(doc, new CurrencyAttribute(attr.Def) {Value = Reader.GetDecimal(i)},
                                    attr.Def);
                                break;
                            case (short) CissaDataType.DateTime:
                                AddAttributeToDoc(doc, new DateTimeAttribute(attr.Def) {Value = Reader.GetDateTime(i)},
                                    attr.Def);
                                break;
                            case (short) CissaDataType.Bool:
                                AddAttributeToDoc(doc, new BoolAttribute(attr.Def) {Value = Reader.GetBoolean(i)},
                                    attr.Def);
                                break;
                            case (short) CissaDataType.Enum:
                                AddAttributeToDoc(doc, new EnumAttribute(attr.Def) {Value = Reader.GetGuid(i)}, attr.Def);
                                break;
                            case (short) CissaDataType.Doc:
                                AddAttributeToDoc(doc, new DocAttribute(attr.Def) {Value = Reader.GetGuid(i)}, attr.Def);
                                break;
                        }
                    }
                }
                i++;
            }
            return null;
        }

        private static void AddAttributeToDoc(Doc doc, AttributeBase attr, AttrDef attrDef)
        {
            var existAttr = doc.Attributes.FirstOrDefault(a => a.AttrDef == attrDef);

            if (existAttr != null) doc.Attributes.Remove(existAttr);

            doc.Attributes.Add(attr);
        }

        public int FieldCount
        {
            get { return (Reader != null) ? Reader.FieldCount : 0; }
        }

        public void PrepareParams()
        {
            foreach (var linkPair in _masterLinks)
            {
                linkPair.Key.Update(linkPair.Value);
            }
        }

        ~SqlQueryReader()
        {
            Close();
        }

        public static void OutputLog(Exception e, string msg, string sql)
        {
            try
            {
                using (var writer = new StreamWriter(Logger.GetLogFileName("SqlQueryReader"), true))
                {
                    writer.WriteLine("{0}: {1} : \"{2}\"", DateTime.Now, msg, e.Message);
                    if (e.InnerException != null)
                    {
                        writer.WriteLine("  InnerException: " + e.InnerException.Message);
                    }
                    writer.WriteLine("   StackTrace: " + e.StackTrace);
                    writer.WriteLine("  SQL: " + sql);
                }
            }
            catch (Exception)
            {
                ;
            }
        }

        public static void OutputSqlLog(string sql)
        {
            try
            {
                using (var writer = new StreamWriter(Logger.GetLogFileName("SqlQueryReaderSql"), true))
                {
                    writer.WriteLine("{0}: {1}", DateTime.Now, sql);
                }
            }
            catch (Exception)
            {
                ;
            }
        }
    }
}
