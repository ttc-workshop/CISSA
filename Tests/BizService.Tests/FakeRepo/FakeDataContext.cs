using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Objects;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace Intersoft.CISSA.BizServiceTests.FakeRepo
{
    public class FakeDataContext: IDataContext
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { return "TestDC"; }
        }

        public DataContextType DataType
        {
            get { throw new NotImplementedException(); }
        }

        public cissaEntities Entities
        {
            get { throw new NotImplementedException(); }
        }

        public DbConnection StoreConnection
        {
            get { throw new NotImplementedException(); }
        }

        public void BeginTransaction()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public IDbCommand CreateCommand(string sql)
        {
            throw new NotImplementedException();
        }

        public ObjectResult<TEntity> ExecQuery<TEntity>(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public int ExecCommand(string sql, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public ObjectSet<Document> Documents
        {
            get { throw new NotImplementedException(); }
        }

        public void SaveChanges()
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
    }
}
