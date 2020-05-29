using System;
using System.Collections.Generic;
using System.Linq;

namespace Intersoft.CISSA.DataAccessLayer.Model.Reports
{
    internal class ReportBand : IDetailItem
    {
        public Guid Id;
        private IList<IDetailItem> _children = new List<IDetailItem>();

        #region IDetailItem Members

        public IList<IDetailItem> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        public bool IsColumn
        {
            get { return false; }
        }

        public bool IsRoot
        {
            get { return false; }
        }

        public int LeafCount
        {
            get { return Children.Sum(child => child.LeafCount); }
        }

        public int ChildrenCountLevels
        {
            get { return Children.Max(child => child.ChildrenCountLevels + 1); }
        }

        public string Text { get; set; }

        #endregion
    }
}