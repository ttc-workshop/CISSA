using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Transactions;
using Raven.Abstractions.Extensions;

namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public interface IMultiDataContext: IDataContext
    {
        IEnumerable<IDataContext> Contexts { get; }

        IDataContext GetDocumentContext { get; }

        IDictionary<IDataContext, IDataContext> GetDataContextMaps();
    }

    public class MultiDataContext: IMultiDataContext
    {
        private readonly IList<IDataContext> _contexts = new List<IDataContext>(); 
        public IEnumerable<IDataContext> Contexts { get { return _contexts; } }

        public IDataContext GetDocumentContext
        {
            get { return Contexts.FirstOrDefault(dc => dc.DataType.HasFlag(DataContextType.Document)) ?? Contexts.First(); }
        }

        private readonly IDictionary<string, string> _contextMaps = new Dictionary<string, string>();

        public MultiDataContext(IEnumerable<IDataContext> contexts)
        {
            _contexts = new List<IDataContext>(contexts);
        }

        public MultiDataContext(string configSectionName)
        {
            var dataContextSettings =
                ConfigurationManager.GetSection(configSectionName) as
                    System.Collections.Specialized.NameValueCollection;

            if (dataContextSettings == null) return;

            foreach (var key in dataContextSettings.AllKeys)
            {
                if (String.IsNullOrEmpty(Logger.DatabaseName)) Logger.DatabaseName = key;

                var connStrSetting = ConfigurationManager.ConnectionStrings[key] as ConnectionStringSettings;

                var connection = CreateDbConnection(connStrSetting);

                var dataContext = CreateDataContext(connection, key, dataContextSettings[key]);

                _contexts.Add(dataContext);
            }

        }

        protected IDataContext CreateDataContext(DbConnection connection, string name, string connectionType)
        {
            DataContextType dcType = DataContextType.None;

            if (connectionType.ToUpper().Contains("META")) dcType |= DataContextType.Meta;
            if (connectionType.ToUpper().Contains("ACCOUNT")) dcType |= DataContextType.Account;
            if (connectionType.ToUpper().Contains("DOCUMENT")) dcType |= DataContextType.Document;

            if (!dcType.HasFlag(DataContextType.Document))
                return new MetaDataContext(connection, name);
            if (!dcType.HasFlag(DataContextType.Meta) && !dcType.HasFlag(DataContextType.Account))
                return new DocumentDataContext(connection, name/*, dcType*/);

            return new DataContext(connection, name);
        }

        private static DbConnection CreateDbConnection(ConnectionStringSettings connStrSetting)
        {
            var factory = DbProviderFactories.GetFactory(connStrSetting.ProviderName);
            
            var connection = factory.CreateConnection();
            if (connection != null)
            {
                connection.ConnectionString = connStrSetting.ConnectionString;

                return connection;
            }

            throw new ApplicationException(String.Format("Cannot create Db Connection for provider \"{0}\"",
                connStrSetting.ProviderName));
        }

        public void Dispose()
        {
            if (_transactionScope != null && _transactionCount != 0)
            {
                _transactionScope.Dispose();
                _transactionScope = null;
            }
            if (_contexts != null)
                foreach (var ctx in Contexts)
                {
                    ctx.Dispose();
                }
        }

        public string Name { get; private set; }

        public DataContextType DataType
        {
            get
            {
                var type = DataContextType.None;
                Contexts.ForEach(dc => type |= dc.DataType);
                return type;
            }
        }

        public DbConnection StoreConnection
        {
            get 
            { 
                var dc = GetDocumentContext;
                
                return dc != null ? dc.StoreConnection : _contexts.First().StoreConnection; 
            }
        }

        private TransactionScope _transactionScope;
        private int _transactionCount = 0;

        private int TransactionContextCount()
        {
            return _contexts.Count(dc => dc.DataType.HasFlag(DataContextType.Document));
        }
        public void BeginTransaction()
        {
            if (TransactionContextCount() > 1)
            {
                if (_transactionScope == null)
                {
                    _transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew, new TimeSpan(0, 0, 10));
                    _transactionCount = 0;
                }
            }
            else
            {
                var dc = GetDocumentContext;
                dc.BeginTransaction();
            }

            _transactionCount++;
        }

        public void Commit()
        {
            if (TransactionContextCount() > 1)
            {
                if (_transactionScope != null)
                {
                    if (_transactionCount == 1)
                    {
                        try
                        {
                            _transactionScope.Complete();
                            _transactionCount--;
                            _transactionScope.Dispose();
                            _transactionScope = null;
                        }
                        catch (Exception e)
                        {
                            Logger.OutputLog(e, "MultiDataContext.Commit exception");
                            throw;
                        }
                    }
                }
            }
            else
            {
                var dc = GetDocumentContext;
                try
                {
                    dc.Commit();
                    _transactionCount--;
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "MultiDataContext.Commit exception");
                    throw;
                }
            }
        }

        public void Rollback()
        {
            _transactionCount--;

            if (TransactionContextCount() > 1)
            {
                if (_transactionScope != null && _transactionCount == 0)
                {
                    _transactionScope.Dispose();
                    _transactionScope = null;
                }
            }
            else
            {
                var dc = GetDocumentContext;
                dc.Rollback();
            }
        }

        public IDbCommand CreateCommand(string sql)
        {
            return GetDocumentContext.CreateCommand(sql);
        }

        public int ExecCommand(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void SaveToCache(Guid id, string data)
        {
            throw new NotImplementedException();
        }

        public string LoadFromCache(Guid id)
        {
            throw new NotImplementedException();
        }


        public IDictionary<IDataContext, IDataContext> GetDataContextMaps()
        {
            var result = new Dictionary<IDataContext, IDataContext>();

            foreach (var map in _contextMaps)
            {
                var master =
                    _contexts.FirstOrDefault(
                        dc => dc.DataType.HasFlag(DataContextType.Meta) && String.Equals(dc.Name, map.Key, StringComparison.CurrentCultureIgnoreCase));
                var slave =
                    _contexts.FirstOrDefault(
                        dc => dc.DataType.HasFlag(DataContextType.Document) && String.Equals(dc.Name, map.Key, StringComparison.CurrentCultureIgnoreCase));

                if (master != null && slave != null)
                    result.Add(master, slave);
            }
            return result;
        }
    }
}