using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Misc;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Sql;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Utils
{
    public static class Csv
    {
        public static string Escape(string s)
        {
            if (s.Contains(Quote))
                s = s.Replace(Quote, EscapedQuote);

            if (s.IndexOfAny(CharactersThatMustBeQuoted) > -1)
                s = Quote + s + Quote;

            return s;
        }

        public static string Unescape(string s)
        {
            if (s.StartsWith(Quote) && s.EndsWith(Quote))
            {
                s = s.Substring(1, s.Length - 2);

                if (s.Contains(EscapedQuote))
                    s = s.Replace(EscapedQuote, Quote);
            }

            return s;
        }


        private const string Quote = "\"";
        private const string EscapedQuote = "\"\"";
        private static readonly char[] CharactersThatMustBeQuoted = { ',', '"', '\n', ';' };
    }

    public abstract class CsvWriter
    {
        public char Delimiter = ';';

        public Func<int, string, string> ValuePrepareFunc;

        public bool Header { get; set; }

        public virtual void Write()
        {
            Prepare();
            if (Header)
            {
                var recordLine = String.Empty;
                for (int i = 0; i < FieldCount; i++)
                {
                    var value = GetFieldTitle(i);
                    if (ValuePrepareFunc != null) value = ValuePrepareFunc(i, value);
                    value = Csv.Escape(value);

                    if (i > 0) recordLine += Delimiter;
                    recordLine += value;
                }
                WriteRecord(recordLine);
            }
            while (GetRecord())
            {
                var recordLine = String.Empty;
                for (int i = 0; i < FieldCount; i++)
                {
                    var value = GetFieldValue(i);
                    if (ValuePrepareFunc != null) value = ValuePrepareFunc(i, value);
                    value = Csv.Escape(value);

                    if (i > 0) recordLine += Delimiter;
                    recordLine += value;
                }
                WriteRecord(recordLine);
            }
        }

        protected abstract void WriteRecord(string recordLine);
        public abstract string GetFieldValue(int index);
        public abstract string GetFieldTitle(int index);
        public abstract int FieldCount { get; }
        public abstract bool GetRecord();
        protected abstract void Prepare();
    }

    public class QueryReaderField
    {
        public Guid AttributeId { get; internal set; }
        public string AttributeName { get; internal set; }
        public BaseDataType DataType { get; internal set; }
        public int FieldIndex { get; internal set; }

        public AttrDef AttrDef { get; internal set; }

        private readonly string _caption;

        public string Caption { get { return String.IsNullOrEmpty(_caption) ? AttributeName : _caption; } }

        public QueryReaderField(Guid attrId)
        {
            AttributeId = attrId;
        }

        public QueryReaderField(string attrName)
        {
            AttributeName = attrName;
        }
        public QueryReaderField(Guid attrId, string caption)
        {
            AttributeId = attrId;
            _caption = caption;
        }

        public QueryReaderField(string attrName, string caption)
        {
            AttributeName = attrName;
            _caption = caption;
        }

        public virtual object PrepareValue(object value)
        {
            return value;
        }
    }

    public class QueryCsvWriter : CsvWriter
    {
        private IAppServiceProvider Provider { get; set; }
        private IDataContext DataContext { get; set; }
        private QueryDef QueryDef { get; set; }
        private SqlQuery Query { get; set; }
        private StreamWriter Writer { get; set; }
        private SqlQueryReader Reader { get; set; }
        private Guid? UserId { get; set; }

        private readonly List<QueryReaderField> _fields = new List<QueryReaderField>();
        public IReadOnlyList<QueryReaderField> Fields { get { return _fields; } }

        private readonly ISqlQueryBuilder _sqlQueryBuilder;

        public QueryCsvWriter(IAppServiceProvider provider, IDataContext dataContext, QueryDef queryDef, StreamWriter writer)
        {
            Provider = provider;
            DataContext = dataContext;
            QueryDef = queryDef;
            Writer = writer;

            _sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
        }
        public QueryCsvWriter(IAppServiceProvider provider, IDataContext dataContext, QueryDef queryDef, StreamWriter writer, Guid userId)
        {
            Provider = provider;
            DataContext = dataContext;
            QueryDef = queryDef;
            Writer = writer;
            UserId = userId;
            _sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
        }

        public QueryCsvWriter(IAppServiceProvider provider, IDataContext dataContext, SqlQuery query, StreamWriter writer)
        {
            Provider = provider;
            DataContext = dataContext;
            Query = query;
            Writer = writer;
            _sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
        }
        public QueryCsvWriter(IAppServiceProvider provider, IDataContext dataContext, SqlQuery query, StreamWriter writer, Guid userId)
        {
            Provider = provider;
            DataContext = dataContext;
            Query = query;
            Writer = writer;
            UserId = userId;
            _sqlQueryBuilder = provider.Get<ISqlQueryBuilder>();
        }

        private IEnumRepository _enumRepo;
        private IDictionary<Guid, IList<EnumValue>> _enumValues = new Dictionary<Guid, IList<EnumValue>>();
        private IComboBoxEnumProvider _comboBoxEnumProvider;

        public QueryReaderField AddField(Guid attrId, string caption = "")
        {
            var field = new QueryReaderField(attrId, caption);
            _fields.Add(field);
            return field;
        }
        public QueryReaderField AddField(string attrName, string caption = "")
        {
            var field = new QueryReaderField(attrName, caption);
            _fields.Add(field);
            return field;
        }

        protected override void WriteRecord(string recordLine)
        {
            Writer.WriteLine(recordLine);
        }

        public override string GetFieldTitle(int index)
        {
            var field = Fields[index];
            return field.Caption;
        }

        public override string GetFieldValue(int index)
        {
            var field = Fields[index];

            if (Reader.IsDbNull(field.FieldIndex))
                return String.Empty;

            switch (field.DataType)
            {
                case BaseDataType.Currency:
                    var mval = Reader.Reader.GetDecimal(field.FieldIndex);
                    return mval.ToString("C");
                case BaseDataType.DateTime:
                    var dval = Reader.Reader.GetDateTime(field.FieldIndex);
                    return dval.ToString("dd.MM.yyyy");
                case BaseDataType.Bool:
                    var bval = Reader.Reader.GetBoolean(field.FieldIndex);
                    return bval.ToString();
                case BaseDataType.Guid:
                    var gval = Reader.Reader.GetGuid(field.FieldIndex);
                    if (field.AttrDef != null)
                    {
                        IList<EnumValue> values = null;
                        if (_enumValues.TryGetValue(field.AttrDef.Id, out values) && values != null)
                        {
                            var enumValue = values.FirstOrDefault(v => v.Id == gval);
                            if (enumValue != null) return enumValue.Value;
                        }
                    }
                    return gval.ToString();
                case BaseDataType.Float:
                    var fval = Reader.Reader.GetDouble(field.FieldIndex);
                    return fval.ToString(CultureInfo.InvariantCulture);
                case BaseDataType.Int:
                    var ival = Reader.Reader.GetInt32(field.FieldIndex);
                    return ival.ToString();
                case BaseDataType.Text:
                    return Reader.Reader.GetString(field.FieldIndex);
            }

            throw new Exception("Не могу вернуть значение поля. Тип данных не известен!");
        }

        public override int FieldCount
        {
            get { return _fields.Count; }
        }

        public override bool GetRecord()
        {
            return Reader.Read();
        }

        protected override void Prepare()
        {
            if (Reader != null)
            {
                Reader.Dispose();
                Reader = null;
            }

            if (Query == null && QueryDef != null)
            {
                Query = _sqlQueryBuilder.Build(QueryDef); // SqlQueryBuilder.Build(DataContext, QueryDef, (Guid) UserId);
                PrepareQueryAttributes(Query);
            }
            Reader = new SqlQueryReader(DataContext, Query);
            Reader.Open();
            PrepareFields(Reader);
        }

        private void PrepareQueryAttributes(SqlQuery query)
        {
            foreach (var field in _fields)
            {
                if (field.AttributeId != Guid.Empty)
                {
                    var attr = query.FindAttribute(field.AttributeId);
                    if (attr != null)
                        query.AddAttribute(field.AttributeId);
                }
                else
                {
                    var attr = query.FindAttribute(field.AttributeName);
                    if (attr != null)
                        query.AddAttribute(field.AttributeName);
                }
            }
        }

        private void PrepareFields(SqlQueryReader reader)
        {
            if (_fields.Count == 0)
            {
                foreach (var readerField in reader.Fields)
                {
                    var field = new QueryReaderField(readerField.AttributeName)
                    {
                        AttributeId = readerField.AttributeId,
                        DataType = readerField.DataType,
                        FieldIndex = readerField.Index,
                        AttrDef = readerField.AttrDef
                    };
                    _fields.Add(field);
                    PrepareField(field);
                }
            }
            else
            {
                foreach (var field in _fields)
                {
                    if (field.AttributeId != Guid.Empty)
                    {
                        var readerField = reader.FindField(field.AttributeId);
                        if (readerField != null)
                        {
                            field.DataType = readerField.DataType;
                            field.AttributeName = readerField.AttributeName;
                            field.FieldIndex = readerField.Index;
                            field.AttrDef = readerField.AttrDef;
                        } 
                        else
                            field.DataType = BaseDataType.Unknown;
                    }
                    else
                    {
                        var readerField = reader.FindField(field.AttributeName);
                        if (readerField != null)
                        {
                            field.DataType = readerField.DataType;
                            field.AttributeId = readerField.AttributeId;
                            field.FieldIndex = readerField.Index;
                            field.AttrDef = readerField.AttrDef;
                        }
                        else
                            field.DataType = BaseDataType.Unknown;
                    }
                    PrepareField(field);
                }
            }
        }

        private void PrepareField(QueryReaderField field)
        {
            if (field.AttrDef != null)
            {
                if (field.AttrDef.Type.Id == (short) CissaDataType.Enum && field.AttrDef.EnumDefType != null)
                {
                    if (_enumRepo == null)
                        _enumRepo = Provider.Get<IEnumRepository>();

                    if (!_enumValues.ContainsKey(field.AttrDef.Id))
                        _enumValues.Add(field.AttrDef.Id,
                            _enumRepo.GetEnumItems(field.AttrDef.EnumDefType.Id));
                }
                else if (field.AttrDef.Type.Id == (short) CissaDataType.Doc)
                {
                    if (_comboBoxEnumProvider == null)
                        _comboBoxEnumProvider = Provider.Get<IComboBoxEnumProvider>();

                    if (!_enumValues.ContainsKey(field.AttrDef.Id))
                        _enumValues.Add(field.AttrDef.Id,
                            _comboBoxEnumProvider.GetEnumDocumentValues(field.AttrDef, "Name"));
                }
                else if (field.AttrDef.Type.Id == (short) CissaDataType.Organization)
                {
                    if (_comboBoxEnumProvider == null)
                        _comboBoxEnumProvider = Provider.Get<IComboBoxEnumProvider>();

                    if (!_enumValues.ContainsKey(field.AttrDef.Id))
                        _enumValues.Add(field.AttrDef.Id,
                            _comboBoxEnumProvider.GetEnumOrganizationValues(field.AttrDef.OrgTypeId));
                }
                else if (field.AttrDef.Type.Id == (short) CissaDataType.User)
                {
                    if (_comboBoxEnumProvider == null)
                        _comboBoxEnumProvider = Provider.Get<IComboBoxEnumProvider>();

                    if (!_enumValues.ContainsKey(field.AttrDef.Id))
                        _enumValues.Add(field.AttrDef.Id,
                            _comboBoxEnumProvider.GetEnumUserValues());
                }
            }
        }
    }
}
