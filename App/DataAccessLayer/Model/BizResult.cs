using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model
{
    [DataContract]
    public class BizResult
    {
        
        [DataMember]
        public BizResultType Type { get; set; }
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Message { get; set; }
/*
        [DataMember]
        public BizObject Object { get; set; } */
    }
}