using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQuery : SqlQueryObject, IDisposable
    {
        private Guid _userId;
        public Guid UserId
        {
            get { return _userId; } 
            set
            {
                _userId = value;
                if (Source != null) Source.UserId = value;
            }
        }

        public static double LevensteinCoefficient = 0.7;

        private readonly SqlQueryConditions _conditions = new SqlQueryConditions();
        public SqlQueryConditions Conditions { get { return _conditions; } }

        private readonly List<SqlQuerySelectAttribute> _attributes = new List<SqlQuerySelectAttribute>();
        public List<SqlQuerySelectAttribute> Attributes { get { return _attributes; } }

        private List<string> AttributeAliases { get; set; }
        public int AttributeAliasNo { get; private set; }

        public SqlQuerySource Source { get; private set; }

        public int TopNo { get; set; }
        public int SkipNo { get; set; }

        private readonly List<SqlQuerySource> _sources = new List<SqlQuerySource>();
        public List<SqlQuerySource> Sources { get { return _sources; } }

        private readonly List<SqlQueryJoin> _sourceJoins = new List<SqlQueryJoin>();
        public List<SqlQueryJoin> SourceJoins { get { return _sourceJoins; } }

        private readonly List<SqlQueryAttribute> _groupAttributes = new List<SqlQueryAttribute>();
        public List<SqlQueryAttribute> GroupAttributes { get { return _groupAttributes; } }

        private readonly SqlQueryConditions _havingConditions = new SqlQueryConditions();
        public SqlQueryConditions HavingConditions { get { return _havingConditions; } }

        private readonly List<SqlQueryOrderAttribute> _orderAttributes = new List<SqlQueryOrderAttribute>();
        public List<SqlQueryOrderAttribute> OrderAttributes { get { return _orderAttributes; } }

        private bool _withNoLock = true;
        public bool WithNoLock 
        { 
            get { return _withNoLock; }
            set
            {
                _withNoLock = value;
                Source.WithNoLock = value;

                foreach(var join in _sourceJoins)
                {
                    if (join.Source != null) join.Source.WithNoLock = value;
                    if (join.Target != null) join.Target.WithNoLock = value;
                }
            }
        }

        public IAppServiceProvider Provider { get; private set; }
        private readonly bool _ownProvider = false;

        private readonly IDataContext _dataContext;
        //private readonly bool _ownDataContext;

        public IDataContext DataContext
        {
            get
            {
                var multiDc = _dataContext as IMultiDataContext;
                return multiDc != null ? multiDc.GetDocumentContext : _dataContext;
            }
        }

        private readonly IDocDefRepository _docDefRepo;
        private readonly IUserRepository _userRepo;

        public SqlQuery(IAppServiceProvider provider)
        {
            Provider = provider;
            _dataContext = Provider.Get<IDataContext>();

            _docDefRepo = Provider.Get<IDocDefRepository>();
            _userRepo = Provider.Get<IUserRepository>();

            // var userData = Provider.Get<IUserDataProvider>();
            UserId = Provider.GetCurrentUserId();
        }

        protected SqlQuery(IDataContext dataContext)
        {
            var factory = AppServiceProviderFactoryProvider.GetFactory();
            Provider = factory.Create();
            _ownProvider = true;

            //_ownDocDefRepo = false;
            if (dataContext == null)
            {
                /*DataContext = new DataContext();
                _ownDataContext = true;*/
                _dataContext = Provider.Get<IDataContext>();
            }
            else
                _dataContext = dataContext;

            _docDefRepo = Provider.Get<IDocDefRepository>();
            _userRepo = Provider.Get<IUserRepository>();

            // var userData = Provider.Get<IUserDataProvider>();
            UserId = Provider.GetCurrentUserId();
        }

        public SqlQuery(IAppServiceProvider provider, Guid docDefId, Guid userId) : this(provider)
        {
            UserId = userId;

            var docDef = _docDefRepo.DocDefById(docDefId);

            var alias = GetSourceAlias(docDef.Name);

            Source = new SqlQueryDocSource(Provider, docDef, alias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(alias);
        }

        public SqlQuery(IDataContext dataContext, Guid docDefId, Guid userId)
            : this(dataContext)
        {
            UserId = userId;

            var docDef = _docDefRepo.DocDefById(docDefId);

            var alias = GetSourceAlias(docDef.Name);

            Source = new SqlQueryDocSource(Provider, docDef, alias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(alias);
        }

        /*public SqlQuery(Guid docDefId, Guid userId) : this(null, docDefId, userId) { }*/
        public SqlQuery(Guid docDefId, IAppServiceProvider provider) : this(provider, docDefId, Guid.Empty) { }
        public SqlQuery(Guid docDefId, IDataContext dataContext) : this(dataContext, docDefId, Guid.Empty) { }

        // public SqlQuery(Guid docDefId, string alias, Guid userId): this(null, docDefId, alias, userId) {}
        public SqlQuery(IDataContext dataContext, Guid docDefId, string alias, Guid userId) : this(dataContext)
        {
            UserId = userId;

            if (String.IsNullOrWhiteSpace(alias))
            {
                var docDef = _docDefRepo.DocDefById(docDefId);
                alias = GetSourceAlias(docDef.Name);
            }
            CheckSourceAlias(alias);

            Source = new SqlQueryDocSource(Provider, docDefId, alias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(alias);
        }
        public SqlQuery(Guid docDefId, string alias) : this(null, docDefId, alias, Guid.Empty) { }

        public SqlQuery(IAppServiceProvider provider, DocDef docDef, Guid userId)
            : this(provider)
        {
            UserId = userId;

            var alias = GetSourceAlias(docDef.Name);

            Source = new SqlQueryDocSource(Provider, docDef, alias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(alias);
        }
        public SqlQuery(IDataContext dataContext, DocDef docDef, Guid userId)
            : this(dataContext)
        {
            UserId = userId;

            var alias = GetSourceAlias(docDef.Name);

            Source = new SqlQueryDocSource(Provider, docDef, alias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(alias);
        }

        //public SqlQuery(DocDef docDef, Guid userId): this(null, docDef, userId) {}
        public SqlQuery(IAppServiceProvider provider, DocDef docDef) : this(provider, docDef, Guid.Empty) { }
        /*public SqlQuery(IDataContext dataContext, DocDef docDef) : this(dataContext, docDef, Guid.Empty) { }*/
        /*public SqlQuery(DocDef docDef) : this(null, docDef, Guid.Empty) { }*/

        public SqlQuery(IAppServiceProvider provider, DocDef docDef, string alias, Guid userId)
            : this(provider)
        {
            UserId = userId;

            if (String.IsNullOrWhiteSpace(alias)) alias = GetSourceAlias(docDef.Name);
            CheckSourceAlias(alias);

            Source = new SqlQueryDocSource(Provider, docDef, alias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(alias);
        }
        public SqlQuery(IDataContext dataContext, DocDef docDef, string alias, Guid userId)
            : this(dataContext)
        {
            UserId = userId;

            if (String.IsNullOrWhiteSpace(alias)) alias = GetSourceAlias(docDef.Name);
            CheckSourceAlias(alias);

            Source = new SqlQueryDocSource(Provider, docDef, alias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(alias);
        }

        //public SqlQuery(DocDef docDef, string alias, Guid userId) : this(null, docDef, alias, userId) {}
        //public SqlQuery(DocDef docDef, string alias) : this(null, docDef, alias, Guid.Empty) {}

        public Guid DocumentId { get; set; }
        public Guid ListAttrDefId { get; set; }
        public string DocListAlias { get; private set; }

        public SqlQuery(Guid docDefId, Guid docId, Guid attrDefId, Guid userId, string alias, IAppServiceProvider provider)
            : this(provider)
        {
            UserId = userId;

            DocumentId = docId;
            ListAttrDefId = attrDefId;
            //            var attrDef = docDef.Attributes.First(a => a.Id == attrDefId);
            var listAttrDocDef = _docDefRepo.DocDefById(docDefId);  //DocDefRepo.DocDefById(attrDef.DocDefType.Id);
            if (String.IsNullOrEmpty(alias)) alias = "DocList";
            CheckSourceAlias(alias);
            DocListAlias = alias;
            AddSourceAlias(alias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name /*doc.DocDef.Name*/);

            Source = new SqlQueryDocSource(Provider, listAttrDocDef, docAlias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(docAlias);
        }
        public SqlQuery(Guid docDefId, Guid docId, Guid attrDefId, Guid userId, string alias, IDataContext dataContext)
            : this(dataContext)
        {
            UserId = userId;

            DocumentId = docId;
            ListAttrDefId = attrDefId;
//            var attrDef = docDef.Attributes.First(a => a.Id == attrDefId);
            var listAttrDocDef = _docDefRepo.DocDefById(docDefId);  //DocDefRepo.DocDefById(attrDef.DocDefType.Id);
            if (String.IsNullOrEmpty(alias)) alias = "DocList";
            CheckSourceAlias(alias);
            DocListAlias = alias;
            AddSourceAlias(alias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name /*doc.DocDef.Name*/);

            Source = new SqlQueryDocSource(Provider, listAttrDocDef, docAlias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(docAlias);
        }

        public SqlQuery(Guid docDefId, Guid docId, Guid attrDefId, Guid userId, IAppServiceProvider provider)
            : this(docDefId, docId, attrDefId, userId, String.Empty, provider) { }
        public SqlQuery(Guid docDefId, Guid docId, Guid attrDefId, Guid userId, IDataContext dataContext) 
            : this(docDefId, docId, attrDefId, userId, String.Empty, dataContext) {}

        public SqlQuery(Guid docDefId, Guid docId, Guid attrDefId, IAppServiceProvider provider)
            : this(docDefId, docId, attrDefId, Guid.Empty, String.Empty, provider) { }
        public SqlQuery(Guid docDefId, Guid docId, Guid attrDefId, IDataContext dataContext) 
            : this(docDefId, docId, attrDefId, Guid.Empty, String.Empty, dataContext) { }

        public SqlQuery(Doc doc, DocListAttribute attr, Guid userId, string alias, IAppServiceProvider provider)
            : this(provider)
        {
            UserId = userId;

            DocumentId = doc.Id;
            ListAttrDefId = attr.AttrDef.Id;
            var listAttrDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);
            if (String.IsNullOrEmpty(alias)) alias = "DocList";
            CheckSourceAlias(alias);
            DocListAlias = alias;
            AddSourceAlias(alias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name /*doc.DocDef.Name*/);

            Source = new SqlQueryDocSource(Provider, listAttrDocDef, docAlias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(docAlias);
        }
        public SqlQuery(Doc doc, DocListAttribute attr, Guid userId, string alias, IDataContext dataContext)
            : this(dataContext)
        {
            UserId = userId;

            DocumentId = doc.Id;
            ListAttrDefId = attr.AttrDef.Id;
            var listAttrDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);
            if (String.IsNullOrEmpty(alias)) alias = "DocList";
            CheckSourceAlias(alias);
            DocListAlias = alias;
            AddSourceAlias(alias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name /*doc.DocDef.Name*/);

            Source = new SqlQueryDocSource(Provider, listAttrDocDef, docAlias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(docAlias);
        }

        public SqlQuery(Doc doc, DocListAttribute attr, Guid userId, IAppServiceProvider provider)
            : this(provider)
        {
            UserId = userId;

            DocumentId = doc.Id;
            ListAttrDefId = attr.AttrDef.Id;
            var listAttrDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);
            DocListAlias = "DocList";
            AddSourceAlias(DocListAlias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name /*doc.DocDef.Name*/);

            Source = new SqlQueryDocSource(Provider, listAttrDocDef, docAlias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(docAlias);
        }
        public SqlQuery(Doc doc, DocListAttribute attr, Guid userId, IDataContext dataContext)
            : this(dataContext)
        {
            UserId = userId;

            DocumentId = doc.Id;
            ListAttrDefId = attr.AttrDef.Id;
            var listAttrDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);
            DocListAlias = "DocList";
            AddSourceAlias(DocListAlias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name /*doc.DocDef.Name*/);

            Source = new SqlQueryDocSource(Provider, listAttrDocDef, docAlias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(docAlias);
        }

        //public SqlQuery(Doc doc, DocListAttribute attr) : this(doc, attr, Guid.Empty) {}

        public SqlQuery(Doc doc, Guid attrDefId, Guid userId, string alias, IAppServiceProvider provider)
            : this(provider)
        {
            UserId = userId;

            DocumentId = doc.Id;
            ListAttrDefId = attrDefId;
            var attr = doc.AttrDocList.First(a => a.AttrDef.Id == attrDefId);
            var listAttrDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);
            if (String.IsNullOrEmpty(alias)) alias = "DocList";
            CheckSourceAlias(alias);
            DocListAlias = alias;
            AddSourceAlias(alias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name);

            Source = new SqlQueryDocSource(Provider, listAttrDocDef, docAlias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(docAlias);
        }
        public SqlQuery(Doc doc, Guid attrDefId, Guid userId, string alias, IDataContext dataContext)
            : this(dataContext)
        {
            UserId = userId;

            DocumentId = doc.Id;
            ListAttrDefId = attrDefId;
            var attr = doc.AttrDocList.First(a => a.AttrDef.Id == attrDefId);
            var listAttrDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);
            if (String.IsNullOrEmpty(alias)) alias = "DocList";
            CheckSourceAlias(alias);
            DocListAlias = alias;
            AddSourceAlias(alias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name);

            Source = new SqlQueryDocSource(Provider, listAttrDocDef, docAlias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(docAlias);
        }

        public SqlQuery(Doc doc, Guid attrDefId, IAppServiceProvider provider)
            : this(provider)
        {
            DocumentId = doc.Id;
            ListAttrDefId = attrDefId;
            var attr = doc.AttrDocList.First(a => a.AttrDef.Id == attrDefId);
            var listAttrDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);
            DocListAlias = "DocList";
            AddSourceAlias(DocListAlias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name);

            Source = new SqlQueryDocSource(Provider, /*doc.DocDef*/listAttrDocDef.Id, docAlias) { WithNoLock = WithNoLock };
            AddSourceAlias(docAlias);
        }
        public SqlQuery(Doc doc, Guid attrDefId, IDataContext dataContext)
            : this(dataContext)
        {
            DocumentId = doc.Id;
            ListAttrDefId = attrDefId;
            var attr = doc.AttrDocList.First(a => a.AttrDef.Id == attrDefId);
            var listAttrDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);
            DocListAlias = "DocList";
            AddSourceAlias(DocListAlias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name);

            Source = new SqlQueryDocSource(Provider, /*doc.DocDef*/listAttrDocDef.Id, docAlias) { WithNoLock = WithNoLock };
            AddSourceAlias(docAlias);
        }

        public SqlQuery(Doc doc, string attrDefName, Guid userId, string alias, IAppServiceProvider provider)
            : this(provider)
        {
            UserId = userId;

            DocumentId = doc.Id;
            var attr = doc.AttrDocList.First(a => String.Equals(a.AttrDef.Name, attrDefName, StringComparison.OrdinalIgnoreCase));
            ListAttrDefId = attr.AttrDef.Id;
            var listAttrDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);
            if (String.IsNullOrEmpty(alias)) alias = "DocList";
            CheckSourceAlias(alias);
            DocListAlias = alias;
            AddSourceAlias(DocListAlias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name);

            Source = new SqlQueryDocSource(Provider, listAttrDocDef.Id, docAlias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(alias);
        }
        public SqlQuery(Doc doc, string attrDefName, Guid userId, string alias, IDataContext dataContext)
            : this(dataContext)
        {
            UserId = userId;

            DocumentId = doc.Id;
            var attr = doc.AttrDocList.First(a => String.Equals(a.AttrDef.Name, attrDefName, StringComparison.OrdinalIgnoreCase));
            ListAttrDefId = attr.AttrDef.Id;
            var listAttrDocDef = _docDefRepo.DocDefById(attr.AttrDef.DocDefType.Id);
            if (String.IsNullOrEmpty(alias)) alias = "DocList";
            CheckSourceAlias(alias);
            DocListAlias = alias;
            AddSourceAlias(DocListAlias);

            var docAlias = GetSourceAlias(listAttrDocDef.Name);

            Source = new SqlQueryDocSource(Provider, listAttrDocDef.Id, docAlias, userId) { WithNoLock = WithNoLock };
            AddSourceAlias(alias);
        }

        public delegate void BuildQueryJoins(SqlQueryJoinBuilder builder);

        public SqlQuery Joins(BuildQueryJoins joins)
        {
            joins.Invoke(new SqlQueryJoinBuilder(this, Source));
            return this;
        }

        private int _sourceAliasNo;
        private readonly List<string> _sourceAliases = new List<string>();

        public SqlQuerySource TryJoinSource(SqlQuerySource master, Guid docDefId, SqlSourceJoinType joinType, Guid attrDefId)
        {
            var exJoin = SourceJoins.FirstOrDefault(j => j.Source == master && j.Target.GetDocDef().Id == docDefId &&
                                                         j.JoinAttrDef.Id == attrDefId);

            if (exJoin != null)
            {
                if (exJoin.JoinType != joinType && joinType == SqlSourceJoinType.Inner)
                    exJoin.JoinType = SqlSourceJoinType.Inner;
                return exJoin.Target;
            }

            var docDef = _docDefRepo.DocDefById(docDefId);

            return JoinSource(master, docDef, joinType, attrDefId);
        }

        public SqlQuerySource JoinSource(SqlQuerySource master, Guid docDefId, SqlSourceJoinType joinType, Guid attrDefId)
        {
            var exJoin = SourceJoins.FirstOrDefault(j => j.Source == master && j.Target.GetDocDef().Id == docDefId &&
                                j.JoinType == joinType && j.JoinAttrDef.Id == attrDefId);

            if (exJoin != null) return exJoin.Target;

            //using (var defRepo = new DocDefRepository(UserId))
            {
                var docDef = _docDefRepo.DocDefById(docDefId);
                
                return JoinSource(master, docDef, joinType, attrDefId);
            }
        }

        public SqlQuerySource JoinSource(SqlQuerySource master, Guid docDefId, SqlSourceJoinType joinType, string attrDefName)
        {
            var docDef = _docDefRepo.DocDefById(docDefId);

            return JoinSource(master, docDef, joinType, attrDefName);
        }

        public SqlQuerySource JoinSource(SqlQuerySource master, DocDef docDef, SqlSourceJoinType joinType, string attrDefName)
        {
            var attrDef =
                docDef.Attributes.FirstOrDefault(
                    a => String.Equals(a.Name, attrDefName, StringComparison.OrdinalIgnoreCase)) ??
                master.FindAttributeDef(attrDefName);

            if (attrDef == null)
                throw new ApplicationException(String.Format("Атрибут с именем \"{0}\" не найден!", attrDefName));

            return JoinSource(master, docDef, joinType, attrDef.Id);
        }

        public SqlQuerySource JoinSource(SqlQuerySource master, DocDef docDef, SqlSourceJoinType joinType, Guid attrDefId)
        {
            var alias = GetSourceAlias(docDef.Name);

            var slave = new SqlQueryDocSource(Provider, docDef, alias) { WithNoLock = _withNoLock };
            Sources.Add(slave);

            var join = new SqlQueryJoin(master, slave, joinType, attrDefId);
            SourceJoins.Add(join);

            AddSourceAlias(alias);
            return slave;
        }

        public SqlQuerySource JoinSource(SqlQuerySource master, SqlQuery subQuery, SqlSourceJoinType joinType, string attrDefName)
        {
            var alias = GetSourceAlias("sub");

            var slave = new SqlQuerySubSource(subQuery, alias) { WithNoLock = _withNoLock };

            var attrDef =
                slave.FindAttributeDef(attrDefName) ??
                master.FindAttributeDef(attrDefName);

            if (attrDef == null)
                throw new ApplicationException(String.Format("Атрибут с именем \"{0}\" не найден!", attrDefName));

            Sources.Add(slave);

            var join = new SqlQueryJoin(master, slave, joinType, attrDef.Id);
            SourceJoins.Add(join);

            AddSourceAlias(alias);
            return slave;
        }

        public SqlQuerySource JoinSource(SqlQuerySource master, SqlQuery subQuery, SqlSourceJoinType joinType, Guid attrDefId)
        {
            var alias = GetSourceAlias("sub");

            var slave = new SqlQuerySubSource(subQuery, alias) { WithNoLock = _withNoLock };
            Sources.Add(slave);

            var join = new SqlQueryJoin(master, slave, joinType, attrDefId);
            SourceJoins.Add(join);

            AddSourceAlias(alias);
            return slave;
        }

        public SqlQueryJoinBuilder InnerJoinSource(Guid docDefId, string attrDefName)
        {
            return
                new SqlQueryJoinBuilder(null, this, JoinSource(Source, docDefId, SqlSourceJoinType.Inner, attrDefName));
        }

        public SqlQueryJoinBuilder InnerJoinSource(DocDef docDef, Guid attrDefId)
        {
            return 
                new SqlQueryJoinBuilder(null, this, JoinSource(Source, docDef, SqlSourceJoinType.Inner, attrDefId));
        }

        public SqlQueryJoinBuilder LeftOuterJoinSource(Guid docDefId, string attrDefName)
        {
            return
                new SqlQueryJoinBuilder(null, this,
                    JoinSource(Source, docDefId, SqlSourceJoinType.LeftOuter, attrDefName));
        }

        public SqlQueryJoinBuilder LeftOuterJoinSource(DocDef docDef, Guid attrDefId)
        {
            return
                new SqlQueryJoinBuilder(null, this, JoinSource(Source, docDef, SqlSourceJoinType.LeftOuter, attrDefId));
        }

        public SqlQueryJoinBuilder RightOuterJoinSource(Guid docDefId, string attrDefName)
        {
            return
                new SqlQueryJoinBuilder(null, this,
                    JoinSource(Source, docDefId, SqlSourceJoinType.RightOuter, attrDefName));
        }

        public SqlQueryJoinBuilder RightOuterJoinSource(DocDef docDef, Guid attrDefId)
        {
            return
                new SqlQueryJoinBuilder(null, this,
                    JoinSource(Source, docDef, SqlSourceJoinType.RightOuter, attrDefId));
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, Guid attrDefId,
            ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByAttributeId(attrDefId);

            var item = new SqlQueryCondition(source, operation, attrDefId, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }
        public SqlQueryCondition AddCondition(ExpressionOperation operation, SqlQuerySource source, Guid attrDefId,
            ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            //var source = GetSourceByAttributeId(attrDefId);

            var item = new SqlQueryCondition(source, operation, attrDefId, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }
        public SqlQueryCondition AddCondition(ExpressionOperation operation, Guid attrDefId,
            ConditionOperation condition, object value, string expression, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByAttributeId(attrDefId);

            var item = new SqlQueryCondition(source, operation, attrDefId, condition, value, expression);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }
        public SqlQueryCondition AddCondition(ExpressionOperation operation, Guid attrDefId,
            ConditionOperation condition, object[] values, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByAttributeId(attrDefId);

            var item = new SqlQueryCondition(source, operation, attrDefId, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, string attrName,
            ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);
//            if (source == null)
//                throw new ApplicationException(String.Format("Источник данных \"{0}\" не найден",
//                                                             docDef.Name ?? docDef.Caption));

            var item = new SqlQueryCondition(source, operation, attrName, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, Guid attrDefId,
            ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, operation, attrDefId, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, Guid attrDefId,
            ConditionOperation condition, object value, string expression, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, operation, attrDefId, condition, value, expression);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, Guid docDefId, string attrName,
            ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDefId);

            var item = new SqlQueryCondition(source, operation, attrName, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, Guid docDefId, string attrName,
            ConditionOperation condition, object value, string expression, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDefId);

            var item = new SqlQueryCondition(source, operation, attrName, condition, value, expression);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, string attrName, ConditionOperation condition, object[] values, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, operation, attrName, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, string attrName, ConditionOperation condition, IEnumerable<object> values, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, operation, attrName, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, Guid docDefId, string attrName, ConditionOperation condition, object[] values, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDefId);

            var item = new SqlQueryCondition(source, operation, attrName, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, Guid docDefId, string[] attrNames, ConditionOperation condition, object value, string expression, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDefId);

            var item = new SqlQueryCondition(source, operation, attrNames, condition, value, expression);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, Guid docDefId, string[] attrNames, ConditionOperation condition, object[] values, string expression, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDefId);

            var item = new SqlQueryCondition(source, operation, attrNames, condition, values, expression);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, Guid attrId, ConditionOperation condition, object[] values, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, operation, attrId, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, string attrName, ConditionOperation condition,
                                              SqlQuery subQuery, Guid subQueryAttrId, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, operation, attrName, condition, subQuery, subQueryAttrId);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, Guid attrId, ConditionOperation condition,
                                              SqlQuery subQuery, Guid subQueryAttrId, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, operation, attrId, condition, subQuery, subQueryAttrId);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, string attrName, ConditionOperation condition,
                                              SqlQuery subQuery, string subQueryAttrName, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, operation, attrName, condition, subQuery, subQueryAttrName);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, Guid docDefId, string attrName, ConditionOperation condition,
                                              SqlQuery subQuery, string subQueryAttrName, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDefId);

            var item = new SqlQueryCondition(source, operation, attrName, condition, subQuery, subQueryAttrName);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, SqlQuerySource source, string attrName, ConditionOperation condition,
                                              SqlQuery subQuery, string subQueryAttrName, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, operation, attrName, condition, subQuery, subQueryAttrName);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, string attrName, string expression, ConditionOperation condition,
                                              object value, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, operation, attrName, condition, value, expression);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddCondition(ExpressionOperation operation, DocDef docDef, Guid attrId, ConditionOperation condition,
                                              SqlQuery subQuery, string subQueryAttrName, SqlQueryCondition parentCondition = null)
        {
            var source = GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, operation, attrId, condition, subQuery, subQueryAttrName);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }
        public SqlQueryCondition AddCondition(ExpressionOperation operation, SqlQuerySource source1, string attrName1, ConditionOperation condition,
                                              SqlQuerySource source2, string attrName2, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(operation, source1, attrName1, condition, source2 , attrName2);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddExpCondition(ExpressionOperation operation, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(null, operation, Guid.Empty, ConditionOperation.Exp, null);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndCondition(Guid attrDefId, ConditionOperation condition, object value, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.And, attrDefId, condition, value);
            Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndCondition(Guid attrDefId, ConditionOperation condition, object[] values, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.And, attrDefId, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndCondition(string attrDefName, ConditionOperation condition, object value, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.And, attrDefName, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, ExpressionOperation.And, attrDefName, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, SqlQuerySource source2, string attrDefName2, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(ExpressionOperation.And, source, attrDefName, condition, source2, attrDefName2);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndCondition(string attrDefName, ConditionOperation condition, object[] values, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.And, attrDefName, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrCondition(Guid attrDefId, ConditionOperation condition, object value, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.Or, attrDefId, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrCondition(Guid attrDefId, ConditionOperation condition, object[] values, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.Or, attrDefId, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrCondition(string attrDefName, ConditionOperation condition, object value, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.Or, attrDefName, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, ExpressionOperation.Or, attrDefName, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrCondition(string attrDefName, ConditionOperation condition, object[] values, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.Or, attrDefName, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, object[] values, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, ExpressionOperation.Or, attrDefName, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, SqlQuerySource source2, string attrDefName2, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(ExpressionOperation.Or, source, attrDefName, condition, source2, attrDefName2);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndNotCondition(Guid attrDefId, ConditionOperation condition, object value, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.AndNot, attrDefId, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndNotCondition(Guid attrDefId, ConditionOperation condition, object[] values, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.AndNot, attrDefId, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndNotCondition(string attrDefName, ConditionOperation condition, object value, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.AndNot, attrDefName, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndNotCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, ExpressionOperation.AndNot, attrDefName, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndNotCondition(string attrDefName, ConditionOperation condition, object[] values, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.AndNot, attrDefName, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndNotCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, object[] values, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, ExpressionOperation.AndNot, attrDefName, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition AndNotCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, SqlQuerySource source2, string attrDefName2, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(ExpressionOperation.AndNot, source, attrDefName, condition, source2, attrDefName2);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrNotCondition(Guid attrDefId, ConditionOperation condition, object value, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.OrNot, attrDefId, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrNotCondition(Guid attrDefId, ConditionOperation condition, object[] values, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.OrNot, attrDefId, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrNotCondition(string attrDefName, ConditionOperation condition, object value, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.OrNot, attrDefName, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrNotCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, object value, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, ExpressionOperation.OrNot, attrDefName, condition, value);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrNotCondition(string attrDefName, ConditionOperation condition, object[] values, DocDef docDef = null, SqlQueryCondition parentCondition = null)
        {
            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(source, ExpressionOperation.OrNot, attrDefName, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrNotCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, object[] values, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, ExpressionOperation.OrNot, attrDefName, condition, values);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public SqlQueryCondition OrNotCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, SqlQuerySource source2, string attrDefName2, SqlQueryCondition parentCondition = null)
        {
            //            var source = docDef == null ? Source : GetSourceByDocDef(docDef);

            var item = new SqlQueryCondition(ExpressionOperation.OrNot, source, attrDefName, condition, source2, attrDefName2);
            if (parentCondition == null)
                Conditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public override SqlQuerySelectAttribute AddAttribute(Guid attrDefId)
        {
            var source = GetSourceByAttributeId(attrDefId);

            var attribute = new SqlQuerySelectAttribute(source, attrDefId);
            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public override SqlQuerySelectAttribute AddAttribute(string attrName)
        {
            SqlQuerySelectAttribute attribute;

            if (attrName.Length > 0 && attrName[0] == '&')
                attribute = new SqlQuerySelectAttribute(Source, attrName);
            else
            {
                var source = GetSourceByAttributeName(attrName);

                attribute = new SqlQuerySelectAttribute(source, attrName);
            }
            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public override SqlQuerySelectAttribute AddAttribute(SystemIdent attrIdent)
        {
            var attribute = new SqlQuerySelectAttribute(Source, attrIdent);
            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQuerySelectAttribute AddAttribute(SqlQuerySource source, string attrName)
        {
            var attribute = new SqlQuerySelectAttribute(source, attrName);
            
            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQuerySelectAttribute AddAttribute(SqlQuerySource source, Guid attrDefId)
        {
            var attribute = new SqlQuerySelectAttribute(source, attrDefId);

            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQuerySelectAttribute AddAttribute(SqlQuerySource source, SystemIdent attrIdent)
        {
            var attribute = new SqlQuerySelectAttribute(source, attrIdent);

            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public override DocDef Def
        {
            get { return Source.GetDocDef(); }
        }

        public SqlQuerySelectAttribute AddAttribute(Guid attrDefId, string expression)
        {
            var source = GetSourceByAttributeId(attrDefId);

            var attribute = new SqlQuerySelectAttribute(source, attrDefId, expression);
            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQuerySelectAttribute AddAttribute(string attrName, string expression)
        {
            SqlQuerySelectAttribute attribute;

            if (attrName.Length > 0 && attrName[0] == '&')
                attribute = new SqlQuerySelectAttribute(Source, attrName, expression);
            else
            {
                var source = GetSourceByAttributeName(attrName);

                attribute = new SqlQuerySelectAttribute(source, attrName, expression);
            }
            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQueryAttribute AddAttribute(SqlQuerySource source, string attrName, string expression)
        {
            var attribute = new SqlQuerySelectAttribute(source, attrName, expression);

            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQueryAttribute AddAttribute(IEnumerable<SqlQuerySourceAttributeRef> attrRefs, string expression)
        {
            var attribute = new SqlQuerySelectAttribute(attrRefs, expression);

            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQuerySelectAttribute AddAttribute(Guid attrDefId, SqlQuerySummaryFunction summary)
        {
            var source = GetSourceByAttributeId(attrDefId);

            var attribute = new SqlQuerySelectAttribute(source, attrDefId, summary);
            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQuerySelectAttribute AddAttribute(string attrName, SqlQuerySummaryFunction summary)
        {
            SqlQuerySelectAttribute attribute;

            if (attrName.Length > 0 && attrName[0] == '&')
                attribute = new SqlQuerySelectAttribute(Source, attrName, summary);
            else
            {
                var source = GetSourceByAttributeName(attrName);

                attribute = new SqlQuerySelectAttribute(source, attrName, summary);
            }
            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQuerySelectAttribute AddAttribute(SqlQuerySource source, Guid attrDefId, SqlQuerySummaryFunction summary)
        {
            var attribute = new SqlQuerySelectAttribute(source, attrDefId, summary);

            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQuerySelectAttribute AddAttribute(SqlQuerySource source, string attrName, SqlQuerySummaryFunction summary)
        {
            var attribute = new SqlQuerySelectAttribute(source, attrName, summary);

            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQuerySelectAttribute AddAttribute(SqlQuerySource source, SystemIdent attrIdent, SqlQuerySummaryFunction summary)
        {
            var attribute = new SqlQuerySelectAttribute(source, attrIdent, summary);

            Attributes.Add(attribute);

            var alias = GetAttributeAlias(attribute.GetName());
            AttributeAliases.Add(alias.ToUpper());
            attribute.Alias = alias;

            return attribute;
        }

        public SqlQueryCondition AddHavingCondition(Guid attrDefId, ConditionOperation condition, object value, ExpressionOperation operation = ExpressionOperation.And)
        {
            var source = FindSourceByAttributeId(attrDefId);

            var item = new SqlQueryCondition(source, operation, attrDefId, condition, value);
            HavingConditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddHavingCondition(Guid attrDefId, ConditionOperation condition, object[] values, ExpressionOperation operation = ExpressionOperation.And)
        {
            var source = FindSourceByAttributeId(attrDefId);

            var item = new SqlQueryCondition(source, operation, attrDefId, condition, values);
            HavingConditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddHavingCondition(Guid docDefId, string attrName, ConditionOperation condition, object value, ExpressionOperation operation)
        {
            var source = GetSourceByDocDef(docDefId);

            var item = new SqlQueryCondition(source, operation, attrName, condition, value);
            HavingConditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddHavingCondition(Guid docDefId, string attrName, ConditionOperation condition, object[] values, ExpressionOperation operation = ExpressionOperation.And)
        {
            var source = GetSourceByDocDef(docDefId);

            var item = new SqlQueryCondition(source, operation, attrName, condition, values);
            HavingConditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddHavingCondition(Guid docDefId, string[] attrNames, string expression, ConditionOperation condition, object value, ExpressionOperation operation = ExpressionOperation.And)
        {
            var source = GetSourceByDocDef(docDefId);

            var item = new SqlQueryCondition(source, operation, attrNames, condition, value, expression);
            HavingConditions.Add(item);

            return item;
        }

        public SqlQueryCondition AddHavingCondition(SqlQuerySource source, string attrDefName, ConditionOperation condition, object value, ExpressionOperation operation, SqlQueryCondition parentCondition = null)
        {
            var item = new SqlQueryCondition(source, ExpressionOperation.And, attrDefName, condition, value);
            if (parentCondition == null)
                HavingConditions.Add(item);
            else
                parentCondition.Conditions.Add(item);

            return item;
        }

        public override SqlQuerySelectAttribute FindAttribute(Guid attrDefId)
        {
            return _attributes.FirstOrDefault(attr => attr.Def != null && attr.Def.Id == attrDefId);
        }

        public SqlQuerySelectAttribute FindAttribute(SqlQuerySource source, Guid attrDefId)
        {
            return _attributes.FirstOrDefault(attr => (attr.Source == source || source == null) && attr.Def != null && attr.Def.Id == attrDefId);
        }

        public override SqlQuerySelectAttribute FindAttribute(string attrDefName)
        {
            if (attrDefName.Length > 0 && attrDefName[0] == '&')
            {
                var ident = SystemIdentConverter.Convert(attrDefName);
                return _attributes.FirstOrDefault(a => a.Attribute.Def == null && a.Attribute.Ident == ident);
            }

            return
                _attributes.FirstOrDefault(
                    a => String.Equals(a.Alias, attrDefName, StringComparison.OrdinalIgnoreCase) ||
                         (a.Def != null &&
                          String.Equals(a.Def.Name, attrDefName, StringComparison.OrdinalIgnoreCase)));
        }

        public override SqlQuerySelectAttribute FindAttribute(SystemIdent attrIdent)
        {
            return _attributes.FirstOrDefault(a => a.Attribute.Def == null && a.Attribute.Ident == attrIdent);
        }

        public SqlQuerySelectAttribute FindAttribute(SqlQuerySource source, string attrDefName)
        {
            if (attrDefName.Length > 0 && attrDefName[0] == '&')
            {
                var ident = SystemIdentConverter.Convert(attrDefName);
                return _attributes.FirstOrDefault(a => a.Source == source && a.Attribute.Def == null && a.Attribute.Ident == ident);
            }

            return
                _attributes.FirstOrDefault(
                    a =>
                        a.Source == source && ((a.Def != null &&
                                                String.Equals(a.Def.Name, attrDefName,
                                                    StringComparison.OrdinalIgnoreCase)) ||
                                               String.Equals(a.Alias, attrDefName, StringComparison.OrdinalIgnoreCase)));
        }

        public SqlQuerySelectAttribute FindAttribute(SqlQuerySource source, SystemIdent attrIdent)
        {
            return _attributes.FirstOrDefault(a => a.Source == source && a.Attribute.Def == null && a.Attribute.Ident == attrIdent);
        }

        public SqlQuerySelectAttribute GetAttribute(SqlQuerySource source, string attrDefName)
        {
            if (source == null)
                return FindAttribute(attrDefName) ?? AddAttribute(attrDefName);
            return FindAttribute(source, attrDefName) ?? AddAttribute(source, attrDefName);
        }
        public SqlQuerySelectAttribute GetAttribute(SqlQuerySource source, Guid attrDefId)
        {
            if (source == null)
                return FindAttribute(attrDefId) ?? AddAttribute(attrDefId);
            return FindAttribute(source, attrDefId) ?? AddAttribute(source, attrDefId);
        }
        public SqlQuerySourceAttributeRef FindSourceAttribute(Guid attrDefId)
        {
            var source = FindSourceByAttributeId(attrDefId);
            if (source == null) return null;
            var attr = source.GetAttribute(attrDefId);
            return new SqlQuerySourceAttributeRef(source, attr);
        }

        public SqlQuerySourceAttributeRef FindSourceAttribute(string attrDefName)
        {
            var source = FindSourceByAttributeName(attrDefName);
            if (source == null) return null;
            var attr = source.GetAttribute(attrDefName);
            return new SqlQuerySourceAttributeRef(source, attr);
        }
        public SqlQuerySourceAttributeRef FindSourceAttribute(QueryAttributeRef attrRef)
        {
            if (attrRef == null) return null;
            var source = FindSource(attrRef.Source);
            if (source == null) return null;
            var attr = source.GetAttribute(attrRef);
            return new SqlQuerySourceAttributeRef(source, attr);
        }

        public SqlQuerySource FindAttributeTargetSource(SqlQuerySource source, Guid attrDefId)
        {
            var join =
                SourceJoins.FirstOrDefault(
                    j => j.Source == source && j.JoinAttribute != null && j.JoinAttribute.SameAttrDefId(attrDefId));
            return join != null ? join.Target : null;
        }

        public SqlQueryJoin FindJoin(SqlQuerySource source, Guid attrDefId)
        {
            return SourceJoins.FirstOrDefault(
                j => j.Source == source && j.JoinAttribute != null && j.JoinAttribute.SameAttrDefId(attrDefId));
        }

        public SqlQuerySourceAttributeRef GetSourceAttribute(Guid attrDefId)
        {
            var result = FindSourceAttribute(attrDefId);

            if (result == null) 
                throw new ApplicationException(String.Format("Атрибут \"{0}\" не найден", attrDefId));

            return result;
        }
        public SqlQuerySourceAttributeRef GetSourceAttribute(QueryAttributeRef attrRef)
        {
            var result = FindSourceAttribute(attrRef);

            if (result == null)
                throw new ApplicationException(String.Format("Атрибут \"{0}\" не найден", attrRef != null ? attrRef.AttributeName : ""));

            return result;
        }

        public SqlQuerySourceAttributeRef GetSourceAttribute(string attrDefName)
        {
            var result = FindSourceAttribute(attrDefName);

            if (result == null)
                throw new ApplicationException(String.Format("Атрибут \"{0}\" не найден", attrDefName));

            return result;
        }

        public SqlQueryAttribute AddGroupAttribute(SqlQueryAttribute attribute)
        {
            GroupAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryAttribute AddGroupAttribute(Guid attrDefId)
        {
            var attribute = FindAttribute(attrDefId) ?? AddAttribute(attrDefId);

            GroupAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryAttribute AddGroupAttribute(string attrDefName)
        {
            SqlQueryGroupAttribute attribute;

            if (attrDefName.Length > 0 && attrDefName[0] == '&')
                attribute = new SqlQueryGroupAttribute(Source, attrDefName);
            else
            {
                var source = GetSourceByAttributeName(attrDefName);

                attribute = new SqlQueryGroupAttribute(source, attrDefName);
            }

            GroupAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryAttribute AddGroupAttribute(IEnumerable<SqlQuerySourceAttributeRef> attrRefs, string expression)
        {
            var attribute = new SqlQueryGroupAttribute(attrRefs, expression);

            GroupAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryAttribute AddGroupAttribute(SqlQuerySource source, string attrName)
        {
            var attribute = new SqlQueryGroupAttribute(source, attrName);

            GroupAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryAttribute AddGroupAttribute(SqlQuerySource source, Guid attrDefId)
        {
            var attribute = new SqlQueryGroupAttribute(source, attrDefId);

            GroupAttributes.Add(attribute);

            return attribute;
        }
        public SqlQueryAttribute AddGroupAttribute(SqlQuerySource source, SystemIdent attrIdent)
        {
            var attribute = new SqlQueryGroupAttribute(source, attrIdent);

            GroupAttributes.Add(attribute);

            return attribute;
        }


        public SqlQueryAttribute AddGroupAttribute(SqlQuerySource source, string attrName, string expression)
        {
            var attribute = new SqlQueryGroupAttribute(source, attrName, expression);

            GroupAttributes.Add(attribute);

            return attribute;

        }

        public SqlQueryOrderAttribute AddOrderAttribute(SqlQueryOrderAttribute attribute, bool asc = true)
        {
            attribute.Asc = asc;

            OrderAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryOrderAttribute AddOrderAttribute(Guid attrDefId, bool asc = true)
        {
            var attrRef = GetSourceAttribute(attrDefId);
            var attr = new SqlQueryOrderAttribute(attrRef.Source, attrRef.Attribute, asc);
            OrderAttributes.Add(attr);

            return attr;
        }

        public SqlQueryOrderAttribute AddOrderAttribute(QueryOrderDef orderDef)
        {
            var attrHelper = new QueryAttributeDefHelper(orderDef.Attribute);
            var aref = attrHelper.GetAttributeRef();
            var attrRef = GetSourceAttribute(aref);
            var attr = new SqlQueryOrderAttribute(attrRef.Source, attrRef.Attribute, orderDef.Asc);
            OrderAttributes.Add(attr);

            return attr;
        }

        public SqlQueryOrderAttribute AddOrderAttribute(string attrDefName, bool asc = true)
        {
            SqlQueryOrderAttribute attribute;

            if (attrDefName.Length > 0 && attrDefName[0] == '&')
                attribute = new SqlQueryOrderAttribute(Source, attrDefName, asc);
            else
            {
                var source = GetSourceByAttributeName(attrDefName);

                attribute = new SqlQueryOrderAttribute(source, attrDefName, asc);
            }
            OrderAttributes.Add(attribute);

            return attribute;
        }
        
        public SqlQueryOrderAttribute AddOrderAttribute(SqlQuerySource source, Guid attrDefId, bool asc = true)
        {
            var attribute = new SqlQueryOrderAttribute(source, attrDefId, asc);

            OrderAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryOrderAttribute AddOrderAttribute(SqlQuerySource source, string attrDefName, bool asc = true)
        {
            var attribute = new SqlQueryOrderAttribute(source, attrDefName, asc);

            OrderAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryOrderAttribute AddOrderAttribute(SqlQuerySource source, SystemIdent attrIdent, bool asc = true)
        {
            var attribute = new SqlQueryOrderAttribute(source, attrIdent, asc);

            OrderAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryOrderAttribute AddOrderAttribute(SqlQuerySource source, SystemIdent attrIdent, string exp, bool asc = true)
        {
            var attribute = new SqlQueryOrderAttribute(source, attrIdent, exp, asc);

            OrderAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryOrderAttribute AddOrderAttribute(SqlQuerySource source, string attrDefName, string expression, bool asc = true)
        {
            var attribute = new SqlQueryOrderAttribute(source, attrDefName, expression, asc);

            OrderAttributes.Add(attribute);

            return attribute;
        }

        public SqlQueryOrderAttribute AddOrderAttribute(SqlQuerySource source, Guid attrDefId, string expression, bool asc = true)
        {
            var attribute = new SqlQueryOrderAttribute(source, attrDefId, expression, asc);

            OrderAttributes.Add(attribute);

            return attribute;
        }

        public List<SqlQueryAttribute> AddAttributes(params string[] attrNames)
        {
            return attrNames.Select(AddAttribute).Cast<SqlQueryAttribute>().ToList();
        }

        public List<SqlQueryAttribute> AddGroupAttributes(string[] attrNames)
        {
            return attrNames.Select(AddGroupAttribute).ToList();
        }

        public SqlQueryGroupAttribute AddGroupAttribute(QueryGroupDef groupDef)
        {
            var attrHelper = new QueryAttributeDefHelper(groupDef.Attribute);
            var aref = attrHelper.GetAttributeRef();
            var attrRef = GetSourceAttribute(aref);
            var attr = new SqlQueryGroupAttribute(attrRef.Source, attrRef.Attribute);
            GroupAttributes.Add(attr);

            return attr;
        }

        public void SetParams(string paramName, object value)
        {
            foreach (var param in FindParams(paramName))
            {
                param.Value = value;
            }
        }
        public IEnumerable<QueryConditionValueDef> GetAllParams()
        {
            if (SourceJoins != null)
                foreach (var condition in SourceJoins.SelectMany(j => j.Conditions))
                {
                    if (condition.Condition == ConditionOperation.Include ||
                        condition.Condition == ConditionOperation.Exp)
                    {
                        foreach (var param in condition.Conditions.GetParams()) // GetConditionsParams(condition.Conditions))
                            yield return param;
                    }
                    else
                    {
                        foreach (var param in GetConditionPartParams(condition.Left))
                            yield return param;
                        foreach (var param in GetConditionPartParams(condition.Right))
                            yield return param;
                    }
                }
            if (Conditions != null)
                foreach (var param in Conditions.GetParams()) //GetConditionsParams(Conditions))
                    yield return param;
            if (HavingConditions != null)
                foreach (var param in HavingConditions.GetParams()) //GetConditionsParams(HavingConditions))
                    yield return param;
        }
        public IEnumerable<QueryConditionValueDef> FindParams(string paramName)
        {
            foreach (var param in FindJoinConditionsParams(paramName, SourceJoins))
                yield return param;
            foreach (var param in Conditions.FindParams(paramName)) // FindConditionsParams(paramName, Conditions))
                yield return param;
            foreach (var param in HavingConditions.FindParams(paramName)) // FindConditionsParams(paramName, HavingConditions))
                yield return param;
        }
        private static IEnumerable<QueryConditionValueDef> FindJoinConditionsParams(string paramName, IEnumerable<SqlQueryJoin> joins)
        {
            if (joins != null)
                foreach (var condition in joins.SelectMany(j => j.Conditions))
                {
                    if (condition.Condition == ConditionOperation.Include ||
                        condition.Condition == ConditionOperation.Exp)
                    {
                        foreach (var param in condition.Conditions.FindParams(paramName)) // FindConditionsParams(paramName, condition.Conditions))
                            yield return param;
                    }
                    else
                    {
                        foreach (var param in FindConditionPartParams(paramName, condition.Left))
                            yield return param;
                        foreach (var param in FindConditionPartParams(paramName, condition.Right))
                            yield return param;
                    }
                }
        }
        private static IEnumerable<QueryConditionValueDef> FindConditionsParams(string paramName,
            IEnumerable<SqlQueryCondition> conditions)
        {
            if (conditions != null)
                foreach (var condition in conditions)
                {
                    if (condition.Condition == ConditionOperation.Include ||
                        condition.Condition == ConditionOperation.Exp)
                    {
                        foreach (var param in condition.Conditions.FindParams(paramName)) //FindConditionsParams(paramName, condition.Conditions))
                            yield return param;
                    }
                    else
                    {
                        foreach (var param in FindConditionPartParams(paramName, condition.Left))
                            yield return param;
                        foreach (var param in FindConditionPartParams(paramName, condition.Right))
                            yield return param;
                    }
                }
        }
        private static IEnumerable<QueryConditionValueDef> GetConditionsParams(IEnumerable<SqlQueryCondition> conditions)
        {
            if (conditions != null)
                foreach (var condition in conditions)
                {
                    if (condition.Condition == ConditionOperation.Include ||
                        condition.Condition == ConditionOperation.Exp)
                    {
                        foreach (var param in condition.Conditions.GetParams()) // GetConditionsParams(condition.Conditions))
                            yield return param;
                    }
                    else
                    {
                        foreach (var param in GetConditionPartParams(condition.Left))
                            yield return param;
                        foreach (var param in GetConditionPartParams(condition.Right))
                            yield return param;
                    }
                }
        }
        private static IEnumerable<QueryConditionValueDef> FindConditionPartParams(string paramName, SqlQueryConditionPart part)
        {
            if (part.Params != null)
                foreach (var param in part.Params.Where(
                    param =>
                        param.Name != null &&
                        String.Equals(param.Name, paramName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    yield return param;
                }
            if (part.SubQuery != null)
            {
                foreach (var param in part.SubQuery.FindParams(paramName))
                    yield return param;
            }
        }
        private static IEnumerable<QueryConditionValueDef> GetConditionPartParams(SqlQueryConditionPart part)
        {
            if (part.Params != null)
                foreach (var param in part.Params.Where(param => !String.IsNullOrEmpty(param.Name)))
                {
                    yield return param;
                }
            if (part.SubQuery != null)
            {
                foreach (var param in part.SubQuery.GetAllParams())
                    yield return param;
            }
        }

        /*private const string DocListAttrJoin =
            @"(
SELECT dla.Document_Id, dla.Value FROM DocumentList_Attributes dla
WHERE dla.Def_Id = '{0}' AND dla.Expired = convert(date, '31/12/9999', 103)) AS [{1}] ON [{1}].Document_Id = [{2}].Id";

        private const string UserVisibilityRestrict = @"([{0}].[{1}] IN (
    SELECT [Id] FROM Organizations WHERE [Type_Id] IN 
        (SELECT [ObjDef_Id] FROM [OrgUnits_ObjectDefs] WHERE [OrgUnit_Id] = '{2}')) OR
    [{0}].[{1}] = '{3}')";*/

        public SqlBuilder BuildSql()
        {
            return BuildSqlScript(TopNo > 0 || SkipNo > 0);
        }

        private void InitSystemParams()
        {
            var userId = UserId;
            UserInfo userInfo = null;

            if (UserId == Guid.Empty && Provider != null)
            {
                userId = Provider.GetCurrentUserId();
            }
            if (userId != Guid.Empty && _userRepo != null)
                userInfo = _userRepo.FindUserInfo(userId);

            SetParams(SystemParamIdentConverter.Convert(SystemParamIdent.UserId), userId);
            if (userInfo != null)
            {
                SetParams(SystemParamIdentConverter.Convert(SystemParamIdent.UserName), userInfo.UserName);
                SetParams(SystemParamIdentConverter.Convert(SystemParamIdent.UserOrgId), userInfo.OrganizationId);
                SetParams(SystemParamIdentConverter.Convert(SystemParamIdent.UserPositionId), userInfo.PositionId);
                SetParams(SystemParamIdentConverter.Convert(SystemParamIdent.UserOrgTypeId), userInfo.OrganizationTypeId);
                SetParams(SystemParamIdentConverter.Convert(SystemParamIdent.UserOrgName), userInfo.OrganizationName);
                SetParams(SystemParamIdentConverter.Convert(SystemParamIdent.UserPositionName), userInfo.PositionName);
            }
            SetParams(SystemParamIdentConverter.Convert(SystemParamIdent.Today), DateTime.Today);
            SetParams(SystemParamIdentConverter.Convert(SystemParamIdent.Now), DateTime.Now);
        }

        public override SqlBuilder BuildSql(ICollection<SqlQueryCondition> conditions, bool isMain = false)
        {
            /*throw new NotImplementedException();*/
            return BuildSqlScript(false, false);
        }

        public override SqlBuilder BuildSql(ICollection<SqlQuerySourceAttribute> attrs, ICollection<SqlQueryCondition> conditions, bool isMain = false)
        {
            throw new NotImplementedException();
        }

        public string BuildCountSqlScript()
        {
            SqlQueryAttribute idAttr = GetAttribute("&Id");
            var sql = BuildSqlScript(false, false, true);
            return String.Format("SELECT COUNT([t].[{0}]) FROM ({1}) AS [t]", idAttr.AliasName, sql);
        }

        public string BuildSumSqlScript(string attrDefName)
        {
            SqlQueryAttribute sumAttr = GetAttribute(attrDefName);
            var sql = BuildSqlScript(false, false, true, sumAttr);
            return String.Format("SELECT SUM([t].[{0}]) FROM ({1}) AS [t]", sumAttr.AliasName, sql);
        }

        public string BuildSumSqlScript(Guid attrDefId)
        {
            SqlQueryAttribute sumAttr = GetAttribute(attrDefId);
            var sql = BuildSqlScript(false, false, true, sumAttr);
            return String.Format("SELECT SUM([t].[{0}]) FROM ({1}) AS [t]", sumAttr.AliasName, sql);
        }

        public string BuildMaxSqlScript(string attrDefName)
        {
            SqlQueryAttribute sumAttr = GetAttribute(attrDefName);
            var sql = BuildSqlScript(false, false, true, sumAttr);
            return String.Format("SELECT MAX([t].[{0}]) FROM ({1}) AS [t]", sumAttr.AliasName, sql);
        }

        public string BuildMaxSqlScript(Guid attrDefId)
        {
            SqlQueryAttribute sumAttr = GetAttribute(attrDefId);
            var sql = BuildSqlScript(false, false, true, sumAttr);
            return String.Format("SELECT MAX([t].[{0}]) FROM ({1}) AS [t]", sumAttr.AliasName, sql);
        }

        public string BuildMinSqlScript(string attrDefName)
        {
            SqlQueryAttribute sumAttr = GetAttribute(attrDefName);
            var sql = BuildSqlScript(false, false, true, sumAttr);
            return String.Format("SELECT MIN([t].[{0}]) FROM ({1}) AS [t]", sumAttr.AliasName, sql);
        }

        public string BuildMinSqlScript(Guid attrDefId)
        {
            SqlQueryAttribute sumAttr = GetAttribute(attrDefId);
            var sql = BuildSqlScript(false, false, true, sumAttr);
            return String.Format("SELECT MIN([t].[{0}]) FROM ({1}) AS [t]", sumAttr.AliasName, sql);
        }

        public SqlBuilder BuildSqlScript(bool rowLimit, bool withOrdering = true, bool forGroup = false, 
            SqlQueryAttribute groupAttr = null)
        {
            InitSystemParams();

            var sourceAttrs = new List<SqlQuerySourceAttribute>();

            if (forGroup && groupAttr == null)
            {
                SqlQueryAttribute idAttr = GetAttribute("&Id");

                sourceAttrs.AddRange(from attr in idAttr.Attributes select attr.Attribute);
            }
            else if (forGroup)
            {
                sourceAttrs.AddRange(from attr in groupAttr.Attributes select attr.Attribute);
            }
            else
            {
                if (Attributes != null)
                    sourceAttrs.AddRange(from attr in Attributes from sAttr in attr.Attributes select sAttr.Attribute);
            }

            foreach (var join in _sourceJoins)
            {
                if (join.JoinAttribute != null)
                {
                    if (!sourceAttrs.Contains(join.JoinAttribute)) sourceAttrs.Add(join.JoinAttribute);
                }
                else if (join.Conditions != null)
                    FillConditionAttributes(join.Conditions, sourceAttrs);
            }

            FillConditionAttributes(_conditions, sourceAttrs);

            foreach (var attr in _groupAttributes)
            {
                foreach (var sAttr in attr.Attributes)
                {
                    if (!sourceAttrs.Contains(sAttr.Attribute)) sourceAttrs.Add(sAttr.Attribute);
                }
            }

            if (!forGroup)
                foreach (var attr in _orderAttributes)
                {
                    foreach (var sAttr in attr.Attributes)
                    {
                        if (!sourceAttrs.Contains(sAttr.Attribute)) sourceAttrs.Add(sAttr.Attribute);
                    }
                }

            foreach (var cond in _havingConditions)
            {
                foreach (var sCond in cond.Left.Attributes)
                {
                    if (!sourceAttrs.Contains(sCond.Attribute)) sourceAttrs.Add(sCond.Attribute);
                }
                foreach (var sCond in cond.Right.Attributes)
                {
                    if (!sourceAttrs.Contains(sCond.Attribute)) sourceAttrs.Add(sCond.Attribute);
                }
            }

            if (sourceAttrs.Count == 0)
            {
                SqlQueryAttribute idAttr = GetAttribute("&Id");
                sourceAttrs.AddRange(from attr in idAttr.Attributes select attr.Attribute);
            }

            return BuildSqlScript(rowLimit, withOrdering, sourceAttrs);
        }

        private static void FillConditionAttributes(IEnumerable<SqlQueryCondition> conditions, ICollection<SqlQuerySourceAttribute> sourceAttrs)
        {
            foreach (var cond in conditions)
            {
                if (cond.Condition == ConditionOperation.Exp)
                {
                    if (cond.Conditions != null) FillConditionAttributes(cond.Conditions, sourceAttrs);
                }
                else
                {
                    foreach (var sCond in cond.Left.Attributes)
                    {
                        if (!sourceAttrs.Contains(sCond.Attribute)) sourceAttrs.Add(sCond.Attribute);
                    }
                    foreach (var sCond in cond.Right.Attributes)
                    {
                        if (!sourceAttrs.Contains(sCond.Attribute)) sourceAttrs.Add(sCond.Attribute);
                    }
                }
            }

        }

        protected bool HasOrConditions()
        {
            return Conditions.Any(condition => condition.Operation == ExpressionOperation.Or || condition.Operation == ExpressionOperation.OrNot);
        }

        protected void DivideContitions(List<SqlQueryCondition> subQueryConditions, List<SqlQueryCondition> externalConditions)
        {
            var hasOr = !HasOrConditions();
            foreach (var condition in Conditions)
            {
/*
                    var enable = true;
                    foreach (var attr in condition.Attributes)
                    {
                        if (attr.Source != condition.Source)
                        {
                            enable = false;
                            break;
                        }
                    }
*/
                if (!hasOr)
                {
                    if (condition.Left.Attributes.Count == 1 && condition.Right.Attributes.Count == 0 &&
                        (condition.Left.Attribute.Def != null || //condition.Attribute.Ident == SystemIdent.InState ||
                         condition.Left.Attribute.Ident == SystemIdent.State) &&
                        condition.Condition != ConditionOperation.IsNull)
                        subQueryConditions.Add(condition);
                    else
                        externalConditions.Add(condition);
                }
                else 
                    externalConditions.Add(condition);
            }
        }

        private SqlBuilder BuildSqlScript(bool rowLimit, bool withOrdering, ICollection<SqlQuerySourceAttribute> attributes)
        {
            var attrs = new List<SqlQuerySourceAttribute>(attributes);

            if ((rowLimit) && OrderAttributes.Count == 0)
            {
                if (GroupAttributes.Count > 0)
                {
                    var groupAttr = GroupAttributes[0];
                    if (groupAttr.IsSystemIdent)
                    {
                        var orderAttr = AddOrderAttribute(groupAttr.Attribute.AliasName);
                        attrs.AddRange(from oAttr in orderAttr.Attributes where !attrs.Contains(oAttr.Attribute) select oAttr.Attribute);
                    }
                    else
                    {
                        var orderAttr = AddOrderAttribute(groupAttr.Def.Id);
                        attrs.AddRange(from oAttr in orderAttr.Attributes where !attrs.Contains(oAttr.Attribute) select oAttr.Attribute);
                    }
                }
                else
                {
                    var orderAttr = AddOrderAttribute("&Modified", false);
                    attrs.AddRange(from oAttr in orderAttr.Attributes where !attrs.Contains(oAttr.Attribute) select oAttr.Attribute);
                }
            }

            var builder = new SqlBuilder();

            var subQueryConditions = new List<SqlQueryCondition>();
//            var externalConditions = new List<SqlQueryCondition>();
//
//            DivideContitions(subQueryConditions, externalConditions);

            var sourceConditions = GetSourceConditions(Source);
            subQueryConditions.AddRange(sourceConditions);
            var externalConditions = GetExternalSourceConditions(Source, sourceConditions);
            attrs.AddRange(from a in GetConditionAttributes(externalConditions) where !attrs.Contains(a) select a);

            var baseSource = attrs.Count > 0
                                 ? Source.BuildSql(attrs, /*null*/ sourceConditions, true)
                                 : Source.BuildSql(/*null*/ sourceConditions, true);

            builder.SetFrom(String.Format("({0}) as [{1}]", baseSource, Source.AliasName));

            if (rowLimit)
            {
                builder.TopNo = TopNo;
                builder.SkipNo = SkipNo;
            }

            if (Attributes != null)
                foreach (var attr in Attributes)
                {
                    var allContains = true;

                    foreach(var sAttr in attr.Attributes)
                    {
                        if (!attributes.Contains(sAttr.Attribute))
                            allContains = false;
                    }

                    if (allContains)
                    {
                        var s = attr.GetExpression();
                        if (!String.IsNullOrEmpty(attr.Alias))
                            s = String.Format("{0} as [{1}]", s, attr.Alias);
                        builder.AddSelect(s);
                    }
                }
            if (Attributes == null || Attributes.Count == 0)
            {
                builder.AddSelect(String.Format("[{0}].[{1}]", Source.AliasName, "Id"));
            }

            var withOption = WithNoLock ? "WITH(NOLOCK)" : "";
            var docListAlias = String.IsNullOrEmpty(DocListAlias) ? "DocList" : DocListAlias;

            if (DocumentId != Guid.Empty && ListAttrDefId != Guid.Empty)
            {
                
                builder.AddJoin(SqlSourceJoinType.Inner,
                                String.Format(
                                    "(SELECT [Value] FROM [DocumentList_Attributes] {4}\n" +
                                    " WHERE [Document_Id] = '{0}' AND [Def_Id] = '{1}' AND [Expired] = '99991231') as [{2}]\n" +
                                    "ON [{2}].[Value] = [{3}].[Id]", DocumentId, ListAttrDefId, docListAlias, Source.AliasName, withOption));
            }

            foreach (var join in SourceJoins)
            {
                sourceConditions = GetSourceConditions(join.Target);
                subQueryConditions.AddRange(sourceConditions);

                externalConditions = GetExternalSourceConditions(join.Target, sourceConditions);
                attrs.AddRange(from a in GetConditionAttributes(externalConditions) where !attrs.Contains(a) select a);

                BuildJoinSources(builder, join, attrs, /*null*/ sourceConditions); 
            }

/*
            int i = 1;
            foreach(var cond in subQueryConditions)
            {
                builder.AddJoin(SqlSourceJoinType.Inner, cond.GetConditionInnerQuery(i));
                i++;
            }
*/
//            if (externalConditions != null)
//                foreach (var cond in externalConditions)
//                {
//                    builder.AddCondition(cond.Operation, cond.GetConditionExp());
//                }

            foreach (var condition in Conditions)
                if (!subQueryConditions.Contains(condition))
                {
                    if (condition.Condition == ConditionOperation.Exp)
                    {
                        BuildExpConditions(builder, condition, subQueryConditions);
                    }
                    else
                        builder.AddCondition(condition.Operation, condition.GetConditionExp());
                }

            if (GroupAttributes != null)
            {
                foreach (var attr in GroupAttributes)
                    builder.AddGroupBy(attr.GetExpression());

                if (HavingConditions.Count > 0)
                    foreach (var condition in HavingConditions)
                        builder.AddHaving(condition.Operation, condition.GetConditionExp());
            }

            if (withOrdering && OrderAttributes != null)
                foreach (var attr in OrderAttributes)
                    builder.AddOrderBy(attr.GetExpression());

            return builder;
        }

        private static void BuildExpConditions(SqlBuilder builder, SqlQueryCondition condition, ICollection<SqlQueryCondition> subQueryConditions)
        {
            builder.AddWhereExp(condition.Operation);
            try
            {
                foreach (var sub in condition.Conditions)
                    if (!subQueryConditions.Contains(sub))
                    {
                        if (sub.Condition == ConditionOperation.Exp)
                        {
                            BuildExpConditions(builder, sub, subQueryConditions);
                        }
                        else
                            builder.AddCondition(sub.Operation, sub.GetConditionExp());
                    }
            }
            finally
            {
                builder.EndWhereExp();
            }
        }

        private static IEnumerable<SqlQuerySourceAttribute> GetConditionAttributes(IEnumerable<SqlQueryCondition> conditions)
        {
            var attrs = new List<SqlQuerySourceAttribute>();

            foreach (var sCond in
                conditions.SelectMany(cond => cond.Attributes.Where(sCond => !attrs.Contains(sCond.Attribute))))
            {
                attrs.Add(sCond.Attribute);
                yield return sCond.Attribute;
            }
        }

        private ICollection<SqlQueryCondition> GetExternalSourceConditions(SqlQuerySource source, 
            ICollection<SqlQueryCondition> sourceConditions)
        {
            var externalConditions = new List<SqlQueryCondition>();

            FillExternalSourceConditions(source, Conditions, sourceConditions, externalConditions);

            return externalConditions;
        }

        private static void FillExternalSourceConditions(SqlQuerySource source, IEnumerable<SqlQueryCondition> conditions,
            ICollection<SqlQueryCondition> sourceConditions, ICollection<SqlQueryCondition> externalConditions)
        {
            foreach (var condition in conditions)
            {
                if (condition.Condition == ConditionOperation.Include || condition.Condition == ConditionOperation.Exp)
                {
                    if (sourceConditions.Contains(condition)) continue;

                    if (condition.Conditions == null || condition.Conditions.Count == 0)
                        continue;

                    if (HasOrConditions(condition.Conditions))
                        externalConditions.Add(condition);
                    else
                        FillExternalSourceConditions(source, condition.Conditions, sourceConditions, externalConditions);
                }
                else
                {
                    if (sourceConditions.Contains(condition)) continue;

                    var sources = new List<SqlQuerySource>();

                    FillConditionSources(sources, condition);

                    if (sources.Contains(source))
                        externalConditions.Add(condition);
                }
            }
        }

        private ICollection<SqlQueryCondition> GetSourceConditions(SqlQuerySource source)
        {
            var sourceConditions = new List<SqlQueryCondition>();

            FillSourceConditions(source, Conditions, sourceConditions);

            return sourceConditions;
        }

        protected static bool HasOrConditions(ICollection<SqlQueryCondition> conditions)
        {
            return conditions.Any(c =>
                                  c.Operation == ExpressionOperation.Or ||
                                  c.Operation == ExpressionOperation.OrNot);
        }

        private void FillSourceConditions(SqlQuerySource source,
            ICollection<SqlQueryCondition> conditions, ICollection<SqlQueryCondition> sourceConditions)
        {
            if (conditions == null) return;

            if (!HasOrConditions(conditions))
                foreach (var condition in conditions)
                {
                    if (condition.Condition == ConditionOperation.Include || condition.Condition == ConditionOperation.Exp)
                    {
                        if (condition.Conditions == null || condition.Conditions.Count == 0)
                            continue;

                        if (HasOrConditions(condition.Conditions))
                        {
                            var sources = GetConditionListSources(condition.Conditions);

                            if (sources.Count == 1 && sources[0] == source) sourceConditions.Add(condition);
                                /*foreach (var sub in conditions)
                                    sourceConditions.Add(sub);*/
                        }
                        else
                            FillSourceConditions(source, condition.Conditions, sourceConditions);
                    }
                    else
                    {
                        var sources = new List<SqlQuerySource>();

                        FillConditionSources(sources, condition);

                        if (sources.Count == 1 && sources[0] == source)
                            sourceConditions.Add(condition);
                    }
                }
            else
            {
                var sources = GetConditionListSources(conditions);

                if (sources.Count == 1 && sources[0] == source)
                    foreach (var condition in conditions)
                        sourceConditions.Add(condition);
            }
        }

        private static void BuildJoinSources(SqlBuilder builder, SqlQueryJoin join, ICollection<SqlQuerySourceAttribute> attrs,
                                             ICollection<SqlQueryCondition> conditions)
        {
            var sourceSb = (attrs != null && attrs.Count > 0)
                               ? join.Target.BuildSql(attrs, conditions)
                               : join.Target.BuildSql(conditions);

            var sb = new StringBuilder(String.Format("({0}) as [{1}] on ", sourceSb, join.Target.AliasName));

            if (join.JoinAttribute != null)
            {
                if (join.IsTargetAttribute)
                {
                    sb.Append(String.Format("[{0}].Id = [{1}].[{2}]", join.Source.AliasName, join.Target.AliasName,
                        join.JoinAttribute.AliasName));
                }
                else
                {
                    sb.Append(String.Format("[{0}].Id = [{1}].[{2}]", join.Target.AliasName, join.Source.AliasName,
                        join.JoinAttribute.AliasName));
                }
            }
            else
            {
                sb.Append(join.BuildConditions());
            }

            if (conditions != null && conditions.Count > 0)
                builder.AddJoin(SqlSourceJoinType.Inner, sb.ToString());
            else
                builder.AddJoin(join.JoinType, sb.ToString());
        }

        public SqlQuerySource FindSourceByAttributeId(Guid attrDefId)
        {
            if (Source.HasAttributeDef(attrDefId)) return Source;

            return Sources.FirstOrDefault(source => source.HasAttributeDef(attrDefId));
        }

        public SqlQuerySource GetSourceByAttributeId(Guid attrDefId)
        {
            var source = FindSourceByAttributeId(attrDefId);

            if (source == null) 
                throw new ApplicationException(String.Format("Источник \"{0}\" не найден!", attrDefId));

            return source;
        }

        public SqlQuerySource FindSourceByAttributeName(string attrDefName)
        {
            if (Source.HasAttributeDef(attrDefName)) return Source;

            return Sources.FirstOrDefault(source => source.HasAttributeDef(attrDefName));
        }

        public SqlQuerySource GetSourceByAttributeName(string attrDefName)
        {
            var source = FindSourceByAttributeName(attrDefName);

            if (source == null) 
                throw new ApplicationException(String.Format("Источник \"{0}\" не найден!", attrDefName));

            return source;
        }

        public SqlQuerySource FindSourceByAlias(string alias)
        {
            if (String.IsNullOrEmpty(alias)) return Source;

            if (String.Equals(Source.AliasName, alias, StringComparison.OrdinalIgnoreCase)) return Source;

            return
                Sources.FirstOrDefault(
                    source => String.Equals(source.AliasName, alias, StringComparison.OrdinalIgnoreCase));
        }
        public SqlQuerySource FindSourceById(Guid docDefId)
        {
            if (Source != null && Source.IsDocDef(docDefId)) return Source;

            return
                Sources.FirstOrDefault(source => source.IsDocDef(docDefId));
        }

        public SqlQuerySource FindSourceByDocDef(DocDef docDef)
        {
            if (Source.IsDocDef(docDef)) return Source;

            return Sources.FirstOrDefault(source => source.IsDocDef(docDef));
        }

        public SqlQuerySource FindSourceByAttrDef(AttrDef attrDef)
        {
//            if (Source.IsDocDef(attrDef.DocDefType.Id)) return Source;

            var j = SourceJoins.FirstOrDefault(join => join.JoinAttrDef.Id == attrDef.Id);
            if (j == null) return null;
            return j.Target;
        }

        public SqlQuerySource FindSourceByAttrDef(Guid attrDefId)
        {
            //            if (Source.IsDocDef(attrDef.DocDefType.Id)) return Source;

            var j = SourceJoins.FirstOrDefault(join => join.JoinAttrDef != null && join.JoinAttrDef.Id == attrDefId);
            if (j == null) return null;
            return j.Target;
        }

        public SqlQuerySource FindSource(QuerySourceDef sourceDef)
        {
            if (sourceDef == null) return null;

            if (Source.IsSame(sourceDef)) return Source;

            return Sources.FirstOrDefault(s => s != null && s.IsSame(sourceDef));

            /*var j = SourceJoins.FirstOrDefault(join => join.Source != null && join.Source.IsSame(sourceDef));
            if (j == null) return null;
            return j.Target;*/
        }

        public SqlQuerySource GetSourceByDocDef(DocDef docDef)
        {
            var source = FindSourceByDocDef(docDef);

            if (source == null)
                throw new ApplicationException(String.Format("Источник данных \"{0}\" не найден", 
                                                             docDef.Caption + " - " + docDef.Name));

            return source;
        }

        public SqlQuerySource FindSourceByDocDef(Guid docDefId)
        {
            if (Source.IsDocDef(docDefId)) return Source;

            return Sources.FirstOrDefault(source => source.IsDocDef(docDefId));
        }

        public SqlQuerySource GetSourceByDocDef(Guid docDefId)
        {
            var source = FindSourceByDocDef(docDefId);

            if (source == null)
                throw new ApplicationException(String.Format("Источник данных \"{0}\" не найден",
                                                             docDefId));

            return source;
        }

        private void CheckSourceAlias(string alias)
        {
            if (String.IsNullOrEmpty(alias))
                throw new ApplicationException("Алиас не может быть пустой строкой!");
            if (alias.Contains(" "))
                throw new ApplicationException("Алиас не может содержать пробелы!");
            if (_sourceAliases != null && _sourceAliases.Contains(alias.ToUpper()))
                throw new ApplicationException("Такой алиас уже существует!");

        }

        public string GetSourceAlias(string alias)
        {
            if (!String.IsNullOrEmpty(alias))
            {
                int i = 0;
                alias = alias.Replace(" ", "_");
                if (_sourceAliases.Contains(alias.ToUpper()))
                {
                    do
                    {
                        i++;
                        alias = alias + i;
                    } while (_sourceAliases.Contains(alias.ToUpper()));
                }
            }
            else
            {
                do
                {
                    _sourceAliasNo++;
                    alias = "doc" + _sourceAliasNo;
                } while (_sourceAliases.Contains(alias.ToUpper()));
            }
            return alias;
        }

        public SqlQuerySource GetSourceByDocDef(string docDefName)
        {
            var source = Source;

            if (String.Equals(source.GetDocDef().Name, docDefName, StringComparison.OrdinalIgnoreCase))
                return source;

            return
                Sources.First(
                    s => String.Equals(s.GetDocDef().Name, docDefName, StringComparison.OrdinalIgnoreCase));
        }

        private void AddSourceAlias(string alias)
        {
            _sourceAliases.Add(alias.ToUpper());
        }

        protected string GetAttributeAlias(string alias)
        {
            if (AttributeAliases == null) AttributeAliases = new List<string>();

            if (!String.IsNullOrEmpty(alias))
            {
                int i = 0;
                alias = alias.Replace(" ", "_");
                alias = alias.Replace("&", "");

                if (String.Equals(alias, "Id", StringComparison.OrdinalIgnoreCase) ||
                    AttributeAliases.Contains(alias.ToUpper()))
                {
                    do
                    {
                        i++;
                        alias = alias + i;
                    } while (AttributeAliases.Contains(alias.ToUpper()));
                }
            }
            else
            {
                do
                {
                    AttributeAliasNo++;
                    alias = "attr" + AttributeAliasNo;
                } 
                while (AttributeAliases.Contains(alias.ToUpper()));
            }
            return alias;
        }

        public void Dispose()
        {
            if (_ownProvider && Provider != null)
            {
                Provider.Dispose();
                Provider = null;
            } 
            /*if (_ownDataContext && DataContext != null)
            {
                DataContext.Dispose();
                DataContext = null;
            }
            if (_ownDocDefRepo && _docDefRepo != null)
            {
                _docDefRepo.Dispose();
                _docDefRepo = null;
            }*/
        }

        /*~SqlQuery()
        {
            if (_ownDocDefRepo && _docDefRepository != null)
                _docDefRepository.Dispose();
        }*/

    }
}