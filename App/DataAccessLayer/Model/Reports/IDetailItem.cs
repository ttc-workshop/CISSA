using System.Collections.Generic;

namespace Intersoft.CISSA.DataAccessLayer.Model.Reports
{
    public interface IDetailItem
    {
        IList<IDetailItem> Children { get; set; }
        bool IsColumn { get; }
        bool IsRoot { get; }
        int LeafCount { get; }
        int ChildrenCountLevels { get; }
        string Text { get; }
    }
}