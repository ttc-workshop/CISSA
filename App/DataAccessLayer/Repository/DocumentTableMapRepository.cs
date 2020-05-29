using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Maps;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class DocumentTableMapRepository: IDocumentTableMapRepository
    {
        private static bool _prepared;
        private static readonly List<DocumentTableMap> Maps = new List<DocumentTableMap>();

        private const int LockTimeout = 500000;

        public IDataContext DataContext { get; private set; }

        public DocumentTableMapRepository(IDataContext dataContext)
        {
            DataContext = dataContext;
        }

        //private static readonly object PrepareLock = new object();
        private static readonly ReaderWriterLock PrepareLock = new ReaderWriterLock();

        #region Implementation of IDocumentTableMapRepository

        public DocumentTableMap Find(Guid docDefId)
        {
            //lock(PrepareLock)
            PrepareLock.AcquireReaderLock(LockTimeout);
            try
            {
                if (_prepared)
                    return Maps.FirstOrDefault(m => m.DocDefId == docDefId);

                var lc = PrepareLock.UpgradeToWriterLock(LockTimeout);
                try
                {
                    if (!_prepared) Prepare(DataContext);
                }
                finally
                {
                    PrepareLock.DowngradeFromWriterLock(ref lc);
                }

                return Maps.FirstOrDefault(m => m.DocDefId == docDefId);
            }
            finally
            {
                PrepareLock.ReleaseReaderLock();
            }
        }

        public DocumentTableMap Get(Guid docDefId)
        {
            var map = Find(docDefId);

            if (map == null)
                throw new ApplicationException(String.Format("DocumentTableMap для \"{0}\" не найден", docDefId));

            return map;
        }

        #endregion

        private const string GetTableListSql = "SELECT TABLE_NAME, TABLE_TYPE FROM information_schema.tables";

        private static void Prepare(IDataContext dataContext)
        {
            using (var command = dataContext.CreateCommand(GetTableListSql))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tableName = reader.GetString(0);
                        var tableType = !reader.IsDBNull(1) ? reader.GetString(1) : String.Empty;

                        Guid tableDocDefId;

                        if (Guid.TryParse(tableName, out tableDocDefId))
                        {
                            AddMap(new DocumentTableMap(tableDocDefId, tableName,
                                                        String.Equals(tableType, "VIEW", StringComparison.OrdinalIgnoreCase)));
                        }
                        else if (tableName.Length == 38) // Временно отключено
                        {
                            var s = tableName.Substring(2).Replace('_', '-');

                            if (Guid.TryParse(s, out tableDocDefId))
                                AddMap(new DocumentTableMap(tableDocDefId, tableName,
                                                            String.Equals(tableType, "VIEW", StringComparison.OrdinalIgnoreCase)));
                        }
                    }
                }
            }
            if (Maps.Count > 0) PrepareAttributes(dataContext);

            Maps.AddRange(MetaobjectDefs.GetMetaobjectTableMaps()); // Добавляет связи с метаобъектами

            _prepared = true;
        }

        private const string GetTableFieldListSql = "select c.Column_Name, c.Data_Type, c.Is_Nullable, cc.Is_Computed " +
                                                    "from INFORMATION_SCHEMA.COLUMNS c " +
                                                      "left outer join sys.computed_columns cc on " +
                                                        "(cc.Object_Id = Object_Id(c.Table_Name) and cc.Name = c.Column_Name) " +
                                                    "where c.TABLE_NAME = @0";

        private static void AddMap(DocumentTableMap map)
        {
            Maps.Add(map);
        }

        private static void PrepareAttributes(IDataContext dataContext)
        {
            using (var command = dataContext.CreateCommand(GetTableFieldListSql))
            {
                var param = command.CreateParameter();
                param.ParameterName = "@0";
                param.DbType = DbType.AnsiString;
                param.Direction = ParameterDirection.Input;
                command.Parameters.Add(param);

                foreach (var map in Maps)
                {
                    param.Value = map.TableName;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var attrName = reader.GetString(0);
                            var attrType = !reader.IsDBNull(1) ? reader.GetString(1) : String.Empty;
                            var attrIsNull = !reader.IsDBNull(2) ? reader.GetString(2) : "YES";
                            var attrIsComputed = !reader.IsDBNull(3) && reader.GetBoolean(3);
                            var attrKind = attrIsComputed
                                               ? (AttributeFieldType.Search | AttributeFieldType.Order)
                                               : (AttributeFieldType.View | AttributeFieldType.Data);

                            Guid attrDefId;

                            if (Guid.TryParse(attrName, out attrDefId))
                            {
                                map.Fields.Add(attrIsComputed
                                                   ? new AttributeFieldMap(attrDefId, attrName, attrKind)
                                                   : new AttributeFieldMap(attrDefId, attrName));
                            }
                            else if (attrName.Length == 38)
                            {
                                var pref = attrName[0];
                                var s = attrName.Substring(2).Replace('_', '-');

                                if (pref == 'a' && !attrIsComputed)
                                    attrKind = AttributeFieldType.View | AttributeFieldType.Data |
                                               AttributeFieldType.Search | AttributeFieldType.Order;
                                else //if (pref == 'a')
                                    attrKind = AttributeFieldType.Search | AttributeFieldType.Order;

                                if (Guid.TryParse(s, out attrDefId))
                                    map.Fields.Add(new AttributeFieldMap(attrDefId, attrName, attrKind));
                            }
                            else
                            {
                                map.Fields.Add(new AttributeFieldMap(Guid.Empty, attrName));
                            }
                        }
                    }
                }
            }
        }

        public static void ClearMaps()
        {
            // lock(PrepareLock)
            PrepareLock.AcquireWriterLock(LockTimeout);
            try
            {
                Maps.Clear();
                _prepared = false;
            }
            finally
            {
                PrepareLock.ReleaseWriterLock();
            }
        }
    }
}
