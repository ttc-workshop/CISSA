using System;
using System.Collections.Generic;

namespace Intersoft.CISSA.DataAccessLayer.Model.Reports
{
    public class ReportColumn : IDetailItem
    {
        public float Width { get; set; }
        public Guid AttributeId { get; set; }

        public AggregateOperation AggregateOperation { get; set; }

        #region IDetailItem Members

        public IList<IDetailItem> Children
        {
            get { return null; }
            set { throw new NotImplementedException(); }
        }

        public bool IsColumn
        {
            get { return true; }
        }

        public bool IsRoot
        {
            get { return false; }
        }

        public int LeafCount
        {
            get { return 1; }
        }

        public int ChildrenCountLevels
        {
            get { return 0; }
        }

        public string Text { get; set; }

        #endregion
    }
}