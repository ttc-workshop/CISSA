using System;
using System.Data;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizButton : BizControl
    {
        [DataMember]
        public bool IsDisabled { get; set; }

        [DataMember]
        public Guid ActionId { get; set; }

        [DataMember]
        public BizButtonActionType ActionType { get; set; }

        [DataMember]
        public BizButtonType ButtonType { get; set; }

        [DataMember]
        public Guid? UserActionId { get; set; }

        [DataMember]
        public Guid? ProcessId { get; set; }
    }

    public enum BizButtonActionType:byte
    {
        Form,
        BizProcess,
        // ... 
    }

    public enum BizButtonType:byte
    {
        Button = 0,
        Link,
        Submit,
        Reset,
        //Push
    }
}