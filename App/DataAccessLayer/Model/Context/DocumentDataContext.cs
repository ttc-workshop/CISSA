using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public class DocumentDataContext: IDataContext
    {
        public const int CommandTimeout = 600;

        public DbConnection Connection { get; private set; }
        private readonly bool _ownConnection;
        private bool _connectionDisposed;

        public string Name { get; private set; }

        public DocumentDataContext(DbConnection connection, string name)
        {
            Connection = connection;
            _ownConnection = false;
            Name = name;
        }

        public DocumentDataContext(string connectionString, string name)
        {
            Connection = new SqlConnection(connectionString);
            _ownConnection = true;
            Name = name;
            Connection.Disposed += OnConnectionDisposed;
        }

        public void Dispose()
        {
            if (_ownConnection && Connection != null)
            {
                if (Transaction != null)
                {
                    Transaction.Dispose();
                    Transaction = null;
                }
                if (!_connectionDisposed) Connection.Dispose();
                Connection = null;
            }
        }

        public DataContextType DataType
        {
            get { return DataContextType.Document; }
        }

        public DbConnection StoreConnection
        {
            get { return Connection; }
        }

        private int _transactionCount;
        protected DbTransaction Transaction;

        public void BeginTransaction()
        {
            if (_transactionCount == 0)
            {
                if (!Connection.State.HasFlag(ConnectionState.Open))
                    Connection.Open();

                Transaction = Connection.BeginTransaction(IsolationLevel.ReadUncommitted);
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
            if (!Connection.State.HasFlag(ConnectionState.Open))
                Connection.Open();

            var command = Connection.CreateCommand();
            command.Connection = Connection;
            command.CommandText = sql;
            command.CommandTimeout = CommandTimeout;
            if (_transactionCount > 0 && Transaction != null)
                command.Transaction = Transaction;

            return command;
        }

        public int ExecCommand(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public void SaveToCache(Guid id, string data)
        {
        }

        public string LoadFromCache(Guid id)
        {
            return String.Empty;
        }
        private void OnConnectionDisposed(object sender, EventArgs args)
        {
            _connectionDisposed = true;
        }

    }
}