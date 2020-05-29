using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    public class BizGrid : BizControl
    {
        [DataMember]
        public Guid DocumentDefId { get; set; }

        /*[DataMember]
        public Guid? DocumentId { get; set; }*/

        [DataMember]
        public string TableWidth { get; set; } // ширину можно записать "5", а можно "5%".  Надеюсь ошибку тип не вызовет   
        
        [DataMember]
        public byte BorderWidth { get; set; }

        [DataMember]
        public bool IsDetail { get; set; }

        /*public override object ObjectValue
        {
            get { return DocumentId; }
            set { DocumentId = value != null ? Guid.Parse(value.ToString()) : (Guid?)null; }
        }*/
    }
}
