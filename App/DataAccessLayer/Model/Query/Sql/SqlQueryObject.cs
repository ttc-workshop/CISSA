using System;
using System.Collections.Generic;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public abstract class SqlQueryObject: SqlQueryBase
    {
        public abstract SqlQuerySelectAttribute FindAttribute(Guid attrDefId);
        public abstract SqlQuerySelectAttribute FindAttribute(string attrDefName);
        public abstract SqlQuerySelectAttribute FindAttribute(SystemIdent attrIdent);
        public abstract SqlQuerySelectAttribute AddAttribute(Guid attrDefId);
        public abstract SqlQuerySelectAttribute AddAttribute(string attrDefName);
        public abstract SqlQuerySelectAttribute AddAttribute(SystemIdent attrIdent);

        public SqlQuerySelectAttribute GetAttribute(Guid attrDefId)
        {
            return FindAttribute(attrDefId) ?? AddAttribute(attrDefId);
        }
        public SqlQuerySelectAttribute GetAttribute(string attrDefName)
        {
            return FindAttribute(attrDefName) ?? AddAttribute(attrDefName);
        }
        public SqlQuerySelectAttribute GetAttribute(SystemIdent attrIdent)
        {
            return FindAttribute(attrIdent) ?? AddAttribute(attrIdent);
        }

        public abstract DocDef Def { get; }
    }
}