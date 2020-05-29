using System;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Cache;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public class DocStateRepository : IDocStateRepository
    {
        public IDataContext DataContext { get; private set; }
//        private readonly bool _ownDataContext;

        public DocStateRepository(IDataContext dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");
            /*{
                DataContext = new DataContext();
                _ownDataContext = true;
            }
            else*/
                DataContext = dataContext;
        }
//        public DocStateRepository() : this((IDataContext)null) { }

        public static readonly ObjectCache<DocStateType> DocStateTypeCache = new ObjectCache<DocStateType>();

        public DocStateType TryLoadById(Guid stateId)
        {
            var cached = DocStateTypeCache.Find(stateId);
            if (cached != null)
                return cached.CachedObject;

            var state = DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Document_State_Type>().FirstOrDefault(s => s.Id == stateId);

            if (state != null)
            {
                var stateType = new DocStateType
                                    {
                                        Id = stateId,
                                        Name = state.Full_Name,
                                        ReadOnly = state.Read_Only ?? false
                                    };

                DocStateTypeCache.Add(stateType, stateId);

                return stateType;
            }
            return null;
        }

        public DocStateType LoadById(Guid stateId)
        {
            var stateType = TryLoadById(stateId);

            if (stateType == null) 
                throw new ApplicationException(String.Format("Состояния с идентификатором {0} не существует", stateId));

            return stateType;
        }

        public DocStateType TryLoadByName(string stateName)
        {
            var cached =
                DocStateTypeCache./*GetItems().*/FirstOrDefault(
                    co => String.Equals(co.CachedObject.Name, stateName, StringComparison.OrdinalIgnoreCase));

            if (cached != null)
                return cached.CachedObject;

            var state =
                DataContext.GetEntityDataContext().Entities.Object_Defs.OfType<Document_State_Type>().FirstOrDefault(
                    s => s.Full_Name.ToUpper() == stateName.ToUpper());

            if (state != null)
            {
                var stateType = new DocStateType
                {
                    Id = state.Id,
                    Name = state.Full_Name,
                    ReadOnly = state.Read_Only ?? false
                };

                DocStateTypeCache.Add(stateType, state.Id);

                return stateType;
            }
            return null;
        }
        
        public DocStateType LoadByName(string stateName)
        {
            var stateType = TryLoadByName(stateName);

            if (stateType == null)
                throw new ApplicationException(String.Format("Состояния с именем \"{0}\" не существует", stateName));

            return stateType;
        }

        public Guid GetDocStateTypeId(string stateName)
        {
            /*var query = from item in DataContext.ObjectDefs.OfType<Document_State_Type>()
                        where
                            item.Name.ToUpper() == stateName.ToUpper() ||
                            item.Full_Name.ToUpper() == stateName.ToUpper()
                        select item.Id;

            if (!query.Any())
            {
                throw new ApplicationException(
                    string.Format("Состояния с именем '{0}' не существует.", stateName));
            }*/

            return LoadByName(stateName).Id;
        }

        public Guid? FindDocStateTypeId(string stateName)
        {
            /*var query = from item in DataContext.ObjectDefs.OfType<Document_State_Type>()
                        where item.Name.ToUpper() == stateName.ToUpper()
                        select item.Id;

            if (!query.Any()) return null;*/

            var stateType = TryLoadByName(stateName);

            return stateType != null ? stateType.Id : (Guid?) null;
        }

        /*public void Dispose()
        {
            if (_ownDataContext && DataContext != null)
            {
                try
                {
                    DataContext.Dispose();
                    DataContext = null;
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "DocStateRepository.Dispose");
                    throw;
                }
            }
        }

        ~DocStateRepository()
        {
            if (_ownDataContext && DataContext != null)
                try
                {
                    DataContext.Dispose();
                }
                catch (Exception e)
                {
                    Logger.OutputLog(e, "DocStateRepository.Finalize");
                    throw;
                }
        }*/
    }
}
