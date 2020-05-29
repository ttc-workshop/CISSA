using System;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQuerySourceAttributeRef
    {
        public SqlQuerySource Source { get; private set; }
        public SqlQuerySourceAttribute Attribute { get; private set; }

        public SqlQuerySourceAttributeRef(SqlQuerySource source, SqlQuerySourceAttribute attribute)
        {
            Source = source;
            Attribute = attribute;
        }

        public SqlQuerySourceAttributeRef(SqlQuerySource source, Guid attrDefId)
        {
            Source = source;
            Attribute = source.GetAttribute(attrDefId);
        }

        public SqlQuerySourceAttributeRef(SqlQuerySource source, string attrDefName)
        {
            Source = source;
            Attribute = source.GetAttribute(attrDefName);
        }

        public SqlQuerySourceAttributeRef(SqlQuerySource source, SystemIdent attrIdent)
        {
            Source = source;
            Attribute = source.GetAttribute(attrIdent);
        }
    }
}