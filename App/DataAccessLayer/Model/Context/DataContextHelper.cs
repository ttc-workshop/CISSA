using System;
using System.Data;

namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public static class DataContextHelper
    {
        public static IEntityDataContext GetEntityDataContext(this IDataContext dataContext)
        {
            var edc = dataContext as IEntityDataContext;

            if (edc == null)
                throw new ApplicationException("Cannot get Entity data context!");

            if (!dataContext.StoreConnection.State.HasFlag(ConnectionState.Open)) dataContext.StoreConnection.Open();

            return edc;
        }
    }
}