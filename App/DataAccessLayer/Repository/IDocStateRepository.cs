using System;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IDocStateRepository //: IDisposable
    {
        DocStateType TryLoadById(Guid stateId);
        DocStateType LoadById(Guid stateId);
        DocStateType TryLoadByName(string stateName);
        DocStateType LoadByName(string stateName);
        Guid GetDocStateTypeId(string stateName);
    }
}