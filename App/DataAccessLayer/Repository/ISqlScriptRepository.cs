using System;
using System.Collections.Generic;

namespace Intersoft.CISSA.DataAccessLayer.Repository
{
    public interface ISqlScriptRepository
    {
        IList<Guid> Execute(Guid scriptId);
    }
}
