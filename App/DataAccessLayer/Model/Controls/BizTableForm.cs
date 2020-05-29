using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizTableForm : BizForm
    {
        [DataMember]
        public Guid? FormId { get; set; }

        [DataMember]
        public Guid? FilterFormId { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int PageNo { get; set; }

        [DataMember]
        public int RowCount { get; set; }

        [DataMember]
        public int PageCount { get; set; }

        [DataMember]
        public bool CanAddNew { get; set; }

        [DataMember]
        public bool AllowAddNew { get; set; }

        [DataMember]
        public bool AllowOpen { get; set; }

        [DataMember]
        public Guid? AddNewPermissionId { get; set; }

        [DataMember]
        public Guid? OpenPermissionId { get; set; }

//        [DataMember]
//        public IEnumerable<Doc> Documents { get; set; }

        public BizTableForm()
        {
            PageSize = 10; // 25;
        }
    }
}