using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Def;
using Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public interface ISqlQueryBuilder
    {
        SqlQuery Build(QueryDef def);
        SqlQuery Build(BizForm form);
        SqlQuery Build(BizForm form, IEnumerable<Guid> docIds, IEnumerable<AttributeSort> sortAttrs);

        SqlQuery Build(Guid docDefId, BizForm filter, IEnumerable<AttributeSort> sortAttrs);
        SqlQuery Build(QueryDef def, BizForm form, IEnumerable<AttributeSort> sortAttrs);
        SqlQuery Build(QueryDef def, BizForm form, BizForm filter, IEnumerable<AttributeSort> sortAttrs);
        SqlQuery Build(BizForm form, Guid? docStateId, BizForm filter, IEnumerable<AttributeSort> sortAttrs);
        SqlQuery BuildAttrList(BizForm form, Guid docId, Guid attrDefId, BizForm filter,
            IEnumerable<AttributeSort> sortAttrs);
        SqlQuery BuildAttrList(BizControl form, Guid docId, Guid docDefId, Guid attrDefId, BizForm filter,
            IEnumerable<AttributeSort> sortAttrs);
        SqlQuery BuildRefList(BizForm form, Guid docId, Guid attrDefId, BizForm filter,
            IEnumerable<AttributeSort> sortAttrs);
        SqlQuery BuildRefList(BizControl form, Guid docId, Guid docDefId, Guid attrDefId, BizForm filter,
            IEnumerable<AttributeSort> sortAttrs);

        // New 2015-06-30
        SqlQuery Build(Guid docDefId);
        SqlQuery BuildAttrList(Doc doc, string attrDefName, string alias);

        // New 2015-07-11
        SqlQuery Build(QueryDefData queryData);

        // New 2015-07-13
        SqlQuery Build(BizControl control, Guid docDefId);
    }
}