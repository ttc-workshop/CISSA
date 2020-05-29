using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Reflection;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Raven.Abstractions.Util;
using Raven.Client.Document;

namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    [Flags]
    public enum DataContextType
    {
        None = 0,
        Meta = 1, 
        Document = 2, 
        Account = 4
    }

    public class DataContext: IDataContext, IEntityDataContext
    {
        public const int CommandTimeout = 600;

        protected EntityConnection Connection { get; private set; }
        protected DbContext DbContext { get; private set; }
        private readonly bool _ownConnection;
        private bool _connectionDisposed;

        private cissaEntities _entities;

        public string Name { get; private set; }

        public DataContextType DataType { get; private set; }

        public cissaEntities Entities 
        {
            get
            {
                return _entities;
            }
        }

//        protected TransactionScope Transaction;

        public DataContext(EntityConnection connection)
        {
            if (connection == null)
            {
                Connection = new EntityConnection("name=cissaEntities");
                _ownConnection = true;
//                OutputLog();
            }
            else
                Connection = connection;
            
            Name = Connection.Database;
            Connection.Disposed += OnConnectionDisposed;
            _entities = new cissaEntities(Connection) {CommandTimeout = 600};
            DbContext = new DbContext(_entities, true);

        }

        public DataContext() : this((DbConnection) null) {}

        public DataContext(DbConnection connection, string name = "", DataContextType type = DataContextType.Meta | DataContextType.Document | DataContextType.Account)
        {
            if (connection != null)
            {
                var workspace = new MetadataWorkspace(EntityDataContextMetadataLocator.GetPath(),
                    new[] {Assembly.GetExecutingAssembly()});

                Connection = new EntityConnection(workspace, connection);
            }
            else
                Connection = new EntityConnection("name=cissaEntities");

            _ownConnection = true;
            Name = String.IsNullOrEmpty(name) ? Connection.Database : name;
            DataType = type;
            Connection.Disposed += OnConnectionDisposed;
            _entities = new cissaEntities(Connection) { CommandTimeout = 600 };
            DbContext = new DbContext(_entities, true);

        }

        public DataContext(string connectionString)
        {
            Connection = new EntityConnection(connectionString);

            _ownConnection = true;

            Name = Connection.Database;
            Connection.Disposed += OnConnectionDisposed;
            _entities = new cissaEntities(Connection) { CommandTimeout = 600 };
            DbContext = new DbContext(_entities, true);
        }

        private void OnConnectionDisposed(object sender, EventArgs args)
        {
            _connectionDisposed = true;
        }

        public DbConnection StoreConnection
        {
            get
            {
                return Connection.StoreConnection;
            }
        }

        private DocumentStore _cacheStore;
        private bool _noCacheStore;

        private DocumentStore CacheStore
        {
            get
            {
                if (_cacheStore != null) return _cacheStore;
                if (_noCacheStore) return null;

                if (ConfigurationManager.ConnectionStrings["CacheDb"] != null)
                {
                    _cacheStore = new DocumentStore {ConnectionStringName = "CacheDb"};
                    _cacheStore.Initialize();

                    _cacheStore.Conventions.RegisterIdConvention<CacheItem>((dbname, commands, doc) => doc.Id.ToString());
                    _cacheStore.Conventions.RegisterAsyncIdConvention<CacheItem>(
                        (dbname, commands, doc) => new CompletedTask<string>(doc.Id.ToString()));
                }
                else
                    _noCacheStore = true;

                return _cacheStore;
            }
        }

        private int _cacheErrorCount;

        private int _transactionCount;

        protected DbContextTransaction Transaction;

        public void BeginTransaction()
        {
            if (_transactionCount == 0)
            {
                if (!Connection.State.HasFlag(ConnectionState.Open))
                    Connection.Open();

                Transaction = DbContext.Database.BeginTransaction(IsolationLevel.ReadUncommitted);
                ChangeSaved = false;
            }
            _transactionCount++;
        }

        public void Commit()
        {
            if (_transactionCount == 1)
            {
                if (Transaction != null)
                {
                    
                    Transaction.Commit();
                }
                _transactionCount--;

                if (ChangeSaved)
                    Entities.AcceptAllChanges();
            }
            else
                _transactionCount--;

            if (_transactionCount == 0 && Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }
        }

        public void Rollback()
        {
            if (_transactionCount == 1)
            {
                if (Transaction != null) Transaction.Rollback();
            }
            _transactionCount--;
            if (_transactionCount == 0 && Transaction != null)
            {
                Transaction.Dispose();
                Transaction = null;
            }
        }

        public IDbCommand CreateCommand(string sql)
        {
            if (!StoreConnection.State.HasFlag(ConnectionState.Open))
                StoreConnection.Open();

            var command = StoreConnection.CreateCommand();
            command.Connection = StoreConnection;
            command.CommandText = sql;
            command.CommandTimeout = CommandTimeout;
            if (_transactionCount > 0 && Transaction != null)
                command.Transaction = Transaction.UnderlyingTransaction;

            return command;

        }

        public ObjectResult<TEntity> ExecQuery<TEntity>(string sql, params object[] parameters)
        {
            try
            {
                return Entities.ExecuteStoreQuery<TEntity>(sql, parameters);
            }
            catch (Exception e)
            {
                Logger.OutputLog(e, "DataContext.ExecQuery");
                throw;
            }
        }

        public int ExecCommand(string sql, params object[] parameters)
        {
            try
            {
                return Entities.ExecuteStoreCommand(sql, parameters);
            }
            catch (Exception e)
            {
                Logger.OutputLog(e, "DataContext.ExecCommand");
                throw;
            }
        }

        public ObjectSet<Document> Documents
        {
            get { return Entities.Documents; }
        }
        
        protected bool ChangeSaved;

        public void SaveChanges()
        {
            try
            {
                if (_transactionCount == 0)
                    Entities.SaveChanges();
                else
                    Entities.SaveChanges(SaveOptions.DetectChangesBeforeSave);

                ChangeSaved = true;
            } 
            catch (Exception e)
            {
                Logger.OutputLog(e, "DataContext.SaveChanges");
                throw;
            }
        }

        public void SaveToCache(Guid id, string data)
        {
            try
            {
                var cache = CacheStore;
                if (cache == null) return;
                using (var session = cache.OpenSession())
                {
                    session.Store(new CacheItem {Id = id, Data = data});
                    session.SaveChanges();
                }
            }
            catch
            {
                _cacheErrorCount ++;
                throw;
            }
        }

        public string LoadFromCache(Guid id)
        {
            try
            {
                var cache = CacheStore;
                if (cache == null) return String.Empty;
                using (var session = cache.OpenSession())
                {
                    var item = session.Load<CacheItem>(id.ToString());
                    return item != null ? item.Data : String.Empty;
                }
            }
            catch
            {
                _cacheErrorCount++;
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                if (/*_transactionCount > 0 &&*/ Transaction != null)
                {
                    Transaction.Dispose();
                    Transaction = null;
                }
                DbContext.Dispose();

                if (_entities != null)
                {
                    _entities.Dispose();
                    _entities = null;
                } 
                
                if (_ownConnection && Connection != null)
                {
                    if (!_connectionDisposed) Connection.Dispose();
                    Connection = null;
                    //Connection.Close();
                }
            }
            catch (Exception e)
            {
                Logger.OutputLog(e, "DataContext.Dispose");
                throw;
            }
        }

    }

    public class CacheItem
    {
        public Guid Id { get; set; }
        public string Data { get; set; }
    }
}
