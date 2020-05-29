using System;
using System.Data;
using System.Data.Common;

namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public interface IDataContext: IDisposable
    {
        string Name { get; }
        DataContextType DataType { get; }
//        cissaEntities Entities { get; }
        DbConnection StoreConnection { get; }
        void BeginTransaction();
        void Commit();
        void Rollback();
        IDbCommand CreateCommand(string sql);
//        ObjectResult<TEntity> ExecQuery<TEntity>(string sql, params object[] parameters);
        int ExecCommand(string sql, params object[] parameters);

//        ObjectSet<Document> Documents { get; }

//        void SaveChanges();
        void SaveToCache(Guid id, string data);
        string LoadFromCache(Guid id);
    }

    public interface ITransactionBroker
    {
        void BeginTransaction();
        void Commit();
        void Rollback();
    }
}