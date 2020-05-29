using System;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryMasterSlaveLink
    {
        public SqlQuery Slave { get; private set; }

        public Guid MasterAttributeId { get; private set; }
        public string MasterAttributeName { get; private set; }
        public string SlaveParamName { get; private set; }

        public SqlQueryMasterSlaveLink(SqlQuery slave, Guid attrId, string paramName)
        {
            Slave = slave;
            MasterAttributeId = attrId;
            SlaveParamName = paramName;
        }

        public SqlQueryMasterSlaveLink(SqlQuery slave, string attrName, string paramName)
        {
            Slave = slave;
            MasterAttributeName = attrName;
            SlaveParamName = paramName;
        }

        public void Update(SqlQueryReader masterSource)
        {
            if (!masterSource.Active) return;

            var masterFieldIndex = MasterAttributeId != Guid.Empty
                ? masterSource.TryGetAttributeIndex(MasterAttributeId)
                : masterSource.TryGetAttributeIndex(MasterAttributeName);

            if (masterFieldIndex >= 0)
            {
                Slave.SetParams(SlaveParamName, masterSource.GetValue(masterFieldIndex));
            }
        }
    }
}
