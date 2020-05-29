using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    [KnownType(typeof(BizDetailForm))]
    [KnownType(typeof(BizTableForm))]
    public class BizForm : BizControl
    {
        [DataMember]
        public Guid? DocumentDefId { get; set; }

        [DataMember]
        public Guid? DocumentId { get; set; }

        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public bool CanDelete { get; set; }

/*        [DataMember]
        public BizFormUpdateOptions UpdateOptions { get; set; }*/
    }

    [Flags]
    public enum BizFormUpdateOptions
    {
        Update = 1,
        AddNew = 2,
        Delete = 4
    }
}