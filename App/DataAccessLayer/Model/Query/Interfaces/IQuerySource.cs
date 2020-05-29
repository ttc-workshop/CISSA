using System.Data.Entity.Core.Objects;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces
{
    public interface IQuerySource
    {
        DocDef GetDocDef();
        string GetAlias();
        string GetPath();
        IQuerySource CreateSource(string attributeName);

        IQueryable<Document> Build(ObjectContext context);

        bool IsDocList { get; }
    }
}
