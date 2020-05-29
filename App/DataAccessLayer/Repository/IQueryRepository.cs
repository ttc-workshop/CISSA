using System;
using Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface IQueryRepository
    {
        QuerySourceDefData FindJoinDef(Guid id);
        QueryConditionDefData FindConditionDef(Guid id);
        QueryDefData FindQuery(Guid id);
        QueryDefData GetQuery(Guid id);
    }
}