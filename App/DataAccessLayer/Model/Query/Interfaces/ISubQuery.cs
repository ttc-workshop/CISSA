using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces
{
    public interface ISubQuery: IQueryExpression
    {
        IQueryCondition Where(string attribute);
    }
}
