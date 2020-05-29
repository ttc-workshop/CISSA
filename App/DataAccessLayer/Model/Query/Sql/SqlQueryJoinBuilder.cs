using System;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryJoinBuilder
    {
        public SqlQuery Query { get; private set; }
        public SqlQuerySource Source { get; private set; }
        public SqlQueryJoinBuilder Parent { get; private set; }

        public SqlQueryJoinBuilder(SqlQueryJoinBuilder parent, SqlQuery query, SqlQuerySource source)
        {
            Query = query;
            Source = source;
            Parent = parent;
        }

        public SqlQueryJoinBuilder(SqlQuery query, SqlQuerySource source) : this(null, query, source) {}

        public SqlQueryJoinBuilder Join(string attrDefName)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, Source.GetDocDef(), SqlSourceJoinType.Inner,
                                                            attrDefName));
        }

        public SqlQueryJoinBuilder Join(Guid attrDefId)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, Source.GetDocDef(), SqlSourceJoinType.Inner,
                                                            attrDefId));
        }

        public SqlQueryJoinBuilder Join(Guid docDefId, string attrDefName)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDefId, SqlSourceJoinType.Inner,
                                                            attrDefName));
        }

        public SqlQueryJoinBuilder Join(Guid docDefId, Guid attrDefId)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDefId, SqlSourceJoinType.Inner,
                                                            attrDefId));
        }

        public SqlQueryJoinBuilder Join(DocDef docDef, string attrDefName)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDef, SqlSourceJoinType.Inner,
                                                            attrDefName));
        }

        public SqlQueryJoinBuilder Join(DocDef docDef, Guid attrDefId)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDef, SqlSourceJoinType.Inner,
                                                            attrDefId));
        }

        public SqlQueryJoinBuilder LeftOuterJoin(string attrDefName)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, Source.GetDocDef(), SqlSourceJoinType.LeftOuter,
                                                            attrDefName));
        }

        public SqlQueryJoinBuilder LeftOuterJoin(Guid attrDefId)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, Source.GetDocDef(), SqlSourceJoinType.LeftOuter,
                                                            attrDefId));
        }

        public SqlQueryJoinBuilder LeftOuterJoin(Guid docDefId, string attrDefName)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDefId, SqlSourceJoinType.LeftOuter,
                                                            attrDefName));
        }

        public SqlQueryJoinBuilder LeftOuterJoin(Guid docDefId, Guid attrDefId)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDefId, SqlSourceJoinType.LeftOuter,
                                                            attrDefId));
        }

        public SqlQueryJoinBuilder LeftOuterJoin(DocDef docDef, string attrDefName)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDef, SqlSourceJoinType.LeftOuter,
                                                            attrDefName));
        }

        public SqlQueryJoinBuilder LeftOuterJoin(DocDef docDef, Guid attrDefId)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDef, SqlSourceJoinType.LeftOuter,
                                                            attrDefId));
        }

        public SqlQueryJoinBuilder RightOuterJoin(string attrDefName)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, Source.GetDocDef(), SqlSourceJoinType.RightOuter,
                                                            attrDefName));
        }

        public SqlQueryJoinBuilder RightOuterJoin(Guid attrDefId)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, Source.GetDocDef(), SqlSourceJoinType.RightOuter,
                                                            attrDefId));
        }

        public SqlQueryJoinBuilder RightOuterJoin(Guid docDefId, string attrDefName)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDefId, SqlSourceJoinType.RightOuter,
                                                            attrDefName));
        }

        public SqlQueryJoinBuilder RightOuterJoin(Guid docDefId, Guid attrDefId)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDefId, SqlSourceJoinType.RightOuter,
                                                            attrDefId));
        }

        public SqlQueryJoinBuilder RightOuterJoin(DocDef docDef, string attrDefName)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDef, SqlSourceJoinType.RightOuter,
                                                            attrDefName));
        }

        public SqlQueryJoinBuilder RightOuterJoin(DocDef docDef, Guid attrDefId)
        {
            return new SqlQueryJoinBuilder(this, Query,
                                           Query.JoinSource(Source, docDef, SqlSourceJoinType.RightOuter,
                                                            attrDefId));
        }

        public SqlQueryJoinBuilder End()
        {
            return Parent;
        }
    }

    public class SqlQuerySelectBuilder
    {
    }
}